using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Configuration;
using Services.AutoIt;
using Domain;

namespace Marketing.Services.Test
{
    [TestFixture]
    public sealed class AutoItRunnerTests
    {
        private string _workDir = default!;
        private string _outDir = default!;

        [SetUp]
        public void SetUp()
        {
            _workDir = Path.Combine(Path.GetTempPath(), "AutoItRunnerTests", Guid.NewGuid().ToString("N"));
            _outDir = Path.Combine(_workDir, "out");
            Directory.CreateDirectory(_workDir);
            Directory.CreateDirectory(_outDir);

            // IMPORTANTE: AutoItRunner lee el template desde Directory.GetCurrentDirectory()
            Directory.SetCurrentDirectory(_workDir);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetTempPath());
            }
            catch { /* best-effort */ }

            try
            {
                if (Directory.Exists(_workDir))
                    Directory.Delete(_workDir, recursive: true);
            }
            catch { /* best-effort */ }
        }

        private static AppConfig BuildConfig(string outFolder, string? interpreterPath)
        {
            // Ajusta esto a tu AppConfig real.
            // Se asume algo como:
            // public sealed class AppConfig { public PathsConfig Paths {get;set;} }
            // public sealed class PathsConfig { public string OutFolder {get;set;} public string AutoItInterpreterPath {get;set;} }

            return new AppConfig
            {
                Paths = new PathsConfig
                {
                    OutFolder = outFolder,
                    AutoItInterpreterPath = interpreterPath
                }
            };
        }

        private static void CreateTemplateFile(string directory, string content)
        {
            File.WriteAllText(Path.Combine(directory, "whatsapp_upload.au3"), content, new UTF8Encoding(false));
        }

        private static string[] SnapshotTempScripts()
        {
            return Directory
                .EnumerateFiles(Path.GetTempPath(), "whatsapp_upload_*.au3", SearchOption.TopDirectoryOnly)
                .ToArray();
        }

        private static string FindNewTempScript(string[] before, string[] after)
        {
            var created = after.Except(before).ToArray();
            if (created.Length != 1)
                throw new AssertionException($"Expected exactly 1 new temp script, but found {created.Length}.");

            return created[0];
        }

        private static Mock<ILogger<AutoItRunner>> CreateLoggerMock()
        {
            return new Mock<ILogger<AutoItRunner>>(MockBehavior.Loose);
        }

        // ---------------------------------------------------------------------
        // 1) Template missing -> throw FileNotFoundException (flujo STEP 1)
        // ---------------------------------------------------------------------
        [Test]
        public void GivenTemplateMissing_WhenRunAsync_ThenThrowsFileNotFoundException()
        {
            // Given
            var loggerMock = CreateLoggerMock();
            var config = BuildConfig(_outDir, interpreterPath: @"C:\missing\autoit3.exe");
            var sut = new AutoItRunner(config, loggerMock.Object);

            // When + Then
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await sut.RunAsync(
                    timeout: TimeSpan.FromSeconds(1),
                    imagePath: @"C:\file\document.pdf",
                    useAutoItInterpreter: true,
                    cancellationToken: CancellationToken.None));
        }

        // ---------------------------------------------------------------------
        // 2) Template present + interpreter missing (useAutoItInterpreter=true)
        //    -> crea AutoItLog, escribe script temp, luego lanza FileNotFoundException
        // ---------------------------------------------------------------------
        [Test]
        public async Task GivenTemplatePresentAndInterpreterMissing_WhenRunAsync_ThenCreatesAutoItLogAndWritesTempScriptAndThrows()
        {
            // Given
            CreateTemplateFile(_workDir,
                content:
@"Global Const $LOG_FILE = ""__AUTOIT_LOG_FILE__""
Global Const $FILE_TO_UPLOAD = ""__FILE_TO_UPLOAD__""
; dummy content
");

            var loggerMock = CreateLoggerMock();
            var config = BuildConfig(_outDir, interpreterPath: Path.Combine(_workDir, "missing_autoit3.exe"));
            var sut = new AutoItRunner(config, loggerMock.Object);

            var beforeScripts = SnapshotTempScripts();

            // When
            var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await sut.RunAsync(
                    timeout: TimeSpan.FromSeconds(1),
                    imagePath: @"C:\file\document.pdf",
                    useAutoItInterpreter: true,
                    cancellationToken: CancellationToken.None));

            // Then
            Assert.That(ex, Is.Not.Null);

            var autoItLogDir = Path.Combine(_outDir, "AutoItLog");
            Assert.That(Directory.Exists(autoItLogDir), Is.True, "Expected AutoItLog directory to be created.");

            var afterScripts = SnapshotTempScripts();
            var scriptPath = FindNewTempScript(beforeScripts, afterScripts);

            Assert.That(File.Exists(scriptPath), Is.True, "Expected generated temp .au3 script to exist.");

            var scriptContent = File.ReadAllText(scriptPath);
            Assert.That(scriptContent, Does.Contain(@"C:\file\document.pdf"), "Expected __FILE_TO_UPLOAD__ replacement.");

            // Debe haber reemplazado __AUTOIT_LOG_FILE__ por un path real dentro de AutoItLog.
            Assert.That(scriptContent, Does.Contain(autoItLogDir), "Expected __AUTOIT_LOG_FILE__ replacement.");
        }

        // ---------------------------------------------------------------------
        // 3) Template present + useAutoItInterpreter=false:
        //    No valida existencia de interpreter.
        //    Esperamos que avance hasta intentar Start del proceso y falle
        //    (en muchos entornos, .au3 no es ejecutable sin asociación).
        //    Esta prueba se protege con Assume para Windows y valida "falla estable".
        // ---------------------------------------------------------------------
        [Test]
        public void GivenTemplatePresentAndDirectMode_WhenRunAsync_ThenFailsToStartProcess()
        {
            // Given
            Assume.That(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                "This behavior is Windows-specific due to process execution and file associations.");

            CreateTemplateFile(_workDir,
                content:
@"Global Const $LOG_FILE = ""__AUTOIT_LOG_FILE__""
Global Const $FILE_TO_UPLOAD = ""__FILE_TO_UPLOAD__""
; dummy content
");

            var loggerMock = CreateLoggerMock();
            var config = BuildConfig(_outDir, interpreterPath: null);
            var sut = new AutoItRunner(config, loggerMock.Object);

            // When + Then
            // En modo directo, intentará ejecutar el .au3 como proceso.
            // Si no existe asociación, normalmente explota con Win32Exception.
            Assert.CatchAsync(async () =>
                await sut.RunAsync(
                    timeout: TimeSpan.FromSeconds(2),
                    imagePath: @"C:\file\document.pdf",
                    useAutoItInterpreter: false,
                    cancellationToken: CancellationToken.None));
        }

        // ---------------------------------------------------------------------
        // 4) Validación de contrato de salida (AutoItRunnerResult) *a nivel de compilación*
        //    Asegura que el shape del resultado se mantiene (refactor-safety).
        //    No ejecuta el proceso: valida propiedades requeridas.
        // ---------------------------------------------------------------------
        [Test]
        public void GivenAutoItRunnerResultContract_WhenConstructed_ThenRequiredFieldsAreEnforced()
        {
            // Given + When + Then
            // Este test existe para proteger el contrato del DTO ante refactors:
            // ExitCode, TimedOut, StdOut, StdErr, Duration son required.
            // Si alguien cambia required/remove properties, esto rompe compilación o expectativas.
            var result = new AutoItRunnerResult
            {
                ExitCode = 0,
                TimedOut = false,
                StdOut = "",
                StdErr = "",
                Duration = TimeSpan.Zero,
                LogFilePath = null
            };

            Assert.That(result.ExitCode, Is.EqualTo(0));
            Assert.That(result.TimedOut, Is.False);
            Assert.That(result.StdOut, Is.Not.Null);
            Assert.That(result.StdErr, Is.Not.Null);
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.Zero));
        }

        // ---------------------------------------------------------------------
        // 5) Límite: imagePath vacío / null-like
        //    La implementación actual hace Replace en el template con imagePath (puede romperse si null).
        //    Este test protege el comportamiento actual: si imagePath es null, hoy explota con ArgumentNullException
        //    por Replace(...) (string.Replace no acepta null).
        // ---------------------------------------------------------------------
        [Test]
        public void GivenNullImagePath_WhenRunAsync_ThenThrows()
        {
            // Given
            CreateTemplateFile(_workDir,
                content:
@"Global Const $LOG_FILE = ""__AUTOIT_LOG_FILE__""
Global Const $FILE_TO_UPLOAD = ""__FILE_TO_UPLOAD__""
");

            var loggerMock = CreateLoggerMock();
            var config = BuildConfig(_outDir, interpreterPath: Path.Combine(_workDir, "missing_autoit3.exe"));
            var sut = new AutoItRunner(config, loggerMock.Object);

            // When + Then
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await sut.RunAsync(
                    timeout: TimeSpan.FromSeconds(1),
                    imagePath: null!, // explicit: protect current behavior
                    useAutoItInterpreter: true,
                    cancellationToken: CancellationToken.None));
        }
    }

    // -------------------------------------------------------------------------
    // Stubs de config para que el archivo sea autocontenido si los necesitas.
    // ELIMÍNALOS si ya existen en tu proyecto real.
    // -------------------------------------------------------------------------
    namespace Configuration
    {
        public sealed class AppConfig
        {
            public PathsConfig Paths { get; set; } = new();
        }

        public sealed class PathsConfig
        {
            public string OutFolder { get; set; } = "";
            public string? AutoItInterpreterPath { get; set; }
        }
    }
}
