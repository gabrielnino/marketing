

=== FILE: F:\Marketing\architecture_review_code.cs ===



=== FILE: F:\Marketing\Application\Common\Messages\ImageMessagePayload.cs ===

﻿namespace Application.Common.Messages
{
public sealed class ImageMessagePayload
{
public string StoredImagePath { get; init; } = default!;
public string Caption { get; init; } = string.Empty;
}
}

=== FILE: F:\Marketing\Application\Common\Pagination\PagedResult.cs ===

﻿namespace Application.Common.Pagination
{
public sealed record PagedResult<T>
{
public IEnumerable<T> Items { get; init; } = [];
public string? NextCursor { get; init; }
public int TotalCount { get; init; }
}
}

=== FILE: F:\Marketing\Application\Constants\Messages.cs ===

﻿namespace Application.Constants
{
public static class Messages
{
public static class InvalidOperation
{
public const string NullMessage = "The 'message' parameter cannot be null, empty, or whitespace.";
}
public static class Operation
{
public const string InvalidOperation = "This method can only be used if the value of IsSuccessful is false.";
}
public static class EnumExtensions
{
public const string Unknown = "UNKNOWN";
public const string DescriptionNotAvailable = "Description not available.";
public const string NoEnumValueFound = "No enum value found for {0} in {1}";
}
public static class EnumMetadata
{
public const string ForNameOrDescription = "For name or description, null, empty, and whitespace are not allowed.";
}
}
}

=== FILE: F:\Marketing\Application\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Application\obj\Debug\net8.0\Application.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Application")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+2889c604867e086d29f73bae138892e6e430cba5")]
[assembly: System.Reflection.AssemblyProductAttribute("Application")]
[assembly: System.Reflection.AssemblyTitleAttribute("Application")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Application\obj\Debug\net8.0\Application.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Application\Result\IErrorHandler.cs ===

﻿
namespace Application.Result
{
public interface IErrorHandler
{
void LoadErrorMappings(string filePath);
Operation<T> Fail<T>(Exception? ex, string? errorMessage = null);
Operation<T> Business<T>(string errorMessage);
bool Any();
}
}

=== FILE: F:\Marketing\Application\Result\IErrorLogger.cs ===

﻿
namespace Application.Result
{
public interface IErrorLogger
{
Task LogAsync(Exception ex, CancellationToken cancellationToken = default);
}
}

=== FILE: F:\Marketing\Application\Result\Operation.cs ===

﻿namespace Application.Result
{
using Application.Result.Error;
using InvalidOperation = Exceptions.InvalidOperation;
using static Application.Constants.Messages;
public class Operation<T> : Result<T>
{
private Operation() { }
public static Operation<T> Success(T? data, string? message = "")
{
return new Operation<T>
{
IsSuccessful = true,
Data         = data,
Message      = message ?? string.Empty,
Type         = ErrorTypes.None
};
}
public static Operation<T> Failure(string message, ErrorTypes errorTypes)
{
return new Operation<T>
{
IsSuccessful = false,
Message      = message,
Type         = errorTypes
};
}
public Operation<U> AsType<U>()
{
EnsureIsFailure();
return new Operation<U>
{
IsSuccessful = false,
Message      = this.Message,
Type         = this.Type
};
}
public Operation<U> ConvertTo<U>() => AsType<U>();
private void EnsureIsFailure()
{
if (IsSuccessful)
{
throw new InvalidOperation(Operation.InvalidOperation);
}
}
}
}

=== FILE: F:\Marketing\Application\Result\OperationStrategy.cs ===

﻿using Application.Result.EnumType.Extensions;
using Application.Result.Error;
namespace Application.Result
{
public interface IErrorCreationStrategy<T>
{
Operation<T> CreateFailure(string message);
Operation<T> CreateFailure();
}
public abstract class ErrorStrategyBase<T>(ErrorTypes errorType) : IErrorCreationStrategy<T>
{
private readonly ErrorTypes _errorType = errorType;
public Operation<T> CreateFailure(string message)
=> Operation<T>.Failure(message, _errorType);
public Operation<T> CreateFailure()
=> Operation<T>.Failure(_errorType.GetDescription(), _errorType);
}
public class BusinessStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.BusinessValidation);
public class ConfigMissingStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.ConfigMissing);
public class DatabaseStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Database);
public class InvalidDataStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.InvalidData);
public class UnexpectedErrorStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Unexpected);
public class NetworkErrorStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Network);
public class NullExceptionStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.NullExceptionStrategy);
public class UserInputStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.UserInput);
public class NotFoundStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.NotFound);
public class AuthenticationStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Authentication);
public class AuthorizationStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Authorization);
public class ResourceStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Resource);
public class TimeoutStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Timeout);
public class NoneStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.None);
public static class OperationStrategy<T>
{
private const string DefaultErrorMessage = "Unknown Error";
public static Operation<T> Fail(string? message, IErrorCreationStrategy<T> strategy)
{
if (strategy == null)
throw new ArgumentNullException(nameof(strategy), "Strategy cannot be null.");
var finalMessage = string.IsNullOrWhiteSpace(message)
? DefaultErrorMessage
: message;
return strategy.CreateFailure(finalMessage);
}
}
}

=== FILE: F:\Marketing\Application\Result\Result.cs ===

﻿using Application.Result.EnumType.Extensions;
using Application.Result.Error;
namespace Application.Result
{
public class Result<T>
{
public ErrorTypes Type { get; set; }
public bool IsSuccessful { get; protected set; }
public T? Data { get; protected set; }
public string? Message { get; protected set; }
public string Error => this.Type.GetCustomName();
}
}

=== FILE: F:\Marketing\Application\Result\EnumType\Extensions\EnumExtensions.cs ===

﻿namespace Application.Result.EnumType.Extensions
{
using Application.Constants;
using System;
using System.Reflection;
public static class EnumExtensions
{
public static string GetCustomName<TEnum>(this TEnum enumValue)
where TEnum : struct, Enum
{
return GetEnumMetadata(enumValue)?.Name ?? Messages.EnumExtensions.Unknown;
}
public static string GetDescription<TEnum>(this TEnum enumValue)
where TEnum : struct, Enum
{
return GetEnumMetadata(enumValue)?.Description ?? Messages.EnumExtensions.DescriptionNotAvailable;
}
private static EnumMetadata? GetEnumMetadata<TEnum>(TEnum enumValue)
where TEnum : Enum
{
var type = enumValue.GetType();
var name = Enum.GetName(type, enumValue);
if (name != null)
{
var field = type.GetField(name);
if (field?.GetCustomAttribute<EnumMetadata>(false) is EnumMetadata attribute)
{
return attribute;
}
}
return null;
}
}
}

=== FILE: F:\Marketing\Application\Result\EnumType\Extensions\EnumMetadata.cs ===

﻿namespace Application.Result.EnumType.Extensions
{
using Application.Constants;
using System;
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class EnumMetadata : Attribute
{
public string Name { get; private set; }
public string Description { get; private set; }
public EnumMetadata(string name, string description)
{
if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
{
throw new ArgumentNullException(Messages.EnumMetadata.ForNameOrDescription);
}
Name = name;
Description = description;
}
}
}

=== FILE: F:\Marketing\Application\Result\Error\ErrorTypes.cs ===

﻿using Application.Result.EnumType.Extensions;
namespace Application.Result.Error
{
public enum ErrorTypes
{
[EnumMetadata("NONE", "No error has occurred.")]
None,
[EnumMetadata("BUSINESS_VALIDATION_ERROR", "Occurs when business logic validation fails.")]
BusinessValidation,
[EnumMetadata("DATABASE_ERROR", "Occurs when an error happens during database interaction.")]
Database,
[EnumMetadata("UNEXPECTED_ERROR", "Occurs for any unexpected or unclassified error.")]
Unexpected,
[EnumMetadata("DATA_SUBMITTED_INVALID", "Occurs when the submitted data is invalid.")]
InvalidData,
[EnumMetadata("CONFIGURATION_MISSING_ERROR", "Occurs when a required configuration is missing.")]
ConfigMissing,
[EnumMetadata("NETWORK_ERROR", "Occurs due to a network connectivity issue.")]
Network,
[EnumMetadata("USER_INPUT_ERROR", "Occurs when user input is invalid.")]
UserInput,
[EnumMetadata("NONE_FOUND_ERROR", "Occurs when a requested resource is not found.")]
NotFound,
[EnumMetadata("AUTHENTICATION_ERROR", "Occurs when user authentication fails.")]
Authentication,
[EnumMetadata("AUTHORIZATION_ERROR", "Occurs when the user is not authorized to perform the action.")]
Authorization,
[EnumMetadata("RESOURCE_ERROR", "Occurs when allocating or accessing a resource fails.")]
Resource,
[EnumMetadata("TIMEOUT_ERROR", "Occurs when an operation times out.")]
Timeout,
[EnumMetadata("NULL_EXCEPTION_STRATEGY", "Occurs when the error-mappings dictionary is uninitialized.")]
NullExceptionStrategy
}
}

=== FILE: F:\Marketing\Application\Result\Exceptions\InvalidOperation.cs ===

﻿namespace Application.Result.Exceptions
{
using Application.Constants;
public class InvalidOperation : Exception
{
public InvalidOperation(string message) : base(message)
{
if (string.IsNullOrWhiteSpace(message))
{
throw new ArgumentNullException(nameof(message), Messages.InvalidOperation.NullMessage);
}
}
}
}

=== FILE: F:\Marketing\Application\UseCases\Repository\CRUD\ICreate.cs ===

﻿using Application.Result;
using Domain.Interfaces.Entity;
namespace Application.UseCases.Repository.CRUD
{
public interface ICreate<T> where T : class, IEntity
{
Task<Operation<bool>> CreateEntity(T entity);
Task<Operation<bool>> CreateEntities(List<T> entity);
}
}

=== FILE: F:\Marketing\Application\UseCases\Repository\CRUD\IDelete.cs ===

﻿using Application.Result;
using Domain.Interfaces.Entity;
namespace Application.UseCases.Repository.CRUD
{
public interface IDelete<T> where T : class, IEntity
{
Task<Operation<bool>> DeleteEntity(string id);
}
}

=== FILE: F:\Marketing\Application\UseCases\Repository\CRUD\IUpdate.cs ===

﻿using Application.Result;
using Domain.Interfaces.Entity;
namespace Application.UseCases.Repository.CRUD
{
public interface IUpdate<T> where T : class, IEntity
{
Task<Operation<bool>> UpdateEntity(T entity);
}
}

=== FILE: F:\Marketing\Application\UseCases\Repository\CRUD\Query\IReadById.cs ===

﻿using Application.Result;
using Domain.Interfaces.Entity;
namespace Application.UseCases.Repository.CRUD.Query
{
public interface IReadById<T> where T : class, IEntity
{
Task<Operation<T>> ReadById(string id);
}
}

=== FILE: F:\Marketing\Application\UseCases\Repository\UseCases\CRUD\IErrorLogCreate.cs ===

﻿using Application.Result;
using Domain;
namespace Application.UseCases.Repository.UseCases.CRUD
{
public interface IErrorLogCreate
{
Task<Operation<bool>> CreateInvoiceAsync(ErrorLog entity);
}
}

=== FILE: F:\Marketing\Bootstrapper\AppHostBuilder.cs ===

﻿using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Commands;
using Configuration;
using Infrastructure.Repositories.CRUD;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Persistence.Context.Implementation;
using Persistence.Context.Interceptors;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Serilog;
using Services;
using Services.Interfaces;
namespace Bootstrapper
{
public static class AppHostBuilder
{
public static IHostBuilder Create(string[] args)
{
var appConfig = new AppConfig();
return Host.CreateDefaultBuilder(args)
.ConfigureAppConfiguration((hostingContext, config) =>
{
config.SetBasePath(Directory.GetCurrentDirectory());
config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
config.AddEnvironmentVariables();
})
.ConfigureServices((hostingContext, services) =>
{
services.AddSingleton<IValidateOptions<SchedulerOptions>, SchedulerOptionsValidator>();
services.AddOptions<SchedulerOptions>()
.Bind(hostingContext.Configuration.GetSection(SchedulerOptions.SectionName))
.PostConfigure(o =>
{
foreach (var key in o.Weekly.Keys.ToList())
{
var list = o.Weekly[key] ?? [];
List<string> value = [.. list
.Where(x => !string.IsNullOrWhiteSpace(x))
.Select(x => x.Trim())
.Distinct(StringComparer.OrdinalIgnoreCase)
.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)];
o.Weekly[key] = value;
}
})
.ValidateOnStart();
hostingContext.Configuration.Bind(appConfig);
var executionMode = hostingContext
.Configuration
.GetValue<ExecutionMode>("ExecutionMode");
if (executionMode == ExecutionMode.Scheduler)
{
services.AddHostedService<ScheduledMessenger>();
}
services.AddSingleton(appConfig);
var executionTracker = new ExecutionTracker(appConfig.Paths.OutFolder);
var cleanupReport = executionTracker.CleanupOrphanedRunningFolders();
LogCleanupReport(cleanupReport);
Directory.CreateDirectory(executionTracker.ExecutionRunning);
services.AddSingleton(executionTracker);
if (executionMode == ExecutionMode.Command)
{
services.AddSingleton(new CommandArgs(args));
services.AddSingleton<CommandFactory>();
services.AddSingleton<CommandArgs>();
services.AddTransient<WhatsAppCommand>();
services.AddTransient<HelpCommand>();
services.AddHostedService<WebDriverLifetimeService>();
}
services.AddScoped<IWhatsAppMessage, WhatsAppMessage>();
services.AddScoped<IWebDriver>(sp =>
{
var factory = sp.GetRequiredService<IWebDriverFactory>();
return factory.Create(false);
});
services.AddSingleton<ISecurityCheck, SecurityCheck>();
services.AddTransient<ILoginService, LoginService>();
services.AddTransient<ICaptureSnapshot, CaptureSnapshot>();
services.AddSingleton<IWebDriverFactory, ChromeDriverFactory>();
services.AddSingleton<IDirectoryCheck, DirectoryCheck>();
services.AddSingleton<IUtil, Util>();
services.AddTransient<IWhatAppOpenChat, WhatAppOpenChat>();
services.AddTransient<IWhatsAppChatService, WhatsAppChatService>();
AddDbContextSQLite(hostingContext, services);
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IDataContext, DataContext>();
services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());
services.AddScoped<IErrorHandler, ErrorHandler>();
services.AddScoped<IErrorLogCreate, ErrorLogCreate>();
services.AddSingleton<IColumnTypes, SQLite>();
})
.UseSerilog((context, services, loggerConfig) =>
{
var execution = services.GetRequiredService<ExecutionTracker>();
var logPath = Path.Combine(execution.ExecutionRunning, "Logs");
Directory.CreateDirectory(logPath);
loggerConfig.MinimumLevel.Debug()
.WriteTo.Console()
.WriteTo.File(
path: Path.Combine(logPath, "Marketing-.log"),
rollingInterval: RollingInterval.Day,
fileSizeLimitBytes: 5_000_000,
retainedFileCountLimit: 7,
rollOnFileSizeLimit: true,
outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"
);
});
}
public static void LogCleanupReport(CleanupReport report)
{
if (report is null)
{
Log.Warning("The folder is clean");
return;
}
foreach (var deleted in report.DeletedRunningFolders)
{
Log.Information(
"Deleted orphaned execution folder: {FolderPath}",
deleted);
}
foreach (var failure in report.DeleteFailures)
{
Log.Warning(
failure.Exception,
"Failed to delete orphaned execution folder: {FolderPath}",
failure.Path);
}
if (report.IsClean)
{
Log.Information("Execution cleanup completed with no errors");
}
else
{
Log.Warning(
"Execution cleanup completed with {FailureCount} failure(s)",
report.DeleteFailures.Count);
}
}
private static void AddDbContextSQLite(HostBuilderContext context, IServiceCollection services)
{
var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
throw new ArgumentNullException(nameof(connectionString),
"Connection string 'DefaultConnection' is missing or empty.");
services.AddDbContext<DataContext>(options =>
{
options
.UseSqlite(connectionString, sqlite =>
{
sqlite.MigrationsAssembly(typeof(DataContext).Assembly.GetName().Name);
})
.AddInterceptors(new SqliteFunctionInterceptor());
});
services.AddMemoryCache();
}
}
}

=== FILE: F:\Marketing\Bootstrapper\ExecutionMode.cs ===

﻿namespace Bootstrapper
{
public enum ExecutionMode
{
Scheduler,
Command
}
}

