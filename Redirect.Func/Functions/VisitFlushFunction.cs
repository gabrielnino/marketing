using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Redirect.Func.Functions;

public sealed class VisitFlushFunction(
    TableServiceClient tableServiceClient,
    QueueServiceClient queueServiceClient,
    IConfiguration config,
    ILogger<VisitFlushFunction> logger)
{
    private readonly TableClient _tableClient = tableServiceClient.GetTableClient("TrackedLinks");
    private readonly QueueClient _queueClient = queueServiceClient.GetQueueClient("trackedlink-visits");
    private readonly ILogger<VisitFlushFunction> _logger = logger;
    private readonly IConfiguration _config = config;

    // Se ejecuta cada 5 minutos por defecto (puedes cambiar a "0 */N * * * *")
    [Function("VisitFlush")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
    {
        var maxMessages = _config.GetValue("RedirectAnalytics:FlushMaxMessages", 5000);
        var batchSize = Math.Clamp(_config.GetValue("RedirectAnalytics:QueueBatchSize", 32), 1, 32);

        _logger.LogInformation("VisitFlush started. maxMessages={MaxMessages} batchSize={BatchSize}", maxMessages, batchSize);

        // Asegura que existan (no falla si ya existen)
        await EnsureResourcesAsync();

        // 1) Drenar queue y agrupar counts por code
        var counts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var processed = 0;

        while (processed < maxMessages)
        {
            var take = Math.Min(batchSize, maxMessages - processed);
            QueueMessage[] messages;

            try
            {
                var received = await _queueClient.ReceiveMessagesAsync(
                    maxMessages: take,
                    visibilityTimeout: TimeSpan.FromMinutes(2)); // tiempo para procesar

                messages = received.Value ?? Array.Empty<QueueMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to receive messages from queue.");
                break;
            }

            if (messages.Length == 0)
                break;

            foreach (var msg in messages)
            {
                processed++;

                var code = TryExtractCode(msg.MessageText);
                if (!string.IsNullOrWhiteSpace(code))
                {
                    counts.AddOrUpdate(code, 1, (_, old) => old + 1);
                }

                // Borrar SIEMPRE (si falla el contador no debe reventar la cola)
                try
                {
                    await _queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete queue message id={MessageId}", msg.MessageId);
                }
            }
        }

        if (counts.Count == 0)
        {
            _logger.LogInformation("VisitFlush finished. No messages.");
            return;
        }

        // 2) Aplicar incrementos en Table
        await ApplyIncrementsAsync(counts);

        _logger.LogInformation("VisitFlush finished. processed={Processed} uniqueCodes={UniqueCodes}",
            processed, counts.Count);
    }

    private async Task EnsureResourcesAsync()
    {
        try
        {
            await _tableClient.CreateIfNotExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CreateIfNotExists table failed (may already exist).");
        }

        try
        {
            await _queueClient.CreateIfNotExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CreateIfNotExists queue failed (may already exist).");
        }
    }

    private async Task ApplyIncrementsAsync(ConcurrentDictionary<string, int> counts)
    {
        // PartitionKey debe coincidir con RedirectFunction
        const string pk = "LINK";

        foreach (var kvp in counts)
        {
            var code = kvp.Key;
            var delta = kvp.Value;

            // Intento simple con reintentos por conflicto
            var retries = 5;

            for (var attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    TableEntity entity;

                    try
                    {
                        var existing = await _tableClient.GetEntityAsync<TableEntity>(pk, code);
                        entity = existing.Value;
                    }
                    catch (RequestFailedException ex) when (ex.Status == 404)
                    {
                        // Si el code no existe, lo creamos con VisitCount inicial
                        entity = new TableEntity(pk, code)
                        {
                            ["TargetUrl"] = "", // opcional: si no existe, queda vacío
                            ["VisitCount"] = 0L
                        };
                    }

                    var current = entity.TryGetValue("VisitCount", out var v) ? Convert.ToInt64(v) : 0L;
                    entity["VisitCount"] = checked(current + delta);
                    entity["LastVisitUtc"] = DateTime.UtcNow;

                    // Upsert para simplificar, ETag=* para evitar bloqueos por concurrencia
                    await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);

                    break; // éxito
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to apply increment. code={Code} delta={Delta} attempt={Attempt}/{Retries}",
                        code, delta, attempt, retries);

                    // Backoff corto
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt));
                }
            }
        }
    }

    private static string? TryExtractCode(string messageText)
    {
        // Azure Queue guarda el contenido como texto; en RedirectFunction lo mandamos base64 de JSON.
        try
        {
            var json = DecodeIfBase64(messageText);
            using var doc = JsonDocument.Parse(json);

            // Acepta "Code" o "code"
            if (doc.RootElement.TryGetProperty("Code", out var c) && c.ValueKind == JsonValueKind.String)
                return c.GetString();

            if (doc.RootElement.TryGetProperty("code", out var c2) && c2.ValueKind == JsonValueKind.String)
                return c2.GetString();

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string DecodeIfBase64(string text)
    {
        // Si no es base64 válido, retornamos el texto original
        try
        {
            var bytes = Convert.FromBase64String(text);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return text;
        }
    }
}