=== FILE: F:\Marketing\Bootstrapper\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Debug\net8.0\Bootstrapper.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Bootstrapper")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+94e1a471b9875dcb71d674cd5bee527643160ff7")]
[assembly: System.Reflection.AssemblyProductAttribute("Bootstrapper")]
[assembly: System.Reflection.AssemblyTitleAttribute("Bootstrapper")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Debug\net8.0\Bootstrapper.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Bootstrapper\obj\Debug\net8.0\Console.Bootstrapper.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Console.Bootstrapper")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+a6b4d3e6fdf732a0e433e9886ef84950f11282b3")]
[assembly: System.Reflection.AssemblyProductAttribute("Console.Bootstrapper")]
[assembly: System.Reflection.AssemblyTitleAttribute("Console.Bootstrapper")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Debug\net8.0\Console.Bootstrapper.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Bootstrapper\obj\Debug\net8.0\ValkyrieHire.Bootstrapper.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("ValkyrieHire.Bootstrapper")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+4e43d427803c36c64153b0c0c7ae980e25b1493b")]
[assembly: System.Reflection.AssemblyProductAttribute("ValkyrieHire.Bootstrapper")]
[assembly: System.Reflection.AssemblyTitleAttribute("ValkyrieHire.Bootstrapper")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Debug\net8.0\ValkyrieHire.Bootstrapper.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Bootstrapper\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Release\net8.0\Console.Bootstrapper.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Console.Bootstrapper")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+1f7b61cc657b6b85a6c4b54e1aee2477c28bbe28")]
[assembly: System.Reflection.AssemblyProductAttribute("Console.Bootstrapper")]
[assembly: System.Reflection.AssemblyTitleAttribute("Console.Bootstrapper")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Release\net8.0\Console.Bootstrapper.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Commands\CommandArgs.cs ===

﻿namespace Commands
{
public class CommandArgs
{
public const string WhatsApp = "--whatsapp";
private static readonly HashSet<string> ValidCommands = new(StringComparer.OrdinalIgnoreCase)
{
WhatsApp
};
public string MainCommand { get; }
public Dictionary<string, string> Arguments { get; }
public CommandArgs(string[] args)
{
var cmd = args.FirstOrDefault(IsCommand);
var firstArg = args.FirstOrDefault(IsArgument);
MainCommand = cmd ?? (firstArg is null ? string.Empty : firstArg.Split('=', 2)[0]);
Arguments = args
.Where(IsArgument)
.Select(arg =>
{
var parts = arg.Split('=', 2);
var key = parts[0];
var value = parts.Length > 1 ? parts[1] : string.Empty;
return new KeyValuePair<string, string>(key, value);
})
.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
}
private static bool IsCommand(string arg) => ValidCommands.Contains(arg);
private static bool IsArgument(string arg) => arg.Contains("=");
}
}

=== FILE: F:\Marketing\Commands\CommandFactory.cs ===

﻿using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
namespace Commands
{
public class CommandFactory
{
private readonly IServiceProvider _serviceProvider;
private readonly CommandArgs _jobCommandArgs;
public CommandFactory(IServiceProvider serviceProvider, CommandArgs jobCommandArgs)
{
_serviceProvider = serviceProvider;
_jobCommandArgs = jobCommandArgs;
}
public IEnumerable<ICommand> CreateCommand()
{
var commands = new List<ICommand>();
switch (_jobCommandArgs.MainCommand.ToLowerInvariant())
{
case CommandArgs.WhatsApp:
commands.Add(_serviceProvider.GetRequiredService<WhatsAppCommand>());
break;
default:
commands.Add(_serviceProvider.GetRequiredService<HelpCommand>());
break;
}
return commands;
}
}
}

=== FILE: F:\Marketing\Commands\HelpCommand.cs ===

﻿using Microsoft.Extensions.Logging;
namespace Commands
{
public class
HelpCommand : ICommand
{
private readonly ILogger<HelpCommand> _logger;
public HelpCommand(ILogger<HelpCommand> logger = null)
{
_logger = logger;
}
public Task ExecuteAsync(Dictionary<string, string>? Arguments)
{
_logger?.LogInformation("Displaying help information");
Console.WriteLine("Available commands:");
Console.WriteLine("--search\tSearch for jobs");
Console.WriteLine("--export\tExport results");
Console.WriteLine("--help\t\tShow this help");
return Task.CompletedTask;
}
}
}

=== FILE: F:\Marketing\Commands\ICommand.cs ===

﻿namespace Commands
{
public interface ICommand
{
Task ExecuteAsync(Dictionary<string, string>? arguments=null);
}
}

=== FILE: F:\Marketing\Commands\WhatsAppCommand.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Services;
using Services.Interfaces;
namespace Commands
{
public class WhatsAppCommand(ILogger<WhatsAppCommand> logger, IWhatsAppMessage iWhatsAppMessage) : ICommand
{
private ILogger<WhatsAppCommand> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));
private IWhatsAppMessage IWhatsAppMessage { get; } = iWhatsAppMessage ?? throw new ArgumentNullException(nameof(WhatsAppCommand));
public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
{
Logger.LogInformation("InviteCommand: starting. args={@Args}", arguments);
await IWhatsAppMessage.LoginAsync();
await IWhatsAppMessage.SendMessageAsync();
Logger.LogInformation("InviteCommand: finished.");
}
}
}

=== FILE: F:\Marketing\Commands\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Commands\obj\Debug\net8.0\Commands.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Commands")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+824c57be25fbd2217ad6312cf623c6356b3db7c2")]
[assembly: System.Reflection.AssemblyProductAttribute("Commands")]
[assembly: System.Reflection.AssemblyTitleAttribute("Commands")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Commands\obj\Debug\net8.0\Commands.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Commands\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Commands\obj\Release\net8.0\Commands.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Commands")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+1f7b61cc657b6b85a6c4b54e1aee2477c28bbe28")]
[assembly: System.Reflection.AssemblyProductAttribute("Commands")]
[assembly: System.Reflection.AssemblyTitleAttribute("Commands")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Commands\obj\Release\net8.0\Commands.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Configuration\AppConfig.cs ===

﻿namespace Configuration
{
public class AppConfig
{
public WhatsAppConfig WhatsApp { get; set; }
public PathsConfig Paths { get; set; }
}
}

=== FILE: F:\Marketing\Configuration\ExecutionTracker.cs ===

﻿namespace Configuration
{
public class ExecutionTracker
{
private const string ExecutionRunningName = "ExecutionRunning";
private const string ExecutionFinishedName = "ExecutionFinished";
private readonly string _outPath;
public ExecutionTracker(string outPath)
{
_outPath = outPath;
TimeStamp = ActiveTimeStamp ?? DateTime.Now.ToString("yyyyMMdd_HHmmss");
}
public string TimeStamp { get; }
public string ExecutionRunning => BuildPath(ExecutionRunningName, TimeStamp);
public FinalizeReport FinalizeByCopyThenDelete(bool overwriteFinishedIfExists = false)
{
var runningPath = ExecutionRunning;
var finishedPath = ExecutionFinished;
if (!Directory.Exists(runningPath))
throw new DirectoryNotFoundException($"Folder not found: {runningPath}");
if (Directory.Exists(finishedPath))
{
if (!overwriteFinishedIfExists)
throw new IOException($"Folder already exists: {finishedPath}");
TryDeleteDirectory(finishedPath, out _);
}
Directory.CreateDirectory(finishedPath);
var report = new FinalizeReport(runningPath, finishedPath);
CopyDirectoryRecursive(runningPath, finishedPath, report);
if (!TryDeleteDirectory(runningPath, out var deleteError) && deleteError is not null)
report.DeleteFailures.Add(new Failure(runningPath, deleteError));
return report;
}
public CleanupReport CleanupOrphanedRunningFolders()
{
var report = new CleanupReport();
if (!Directory.Exists(_outPath))
return report;
foreach (var runningDir in Directory.GetDirectories(_outPath, $"{ExecutionRunningName}_*"))
{
var ts = ExtractTimeStampFromFolder(runningDir, ExecutionRunningName);
if (ts is null)
continue;
var finishedDir = BuildPath(ExecutionFinishedName, ts);
if (!Directory.Exists(finishedDir))
continue;
if (!TryDeleteDirectory(runningDir, out var error) && error is not null)
report.DeleteFailures.Add(new Failure(runningDir, error));
else
report.DeletedRunningFolders.Add(runningDir);
}
return report;
}
private string ExecutionFinished => BuildPath(ExecutionFinishedName, TimeStamp);
private string? ActiveTimeStamp => GetLatestTimeStamp();
private string BuildPath(string prefix, string timeStamp)
=> Path.Combine(_outPath, $"{prefix}_{timeStamp}");
private string? GetLatestTimeStamp()
{
if (!Directory.Exists(_outPath))
return null;
var lastRunning = Directory
.GetDirectories(_outPath, $"{ExecutionRunningName}_*")
.OrderByDescending(Path.GetFileName)
.FirstOrDefault();
if (lastRunning is null)
return null;
var timestamp = ExtractTimeStampFromFolder(lastRunning, ExecutionRunningName);
if (timestamp is null)
return null;
var finishedPath = Path.Combine(_outPath, $"{ExecutionFinishedName}_{timestamp}");
return Directory.Exists(finishedPath) ? null : timestamp;
}
private static string? ExtractTimeStampFromFolder(string fullPath, string prefix)
{
var name = Path.GetFileName(fullPath);
if (name is null || !name.StartsWith($"{prefix}_"))
return null;
return name[(prefix.Length + 1)..];
}
private static bool TryDeleteDirectory(string path, out Exception? error)
{
try
{
if (!Directory.Exists(path))
{
error = null;
return true;
}
foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
{
try { File.SetAttributes(dir, FileAttributes.Normal); } catch { }
}
foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
{
try { File.SetAttributes(file, FileAttributes.Normal); } catch { }
}
try { File.SetAttributes(path, FileAttributes.Normal); } catch { }
Directory.Delete(path, recursive: true);
error = null;
return true;
}
catch (Exception ex)
{
error = ex;
return false;
}
}
private static void CopyDirectoryRecursive(string sourceDir, string destDir, FinalizeReport report)
{
foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
{
var rel = Path.GetRelativePath(sourceDir, dir);
var targetDir = Path.Combine(destDir, rel);
try { Directory.CreateDirectory(targetDir); }
catch (Exception ex) { report.CopyFailures.Add(new Failure(targetDir, ex)); }
}
foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
{
var rel = Path.GetRelativePath(sourceDir, file);
var targetFile = Path.Combine(destDir, rel);
try
{
Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
File.Copy(file, targetFile, overwrite: true);
}
catch (Exception ex)
{
report.CopyFailures.Add(new Failure(file, ex));
}
}
}
public sealed record Failure(string Path, Exception Exception);
}
public sealed class FinalizeReport(string runningPath, string finishedPath)
{
public string RunningPath { get; } = runningPath;
public string FinishedPath { get; } = finishedPath;
public List<ExecutionTracker.Failure> CopyFailures { get; } = [];
public List<ExecutionTracker.Failure> DeleteFailures { get; } = [];
public bool IsClean => CopyFailures.Count == 0 && DeleteFailures.Count == 0;
}
public sealed class CleanupReport
{
public List<string> DeletedRunningFolders { get; } = [];
public List<ExecutionTracker.Failure> DeleteFailures { get; } = [];
public bool IsClean => DeleteFailures.Count == 0;
}
}

=== FILE: F:\Marketing\Configuration\PathsConfig.cs ===

﻿namespace Configuration
{
public class PathsConfig
{
public string OutFolder { get; set; }
public string DownloadFolder { get; set; }
}
}

=== FILE: F:\Marketing\Configuration\SchedulerOptions.cs ===

﻿namespace Configuration
{
public sealed class SchedulerOptions
{
public const string SectionName = "WhatsApp:Scheduler";
public bool Enabled { get; init; } = true;
public string TimeZoneId { get; init; } = "America/Vancouver";
public Dictionary<string, List<string>> Weekly { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
}

=== FILE: F:\Marketing\Configuration\SchedulerOptionsValidator.cs ===

﻿using System.Globalization;
using Microsoft.Extensions.Options;
namespace Configuration
{
public sealed class SchedulerOptionsValidator : IValidateOptions<SchedulerOptions>
{
public ValidateOptionsResult Validate(string? name, SchedulerOptions options)
{
var errors = new List<string>();
if (string.IsNullOrWhiteSpace(options.TimeZoneId))
errors.Add("Scheduler:TimeZoneId is required.");
if (!string.IsNullOrWhiteSpace(options.TimeZoneId))
{
try { TimeZoneInfo.FindSystemTimeZoneById(options.TimeZoneId); }
catch (TimeZoneNotFoundException)
{
errors.Add($"Scheduler:TimeZoneId '{options.TimeZoneId}' not found on this OS.");
}
catch (InvalidTimeZoneException)
{
errors.Add($"Scheduler:TimeZoneId '{options.TimeZoneId}' is invalid.");
}
}
foreach (var (day, times) in options.Weekly)
{
if (times is null) continue;
for (var i = 0; i < times.Count; i++)
{
var value = times[i];
if (!TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out _))
errors.Add($"Scheduler:Weekly:{day}[{i}] value '{value}' must be HH:mm (24-hour).");
}
}
return errors.Count == 0
? ValidateOptionsResult.Success
: ValidateOptionsResult.Fail(errors);
}
}
}

=== FILE: F:\Marketing\Configuration\WhatsAppConfig.cs ===

﻿namespace Configuration
{
public class WhatsAppConfig
{
public required string Url { get; set; }
public required TimeSpan LoginPollInterval { get; init; }
public required TimeSpan LoginTimeout { get; init; }
public required List<string> AllowedChatTargets { get; init; }
public required SchedulerOptions Scheduler { get; init; }
}
}

=== FILE: F:\Marketing\Configuration\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Configuration\obj\Debug\net8.0\Configuration.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Configuration")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+824c57be25fbd2217ad6312cf623c6356b3db7c2")]
[assembly: System.Reflection.AssemblyProductAttribute("Configuration")]
[assembly: System.Reflection.AssemblyTitleAttribute("Configuration")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Configuration\obj\Debug\net8.0\Configuration.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Configuration\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Configuration\obj\Release\net8.0\Configuration.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Configuration")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+1f7b61cc657b6b85a6c4b54e1aee2477c28bbe28")]
[assembly: System.Reflection.AssemblyProductAttribute("Configuration")]
[assembly: System.Reflection.AssemblyTitleAttribute("Configuration")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Configuration\obj\Release\net8.0\Configuration.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Domain\Entity.cs ===

﻿using Domain.Interfaces.Entity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
namespace Domain
{
public class Entity : IEntity
{
[Key]
[Required(ErrorMessage = "Id is required.")]
public required string Id { get; set; }
[SetsRequiredMembers]
public Entity(string id)
{
ArgumentNullException.ThrowIfNull(id);
if (string.IsNullOrWhiteSpace(id))
{
throw new ArgumentException("Id cannot be empty or whitespace.", nameof(id));
}
Id = id;
}
public bool Active { get; set; }
}
}

=== FILE: F:\Marketing\Domain\ErrorLog.cs ===

﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
namespace Domain
{
[method: SetsRequiredMembers]
public class ErrorLog(string id) : Entity(id)
{
public DateTime Timestamp { get; set; } = DateTime.UtcNow;
[Required(ErrorMessage = "Level is required.")]
[MinLength(5, ErrorMessage = "Level must be at least 3 characters long.")]
[MaxLength(150, ErrorMessage = "Level must be maximun 150 characters long.")]
public required string Level { get; set; }
[Required(ErrorMessage = "Message is required.")]
[MinLength(5, ErrorMessage = "Message must be at least 3 characters long.")]
[MaxLength(150, ErrorMessage = "Message must be maximun 150 characters long.")]
public required string Message { get; set; }
[Required(ErrorMessage = "ExceptionType is required.")]
[MinLength(5, ErrorMessage = "ExceptionType must be at least 3 characters long.")]
[MaxLength(150, ErrorMessage = "ExceptionType must be maximun 150 characters long.")]
public string? ExceptionType { get; set; }
[Required(ErrorMessage = "StackTrace is required.")]
[MinLength(5, ErrorMessage = "StackTrace must be at least 3 characters long.")]
[MaxLength(150, ErrorMessage = "StackTrace must be maximun 150 characters long.")]
public string? StackTrace { get; set; }
[Required(ErrorMessage = "Context is required.")]
[MinLength(5, ErrorMessage = "Context must be at least 3 characters long.")]
[MaxLength(150, ErrorMessage = "Context must be maximun 150 characters long.")]
public string? Context { get; set; }
}
}

=== FILE: F:\Marketing\Domain\Interfaces\Entity\IActivatable.cs ===

﻿namespace Domain.Interfaces.Entity
{
public interface IActivatable
{
bool Active { get; set; }
}
}

=== FILE: F:\Marketing\Domain\Interfaces\Entity\IEntity.cs ===

﻿namespace Domain.Interfaces.Entity
{
public interface IEntity : IIdentifiable, IActivatable {}
}

=== FILE: F:\Marketing\Domain\Interfaces\Entity\IIdentifiable.cs ===

﻿namespace Domain.Interfaces.Entity
{
public interface IIdentifiable
{
string Id { get; }
}
}

=== FILE: F:\Marketing\Domain\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Domain\obj\Debug\net8.0\Domain.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Domain")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+4f69044b1c8e17b1586ebe3470a52fa586c2ee1f")]
[assembly: System.Reflection.AssemblyProductAttribute("Domain")]
[assembly: System.Reflection.AssemblyTitleAttribute("Domain")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Domain\obj\Debug\net8.0\Domain.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Infrastructure\Class1.cs ===

﻿namespace Infrastructure
{
public class Class1
{
}
}

=== FILE: F:\Marketing\Infrastructure\Constants\Message.cs ===

﻿namespace Infrastructure.Constants
{
public static class Message
{
public static class GuidValidator
{
public const string InvalidGuid = "The submitted value was invalid.";
public const string Success = "Success";
}
}
}

=== FILE: F:\Marketing\Infrastructure\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Infrastructure\obj\Debug\net8.0\Infrastructure.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Infrastructure")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+2889c604867e086d29f73bae138892e6e430cba5")]
[assembly: System.Reflection.AssemblyProductAttribute("Infrastructure")]
[assembly: System.Reflection.AssemblyTitleAttribute("Infrastructure")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Infrastructure\obj\Debug\net8.0\Infrastructure.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Create\CreateLabels.Designer.cs ===

﻿
namespace Infrastructure.Repositories.Abstract.CRUD.Create {
using System;
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
internal class CreateLabels {
private static global::System.Resources.ResourceManager resourceMan;
private static global::System.Globalization.CultureInfo resourceCulture;
[global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
internal CreateLabels() {
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Resources.ResourceManager ResourceManager {
get {
if (object.ReferenceEquals(resourceMan, null)) {
global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Infrastructure.Repositories.Abstract.CRUD.Create.CreateLabels", typeof(CreateLabels).Assembly);
resourceMan = temp;
}
return resourceMan;
}
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Globalization.CultureInfo Culture {
get {
return resourceCulture;
}
set {
resourceCulture = value;
}
}
internal static string CreationSuccess {
get {
return ResourceManager.GetString("CreationSuccess", resourceCulture);
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Create\CreateRepository.cs ===

﻿using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;
namespace Infrastructure.Repositories.Abstract.CRUD.Create
{
public abstract class CreateRepository<T>(IUnitOfWork unitOfWork)
: RepositoryCreate<T>(unitOfWork), ICreate<T> where T : class, IEntity
{
public async Task<Operation<bool>> CreateEntity(T entity)
{
await Create(entity);
var success = CreateLabels.CreationSuccess;
var message = string.Format(success, typeof(T).Name);
return Operation<bool>.Success(true, message);
}
public async Task<Operation<bool>> CreateEntities(List<T> entities)
{
await CreateRange(entities);
var success = CreateLabels.CreationSuccess;
var message = string.Format(success, typeof(T).Name);
return Operation<bool>.Success(true, message);
}
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Delete\DeleteLabels.Designer.cs ===

﻿
namespace Infrastructure.Repositories.Abstract.CRUD.Delete {
using System;
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
internal class DeleteLabels {
private static global::System.Resources.ResourceManager resourceMan;
private static global::System.Globalization.CultureInfo resourceCulture;
[global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
internal DeleteLabels() {
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Resources.ResourceManager ResourceManager {
get {
if (object.ReferenceEquals(resourceMan, null)) {
global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Infrastructure.Repositories.Abstract.CRUD.Delete.DeleteLabels", typeof(DeleteLabels).Assembly);
resourceMan = temp;
}
return resourceMan;
}
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Globalization.CultureInfo Culture {
get {
return resourceCulture;
}
set {
resourceCulture = value;
}
}
internal static string DeletionSuccess {
get {
return ResourceManager.GetString("DeletionSuccess", resourceCulture);
}
}
internal static string EntityNotFound {
get {
return ResourceManager.GetString("EntityNotFound", resourceCulture);
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Delete\DeleteRepository.cs ===

﻿using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;
namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
public abstract class DeleteRepository<T>(IUnitOfWork unitOfWork)
: RepositoryDelete<T>(unitOfWork), IDelete<T> where T : class, IEntity
{
public async Task<Operation<bool>> DeleteEntity(string id)
{
var entity = await HasId(id);
if (entity is null)
{
var strategy = new BusinessStrategy<bool>();
return OperationStrategy<bool>.Fail(DeleteLabels.EntityNotFound, strategy);
}
Delete(entity);
var success = DeleteLabels.DeletionSuccess;
var message = string.Format(success, typeof(T).Name);
return Operation<bool>.Success(true, message);
}
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Query\Read\ReadRepository.cs ===

﻿using Domain.Interfaces.Entity;
using Application.Result;
using Microsoft.EntityFrameworkCore;
using Application.Common.Pagination;
using System.Linq.Expressions;
using Persistence.Context.Interface;
namespace Infrastructure.Repositories.Abstract.CRUD.Query.Read
{
public abstract class ReadRepository<T>(
IUnitOfWork unitOfWork,
Func<IQueryable<T>, IOrderedQueryable<T>> orderBy
) where T : class, IEntity
{
public async Task<Operation<PagedResult<T>>> GetPageAsync(
Expression<Func<T, bool>>? filter,
string? cursor,
int pageSize
)
{
var query = BuildBaseQuery(filter);
var count = query.Count();
if (!string.IsNullOrEmpty(cursor))
query = ApplyCursorFilter(query, cursor);
var items = await query.Take(pageSize + 1).ToListAsync();
var next = BuildNextCursor(items, pageSize);
if (next != null)
items.RemoveAt(pageSize);
var result = new PagedResult<T>
{
Items = items,
NextCursor = next,
TotalCount = count
};
return Operation<PagedResult<T>>.Success(result);
}
public async Task<Operation<PagedResult<T>>> GetAllMembers(CancellationToken cancellationToken = default)
{
var query = unitOfWork.Context.Set<T>().AsNoTracking();
var items = await query.ToListAsync(cancellationToken);
var result = new PagedResult<T>
{
Items = items,
NextCursor = null,
TotalCount = items.Count
};
return Operation<PagedResult<T>>.Success(result);
}
protected virtual IQueryable<T> BuildBaseQuery(Expression<Func<T, bool>>? filter)
{
var q = unitOfWork.Context.Set<T>().AsNoTracking();
if (filter != null)
{
q = q.Where(filter);
}
return orderBy(q);
}
protected abstract IQueryable<T> ApplyCursorFilter(IQueryable<T> query, string cursor);
protected abstract string? BuildNextCursor(List<T> items, int size);
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Query\ReadId\ReadByIdRepository.cs ===

﻿using Application.Result;
using Application.UseCases.Repository.CRUD.Query;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;
namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadId
{
public abstract class ReadByIdRepository<T>(
IUnitOfWork unitOfWork,
IErrorHandler errorHandler
) : EntityChecker<T>(unitOfWork), IReadById<T> where T : class, IEntity
{
public async Task<Operation<T>> ReadById(string id)
{
var found = await HasId(id);
if (found is null)
{
var strategy = new BusinessStrategy<T>();
return OperationStrategy<T>.Fail(ReadIdLabels.EntityNotFound, strategy);
}
var success = ReadIdLabels.ReadIdSuccess;
return Operation<T>.Success(found, success);
}
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Query\ReadId\ReadIdLabels.Designer.cs ===

﻿
namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadId {
using System;
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
internal class ReadIdLabels {
private static global::System.Resources.ResourceManager resourceMan;
private static global::System.Globalization.CultureInfo resourceCulture;
[global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
internal ReadIdLabels() {
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Resources.ResourceManager ResourceManager {
get {
if (object.ReferenceEquals(resourceMan, null)) {
global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Infrastructure.Repositories.Abstract.CRUD.Query.ReadId.ReadIdLabels", typeof(ReadIdLabels).Assembly);
resourceMan = temp;
}
return resourceMan;
}
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Globalization.CultureInfo Culture {
get {
return resourceCulture;
}
set {
resourceCulture = value;
}
}
internal static string EntityNotFound {
get {
return ResourceManager.GetString("EntityNotFound", resourceCulture);
}
}
internal static string ReadIdSuccess {
get {
return ResourceManager.GetString("ReadIdSuccess", resourceCulture);
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Update\UpdateLabels.Designer.cs ===

﻿
namespace Infrastructure.Repositories.Abstract.CRUD.Update {
using System;
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
internal class UpdateLabels {
private static global::System.Resources.ResourceManager resourceMan;
private static global::System.Globalization.CultureInfo resourceCulture;
[global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
internal UpdateLabels() {
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Resources.ResourceManager ResourceManager {
get {
if (object.ReferenceEquals(resourceMan, null)) {
global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Infrastructure.Repositories.Abstract.CRUD.Update.UpdateLabels", typeof(UpdateLabels).Assembly);
resourceMan = temp;
}
return resourceMan;
}
}
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
internal static global::System.Globalization.CultureInfo Culture {
get {
return resourceCulture;
}
set {
resourceCulture = value;
}
}
internal static string EntityNotFound {
get {
return ResourceManager.GetString("EntityNotFound", resourceCulture);
}
}
internal static string UpdationSuccess {
get {
return ResourceManager.GetString("UpdationSuccess", resourceCulture);
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\Abstract\CRUD\Update\UpdateRepository.cs ===

﻿using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;
namespace Infrastructure.Repositories.Abstract.CRUD.Update
{
public abstract class UpdateRepository<T>(IUnitOfWork unitOfWork)
: RepositoryUpdate<T>(unitOfWork), IUpdate<T>
where T : class, IEntity
{
public async Task<Operation<bool>> UpdateEntity(T modify)
{
var entity = await HasId(modify.Id);
if (entity is null)
{
var strategy = new BusinessStrategy<bool>();
return OperationStrategy<bool>.Fail(UpdateLabels.EntityNotFound, strategy);
}
var modified = ApplyUpdates(modify, entity);
Update(modified);
var success = UpdateLabels.UpdationSuccess;
var message = string.Format(success, typeof(T).Name);
return Operation<bool>.Success(true, message);
}
public abstract T ApplyUpdates(T modified, T unmodified);
}
}

=== FILE: F:\Marketing\Infrastructure\Repositories\CRUD\ErrorLogCreate.cs ===

﻿using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;
namespace Infrastructure.Repositories.CRUD
{
using ErrorLog = Domain.ErrorLog;
public class ErrorLogCreate(IUnitOfWork unitOfWork) : CreateRepository<ErrorLog>(unitOfWork), IErrorLogCreate
{
public async Task<Operation<bool>> CreateInvoiceAsync(ErrorLog entity)
{
return await CreateEntity(entity);
}
}
}

=== FILE: F:\Marketing\Infrastructure\Result\ErrorHandler.cs ===

﻿
using Application.Result;
using Application.Result.Error;
using System.Collections.Concurrent;
using System.Text.Json;
namespace Infrastructure.Result
{
public sealed class ErrorHandler : IErrorHandler
{
private readonly IErrorLogger _errorLogger;
private static readonly Lazy<ConcurrentDictionary<string, string>> ErrorMappings
= new(() => new ConcurrentDictionary<string, string>());
private static readonly IDictionary<string, string> DefaultMappings = new Dictionary<string, string>
{
{ "SqliteException",      "DatabaseStrategy" },
{ "HttpRequestException", "NetworkErrorStrategy" },
{ "JsonException",        "InvalidDataStrategy" },
{ "Exception",            "UnexpectedErrorStrategy" }
};
public ErrorHandler(IErrorLogger errorLogger)
{
_errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
}
public Operation<T> Fail<T>(Exception? ex, string? errorMessage = null)
{
if (ex is null)
return new NullExceptionStrategy<T>().CreateFailure("Exception is null.");
if (ErrorMappings.Value.IsEmpty)
return new NullExceptionStrategy<T>().CreateFailure("ErrorMappings is not loaded or empty.");
if (!ErrorMappings.Value.TryGetValue(ex.GetType().Name, out var strategyName))
return new NullExceptionStrategy<T>().CreateFailure($"No strategy matches exception type: {ex.GetType().Name}.");
var strategy = CreateStrategyInstance<T>(strategyName);
var op = string.IsNullOrWhiteSpace(errorMessage)
? strategy.CreateFailure()
: strategy.CreateFailure(errorMessage);
_ = SafeLogAsync(ex);
return op;
}
public Operation<T> Business<T>(string errorMessage)
=> new BusinessStrategy<T>().CreateFailure(errorMessage);
public void LoadErrorMappings(string filePath)
{
foreach (var kv in DefaultMappings)
ErrorMappings.Value[kv.Key] = kv.Value;
if (!File.Exists(filePath))
throw new FileNotFoundException($"Error mappings file not found: {filePath}");
try
{
var jsonContent = File.ReadAllText(filePath);
var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
if (mappings is null || mappings.Count == 0)
throw new InvalidOperationException("ErrorMappings.json is empty or invalid.");
foreach (var kvp in mappings)
ErrorMappings.Value[kvp.Key] = kvp.Value;
}
catch (JsonException ex)
{
throw new InvalidOperationException("ErrorMappings.json contains invalid JSON format.", ex);
}
catch (Exception ex)
{
throw new Exception("An error occurred while loading error mappings.", ex);
}
}
public bool Any() => ErrorMappings.IsValueCreated && !ErrorMappings.Value.IsEmpty;
private static IErrorCreationStrategy<T> CreateStrategyInstance<T>(string strategyName) =>
strategyName switch
{
"NetworkErrorStrategy" => new NetworkErrorStrategy<T>(),
"ConfigMissingStrategy" => new ConfigMissingStrategy<T>(),
"InvalidDataStrategy" => new InvalidDataStrategy<T>(),
"DatabaseStrategy" => new DatabaseStrategy<T>(),
"UnexpectedErrorStrategy" => new UnexpectedErrorStrategy<T>(),
_ => new UnexpectedErrorStrategy<T>()
};
private async Task SafeLogAsync(Exception ex)
{
try
{
await _errorLogger.LogAsync(ex).ConfigureAwait(false);
}
catch
{
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\Utilities\GuidValidator.cs ===

﻿using Application.Result;
using Infrastructure.Constants;
namespace Infrastructure.Utilities
{
public class GuidValidator
{
public static Operation<string> HasGuid(string id)
{
bool isSuccess = Guid.TryParse(id, out _);
if (!isSuccess)
{
var business = new BusinessStrategy<string>();
var invalidGuidMessage = Message.GuidValidator.InvalidGuid;
return OperationStrategy<string>.Fail(invalidGuidMessage, business);
}
return Operation<string>.Success(id, Message.GuidValidator.Success);
}
}
}

=== FILE: F:\Marketing\Marketing.Tests\ErrorHandlerTests.cs ===

﻿using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Result;
using Application.Result.Error;
using FluentAssertions;
using Infrastructure.Result;
using Xunit;
namespace Marketing.Tests;
public sealed class ErrorHandlerTests
{
private static string CreateTempMappingsFile(object mappings)
{
var path = Path.Combine(Path.GetTempPath(), $"ErrorMappings_{Guid.NewGuid():N}.json");
File.WriteAllText(path, JsonSerializer.Serialize(mappings));
return path;
}
private sealed class SpyErrorLogger : IErrorLogger
{
private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
public int CallCount { get; private set; }
public Exception? LastException { get; private set; }
public Task WhenCalled => _tcs.Task;
public Task LogAsync(Exception ex, CancellationToken cancellationToken = default)
{
CallCount++;
LastException = ex;
_tcs.TrySetResult(true);
return Task.CompletedTask;
}
}
private sealed class ThrowingErrorLogger : IErrorLogger
{
public Task LogAsync(Exception ex, CancellationToken cancellationToken = default)
=> throw new InvalidOperationException("Logger failed");
}
private static void ResetStaticMappings()
{
var type = typeof(ErrorHandler);
var field = type.GetField("ErrorMappings",
System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
field.Should().NotBeNull("ErrorMappings static field must exist for test isolation.");
var lazy = field!.GetValue(null);
lazy.Should().NotBeNull();
var valueProp = lazy!.GetType().GetProperty("Value");
valueProp.Should().NotBeNull();
var dict = valueProp!.GetValue(lazy);
dict.Should().NotBeNull();
var clearMethod = dict!.GetType().GetMethod("Clear");
clearMethod.Should().NotBeNull();
clearMethod!.Invoke(dict, null);
}
private static ErrorHandler CreateSut(IErrorLogger logger)
=> new ErrorHandler(logger);
[Fact]
public void Ctor_When_logger_is_null_should_throw()
{
var act = () => new ErrorHandler(null!);
act.Should().Throw<ArgumentNullException>();
}
[Fact]
public void Fail_When_exception_is_null_should_return_NullExceptionStrategy_failure()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
var op = sut.Fail<int>(null);
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.NullExceptionStrategy);
op.Message.Should().Be("Exception is null.");
}
[Fact]
public void Fail_When_mappings_not_loaded_should_return_NullExceptionStrategy_failure()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
var op = sut.Fail<int>(new Exception("x"));
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.NullExceptionStrategy);
op.Message.Should().Be("ErrorMappings is not loaded or empty.");
}
[Fact]
public void Fail_When_exception_type_not_in_mappings_should_return_NullExceptionStrategy_failure()
{
ResetStaticMappings();
var logger = new SpyErrorLogger();
var sut = CreateSut(logger);
var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
sut.LoadErrorMappings(file);
var op = sut.Fail<int>(new InvalidOperationException("not mapped"));
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.NullExceptionStrategy);
op.Message.Should().StartWith("No strategy matches exception type:");
logger.CallCount.Should().Be(0, "logging should not occur when no strategy matches");
}
[Fact]
public async Task Fail_When_mapped_and_no_custom_message_should_use_strategy_description_and_log()
{
ResetStaticMappings();
var logger = new SpyErrorLogger();
var sut = CreateSut(logger);
var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
sut.LoadErrorMappings(file);
var ex = new HttpRequestException("network");
var op = sut.Fail<int>(ex);
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.Network);
op.Message.Should().Be("Occurs due to a network connectivity issue.");
await logger.WhenCalled.WaitAsync(TimeSpan.FromSeconds(2));
logger.CallCount.Should().Be(1);
logger.LastException.Should().BeSameAs(ex);
}
[Fact]
public async Task Fail_When_mapped_and_custom_message_should_use_custom_message_and_log()
{
ResetStaticMappings();
var logger = new SpyErrorLogger();
var sut = CreateSut(logger);
var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
sut.LoadErrorMappings(file);
var ex = new JsonException("bad json");
var op = sut.Fail<int>(ex, "payload invalid");
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.InvalidData);
op.Message.Should().Be("payload invalid");
await logger.WhenCalled.WaitAsync(TimeSpan.FromSeconds(2));
logger.CallCount.Should().Be(1);
logger.LastException.Should().BeSameAs(ex);
}
[Fact]
public void Fail_When_logger_throws_should_still_return_operation()
{
ResetStaticMappings();
var sut = CreateSut(new ThrowingErrorLogger());
var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
sut.LoadErrorMappings(file);
var ex = new HttpRequestException("network");
var op = sut.Fail<int>(ex);
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.Network);
}
[Fact]
public void Fail_When_mapping_points_to_unknown_strategy_should_fallback_to_Unexpected()
{
ResetStaticMappings();
var logger = new SpyErrorLogger();
var sut = CreateSut(logger);
var file = CreateTempMappingsFile(new { InvalidOperationException = "DoesNotExistStrategy" });
sut.LoadErrorMappings(file);
var op = sut.Fail<int>(new InvalidOperationException("x"));
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.Unexpected);
op.Message.Should().Be("Occurs for any unexpected or unclassified error.");
}
[Fact]
public void Business_Should_return_BusinessValidation_failure_with_message()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
var op = sut.Business<int>("invalid input");
op.IsSuccessful.Should().BeFalse();
op.Type.Should().Be(ErrorTypes.BusinessValidation);
op.Message.Should().Be("invalid input");
}
[Fact]
public void LoadErrorMappings_When_file_missing_should_throw_FileNotFoundException()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
var act = () => sut.LoadErrorMappings(Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.json"));
act.Should().Throw<FileNotFoundException>();
}
[Fact]
public void LoadErrorMappings_When_json_invalid_should_throw_InvalidOperationException_with_inner_JsonException()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
var path = Path.Combine(Path.GetTempPath(), $"badjson_{Guid.NewGuid():N}.json");
File.WriteAllText(path, "{ not valid json }");
var act = () => sut.LoadErrorMappings(path);
act.Should().Throw<InvalidOperationException>()
.WithMessage("ErrorMappings.json contains invalid JSON format.")
.WithInnerException<JsonException>();
}
[Fact]
public void LoadErrorMappings_When_json_empty_object_should_throw_wrapped_exception()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
var file = CreateTempMappingsFile(new { });
Action act = () => sut.LoadErrorMappings(file);
act.Should()
.Throw<Exception>()
.WithMessage("An error occurred while loading error mappings.")
.WithInnerException<InvalidOperationException>()
.Which.Message.Should().Be("ErrorMappings.json is empty or invalid.");
}
[Fact]
public void LoadErrorMappings_When_file_does_not_exist_should_throw_FileNotFoundException()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
var missingFile = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.json");
Action act = () => sut.LoadErrorMappings(missingFile);
act.Should()
.Throw<FileNotFoundException>()
.WithMessage($"Error mappings file not found: {missingFile}");
}
[Fact]
public void Any_Should_be_false_before_load_and_true_after_successful_load()
{
ResetStaticMappings();
var sut = CreateSut(new SpyErrorLogger());
sut.Any().Should().BeFalse();
var file = CreateTempMappingsFile(new { CustomException = "UnexpectedErrorStrategy" });
sut.LoadErrorMappings(file);
sut.Any().Should().BeTrue();
}
}

=== FILE: F:\Marketing\Marketing.Tests\GuidValidatorTests.cs ===

﻿using FluentAssertions;
using Infrastructure.Utilities;
using Xunit;
namespace Marketing.Tests
{
public class GuidValidatorTests
{
[Theory]
[InlineData("1b4e28ba-2fa1-11d2-883f-0016d3cca427")]
[InlineData("6F9619FF-8B86-D011-B42D-00C04FC964FF")]
public void HasGuid_When_valid_guid_returns_success(string id)
{
var op = GuidValidator.HasGuid(id);
op.IsSuccessful.Should().BeTrue();
op.Data.Should().Be(id);
op.Message.Should().NotBeNullOrWhiteSpace();
}
[Theory]
[InlineData("not-a-guid")]
[InlineData("")]
public void HasGuid_When_invalid_returns_business_failure(string id)
{
var op = GuidValidator.HasGuid(id);
op.IsSuccessful.Should().BeFalse();
op.Data.Should().BeNull();
}
}
}

=== FILE: F:\Marketing\Marketing.Tests\OperationTests.cs ===

﻿using System;
using Application.Result;
using Application.Result.Error;
using Application.Result.Exceptions;
using FluentAssertions;
using Xunit;
namespace Marketing.Tests;
public class OperationTests
{
[Fact]
public void Success_When_message_is_null_should_normalize_to_empty_string()
{
var op = Operation<int>.Success(10, null);
op.IsSuccessful.Should().BeTrue();
op.Data.Should().Be(10);
op.Message.Should().Be(string.Empty);
op.Type.Should().Be(ErrorTypes.None);
}
[Fact]
public void Success_When_message_is_omitted_should_default_to_empty_string()
{
var op = Operation<int>.Success(10);
op.IsSuccessful.Should().BeTrue();
op.Data.Should().Be(10);
op.Message.Should().Be(string.Empty);
op.Type.Should().Be(ErrorTypes.None);
}
[Fact]
public void Success_When_data_is_null_should_still_be_success()
{
var op = Operation<string>.Success(null, "ok");
op.IsSuccessful.Should().BeTrue();
op.Data.Should().BeNull();
op.Message.Should().Be("ok");
op.Type.Should().Be(ErrorTypes.None);
}
[Fact]
public void Success_Should_allow_empty_message()
{
var op = Operation<int>.Success(1, "");
op.IsSuccessful.Should().BeTrue();
op.Message.Should().Be(string.Empty);
op.Type.Should().Be(ErrorTypes.None);
}
[Theory]
[InlineData(ErrorTypes.Unexpected)]
[InlineData(ErrorTypes.BusinessValidation)]
[InlineData(ErrorTypes.Database)]
public void Failure_Should_set_failure_fields(ErrorTypes type)
{
var op = Operation<int>.Failure("boom", type);
op.IsSuccessful.Should().BeFalse();
op.Data.Should().Be(default(int));
op.Message.Should().Be("boom");
op.Type.Should().Be(type);
}
[Fact]
public void Failure_Should_allow_null_message_and_preserve_it()
{
var op = Operation<int>.Failure(null!, ErrorTypes.Unexpected);
op.IsSuccessful.Should().BeFalse();
op.Message.Should().BeNull();
op.Type.Should().Be(ErrorTypes.Unexpected);
}
[Fact]
public void AsType_When_called_on_success_should_throw_InvalidOperation_with_exact_message()
{
var op = Operation<int>.Success(1, "ok");
var act = () => op.AsType<string>();
act.Should()
.Throw<InvalidOperation>()
.WithMessage("This method can only be used if the value of IsSuccessful is false.");
}
[Fact]
public void ConvertTo_When_called_on_success_should_throw_same_InvalidOperation()
{
var op = Operation<int>.Success(1);
var act = () => op.ConvertTo<string>();
act.Should()
.Throw<InvalidOperation>()
.WithMessage("This method can only be used if the value of IsSuccessful is false.");
}
[Fact]
public void AsType_When_called_on_failure_should_preserve_message_and_type_and_return_failure()
{
var op = Operation<int>.Failure("bad", ErrorTypes.BusinessValidation);
var converted = op.AsType<string>();
converted.IsSuccessful.Should().BeFalse();
converted.Data.Should().BeNull();
converted.Message.Should().Be("bad");
converted.Type.Should().Be(ErrorTypes.BusinessValidation);
}
[Fact]
public void AsType_When_called_on_failure_should_work_for_value_types()
{
var op = Operation<string>.Failure("bad", ErrorTypes.InvalidData);
var converted = op.AsType<int>();
converted.IsSuccessful.Should().BeFalse();
converted.Data.Should().Be(default(int));
converted.Message.Should().Be("bad");
converted.Type.Should().Be(ErrorTypes.InvalidData);
}
[Fact]
public void ConvertTo_Should_be_equivalent_to_AsType_for_failures()
{
var op = Operation<Guid>.Failure("x", ErrorTypes.Network);
var a = op.AsType<int>();
var b = op.ConvertTo<int>();
b.IsSuccessful.Should().Be(a.IsSuccessful);
b.Type.Should().Be(a.Type);
b.Message.Should().Be(a.Message);
b.Data.Should().Be(a.Data);
}
[Fact]
public void Multiple_conversions_should_be_stable_and_not_mutate_original()
{
var original = Operation<int>.Failure("bad", ErrorTypes.Timeout);
var c1 = original.AsType<string>();
var c2 = original.AsType<Guid>();
original.IsSuccessful.Should().BeFalse();
original.Message.Should().Be("bad");
original.Type.Should().Be(ErrorTypes.Timeout);
c1.Message.Should().Be("bad");
c1.Type.Should().Be(ErrorTypes.Timeout);
c2.Message.Should().Be("bad");
c2.Type.Should().Be(ErrorTypes.Timeout);
}
}

=== FILE: F:\Marketing\Marketing.Tests\PagingTests.cs ===

﻿using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Marketing.Tests.Integration.Db;
using Persistence.CreateStructure.Constants.ColumnType;
using Xunit;
using Marketing.Tests.Integration.TestEntities;
namespace Marketing.Tests
{
public class PagingTests
{
[Fact]
public async Task Smoke_Can_create_db_and_insert()
{
using var conn = new SqliteConnection("DataSource=:memory:");
conn.Open();
var options = new DbContextOptionsBuilder()
.UseSqlite(conn)
.Options;
IColumnTypes columnTypes = new TestColumnTypes();
await using var ctx = new TestDataContext(options, columnTypes);
await ctx.Database.EnsureCreatedAsync();
ctx.Set<TestEntity>()
.Add(new() { Id = "001", Active = true });
await ctx.SaveChangesAsync();
var count = await ctx.Set<TestEntity>().CountAsync();
count.Should().Be(1);
}
}
}

=== FILE: F:\Marketing\Marketing.Tests\SanityTests.cs ===

﻿using FluentAssertions;
using Xunit;
namespace Marketing.Tests
{
public class SanityTests
{
[Fact]
public void TestRunner_Works()
{
true.Should().BeTrue();
}
}
}

=== FILE: F:\Marketing\Marketing.Tests\Integration\ReadRepositoryTests.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Pagination;
using Application.Result;
using Domain.Interfaces.Entity;
using FluentAssertions;
using Infrastructure.Repositories.Abstract.CRUD.Query.Read;
using Marketing.Tests.Integration.Db;
using Marketing.Tests.Integration.TestEntities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Xunit;
namespace Marketing.Tests.Integration;
public sealed class ReadRepositoryTests
{
private sealed class TestReadRepo : ReadRepository<TestEntity>
{
public int ApplyCursorFilterCallCount { get; private set; }
public TestReadRepo(IUnitOfWork uow, Func<IQueryable<TestEntity>, IOrderedQueryable<TestEntity>> orderBy)
: base(uow, orderBy)
{
}
protected override IQueryable<TestEntity> ApplyCursorFilter(IQueryable<TestEntity> query, string cursor)
{
ApplyCursorFilterCallCount++;
return query.Where(x => string.CompareOrdinal(x.Id, cursor) > 0);
}
protected override string? BuildNextCursor(List<TestEntity> items, int size)
{
if (size <= 0) return null;
return items.Count > size ? items[size - 1].Id : null;
}
}
private static async Task<(SqliteConnection conn, TestDataContext ctx)> CreateContextAsync()
{
var conn = new SqliteConnection("DataSource=:memory:");
await conn.OpenAsync();
var options = new DbContextOptionsBuilder()
.UseSqlite(conn)
.Options;
IColumnTypes columnTypes = new TestColumnTypes();
var ctx = new TestDataContext(options, columnTypes);
await ctx.Database.EnsureCreatedAsync();
return (conn, ctx);
}
private static IUnitOfWork CreateUnitOfWork(TestDataContext ctx)
{
var uow = new Mock<IUnitOfWork>();
uow.SetupGet(x => x.Context).Returns(ctx);
return uow.Object;
}
private static async Task SeedAsync(TestDataContext ctx, params (string id, bool active)[] rows)
{
ctx.Set<TestEntity>().AddRange(rows.Select(r => new TestEntity { Id = r.id, Active = r.active }));
await ctx.SaveChangesAsync();
}
private static Func<IQueryable<TestEntity>, IOrderedQueryable<TestEntity>> OrderByIdAsc()
=> q => q.OrderBy(x => x.Id);
private static Func<IQueryable<TestEntity>, IOrderedQueryable<TestEntity>> OrderByIdDesc()
=> q => q.OrderByDescending(x => x.Id);
[Fact]
public async Task GetAllMembers_When_empty_returns_empty_items_next_null_total_0()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetAllMembers();
op.IsSuccessful.Should().BeTrue();
op.Data.Should().NotBeNull();
op.Data!.Items.Should().BeEmpty();
op.Data.NextCursor.Should().BeNull();
op.Data.TotalCount.Should().Be(0);
}
[Fact]
public async Task GetAllMembers_When_has_rows_returns_all_items_and_totalcount()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx,
("003", true),
("001", true),
("002", false));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetAllMembers();
op.IsSuccessful.Should().BeTrue();
op.Data!.TotalCount.Should().Be(3);
op.Data.NextCursor.Should().BeNull();
op.Data.Items.Should().HaveCount(3);
}
[Fact]
public async Task GetAllMembers_Should_use_AsNoTracking_no_tracked_entities_created_by_query()
{
var (conn, ctx1) = await CreateContextAsync();
await using var __ = conn;
await SeedAsync(ctx1, ("001", true), ("002", true));
await ctx1.DisposeAsync();
var options = new DbContextOptionsBuilder()
.UseSqlite(conn)
.Options;
IColumnTypes columnTypes = new TestColumnTypes();
await using var ctx2 = new TestDataContext(options, columnTypes);
var repo = new TestReadRepo(CreateUnitOfWork(ctx2), OrderByIdAsc());
ctx2.ChangeTracker.Entries<TestEntity>().Should().BeEmpty();
_ = await repo.GetAllMembers();
ctx2.ChangeTracker.Entries<TestEntity>().Should().BeEmpty();
}
[Fact]
public async Task GetAllMembers_When_cancelled_should_throw_OperationCanceledException()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
using var cts = new CancellationTokenSource();
cts.Cancel();
Func<Task> act = async () => await repo.GetAllMembers(cts.Token);
await act.Should().ThrowAsync<OperationCanceledException>();
}
[Fact]
public async Task GetPageAsync_When_pageSize_less_than_total_returns_pageSize_items_and_next_cursor()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true), ("003", true), ("004", true), ("005", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 2);
op.IsSuccessful.Should().BeTrue();
op.Data!.TotalCount.Should().Be(5);
op.Data.Items.Select(x => x.Id).Should().Equal("001", "002");
op.Data.NextCursor.Should().Be("002");
}
[Fact]
public async Task GetPageAsync_When_pageSize_equals_total_returns_all_items_and_next_null()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true), ("003", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 3);
op.IsSuccessful.Should().BeTrue();
op.Data!.TotalCount.Should().Be(3);
op.Data.Items.Select(x => x.Id).Should().Equal("001", "002", "003");
op.Data.NextCursor.Should().BeNull();
}
[Fact]
public async Task GetPageAsync_When_pageSize_greater_than_total_returns_all_items_and_next_null()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 10);
op.IsSuccessful.Should().BeTrue();
op.Data!.TotalCount.Should().Be(2);
op.Data.Items.Select(x => x.Id).Should().Equal("001", "002");
op.Data.NextCursor.Should().BeNull();
}
[Fact]
public async Task GetPageAsync_When_empty_dataset_returns_empty_and_next_null_total_0()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 2);
op.IsSuccessful.Should().BeTrue();
op.Data!.TotalCount.Should().Be(0);
op.Data.Items.Should().BeEmpty();
op.Data.NextCursor.Should().BeNull();
}
[Fact]
public async Task GetPageAsync_Should_use_AsNoTracking_no_tracked_entities_created_by_query()
{
var (conn, ctx1) = await CreateContextAsync();
await using var __ = conn;
await SeedAsync(ctx1, ("001", true), ("002", true));
await ctx1.DisposeAsync();
var options = new DbContextOptionsBuilder()
.UseSqlite(conn)
.Options;
IColumnTypes columnTypes = new TestColumnTypes();
await using var ctx2 = new TestDataContext(options, columnTypes);
var repo = new TestReadRepo(CreateUnitOfWork(ctx2), OrderByIdAsc());
ctx2.ChangeTracker.Entries<TestEntity>().Should().BeEmpty();
_ = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 1);
ctx2.ChangeTracker.Entries<TestEntity>().Should().BeEmpty();
}
[Theory]
[InlineData(null)]
[InlineData("")]
public async Task GetPageAsync_When_cursor_is_null_or_empty_should_not_apply_cursor_filter(string? cursor)
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true), ("003", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: cursor, pageSize: 2);
repo.ApplyCursorFilterCallCount.Should().Be(0);
op.IsSuccessful.Should().BeTrue();
op.Data!.Items.Select(x => x.Id).Should().Equal("001", "002");
}
[Fact]
public async Task GetPageAsync_When_filter_is_provided_should_filter_items_and_TotalCount_reflects_filtered_count()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx,
("001", true),
("002", false),
("003", true),
("004", false),
("005", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
Expression<Func<TestEntity, bool>> filter = x => x.Active;
var op = await repo.GetPageAsync(filter, cursor: null, pageSize: 10);
op.IsSuccessful.Should().BeTrue();
op.Data!.TotalCount.Should().Be(3);
op.Data.Items.Select(x => x.Id).Should().Equal("001", "003", "005");
op.Data.NextCursor.Should().BeNull();
}
[Fact]
public async Task GetPageAsync_Should_apply_orderBy_before_taking_page()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("003", true), ("001", true), ("002", true), ("005", true), ("004", true));
var repoAsc = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var opAsc = await repoAsc.GetPageAsync(filter: null, cursor: null, pageSize: 3);
opAsc.IsSuccessful.Should().BeTrue();
opAsc.Data!.Items.Select(x => x.Id).Should().Equal("001", "002", "003");
opAsc.Data.NextCursor.Should().Be("003");
var repoDesc = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdDesc());
var opDesc = await repoDesc.GetPageAsync(filter: null, cursor: null, pageSize: 3);
opDesc.IsSuccessful.Should().BeTrue();
opDesc.Data!.Items.Select(x => x.Id).Should().Equal("005", "004", "003");
opDesc.Data.NextCursor.Should().Be("003");
}
[Fact]
public async Task GetPageAsync_When_pageSize_is_negative_returns_empty_items_next_null_totalcount_is_count()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true), ("003", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: -1);
op.IsSuccessful.Should().BeTrue();
op.Data!.TotalCount.Should().Be(3);
op.Data.Items.Should().BeEmpty();
op.Data.NextCursor.Should().BeNull();
}
[Fact]
public async Task GetPageAsync_When_more_items_exist_should_remove_extra_item_and_return_exact_pageSize()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true), ("003", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 2);
op.IsSuccessful.Should().BeTrue();
op.Data!.Items.Should().HaveCount(2);
op.Data.Items.Select(x => x.Id).Should().Equal("001", "002");
op.Data.NextCursor.Should().Be("002");
}
[Fact]
public async Task GetPageAsync_When_exact_pageSize_items_exist_should_not_remove_any_item_and_next_null()
{
var (conn, ctx) = await CreateContextAsync();
await using var _ = ctx;
await using var __ = conn;
await SeedAsync(ctx, ("001", true), ("002", true));
var repo = new TestReadRepo(CreateUnitOfWork(ctx), OrderByIdAsc());
var op = await repo.GetPageAsync(filter: null, cursor: null, pageSize: 2);
op.IsSuccessful.Should().BeTrue();
op.Data!.Items.Should().HaveCount(2);
op.Data.Items.Select(x => x.Id).Should().Equal("001", "002");
op.Data.NextCursor.Should().BeNull();
}
}

=== FILE: F:\Marketing\Marketing.Tests\Integration\TestDataContext.cs ===

﻿using Microsoft.EntityFrameworkCore;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants.ColumnType;
using Marketing.Tests.Integration.TestEntities;
namespace Marketing.Tests.Integration.Db;
internal sealed class TestDataContext : DataContext
{
public TestDataContext(DbContextOptions options, IColumnTypes columnTypes)
: base(options, columnTypes)
{
}
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
base.OnModelCreating(modelBuilder);
modelBuilder.Entity<TestEntity>(b =>
{
b.ToTable("TestEntities");
b.HasKey(x => x.Id);
b.Property(x => x.Id).HasMaxLength(64);
b.Property(x => x.Active);
});
}
}

=== FILE: F:\Marketing\Marketing.Tests\Integration\TestDbContextFactory.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Marketing.Tests.Integration
{
using System.Threading.Tasks;
using global::Marketing.Tests.Integration.Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants.ColumnType;
internal static class TestDbContextFactory
{
public static async Task<(SqliteConnection Connection, TestDataContext Context)> CreateContextAsync()
{
var connection = new SqliteConnection("DataSource=:memory:");
await connection.OpenAsync();
connection.CreateFunction<string, string, int>(
"StringCompareOrdinal",
(a, b) => string.CompareOrdinal(a, b)
);
var options = new DbContextOptionsBuilder()
.UseSqlite(connection)
.Options;
IColumnTypes columnTypes = new TestColumnTypes();
var context = new TestDataContext(options, columnTypes);
await context.Database.EnsureCreatedAsync();
return (connection, context);
}
}
}

=== FILE: F:\Marketing\Marketing.Tests\Integration\Db\TestColumnTypes.cs ===

﻿using Persistence.CreateStructure.Constants.ColumnType;
namespace Marketing.Tests.Integration.Db;
internal sealed class TestColumnTypes : IColumnTypes
{
public string Id => TypeVar64;
public string Guid => TypeVar64;
public string String => TypeVar;
public string ShortString => TypeVar50;
public string LongString => TypeVar;
public string Bool => TypeBool;
public string Int => Integer;
public string Long => Integer;
public string Decimal => "NUMERIC";
public string DateTime => TypeDateTime;
public string TypeBool => "INTEGER";
public string TypeTime => "TEXT";
public string TypeDateTime => "TEXT";
public string TypeDateTimeOffset => "TEXT";
public string TypeVar => "TEXT";
public string TypeVar50 => "TEXT";
public string TypeVar150 => "TEXT";
public string TypeVar64 => "TEXT";
public string TypeBlob => "BLOB";
public string Integer => "INTEGER";
public string Strategy => "SQLiteTest";
public object? SqlStrategy => null;
public string Name => "SQLiteTestColumnTypes";
public object? Value => null;
}

=== FILE: F:\Marketing\Marketing.Tests\Integration\TestEntities\TestEntity.cs ===

﻿using Domain.Interfaces.Entity;
namespace Marketing.Tests.Integration.TestEntities;
internal sealed class TestEntity : IEntity
{
public string Id { get; set; } = default!;
public bool Active { get; set; }
}

=== FILE: F:\Marketing\Marketing.Tests\obj\Debug\net9.0\.NETCoreApp,Version=v9.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]

=== FILE: F:\Marketing\Marketing.Tests\obj\Debug\net9.0\Marketing.Tests.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+824c57be25fbd2217ad6312cf623c6356b3db7c2")]
[assembly: System.Reflection.AssemblyProductAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyTitleAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Marketing.Tests\obj\Debug\net9.0\Marketing.Tests.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
global using global::Xunit;

=== FILE: F:\Marketing\Persistence\Class1.cs ===

﻿namespace Persistence
{
public class Class1
{
}
}

=== FILE: F:\Marketing\Persistence\Context\Implementation\DataContext.cs ===

﻿using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
namespace Persistence.Context.Implementation
{
public class DataContext(DbContextOptions options, IColumnTypes columnTypes) : DbContext(options), IDataContext
{
protected readonly IColumnTypes _columnTypes = columnTypes;
public virtual bool Initialize()
{
try
{
Database.Migrate();
return true;
}
catch (Exception ex)
{
Console.WriteLine("An error occurred while initializing the database:");
Console.WriteLine(ex.Message);
Console.WriteLine(ex.StackTrace);
return false;
}
}
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
base.OnModelCreating(modelBuilder);
ErrorLogTable.Create(modelBuilder, _columnTypes);
modelBuilder
.HasDbFunction(typeof(DataContext)
.GetMethod(nameof(StringCompareOrdinal), new[] { typeof(string), typeof(string) })!)
.HasName("StringCompareOrdinal");
}
public static int StringCompareOrdinal(string a, string b) => throw new NotSupportedException();
}
}

=== FILE: F:\Marketing\Persistence\Context\Implementation\ErrorLogTable.cs ===

﻿using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants;
namespace Persistence.Context.Implementation
{
public static class ErrorLogTable
{
public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
{
modelBuilder.Entity<ErrorLog>().ToTable(Database.Tables.ErrorLogs);
modelBuilder.Entity<ErrorLog>(entity =>
{
entity.Property(i => i.Id)
.HasColumnType(columnTypes.TypeVar)
.IsRequired();
entity.HasKey(i => i.Id);
entity.Property(i => i.Timestamp)
.HasColumnType(columnTypes.TypeTime)
.IsRequired();
entity.Property(i => i.Level)
.HasColumnType(columnTypes.TypeVar150)
.HasMaxLength(150)
.IsRequired();
entity.Property(i => i.Message)
.HasColumnType(columnTypes.TypeVar150)
.HasMaxLength(150)
.IsRequired();
entity.Property(i => i.ExceptionType)
.HasColumnType(columnTypes.TypeVar150)
.HasMaxLength(150)
.IsRequired();
entity.Property(i => i.StackTrace)
.HasColumnType(columnTypes.TypeVar150)
.HasMaxLength(150)
.IsRequired();
entity.Property(i => i.Context)
.HasColumnType(columnTypes.TypeVar150)
.HasMaxLength(150)
.IsRequired(); ;
});
}
}
}

=== FILE: F:\Marketing\Persistence\Context\Implementation\UnitOfWork.cs ===

﻿using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Interface;
namespace Persistence.Context.Implementation
{
public class UnitOfWork(DataContext context) : IUnitOfWork
{
private readonly DataContext _context = context;
public DataContext Context => _context;
public async Task<int> CommitAsync()
=> await Context.SaveChangesAsync();
public async Task<IDbContextTransaction> BeginTransactionAsync()
=> await Context.Database.BeginTransactionAsync();
public async Task CommitTransactionAsync(IDbContextTransaction tx)
{
await Context.SaveChangesAsync();
await tx.CommitAsync();
}
public async Task RollbackAsync(IDbContextTransaction tx)
=> await tx.RollbackAsync();
public void Dispose()
=> Context.Dispose();
}
}

=== FILE: F:\Marketing\Persistence\Context\Interceptors\SqliteFunctionInterceptor.cs ===

﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
namespace Persistence.Context.Interceptors
{
public class SqliteFunctionInterceptor : DbConnectionInterceptor
{
public override void ConnectionOpened(
DbConnection connection,
ConnectionEndEventData eventData)
{
if (connection is SqliteConnection sqlite)
RegisterFunction(sqlite);
base.ConnectionOpened(connection, eventData);
}
public override async Task ConnectionOpenedAsync(
DbConnection connection,
ConnectionEndEventData eventData,
CancellationToken cancellationToken = default)
{
if (connection is SqliteConnection sqlite)
RegisterFunction(sqlite);
await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
}
private static void RegisterFunction(SqliteConnection sqlite)
{
sqlite.CreateFunction<string, string, int>(
"StringCompareOrdinal",
(a, b) => a == b ? 0
: string.Compare(a, b, StringComparison.Ordinal) > 0 ? 1
: -1
);
}
}
}

=== FILE: F:\Marketing\Persistence\Context\Interface\IDataContext.cs ===

﻿namespace Persistence.Context.Interface
{
public interface IDataContext
{
bool Initialize();
}
}

=== FILE: F:\Marketing\Persistence\Context\Interface\IUnitOfWork.cs ===

﻿using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Implementation;
namespace Persistence.Context.Interface
{
public interface IUnitOfWork
{
Task<int> CommitAsync();
Task<IDbContextTransaction> BeginTransactionAsync();
Task CommitTransactionAsync(IDbContextTransaction tx);
Task RollbackAsync(IDbContextTransaction tx);
DataContext Context { get; }
}
}

=== FILE: F:\Marketing\Persistence\CreateStruture\Constants\Database.cs ===

﻿namespace Persistence.CreateStructure.Constants
{
public static class Database
{
public static class Tables
{
public const string Users = "Users";
public const string Invoices = "Invoices";
public const string Products = "Products";
public const string ErrorLogs = "ErrorLogs";
public const string Profiles = "Profiles";
public const string Experiences = "Experiences";
public const string ExperienceRoles = "ExperienceRoles";
public const string Educations = "Educations";
public const string Communications = "Communications";
}
public static class Index
{
public const string IndexEmail = "UC_Users_Email";
public const string IndexProfileFullName = "IX_Profiles_FullName";
public const string IndexProfileUrl = "UC_Profiles_Url";
public const string IndexExperienceByProfile = "IX_Experiences_ProfileId";
public const string IndexRoleByExperience = "IX_ExperienceRoles_ExperienceId";
public const string IndexEducationByProfile = "IX_Educations_ProfileId";
public const string IndexCommunicationByProfile = "IX_Communications_ProfileId";
}
}
}

=== FILE: F:\Marketing\Persistence\CreateStruture\Constants\ColumnType\IColumnTypes.cs ===

﻿namespace Persistence.CreateStructure.Constants.ColumnType
{
public interface IColumnTypes
{
string TypeBool { get; }
string TypeTime { get; }
string TypeDateTime { get; }
string TypeDateTimeOffset { get; }
string TypeVar { get; }
string TypeVar50 { get; }
string TypeVar150 { get; }
string TypeVar64 { get; }
string TypeBlob { get; }
string Integer { get; }
string Long { get; }
string Strategy { get; }
object? SqlStrategy { get; }
string Name { get; }
object? Value { get; }
}
}

=== FILE: F:\Marketing\Persistence\CreateStruture\Constants\ColumnType\Database\SQLite.cs ===

﻿namespace Persistence.CreateStructure.Constants.ColumnType.Database
{
public class SQLite : IColumnTypes
{
public string Integer => "INTEGER";
public string Long => "INTEGER";
public string TypeBool => "INTEGER";
public string TypeTime => "TEXT";
public string TypeDateTime => "TEXT";
public string TypeDateTimeOffset => "TEXT";
public string TypeVar50 => "TEXT";
public string TypeVar => "TEXT";
public string TypeVar150 => "TEXT";
public string TypeVar64 => "TEXT";
public string TypeBlob => "BLOB";
public string Strategy => "Sqlite:Autoincrement";
public object? SqlStrategy => true;
public string Name => string.Empty;
public object? Value => null;
}
}

=== FILE: F:\Marketing\Persistence\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Persistence\obj\Debug\net8.0\Persistence.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Persistence")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+4f69044b1c8e17b1586ebe3470a52fa586c2ee1f")]
[assembly: System.Reflection.AssemblyProductAttribute("Persistence")]
[assembly: System.Reflection.AssemblyTitleAttribute("Persistence")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Persistence\obj\Debug\net8.0\Persistence.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Persistence\Repositories\EntityChecker.cs ===

﻿using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
namespace Persistence.Repositories
{
public abstract class EntityChecker<T> : Read<T> where T : class, IEntity
{
private readonly Func<string, bool> _idValidator;
protected EntityChecker(IUnitOfWork unitOfWork, Func<string, bool>? idValidator = null)
: base(unitOfWork)
{
_idValidator = idValidator ?? (id => Guid.TryParse(id, out _));
}
protected async Task<T?> HasEntity(string id)
{
var results = await ReadFilter(e => e.Id == id);
return results?.FirstOrDefault();
}
protected async Task<T?> HasId(string id)
{
if (!TryValidateId(id, out _))
{
return null;
}
return await HasEntity(id);
}
protected bool TryValidateId(string? id, out string error)
{
if (string.IsNullOrWhiteSpace(id))
{
error = "Id cannot be null, empty, or whitespace.";
return false;
}
if (!_idValidator(id))
{
error = "Id format is invalid.";
return false;
}
error = string.Empty;
return true;
}
}
}

=== FILE: F:\Marketing\Persistence\Repositories\Read.cs ===

﻿using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using System.Linq.Expressions;
namespace Persistence.Repositories
{
public abstract class Read<T>(IUnitOfWork unitOfWork) : Repository<T>(unitOfWork) where T : class, IEntity
{
protected Task<IQueryable<T>> ReadFilter(Expression<Func<T, bool>> predicate)
{
return Task.FromResult(_dbSet.Where(predicate));
}
}
}

=== FILE: F:\Marketing\Persistence\Repositories\Repository.cs ===

﻿using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
namespace Persistence.Repositories
{
public abstract class Repository<T>(IUnitOfWork unitOfWork) where T : class
{
protected readonly DbSet<T> _dbSet = unitOfWork.Context.Set<T>();
}
}

=== FILE: F:\Marketing\Persistence\Repositories\RepositoryCreate.cs ===

﻿using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
namespace Persistence.Repositories
{
public abstract class RepositoryCreate<T>(IUnitOfWork unitOfWork)
: Read<T>(unitOfWork) where T : class, IEntity
{
protected async Task Create(T entity)
{
await _dbSet.AddAsync(entity);
}
protected async Task CreateRange(List<T> entities)
{
await _dbSet.AddRangeAsync(entities);
}
}
}

=== FILE: F:\Marketing\Persistence\Repositories\RepositoryDelete.cs ===

﻿using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
namespace Persistence.Repositories
{
public abstract class RepositoryDelete<T>(IUnitOfWork unitOfWork)
: EntityChecker<T>(unitOfWork) where T : class, IEntity
{
protected void Delete(T entity)
{
_dbSet.Remove(entity);
}
}
}

=== FILE: F:\Marketing\Persistence\Repositories\RepositoryUpdate.cs ===

﻿using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
namespace Persistence.Repositories
{
public abstract class RepositoryUpdate<T>(IUnitOfWork unitOfWork)
: EntityChecker<T>(unitOfWork) where T : class, IEntity
{
protected void Update(T entity)
{
unitOfWork.Context.Entry(entity).State = EntityState.Modified;
}
}
}

=== FILE: F:\Marketing\Services\CaptureSnapshot.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;
namespace Services
{
public class CaptureSnapshot : ICaptureSnapshot
{
private readonly IWebDriver _driver;
private readonly ILogger<CaptureSnapshot> _logger;
public CaptureSnapshot(IWebDriver driver, ILogger<CaptureSnapshot> logger)
{
_driver = driver;
_logger = logger;
}
public async Task<string> CaptureArtifactsAsync(string executionFolder, string stage)
{
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
if (string.IsNullOrWhiteSpace(stage))
stage = "UnknownStage";
_logger.LogInformation("Capturing artifacts for stage: {Stage} at {Timestamp}", stage, timestamp);
var htmlPath = Path.Combine(executionFolder, $"{timestamp}.html");
var screenshotPath = Path.Combine(executionFolder, $"{timestamp}.png");
Directory.CreateDirectory(executionFolder);
await File.WriteAllTextAsync(htmlPath, _driver.PageSource);
var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
screenshot.SaveAsFile(screenshotPath);
_logger.LogInformation("Artifacts captured: {HtmlPath}, {ScreenshotPath}", htmlPath, screenshotPath);
return timestamp;
}
}
}

=== FILE: F:\Marketing\Services\ChromeDriverFactory.cs ===

﻿using System.Collections.Concurrent;
using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Services.Interfaces;
namespace Services
{
public sealed class ChromeDriverFactory : IWebDriverFactory, IDisposable
{
private readonly ILogger<ChromeDriverFactory> _logger;
private readonly AppConfig _appConfig;
private readonly ChromeDriverService _driverService;
private readonly ConcurrentBag<IWebDriver> _createdDrivers = new();
private bool _disposed;
public ChromeDriverFactory(ILogger<ChromeDriverFactory> logger, AppConfig appConfig)
{
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
_driverService = ChromeDriverService.CreateDefaultService();
_driverService.HideCommandPromptWindow = true;
}
public IWebDriver Create(bool hide = false)
{
ThrowIfDisposed();
var downloadFolder = EnsureDownloadFolder();
var options = hide
? GetHeadlessOptions(downloadFolder)
: GetDefaultOptions(downloadFolder);
return CreateDriver(options);
}
public IWebDriver Create(Action<ChromeOptions> configureOptions)
{
ThrowIfDisposed();
if (configureOptions is null) throw new ArgumentNullException(nameof(configureOptions));
var downloadFolder = EnsureDownloadFolder();
var options = GetDefaultOptions(downloadFolder);
configureOptions(options);
return CreateDriver(options);
}
public ChromeOptions GetDefaultOptions(string downloadFolder)
{
if (string.IsNullOrWhiteSpace(downloadFolder))
throw new ArgumentNullException(nameof(downloadFolder));
var options = new ChromeOptions();
options.AddArguments("--start-maximized");
options.AddExcludedArgument("enable-automation");
options.AddAdditionalOption("useAutomationExtension", false);
ConfigureDownloads(downloadFolder, options);
return options;
}
private static ChromeOptions GetHeadlessOptions(string downloadFolder)
{
if (string.IsNullOrWhiteSpace(downloadFolder))
throw new ArgumentNullException(nameof(downloadFolder));
var options = new ChromeOptions();
options.AddArguments("--headless=new");
options.AddArguments("--disable-gpu");
options.AddArguments("--window-size=1920,1080");
options.AddArguments("--start-maximized");
options.AddExcludedArgument("enable-automation");
options.AddAdditionalOption("useAutomationExtension", false);
ConfigureDownloads(downloadFolder, options);
return options;
}
private IWebDriver CreateDriver(ChromeOptions options)
{
try
{
_logger.LogInformation("Creating new ChromeDriver instance");
var driver = new ChromeDriver(_driverService, options);
SetTimeouts(driver);
_createdDrivers.Add(driver);
return driver;
}
catch (Exception ex)
{
_logger.LogError(ex, "Failed to create ChromeDriver");
throw new WebDriverException("Failed to initialize ChromeDriver", ex);
}
}
private static void SetTimeouts(IWebDriver driver)
{
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
}
private string EnsureDownloadFolder()
{
var downloadFolder = _appConfig?.Paths?.DownloadFolder;
if (string.IsNullOrWhiteSpace(downloadFolder))
throw new InvalidOperationException("AppConfig.Paths.DownloadFolder is missing or empty.");
Directory.CreateDirectory(downloadFolder);
return downloadFolder;
}
private static void ConfigureDownloads(string downloadFolder, ChromeOptions options)
{
options.AddUserProfilePreference("download.default_directory", downloadFolder);
options.AddUserProfilePreference("download.prompt_for_download", false);
options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
options.AddUserProfilePreference("safebrowsing.enabled", true);
options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
}
public void Dispose()
{
if (_disposed) return;
_disposed = true;
while (_createdDrivers.TryTake(out var driver))
{
try { driver.Quit(); } catch {  }
try { driver.Dispose(); } catch {  }
}
try { _driverService.Dispose(); } catch {  }
}
private void ThrowIfDisposed()
{
if (_disposed)
{
throw new ObjectDisposedException(nameof(ChromeDriverFactory));
}
}
}
}

=== FILE: F:\Marketing\Services\DirectoryCheck.cs ===

﻿using Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
namespace Services
{
public class DirectoryCheck : IDirectoryCheck
{
private readonly ILogger<DirectoryCheck> _logger;
private readonly ExecutionTracker _executionOptions;
public DirectoryCheck(ILogger<DirectoryCheck> logger, ExecutionTracker executionOptions)
{
_logger = logger;
_executionOptions = executionOptions;
}
public void EnsureDirectoryExists(string path)
{
if (!Directory.Exists(path))
{
Directory.CreateDirectory(path);
_logger.LogInformation($"📁 Created execution folder at: {_executionOptions.ExecutionRunning}");
}
}
}
}

=== FILE: F:\Marketing\Services\LoginService.cs ===

﻿using System.Text.RegularExpressions;
using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;
namespace Services
{
public class LoginService(
AppConfig config,
IWebDriver driver,
ILogger<LoginService> logger,
ICaptureSnapshot capture,
ExecutionTracker executionOptions
) : ILoginService
{
private AppConfig Config { get; } = config;
private IWebDriver Driver { get; } = driver;
private ILogger<LoginService> Logger { get; } = logger;
private ICaptureSnapshot Capture { get; } = capture;
private ExecutionTracker ExecutionOptions { get; } = executionOptions;
private const string FolderName = "Login";
private string FolderPath => Path.Combine(ExecutionOptions.ExecutionRunning, FolderName);
public async Task LoginAsync(CancellationToken cancellationToken = default)
{
Logger.LogInformation(
"🔐 ID:{TimeStamp} Starting login process...",
ExecutionOptions.TimeStamp
);
var url = Config.WhatsApp.Url;
Logger.LogInformation(
"🌐 ID:{TimeStamp} Navigating to login URL: {Url}",
ExecutionOptions.TimeStamp,
url
);
Driver.Navigate().GoToUrl(url);
await Capture.CaptureArtifactsAsync(FolderPath, "Login_Page_Loaded");
Logger.LogInformation(
"📲 ID:{TimeStamp} Please open WhatsApp on your phone and complete login (scan QR / approve). Waiting for login...",
ExecutionOptions.TimeStamp
);
await WaitForWhatsAppLoginAsync(
pollInterval: Config.WhatsApp.LoginPollInterval,
timeout: Config.WhatsApp.LoginTimeout,
cancellationToken: cancellationToken
);
Logger.LogInformation(
"✅ ID:{TimeStamp} WhatsApp Web is logged in. Continuing...",
ExecutionOptions.TimeStamp
);
await Task.CompletedTask;
}
private async Task WaitForWhatsAppLoginAsync(
TimeSpan pollInterval,
TimeSpan timeout,
CancellationToken cancellationToken)
{
var start = DateTimeOffset.UtcNow;
while (true)
{
cancellationToken.ThrowIfCancellationRequested();
var state = GetWhatsAppLoginState();
if (state == WhatsAppLoginState.LoggedIn)
{
await Capture.CaptureArtifactsAsync(FolderPath, "Login_Success_LoggedIn");
return;
}
if (state == WhatsAppLoginState.NeedsQrScan)
{
Logger.LogInformation(
"⏳ ID:{TimeStamp} Still not logged in. Please scan the QR / approve on phone...",
ExecutionOptions.TimeStamp
);
}
else
{
Logger.LogInformation(
"⏳ ID:{TimeStamp} Waiting for WhatsApp Web to be ready... state={State}",
ExecutionOptions.TimeStamp,
state
);
}
if (DateTimeOffset.UtcNow - start > timeout)
{
await Capture.CaptureArtifactsAsync(FolderPath, "Login_Timeout");
throw new TimeoutException($"WhatsApp login not completed within {timeout.TotalSeconds:N0} seconds.");
}
await Task.Delay(pollInterval, cancellationToken);
}
}
private enum WhatsAppLoginState
{
LoggedIn,
NeedsQrScan,
Loading,
Unknown
}
private WhatsAppLoginState GetWhatsAppLoginState()
{
try
{
if (Driver.FindElements(By.CssSelector("div[role='textbox'][contenteditable='true']")).Count > 0)
return WhatsAppLoginState.LoggedIn;
if (Driver.FindElements(By.CssSelector("div[role='grid'], div[role='application']")).Count > 0)
return WhatsAppLoginState.LoggedIn;
if (Driver.FindElements(By.CssSelector("canvas")).Count > 0)
return WhatsAppLoginState.NeedsQrScan;
var body = Driver.FindElements(By.TagName("body")).FirstOrDefault()?.Text ?? string.Empty;
if (body.Contains("Log in", StringComparison.OrdinalIgnoreCase) ||
body.Contains("Use WhatsApp", StringComparison.OrdinalIgnoreCase))
return WhatsAppLoginState.NeedsQrScan;
var ready = ((IJavaScriptExecutor)Driver).ExecuteScript("return document.readyState")?.ToString();
if (!string.Equals(ready, "complete", StringComparison.OrdinalIgnoreCase))
return WhatsAppLoginState.Loading;
return WhatsAppLoginState.Unknown;
}
catch
{
return WhatsAppLoginState.Unknown;
}
}
}
}

=== FILE: F:\Marketing\Services\SecurityCheck.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;
using Configuration;
namespace Services
{
public class SecurityCheck : ISecurityCheck
{
private readonly IWebDriver _driver;
private readonly ICaptureSnapshot _capture;
private readonly ExecutionTracker _executionOptions;
private readonly ILogger<SecurityCheck> _logger;
private const string FolderName = "SecurityCheck";
private string FolderPath => Path.Combine(_executionOptions.ExecutionRunning, FolderName);
private readonly IDirectoryCheck _directoryCheck;
public SecurityCheck(IWebDriver driver,
ILogger<SecurityCheck> logger,
ICaptureSnapshot capture,
ExecutionTracker executionOptions,
IDirectoryCheck directoryCheck)
{
_logger = logger;
_capture = capture;
_executionOptions = executionOptions;
_directoryCheck = directoryCheck;
_driver = driver;
_directoryCheck.EnsureDirectoryExists(FolderPath);
}
public bool IsSecurityCheck()
{
try
{
var title = _driver.Title.Contains("Security Verification");
if (title)
{
_logger.LogWarning("⚠️ Title Security Verification detected on the page.");
return true;
}
var captcha = _driver.FindElements(By.Id("captcha-internal")).Any();
if (captcha)
{
_logger.LogWarning("⚠️ CAPTCHA image detected on the page.");
return true;
}
var text = _driver.FindElements(By.XPath("
if (text)
{
_logger.LogWarning("⚠️ Text 'Let’s do a quick security check' detected on the page.");
return true;
}
var captchaImages = _driver.FindElements(By.XPath("
if (captchaImages)
{
_logger.LogWarning("⚠️ CAPTCHA image detected on the page.");
return true;
}
var bodyText = _driver.FindElement(By.TagName("body")).Text;
var indicators = new[] { "are you a human", "please verify", "unusual activity", "security check", "confirm your identity" };
if (indicators.Any(indicator => bodyText.IndexOf(indicator, StringComparison.OrdinalIgnoreCase) >= 0))
{
_logger.LogWarning("⚠️ Security check text detected on the page.");
return true;
}
var loginForm = _driver.FindElements(By.XPath("
if (loginForm.Any())
{
_logger.LogWarning("⚠️ Unexpected LinkedIn login form detected. Session might have expired.");
return true;
}
return false;
}
catch (Exception ex)
{
_logger.LogError(ex, "⚠️ Error while checking for security verification.");
return false;
}
}
public async Task TryStartPuzzle()
{
try
{
_logger.LogInformation("🧩 Attempting to click on 'Start Puzzle' button...");
Console.WriteLine("🛑 Pausado. Por favor, resuelve el captcha y presiona ENTER para continuar...");
Console.ReadLine();
var timestampEnd = await _capture.CaptureArtifactsAsync(FolderPath, "Start_Puzzle_Clicked");
_logger.LogInformation($"📸 Captured screenshot after clicking 'Start Puzzle' at {timestampEnd}.");
}
catch (Exception ex)
{
_logger.LogError(ex, $"❌ ID:{_executionOptions.TimeStamp} Failed to simulate click on 'Start Puzzle' button.");
}
}
public async Task HandleSecurityPage()
{
var timestamp = await _capture.CaptureArtifactsAsync(FolderPath, "SecurityPageDetected");
_logger.LogError($" ID:{_executionOptions.TimeStamp} Unexpected page layout detected.");
Console.WriteLine("\n╔════════════════════════════════════════════╗");
Console.WriteLine("║           SECURITY PAGE DETECTED          ║");
Console.WriteLine("╠════════════════════════════════════════════╣");
Console.WriteLine($"║ Current URL: {_driver.Url,-30} ║");
Console.WriteLine("║                                            ║");
Console.WriteLine($"║ HTML saved to: {timestamp}.html ║");
Console.WriteLine($"║ Screenshot saved to: {timestamp}.png ║");
Console.WriteLine("╚════════════════════════════════════════════╝\n");
}
public async Task HandleUnexpectedPage()
{
var timestamp = await _capture.CaptureArtifactsAsync(FolderPath, "UnexpectedPageDetected");
_logger.LogError($" ID:{_executionOptions.TimeStamp} Unexpected page layout detected.");
Console.WriteLine("\n╔════════════════════════════════════════════╗");
Console.WriteLine("║           UNEXPECTED PAGE DETECTED          ║");
Console.WriteLine("╠════════════════════════════════════════════╣");
Console.WriteLine($"║ Current URL: {_driver.Url,-30} ║");
Console.WriteLine("║                                            ║");
Console.WriteLine($"║ HTML saved to: {timestamp}.html ║");
Console.WriteLine($"║ Screenshot saved to: {timestamp}.png ║");
Console.WriteLine("╚════════════════════════════════════════════╝\n");
}
}
}

=== FILE: F:\Marketing\Services\Util.cs ===

﻿using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;
namespace Services
{
public class Util: IUtil
{
private readonly IWebDriver _driver;
private readonly AppConfig _config;
private readonly ILogger<Util> _logger;
private readonly ExecutionTracker _executionOptions;
private const string FolderName = "Page";
private readonly ISecurityCheck _securityCheck;
private string FolderPath => Path.Combine(_executionOptions.ExecutionRunning, FolderName);
private readonly ICaptureSnapshot _capture;
private readonly IDirectoryCheck _directoryCheck;
public Util(IWebDriver driver,
AppConfig config,
ILogger<Util> logger,
ExecutionTracker executionOptions,
ICaptureSnapshot capture,
ISecurityCheck securityCheck,
IDirectoryCheck directoryCheck)
{
_driver = driver;
_config = config;
_logger = logger;
_executionOptions = executionOptions;
_capture = capture;
_securityCheck = securityCheck;
_directoryCheck = directoryCheck;
_directoryCheck.EnsureDirectoryExists(FolderPath);
}
public async Task<bool> WaitForPageLoadAsync(int timeoutInSeconds = 30)
{
try
{
string xpath = "
var nextButton = _driver.FindElements(By.XPath(xpath))
.FirstOrDefault(b => b.Enabled);
if (nextButton == null)
{
_logger.LogInformation($"⏹️ ID:{_executionOptions.TimeStamp} No 'Next' pagination button found. Pagination completed.");
return false;
}
_logger.LogDebug($"⏭️ ID:{_executionOptions.TimeStamp} Clicking 'Next' button to go to next results page...");
nextButton.Click();
await Task.Delay(3000);
if (_securityCheck.IsSecurityCheck())
{
await _securityCheck.HandleSecurityPage();
throw new InvalidOperationException(
$"❌ ID:{_executionOptions.TimeStamp} LinkedIn requires manual security verification. Please complete it in the browser.");
}
var container = _driver.FindElements(By.XPath("
if (container == null)
{
await _securityCheck.HandleUnexpectedPage();
throw new InvalidOperationException(
$"❌ ID:{_executionOptions.TimeStamp} Failed to load next page. Current URL: {_driver.Url}");
}
_logger.LogInformation($"✅ ID:{_executionOptions.TimeStamp} Successfully navigated to the next page.");
return true;
}
catch (Exception ex)
{
_logger.LogWarning(ex, $"⚠️ ID:{_executionOptions.TimeStamp} Exception during next-page navigation.");
return false;
}
}
public async Task<bool> NavigateToNextPageAsync()
{
try
{
string xpath = "
var nextButton = _driver.FindElements(By.XPath(xpath))
.FirstOrDefault(b => b.Enabled);
if (nextButton == null)
{
_logger.LogInformation($"⏹️ ID:{_executionOptions.TimeStamp} No 'Next' pagination button found. Pagination completed.");
return false;
}
_logger.LogDebug($"⏭️ ID:{_executionOptions.TimeStamp} Clicking 'Next' button to go to next results page...");
nextButton.Click();
await Task.Delay(3000);
if (_securityCheck.IsSecurityCheck())
{
await _securityCheck.HandleSecurityPage();
throw new InvalidOperationException(
$"❌ ID:{_executionOptions.TimeStamp} LinkedIn requires manual security verification. Please complete it in the browser.");
}
var container = _driver.FindElements(By.XPath("
if (container == null)
{
await _securityCheck.HandleUnexpectedPage();
throw new InvalidOperationException(
$"❌ ID:{_executionOptions.TimeStamp} Failed to load next page. Current URL: {_driver.Url}");
}
_logger.LogInformation($"✅ ID:{_executionOptions.TimeStamp} Successfully navigated to the next page.");
return true;
}
catch (Exception ex)
{
_logger.LogWarning(ex, $"⚠️ ID:{_executionOptions.TimeStamp} Exception during next-page navigation.");
return false;
}
}
public void ScrollMove()
{
var jsExecutor = (IJavaScriptExecutor)_driver;
const int stepSize = 500;
const int delayMs = 800;
long totalHeight = (long)jsExecutor.ExecuteScript("return document.body.scrollHeight");
long currentPosition = 0;
_logger.LogInformation("⬇️ ID:{TimeStamp} Starting step-by-step scroll on LinkedIn...", _executionOptions.TimeStamp);
while (currentPosition < totalHeight)
{
jsExecutor.ExecuteScript($"window.scrollTo(0, {currentPosition});");
Thread.Sleep(delayMs);
currentPosition += stepSize;
totalHeight = (long)jsExecutor.ExecuteScript("return document.body.scrollHeight");
_logger.LogDebug("🔁 ID:{TimeStamp} Scrolled to: {CurrentPosition}/{TotalHeight}", _executionOptions.TimeStamp, currentPosition, totalHeight);
}
_logger.LogInformation("✅ ID:{TimeStamp} Step-by-step scroll completed.", _executionOptions.TimeStamp);
}
public void ScrollToTop()
{
var jsExecutor = (IJavaScriptExecutor)_driver;
_logger.LogInformation("⬆️ ID:{TimeStamp} Scrolling to the top of the page...", _executionOptions.TimeStamp);
jsExecutor.ExecuteScript("window.scrollTo(0, 0);");
Thread.Sleep(1000);
_logger.LogInformation("✅ ID:{TimeStamp} Scroll to top completed.", _executionOptions.TimeStamp);
}
public void ScrollToExperienceSection()
{
try
{
_logger.LogInformation("🔍 ID:{TimeStamp} Scrolling to the 'Experience' section...", _executionOptions.TimeStamp);
var jsExecutor = (IJavaScriptExecutor)_driver;
var experienceSection = _driver.FindElement(By.CssSelector("section[data-section='experience']"));
jsExecutor.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth', block: 'start' });", experienceSection);
Thread.Sleep(1000);
_logger.LogInformation("✅ ID:{TimeStamp} Successfully scrolled to the 'Experience' section.", _executionOptions.TimeStamp);
}
catch (NoSuchElementException)
{
_logger.LogWarning("⚠️ ID:{TimeStamp} 'Experience' section not found using [data-section='experience'].", _executionOptions.TimeStamp);
}
catch (Exception ex)
{
_logger.LogError(ex, "❌ ID:{TimeStamp} Failed to scroll to the 'Experience' section.", _executionOptions.TimeStamp);
}
}
}
}

=== FILE: F:\Marketing\Services\WebDriverLifetimeService.cs ===

﻿using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
namespace Services
{
public sealed class WebDriverLifetimeService : IHostedService
{
private readonly IWebDriver _driver;
public WebDriverLifetimeService(IWebDriver driver)
{
_driver = driver;
}
public Task StartAsync(CancellationToken cancellationToken)
=> Task.CompletedTask;
public Task StopAsync(CancellationToken cancellationToken)
{
try
{
_driver.Quit();
_driver.Dispose();
}
catch
{
}
return Task.CompletedTask;
}
}
}

=== FILE: F:\Marketing\Services\WhatAppOpenChat.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;
namespace Services
{
public class WhatAppOpenChat(
IWebDriver driver,
ILogger<LoginService> logger
) : IWhatAppOpenChat
{
private const string WhatAppMessage = "WhatsApp Web is not logged in. Call LoginAsync() before opening a chat.";
private const string CssSelectorToFind = "div[role='textbox'][contenteditable='true']";
private const string CssSelectorToFindSearchInput = "div[role='textbox'][contenteditable='true'][aria-label='Search input textbox']";
private const string XpathToFindGridcell = "./ancestor::*[@role='gridcell' or @role='row' or @tabindex][1]";
private IWebDriver Driver { get; } = driver;
public ILogger<LoginService> Logger { get; } = logger;
public async Task OpenContactChatAsync(
string chatIdentifier,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default)
{
Logger.LogInformation("OpenContactChatAsync started. chatIdentifier='{ChatIdentifier}'", chatIdentifier);
ct.ThrowIfCancellationRequested();
if (string.IsNullOrWhiteSpace(chatIdentifier))
{
Logger.LogWarning("OpenContactChatAsync aborted: chatIdentifier is null/empty/whitespace.");
throw new ArgumentException("Chat identifier cannot be null, empty, or whitespace.", nameof(chatIdentifier));
}
Logger.LogInformation("Step 1/4: Checking WhatsApp Web login state...");
if (!IsWhatsAppLoggedIn())
{
Logger.LogError("OpenContactChatAsync failed: WhatsApp Web is not logged in.");
throw new InvalidOperationException(WhatAppMessage);
}
Logger.LogInformation("Step 1/4: Logged in confirmed.");
var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);
var effectivePoll = pollInterval ?? TimeSpan.FromMilliseconds(200);
if (effectiveTimeout <= TimeSpan.Zero)
{
Logger.LogWarning("Invalid timeout provided: {Timeout}.", effectiveTimeout);
throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
}
if (effectivePoll <= TimeSpan.Zero)
{
Logger.LogWarning("Invalid pollInterval provided: {PollInterval}.", effectivePoll);
throw new ArgumentOutOfRangeException(nameof(pollInterval), "Poll interval must be greater than zero.");
}
if (effectivePoll > effectiveTimeout)
{
var adjusted = TimeSpan.FromMilliseconds(Math.Max(50, effectiveTimeout.TotalMilliseconds / 10));
Logger.LogWarning(
"PollInterval {PollInterval} > Timeout {Timeout}. Adjusting pollInterval to {AdjustedPollInterval}.",
effectivePoll, effectiveTimeout, adjusted);
effectivePoll = adjusted;
}
Logger.LogInformation(
"Step 2/4: Using timeout={Timeout} pollInterval={PollInterval}.",
effectiveTimeout, effectivePoll);
ct.ThrowIfCancellationRequested();
Logger.LogInformation("Step 3/4: Typing chatIdentifier into WhatsApp search box...");
try
{
await TypeIntoSearchBoxAsync(chatIdentifier, effectiveTimeout, effectivePoll )
.ConfigureAwait(false);
Logger.LogInformation("Step 3/4: Search input completed.");
}
catch (OperationCanceledException)
{
Logger.LogWarning("OpenContactChatAsync canceled during Step 3/4 (TypeIntoSearchBoxAsync).");
throw;
}
catch (Exception ex)
{
Logger.LogError(ex, "OpenContactChatAsync failed during Step 3/4 (TypeIntoSearchBoxAsync).");
throw;
}
ct.ThrowIfCancellationRequested();
Logger.LogInformation("Step 4/4: Clicking chat by title '{ChatIdentifier}'...", chatIdentifier);
try
{
await ClickChatByTitleAsync(chatIdentifier, effectiveTimeout, effectivePoll )
.ConfigureAwait(false);
Logger.LogInformation("Step 4/4: Chat opened successfully. chatIdentifier='{ChatIdentifier}'", chatIdentifier);
}
catch (OperationCanceledException)
{
Logger.LogWarning("OpenContactChatAsync canceled during Step 4/4 (ClickChatByTitleAsync).");
throw;
}
catch (Exception ex)
{
Logger.LogError(ex, "OpenContactChatAsync failed during Step 4/4 (ClickChatByTitleAsync).");
throw;
}
}
private bool IsWhatsAppLoggedIn()
{
Logger.LogDebug("IsWhatsAppLoggedIn: Checking WhatsApp Web login state...");
try
{
var elements = Driver.FindElements(By.CssSelector(CssSelectorToFind));
var isLoggedIn = elements.Count > 0;
Logger.LogDebug(
"IsWhatsAppLoggedIn: Selector '{Selector}' returned {Count} elements. LoggedIn={IsLoggedIn}.",
CssSelectorToFind,
elements.Count,
isLoggedIn
);
return isLoggedIn;
}
catch (NoSuchElementException ex)
{
Logger.LogWarning(
ex,
"IsWhatsAppLoggedIn: Selector '{Selector}' not found. Assuming not logged in.",
CssSelectorToFind
);
return false;
}
catch (WebDriverException ex)
{
Logger.LogError(
ex,
"IsWhatsAppLoggedIn: WebDriver error while checking login state."
);
return false;
}
catch (Exception ex)
{
Logger.LogError(
ex,
"IsWhatsAppLoggedIn: Unexpected error while checking login state."
);
return false;
}
}
private async Task TypeIntoSearchBoxAsync(
string text,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default)
{
Logger.LogInformation(
"TypeIntoSearchBoxAsync started. textLength={TextLength}",
text?.Length ?? 0
);
if (string.IsNullOrWhiteSpace(text))
{
Logger.LogWarning("TypeIntoSearchBoxAsync aborted: text is null or whitespace.");
throw new ArgumentException("Text cannot be empty.", nameof(text));
}
timeout ??= TimeSpan.FromSeconds(10);
pollInterval ??= TimeSpan.FromMilliseconds(200);
Logger.LogInformation(
"Using timeout={Timeout} pollInterval={PollInterval}.",
timeout, pollInterval
);
var deadline = DateTimeOffset.UtcNow + timeout.Value;
var attempt = 0;
while (DateTimeOffset.UtcNow < deadline)
{
ct.ThrowIfCancellationRequested();
attempt++;
try
{
Logger.LogDebug(
"Attempt {Attempt}: Locating search input using selector '{Selector}'.",
attempt,
CssSelectorToFindSearchInput
);
var input = Driver
.FindElements(By.CssSelector(CssSelectorToFindSearchInput))
.FirstOrDefault();
if (input is { Displayed: true, Enabled: true })
{
Logger.LogInformation(
"Search input found and ready on attempt {Attempt}. Focusing and typing...",
attempt
);
input.Click();
Logger.LogDebug("Clearing existing search input content.");
input.SendKeys(Keys.Control + "a");
input.SendKeys(Keys.Backspace);
Logger.LogDebug("Typing search text and submitting.");
input.SendKeys(text);
input.SendKeys(Keys.Enter);
Logger.LogInformation("TypeIntoSearchBoxAsync completed successfully.");
return;
}
Logger.LogDebug(
"Attempt {Attempt}: Search input not ready (null, hidden, or disabled).",
attempt
);
}
catch (StaleElementReferenceException)
{
Logger.LogDebug(
"Attempt {Attempt}: StaleElementReferenceException encountered. Retrying...",
attempt
);
}
catch (InvalidElementStateException)
{
Logger.LogDebug(
"Attempt {Attempt}: InvalidElementStateException encountered. Retrying...",
attempt
);
}
catch (Exception ex)
{
Logger.LogError(
ex,
"TypeIntoSearchBoxAsync failed unexpectedly on attempt {Attempt}.",
attempt
);
throw;
}
await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
}
Logger.LogError(
"TypeIntoSearchBoxAsync timed out after {TimeoutSeconds} seconds.",
timeout.Value.TotalSeconds
);
throw new WebDriverTimeoutException(
$"Search input textbox not available within {timeout.Value.TotalSeconds} seconds."
);
}
private string EscapeXPathLiteral(string value)
{
if (value is null)
{
throw new ArgumentNullException(nameof(value));
}
Logger.LogDebug(
"EscapeXPathLiteral started. valueLength={ValueLength}",
value.Length
);
if (!value.Contains("'"))
{
Logger.LogDebug("EscapeXPathLiteral: Using single-quoted XPath literal.");
return $"'{value}'";
}
if (!value.Contains("\""))
{
Logger.LogDebug("EscapeXPathLiteral: Using double-quoted XPath literal.");
return $"\"{value}\"";
}
Logger.LogDebug("EscapeXPathLiteral: Using concat() XPath literal strategy.");
var parts = value.Split('\'');
var partsString = string.Join(", \"'\", ", parts.Select(p => $"'{p}'"));
var result = "concat(" + partsString + ")";
Logger.LogDebug(
"EscapeXPathLiteral completed. partCount={PartCount}",
parts.Length
);
return result;
}
private string GetXpathToFind(string needle)
{
return $"
}
private async Task ClickChatByTitleAsync(
string chatTitle,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default)
{
Logger.LogInformation(
"ClickChatByTitleAsync started. chatTitleLength={ChatTitleLength}",
chatTitle?.Length ?? 0
);
if (string.IsNullOrWhiteSpace(chatTitle))
{
Logger.LogWarning("ClickChatByTitleAsync aborted: chatTitle is null or whitespace.");
throw new ArgumentException("Chat title cannot be empty.", nameof(chatTitle));
}
timeout ??= TimeSpan.FromSeconds(10);
pollInterval ??= TimeSpan.FromMilliseconds(250);
Logger.LogInformation(
"Using timeout={Timeout} pollInterval={PollInterval}.",
timeout, pollInterval
);
var needle = chatTitle.Trim().ToLowerInvariant();
var end = DateTimeOffset.UtcNow + timeout.Value;
Logger.LogDebug(
"Normalized chat title for search. originalLength={OriginalLength} normalizedLength={NormalizedLength}",
chatTitle.Length,
needle.Length
);
var attempt = 0;
while (DateTimeOffset.UtcNow < end)
{
ct.ThrowIfCancellationRequested();
attempt++;
try
{
var xpathToFind = GetXpathToFind(needle);
Logger.LogDebug(
"Attempt {Attempt}: Searching chat span using XPath: {XPath}",
attempt,
xpathToFind
);
var span = Driver.FindElements(By.XPath(xpathToFind)).FirstOrDefault();
if (span is { Displayed: true })
{
Logger.LogInformation(
"Attempt {Attempt}: Matching chat span found and displayed. Resolving clickable target...",
attempt
);
var target = span.FindElements(By.XPath(XpathToFindGridcell)).FirstOrDefault() ?? span;
Logger.LogDebug(
"Attempt {Attempt}: Target resolved. targetDisplayed={Displayed} targetEnabled={Enabled}",
attempt,
target.Displayed,
target.Enabled
);
if (target.Displayed && target.Enabled)
{
Logger.LogInformation("Attempt {Attempt}: Clicking chat target...", attempt);
target.Click();
Logger.LogInformation("ClickChatByTitleAsync completed successfully.");
return;
}
Logger.LogDebug(
"Attempt {Attempt}: Target found but not clickable (displayed/enabled check failed).",
attempt
);
}
else
{
Logger.LogDebug(
"Attempt {Attempt}: No displayed span matched the chat title yet.",
attempt
);
}
}
catch (StaleElementReferenceException)
{
Logger.LogDebug(
"Attempt {Attempt}: StaleElementReferenceException encountered (DOM rerender). Retrying...",
attempt
);
}
catch (NoSuchElementException)
{
Logger.LogDebug(
"Attempt {Attempt}: NoSuchElementException encountered (ancestor/target missing). Retrying...",
attempt
);
}
catch (Exception ex)
{
Logger.LogError(
ex,
"ClickChatByTitleAsync failed unexpectedly on attempt {Attempt}.",
attempt
);
throw;
}
await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
}
Logger.LogError(
"ClickChatByTitleAsync timed out after {TimeoutSeconds} seconds. Chat not found/clickable. chatTitleLength={ChatTitleLength}",
timeout.Value.TotalSeconds,
chatTitle.Length
);
throw new WebDriverTimeoutException($"Chat not found or not clickable: '{chatTitle}'.");
}
}
}

=== FILE: F:\Marketing\Services\WhatsAppChatService.cs ===

﻿using System.Diagnostics;
using System.Text;
using System.Threading;
using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Interfaces;
using Services.Messages;
namespace Services
{
public sealed class WhatsAppChatService(
IWebDriver driver,
ILogger<WhatsAppChatService> logger,
AppConfig config
) : IWhatsAppChatService
{
private const string XpathToFindAttachButton = "
private const string FindPhotosAndVideosOption = "
private const string XpathFindCaption = "
private IWebDriver Driver { get; } = driver;
public ILogger<WhatsAppChatService> Logger { get; } = logger;
private AppConfig Config { get; } = config;
private void OpenFileDialogWithAutoIT(string imagePath)
{
Logger.LogInformation("OpenFileDialogWithAutoIT started.");
if (string.IsNullOrWhiteSpace(imagePath))
{
Logger.LogError("OpenFileDialogWithAutoIT aborted: imagePath is null or whitespace.");
throw new ArgumentException("imagePath is null/empty.", nameof(imagePath));
}
imagePath = Path.GetFullPath(imagePath);
Logger.LogDebug("Resolved imagePath to '{ImagePath}'", imagePath);
if (!File.Exists(imagePath))
{
Logger.LogError("Image file not found at path '{ImagePath}'", imagePath);
throw new FileNotFoundException("Image file not found.", imagePath);
}
var autoItExePath = @"C:\Program Files (x86)\AutoIt3\AutoIt3.exe";
Logger.LogDebug("Using AutoIt executable at '{AutoItExePath}'", autoItExePath);
if (!File.Exists(autoItExePath))
{
Logger.LogError("AutoIt executable not found at '{AutoItExePath}'", autoItExePath);
throw new FileNotFoundException("AutoIt3.exe not found.", autoItExePath);
}
var escapedPath = imagePath.Replace("\"", "\"\"");
Logger.LogDebug("Escaped image path for AutoIt.");
Logger.LogDebug("Building AutoIt script...");
var autoItScript = new StringBuilder()
.AppendLine("; AutoIt Script - whatsapp_upload.au3")
.AppendLine("Opt('WinTitleMatchMode', 2)")
.AppendLine("Local $timeout = 10")
.AppendLine("")
.AppendLine("If WinWaitActive('Open', '', $timeout) = 0 Then")
.AppendLine("    If WinWaitActive('Abrir', '', $timeout) = 0 Then")
.AppendLine("        Exit 1")
.AppendLine("    EndIf")
.AppendLine("EndIf")
.AppendLine("")
.AppendLine("Local $title = ''")
.AppendLine("If WinActive('Open') Then")
.AppendLine("    $title = 'Open'")
.AppendLine("ElseIf WinActive('Abrir') Then")
.AppendLine("    $title = 'Abrir'")
.AppendLine("Else")
.AppendLine("    Exit 2")
.AppendLine("EndIf")
.AppendLine("")
.AppendLine($"ControlSetText($title, '', '[CLASS:Edit; INSTANCE:1]', \"{escapedPath}\")")
.AppendLine("Sleep(300)")
.AppendLine("")
.AppendLine("If ControlClick($title, '', '[CLASS:Button; INSTANCE:1]') = 0 Then")
.AppendLine("    ControlSend($title, '', '[CLASS:Edit; INSTANCE:1]', '{ENTER}')")
.AppendLine("EndIf")
.AppendLine("")
.AppendLine("Exit 0")
.ToString();
var scriptPath = Path.Combine(Path.GetTempPath(), $"whatsapp_upload_{Guid.NewGuid():N}.au3");
Logger.LogDebug("Writing AutoIt script to '{ScriptPath}'", scriptPath);
File.WriteAllText(scriptPath, autoItScript, Encoding.UTF8);
var psi = new ProcessStartInfo
{
FileName = autoItExePath,
Arguments = $"\"{scriptPath}\"",
UseShellExecute = false,
CreateNoWindow = true,
RedirectStandardOutput = true,
RedirectStandardError = true
};
Logger.LogInformation("Starting AutoIt process...");
using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start AutoIt process.");
if (!proc.WaitForExit(15000))
{
Logger.LogWarning("AutoIt process timeout exceeded. Attempting to kill process.");
try { proc.Kill(true); } catch {  }
throw new TimeoutException("AutoIt file upload script timed out.");
}
Logger.LogInformation("AutoIt process exited with code {ExitCode}", proc.ExitCode);
try
{
File.Delete(scriptPath);
Logger.LogDebug("Temporary AutoIt script deleted.");
}
catch
{
Logger.LogWarning("Failed to delete temporary AutoIt script at '{ScriptPath}'", scriptPath);
}
if (proc.ExitCode != 0)
{
var err = proc.StandardError.ReadToEnd();
Logger.LogError(
"AutoIt script failed. ExitCode={ExitCode}. Error={Error}",
proc.ExitCode,
err
);
throw new InvalidOperationException($"AutoIt script failed. ExitCode={proc.ExitCode}. {err}");
}
Logger.LogInformation("OpenFileDialogWithAutoIT completed successfully.");
}
public Task SendMessageAsync(
ImageMessagePayload imageMessagePayload,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default)
{
Logger.LogInformation(
"SendMessageAsync started. messageLength={MessageLength}",
imageMessagePayload?.Caption?.Length ?? 0
);
ct.ThrowIfCancellationRequested();
if (imageMessagePayload is null)
{
Logger.LogError("SendMessageAsync aborted: imageMessagePayload is null.");
throw new ArgumentNullException(nameof(imageMessagePayload));
}
if (string.IsNullOrWhiteSpace(imageMessagePayload.Caption))
{
Logger.LogWarning("SendMessageAsync aborted: message is null or whitespace.");
throw new ArgumentException("Message cannot be empty.", nameof(imageMessagePayload.Caption));
}
Logger.LogInformation("Step 1/3: Locating WhatsApp compose box...");
Logger.LogInformation(
"Locating attach button using XPath '{XPath}'...",
XpathToFindAttachButton
);
TimeSpan loginTimeout = Config.WhatsApp.LoginTimeout;
TimeSpan loginPollInterval = Config.WhatsApp.LoginPollInterval;
var attachButton = FindAttachButton(loginTimeout, loginPollInterval);
if (attachButton is null)
{
Logger.LogError("Attach button not found. XPath='{XPath}'", XpathToFindAttachButton);
throw new NoSuchElementException("Attach button not found.");
}
Logger.LogDebug(
"Attach button found. displayed={Displayed}, enabled={Enabled}",
attachButton.Displayed,
attachButton.Enabled
);
Logger.LogInformation("Clicking attach button...");
attachButton.Click();
Logger.LogInformation(
"Locating 'Photos & videos' option using XPath '{XPath}'...",
FindPhotosAndVideosOption
);
var photoAndVideo = FindPhotosAndVideosOptionButton(loginTimeout, loginPollInterval);
if (photoAndVideo is null)
{
Logger.LogError("'Photos & videos' option not found. XPath='{XPath}'", FindPhotosAndVideosOption);
throw new NoSuchElementException("'Photos & videos' option not found.");
}
Logger.LogDebug(
"'Photos & videos' option found. displayed={Displayed}, enabled={Enabled}",
photoAndVideo.Displayed,
photoAndVideo.Enabled
);
Logger.LogInformation("Clicking 'Photos & videos' option...");
photoAndVideo.Click();
Logger.LogInformation("Opening file dialog via AutoIT... storedImagePath='{StoredImagePath}'", imageMessagePayload.StoredImagePath);
try
{
OpenFileDialogWithAutoIT(imageMessagePayload.StoredImagePath);
Logger.LogInformation("AutoIT completed file selection.");
}
catch (Exception ex)
{
Logger.LogError(ex, "AutoIT failed while selecting file.");
throw;
}
Logger.LogInformation(
"Locating caption element using XPath '{XPath}'...",
XpathFindCaption
);
var caption = FindCaption(loginTimeout, loginPollInterval);
if (caption is null)
{
Logger.LogError("Caption element not found. XPath='{XPath}'", XpathFindCaption);
throw new NoSuchElementException("Caption element not found.");
}
Logger.LogDebug(
"Caption element found. displayed={Displayed}, enabled={Enabled}",
caption.Displayed,
caption.Enabled
);
Logger.LogInformation("Typing caption...");
caption.SendKeys(imageMessagePayload.Caption);
Logger.LogInformation("Submitting caption (Enter)...");
caption.SendKeys(Keys.Enter);
ct.ThrowIfCancellationRequested();
Logger.LogInformation("Step 2/3: Focusing compose box...");
ct.ThrowIfCancellationRequested();
Logger.LogInformation(
"Step 3/3: Sending message ({Length} chars) and submitting...",
imageMessagePayload.Caption.Length
);
Logger.LogInformation("SendMessageAsync completed successfully.");
return Task.CompletedTask;
}
private IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval)
{
Logger.LogDebug(
"FindAttachButton started. timeout={Timeout} pollingInterval={PollingInterval} xpath='{XPath}'",
timeout,
pollingInterval,
XpathToFindAttachButton
);
var wait = new WebDriverWait(Driver, timeout)
{
PollingInterval = pollingInterval
};
wait.IgnoreExceptionTypes(
typeof(NoSuchElementException),
typeof(StaleElementReferenceException)
);
IWebElement attachButton;
try
{
attachButton = wait.Until(driver =>
{
var element = driver
.FindElements(By.XPath(XpathToFindAttachButton))
.FirstOrDefault();
if (element is null)
{
Logger.LogTrace("FindAttachButton: Attach button not present yet.");
return null;
}
if (!element.Displayed || !element.Enabled)
{
Logger.LogTrace(
"FindAttachButton: Element found but not ready. displayed={Displayed}, enabled={Enabled}",
element.Displayed,
element.Enabled
);
return null;
}
return element;
});
}
catch (WebDriverTimeoutException ex)
{
Logger.LogError(
ex,
"FindAttachButton timed out after {Timeout}. xpath='{XPath}'",
timeout,
XpathToFindAttachButton
);
throw;
}
Logger.LogDebug(
"FindAttachButton completed. found={Found} displayed={Displayed} enabled={Enabled}",
attachButton is not null,
attachButton?.Displayed,
attachButton?.Enabled
);
return attachButton;
}
private IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval)
{
Logger.LogDebug(
"FindPhotosAndVideosOptionButton started. timeout={Timeout} pollingInterval={PollingInterval} xpath='{XPath}'",
timeout,
pollingInterval,
FindPhotosAndVideosOption
);
var wait = new WebDriverWait(Driver, timeout)
{
PollingInterval = pollingInterval
};
wait.IgnoreExceptionTypes(
typeof(NoSuchElementException),
typeof(StaleElementReferenceException)
);
IWebElement photosAndVideosOption;
try
{
photosAndVideosOption = wait.Until(driver =>
{
var element = driver
.FindElements(By.XPath(FindPhotosAndVideosOption))
.FirstOrDefault();
if (element is null)
{
Logger.LogTrace("FindPhotosAndVideosOptionButton: Option not present yet.");
return null;
}
if (!element.Displayed || !element.Enabled)
{
Logger.LogTrace(
"FindPhotosAndVideosOptionButton: Element found but not ready. displayed={Displayed}, enabled={Enabled}",
element.Displayed,
element.Enabled
);
return null;
}
return element;
});
}
catch (WebDriverTimeoutException ex)
{
Logger.LogError(
ex,
"FindPhotosAndVideosOptionButton timed out after {Timeout}. xpath='{XPath}'",
timeout,
FindPhotosAndVideosOption
);
throw;
}
Logger.LogDebug(
"FindPhotosAndVideosOptionButton completed. found={Found} displayed={Displayed} enabled={Enabled}",
photosAndVideosOption is not null,
photosAndVideosOption?.Displayed,
photosAndVideosOption?.Enabled
);
return photosAndVideosOption;
}
private IWebElement FindCaption(TimeSpan timeout, TimeSpan pollingInterval)
{
Logger.LogDebug(
"FindCaption started. timeout={Timeout} pollingInterval={PollingInterval} xpath='{XPath}'",
timeout,
pollingInterval,
XpathFindCaption
);
var wait = new WebDriverWait(Driver, timeout)
{
PollingInterval = pollingInterval
};
wait.IgnoreExceptionTypes(
typeof(NoSuchElementException),
typeof(StaleElementReferenceException)
);
IWebElement caption;
try
{
caption = wait.Until(driver =>
{
var element = driver
.FindElements(By.XPath(XpathFindCaption))
.FirstOrDefault();
if (element is null)
{
Logger.LogTrace("FindCaption: Caption element not present yet.");
return null;
}
if (!element.Displayed || !element.Enabled)
{
Logger.LogTrace(
"FindCaption: Element found but not ready. displayed={Displayed}, enabled={Enabled}",
element.Displayed,
element.Enabled
);
return null;
}
return element;
});
}
catch (WebDriverTimeoutException ex)
{
Logger.LogError(
ex,
"FindCaption timed out after {Timeout}. xpath='{XPath}'",
timeout,
XpathFindCaption
);
throw;
}
Logger.LogDebug(
"FindCaption completed. found={Found} displayed={Displayed} enabled={Enabled}",
caption is not null,
caption?.Displayed,
caption?.Enabled
);
return caption;
}
}
}

=== FILE: F:\Marketing\Services\WhatsAppMessage.cs ===

﻿using Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Services.Messages;
namespace Services
{
public class WhatsAppMessage(
ILogger<LoginService> logger,
ILoginService loginService,
ExecutionTracker executionOption,
IWhatsAppChatService whatsAppChatService,
AppConfig config,
IWhatAppOpenChat whatAppOpenChat
) : IWhatsAppMessage
{
public ILogger<LoginService> Logger { get; } = logger;
public ILoginService Login { get; } = loginService;
public ExecutionTracker ExecutionOption { get; } = executionOption;
public IWhatsAppChatService WhatsAppChatService { get; } = whatsAppChatService;
private AppConfig Config { get; } = config;
private IWhatAppOpenChat WhatAppOpenChat { get; } = whatAppOpenChat;
public async Task LoginAsync()
{
Logger.LogInformation("WhatsAppMessage execution started");
Logger.LogInformation("Starting WhatsApp login process");
await Login.LoginAsync();
Logger.LogInformation("WhatsApp login completed successfully");
Logger.LogInformation("Finalizing execution folder");
var finalizeReport = ExecutionOption.FinalizeByCopyThenDelete(true);
LogFinalizeReport(finalizeReport);
}
public async Task SendMessageAsync()
{
int count = Config.WhatsApp.AllowedChatTargets.Count;
Logger.LogInformation("Beginning message dispatch. Total contacts: {ContactCount}", count);
List<string> allowedChatTargets = Config.WhatsApp.AllowedChatTargets;
foreach (var contact in allowedChatTargets)
{
await SendMessageToContact(contact);
}
Logger.LogInformation("WhatsAppMessage execution finished");
}
private async Task SendMessageToContact(string contact)
{
Logger.LogInformation("Opening chat for contact: {Contact}", contact);
await WhatAppOpenChat.OpenContactChatAsync(
contact,
Config.WhatsApp.LoginPollInterval,
Config.WhatsApp.LoginTimeout);
Logger.LogInformation("Chat opened successfully for contact: {Contact}", contact);
Logger.LogInformation("Sending message to contact: {Contact}", contact);
ImageMessagePayload imageMessagePayload = new()
{
StoredImagePath = "E:\\Company\\whatappmessage\\superO.png",
Caption = "This is an automated message with image."
};
await WhatsAppChatService.SendMessageAsync(
imageMessagePayload,
Config.WhatsApp.LoginPollInterval,
Config.WhatsApp.LoginTimeout);
Logger.LogInformation("Message sent successfully to contact: {Contact}", contact);
}
public void LogFinalizeReport(FinalizeReport report)
{
if (report is null)
{
Logger.LogWarning("Finalize report is null");
return;
}
Logger.LogInformation(
"Finalizing execution from {RunningPath} to {FinishedPath}",
report.RunningPath,
report.FinishedPath);
foreach (var failure in report.CopyFailures)
{
Logger.LogError(
failure.Exception,
"Copy failed during execution finalization: {Path}",
failure.Path);
}
foreach (var failure in report.DeleteFailures)
{
Logger.LogWarning(
failure.Exception,
"Delete failed for running execution folder: {Path}",
failure.Path);
}
if (report.IsClean)
{
Logger.LogInformation("Execution finalized successfully");
}
else
{
Logger.LogWarning(
"Execution finalized with issues. CopyFailures={CopyFailures}, DeleteFailures={DeleteFailures}",
report.CopyFailures.Count,
report.DeleteFailures.Count);
}
}
}
}

=== FILE: F:\Marketing\Services\Interfaces\ICaptureSnapshot.cs ===

﻿namespace Services.Interfaces
{
public interface ICaptureSnapshot
{
Task<string> CaptureArtifactsAsync(string executionFolder, string stage);
}
}

=== FILE: F:\Marketing\Services\Interfaces\IDirectoryCheck.cs ===

﻿namespace Services.Interfaces
{
public interface IDirectoryCheck
{
void EnsureDirectoryExists(string path);
}
}

=== FILE: F:\Marketing\Services\Interfaces\ILoginService.cs ===

﻿namespace Services.Interfaces
{
public interface ILoginService
{
Task LoginAsync(CancellationToken cancellationToken = default);
}
}

=== FILE: F:\Marketing\Services\Interfaces\IProcessor.cs ===

﻿namespace Services.Interfaces
{
public interface IProcessor
{
Task ProcessAllPagesAsync();
}
}

=== FILE: F:\Marketing\Services\Interfaces\IPromptGenerator.cs ===

﻿namespace Services.Interfaces
{
public interface IPromptGenerator
{
Task GeneratPrompt();
}
}

=== FILE: F:\Marketing\Services\Interfaces\ISearch.cs ===

﻿namespace Services.Interfaces
{
public interface ISearch
{
Task RunSearchAsync();
}
}

=== FILE: F:\Marketing\Services\Interfaces\ISecurityCheck.cs ===

﻿namespace Services.Interfaces
{
public interface ISecurityCheck
{
bool IsSecurityCheck();
Task TryStartPuzzle();
Task HandleSecurityPage();
Task HandleUnexpectedPage();
}
}

=== FILE: F:\Marketing\Services\Interfaces\IUtil.cs ===

﻿namespace Services.Interfaces
{
public interface IUtil
{
Task<bool> WaitForPageLoadAsync(int timeoutInSeconds = 30);
void ScrollMove();
void ScrollToTop();
void ScrollToExperienceSection();
Task<bool> NavigateToNextPageAsync();
}
}

=== FILE: F:\Marketing\Services\Interfaces\IWebDriverFactory.cs ===

﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace Services.Interfaces
{
public interface IWebDriverFactory
{
IWebDriver Create(bool hide = false);
IWebDriver Create(Action<ChromeOptions> configureOptions);
ChromeOptions GetDefaultOptions(string downloadFolder);
}
}

=== FILE: F:\Marketing\Services\Interfaces\IWhatAppOpenChat.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Services.Interfaces
{
public interface IWhatAppOpenChat
{
Task OpenContactChatAsync(
string chatIdentifier,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default
);
}
}

=== FILE: F:\Marketing\Services\Interfaces\IWhatsAppChatService.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Messages;
namespace Services.Interfaces
{
public interface IWhatsAppChatService
{
Task SendMessageAsync(
ImageMessagePayload imageMessagePayload,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default
);
}
}

=== FILE: F:\Marketing\Services\Interfaces\IWhatsAppMessage.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Services.Interfaces
{
public interface IWhatsAppMessage
{
Task LoginAsync();
Task SendMessageAsync();
}
}

=== FILE: F:\Marketing\Services\Messages\ImageMessagePayload.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Services.Messages
{
public sealed class ImageMessagePayload
{
public string StoredImagePath { get; init; } = default!;
public string Caption { get; init; } = string.Empty;
}
}

=== FILE: F:\Marketing\Services\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Services\obj\Debug\net8.0\Services.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Services")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+824c57be25fbd2217ad6312cf623c6356b3db7c2")]
[assembly: System.Reflection.AssemblyProductAttribute("Services")]
[assembly: System.Reflection.AssemblyTitleAttribute("Services")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Services\obj\Debug\net8.0\Services.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Services\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Services\obj\Release\net8.0\Services.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Services")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+1f7b61cc657b6b85a6c4b54e1aee2477c28bbe28")]
[assembly: System.Reflection.AssemblyProductAttribute("Services")]
[assembly: System.Reflection.AssemblyTitleAttribute("Services")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Services\obj\Release\net8.0\Services.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Services\Scheduling\ScheduledMessenger.cs ===

﻿using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Configuration;
using Services.Interfaces;
public sealed class ScheduledMessenger(
IServiceScopeFactory scopeFactory,
IOptionsMonitor<SchedulerOptions> options,
ILogger<ScheduledMessenger> logger) : BackgroundService
{
private IServiceScopeFactory ScopeFactory { get; } = scopeFactory;
private IOptionsMonitor<SchedulerOptions> Options { get; } = options;
private ILogger<ScheduledMessenger> Logger { get; } = logger;
private readonly SemaphoreSlim _gate = new(1, 1);
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
Logger.LogInformation("WhatsAppSchedulerHostedService started.");
using var scope = ScopeFactory.CreateScope();
var sender = scope.ServiceProvider.GetRequiredService<IWhatsAppMessage>();
await sender.LoginAsync();
while (!stoppingToken.IsCancellationRequested)
{
var opt = Options.CurrentValue;
if (!opt.Enabled)
{
Logger.LogInformation("Scheduler disabled. Sleeping 60s.");
await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
continue;
}
var tz = ResolveTimeZone(opt.TimeZoneId);
var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
var next = FindNextOccurrence(nowLocal, tz, opt);
if (next is null)
{
Logger.LogWarning("No schedule times found. Sleeping 10 minutes.");
await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
continue;
}
var delay = next.Value - nowLocal;
if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
Logger.LogInformation("Next scheduled run at {NextLocal} (in {Delay}).", next.Value, delay);
await Task.Delay(delay, stoppingToken);
if (!await _gate.WaitAsync(0, stoppingToken))
{
Logger.LogWarning("Previous run still in progress. Skipping this occurrence.");
continue;
}
try
{
Logger.LogInformation("Scheduled run started.");
await sender.SendMessageAsync();
Logger.LogInformation("Scheduled run finished.");
}
catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
{
}
catch (Exception ex)
{
Logger.LogError(ex, "Scheduled run failed.");
}
finally
{
_gate.Release();
}
}
Logger.LogInformation("WhatsAppSchedulerHostedService stopped.");
}
private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
{
return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
}
private static DateTimeOffset? FindNextOccurrence(
DateTimeOffset nowLocal,
TimeZoneInfo tz,
SchedulerOptions opt)
{
for (var dayOffset = 0; dayOffset <= 7; dayOffset++)
{
var day = nowLocal.Date.AddDays(dayOffset);
var dowName = day.DayOfWeek.ToString();
if (!opt.Weekly.TryGetValue(dowName, out var times) || times is null || times.Count == 0)
continue;
foreach (var t in times
.Select(ParseTime)
.Where(x => x is not null)
.Select(x => x!.Value)
.OrderBy(x => x))
{
var localUnspecified = new DateTime(
day.Year,
day.Month,
day.Day,
t.Hours,
t.Minutes,
0,
DateTimeKind.Unspecified
);
if(tz.IsInvalidTime(localUnspecified))
continue;
var offset = tz.GetUtcOffset(localUnspecified);
var candidateLocal = new DateTimeOffset(localUnspecified, offset);
if (candidateLocal > nowLocal)
return candidateLocal;
}
}
return null;
}
private static (int Hours, int Minutes)? ParseTime(string value)
{
if (TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
return ((int)ts.TotalHours, ts.Minutes);
return null;
}
}

=== FILE: F:\Marketing\WhatsAppSender\Program.cs ===

﻿using Bootstrapper;
using Commands;
using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Context.Implementation;
using Serilog;
public class Program
{
public static async Task Main(string[] args)
{
Log.Information("🚀 Application started at {StartTime}", DateTimeOffset.Now);
try
{
using var host = AppHostBuilder.Create(args).Build();
Log.Information("Initializing database...");
EnsureDatabaseInitialized(host.Services);
Log.Information("Database initialized successfully");
var config = host.Services.GetRequiredService<AppConfig>();
var executionMode = host.Services
.GetRequiredService<IConfiguration>()
.GetValue<ExecutionMode>("ExecutionMode");
if (executionMode == ExecutionMode.Command)
{
Log.Information("🧭 ExecutionMode = Command");
var commandFactory = host.Services.GetRequiredService<CommandFactory>();
var commands = commandFactory.CreateCommand().ToList();
var jobArgs = host.Services.GetRequiredService<CommandArgs>();
Log.Information("Discovered {CommandCount} command(s) to execute", commands.Count);
foreach (var command in commands)
{
var commandName = command.GetType().Name;
try
{
Log.Information("▶ Executing command: {CommandName}", commandName);
await command.ExecuteAsync(jobArgs.Arguments);
Log.Information("✔ Command completed successfully: {CommandName}", commandName);
}
catch (Exception ex)
{
Log.Error(ex, "✖ Command execution failed: {CommandName}", commandName);
throw new AggregateException($"Command '{commandName}' failed", ex);
}
}
Log.Information("✅ All commands executed successfully");
return;
}
Log.Information("🧭 ExecutionMode = Scheduler");
Log.Information("Starting host (scheduler mode)...");
await host.RunAsync();
}
catch (Exception ex)
{
Log.Fatal(ex, "❌ Application terminated due to an unrecoverable error");
Environment.ExitCode = 1;
}
finally
{
Log.Information("🧹 Flushing logs and shutting down");
await Log.CloseAndFlushAsync();
}
}
static void EnsureDatabaseInitialized(IServiceProvider services)
{
using var scope = services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DataContext>();
if (!db.Initialize())
{
throw new Exception("Database initialization failed");
}
}
}

=== FILE: F:\Marketing\WhatsAppSender\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\WhatsAppSender\obj\Debug\net8.0\WhatsAppSender.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+94e1a471b9875dcb71d674cd5bee527643160ff7")]
[assembly: System.Reflection.AssemblyProductAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyTitleAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\WhatsAppSender\obj\Debug\net8.0\WhatsAppSender.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;