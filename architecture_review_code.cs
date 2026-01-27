

=== FILE: F:\Marketing\architecture_review_code.cs ===



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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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

=== FILE: F:\Marketing\Application\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Application\obj\Release\net8.0\Application.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Application")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
[assembly: System.Reflection.AssemblyProductAttribute("Application")]
[assembly: System.Reflection.AssemblyTitleAttribute("Application")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Application\obj\Release\net8.0\Application.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Application\PixVerse\IBalanceClient.cs ===

﻿using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface IBalanceClient
{
Task<Operation<AccountCredits>> GetAsync(CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\IImageClient.cs ===

﻿using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface IImageClient
{
Task<Operation<ImageResult>> UploadAsync(
Stream imageStream,
string fileName,
string contentType,
CancellationToken ct = default);
Task<Operation<ImageResult>> UploadAsync(
string imageUrl,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\IImageToVideoClient.cs ===

﻿using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface IImageToVideoClient
{
Task<Operation<JobReceipt>> SubmitAsync(
ImageToVideo request,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\IJobClient.cs ===

﻿using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface IJobClient
{
Task<Operation<JobResult>> WaitForCompletionAsync(
long jobId,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\ILipSyncClient.cs ===

﻿using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface ILipSyncClient
{
Task<Operation<JobReceipt>> SubmitAsync(
VideoLipSync request,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\ITextToVideoClient.cs ===

﻿using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface ITextToVideoClient
{
Task<Operation<JobReceipt>> SubmitTAsync(
TextToVideo request,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\ITransitionClient.cs ===

﻿using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface ITransitionClient
{
Task<Operation<JobReceipt>> SubmitAsync(
VideoTransition request,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\IVideoClient.cs ===

﻿using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface IVideoClient
{
Task<Operation<FileInfo>> DownloadAsync(
long jobId,
string destinationFilePath,
int videoIndex = 0,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\IVideoJobQueryClient.cs ===

﻿using Application.PixVerse.Response;
using Application.Result;
namespace Application.PixVerse
{
public interface IVideoJobQueryClient
{
Task<Operation<JobStatus>> GetStatusAsync(
long jobId,
CancellationToken ct = default);
Task<Operation<JobResult>> GetResultAsync(
long jobId,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Application\PixVerse\Request\ImageToVideo.cs ===

﻿using System.Text.Json.Serialization;
namespace Application.PixVerse.Request
{
public sealed class ImageToVideo
{
[JsonPropertyName("duration")]
public required int Duration { get; init; }
[JsonPropertyName("img_id")]
public required long ImgId { get; init; }
[JsonPropertyName("img_ids")]
public long[]? ImgIds { get; init; }
[JsonPropertyName("model")]
public required string Model { get; init; }
[JsonPropertyName("prompt")]
public required string Prompt { get; init; }
[JsonPropertyName("quality")]
public required string Quality { get; init; }
[JsonPropertyName("negative_prompt")]
public string? NegativePrompt { get; init; }
[JsonPropertyName("motion_mode")]
public string? MotionMode { get; init; }
[JsonPropertyName("style")]
public string? Style { get; init; }
[JsonPropertyName("template_id")]
public long? TemplateId { get; init; }
[JsonPropertyName("sound_effect_switch")]
public bool? SoundEffectSwitch { get; init; }
[JsonPropertyName("sound_effect_content")]
public string? SoundEffectContent { get; init; }
[JsonPropertyName("lip_sync_switch")]
public bool? LipSyncSwitch { get; init; }
[JsonPropertyName("lip_sync_tts_content")]
public string? LipSyncTtsContent { get; init; }
[JsonPropertyName("lip_sync_tts_speaker_id")]
public string? LipSyncTtsSpeakerId { get; init; }
[JsonPropertyName("generate_audio_switch")]
public bool? GenerateAudioSwitch { get; init; }
[JsonPropertyName("generate_multi_clip_switch")]
public bool? GenerateMultiClipSwitch { get; init; }
[JsonPropertyName("thinking_type")]
public string? ThinkingType { get; init; }
[JsonPropertyName("seed")]
public int? Seed { get; init; }
public void Validate()
{
if (Duration <= 0)
throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be > 0.");
if (ImgId <= 0 && (ImgIds is null || ImgIds.Length == 0))
throw new ArgumentException("Either ImgId or ImgIds must be provided.");
if (string.IsNullOrWhiteSpace(Model))
throw new ArgumentException("Model cannot be null/empty.", nameof(Model));
if (string.IsNullOrWhiteSpace(Prompt))
throw new ArgumentException("Prompt cannot be null/empty.", nameof(Prompt));
if (Prompt.Length > 2048)
throw new ArgumentException("Prompt exceeds 2048 characters.", nameof(Prompt));
if (NegativePrompt is not null && NegativePrompt.Length > 2048)
throw new ArgumentException("NegativePrompt exceeds 2048 characters.", nameof(NegativePrompt));
if (string.IsNullOrWhiteSpace(Quality))
throw new ArgumentException("Quality cannot be null/empty.", nameof(Quality));
if (Seed is < 0 or > 2147483647)
throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be between 0 and 2147483647.");
}
}
}

=== FILE: F:\Marketing\Application\PixVerse\Request\TextToVideo.cs ===

﻿using System.Text.Json.Serialization;
namespace Application.PixVerse.Request
{
public sealed class TextToVideo
{
[JsonPropertyName("aspect_ratio")]
public required string AspectRatio { get; init; }
[JsonPropertyName("duration")]
public required int Duration { get; init; }
[JsonPropertyName("model")]
public required string Model { get; init; }
[JsonPropertyName("negative_prompt")]
public string? NegativePrompt { get; init; }
[JsonPropertyName("prompt")]
public required string Prompt { get; init; }
[JsonPropertyName("quality")]
public required string Quality { get; init; }
[JsonPropertyName("seed")]
public int? Seed { get; init; }
[JsonPropertyName("camera_movement")]
public string? CameraMovement { get; init; }
[JsonPropertyName("style")]
public string? Style { get; init; }
[JsonPropertyName("motion_mode")]
public string? MotionMode { get; init; }
[JsonPropertyName("template_id")]
public long? TemplateId { get; init; }
public void Validate()
{
if (string.IsNullOrWhiteSpace(AspectRatio))
throw new ArgumentException("AspectRatio cannot be null/empty.", nameof(AspectRatio));
if (string.IsNullOrWhiteSpace(Model))
throw new ArgumentException("Model cannot be null/empty.", nameof(Model));
if (string.IsNullOrWhiteSpace(Prompt))
throw new ArgumentException("Prompt cannot be null/empty.", nameof(Prompt));
if (Prompt.Length > 2048)
throw new ArgumentException("Prompt exceeds 2048 characters.", nameof(Prompt));
if (NegativePrompt is not null && NegativePrompt.Length > 2048)
throw new ArgumentException("NegativePrompt exceeds 2048 characters.", nameof(NegativePrompt));
if (Duration is not (5 or 8))
throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be 5 or 8.");
if (string.IsNullOrWhiteSpace(Quality))
throw new ArgumentException("Quality cannot be null/empty.", nameof(Quality));
if (Seed is < 0 or > 2147483647)
throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be between 0 and 2147483647.");
}
}
}

=== FILE: F:\Marketing\Application\PixVerse\Request\VideoLipSync.cs ===

﻿using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
namespace Application.PixVerse.Request
{
public sealed class VideoLipSync
{
[JsonPropertyName("video_media_id")]
public long VideoMediaId { get; init; } = 0;
[JsonPropertyName("source_video_id")]
public long? SourceVideoId { get; init; }
[JsonPropertyName("audio_media_id")]
public long? AudioMediaId { get; init; } = 0;
[JsonPropertyName("lip_sync_tts_speaker_id")]
public string? LipSyncTtsSpeakerId { get; init; }
[JsonPropertyName("lip_sync_tts_content")]
public string? LipSyncTtsContent { get; init; }
public void Validate()
{
var hasSourceVideo = SourceVideoId.HasValue && SourceVideoId.Value > 0;
var hasAudioMedia = AudioMediaId.HasValue && AudioMediaId.Value > 0;
var hasTtsPair =
!string.IsNullOrWhiteSpace(LipSyncTtsSpeakerId) &&
!string.IsNullOrWhiteSpace(LipSyncTtsContent);
if (hasAudioMedia == hasTtsPair)
throw new ArgumentException(
"Either AudioMediaId OR (LipSyncTtsSpeakerId + LipSyncTtsContent) must be set (exactly one).");
if (!hasSourceVideo && VideoMediaId <= 0)
throw new ArgumentException(
"Either SourceVideoId or VideoMediaId must be provided.");
if (hasSourceVideo && VideoMediaId > 0)
throw new ArgumentException(
"SourceVideoId and VideoMediaId cannot be provided together.");
if (hasTtsPair)
{
var sanitized = SanitizeToStandardCharacters(LipSyncTtsContent!);
if (string.IsNullOrWhiteSpace(sanitized))
throw new ArgumentException(
"LipSyncTtsContent is empty after sanitization. Only standard characters are allowed.");
}
}
public VideoLipSync Normalize()
{
if (string.IsNullOrWhiteSpace(LipSyncTtsContent))
return this;
var sanitized = SanitizeToStandardCharacters(LipSyncTtsContent);
return new VideoLipSync
{
VideoMediaId = this.VideoMediaId,
SourceVideoId = this.SourceVideoId,
AudioMediaId = this.AudioMediaId,
LipSyncTtsSpeakerId = this.LipSyncTtsSpeakerId,
LipSyncTtsContent = sanitized
};
}
private static string SanitizeToStandardCharacters(string input)
{
var normalized = input.Normalize(NormalizationForm.FormKC);
var sb = new StringBuilder(normalized.Length);
foreach (var ch in normalized)
{
if (char.IsLetterOrDigit(ch) ||
ch == ' ' ||
".,;:!?\"'()-".IndexOf(ch) >= 0)
{
sb.Append(ch);
}
}
var cleaned = Regex.Replace(sb.ToString(), @"\s{2,}", " ").Trim();
return cleaned;
}
}
}

=== FILE: F:\Marketing\Application\PixVerse\Request\VideoTransition.cs ===

﻿using System;
using System.Text.Json.Serialization;
namespace Application.PixVerse.Request
{
public sealed class VideoTransition
{
[JsonIgnore]
public long FromImgId
{
init => FirstFrameImg = value;
}
[JsonIgnore]
public long ToImgId
{
init => LastFrameImg = value;
}
[JsonPropertyName("prompt")]
public required string Prompt { get; init; }
[JsonPropertyName("model")]
public required string Model { get; init; }
[JsonPropertyName("duration")]
public required int Duration { get; init; }
[JsonPropertyName("quality")]
public required string Quality { get; init; }
[JsonPropertyName("motion_mode")]
public string? MotionMode { get; init; }
[JsonPropertyName("seed")]
public int? Seed { get; init; }
[JsonPropertyName("first_frame_img")]
public required long FirstFrameImg { get; init; }
[JsonPropertyName("last_frame_img")]
public required long LastFrameImg { get; init; }
[JsonPropertyName("sound_effect_switch")]
public bool? SoundEffectSwitch { get; init; }
[JsonPropertyName("sound_effect_content")]
public string? SoundEffectContent { get; init; }
[JsonPropertyName("lip_sync_switch")]
public bool? LipSyncSwitch { get; init; }
[JsonPropertyName("lip_sync_tts_content")]
public string? LipSyncTtsContent { get; init; }
[JsonPropertyName("lip_sync_tts_speaker_id")]
public string? LipSyncTtsSpeakerId { get; init; }
[JsonPropertyName("generate_audio_switch")]
public bool? GenerateAudioSwitch { get; init; }
public void Validate()
{
if (string.IsNullOrWhiteSpace(Prompt))
throw new ArgumentException("Prompt cannot be null/empty.", nameof(Prompt));
if (Prompt.Length > 2048)
throw new ArgumentException("Prompt exceeds 2048 characters.", nameof(Prompt));
if (string.IsNullOrWhiteSpace(Model))
throw new ArgumentException("Model cannot be null/empty.", nameof(Model));
if (Duration <= 0)
throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be > 0.");
if (string.IsNullOrWhiteSpace(Quality))
throw new ArgumentException("Quality cannot be null/empty.", nameof(Quality));
if (FirstFrameImg <= 0)
throw new ArgumentOutOfRangeException(nameof(FirstFrameImg), "FirstFrameImg must be > 0.");
if (LastFrameImg <= 0)
throw new ArgumentOutOfRangeException(nameof(LastFrameImg), "LastFrameImg must be > 0.");
if (Seed is < 0 or > 2147483647)
throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be between 0 and 2147483647.");
}
}
}

=== FILE: F:\Marketing\Application\PixVerse\Response\AccountCredits.cs ===

﻿using System.Text.Json.Serialization;
namespace Application.PixVerse.Response
{
public sealed class AccountCredits
{
[JsonPropertyName("account_id")]
public long AccountId { get; init; }
[JsonPropertyName("credit_monthly")]
public int CreditMonthly { get; init; }
[JsonPropertyName("credit_package")]
public int CreditPackage { get; init; }
[JsonIgnore]
public int TotalCredits => CreditMonthly + CreditPackage;
[JsonIgnore]
public DateTimeOffset RetrievedAtUtc { get; init; } = DateTimeOffset.UtcNow;
public bool HasAtLeast(int minimum) => TotalCredits >= minimum;
}
}

=== FILE: F:\Marketing\Application\PixVerse\Response\ImageResult.cs ===

﻿using System.Text.Json.Serialization;
namespace Application.PixVerse.Response
{
public sealed class ImageResult
{
[JsonPropertyName("img_id")]
public long ImgId { get; init; }
[JsonPropertyName("img_url")]
public string? ImgUrl { get; init; }
}
}

=== FILE: F:\Marketing\Application\PixVerse\Response\JobReceipt.cs ===

﻿namespace Application.PixVerse.Response
{
public sealed class JobReceipt
{
public required long JobId { get; init; }
public string? Message { get; init; }
public DateTimeOffset SubmittedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
}

=== FILE: F:\Marketing\Application\PixVerse\Response\JobResult.cs ===

﻿using System.Text.Json.Serialization;
namespace Application.PixVerse.Response
{
public sealed class JobResult
{
[JsonPropertyName("id")]
public long RawJobId { get; init; }
[JsonPropertyName("status")]
public int RawStatus { get; init; }
[JsonPropertyName("url")]
public string? Url { get; init; }
[JsonPropertyName("prompt")]
public string? Prompt { get; init; }
[JsonPropertyName("negative_prompt")]
public string? NegativePrompt { get; init; }
[JsonPropertyName("seed")]
public int? Seed { get; init; }
[JsonPropertyName("size")]
public int? DurationSeconds { get; init; }
[JsonPropertyName("has_audio")]
public bool? HasAudio { get; init; }
[JsonPropertyName("credits")]
public int? Credits { get; init; }
[JsonPropertyName("create_time")]
public DateTimeOffset? CreateTimeUtc { get; init; }
[JsonPropertyName("modify_time")]
public DateTimeOffset? ModifyTimeUtc { get; init; }
[JsonIgnore]
public string JobId => RawJobId.ToString();
[JsonIgnore]
public JobState State => RawStatus switch
{
0 => JobState.Pending,
1 => JobState.Queued,
2 => JobState.Processing,
5 => JobState.Succeeded,
6 => JobState.Failed,
_ => JobState.Unknown
};
[JsonIgnore]
public IReadOnlyList<string> VideoUrls =>
string.IsNullOrWhiteSpace(Url)
? []
: new[] { Url };
[JsonIgnore]
public IReadOnlyList<string> PreviewUrls => Array.Empty<string>();
[JsonIgnore]
public string? ErrorCode => State == JobState.Failed ? RawStatus.ToString() : null;
[JsonIgnore]
public string? ErrorMessage => State == JobState.Failed ? "PixVerse generation failed" : null;
[JsonIgnore]
public DateTimeOffset RetrievedAtUtc { get; init; } = DateTimeOffset.UtcNow;
[JsonIgnore]
public bool HasAnyVideoUrl => !string.IsNullOrWhiteSpace(Url);
}
}

=== FILE: F:\Marketing\Application\PixVerse\Response\JobState.cs ===

﻿namespace Application.PixVerse.Response
{
public enum JobState
{
Unknown = 0,
Queued = 1,
Processing = 2,
Succeeded = 3,
Failed = 4,
Cancelled = 5,
Pending
}
}

=== FILE: F:\Marketing\Application\PixVerse\Response\JobStatus.cs ===

﻿namespace Application.PixVerse.Response
{
public sealed class JobStatus
{
public required string JobId { get; init; }
public JobState State { get; init; } = JobState.Unknown;
public int? ProgressPercent { get; init; }
public string? ErrorCode { get; init; }
public string? ErrorMessage { get; init; }
public DateTimeOffset CheckedAtUtc { get; init; } = DateTimeOffset.UtcNow;
public bool IsTerminal =>
State is JobState.Succeeded
or JobState.Failed
or JobState.Cancelled;
}
}

=== FILE: F:\Marketing\Application\Result\IErrorHandler.cs ===

﻿namespace Application.Result
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

﻿namespace Application.Result
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

=== FILE: F:\Marketing\Application\TrackedLinks\ITrackedLink.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Application.TrackedLinks;
public interface ITrackedLink
{
Task UpsertAsync(string id, string targetUrl, CancellationToken ct = default);
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

=== FILE: F:\Marketing\AzureTable\Program.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Bootstrapper;
using Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
namespace AzureTable
{
public sealed class Program
{
public static async Task Main(string[] args)
{
var runId = Guid.NewGuid().ToString("N")[..8];
var sw = Stopwatch.StartNew();
try
{
using var host = AppHostBuilder.Create(args).Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var executionRunning = TryGetExecutionRunning(host.Services) ?? null;
if(executionRunning is null)
{
return;
}
var logsFolder = Path.Combine(executionRunning.ExecutionRunning, "Logs");
var logFilePattern = Path.Combine(logsFolder, $"Marketing-{executionRunning.TimeStamp}.log");
logger.LogInformation("[RUN {RunId}] === START PixVerse IMAGE->VIDEO + LIPSYNC(TTS) (FIXED) ===", runId);
logger.LogInformation("[RUN {RunId}] ArgsCount={ArgsCount}", runId, args?.Length ?? 0);
logger.LogInformation("[RUN {RunId}] LOG FILE TARGET (pattern) => {LogFilePattern}", runId, logFilePattern);
var imageClient = host.Services.GetRequiredService<IImageClient>();
var balanceClient = host.Services.GetRequiredService<IBalanceClient>();
var videoJobQueryClient = host.Services.GetRequiredService<IVideoJobQueryClient>();
var imageToVideoClient = host.Services.GetRequiredService<IImageToVideoClient>();
var lipSyncClient = host.Services.GetRequiredService<ILipSyncClient>();
logger.LogInformation("[RUN {RunId}] [STEP 1] Checking balance...", runId);
var balOp = await balanceClient.GetAsync();
if (!balOp.IsSuccessful)
{
logger.LogError(
"[RUN {RunId}] [STEP 1] Balance FAILED. Error={Error}. ElapsedMs={ElapsedMs}",
runId, balOp.Error ?? "unknown", sw.ElapsedMilliseconds);
return;
}
logger.LogInformation(
"[RUN {RunId}] [STEP 1] Balance OK. Credits={Credits}. ElapsedMs={ElapsedMs}",
runId, balOp.Data?.TotalCredits, sw.ElapsedMilliseconds);
var imagePath = @"E:\Marketing-Logs\PixVerse\Inputs\gustavo.webp";
var fileName = Path.GetFileName(imagePath);
var fileExt = Path.GetExtension(imagePath);
var contentType = GuessContentType(fileExt);
logger.LogInformation(
"[RUN {RunId}] [STEP 2] Upload image START. Path={ImagePath} FileName={FileName} Ext={Ext} ContentType={ContentType} Exists={Exists} SizeBytes={SizeBytes}",
runId,
imagePath,
fileName,
fileExt,
contentType,
File.Exists(imagePath),
File.Exists(imagePath) ? new FileInfo(imagePath).Length : -1);
if (!File.Exists(imagePath))
{
logger.LogError("[RUN {RunId}] [STEP 2] Upload image ABORT. File does not exist. Path={ImagePath}", runId, imagePath);
return;
}
await using var imgStream = File.OpenRead(imagePath);
var upOp = await imageClient.UploadAsync(
imgStream,
fileName: fileName,
contentType: contentType);
if (!upOp.IsSuccessful || upOp.Data is null)
{
logger.LogError(
"[RUN {RunId}] [STEP 2] Upload FAILED. Error={Error}. PayloadNull={PayloadNull}. ElapsedMs={ElapsedMs}",
runId, upOp.Error ?? "unknown", upOp.Data is null, sw.ElapsedMilliseconds);
return;
}
var imgId = upOp.Data.ImgId;
logger.LogInformation(
"[RUN {RunId}] [STEP 2] Upload OK. ImgId={ImgId}. ElapsedMs={ElapsedMs}",
runId, imgId, sw.ElapsedMilliseconds);
var i2vReq = new ImageToVideo
{
ImgId = imgId,
Duration = 5,
Model = "v5",
Quality = "540p",
Prompt = "adult guy speaking directly to camera, serious style, expressive mouth, clear face, neutral background",
NegativePrompt = "blurry, distorted face, artifacts",
Seed = 0
};
logger.LogInformation(
"[RUN {RunId}] [STEP 3] I2V Submit START. ImgId={ImgId} Duration={Duration} Model={Model} Quality={Quality} Seed={Seed} PromptLen={PromptLen} NegPromptLen={NegPromptLen}",
runId,
i2vReq.ImgId,
i2vReq.Duration,
i2vReq.Model,
i2vReq.Quality,
i2vReq.Seed,
i2vReq.Prompt?.Length ?? 0,
i2vReq.NegativePrompt?.Length ?? 0);
var i2vSubmitOp = await imageToVideoClient.SubmitAsync(i2vReq);
if (!i2vSubmitOp.IsSuccessful || i2vSubmitOp.Data is null)
{
logger.LogError(
"[RUN {RunId}] [STEP 3] I2V Submit FAILED. Error={Error}. PayloadNull={PayloadNull}. ElapsedMs={ElapsedMs}",
runId, i2vSubmitOp.Error ?? "unknown", i2vSubmitOp.Data is null, sw.ElapsedMilliseconds);
return;
}
var jobId = i2vSubmitOp.Data.JobId;
logger.LogInformation(
"[RUN {RunId}] [STEP 3] I2V Submit OK. JobId={JobId}. ElapsedMs={ElapsedMs}",
runId, jobId, sw.ElapsedMilliseconds);
logger.LogInformation(
"[RUN {RunId}] [STEP 4] Poll I2V status START. JobId={JobId} MaxAttempts={MaxAttempts} DelaySec={DelaySec}",
runId, jobId, 60, 2);
JobStatus? finalStatus = null;
for (var attempt = 1; attempt <= 60; attempt++)
{
var attemptSw = Stopwatch.StartNew();
var stOp = await videoJobQueryClient.GetStatusAsync(jobId);
if (!stOp.IsSuccessful || stOp.Data is null)
{
logger.LogWarning(
"[RUN {RunId}] [STEP 4] I2V Status attempt {Attempt} FAILED. Error={Error}. PayloadNull={PayloadNull}. AttemptMs={AttemptMs} ElapsedMs={ElapsedMs}",
runId, attempt, stOp.Error ?? "unknown", stOp.Data is null, attemptSw.ElapsedMilliseconds, sw.ElapsedMilliseconds);
}
else
{
logger.LogInformation(
"[RUN {RunId}] [STEP 4] I2V Status attempt {Attempt} OK. State={State} IsTerminal={IsTerminal} AttemptMs={AttemptMs} ElapsedMs={ElapsedMs}",
runId, attempt, stOp.Data.State, stOp.Data.IsTerminal, attemptSw.ElapsedMilliseconds, sw.ElapsedMilliseconds);
if (stOp.Data.IsTerminal)
{
finalStatus = stOp.Data;
break;
}
}
await Task.Delay(TimeSpan.FromSeconds(2));
}
if (finalStatus is null)
{
logger.LogError(
"[RUN {RunId}] [STEP 4] I2V Status polling TIMEOUT. JobId={JobId}. ElapsedMs={ElapsedMs}",
runId, jobId, sw.ElapsedMilliseconds);
return;
}
if (finalStatus.State != JobState.Succeeded)
{
logger.LogError(
"[RUN {RunId}] [STEP 4] I2V job NOT SUCCEEDED. JobId={JobId} FinalState={State} ElapsedMs={ElapsedMs}",
runId, jobId, finalStatus.State, sw.ElapsedMilliseconds);
return;
}
logger.LogInformation(
"[RUN {RunId}] [STEP 4] I2V job SUCCEEDED. JobId={JobId} ElapsedMs={ElapsedMs}",
runId, jobId, sw.ElapsedMilliseconds);
logger.LogInformation(
"[RUN {RunId}] [STEP 5] Get I2V result START. JobId={JobId}",
runId, jobId);
var resOp = await videoJobQueryClient.GetResultAsync(jobId);
if (!resOp.IsSuccessful || resOp.Data is null)
{
logger.LogError(
"[RUN {RunId}] [STEP 5] Get result FAILED. JobId={JobId} Error={Error} PayloadNull={PayloadNull} ElapsedMs={ElapsedMs}",
runId, jobId, resOp.Error ?? "unknown", resOp.Data is null, sw.ElapsedMilliseconds);
return;
}
var resultJson = SafeSerialize(resOp.Data);
logger.LogInformation(
"[RUN {RunId}] [STEP 5] Get result OK. JobId={JobId} ResultJsonLen={Len} ElapsedMs={ElapsedMs}",
runId, jobId, resultJson.Length, sw.ElapsedMilliseconds);
long? videoMediaId = TryGetVideoMediaIdFromKnownModel(resOp.Data);
if (videoMediaId is null || videoMediaId <= 0)
{
videoMediaId = TryExtractMediaIdFromJson(resultJson);
logger.LogInformation(
"[RUN {RunId}] [STEP 5] MediaId extraction (fallback) VideoMediaId={VideoMediaId}",
runId, videoMediaId ?? 0);
}
if (videoMediaId is null || videoMediaId <= 0)
{
logger.LogError(
"[RUN {RunId}] [STEP 5] Cannot proceed: VideoMediaId NOT FOUND. JobId={JobId}. ResultJson={ResultJson}",
runId, jobId, resultJson);
return;
}
logger.LogInformation(
"[RUN {RunId}] [STEP 5] Using VideoMediaId={VideoMediaId} for LipSync (NOT JobId={JobId}).",
runId, videoMediaId, jobId);
var lipReq = new VideoLipSync
{
VideoMediaId = videoMediaId.Value,
SourceVideoId = null,
AudioMediaId = null,
LipSyncTtsSpeakerId = "auto",
LipSyncTtsContent = "¡Hola Vancouver! Soy Goku. No olviden apoyar al Tricolor Fan Club. ¡Vamos con toda!"
};
logger.LogInformation(
"[RUN {RunId}] [STEP 6] LipSync Submit START. VideoMediaId={VideoMediaId} Speaker={Speaker} ContentLen={ContentLen} HasAudioMediaId={HasAudioMediaId} HasSourceVideoId={HasSourceVideoId}",
runId,
lipReq.VideoMediaId,
lipReq.LipSyncTtsSpeakerId,
lipReq.LipSyncTtsContent?.Length ?? 0,
lipReq.AudioMediaId.HasValue && lipReq.AudioMediaId.Value > 0,
lipReq.SourceVideoId.HasValue && lipReq.SourceVideoId.Value > 0);
var lipOp = await lipSyncClient.SubmitAsync(lipReq);
if (!lipOp.IsSuccessful)
{
logger.LogError(
"[RUN {RunId}] [STEP 6] LipSync Submit FAILED. Error={Error}. ElapsedMs={ElapsedMs}",
runId, lipOp.Error ?? "unknown", sw.ElapsedMilliseconds);
return;
}
logger.LogInformation(
"[RUN {RunId}] [STEP 6] LipSync Submit OK. LipJobId={LipJobId}. TotalElapsedMs={ElapsedMs}",
runId, lipOp.Data?.JobId, sw.ElapsedMilliseconds);
}
catch (Exception ex)
{
Log.Fatal(ex, "[RUN {RunId}] Unhandled exception. Application terminating.", runId);
Environment.ExitCode = 1;
}
finally
{
Log.Information("[RUN {RunId}] Flushing logs. TotalElapsedMs={ElapsedMs}", runId, sw.ElapsedMilliseconds);
await Log.CloseAndFlushAsync();
}
}
private static ExecutionTracker? TryGetExecutionRunning(IServiceProvider services)
{
try
{
var tracker = services.GetService(typeof(ExecutionTracker)) as ExecutionTracker;
return tracker;
}
catch
{
return null;
}
}
private static long? TryGetVideoMediaIdFromKnownModel(JobResult result)
{
return null;
}
private static string SafeSerialize<T>(T obj)
{
try { return JsonSerializer.Serialize(obj); }
catch { return "(serialize_failed)"; }
}
private static long? TryExtractMediaIdFromJson(string json)
{
if (string.IsNullOrWhiteSpace(json) || json == "(serialize_failed)")
return null;
try
{
using var doc = JsonDocument.Parse(json);
var root = doc.RootElement;
var candidates = new[]
{
"video_media_id",
"videoMediaId",
"media_id",
"mediaId",
"video_id",
"videoId"
};
foreach (var c in candidates)
{
if (TryFindLong(root, c, out var value) && value > 0)
return value;
}
return null;
}
catch
{
return null;
}
}
private static bool TryFindLong(JsonElement el, string propertyName, out long value)
{
value = 0;
if (el.ValueKind == JsonValueKind.Object)
{
if (el.TryGetProperty(propertyName, out var prop))
{
if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt64(out value))
return true;
if (prop.ValueKind == JsonValueKind.String && long.TryParse(prop.GetString(), out value))
return true;
}
foreach (var p in el.EnumerateObject())
{
if (TryFindLong(p.Value, propertyName, out value))
return true;
}
}
else if (el.ValueKind == JsonValueKind.Array)
{
foreach (var item in el.EnumerateArray())
{
if (TryFindLong(item, propertyName, out value))
return true;
}
}
return false;
}
private static string GuessContentType(string ext)
{
ext = (ext ?? string.Empty).Trim().ToLowerInvariant();
return ext switch
{
".jpg" or ".jpeg" => "image/jpeg",
".png" => "image/png",
".webp" => "image/webp",
".gif" => "image/gif",
_ => "application/octet-stream"
};
}
}
}

=== FILE: F:\Marketing\AzureTable\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\AzureTable\obj\Debug\net8.0\AzureTable.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("AzureTable")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+9cf37dd2c874cb16f7035747f1f95ced1f2c5baf")]
[assembly: System.Reflection.AssemblyProductAttribute("AzureTable")]
[assembly: System.Reflection.AssemblyTitleAttribute("AzureTable")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\AzureTable\obj\Debug\net8.0\AzureTable.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Bootstrapper\AppHostBuilder.cs ===

﻿using Application.PixVerse;
using Application.Result;
using Application.TrackedLinks;
using Commands;
using Configuration;
using Configuration.PixVerse;
using Configuration.UrlValidation;
using Configuration.YouTube;
using Infrastructure.AzureTables;
using Infrastructure.PixVerse;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Persistence.Context.Implementation;
using Persistence.Context.Interceptors;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Serilog;
using Services.Abstractions.AutoIt;
using Services.Abstractions.Check;
using Services.Abstractions.Login;
using Services.Abstractions.OpenAI;
using Services.Abstractions.OpenAI.news;
using Services.Abstractions.OpenChat;
using Services.Abstractions.Search;
using Services.Abstractions.UrlValidation;
using Services.Abstractions.YouTube;
using Services.AutoIt;
using Services.Check;
using Services.Login;
using Services.OpenAI;
using Services.OpenAI.news;
using Services.OpenChat;
using Services.Selector;
using Services.UrlValidation;
using Services.WhatsApp;
using Services.YouTube;
using System.Net.Http.Headers;
namespace Bootstrapper
{
public static class AppHostBuilder
{
private const string AppSettingsFileName = "appsettings.json";
private const string Connection = "Connection string 'DefaultConnection' is missing or empty.";
private const string FailureMessage = "WhatsApp:Message configuration is incomplete";
private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";
public static IHostBuilder Create(string[] args)
{
var appConfig = new AppConfig();
var basePath = Directory.GetCurrentDirectory();
var appSettingsPath = Path.Combine(basePath, AppSettingsFileName);
if (!File.Exists(appSettingsPath))
{
var configMessage = $"Required configuration file '{AppSettingsFileName}' was not found.";
Log.Warning(configMessage);
throw new FileNotFoundException(configMessage, appSettingsPath);
}
Log.Information($"The configuration file '{AppSettingsFileName}' was found.");
return Host.CreateDefaultBuilder(args)
.ConfigureAppConfiguration((hostingContext, config) =>
{
Configure(config, basePath);
})
.ConfigureServices((hostingContext, services) =>
{
services.AddOptions<SchedulerOptions>()
.Bind(hostingContext.Configuration.GetSection(SchedulerOptions.SectionName))
.PostConfigure(SetScheduler)
.ValidateOnStart();
services.AddOptions<MessageConfig>()
.Bind(hostingContext.Configuration.GetSection("WhatsApp:Message"))
.Validate(o =>
!string.IsNullOrWhiteSpace(o.ImageDirectory) &&
!string.IsNullOrWhiteSpace(o.ImageFileName) &&
!string.IsNullOrWhiteSpace(o.Caption),
FailureMessage
);
services.AddOptions<AzureTablesConfig>()
.Bind(hostingContext.Configuration.GetSection("WhatsApp:AzureTables"))
.Validate(o => !string.IsNullOrWhiteSpace(o.ServiceSasUrl),
"WhatsApp:AzureTables:ServiceSasUrl is required.")
.ValidateOnStart();
services.AddScoped<ITrackedLink>(sp =>
{
var opt = sp.GetRequiredService<IOptions<AzureTablesConfig>>().Value;
return new TrackedLink(opt.ServiceSasUrl);
});
services.AddOptions<OpenAIConfig>()
.Bind(hostingContext.Configuration.GetSection("WhatsApp:OpenAI"))
.Validate(o =>
!string.IsNullOrWhiteSpace(o.ApiKey) &&
!string.IsNullOrWhiteSpace(o.UriString) &&
!string.IsNullOrWhiteSpace(o.Model),
"Configuration WhatsApp:OpenAI is required.")
.ValidateOnStart();
services.AddHttpClient<IOpenAIClient, OpenAIClient>((sp, http) =>
{
var opt = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;
http.BaseAddress = new Uri(opt.UriString);
http.Timeout = TimeSpan.FromSeconds(60);
var apiKey = Environment.GetEnvironmentVariable(opt.ApiKey, EnvironmentVariableTarget.Machine);
if (string.IsNullOrWhiteSpace(apiKey))
throw new InvalidOperationException($"OpenAI API key env var '{opt.ApiKey}' was not found at Machine scope.");
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
hostingContext.Configuration.Bind(appConfig);
var executionMode = hostingContext.Configuration.GetValue<ExecutionMode>("ExecutionMode");
if (executionMode == ExecutionMode.Scheduler)
services.AddHostedService<ScheduledMessenger>();
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
services.AddTransient<WhatsAppCommand>();
services.AddTransient<HelpCommand>();
services.AddHostedService<WebDriverLifetimeService>();
}
services.AddScoped<IMessage, Message>();
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
services.AddTransient<IOpenChat, OpenChat>();
services.AddTransient<IChatService, ChatService>();
services.AddTransient<IAttachments, Attachments>();
services.AddTransient<IAutoItRunner, AutoItRunner>();
AddDbContextSQLite(hostingContext, services);
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IDataContext, DataContext>();
services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());
services.AddSingleton<IErrorLogger, SerilogErrorLogger>();
services.AddScoped<IErrorHandler, ErrorHandler>();
services.AddSingleton<IColumnTypes, SQLite>();
services.AddSingleton<INewsHistoryStore, NewsHistoryStore>();
services.AddSingleton<INostalgiaPromptLoader, NostalgiaPromptLoader>();
services.AddSingleton<IJsonPromptRunner, JsonPromptRunner>();
services.AddUrlValidation(hostingContext.Configuration);
services.AddSingleton<ImageClient, ImageClient>();
services.AddSingleton<IBalanceClient, BalanceClient>();
services.AddSingleton<ImageClient, ImageClient>();
services.AddSingleton<IImageToVideoClient, ImageToVideoClient>();
services.AddSingleton<IVideoClient, VideoClient>();
services.AddSingleton<IJobClient, JobClient>();
services.AddSingleton<IVideoJobQueryClient, VideoJobQueryClient>();
services.AddSingleton<ILipSyncClient, LipSyncClient>();
services.AddSingleton<IImageClient, ImageClient>();
services.AddOptions<YouTubeApiOptions>()
.Bind(hostingContext.Configuration.GetSection(YouTubeApiOptions.SectionName))
.Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "YouTube:ApiKey is required.")
.Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "YouTube:BaseUrl is required.")
.ValidateOnStart();
services.AddHttpClient<IYouTubeService, YouTubeService>((sp, http) =>
{
var opt = sp.GetRequiredService<IOptions<YouTubeApiOptions>>().Value;
http.BaseAddress = new Uri(opt.BaseUrl);
http.DefaultRequestHeaders.Accept.Add(
new MediaTypeWithQualityHeaderValue("application/json"));
});
services.AddSingleton<IYouTubeViralVideoDiscoverer, YouTubeViralVideoDiscoverer>();
services.AddSingleton<IPlatformResolver, PlatformResolver>();
services.AddOptions<PixVerseOptions>()
.Bind(hostingContext.Configuration.GetSection("PixVerse"))
.ValidateOnStart();
})
.UseSerilog((context, services, loggerConfig) =>
{
var execution = services.GetRequiredService<ExecutionTracker>();
var logPath = Path.Combine(execution.ExecutionRunning, "Logs");
Directory.CreateDirectory(logPath);
loggerConfig
.MinimumLevel.Debug()
.WriteTo.Console()
.WriteTo.File(
path: Path.Combine(logPath, "Marketing-.log"),
rollingInterval: RollingInterval.Day,
fileSizeLimitBytes: 5_000_000,
retainedFileCountLimit: 7,
rollOnFileSizeLimit: true,
outputTemplate: OutputTemplate
);
});
}
public static IServiceCollection AddUrlValidation(this IServiceCollection services, IConfiguration cfg)
{
services.Configure<UrlOptions>(cfg.GetSection(UrlOptions.SectionName));
services.AddSingleton<IPlatformResolver, PlatformResolver>();
services.AddSingleton<IUrlFactory, UrlValidatorFactory>();
services.AddSingleton<IValidationPipeline, UrlValidationPipeline>();
services.AddHttpClient<YouTubeUrlValidator>();
services.AddHttpClient<TikTokUrlValidator>();
services.AddHttpClient<InstagramUrlValidator>();
services.AddSingleton<IUrValidator>(sp => sp.GetRequiredService<YouTubeUrlValidator>());
services.AddSingleton<IUrValidator>(sp => sp.GetRequiredService<TikTokUrlValidator>());
services.AddSingleton<IUrValidator>(sp => sp.GetRequiredService<InstagramUrlValidator>());
return services;
}
private static void SetScheduler(SchedulerOptions o)
{
foreach (var key in o.Weekly.Keys.ToList())
{
var list = o.Weekly[key] ?? [];
List<string> value =
[
.. list
.Where(x => !string.IsNullOrWhiteSpace(x))
.Select(x => x.Trim())
.Distinct(StringComparer.OrdinalIgnoreCase)
.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
];
o.Weekly[key] = value;
}
}
private static void Configure(IConfigurationBuilder config, string basePath)
{
config.SetBasePath(basePath);
config.AddJsonFile(
path: AppSettingsFileName,
optional: false,
reloadOnChange: true
);
config.AddEnvironmentVariables();
}
public static void LogCleanupReport(CleanupReport report)
{
if (report is null)
{
Log.Warning("The folder is clean");
return;
}
if (report.IsClean)
{
Log.Information("Execution cleanup completed with no errors");
}
else
{
Log.Warning(
"Execution cleanup completed with {FailureCount} failure(s)",
report.DeleteFailures.Count
);
}
}
private static void AddDbContextSQLite(HostBuilderContext context, IServiceCollection services)
{
var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
throw new ArgumentNullException(nameof(context), Connection);
services.AddHttpClient();
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddDistributedMemoryCache();
services.AddDbContext<DataContext>(options =>
{
options
.UseSqlite(connectionString, sqlite =>
{
string? name = typeof(DataContext).Assembly.GetName().Name;
sqlite.MigrationsAssembly(name);
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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

=== FILE: F:\Marketing\Bootstrapper\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Release\net8.0\Bootstrapper.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Bootstrapper")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
[assembly: System.Reflection.AssemblyProductAttribute("Bootstrapper")]
[assembly: System.Reflection.AssemblyTitleAttribute("Bootstrapper")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Bootstrapper\obj\Release\net8.0\Bootstrapper.GlobalUsings.g.cs ===

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
using Services.Abstractions.Check;
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

﻿using Microsoft.Extensions.Logging;
using Services.Abstractions.Login;
namespace Commands
{
public class WhatsAppCommand(ILogger<WhatsAppCommand> logger, IMessage iWhatsAppMessage) : ICommand
{
private ILogger<WhatsAppCommand> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));
private IMessage IWhatsAppMessage { get; } = iWhatsAppMessage ?? throw new ArgumentNullException(nameof(WhatsAppCommand));
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
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

=== FILE: F:\Marketing\Common\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Common\obj\Debug\net8.0\Common.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Common")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
[assembly: System.Reflection.AssemblyProductAttribute("Common")]
[assembly: System.Reflection.AssemblyTitleAttribute("Common")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Common\obj\Debug\net8.0\Common.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Common\StringExtensions\JsonExtractionExtensions.cs ===

﻿namespace Common.StringExtensions
{
public static class JsonExtractionExtensions
{
public static string ExtractJsonContent(this string? input)
{
if (string.IsNullOrWhiteSpace(input))
return string.Empty;
const string JsonFence = "```json";
const string Fence = "```";
if (!input.Contains(Fence, StringComparison.Ordinal))
return input.Trim();
var startIndex = input.IndexOf(JsonFence, StringComparison.OrdinalIgnoreCase);
if (startIndex >= 0)
{
startIndex += JsonFence.Length;
var endIndex = input.IndexOf(Fence, startIndex, StringComparison.Ordinal);
if (endIndex < 0)
endIndex = input.Length;
return input[startIndex..endIndex].Trim();
}
startIndex = input.IndexOf(Fence, StringComparison.Ordinal);
if (startIndex < 0)
return input.Trim();
startIndex += Fence.Length;
var genericEndIndex = input.IndexOf(Fence, startIndex, StringComparison.Ordinal);
if (genericEndIndex < 0)
genericEndIndex = input.Length;
return input[startIndex..genericEndIndex].Trim();
}
}
}

=== FILE: F:\Marketing\Configuration\AppConfig.cs ===

﻿namespace Configuration
{
public class AppConfig
{
public WhatsAppConfig WhatsApp { get; set; }
public PathsConfig Paths { get; set; }
}
}

=== FILE: F:\Marketing\Configuration\AzureTablesConfig.cs ===

﻿namespace Configuration
{
public sealed class AzureTablesConfig
{
public const string SectionName = "AzureTables";
public string ServiceSasUrl { get; init; } = default!;
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

=== FILE: F:\Marketing\Configuration\MessageConfig.cs ===

﻿namespace Configuration
{
public sealed class MessageConfig
{
public string ImageDirectory { get; init; } = null!;
public string ImageFileName { get; init; } = null!;
public string Caption { get; init; } = null!;
}
}

=== FILE: F:\Marketing\Configuration\OpenAIConfig.cs ===

﻿namespace Configuration
{
public sealed class OpenAIConfig
{
public string ApiKey { get; init; } = null!;
public string UriString { get; init; } = null!;
public string Model { get; init; } = null!;
}
}

=== FILE: F:\Marketing\Configuration\PathsConfig.cs ===

﻿namespace Configuration
{
public class PathsConfig
{
public required string OutFolder { get; set; }
public required string AutoItInterpreterPath { get; set; }
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
public required MessageConfig Message { get; init; }
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
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

=== FILE: F:\Marketing\Configuration\PixVerse\PixVerseOptions.cs ===

﻿namespace Configuration.PixVerse
{
public sealed class PixVerseOptions
{
public required string BaseUrl { get; init; }
public required string ApiKey { get; init; }
public TimeSpan HttpTimeout { get; init; } = TimeSpan.FromSeconds(30);
public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(3);
public int MaxPollingAttempts { get; init; } = 40;
public decimal MinimumRequiredBalance { get; init; } = 0m;
}
}

=== FILE: F:\Marketing\Configuration\UrlValidation\UrlOptions.cs ===

﻿namespace Configuration.UrlValidation
{
public sealed class UrlOptions
{
public const string SectionName = "UrlValidation";
public int TimeoutSeconds { get; set; } = 15;
public int MaxBodyCharsToScan { get; set; } = 200_000;
public PlatformRules YouTube { get; set; } = new();
public PlatformRules TikTok { get; set; } = new();
public PlatformRules Instagram { get; set; } = new();
}
public sealed class PlatformRules
{
public List<string> MustContain { get; set; } = [];
public List<string> MustNotContain { get; set; } = [];
}
}

=== FILE: F:\Marketing\Configuration\YouTube\YouTubeApiOptions.cs ===

﻿namespace Configuration.YouTube
{
public sealed class YouTubeApiOptions
{
public const string SectionName = "YouTube";
public string ApiKey { get; init; } = null!;
public string BaseUrl { get; init; } = "https://www.googleapis.com/youtube/v3/";
}
}

=== FILE: F:\Marketing\Configuration\YouTube\YouTubeCurationRunnerOptions.cs ===

﻿namespace Configuration.YouTube
{
public sealed class YouTubeCurationRunnerOptions
{
public const string SectionName = "YouTubeCurationRunner";
public string Query { get; init; } = "Colombia 2014 world cup highlights James Rodriguez";
public SearchOptions Search { get; init; } = new();
}
public sealed class SearchOptions
{
public int MaxResults { get; init; } = 25;
public string? RegionCode { get; init; } = "CA";
public string? RelevanceLanguage { get; init; } = "es";
public string? Order { get; init; } = "viewCount";
public string? PublishedAfterIso { get; init; }
public string? PublishedBeforeIso { get; init; }
public string? SafeSearch { get; init; }
}
}

=== FILE: F:\Marketing\Domain\AutoItRunnerResult.cs ===

﻿namespace Domain
{
public sealed class AutoItRunnerResult
{
public required int ExitCode { get; init; }
public required bool TimedOut { get; init; }
public required string StdOut { get; init; }
public required string StdErr { get; init; }
public required TimeSpan Duration { get; init; }
public string? LogFilePath { get; init; }
}
}

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

=== FILE: F:\Marketing\Domain\ImageMessagePayload.cs ===

﻿namespace Domain
{
public sealed class ImageMessagePayload
{
public string StoredImagePath { get; init; } = default!;
public string Caption { get; init; } = string.Empty;
}
}

=== FILE: F:\Marketing\Domain\NostalgiaPrompt.cs ===

﻿using System.Text.Json;
using System.Text.Json.Serialization;
namespace Prompts.NostalgiaRank
{
public sealed class NostalgiaRankPrompt
{
[JsonPropertyName("prompt_name")]
public string PromptName { get; init; } = "";
[JsonPropertyName("role")]
public List<string> Role { get; init; } = [];
[JsonPropertyName("context")]
public NostalgiaRankContext Context { get; init; } = new();
[JsonPropertyName("task")]
public NostalgiaRankTask Task { get; init; } = new();
}
public sealed class NostalgiaRankContext
{
[JsonPropertyName("audience")]
public string Audience { get; init; } = "";
[JsonPropertyName("language_of_output")]
public string LanguageOfOutput { get; init; } = "";
[JsonPropertyName("platform")]
public string Platform { get; init; } = "";
[JsonPropertyName("historical_rule")]
public List<string> HistoricalRule { get; init; } = [];
[JsonPropertyName("temporal_focus")]
public List<string> TemporalFocus { get; init; } = [];
[JsonPropertyName("input_description")]
public NostalgiaRankInputDescription InputDescription { get; init; } = new();
}
public sealed class NostalgiaRankInputDescription
{
[JsonPropertyName("new_urls")]
public string NewUrls { get; init; } = "";
[JsonPropertyName("historical_urls")]
public string HistoricalUrls { get; init; } = "";
}
public sealed class NostalgiaRankTask
{
[JsonPropertyName("goal")]
public List<string> Goal { get; init; } = [];
[JsonPropertyName("evaluation_criteria")]
public NostalgiaRankEvaluationCriteria EvaluationCriteria { get; init; } = new();
}
public sealed class NostalgiaRankEvaluationCriteria
{
[JsonPropertyName("score_viral")]
public string ScoreViral { get; init; } = "";
}
public interface INostalgiaRankPromptLoader
{
Task<NostalgiaRankPrompt> LoadAsync(string jsonPath, CancellationToken ct = default);
}
public sealed class NostalgiaRankPromptLoader : INostalgiaRankPromptLoader
{
private static readonly JsonSerializerOptions JsonOptions = new()
{
PropertyNameCaseInsensitive = true,
ReadCommentHandling = JsonCommentHandling.Skip,
AllowTrailingCommas = true
};
public async Task<NostalgiaRankPrompt> LoadAsync(string jsonPath, CancellationToken ct = default)
{
if (string.IsNullOrWhiteSpace(jsonPath))
throw new ArgumentException("JSON path is required.", nameof(jsonPath));
if (!File.Exists(jsonPath))
throw new FileNotFoundException("Prompt JSON file not found.", jsonPath);
var json = await File.ReadAllTextAsync(jsonPath, ct).ConfigureAwait(false);
var prompt = JsonSerializer.Deserialize<NostalgiaRankPrompt>(json, JsonOptions);
if (prompt is null)
throw new InvalidOperationException("Failed to deserialize NostalgiaRankPrompt from JSON.");
return prompt;
}
}
public static class NostalgiaRankPromptPlaceholderReplacer
{
public static NostalgiaRankPrompt ReplaceUrlPlaceholders(
NostalgiaRankPrompt prompt,
IReadOnlyList<string> newUrls,
IReadOnlyList<string> historicalUrls)
{
if (prompt is null) throw new ArgumentNullException(nameof(prompt));
string newUrlsJson = JsonSerializer.Serialize(newUrls ?? []);
string historicalUrlsJson = JsonSerializer.Serialize(historicalUrls ?? []);
var input = prompt.Context.InputDescription;
var replacedInput = new NostalgiaRankInputDescription
{
NewUrls = (input.NewUrls ?? "").Replace("__NEW_URLS__", newUrlsJson),
HistoricalUrls = (input.HistoricalUrls ?? "").Replace("__HISTORICAL_URLS__", historicalUrlsJson)
};
return new NostalgiaRankPrompt
{
PromptName = prompt.PromptName,
Role = prompt.Role,
Context = new NostalgiaRankContext
{
Audience = prompt.Context.Audience,
LanguageOfOutput = prompt.Context.LanguageOfOutput,
Platform = prompt.Context.Platform,
HistoricalRule = prompt.Context.HistoricalRule,
TemporalFocus = prompt.Context.TemporalFocus,
InputDescription = replacedInput
},
Task = prompt.Task
};
}
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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

=== FILE: F:\Marketing\Domain\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Domain\obj\Release\net8.0\Domain.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Domain")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
[assembly: System.Reflection.AssemblyProductAttribute("Domain")]
[assembly: System.Reflection.AssemblyTitleAttribute("Domain")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Domain\obj\Release\net8.0\Domain.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Domain\OpenAI\OpenAIChatChoice.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Domain.OpenAI
{
using System.Text.Json.Serialization;
public class OpenAIChatChoice
{
[JsonPropertyName("message")]
public required OpenAIMessage Message { get; set; }
[JsonPropertyName("finish_reason")]
public required string FinishReason { get; set; }
[JsonPropertyName("index")]
public int Index { get; set; }
}
}

=== FILE: F:\Marketing\Domain\OpenAI\OpenAIChatRequest.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Domain.OpenAI
{
public class OpenAIChatRequest(string model)
{
[JsonPropertyName("model")]
public string Model { get; } = model;
[JsonPropertyName("messages")]
public required List<OpenAIMessage> Messages { get; set; }
[JsonPropertyName("stream")]
public bool Stream { get; set; } = false;
}
}

=== FILE: F:\Marketing\Domain\OpenAI\OpenAIChatResponse.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Domain.OpenAI
{
public class OpenAIChatResponse
{
[JsonPropertyName("choices")]
public required List<OpenAIChatChoice> Choices { get; set; }
}
}

=== FILE: F:\Marketing\Domain\OpenAI\OpenAIMessage.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Domain.OpenAI
{
public class OpenAIMessage
{
[JsonPropertyName("role")]
public required string Role { get; set; }
[JsonPropertyName("content")]
public required string Content { get; set; }
}
}

=== FILE: F:\Marketing\Domain\OpenAI\Prompt.cs ===

﻿namespace Domain.OpenAI
{
public class Prompt
{
public required string SystemContent { get; set; }
public required string UserContent { get; set; }
}
}

=== FILE: F:\Marketing\Infrastructure\AzureTables\TrackedLink.cs ===

﻿using System.Text.RegularExpressions;
using Azure.Data.Tables;
using Application.TrackedLinks;
namespace Infrastructure.AzureTables;
public sealed partial class TrackedLink : ITrackedLink
{
private const string TableName = "TrackedLinks";
private const string PartitionKey = "urls";
private static readonly Regex IdRegex = BuildIdRegex();
private readonly TableClient _table;
public TrackedLink(string serviceSasUrl)
{
if (string.IsNullOrWhiteSpace(serviceSasUrl))
throw new InvalidOperationException("AzureTables:ServiceSasUrl is missing.");
var serviceClient = new TableServiceClient(new Uri(serviceSasUrl));
_table = serviceClient.GetTableClient(TableName);
}
public async Task UpsertAsync(string id, string targetUrl, CancellationToken ct = default)
{
ValidateId(id);
if (string.IsNullOrWhiteSpace(targetUrl))
throw new ArgumentException("targetUrl is required.", nameof(targetUrl));
var entity = new TableEntity(PartitionKey, id)
{
["TargetUrl"] = targetUrl.Trim(),
["CreatedUtc"] = DateTime.UtcNow,
["Source"] = "marketing"
};
await _table.UpsertEntityAsync(entity, TableUpdateMode.Merge, ct);
}
private static void ValidateId(string id)
{
if (!IdRegex.IsMatch(id))
throw new ArgumentException(
"Id must be exactly between 4 and 15 alphanumeric characters (A–Z, a–z, 0–9).",
nameof(id));
}
[GeneratedRegex(@"^[A-Za-z0-9]{4,15}$", RegexOptions.Compiled)]
private static partial Regex BuildIdRegex();
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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

=== FILE: F:\Marketing\Infrastructure\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Infrastructure\obj\Release\net8.0\Infrastructure.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Infrastructure")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
[assembly: System.Reflection.AssemblyProductAttribute("Infrastructure")]
[assembly: System.Reflection.AssemblyTitleAttribute("Infrastructure")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Infrastructure\obj\Release\net8.0\Infrastructure.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Infrastructure\PixVerse\BalanceClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Infrastructure.PixVerse
{
public class BalanceClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger
) : PixVerseBase(options.Value), IBalanceClient
{
private readonly HttpClient _http = httpClient;
private readonly PixVerseOptions _opt = options.Value;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
public async Task<Operation<AccountCredits>> GetAsync(CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START PixVerse.CheckBalance", runId);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-1 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-BAL-1 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<AccountCredits>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-2 Build endpoint. Path={Path}", runId, Api.BalancePath);
var endpoint = BuildEndpoint(Api.BalancePath);
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-4 Create timeout tokens. HttpTimeout={Timeout}", runId, _opt.HttpTimeout);
using var timeoutCts = _opt.HttpTimeout > TimeSpan.Zero
? new CancellationTokenSource(_opt.HttpTimeout)
: new CancellationTokenSource();
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-5 Send request", runId);
using var res = await _http.SendAsync(req, linkedCts.Token);
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-6 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-BAL-6 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<AccountCredits>(null, $"PixVerse balance failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-7 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(linkedCts.Token);
_logger.LogDebug("[RUN {RunId}] STEP PV-BAL-7 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-8 Deserialize envelope", runId);
var env = TryDeserialize<Envelope<AccountCredits>>(json);
if (env is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-BAL-8 FAILED Envelope is null", runId);
return _error.Fail<AccountCredits>(null, "Invalid balance response (null).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-BAL-9 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-BAL-9 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<AccountCredits>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-BAL-9 FAILED Resp is null", runId);
return _error.Fail<AccountCredits>(null, "Invalid balance payload (Resp null).");
}
_logger.LogInformation("[RUN {RunId}] SUCCESS PixVerse.CheckBalance", runId);
return Operation<AccountCredits>.Success(env.Resp, env.ErrMsg);
}
catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
{
_logger.LogError(ex, "[RUN {RunId}] TIMEOUT CheckBalance after {Timeout}", runId, _opt.HttpTimeout);
return _error.Fail<AccountCredits>(ex, $"Balance check timed out after {_opt.HttpTimeout}");
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED CheckBalance", runId);
return _error.Fail<AccountCredits>(ex, "Balance check failed");
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\ImageClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace Infrastructure.PixVerse;
public sealed partial class ImageClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger
) : PixVerseBase(options.Value), IImageClient
{
private readonly HttpClient _http = httpClient;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
public async Task<Operation<ImageResult>> UploadAsync(
Stream imageStream,
string fileName,
string contentType,
CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation(
"[RUN {RunId}] START UploadImage (file). FileName={FileName} ContentType={ContentType}",
runId, fileName, contentType);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-1 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPF-1 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<ImageResult>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-2 Validate inputs", runId);
if (imageStream is null)
return _error.Business<ImageResult>("imageStream cannot be null.");
if (!imageStream.CanRead)
return _error.Business<ImageResult>("imageStream must be readable.");
if (string.IsNullOrWhiteSpace(fileName))
return _error.Business<ImageResult>("fileName cannot be null or empty.");
if (string.IsNullOrWhiteSpace(contentType))
return _error.Business<ImageResult>("contentType cannot be null or empty.");
if (!Api.AllowedImageMimeTypes.Contains(contentType))
return _error.Business<ImageResult>(
$"Unsupported contentType '{contentType}'. Allowed: image/jpeg, image/jpg, image/png, image/webp");
var ext = Path.GetExtension(fileName);
if (string.IsNullOrWhiteSpace(ext) || !Api.AllowedExtensions.Contains(ext))
return _error.Business<ImageResult>(
$"Unsupported file extension '{ext}'. Allowed: .png, .webp, .jpeg, .jpg");
if (imageStream.CanSeek)
{
const long maxBytes = 20L * 1024L * 1024L;
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-3 Validate size (seekable). MaxBytes={MaxBytes}", runId, maxBytes);
if (imageStream.Length > maxBytes)
return _error.Business<ImageResult>("Image file size must be < 20MB.");
imageStream.Position = 0;
}
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-4 Build endpoint. Path={Path}", runId, Api.UploadImagePath);
var endpoint = BuildEndpoint(Api.UploadImagePath);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-5 Build multipart form", runId);
using var form = new MultipartFormDataContent();
var fileContent = new StreamContent(imageStream);
fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
form.Add(fileContent, "image", fileName);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-6 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-7 Send request", runId);
using var res = await _http.SendAsync(req, ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-8 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPF-8 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<ImageResult>(null, $"UploadImage failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-9 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(ct);
_logger.LogDebug("[RUN {RunId}] STEP PV-UPF-9 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-10 Deserialize envelope", runId);
var env = JsonSerializer.Deserialize<Envelope<ImageResult>>(json, JsonOpts);
if (env is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPF-10 FAILED Envelope is null", runId);
return _error.Fail<ImageResult>(null, "Invalid upload response (null).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-UPF-11 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPF-11 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<ImageResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPF-11 FAILED Resp is null", runId);
return _error.Fail<ImageResult>(null, "Invalid upload payload (Resp null).");
}
_logger.LogInformation("[RUN {RunId}] SUCCESS UploadImage (file)", runId);
return Operation<ImageResult>.Success(env.Resp, env.ErrMsg);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (file)", runId);
return _error.Fail<ImageResult>(ex, "Upload image failed");
}
}
public async Task<Operation<ImageResult>> UploadAsync(string imageUrl, CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START UploadImage (url). Url={Url}", runId, imageUrl);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-1 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPU-1 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<ImageResult>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-2 Validate inputs", runId);
if (string.IsNullOrWhiteSpace(imageUrl))
return _error.Business<ImageResult>("imageUrl cannot be null or empty.");
if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
(uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
return _error.Business<ImageResult>("imageUrl must be a valid http/https absolute URL.");
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-3 Build endpoint. Path={Path}", runId, Api.UploadImagePath);
var endpoint = BuildEndpoint(Api.UploadImagePath);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-4 Build multipart form", runId);
using var form = new MultipartFormDataContent
{
{ new StringContent(imageUrl, Encoding.UTF8), "image_url" }
};
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-6 Send request", runId);
using var res = await _http.SendAsync(req, ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPU-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<ImageResult>(null, $"UploadImage (url) failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-8 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(ct);
_logger.LogDebug("[RUN {RunId}] STEP PV-UPU-8 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-9 Deserialize envelope", runId);
var env = JsonSerializer.Deserialize<Envelope<ImageResult>>(json, JsonOpts);
if (env is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPU-9 FAILED Envelope is null", runId);
return _error.Fail<ImageResult>(null, "Invalid upload response (null).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-UPU-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPU-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<ImageResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-UPU-10 FAILED Resp is null", runId);
return _error.Fail<ImageResult>(null, "Invalid upload payload (Resp null).");
}
_logger.LogInformation("[RUN {RunId}] SUCCESS UploadImage (url)", runId);
return Operation<ImageResult>.Success(env.Resp, env.ErrMsg);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (url)", runId);
return _error.Fail<ImageResult>(ex, "Upload image failed");
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\ImageToVideoClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
namespace Infrastructure.PixVerse
{
public class ImageToVideoClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger
) : PixVerseBase(options.Value), IImageToVideoClient
{
private readonly HttpClient _http = httpClient;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
public async Task<Operation<JobReceipt>> SubmitAsync(
ImageToVideo request,
CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START SubmitImageToVideo", runId);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-1 Validate request", runId);
request.Validate();
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-2 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-I2V-2 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<JobReceipt>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-3 Build endpoint. Path={Path}", runId, Api.ImageToVideoPath);
var endpoint = BuildEndpoint(Api.ImageToVideoPath);
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-4 Serialize payload", runId);
var payload = JsonSerializer.Serialize(request, JsonOpts);
_logger.LogDebug("[RUN {RunId}] STEP PV-I2V-4 PayloadLength={Length}", runId, payload?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
{
Content = new StringContent(payload, Encoding.UTF8, "application/json")
};
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-6 Send request", runId);
using var res = await _http.SendAsync(req, ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-I2V-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<JobReceipt>(null, $"SubmitImageToVideo failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-8 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(ct);
_logger.LogDebug("[RUN {RunId}] STEP PV-I2V-8 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-9 Deserialize envelope", runId);
var env = JsonSerializer.Deserialize<Envelope<I2VSubmitResp>>(json, JsonOpts);
if (env is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-I2V-9 FAILED Envelope is null", runId);
return _error.Fail<JobReceipt>(null, "Invalid ImageToVideo response (null).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-I2V-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<JobReceipt>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null || env.Resp.VideoId == 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-I2V-10 FAILED Missing Resp.video_id. VideoId={VideoId}", runId, env.Resp?.VideoId ?? 0);
return _error.Fail<JobReceipt>(null, "Invalid ImageToVideo response (missing Resp.video_id).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-I2V-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
var submitted = new JobReceipt
{
JobId = env.Resp.VideoId,
Message = env.ErrMsg
};
_logger.LogInformation("[RUN {RunId}] SUCCESS SubmitImageToVideo. JobId={JobId}", runId, submitted.JobId);
return Operation<JobReceipt>.Success(submitted, env.ErrMsg);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED SubmitImageToVideo", runId);
return _error.Fail<JobReceipt>(ex, "SubmitImageToVideo failed");
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\JobClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Infrastructure.PixVerse
{
public class JobClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger,
IVideoJobQueryClient videoJobQueryClient
) : PixVerseBase(options.Value), IJobClient
{
private readonly PixVerseOptions _opt = options.Value;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
private readonly IVideoJobQueryClient _videoJobQueryClient = videoJobQueryClient;
public async Task<Operation<JobResult>> WaitForCompletionAsync(long jobId, CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START WaitForCompletion. JobId={JobId}", runId, jobId);
if (jobId == 0)
{
_logger.LogWarning("[RUN {RunId}] WaitForCompletion aborted: jobId=0", runId);
return _error.Business<JobResult>("jobId cannot be null or empty.");
}
_logger.LogInformation(
"[RUN {RunId}] STEP PV-POLL-0 Polling settings. Attempts={Attempts} Interval={Interval}",
runId, _opt.MaxPollingAttempts, _opt.PollingInterval);
for (var i = 0; i < _opt.MaxPollingAttempts; i++)
{
ct.ThrowIfCancellationRequested();
_logger.LogInformation(
"STEP PV-4.1 - Poll {Poll}/{Max}. Getting generation status. JobId={JobId}",
i + 1, _opt.MaxPollingAttempts, jobId);
var st = await _videoJobQueryClient.GetStatusAsync(jobId, ct);
if (!st.IsSuccessful)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-POLL-1 Status call failed. Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
return st.ConvertTo<JobResult>();
}
if (st.Data is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-POLL-2 Invalid status payload (null). Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
return _error.Fail<JobResult>(null, "Invalid status payload (null).");
}
_logger.LogInformation(
"[RUN {RunId}] STEP PV-POLL-3 Status received. State={State} IsTerminal={IsTerminal} Poll={Poll}/{Max} JobId={JobId}",
runId, st.Data.State, st.Data.IsTerminal, i + 1, _opt.MaxPollingAttempts, jobId);
if (st.Data.IsTerminal)
{
if (st.Data.State == JobState.Succeeded)
{
_logger.LogInformation("[RUN {RunId}] STEP PV-POLL-4 Terminal=Succeeded. Fetching result. JobId={JobId}", runId, jobId);
return await _videoJobQueryClient.GetResultAsync(jobId, ct);
}
var msg = $"Job ended with terminal state: {st.Data.State}.";
_logger.LogWarning("[RUN {RunId}] STEP PV-POLL-4 Terminal!=Succeeded. {Message} JobId={JobId}", runId, msg, jobId);
return Operation<JobResult>.Success(new JobResult
{
RawJobId = jobId,
RawStatus = (int)st.Data.State
}, msg);
}
_logger.LogDebug("[RUN {RunId}] STEP PV-POLL-5 Delay {Delay} before next poll. Poll={Poll}/{Max} JobId={JobId}",
runId, _opt.PollingInterval, i + 1, _opt.MaxPollingAttempts, jobId);
await Task.Delay(_opt.PollingInterval, ct);
}
_logger.LogError("[RUN {RunId}] FAILED WaitForCompletion: Polling timed out. JobId={JobId}", runId, jobId);
return _error.Fail<JobResult>(null, "Polling timed out.");
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\LipSyncClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace Infrastructure.PixVerse
{
public class LipSyncClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger
) : PixVerseBase(options.Value), ILipSyncClient
{
private readonly HttpClient _http = httpClient;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
private const int MaxBodyLogChars = 4000;
private const int MaxPayloadLogChars = 4000;
public async Task<Operation<JobReceipt>> SubmitAsync(
VideoLipSync request,
CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START SubmitLipSync", runId);
_logger.LogInformation(
"[RUN {RunId}] Client settings: BaseAddress={BaseAddress} Timeout={TimeoutMs} DefaultHeaders={DefaultHeaders}",
runId,
_http.BaseAddress?.ToString() ?? "(null)",
(int)_http.Timeout.TotalMilliseconds,
DumpHeaders(_http.DefaultRequestHeaders)
);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-1 Validate request", runId);
_logger.LogInformation(
"[RUN {RunId}] Request(before normalize): SourceVideoId={SourceVideoId} VideoMediaId={VideoMediaId} AudioMediaId={AudioMediaId} SpeakerId={SpeakerId} TtsLen={TtsLen}",
runId,
request.SourceVideoId,
request.VideoMediaId,
request.AudioMediaId,
request.LipSyncTtsSpeakerId,
request.LipSyncTtsContent?.Length ?? 0
);
request.Validate();
request.Normalize();
_logger.LogInformation(
"[RUN {RunId}] Request(after normalize): SourceVideoId={SourceVideoId} VideoMediaId={VideoMediaId} AudioMediaId={AudioMediaId} SpeakerId={SpeakerId} TtsLen={TtsLen}",
runId,
request.SourceVideoId,
request.VideoMediaId,
request.AudioMediaId,
request.LipSyncTtsSpeakerId,
request.LipSyncTtsContent?.Length ?? 0
);
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-2 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-LS-2 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<JobReceipt>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-3 Build endpoint. Path={Path}", runId, Api.LipSyncPath);
var endpoint = BuildEndpoint(Api.LipSyncPath);
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-4 Serialize payload", runId);
var payload = JsonSerializer.Serialize(request, JsonOpts);
_logger.LogInformation(
"[RUN {RunId}] STEP PV-LS-4 PayloadLength={Length}",
runId,
payload?.Length ?? 0
);
_logger.LogDebug(
"[RUN {RunId}] STEP PV-LS-4 Payload(truncated)={Payload}",
runId,
Truncate(payload, MaxPayloadLogChars)
);
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
{
Content = new StringContent(payload, Encoding.UTF8, "application/json")
};
req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
{
CharSet = "utf-8"
};
_logger.LogDebug("[RUN {RunId}] STEP PV-LS-5 Request headers(before auth)={Headers}", runId, DumpHeaders(req.Headers));
ApplyAuth(req);
var traceId = req.Headers.TryGetValues(Api.TraceIdHeader, out var traceVals)
? traceVals.FirstOrDefault()
: null;
_logger.LogInformation(
"[RUN {RunId}] STEP PV-LS-5 Auth applied. TraceId={TraceId} Headers(after auth)={Headers}",
runId,
traceId ?? "(null)",
DumpHeaders(req.Headers, redactApiKey: true)
);
_logger.LogDebug(
"[RUN {RunId}] STEP PV-LS-5 Content headers={Headers}",
runId,
DumpHeaders(req.Content.Headers)
);
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-6 Send request", runId);
HttpResponseMessage? res = null;
var startedAt = DateTimeOffset.UtcNow;
try
{
res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
}
catch (TaskCanceledException tex) when (!ct.IsCancellationRequested)
{
_logger.LogError(
tex,
"[RUN {RunId}] STEP PV-LS-6 FAILED SendAsync TIMEOUT. ElapsedMs={ElapsedMs} Endpoint={Endpoint} TraceId={TraceId}",
runId,
(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds,
endpoint,
traceId ?? "(null)"
);
return _error.Fail<JobReceipt>(tex, "SubmitLipSync failed (timeout).");
}
catch (OperationCanceledException ocex) when (ct.IsCancellationRequested)
{
_logger.LogWarning(
ocex,
"[RUN {RunId}] STEP PV-LS-6 CANCELED by caller. ElapsedMs={ElapsedMs} Endpoint={Endpoint} TraceId={TraceId}",
runId,
(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds,
endpoint,
traceId ?? "(null)"
);
return _error.Fail<JobReceipt>(ocex, "SubmitLipSync canceled.");
}
_logger.LogInformation(
"[RUN {RunId}] STEP PV-LS-7 Response received. StatusCode={StatusCode} Reason={Reason} ElapsedMs={ElapsedMs}",
runId,
(int)res.StatusCode,
res.ReasonPhrase ?? "(null)",
(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds
);
_logger.LogDebug(
"[RUN {RunId}] STEP PV-LS-7 Response headers={Headers}",
runId,
DumpHeaders(res.Headers)
);
if (res.Content != null)
{
_logger.LogDebug(
"[RUN {RunId}] STEP PV-LS-7 Response content headers={Headers}",
runId,
DumpHeaders(res.Content.Headers)
);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-8 Read response body (always)", runId);
var body = res.Content is null ? string.Empty : await res.Content.ReadAsStringAsync(ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-8 BodyLength={Length}", runId, body?.Length ?? 0);
_logger.LogDebug("[RUN {RunId}] STEP PV-LS-8 Body(truncated)={Body}", runId, Truncate(body, MaxBodyLogChars));
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning(
"[RUN {RunId}] STEP PV-LS-9 FAILED Non-success status. StatusCode={StatusCode} TraceId={TraceId}",
runId,
(int)res.StatusCode,
traceId ?? "(null)"
);
var envErr = TryDeserialize<Envelope<SubmitResp>>(body ?? string.Empty);
if (envErr is not null)
{
_logger.LogWarning(
"[RUN {RunId}] STEP PV-LS-9 Parsed error envelope. ErrCode={ErrCode} ErrMsg={ErrMsg}",
runId,
envErr.ErrCode,
envErr.ErrMsg
);
}
else
{
_logger.LogWarning("[RUN {RunId}] STEP PV-LS-9 Could not parse error envelope (raw body logged).", runId);
}
var msg =
$"SubmitLipSync failed. HTTP {(int)res.StatusCode}. " +
$"Reason={res.ReasonPhrase}. TraceId={traceId}. " +
$"Body(truncated)={Truncate(body, 500)}";
return _error.Fail<JobReceipt>(null, msg);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-10 Deserialize envelope", runId);
var env = JsonSerializer.Deserialize<Envelope<SubmitResp>>(body, JsonOpts);
if (env is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-LS-10 FAILED Envelope is null", runId);
return _error.Fail<JobReceipt>(null, "Invalid LipSync response (null envelope).");
}
_logger.LogInformation(
"[RUN {RunId}] STEP PV-LS-11 Validate envelope. ErrCode={ErrCode} ErrMsg={ErrMsg}",
runId,
env.ErrCode,
env.ErrMsg
);
if (env.ErrCode != 0)
{
_logger.LogWarning(
"[RUN {RunId}] STEP PV-LS-11 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}",
runId,
env.ErrCode,
env.ErrMsg
);
return _error.Fail<JobReceipt>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null || env.Resp.VideoId == 0)
{
_logger.LogWarning(
"[RUN {RunId}] STEP PV-LS-11 FAILED Missing Resp.video_id. VideoId={VideoId}",
runId,
env.Resp?.VideoId ?? 0
);
return _error.Fail<JobReceipt>(null, "Invalid LipSync response (missing Resp.video_id).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-LS-12 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
var submitted = new JobReceipt
{
JobId = env.Resp.VideoId,
Message = env.ErrMsg
};
_logger.LogInformation(
"[RUN {RunId}] SUCCESS SubmitLipSync. JobId={JobId} TraceId={TraceId}",
runId,
submitted.JobId,
traceId ?? "(null)"
);
return Operation<JobReceipt>.Success(submitted, env.ErrMsg);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED SubmitLipSync (exception)", runId);
return _error.Fail<JobReceipt>(ex, "SubmitLipSync failed");
}
}
private static string Truncate(string? value, int maxChars)
{
if (string.IsNullOrEmpty(value)) return string.Empty;
if (value.Length <= maxChars) return value;
return value.Substring(0, maxChars) + $"... (truncated, len={value.Length})";
}
private static string DumpHeaders(HttpHeaders headers, bool redactApiKey = false)
{
try
{
var sb = new StringBuilder();
foreach (var h in headers)
{
var key = h.Key;
if (redactApiKey && key.Equals(Api.ApiKeyHeader, StringComparison.OrdinalIgnoreCase))
{
sb.Append(key).Append("=").Append("[REDACTED]").Append("; ");
continue;
}
sb.Append(key).Append("=").Append(string.Join(",", h.Value)).Append("; ");
}
return sb.Length == 0 ? "(none)" : sb.ToString();
}
catch
{
return "(failed-to-dump-headers)";
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\PixVerseBase.cs ===

﻿using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
namespace Infrastructure.PixVerse
{
public class PixVerseBase(PixVerseOptions opt)
{
public static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
{
DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
};
internal static string NewRunId() => Guid.NewGuid().ToString("N")[..8];
public bool TryValidateConfig(out string error)
{
if (string.IsNullOrWhiteSpace(opt.BaseUrl))
{
error = "PixVerse BaseUrl is not configured.";
return false;
}
if (string.IsNullOrWhiteSpace(opt.ApiKey))
{
error = "PixVerse ApiKey is not configured.";
return false;
}
error = string.Empty;
return true;
}
public Uri BuildEndpoint(string pathOrPathWithId)
{
var baseUri = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
return new Uri(baseUri, pathOrPathWithId.TrimStart('/'));
}
public void ApplyAuth(HttpRequestMessage req)
{
req.Headers.Remove(Api.ApiKeyHeader);
req.Headers.Add(Api.ApiKeyHeader, opt.ApiKey);
if (!req.Headers.Contains(Api.TraceIdHeader))
req.Headers.Add(Api.TraceIdHeader, Guid.NewGuid().ToString());
req.Headers.Accept.Clear();
req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}
public static T? TryDeserialize<T>(string json)
{
try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
catch { return default; }
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\TextToVideoClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
namespace Infrastructure.PixVerse
{
public class TextToVideoClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger
) : PixVerseBase(options.Value), ITextToVideoClient
{
private readonly HttpClient _http = httpClient;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
public async Task<Operation<JobReceipt>> SubmitTAsync(
TextToVideo request,
CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START SubmitTextToVideo", runId);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-1 Validate request", runId);
request.Validate();
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-2 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-T2V-2 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<JobReceipt>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-3 Build endpoint. Path={Path}", runId, Api.TextToVideoPath);
var endpoint = BuildEndpoint(Api.TextToVideoPath);
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-4 Serialize payload", runId);
var payload = JsonSerializer.Serialize(request, JsonOpts);
_logger.LogDebug("[RUN {RunId}] STEP PV-T2V-4 PayloadLength={Length}", runId, payload?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
{
Content = new StringContent(payload, Encoding.UTF8, "application/json")
};
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-6 Send request", runId);
using var res = await _http.SendAsync(req, ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-T2V-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<JobReceipt>(null, $"Submit failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-8 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(ct);
_logger.LogDebug("[RUN {RunId}] STEP PV-T2V-8 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-9 Deserialize envelope", runId);
var env = JsonSerializer.Deserialize<Envelope<SubmitResp>>(json, JsonOpts);
if (env is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-T2V-9 FAILED Envelope is null", runId);
return _error.Fail<JobReceipt>(null, "Invalid submit response (null).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-T2V-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<JobReceipt>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null || env.Resp.VideoId == 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-T2V-10 FAILED Missing Resp.video_id. VideoId={VideoId}", runId, env.Resp?.VideoId ?? 0);
return _error.Fail<JobReceipt>(null, "Invalid submit response (missing Resp.video_id).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-T2V-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
var submitted = new JobReceipt
{
JobId = env.Resp.VideoId,
Message = env.ErrMsg
};
_logger.LogInformation("[RUN {RunId}] SUCCESS SubmitTextToVideo. JobId={JobId}", runId, submitted.JobId);
return Operation<JobReceipt>.Success(submitted, env.ErrMsg);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTextToVideo", runId);
return _error.Fail<JobReceipt>(ex, "Submit failed");
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\TransitionClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
namespace Infrastructure.PixVerse
{
public class TransitionClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger
) : PixVerseBase(options.Value), ITransitionClient
{
private readonly HttpClient _http = httpClient;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
public async Task<Operation<JobReceipt>> SubmitAsync(
VideoTransition request,
CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START SubmitTransition", runId);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-1 Validate request", runId);
request.Validate();
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-2 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-TR-2 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<JobReceipt>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-3 Build endpoint. Path={Path}", runId, Api.TransitionPath);
var endpoint = BuildEndpoint(Api.TransitionPath);
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-4 Serialize payload", runId);
var payload = JsonSerializer.Serialize(request, JsonOpts);
_logger.LogDebug("[RUN {RunId}] STEP PV-TR-4 PayloadLength={Length}", runId, payload?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
{
Content = new StringContent(payload, Encoding.UTF8, "application/json")
};
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-6 Send request", runId);
using var res = await _http.SendAsync(req, ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-TR-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<JobReceipt>(null, $"SubmitTransition failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-8 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(ct);
_logger.LogDebug("[RUN {RunId}] STEP PV-TR-8 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-9 Deserialize envelope", runId);
var env = JsonSerializer.Deserialize<Envelope<I2VSubmitResp>>(json, JsonOpts);
if (env is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-TR-9 FAILED Envelope is null", runId);
return _error.Fail<JobReceipt>(null, "Invalid Transition response (null).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-TR-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<JobReceipt>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null || env.Resp.VideoId == 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-TR-10 FAILED Missing Resp.video_id. VideoId={VideoId}", runId, env.Resp?.VideoId ?? 0);
return _error.Fail<JobReceipt>(null, "Invalid Transition response (missing Resp.video_id).");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-TR-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
var submitted = new JobReceipt
{
JobId = env.Resp.VideoId,
Message = env.ErrMsg
};
_logger.LogInformation("[RUN {RunId}] SUCCESS SubmitTransition. JobId={JobId}", runId, submitted.JobId);
return Operation<JobReceipt>.Success(submitted, env.ErrMsg);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTransition", runId);
return _error.Fail<JobReceipt>(ex, "SubmitTransition failed");
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\VideoClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using File = System.IO.File;
namespace Infrastructure.PixVerse
{
public class VideoClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger,
IVideoJobQueryClient videoJobQueryClient
) : PixVerseBase(options.Value), IVideoClient
{
private readonly HttpClient _http = httpClient;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
private readonly IVideoJobQueryClient _videoJobQueryClient = videoJobQueryClient;
public async Task<Operation<FileInfo>> DownloadAsync(
long jobId,
string destinationFilePath,
int videoIndex = 0,
CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation(
"[RUN {RunId}] START PixVerse.DownloadVideo jobId={JobId} videoIndex={VideoIndex} dest={Dest}",
runId, jobId, videoIndex, destinationFilePath);
try
{
if (jobId == 0)
return _error.Business<FileInfo>("jobId cannot be null or empty.");
if (string.IsNullOrWhiteSpace(destinationFilePath))
return _error.Business<FileInfo>("destinationFilePath cannot be null or empty.");
if (videoIndex < 0)
return _error.Business<FileInfo>("videoIndex cannot be negative.");
_logger.LogInformation("[RUN {RunId}] STEP 1: Fetch generation result", runId);
var resOp = await _videoJobQueryClient.GetResultAsync(jobId, ct);
if (!resOp.IsSuccessful|| resOp.Data is null)
return _error.Fail<FileInfo>(null, $"Cannot download video because generation result is not available. jobId={jobId}");
var result = resOp.Data;
if (result.VideoUrls is null || result.VideoUrls.Count == 0)
return _error.Fail<FileInfo>(null, $"No video URLs found for jobId={jobId}");
if (videoIndex >= result.VideoUrls.Count)
return _error.Fail<FileInfo>(
null,
$"videoIndex out of range. videoIndex={videoIndex}, available={result.VideoUrls.Count}, jobId={jobId}");
var videoUrl = result.VideoUrls[videoIndex];
if (string.IsNullOrWhiteSpace(videoUrl))
return _error.Fail<FileInfo>(null, $"Video URL is empty for jobId={jobId}, videoIndex={videoIndex}");
_logger.LogInformation("[RUN {RunId}] STEP 2: Selected videoUrl={VideoUrl}", runId, videoUrl);
string finalPath;
if (Directory.Exists(destinationFilePath) ||
destinationFilePath.EndsWith(Path.DirectorySeparatorChar) ||
destinationFilePath.EndsWith(Path.AltDirectorySeparatorChar))
{
var fileName = $"{jobId}_{videoIndex}.mp4";
finalPath = Path.Combine(destinationFilePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), fileName);
}
else
{
finalPath = destinationFilePath;
if (string.IsNullOrEmpty(Path.GetExtension(finalPath)))
finalPath += ".mp4";
}
var dir = Path.GetDirectoryName(finalPath);
if (!string.IsNullOrWhiteSpace(dir))
Directory.CreateDirectory(dir);
var tmpPath = finalPath + ".download.tmp";
_logger.LogInformation("[RUN {RunId}] STEP 3: Downloading to temp file tmp={Tmp}", runId, tmpPath);
using (var req = new HttpRequestMessage(HttpMethod.Get, videoUrl))
{
using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
if (!resp.IsSuccessStatusCode)
return _error.Fail<FileInfo>(
null,
$"PixVerse video download failed. HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
await using var httpStream = await resp.Content.ReadAsStreamAsync(ct);
await using var fileStream = new FileStream(
tmpPath,
FileMode.Create,
FileAccess.Write,
FileShare.None,
bufferSize: 81920,
useAsync: true);
await httpStream.CopyToAsync(fileStream, ct);
await fileStream.FlushAsync(ct);
}
_logger.LogInformation("[RUN {RunId}] STEP 4: Moving temp to final final={Final}", runId, finalPath);
if (System.IO.File.Exists(finalPath))
File.Delete(finalPath);
File.Move(tmpPath, finalPath);
var fi = new FileInfo(finalPath);
_logger.LogInformation(
"[RUN {RunId}] SUCCESS PixVerse.DownloadVideo saved={Path} bytes={Bytes}",
runId, fi.FullName, fi.Length);
return Operation<FileInfo>.Success(fi);
}
catch (OperationCanceledException)
{
_logger.LogWarning("[RUN {RunId}] CANCELED PixVerse.DownloadVideo", runId);
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED PixVerse.DownloadVideo", runId);
try
{
var tmp = destinationFilePath + ".download.tmp";
if (File.Exists(tmp))
File.Delete(tmp);
}
catch {  }
return _error.Fail<FileInfo>(ex, $"PixVerse video download failed for jobId={jobId}");
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\VideoJobQueryClient.cs ===

﻿using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Infrastructure.PixVerse
{
public class VideoJobQueryClient(
HttpClient httpClient,
IOptions<PixVerseOptions> options,
IErrorHandler errorHandler,
ILogger<ImageClient> logger
) : PixVerseBase(options.Value), IVideoJobQueryClient
{
private readonly HttpClient _http = httpClient;
private readonly PixVerseOptions _opt = options.Value;
private readonly IErrorHandler _error = errorHandler;
private readonly ILogger<ImageClient> _logger = logger;
public async Task<Operation<JobStatus>> GetStatusAsync(long jobId, CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START GetGenerationStatus. JobId={JobId}", runId, jobId);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-1 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-ST-1 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<JobStatus>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-2 Build endpoint. JobId={JobId}", runId, jobId);
var endpoint = BuildEndpoint(Api.StatusPath + Uri.EscapeDataString(jobId.ToString()));
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-4 Send request", runId);
using var res = await _http.SendAsync(req, ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-ST-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<JobStatus>(null, $"Status failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-6 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(ct);
_logger.LogDebug("[RUN {RunId}] STEP PV-ST-6 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-7 Try deserialize envelope", runId);
var env = TryDeserialize<Envelope<JobStatus>>(json);
if (env is not null)
{
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<JobStatus>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED Resp is null", runId);
return _error.Fail<JobStatus>(null, "Invalid status payload (Resp null).");
}
_logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (envelope)", runId);
return Operation<JobStatus>.Success(env.Resp, env.ErrMsg);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-ST-9 Envelope parse failed. Fallback to raw status model", runId);
var status = JsonSerializer.Deserialize<JobStatus>(json, JsonOpts);
if (status is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-ST-9 FAILED Status model is null", runId);
return _error.Fail<JobStatus>(null, "Invalid status payload");
}
_logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (raw)", runId);
return Operation<JobStatus>.Success(status);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationStatus. JobId={JobId}", runId, jobId);
return _error.Fail<JobStatus>(ex, "Status check failed");
}
}
public async Task<Operation<JobResult>> GetResultAsync(long jobId, CancellationToken ct = default)
{
var runId = NewRunId();
_logger.LogInformation("[RUN {RunId}] START GetGenerationResult. JobId={JobId}", runId, jobId);
try
{
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-1 Validate config", runId);
if (!TryValidateConfig(out var configError))
{
_logger.LogWarning("[RUN {RunId}] STEP PV-RS-1 FAILED Config invalid: {Error}", runId, configError);
return _error.Fail<JobResult>(null, configError);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-2 Build endpoint. JobId={JobId}", runId, jobId);
var endpoint = BuildEndpoint(Api.ResultPath + Uri.EscapeDataString(jobId.ToString()));
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
ApplyAuth(req);
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-4 Send request", runId);
using var res = await _http.SendAsync(req, ct);
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
if (!res.IsSuccessStatusCode)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-RS-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
return _error.Fail<JobResult>(null, $"Result failed. HTTP {(int)res.StatusCode}");
}
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-6 Read response body", runId);
var json = await res.Content.ReadAsStringAsync(ct);
_logger.LogDebug("[RUN {RunId}] STEP PV-RS-6 BodyLength={Length}", runId, json?.Length ?? 0);
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-7 Try deserialize envelope", runId);
var env = TryDeserialize<Envelope<JobResult>>(json);
if (env is not null)
{
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);
if (env.ErrCode != 0)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
return _error.Fail<JobResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
}
if (env.Resp is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED Resp is null", runId);
return _error.Fail<JobResult>(null, "Invalid result payload (Resp null).");
}
_logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (envelope)", runId);
return Operation<JobResult>.Success(env.Resp, env.ErrMsg);
}
_logger.LogInformation("[RUN {RunId}] STEP PV-RS-9 Envelope parse failed. Fallback to raw result model", runId);
var result = JsonSerializer.Deserialize<JobResult>(json, JsonOpts);
if (result is null)
{
_logger.LogWarning("[RUN {RunId}] STEP PV-RS-9 FAILED Result model is null", runId);
return _error.Fail<JobResult>(null, "Invalid result payload");
}
_logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (raw)", runId);
return Operation<JobResult>.Success(result);
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationResult. JobId={JobId}", runId, jobId);
return _error.Fail<JobResult>(ex, "Result check failed");
}
}
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\Constants\Api.cs ===

﻿namespace Infrastructure.PixVerse.Constants
{
public class Api
{
public static IReadOnlySet<string> AllowedImageMimeTypes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
"image/jpeg",
"image/jpg",
"image/png",
"image/webp"
};
public static IReadOnlySet<string> AllowedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
".jpeg",
".jpg",
".png",
".webp"
};
public const string BalancePath  = "/openapi/v2/account/balance";
public const string TextToVideoPath = "/openapi/v2/video/text/generate";
public const string ImageToVideoPath = "/openapi/v2/video/img/generate";
public const string TransitionPath = "/openapi/v2/video/transition/generate";
public const string StatusPath = "/openapi/v2/video/status/";
public const string ResultPath = "/openapi/v2/video/result/";
public const string UploadImagePath = "/openapi/v2/image/upload";
public const string LipSyncPath = "/openapi/v2/video/lip_sync/generate";
public const string ApiKeyHeader = "API-KEY";
public const string TraceIdHeader = "Ai-trace-id";
}
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\Result\Envelope.cs ===

﻿using System.Text.Json.Serialization;
namespace Infrastructure.PixVerse.Result;
internal sealed class Envelope<T>
{
[JsonPropertyName("ErrCode")]
public int ErrCode { get; init; }
[JsonPropertyName("ErrMsg")]
public string? ErrMsg { get; init; }
[JsonPropertyName("Resp")]
public T? Resp { get; init; }
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\Result\I2VSubmitResp.cs ===

﻿using System.Text.Json.Serialization;
namespace Infrastructure.PixVerse.Result;
internal class I2VSubmitResp
{
[JsonPropertyName("video_id")]
public long VideoId { get; init; }
}

=== FILE: F:\Marketing\Infrastructure\PixVerse\Result\SubmitResp.cs ===

﻿using System.Text.Json.Serialization;
namespace Infrastructure.PixVerse.Result;
internal sealed class SubmitResp : I2VSubmitResp
{
[JsonPropertyName("credits")]
public int Credits { get; init; }
}

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
internal static string CreationFail {
get {
return ResourceManager.GetString("CreationFail", resourceCulture);
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
public abstract class CreateRepository<T>(
IUnitOfWork unitOfWork,
IErrorHandler errorHandler)
: RepositoryCreate<T>(unitOfWork), ICreate<T> where T : class, IEntity
{
private readonly IErrorHandler ErrorHandler = errorHandler;
public async Task<Operation<bool>> CreateEntity(T entity)
{
try
{
await Create(entity);
var success = CreateLabels.CreationSuccess;
var message = string.Format(success, typeof(T).Name);
await unitOfWork.CommitAsync();
return Operation<bool>.Success(true, message);
}
catch (Exception ex)
{
var fail = CreateLabels.CreationFail;
var message = string.Format(fail, typeof(T).Name);
return ErrorHandler.Fail<bool>(ex, message);
}
}
public async Task<Operation<bool>> CreateEntities(List<T> entities)
{
await CreateRange(entities);
var success = CreateLabels.CreationSuccess;
var message = string.Format(success, typeof(T).Name);
await unitOfWork.CommitAsync();
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
internal static string DeletionFailed {
get {
return ResourceManager.GetString("DeletionFailed", resourceCulture);
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
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;
using Persistence.Repositories;
namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
public abstract class DeleteRepository<T>(IUnitOfWork unitOfWork,
IErrorHandler errorHandler)
: RepositoryDelete<T>(unitOfWork), IDelete<T> where T : class, IEntity
{
private readonly IErrorHandler ErrorHandler = errorHandler;
public async Task<Operation<bool>> DeleteEntity(string id)
{
try
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
await unitOfWork.CommitAsync();
return Operation<bool>.Success(true, message);
}
catch (Exception ex)
{
var fail = DeleteLabels.DeletionFailed;
var message = string.Format(fail, typeof(T).Name);
return ErrorHandler.Fail<bool>(ex, message);
}
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
internal static string UpdationFail {
get {
return ResourceManager.GetString("UpdationFail", resourceCulture);
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
using Infrastructure.Repositories.Abstract.CRUD.Delete;
using Infrastructure.Result;
using Persistence.Context.Interface;
using Persistence.Repositories;
namespace Infrastructure.Repositories.Abstract.CRUD.Update
{
public abstract class UpdateRepository<T>(
IUnitOfWork unitOfWork,
IErrorHandler errorHandler)
: RepositoryUpdate<T>(unitOfWork), IUpdate<T>
where T : class, IEntity
{
private readonly IErrorHandler ErrorHandler = errorHandler;
public async Task<Operation<bool>> UpdateEntity(T modify)
{
try
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
await unitOfWork.CommitAsync();
return Operation<bool>.Success(true, message);
}
catch (Exception ex)
{
var fail = UpdateLabels.UpdationFail;
var message = string.Format(fail, typeof(T).Name);
return ErrorHandler.Fail<bool>(ex, message);
}
}
public abstract T ApplyUpdates(T modified, T unmodified);
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

=== FILE: F:\Marketing\Infrastructure\Result\SerilogErrorLogger.cs ===

﻿using Application.Result;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Result
{
public sealed class SerilogErrorLogger(ILogger<SerilogErrorLogger> logger) : IErrorLogger
{
public Task LogAsync(Exception ex, CancellationToken cancellationToken = default)
{
logger.LogError(ex, "Unhandled exception captured by IErrorLogger");
return Task.CompletedTask;
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

=== FILE: F:\Marketing\Marketing.Services.Test\AutoItRunnerTests.cs ===

﻿using System;
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
Directory.SetCurrentDirectory(_workDir);
}
[TearDown]
public void TearDown()
{
try
{
Directory.SetCurrentDirectory(Path.GetTempPath());
}
catch {  }
try
{
if (Directory.Exists(_workDir))
Directory.Delete(_workDir, recursive: true);
}
catch {  }
}
private static AppConfig BuildConfig(string outFolder, string? interpreterPath)
{
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
[Test]
public void GivenTemplateMissing_WhenRunAsync_ThenThrowsFileNotFoundException()
{
var loggerMock = CreateLoggerMock();
var config = BuildConfig(_outDir, interpreterPath: @"C:\missing\autoit3.exe");
var sut = new AutoItRunner(config, loggerMock.Object);
Assert.ThrowsAsync<FileNotFoundException>(async () =>
await sut.RunAsync(
timeout: TimeSpan.FromSeconds(1),
imagePath: @"C:\file\document.pdf",
useAutoItInterpreter: true,
cancellationToken: CancellationToken.None));
}
[Test]
public async Task GivenTemplatePresentAndInterpreterMissing_WhenRunAsync_ThenCreatesAutoItLogAndWritesTempScriptAndThrows()
{
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
var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
await sut.RunAsync(
timeout: TimeSpan.FromSeconds(1),
imagePath: @"C:\file\document.pdf",
useAutoItInterpreter: true,
cancellationToken: CancellationToken.None));
Assert.That(ex, Is.Not.Null);
var autoItLogDir = Path.Combine(_outDir, "AutoItLog");
Assert.That(Directory.Exists(autoItLogDir), Is.True, "Expected AutoItLog directory to be created.");
var afterScripts = SnapshotTempScripts();
var scriptPath = FindNewTempScript(beforeScripts, afterScripts);
Assert.That(File.Exists(scriptPath), Is.True, "Expected generated temp .au3 script to exist.");
var scriptContent = File.ReadAllText(scriptPath);
Assert.That(scriptContent, Does.Contain(@"C:\file\document.pdf"), "Expected __FILE_TO_UPLOAD__ replacement.");
Assert.That(scriptContent, Does.Contain(autoItLogDir), "Expected __AUTOIT_LOG_FILE__ replacement.");
}
[Test]
public void GivenTemplatePresentAndDirectMode_WhenRunAsync_ThenFailsToStartProcess()
{
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
Assert.CatchAsync(async () =>
await sut.RunAsync(
timeout: TimeSpan.FromSeconds(2),
imagePath: @"C:\file\document.pdf",
useAutoItInterpreter: false,
cancellationToken: CancellationToken.None));
}
[Test]
public void GivenAutoItRunnerResultContract_WhenConstructed_ThenRequiredFieldsAreEnforced()
{
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
[Test]
public void GivenNullImagePath_WhenRunAsync_ThenThrows()
{
CreateTemplateFile(_workDir,
content:
@"Global Const $LOG_FILE = ""__AUTOIT_LOG_FILE__""
Global Const $FILE_TO_UPLOAD = ""__FILE_TO_UPLOAD__""
");
var loggerMock = CreateLoggerMock();
var config = BuildConfig(_outDir, interpreterPath: Path.Combine(_workDir, "missing_autoit3.exe"));
var sut = new AutoItRunner(config, loggerMock.Object);
Assert.ThrowsAsync<FileNotFoundException>(async () =>
await sut.RunAsync(
timeout: TimeSpan.FromSeconds(1),
imagePath: null!,
useAutoItInterpreter: true,
cancellationToken: CancellationToken.None));
}
}
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

=== FILE: F:\Marketing\Marketing.Services.Test\CaptureSnapshotTests.cs ===

﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;
using Services.Check;
namespace Marketing.Services.Test
{
[TestFixture]
public sealed class CaptureSnapshotTests
{
private Mock<IWebDriver> _driver = null!;
private Mock<ILogger<CaptureSnapshot>> _logger = null!;
[SetUp]
public void SetUp()
{
_driver = new Mock<IWebDriver>(MockBehavior.Strict);
_logger = new Mock<ILogger<CaptureSnapshot>>(MockBehavior.Loose);
}
[TearDown]
public void TearDown()
{
_driver.VerifyNoOtherCalls();
}
[Test]
public async Task Given_ValidExecutionFolderAndStage_When_CaptureArtifactsAsync_Then_ReturnsTimestampAndWritesHtmlAndPng()
{
var executionFolder = CreateUniqueTempFolderPath();
var stage = "BeforeSend";
var pageSource = "<html><body>ok</body></html>";
_driver.Setup(d => d.PageSource).Returns(pageSource);
var takesScreenshot = _driver.As<ITakesScreenshot>();
takesScreenshot.Setup(t => t.GetScreenshot())
.Returns(CreateMinimalPngScreenshot());
var sut = new CaptureSnapshot(_driver.Object, _logger.Object);
var timestamp = await sut.CaptureArtifactsAsync(executionFolder, stage);
Assert.That(timestamp, Is.Not.Null.And.Not.Empty);
Assert.That(IsTimestampFormat(timestamp), Is.True, "Timestamp must be yyyyMMdd_HHmmss.");
var htmlPath = Path.Combine(executionFolder, $"{timestamp}.html");
var pngPath = Path.Combine(executionFolder, $"{timestamp}.png");
Assert.That(Directory.Exists(executionFolder), Is.True, "Execution folder must be created.");
Assert.That(File.Exists(htmlPath), Is.True, "HTML artifact must be created.");
Assert.That(File.Exists(pngPath), Is.True, "PNG artifact must be created.");
var html = await File.ReadAllTextAsync(htmlPath);
Assert.That(html, Is.EqualTo(pageSource), "HTML file must contain driver.PageSource.");
_driver.VerifyGet(d => d.PageSource, Times.Once);
takesScreenshot.Verify(t => t.GetScreenshot(), Times.Once);
}
[Test]
public async Task Given_StageIsNullOrWhitespace_When_CaptureArtifactsAsync_Then_UsesUnknownStageInLoggingAndStillCreatesArtifacts()
{
var executionFolder = CreateUniqueTempFolderPath();
string stage = "   ";
var pageSource = "<html>stage-default</html>";
_driver.Setup(d => d.PageSource).Returns(pageSource);
var takesScreenshot = _driver.As<ITakesScreenshot>();
takesScreenshot.Setup(t => t.GetScreenshot())
.Returns(CreateMinimalPngScreenshot());
var sut = new CaptureSnapshot(_driver.Object, _logger.Object);
var timestamp = await sut.CaptureArtifactsAsync(executionFolder, stage);
var htmlPath = Path.Combine(executionFolder, $"{timestamp}.html");
var pngPath = Path.Combine(executionFolder, $"{timestamp}.png");
Assert.That(File.Exists(htmlPath), Is.True);
Assert.That(File.Exists(pngPath), Is.True);
_logger.VerifyLogContains(
Microsoft.Extensions.Logging.LogLevel.Information,
"Capturing artifacts for stage",
Times.AtLeastOnce(),
mustContain: "UnknownStage");
_driver.VerifyGet(d => d.PageSource, Times.Once);
takesScreenshot.Verify(t => t.GetScreenshot(), Times.Once);
}
[Test]
public void Given_ExecutionFolderIsNull_When_CaptureArtifactsAsync_Then_ThrowsArgumentNullException()
{
var sut = new CaptureSnapshot(_driver.Object, _logger.Object);
Assert.ThrowsAsync<ArgumentNullException>(() => sut.CaptureArtifactsAsync(null!, "AnyStage"));
}
[Test]
public void Given_DriverDoesNotImplementITakesScreenshot_When_CaptureArtifactsAsync_Then_ThrowsInvalidCastException()
{
var executionFolder = CreateUniqueTempFolderPath();
_driver.Setup(d => d.PageSource).Returns("<html/>");
var sut = new CaptureSnapshot(_driver.Object, _logger.Object);
Assert.ThrowsAsync<InvalidCastException>(() => sut.CaptureArtifactsAsync(executionFolder, "Stage"));
_driver.VerifyGet(d => d.PageSource, Times.Once);
}
[Test]
public void Given_ExecutionFolderPointsToInvalidPath_When_CaptureArtifactsAsync_Then_Throws()
{
var executionFolder = Path.Combine(Path.GetTempPath(), "bad*folder");
_driver.Setup(d => d.PageSource).Returns("<html/>");
var takesScreenshot = _driver.As<ITakesScreenshot>();
takesScreenshot.Setup(t => t.GetScreenshot())
.Returns(CreateMinimalPngScreenshot());
var sut = new CaptureSnapshot(_driver.Object, _logger.Object);
Assert.ThrowsAsync<IOException>(() => sut.CaptureArtifactsAsync(executionFolder, "Stage"));
}
private static string CreateUniqueTempFolderPath()
{
var root = Path.Combine(Path.GetTempPath(), "CaptureSnapshotTests");
return Path.Combine(root, Guid.NewGuid().ToString("N"));
}
private static bool IsTimestampFormat(string timestamp)
{
return Regex.IsMatch(timestamp, @"^\d{8}_\d{6}$");
}
private static Screenshot CreateMinimalPngScreenshot()
{
const string base64Png =
"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO3Z0XkAAAAASUVORK5CYII=";
return new Screenshot(base64Png);
}
}
internal static class LoggerMoqExtensions
{
public static void VerifyLogContains<T>(
this Mock<ILogger<T>> logger,
Microsoft.Extensions.Logging.LogLevel level,
string contains,
Times times,
string? mustContain = null)
{
logger.Verify(
x => x.Log(
level,
It.IsAny<EventId>(),
It.Is<It.IsAnyType>((state, _) =>
state != null &&
state.ToString()!.Contains(contains, StringComparison.OrdinalIgnoreCase) &&
(mustContain == null ||
state.ToString()!.Contains(mustContain, StringComparison.OrdinalIgnoreCase))
),
It.IsAny<Exception>(),
It.IsAny<Func<It.IsAnyType, Exception?, string>>()
),
times
);
}
}
}

=== FILE: F:\Marketing\Marketing.Services.Test\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Marketing.Services.Test\obj\Debug\net8.0\Marketing.Services.Test.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Marketing.Services.Test")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
[assembly: System.Reflection.AssemblyProductAttribute("Marketing.Services.Test")]
[assembly: System.Reflection.AssemblyTitleAttribute("Marketing.Services.Test")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Marketing.Services.Test\obj\Debug\net8.0\Marketing.Services.Test.GlobalUsings.g.cs ===

global using global::NUnit.Framework;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Marketing.Services.Test\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Marketing.Services.Test\obj\Release\net8.0\Marketing.Services.Test.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Marketing.Services.Test")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+686d4a9fd23366c79861334aafe6e4c8aa0b6377")]
[assembly: System.Reflection.AssemblyProductAttribute("Marketing.Services.Test")]
[assembly: System.Reflection.AssemblyTitleAttribute("Marketing.Services.Test")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Marketing.Services.Test\obj\Release\net8.0\Marketing.Services.Test.GlobalUsings.g.cs ===

global using global::NUnit.Framework;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

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

=== FILE: F:\Marketing\Marketing.Tests\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Marketing.Tests\obj\Debug\net8.0\Marketing.Tests.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
[assembly: System.Reflection.AssemblyProductAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyTitleAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Marketing.Tests\obj\Debug\net8.0\Marketing.Tests.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
global using global::Xunit;

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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+9155d521a438f149d7e348e4ab6e3b27738f5057")]
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

=== FILE: F:\Marketing\Marketing.Tests\obj\Release\net9.0\.NETCoreApp,Version=v9.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]

=== FILE: F:\Marketing\Marketing.Tests\obj\Release\net9.0\Marketing.Tests.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+686d4a9fd23366c79861334aafe6e4c8aa0b6377")]
[assembly: System.Reflection.AssemblyProductAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyTitleAttribute("Marketing.Tests")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Marketing.Tests\obj\Release\net9.0\Marketing.Tests.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
global using global::Xunit;

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

=== FILE: F:\Marketing\Persistence\Context\Implementation\Migrations\20260112035756_InitialCreate.cs ===

﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Persistence.Context.Implementation.Migrations
{
public partial class InitialCreate : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.CreateTable(
name: "ErrorLogs",
columns: table => new
{
Id = table.Column<string>(type: "TEXT", nullable: false),
Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
Level = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
Message = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
ExceptionType = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
StackTrace = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
Context = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
Active = table.Column<bool>(type: "INTEGER", nullable: false)
},
constraints: table =>
{
table.PrimaryKey("PK_ErrorLogs", x => x.Id);
});
migrationBuilder.CreateTable(
name: "TrackedLinks",
columns: table => new
{
Id = table.Column<string>(type: "TEXT", nullable: false),
TargetUrl = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
VisitCount = table.Column<long>(type: "INTEGER", nullable: false),
Active = table.Column<bool>(type: "INTEGER", nullable: false)
},
constraints: table =>
{
table.PrimaryKey("PK_TrackedLinks", x => x.Id);
});
}
protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.DropTable(
name: "ErrorLogs");
migrationBuilder.DropTable(
name: "TrackedLinks");
}
}
}

=== FILE: F:\Marketing\Persistence\Context\Implementation\Migrations\20260112035756_InitialCreate.Designer.cs ===

﻿
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence.Context.Implementation;
#nullable disable
namespace Persistence.Context.Implementation.Migrations
{
[DbContext(typeof(DataContext))]
[Migration("20260112035756_InitialCreate")]
partial class InitialCreate
{
protected override void BuildTargetModel(ModelBuilder modelBuilder)
{
#pragma warning disable 612, 618
modelBuilder.HasAnnotation("ProductVersion", "9.0.6");
modelBuilder.Entity("Domain.ErrorLog", b =>
{
b.Property<string>("Id")
.HasColumnType("TEXT");
b.Property<bool>("Active")
.HasColumnType("INTEGER");
b.Property<string>("Context")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("ExceptionType")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("Level")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("Message")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("StackTrace")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<DateTime>("Timestamp")
.HasColumnType("TEXT");
b.HasKey("Id");
b.ToTable("ErrorLogs", (string)null);
});
modelBuilder.Entity("Domain.WhatsApp.Redirect.TrackedLink", b =>
{
b.Property<string>("Id")
.HasColumnType("TEXT");
b.Property<bool>("Active")
.HasColumnType("INTEGER");
b.Property<string>("TargetUrl")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<long>("VisitCount")
.HasColumnType("INTEGER");
b.HasKey("Id");
b.ToTable("TrackedLinks", (string)null);
});
#pragma warning restore 612, 618
}
}
}

=== FILE: F:\Marketing\Persistence\Context\Implementation\Migrations\DataContextModelSnapshot.cs ===

﻿
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence.Context.Implementation;
#nullable disable
namespace Persistence.Context.Implementation.Migrations
{
[DbContext(typeof(DataContext))]
partial class DataContextModelSnapshot : ModelSnapshot
{
protected override void BuildModel(ModelBuilder modelBuilder)
{
#pragma warning disable 612, 618
modelBuilder.HasAnnotation("ProductVersion", "9.0.6");
modelBuilder.Entity("Domain.ErrorLog", b =>
{
b.Property<string>("Id")
.HasColumnType("TEXT");
b.Property<bool>("Active")
.HasColumnType("INTEGER");
b.Property<string>("Context")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("ExceptionType")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("Level")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("Message")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<string>("StackTrace")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<DateTime>("Timestamp")
.HasColumnType("TEXT");
b.HasKey("Id");
b.ToTable("ErrorLogs", (string)null);
});
modelBuilder.Entity("Domain.WhatsApp.Redirect.TrackedLink", b =>
{
b.Property<string>("Id")
.HasColumnType("TEXT");
b.Property<bool>("Active")
.HasColumnType("INTEGER");
b.Property<string>("TargetUrl")
.IsRequired()
.HasMaxLength(150)
.HasColumnType("TEXT");
b.Property<long>("VisitCount")
.HasColumnType("INTEGER");
b.HasKey("Id");
b.ToTable("TrackedLinks", (string)null);
});
#pragma warning restore 612, 618
}
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
public const string TrackedLinks = "TrackedLinks";
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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

=== FILE: F:\Marketing\Persistence\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Persistence\obj\Release\net8.0\Persistence.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Persistence")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
[assembly: System.Reflection.AssemblyProductAttribute("Persistence")]
[assembly: System.Reflection.AssemblyTitleAttribute("Persistence")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Persistence\obj\Release\net8.0\Persistence.GlobalUsings.g.cs ===

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

=== FILE: F:\Marketing\Services\AutoIt\AutoItRunnerResult.cs ===

﻿using System.Diagnostics;
using System.Text;
using Configuration;
using Domain;
using Microsoft.Extensions.Logging;
using Services.Abstractions.AutoIt;
namespace Services.AutoIt
{
public sealed class AutoItRunner(AppConfig config, ILogger<AutoItRunner> logger) : IAutoItRunner
{
private AppConfig Config { get; } = config;
private ILogger<AutoItRunner> Logger { get; } = logger;
private string ReadTemplete(string scriptTemplete)
{
if (!File.Exists(scriptTemplete))
{
Logger.LogError("AutoIt script template not found at {TemplatePath}", scriptTemplete);
throw new FileNotFoundException("AutoIt script template not found.", scriptTemplete);
}
Logger.LogInformation("Reading AutoIt script template from {TemplatePath}", scriptTemplete);
var content = File.ReadAllText(scriptTemplete);
Logger.LogDebug("AutoIt template size={Length} chars", content.Length);
return content;
}
private void WriteScript(string scriptPath, string autoItScript)
{
Logger.LogInformation("Writing AutoIt script to {ScriptPath}", scriptPath);
File.WriteAllText(scriptPath, autoItScript, new UTF8Encoding(false));
Logger.LogInformation("AutoIt script written successfully. Size={Length} chars", autoItScript.Length);
}
public async Task<AutoItRunnerResult> RunAsync(
TimeSpan timeout,
string imagePath,
bool useAutoItInterpreter = false,
CancellationToken cancellationToken = default)
{
int step = 0;
void LogStep(string message)
{
step++;
Logger.LogInformation("Step {Step}: {Message}", step, message);
}
Logger.LogInformation("============================================================");
Logger.LogInformation("AutoItRunner START");
Logger.LogInformation(
"timeout={Timeout} imagePath={ImagePath} useAutoItInterpreter={UseAutoItInterpreter} cancellationRequested={CancellationRequested}",
timeout, imagePath, useAutoItInterpreter, cancellationToken.IsCancellationRequested);
Logger.LogInformation("============================================================");
LogStep("Resolving base paths and preparing AutoIt script.");
var basePath = Directory.GetCurrentDirectory();
Logger.LogDebug("CurrentDirectory={BasePath}", basePath);
var scriptTemplete = Path.Combine(basePath, "whatsapp_upload.au3");
var autoItScript = ReadTemplete(scriptTemplete);
var scriptPath = Path.Combine(Path.GetTempPath(), $"whatsapp_upload_{Guid.NewGuid():N}.au3");
Logger.LogInformation("Generated temp AutoIt script path {ScriptPath}", scriptPath);
Logger.LogDebug("Replacing __FILE_TO_UPLOAD__ with {ImagePath}", imagePath);
autoItScript = autoItScript.Replace("__FILE_TO_UPLOAD__", imagePath);
var autoItInterpreterPath = Config.Paths.AutoItInterpreterPath;
var autoItLogDir = Path.Combine(Config.Paths.OutFolder, "AutoItLog");
Logger.LogInformation("autoItInterpreterPath={AutoItInterpreterPath}", autoItInterpreterPath);
Logger.LogInformation("autoItLogDir={AutoItLogDir}", autoItLogDir);
if (!Directory.Exists(autoItLogDir))
{
Logger.LogInformation("AutoItLog directory does not exist. Creating.");
Directory.CreateDirectory(autoItLogDir);
}
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
var logFilePath = Path.Combine(autoItLogDir, $"AutoItRunner_{timestamp}.log");
Logger.LogDebug("Replacing __AUTOIT_LOG_FILE__ with {LogFilePath}", logFilePath);
autoItScript = autoItScript.Replace("__AUTOIT_LOG_FILE__", logFilePath);
WriteScript(scriptPath, autoItScript);
LogStep("Validating generated script and execution mode.");
if (!File.Exists(scriptPath))
{
Logger.LogError("Generated AutoIt script missing at {ScriptPath}", scriptPath);
throw new FileNotFoundException("Generated AutoIt script not found.", scriptPath);
}
if (!useAutoItInterpreter)
{
Logger.LogWarning(
"useAutoItInterpreter=false but script is .au3. Execution depends on file association.");
}
if (useAutoItInterpreter && !File.Exists(autoItInterpreterPath))
{
Logger.LogError("AutoIt interpreter not found at {Path}", autoItInterpreterPath);
throw new FileNotFoundException("AutoIt interpreter not found.", autoItInterpreterPath);
}
LogStep("Building process command line.");
string fileName;
string arguments;
if (useAutoItInterpreter)
{
fileName = autoItInterpreterPath!;
arguments = scriptPath;
Logger.LogInformation("Interpreter mode. FileName={FileName} Args={Arguments}", fileName, arguments);
}
else
{
fileName = scriptPath;
arguments = imagePath ?? string.Empty;
Logger.LogInformation("Direct mode. FileName={FileName} Args={Arguments}", fileName, arguments);
}
LogStep("Creating ProcessStartInfo.");
var psi = new ProcessStartInfo
{
FileName = fileName,
Arguments = arguments,
WorkingDirectory = autoItLogDir,
UseShellExecute = false,
CreateNoWindow = true,
RedirectStandardOutput = true,
RedirectStandardError = true
};
var stdout = new StringBuilder();
var stderr = new StringBuilder();
using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
var tcsExit = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
LogStep("Attaching process event handlers.");
proc.OutputDataReceived += (_, e) =>
{
if (e.Data != null)
{
stdout.AppendLine(e.Data);
Logger.LogDebug("STDOUT: {Line}", e.Data);
}
};
proc.ErrorDataReceived += (_, e) =>
{
if (e.Data != null)
{
stderr.AppendLine(e.Data);
Logger.LogWarning("STDERR: {Line}", e.Data);
}
};
proc.Exited += (_, __) =>
{
Logger.LogInformation("Process exited. ExitCode={ExitCode}", proc.ExitCode);
tcsExit.TrySetResult(proc.ExitCode);
};
LogStep("Starting AutoIt process.");
if (!proc.Start())
{
Logger.LogError("Process.Start() returned false.");
throw new InvalidOperationException("Failed to start AutoIt process.");
}
Logger.LogInformation("Process started. PID={Pid}", proc.Id);
proc.BeginOutputReadLine();
proc.BeginErrorReadLine();
LogStep("Waiting for process completion.");
using var timeoutCts = new CancellationTokenSource(timeout);
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
bool timedOut = false;
int exitCode;
try
{
var completed = await Task.WhenAny(
tcsExit.Task,
Task.Delay(Timeout.Infinite, linkedCts.Token));
if (completed == tcsExit.Task)
{
exitCode = await tcsExit.Task;
}
else
{
timedOut = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;
Logger.LogWarning(
"Process interrupted. TimedOut={TimedOut} ExternalCancel={ExternalCancel}",
timedOut, cancellationToken.IsCancellationRequested);
Logger.LogWarning("Killing AutoIt process tree.");
TryKillProcessTree(proc);
exitCode = await SafeWaitForExitCodeAsync(tcsExit, TimeSpan.FromSeconds(3));
if (exitCode == -1)
Logger.LogWarning("Exit code unresolved after forced termination.");
}
}
finally
{
Logger.LogDebug("Final cleanup: ensuring process is terminated.");
TryKillProcessTree(proc);
}
LogStep("Finalizing execution.");
var duration = DateTimeOffset.UtcNow - DateTimeOffset.UtcNow.Add(-timeout);
Logger.LogInformation(
"AutoItRunner END. ExitCode={ExitCode} TimedOut={TimedOut} Duration={Duration}",
exitCode, timedOut, duration);
return new AutoItRunnerResult
{
ExitCode = exitCode,
TimedOut = timedOut,
StdOut = stdout.ToString(),
StdErr = stderr.ToString(),
Duration = duration,
LogFilePath = logFilePath
};
}
private static void TryKillProcessTree(Process proc)
{
try
{
if (proc.HasExited) return;
proc.Kill(entireProcessTree: true);
}
catch
{
}
}
private static async Task<int> SafeWaitForExitCodeAsync(TaskCompletionSource<int> tcs, TimeSpan maxWait)
{
var completed = await Task.WhenAny(tcs.Task, Task.Delay(maxWait));
return completed == tcs.Task ? await tcs.Task : -1;
}
}
}

=== FILE: F:\Marketing\Services\Check\CaptureSnapshot.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.Check;
namespace Services.Check
{
public class CaptureSnapshot(IWebDriver driver, ILogger<CaptureSnapshot> logger) : ICaptureSnapshot
{
private readonly IWebDriver _driver = driver;
private readonly ILogger<CaptureSnapshot> _logger = logger;
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

=== FILE: F:\Marketing\Services\Check\ChromeDriverFactory.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Services.Abstractions.Check;
namespace Services.Check;
public sealed class ChromeDriverFactory(ILogger<ChromeDriverFactory> logger) : IWebDriverFactory
{
private readonly ILogger<ChromeDriverFactory> _logger = logger;
public IWebDriver Create(bool hide)
{
var options = BuildDefaultOptions(hide);
return CreateDriver(options);
}
public IWebDriver Create(Action<ChromeOptions> configure)
{
var options = BuildDefaultOptions(hide: false);
configure?.Invoke(options);
return CreateDriver(options);
}
public ChromeOptions GetDefaultOptions(string mode)
{
var hide = mode.Equals("headless", StringComparison.OrdinalIgnoreCase);
return BuildDefaultOptions(hide);
}
private IWebDriver CreateDriver(ChromeOptions options)
{
try
{
var logDir = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
"WhatsAppSender",
"chromedriver-logs"
);
Directory.CreateDirectory(logDir);
var logFile = Path.Combine(
logDir,
$"chromedriver_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Environment.ProcessId}.log"
);
var service = ChromeDriverService.CreateDefaultService();
service.EnableVerboseLogging = true;
service.LogPath = logFile;
service.HideCommandPromptWindow = true;
_logger.LogInformation(
"Creating ChromeDriver (Selenium Manager). Log={LogFile}",
logFile
);
var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(60));
driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(60);
driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
return driver;
}
catch (Exception ex)
{
_logger.LogError(ex, "Failed to create ChromeDriver");
throw;
}
}
private static ChromeOptions BuildDefaultOptions(bool hide)
{
var options = new ChromeOptions();
options.AddArgument("--disable-notifications");
options.AddArgument("--disable-extensions");
options.AddArgument("--disable-popup-blocking");
options.AddArgument("--disable-gpu");
options.AddArgument("--disable-dev-shm-usage");
options.AddArgument("--no-sandbox");
options.AddArgument("--remote-allow-origins=*");
if (hide)
{
options.AddArgument("--headless=new");
options.AddArgument("--window-size=1920,1080");
}
return options;
}
}

=== FILE: F:\Marketing\Services\Check\DirectoryCheck.cs ===

﻿using Configuration;
using Microsoft.Extensions.Logging;
using Services.Abstractions.Check;
namespace Services.Check
{
public class DirectoryCheck(ILogger<DirectoryCheck> logger, ExecutionTracker executionOptions) : IDirectoryCheck
{
private readonly ILogger<DirectoryCheck> _logger = logger;
private readonly ExecutionTracker _executionOptions = executionOptions;
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

=== FILE: F:\Marketing\Services\Check\SecurityCheck.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.Check;
using Configuration;
namespace Services.Check
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

=== FILE: F:\Marketing\Services\Check\WebDriverLifetimeService.cs ===

﻿using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
namespace Services.Check
{
public sealed class WebDriverLifetimeService(IWebDriver driver) : IHostedService
{
private readonly IWebDriver _driver = driver;
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

=== FILE: F:\Marketing\Services\Login\LoginService.cs ===

﻿using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.Login;
using Services.Abstractions.Check;
namespace Services.Login
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

=== FILE: F:\Marketing\Services\Login\LoginStateChecker.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.Login;
using Services.Abstractions.Selector;
using Services.Abstractions.Selenium;
namespace Services.Login
{
internal sealed class LoginStateChecker(
IWebDriverFacade driver,
ISelectors selectors,
ILogger<LoginService> logger) : ILoginStateChecker
{
public bool IsWhatsAppLoggedIn()
{
logger.LogDebug("IsWhatsAppLoggedIn: Checking WhatsApp Web login state...");
try
{
var elements = driver.FindElements(By.CssSelector(selectors.CssSelectorToFindLoggedInMarker));
var isLoggedIn = elements.Count > 0;
logger.LogDebug(
"IsWhatsAppLoggedIn: Selector '{Selector}' returned {Count} elements. LoggedIn={IsLoggedIn}.",
selectors.CssSelectorToFindLoggedInMarker,
elements.Count,
isLoggedIn
);
return isLoggedIn;
}
catch (NoSuchElementException ex)
{
logger.LogWarning(
ex,
"IsWhatsAppLoggedIn: Selector '{Selector}' not found. Assuming not logged in.",
selectors.CssSelectorToFindLoggedInMarker
);
return false;
}
catch (WebDriverException ex)
{
logger.LogError(
ex,
"IsWhatsAppLoggedIn: WebDriver error while checking login state."
);
return false;
}
catch (Exception ex)
{
logger.LogError(
ex,
"IsWhatsAppLoggedIn: Unexpected error while checking login state."
);
return false;
}
}
}
}

=== FILE: F:\Marketing\Services\Login\Message.cs ===

﻿using Configuration;
using Domain;
using Microsoft.Extensions.Logging;
using Services.Abstractions.Login;
using Services.Abstractions.OpenChat;
namespace Services.Login
{
public class Message(
ILogger<LoginService> logger,
ILoginService loginService,
ExecutionTracker executionOption,
IChatService whatsAppChatService,
AppConfig config,
IOpenChat whatAppOpenChat
) : IMessage
{
public ILogger<LoginService> Logger { get; } = logger;
public ILoginService Login { get; } = loginService;
public ExecutionTracker ExecutionOption { get; } = executionOption;
public IChatService WhatsAppChatService { get; } = whatsAppChatService;
private AppConfig Config { get; } = config;
private IOpenChat WhatAppOpenChat { get; } = whatAppOpenChat;
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
var msg = Config.WhatsApp.Message;
var imagePath = Path.Combine(msg.ImageDirectory, msg.ImageFileName);
var payload = new ImageMessagePayload
{
StoredImagePath = imagePath,
Caption = msg.Caption
};
await WhatsAppChatService.SendMessageAsync(
payload,
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
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

=== FILE: F:\Marketing\Services\OpenAI\OpenAIClient.cs ===

﻿using Configuration;
using Domain.OpenAI;
using Microsoft.Extensions.Options;
using Services.Abstractions.OpenAI;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
namespace Services.OpenAI
{
public class OpenAIClient(IOptions<OpenAIConfig> openAI, HttpClient httpClient) : IOpenAIClient
{
private readonly IOptions<OpenAIConfig> _openAI = openAI;
private readonly HttpClient _httpClient = httpClient;
public async Task<string> GetChatCompletionAsync(Prompt prompt, CancellationToken ct = default)
{
var apiKey = Environment.GetEnvironmentVariable(_openAI.Value.ApiKey);
var requestBody = new OpenAIChatRequest(_openAI.Value.Model)
{
Messages =
[
new() { Role = "system", Content = prompt.SystemContent },
new() { Role = "user",   Content = prompt.UserContent }
],
Stream = false
};
var jsonBody = JsonSerializer.Serialize(requestBody);
var contentRequest = new StringContent(jsonBody, Encoding.UTF8, "application/json");
var requestUri = $"{_openAI.Value.UriString}/v1/chat/completions";
const int maxAttempts = 3;
for (var attempt = 1; attempt <= maxAttempts; attempt++)
{
var response = await _httpClient.PostAsync(requestUri, contentRequest, ct);
var statusResponse = response.IsSuccessStatusCode;
if (statusResponse)
{
var responseData = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>(cancellationToken: ct);
if (responseData?.Choices == null || responseData.Choices.Count == 0)
throw new Exception("No response received from OpenAI API.");
var content = responseData.Choices[0]?.Message?.Content;
if (string.IsNullOrWhiteSpace(content))
throw new Exception("OpenAI API returned an empty completion.");
return content.Trim();
}
var status = response.StatusCode;
var isTransient =
status == (HttpStatusCode)429 ||
status == HttpStatusCode.ServiceUnavailable ||
status == HttpStatusCode.GatewayTimeout;
if (!isTransient || attempt == maxAttempts)
{
var errorContent = await response.Content.ReadAsStringAsync(ct);
throw new Exception($"OpenAI API request failed with status code {status}: {errorContent}");
}
TimeSpan delay;
if (response.Headers.RetryAfter?.Delta is { } retryAfterDelta)
delay = retryAfterDelta;
else
delay = TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1));
await Task.Delay(delay, ct);
}
throw new Exception("Unexpected failure executing OpenAI request.");
}
}
}

=== FILE: F:\Marketing\Services\OpenAI\news\JsonPromptRunner.cs ===

﻿using Application.Result;
using Common.StringExtensions;
using Configuration.YouTube;
using Domain.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prompts.NostalgiaRank;
using Services.Abstractions.OpenAI;
using Services.Abstractions.OpenAI.news;
using Services.Abstractions.UrlValidation;
using Services.Abstractions.YouTube;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
namespace Services.OpenAI.news
{
public class JsonPromptRunner(
INewsHistoryStore newsHistoryStore,
INostalgiaPromptLoader nostalgiaPromptLoader,
IOpenAIClient openAIClient,
IUrlFactory urlValidatorFactory,
IYouTubeService youTubeService,
IOptions<YouTubeCurationRunnerOptions> youTubeRunnerOptions,
ILogger<JsonPromptRunner> logger
) : IJsonPromptRunner
{
private readonly INewsHistoryStore _newsHistoryStore = newsHistoryStore;
private readonly INostalgiaPromptLoader _nostalgiaPromptLoader = nostalgiaPromptLoader;
private readonly IOpenAIClient _openAIClient = openAIClient;
private readonly IUrlFactory _urlValidatorFactory = urlValidatorFactory;
private readonly IYouTubeService _youTubeService = youTubeService;
private readonly YouTubeCurationRunnerOptions _yt = youTubeRunnerOptions.Value;
private readonly ILogger<JsonPromptRunner> _logger = logger;
private const int DesiredCount = 5;
private static readonly TimeSpan MaxOverallDuration = TimeSpan.FromMinutes(2);
public async Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default)
{
var runId = Guid.NewGuid().ToString("N")[..8];
var startedUtc = DateTimeOffset.UtcNow;
_logger.LogInformation("[RUN {RunId}] START JsonPromptRunner. DesiredCount={DesiredCount}, Timeout={TimeoutSeconds}s, Query={Query}",
runId, DesiredCount, (int)MaxOverallDuration.TotalSeconds, _yt.Query);
try
{
Step0_Preflight(runId);
var history = await LoadHistoryAsync(runId, ct);
var promptNostalgia = await LoadPromptAsync(runId, ct);
var attempted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var deadline = DateTimeOffset.UtcNow + MaxOverallDuration;
_logger.LogInformation("[RUN {RunId}] STEP 3: Init iteration state. HistoryCount={HistoryCount}, DeadlineUtc={DeadlineUtc:o}",
runId, history.Count, deadline);
var loop = 0;
while (DateTimeOffset.UtcNow < deadline && !ct.IsCancellationRequested)
{
loop++;
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: Begin. AttemptedCount={AttemptedCount}, UtcNow={UtcNow:o}",
runId, loop, attempted.Count, DateTimeOffset.UtcNow);
var searchOp = await CallYouTubeSearchAsync(runId, loop, _yt.Query, _yt.Search, ct);
if (!searchOp.IsSuccessful || searchOp.Data is null || searchOp.Data.Items.Count == 0)
{
_logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.2 YouTube search returned no items. Success={Success}, Items={Items}. Retrying.",
runId, loop, searchOp.IsSuccessful, searchOp.Data?.Items.Count ?? 0);
continue;
}
var candidatesFromYoutube = ExtractCandidateUrlsFromYouTube(runId, loop, searchOp.Data, history, attempted);
if (candidatesFromYoutube.Count == 0)
{
_logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.3 No candidate URLs after exclusions (history/attempted). Retrying.",
runId, loop);
continue;
}
var prompt = BuildPrompt(runId, loop, promptNostalgia, [.. history], candidatesFromYoutube);
var llmRaw = await CallLlmAsync(runId, loop, prompt, ct);
var candidatesFiltered = ExtractCandidateUrls(runId, loop, llmRaw);
if (candidatesFiltered.Count == 0)
{
_logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.6 LLM returned no URLs. Retrying.",
runId, loop);
AddAttempted(attempted, candidatesFromYoutube);
continue;
}
AddAttempted(attempted, candidatesFromYoutube);
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.7 Attempted updated. AttemptedCount={AttemptedCount}",
runId, loop, attempted.Count);
var validation = await ValidateCandidatesUntilDesiredAsync(runId, loop, candidatesFiltered, DesiredCount, ct);
await AppendUsedUrlsAsync(runId, loop, candidatesFiltered, ct);
if (validation.Success && validation.ValidUrls.Count == DesiredCount)
{
_logger.LogInformation("[RUN {RunId}] SUCCESS. ValidCount={Count}. ElapsedMs={ElapsedMs}",
runId, validation.ValidUrls.Count, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);
return [.. validation.ValidUrls];
}
LogFirstFailure(runId, loop, validation.Results);
_logger.LogWarning("[RUN {RunId}] LOOP {Loop}: Not enough valid URLs. ValidCount={ValidCount}/{DesiredCount}. Retrying.",
runId, loop, validation.ValidUrls.Count, DesiredCount);
}
_logger.LogWarning("[RUN {RunId}] END: Timed out or cancelled. Cancelled={Cancelled}. ElapsedMs={ElapsedMs}",
runId, ct.IsCancellationRequested, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);
return new List<string>();
}
catch (OperationCanceledException)
{
_logger.LogWarning("[RUN {RunId}] CANCELLED. ElapsedMs={ElapsedMs}",
runId, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);
return new List<string>();
}
catch (Exception ex)
{
_logger.LogError(ex, "[RUN {RunId}] FAILED with unhandled exception. ElapsedMs={ElapsedMs}",
runId, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);
return new List<string>();
}
}
private void Step0_Preflight(string runId)
{
_logger.LogDebug("[RUN {RunId}] STEP 0: Preflight checks...", runId);
if (_yt is null)
throw new InvalidOperationException("YouTubeCurationRunnerOptions is null.");
if (string.IsNullOrWhiteSpace(_yt.Query))
_logger.LogWarning("[RUN {RunId}] STEP 0: YouTube query is empty/blank.", runId);
if (_yt.Search is null)
_logger.LogWarning("[RUN {RunId}] STEP 0: YouTube search options are null.", runId);
_logger.LogDebug("[RUN {RunId}] STEP 0: Preflight completed.", runId);
}
private List<string> ExtractCandidateUrls(string runId, int loop, string llmRaw)
{
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.6 Extracting URLs from LLM response. RawLen={Len}",
runId, loop, llmRaw?.Length ?? 0);
var extracted = llmRaw.ExtractJsonContent();
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.6 JSON extracted from LLM. ExtractedLen={Len}",
runId, loop, extracted?.Length ?? 0);
var urls = ExtractUrlsFromLlmResponse(extracted ?? string.Empty)
.Where(u => !string.IsNullOrWhiteSpace(u))
.Select(u => u.Trim())
.Distinct(StringComparer.OrdinalIgnoreCase)
.ToList();
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.6 Extracted {Count} candidate URLs from LLM.",
runId, loop, urls.Count);
return urls;
}
private async Task<HashSet<string>> LoadHistoryAsync(string runId, CancellationToken ct)
{
_logger.LogInformation("[RUN {RunId}] STEP 1: Loading history URLs...", runId);
var history = await _newsHistoryStore.LoadUrlsAsync(ct);
_logger.LogInformation("[RUN {RunId}] STEP 1: Loaded {Count} historical URLs.", runId, history.Count);
return history;
}
private async Task AppendUsedUrlsAsync(string runId, int loop, IEnumerable<string> urls, CancellationToken ct)
{
var list = urls?.ToList() ?? new List<string>();
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.9 Appending used URLs to history. Count={Count}",
runId, loop, list.Count);
if (list.Count == 0)
{
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.9 No URLs to append. Skipping.", runId, loop);
return;
}
await _newsHistoryStore.AppendUsedUrlsAsync(list, ct);
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.9 Appended {Count} URLs to history.",
runId, loop, list.Count);
}
private async Task<NostalgiaRankPrompt> LoadPromptAsync(string runId, CancellationToken ct)
{
_logger.LogInformation("[RUN {RunId}] STEP 2: Loading NostalgiaPrompt template...", runId);
var prompt = await _nostalgiaPromptLoader.LoadPromptAsync();
_logger.LogInformation("[RUN {RunId}] STEP 2: Prompt loaded. RoleLines={RoleLines}.",
runId, prompt?.Role?.Count ?? 0);
return prompt;
}
private async Task<Operation<SearchResponse>> CallYouTubeSearchAsync(
string runId,
int loop,
string query,
SearchOptions options,
CancellationToken ct)
{
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.2 Calling YouTube Search API. Query={Query}",
runId, loop, query);
var op = await _youTubeService.SearchVideosAsync(query, options);
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.2 YouTube Search completed. Success={Success}, Items={Items}",
runId, loop, op.IsSuccessful, op.Data?.Items.Count ?? 0);
return op;
}
private List<string> ExtractCandidateUrlsFromYouTube(
string runId,
int loop,
SearchResponse response,
HashSet<string> history,
HashSet<string> attempted)
{
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.3 Building candidate URLs from YouTube items...",
runId, loop);
var urls =
response.Items
.Select(i => $"https://www.youtube.com/watch?v={i.VideoId}")
.Where(u => !string.IsNullOrWhiteSpace(u))
.Distinct(StringComparer.OrdinalIgnoreCase)
.Where(u => !history.Contains(u))
.Where(u => !attempted.Contains(u))
.ToList();
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.3 Candidate URLs after exclusions: {Count}",
runId, loop, urls.Count);
return urls;
}
private static void AddAttempted(HashSet<string> attempted, List<string> candidates)
{
foreach (var c in candidates)
attempted.Add(c);
}
private async Task<(bool Success, IReadOnlyList<string> ValidUrls, IReadOnlyList<UrlValidationResult> Results)>
ValidateCandidatesUntilDesiredAsync(
string runId,
int loop,
IReadOnlyList<string> urls,
int desiredCount,
CancellationToken ct)
{
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Validating URLs linearly. Target={DesiredCount}, InputCount={InputCount}",
runId, loop, desiredCount, urls.Count);
var validUrls = new List<string>(desiredCount);
var results = new List<UrlValidationResult>(urls.Count);
var index = 0;
foreach (var url in urls)
{
index++;
ct.ThrowIfCancellationRequested();
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Validating URL {Index}/{Total}: {Url}",
runId, loop, index, urls.Count, url);
var res = await ValidateOneUrlAsync(runId, loop, url, ct);
results.Add(res);
if (res.IsValid)
{
validUrls.Add(url);
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.8 URL valid ({Valid}/{Desired}).",
runId, loop, validUrls.Count, desiredCount);
if (validUrls.Count == desiredCount)
{
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Desired number of valid URLs reached.",
runId, loop);
return (true, validUrls, results);
}
}
else
{
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8 URL invalid. Platform={Platform} Status={Status} Reason={Reason}",
runId, loop, res.Platform, res.HttpStatusCode, res.FailureReason);
}
}
_logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Validation finished. Valid={Valid}/{Desired}.",
runId, loop, validUrls.Count, desiredCount);
return (false, validUrls, results);
}
private async Task<UrlValidationResult> ValidateOneUrlAsync(string runId, int loop, string url, CancellationToken ct)
{
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 ValidateOneUrl start. Url={Url}",
runId, loop, url);
if (!Uri.TryCreate(url, UriKind.Absolute, out _))
{
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Invalid absolute URL format. Url={Url}",
runId, loop, url);
return new UrlValidationResult(false, UrlPlatform.Unknown, null, "Invalid absolute URL.");
}
try
{
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Resolving validator from factory...", runId, loop);
var validator = _urlValidatorFactory.GetValidator(url);
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Validator resolved: {ValidatorType}",
runId, loop, validator.GetType().Name);
var result = await validator.ValidateAsync(url, ct);
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Validation done. IsValid={IsValid} Platform={Platform} Status={Status} Reason={Reason}",
runId, loop, result.IsValid, result.Platform, result.HttpStatusCode, result.FailureReason);
return result;
}
catch (Exception ex)
{
_logger.LogWarning(ex, "[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Validation exception for Url={Url}",
runId, loop, url);
return new UrlValidationResult(false, UrlPlatform.Unknown, null, ex.Message);
}
}
private void LogFirstFailure(string runId, int loop, IReadOnlyList<UrlValidationResult> results)
{
var firstFail = results.FirstOrDefault(r => !r.IsValid);
if (firstFail is null)
{
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.11 No failures to report (all valid).",
runId, loop);
return;
}
_logger.LogInformation(
"[RUN {RunId}] LOOP {Loop}: STEP 4.11 Batch rejected. First failure: Platform={Platform} Status={Status} Reason={Reason}",
runId,
loop,
firstFail.Platform,
firstFail.HttpStatusCode,
firstFail.FailureReason);
}
private static List<string> ExtractUrlsFromLlmResponse(string llmRaw)
{
try
{
using var doc = JsonDocument.Parse(llmRaw);
var root = doc.RootElement;
if (root.ValueKind == JsonValueKind.Array)
return [.. root.EnumerateArray()
.Where(e => e.ValueKind == JsonValueKind.String)
.Select(e => e.GetString()!)
.Where(IsProbablyUrl)];
if (root.ValueKind == JsonValueKind.Object &&
root.TryGetProperty("urls", out var urlsEl) &&
urlsEl.ValueKind == JsonValueKind.Array)
{
return [.. urlsEl.EnumerateArray()
.Where(e => e.ValueKind == JsonValueKind.String)
.Select(e => e.GetString()!)
.Where(IsProbablyUrl)];
}
if (root.ValueKind == JsonValueKind.Object &&
root.TryGetProperty("items", out var itemsEl) &&
itemsEl.ValueKind == JsonValueKind.Array)
{
return [.. itemsEl.EnumerateArray()
.Select(e => e.ValueKind == JsonValueKind.Object &&
e.TryGetProperty("url", out var u) &&
u.ValueKind == JsonValueKind.String
? u.GetString()
: null)
.Where(u => !string.IsNullOrWhiteSpace(u) && IsProbablyUrl(u!))
.Cast<string>()];
}
}
catch
{
}
return [.. Regex.Matches(llmRaw, @"https?:
.Select(m => TrimUrl(m.Value))
.Where(IsProbablyUrl)];
}
private static bool IsProbablyUrl(string s)
=> Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
private static string TrimUrl(string url)
=> url.Trim().TrimEnd('.', ',', ';', ')', ']', '}', '"', '\'');
public static string ToJsonWithoutRole(NostalgiaRankPrompt prompt, bool indented = true)
{
if (prompt is null) throw new ArgumentNullException(nameof(prompt));
var options = new JsonSerializerOptions { WriteIndented = indented };
JsonNode node = JsonSerializer.SerializeToNode(prompt, options)
?? throw new JsonException("Failed to serialize NostalgiaPrompt.");
node.AsObject().Remove("role");
return node.ToJsonString(options);
}
public static string ConvertJsonWithoutRole(string json)
{
if (string.IsNullOrWhiteSpace(json))
throw new ArgumentException("JSON input cannot be null or empty.", nameof(json));
var node = JsonNode.Parse(json) ?? throw new JsonException("Invalid JSON.");
node.AsObject().Remove("role");
return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
}
private Prompt BuildPrompt(
string runId,
int loop,
NostalgiaRankPrompt promptNostalgia,
IReadOnlyList<string> historicalUrls,
IReadOnlyList<string> newUrls)
{
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.4 Building LLM prompt (System + User JSON without role)...",
runId, loop);
ArgumentNullException.ThrowIfNull(promptNostalgia);
historicalUrls ??= [];
newUrls ??= [];
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.4 Injecting URLs into JSON. HistoricalCount={HCount}, NewCount={NCount}",
runId, loop, historicalUrls.Count, newUrls.Count);
var content = ToJsonWithoutRole(promptNostalgia);
content = ReplaceJsonTagWithArray(content, "__HISTORICAL_URLS__", historicalUrls);
content = ReplaceJsonTagWithArray(content, "__NEW_URLS__", newUrls);
var systemContent = string.Join(" ", promptNostalgia.Role.Where(r => !string.IsNullOrWhiteSpace(r)));
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.4 Prompt built. SystemLen={SysLen}, UserLen={UserLen}",
runId, loop, systemContent.Length, content.Length);
return new Prompt
{
SystemContent = systemContent,
UserContent = content
};
}
private static string ReplaceJsonTagWithArray(
string json,
string tag,
IReadOnlyList<string> urls)
{
if (string.IsNullOrWhiteSpace(json))
return json;
urls ??= [];
var arrayJson = JsonSerializer.Serialize(urls);
var quotedTag = JsonSerializer.Serialize(tag);
return json.Replace(quotedTag, arrayJson, StringComparison.Ordinal);
}
private async Task<string> CallLlmAsync(string runId, int loop, Prompt prompt, CancellationToken ct)
{
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.5 Calling LLM...",
runId, loop);
_logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.5 Prompt sizes. SystemLen={SysLen}, UserLen={UserLen}",
runId, loop, prompt.SystemContent?.Length ?? 0, prompt.UserContent?.Length ?? 0);
var raw = await _openAIClient.GetChatCompletionAsync(prompt, ct);
_logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.5 LLM response received. Length={Len}",
runId, loop, raw?.Length ?? 0);
return raw ?? string.Empty;
}
}
}

=== FILE: F:\Marketing\Services\OpenAI\news\NewsHistoryStore.cs ===

﻿using Services.Abstractions.OpenAI.news;
using System.Text.Json;
namespace Services.OpenAI.news
{
public sealed class NewsHistoryStore : INewsHistoryStore
{
private const string DefaultNameFileNewsHistoryStore = "NewsHistoryStore.json";
private readonly string _historyPath = Path.Combine(AppContext.BaseDirectory, DefaultNameFileNewsHistoryStore);
private static readonly JsonSerializerOptions JsonOpts = new()
{
PropertyNameCaseInsensitive = true,
WriteIndented = true
};
public async Task<HashSet<string>> LoadUrlsAsync(CancellationToken ct = default)
{
EnsureFileExists();
await using var fs = File.OpenRead(_historyPath);
var doc = await JsonSerializer.DeserializeAsync<HistoryDoc>(fs, JsonOpts, ct);
return (doc?.Urls ?? [])
.Where(u => !string.IsNullOrWhiteSpace(u))
.ToHashSet(StringComparer.Ordinal);
}
public async Task AppendUsedUrlsAsync(IEnumerable<string> urls, CancellationToken ct = default)
{
var toAdd = urls
.Where(u => !string.IsNullOrWhiteSpace(u))
.Select(u => u.Trim())
.ToList();
if (toAdd.Count == 0) return;
var used = await LoadUrlsAsync(ct);
var changed = toAdd.Any(u => used.Add(u));
if (!changed) return;
var doc = new HistoryDoc
{
Urls = [.. used.OrderBy(x => x, StringComparer.Ordinal)]
};
await WriteJsonAtomicallyAsync(_historyPath, doc, ct);
}
private static async Task WriteJsonAtomicallyAsync(string path, HistoryDoc doc, CancellationToken ct)
{
var dir = Path.GetDirectoryName(path);
if (!string.IsNullOrWhiteSpace(dir))
Directory.CreateDirectory(dir);
var tempPath = Path.Combine(dir ?? ".", $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
try
{
await using (var fs = new FileStream(
tempPath,
FileMode.CreateNew,
FileAccess.Write,
FileShare.None,
bufferSize: 64 * 1024,
options: FileOptions.Asynchronous | FileOptions.WriteThrough))
{
await JsonSerializer.SerializeAsync(fs, doc, JsonOpts, ct);
await fs.FlushAsync(ct);
}
if (File.Exists(path))
{
var backupPath = path + ".bak";
File.Replace(tempPath, path, backupPath, ignoreMetadataErrors: true);
TryDelete(backupPath);
}
else
{
File.Move(tempPath, path);
}
}
finally
{
TryDelete(tempPath);
}
}
private static void TryDelete(string path)
{
try
{
if (File.Exists(path)) File.Delete(path);
}
catch
{
}
}
private void EnsureFileExists()
{
var dir = Path.GetDirectoryName(_historyPath);
if (!string.IsNullOrWhiteSpace(dir))
Directory.CreateDirectory(dir);
if (!File.Exists(_historyPath))
{
File.WriteAllText(_historyPath, JsonSerializer.Serialize(new HistoryDoc { Urls = [] }, JsonOpts));
}
}
private sealed class HistoryDoc
{
public List<string> Urls { get; set; } = [];
}
}
}

=== FILE: F:\Marketing\Services\OpenAI\news\NostalgiaPromptLoader.cs ===

﻿using Prompts.NostalgiaRank;
using Services.Abstractions.OpenAI.news;
using System.Text.Json;
namespace Services.OpenAI.news
{
public class NostalgiaPromptLoader: INostalgiaPromptLoader
{
private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };
private const string DefaultNameFilePrompt = "NostalgiaRank.json";
public async Task<NostalgiaRankPrompt> LoadPromptAsync()
{
var path = Path.Combine(AppContext.BaseDirectory, "OpenAI", "news", DefaultNameFilePrompt);
if (!File.Exists(path))
throw new FileNotFoundException($"Prompt file not found: {path}");
var json = await File.ReadAllTextAsync(path);
var cfg = JsonSerializer.Deserialize<NostalgiaRankPrompt>(json, Opts);
return cfg ?? throw new InvalidOperationException("Prompt JSON is invalid or empty.");
}
}
}

=== FILE: F:\Marketing\Services\OpenChat\ChatClicker.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.OpenChat;
using Services.Abstractions.Selector;
using Services.Abstractions.Selenium;
using Services.Abstractions.XPath;
using Services.Login;
namespace Services.OpenChat
{
internal sealed class ChatClicker(
IWebDriverFacade driver,
ISelectors selectors,
IChatXPathBuilder xpathBuilder,
ILogger<LoginService> logger) : IClicker
{
public async Task ClickChatByTitleAsync(
string chatTitle,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default)
{
logger.LogInformation(
"ClickChatByTitleAsync started. chatTitleLength={ChatTitleLength}",
chatTitle?.Length ?? 0
);
if (string.IsNullOrWhiteSpace(chatTitle))
{
logger.LogWarning("ClickChatByTitleAsync aborted: chatTitle is null or whitespace.");
throw new ArgumentException("Chat title cannot be empty.", nameof(chatTitle));
}
timeout ??= TimeSpan.FromSeconds(10);
pollInterval ??= TimeSpan.FromMilliseconds(250);
logger.LogInformation(
"Using timeout={Timeout} pollInterval={PollInterval}.",
timeout, pollInterval
);
var needle = chatTitle.Trim().ToLowerInvariant();
var end = DateTimeOffset.UtcNow + timeout.Value;
logger.LogDebug(
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
var xpathToFind = xpathBuilder.GetXpathToFind(needle);
logger.LogDebug(
"Attempt {Attempt}: Searching chat span using XPath: {XPath}",
attempt,
xpathToFind
);
var span = driver.FindElements(By.XPath(xpathToFind)).FirstOrDefault();
if (span is { Displayed: true })
{
logger.LogInformation(
"Attempt {Attempt}: Matching chat span found and displayed. Resolving clickable target...",
attempt
);
var target = span.FindElements(By.XPath(selectors.XpathToFindGridcellAncestor)).FirstOrDefault() ?? span;
logger.LogDebug(
"Attempt {Attempt}: Target resolved. targetDisplayed={Displayed} targetEnabled={Enabled}",
attempt,
target.Displayed,
target.Enabled
);
if (target.Displayed && target.Enabled)
{
logger.LogInformation("Attempt {Attempt}: Clicking chat target...", attempt);
target.Click();
logger.LogInformation("ClickChatByTitleAsync completed successfully.");
return;
}
logger.LogDebug(
"Attempt {Attempt}: Target found but not clickable (displayed/enabled check failed).",
attempt
);
}
else
{
logger.LogDebug(
"Attempt {Attempt}: No displayed span matched the chat title yet.",
attempt
);
}
}
catch (StaleElementReferenceException)
{
logger.LogDebug(
"Attempt {Attempt}: StaleElementReferenceException encountered (DOM rerender). Retrying...",
attempt
);
}
catch (NoSuchElementException)
{
logger.LogDebug(
"Attempt {Attempt}: NoSuchElementException encountered (ancestor/target missing). Retrying...",
attempt
);
}
catch (Exception ex)
{
logger.LogError(
ex,
"ClickChatByTitleAsync failed unexpectedly on attempt {Attempt}.",
attempt
);
throw;
}
await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
}
logger.LogError(
"ClickChatByTitleAsync timed out after {TimeoutSeconds} seconds. Chat not found/clickable. chatTitleLength={ChatTitleLength}",
timeout.Value.TotalSeconds,
chatTitle.Length
);
throw new WebDriverTimeoutException($"Chat not found or not clickable: '{chatTitle}'.");
}
}
}

=== FILE: F:\Marketing\Services\OpenChat\ChatService.cs ===

﻿using Configuration;
using Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Abstractions.AutoIt;
using Services.Abstractions.OpenChat;
using Services.Abstractions.Search;
namespace Services.OpenChat
{
public sealed class ChatService(
IWebDriver driver,
ILogger<ChatService> logger,
AppConfig config,
IAutoItRunner autoItRunner,
IAttachments controls
) : IChatService
{
private const string XpathFindCaption =
"
private IWebDriver Driver { get; } = driver;
public ILogger<ChatService> Logger { get; } = logger;
private AppConfig Config { get; } = config;
private IAutoItRunner AutoItRunner { get; } = autoItRunner;
private IAttachments Controls { get; } = controls;
public async Task SendMessageAsync(
ImageMessagePayload imageMessagePayload,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default)
{
Logger.LogInformation(
"SendMessageAsync started. hasPayload={HasPayload} messageLength={MessageLength} storedImagePath='{StoredImagePath}'",
imageMessagePayload is not null,
imageMessagePayload?.Caption?.Length ?? 0,
imageMessagePayload?.StoredImagePath
);
ct.ThrowIfCancellationRequested();
if (imageMessagePayload is null)
{
Logger.LogError("SendMessageAsync aborted: imageMessagePayload is null.");
throw new ArgumentNullException(nameof(imageMessagePayload));
}
if (string.IsNullOrWhiteSpace(imageMessagePayload.Caption))
{
Logger.LogWarning("SendMessageAsync aborted: Caption is null/empty/whitespace.");
throw new ArgumentException("Message cannot be empty.", nameof(imageMessagePayload.Caption));
}
if (string.IsNullOrWhiteSpace(imageMessagePayload.StoredImagePath))
{
Logger.LogError("SendMessageAsync aborted: StoredImagePath is null/empty.");
throw new ArgumentException("StoredImagePath cannot be empty.", nameof(imageMessagePayload.StoredImagePath));
}
if (!File.Exists(imageMessagePayload.StoredImagePath))
{
Logger.LogError("SendMessageAsync aborted: StoredImagePath does not exist. path='{Path}'", imageMessagePayload.StoredImagePath);
throw new FileNotFoundException("Image file not found.", imageMessagePayload.StoredImagePath);
}
TimeSpan loginTimeout = timeout ?? Config.WhatsApp.LoginTimeout;
TimeSpan loginPollInterval = pollInterval ?? Config.WhatsApp.LoginPollInterval;
Logger.LogInformation(
"Using timeouts. timeout={Timeout} pollInterval={PollInterval}",
loginTimeout, loginPollInterval
);
Logger.LogInformation("Step 1/3: Locating attach button...");
var attachButton = Controls.FindAttachButton(loginTimeout, loginPollInterval);
if (attachButton is null)
{
throw new NoSuchElementException("Attach button not found.");
}
Logger.LogDebug(
"Attach button found. tag={Tag} displayed={Displayed}, enabled={Enabled} aria-label={AriaLabel} title={Title}",
attachButton.TagName,
attachButton.Displayed,
attachButton.Enabled,
attachButton.GetAttribute("aria-label"),
attachButton.GetAttribute("title")
);
Logger.LogInformation("Clicking attach button...");
attachButton.Click();
Logger.LogDebug("Attach button clicked.");
var photoAndVideo = controls.FindPhotosAndVideosOptionButton(loginTimeout, loginPollInterval);
if (photoAndVideo is null)
{
throw new NoSuchElementException("'Photos & videos' option not found.");
}
Logger.LogDebug(
"'Photos & videos' option found. tag={Tag} displayed={Displayed}, enabled={Enabled} aria-label={AriaLabel} title={Title}",
photoAndVideo.TagName,
photoAndVideo.Displayed,
photoAndVideo.Enabled,
photoAndVideo.GetAttribute("aria-label"),
photoAndVideo.GetAttribute("title")
);
Logger.LogInformation("Clicking 'Photos & videos' option...");
photoAndVideo.Click();
Logger.LogDebug("'Photos & videos' option clicked.");
Logger.LogInformation(
"Opening file dialog via AutoIT... storedImagePath='{StoredImagePath}' useInterpreter={UseInterpreter}",
imageMessagePayload.StoredImagePath,
true
);
AutoItRunnerResult autoItRunnerResult;
try
{
autoItRunnerResult = await AutoItRunner.RunAsync(
timeout: loginTimeout,
imagePath: imageMessagePayload.StoredImagePath,
useAutoItInterpreter: true,
cancellationToken: ct
);
Logger.LogInformation(
"AutoIT finished. exitCode={ExitCode} timedOut={TimedOut} duration={Duration} logFilePath='{LogFilePath}'",
autoItRunnerResult.ExitCode,
autoItRunnerResult.TimedOut,
autoItRunnerResult.Duration,
autoItRunnerResult.LogFilePath
);
if (autoItRunnerResult.TimedOut)
{
Logger.LogError("AutoIT timed out. duration={Duration} logFilePath='{LogFilePath}'",
autoItRunnerResult.Duration, autoItRunnerResult.LogFilePath);
throw new TimeoutException("AutoIT timed out while selecting file.");
}
if (autoItRunnerResult.ExitCode != 0)
{
Logger.LogError(
"AutoIT failed. exitCode={ExitCode} stderrLength={StdErrLength} stdoutLength={StdOutLength} logFilePath='{LogFilePath}'",
autoItRunnerResult.ExitCode,
autoItRunnerResult.StdErr?.Length ?? 0,
autoItRunnerResult.StdOut?.Length ?? 0,
autoItRunnerResult.LogFilePath
);
throw new InvalidOperationException($"AutoIT failed with exit code {autoItRunnerResult.ExitCode}.");
}
}
catch (Exception ex)
{
Logger.LogError(ex, "AutoIT failed while selecting file. storedImagePath='{StoredImagePath}'", imageMessagePayload.StoredImagePath);
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
"Caption element found. tag={Tag} displayed={Displayed}, enabled={Enabled} aria-label={AriaLabel}",
caption.TagName,
caption.Displayed,
caption.Enabled,
caption.GetAttribute("aria-label")
);
Logger.LogInformation("Typing caption via execCommand (emoji-safe)... captionLength={Length}", imageMessagePayload.Caption.Length);
try
{
SetCaptionViaExecCommand(caption, imageMessagePayload.Caption ?? string.Empty);
Logger.LogDebug("Caption text injected via execCommand.");
}
catch (Exception ex)
{
Logger.LogError(ex, "Failed while setting caption via execCommand.");
throw;
}
Logger.LogInformation("Submitting caption (Enter)...");
caption.SendKeys(Keys.Enter);
Logger.LogDebug("Enter sent to caption element.");
ct.ThrowIfCancellationRequested();
Logger.LogInformation("Step 2/3: Focusing compose box...");
ct.ThrowIfCancellationRequested();
Logger.LogInformation(
"Step 3/3: Sending message completed. length={Length}",
imageMessagePayload.Caption.Length
);
Logger.LogInformation("SendMessageAsync completed successfully.");
}
private void SetCaptionViaExecCommand(IWebElement element, string text)
{
Logger.LogDebug("SetCaptionViaExecCommand started. textLength={Length}", text?.Length ?? 0);
if (Driver is not IJavaScriptExecutor js)
{
Logger.LogError("Driver does not support JavaScript execution. driverType={DriverType}", Driver.GetType().FullName);
throw new NotSupportedException("Driver does not support JavaScript execution.");
}
js.ExecuteScript(@"
const el = arguments[0];
const value = arguments[1] ?? '';
el.focus();
document.execCommand('selectAll', false, null);
document.execCommand('delete', false, null);
document.execCommand('insertText', false, value);
", element, text);
Logger.LogDebug("SetCaptionViaExecCommand completed.");
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

=== FILE: F:\Marketing\Services\OpenChat\OpenChat.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.Login;
using Services.Abstractions.OpenChat;
using Services.Abstractions.Search;
using Services.Login;
using Services.Search;
using Services.Selector;
using Services.Selenium;
using Services.XPath;
namespace Services.OpenChat
{
public class OpenChat : IOpenChat
{
private const string WhatAppMessage = "WhatsApp Web is not logged in. Call LoginAsync() before opening a chat.";
public ILogger<LoginService> Logger { get; }
private readonly ILoginStateChecker _loginChecker;
private readonly ISearchBoxTyper _searchTyper;
private readonly IClicker _chatClicker;
public OpenChat(IWebDriver driver, ILogger<LoginService> logger)
: this(
loginChecker: new LoginStateChecker(
driver: new WebDriverFacade(driver),
selectors: new Selectors(),
logger: logger),
searchTyper: new SearchBoxTyper(
driver: new WebDriverFacade(driver),
selectors: new Selectors(),
logger: logger),
chatClicker: new ChatClicker(
driver: new WebDriverFacade(driver),
selectors: new Selectors(),
xpathBuilder: new ChatXPathBuilder(new XPathLiteralEscaper(logger)),
logger: logger),
logger: logger)
{
}
public OpenChat(
ILoginStateChecker loginChecker,
ISearchBoxTyper searchTyper,
IClicker chatClicker,
ILogger<LoginService> logger)
{
_loginChecker = loginChecker ?? throw new ArgumentNullException(nameof(loginChecker));
_searchTyper = searchTyper ?? throw new ArgumentNullException(nameof(searchTyper));
_chatClicker = chatClicker ?? throw new ArgumentNullException(nameof(chatClicker));
Logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
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
if (!_loginChecker.IsWhatsAppLoggedIn())
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
await _searchTyper.TypeIntoSearchBoxAsync(chatIdentifier, effectiveTimeout, effectivePoll )
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
await _chatClicker.ClickChatByTitleAsync(chatIdentifier, effectiveTimeout, effectivePoll )
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
}
}

=== FILE: F:\Marketing\Services\Search\SearchBoxTyper.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.Search;
using Services.Abstractions.Selector;
using Services.Abstractions.Selenium;
using Services.Login;
namespace Services.Search
{
internal sealed class SearchBoxTyper(
IWebDriverFacade driver,
ISelectors selectors,
ILogger<LoginService> logger) : ISearchBoxTyper
{
public async Task TypeIntoSearchBoxAsync(
string text,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default)
{
logger.LogInformation(
"TypeIntoSearchBoxAsync started. textLength={TextLength}",
text?.Length ?? 0
);
if (string.IsNullOrWhiteSpace(text))
{
logger.LogWarning("TypeIntoSearchBoxAsync aborted: text is null or whitespace.");
throw new ArgumentException("Text cannot be empty.", nameof(text));
}
timeout ??= TimeSpan.FromSeconds(10);
pollInterval ??= TimeSpan.FromMilliseconds(200);
logger.LogInformation(
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
logger.LogDebug(
"Attempt {Attempt}: Locating search input using selector '{Selector}'.",
attempt,
selectors.CssSelectorToFindSearchInput
);
var input = driver
.FindElements(By.CssSelector(selectors.CssSelectorToFindSearchInput))
.FirstOrDefault();
if (input is { Displayed: true, Enabled: true })
{
logger.LogInformation(
"Search input found and ready on attempt {Attempt}. Focusing and typing...",
attempt
);
input.Click();
logger.LogDebug("Clearing existing search input content.");
input.SendKeys(Keys.Control + "a");
input.SendKeys(Keys.Backspace);
logger.LogDebug("Typing search text and submitting.");
input.SendKeys(text);
input.SendKeys(Keys.Enter);
logger.LogInformation("TypeIntoSearchBoxAsync completed successfully.");
return;
}
logger.LogDebug(
"Attempt {Attempt}: Search input not ready (null, hidden, or disabled).",
attempt
);
}
catch (StaleElementReferenceException)
{
logger.LogDebug(
"Attempt {Attempt}: StaleElementReferenceException encountered. Retrying...",
attempt
);
}
catch (InvalidElementStateException)
{
logger.LogDebug(
"Attempt {Attempt}: InvalidElementStateException encountered. Retrying...",
attempt
);
}
catch (Exception ex)
{
logger.LogError(
ex,
"TypeIntoSearchBoxAsync failed unexpectedly on attempt {Attempt}.",
attempt
);
throw;
}
await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
}
logger.LogError(
"TypeIntoSearchBoxAsync timed out after {TimeoutSeconds} seconds.",
timeout.Value.TotalSeconds
);
throw new WebDriverTimeoutException(
$"Search input textbox not available within {timeout.Value.TotalSeconds} seconds."
);
}
}
}

=== FILE: F:\Marketing\Services\Selector\Attachments.cs ===

﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Abstractions.Search;
namespace Services.Selector
{
public class Attachments(IWebDriver driver, ILogger<Attachments> logger) : IAttachments
{
private IWebDriver Driver { get; } = driver;
public ILogger<Attachments> Logger { get; } = logger;
private const string XpathToFindAttachButton =
"
private const string FindPhotosAndVideosOption =
"
"[@aria-label='Photos & videos' or title='Photos & videos' or @data-testid='attach-photos' or .
public IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval)
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
public IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval)
{
Logger.LogInformation(
"Locating 'Photos & videos' option using XPath '{XPath}'...",
FindPhotosAndVideosOption
);
Logger.LogDebug(
"FindPhotosAndVideosOptionButton started. timeout={Timeout} pollingInterval={PollingInterval} xpath='{XPath}'",
timeout, pollingInterval, FindPhotosAndVideosOption
);
var wait = new WebDriverWait(Driver, timeout)
{
PollingInterval = pollingInterval
};
wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
try
{
var result = wait.Until(driver =>
{
var candidates = driver.FindElements(By.XPath(FindPhotosAndVideosOption))
.Where(e => e.Displayed && e.Enabled)
.ToList();
if (candidates.Count == 0)
{
Logger.LogTrace("FindPhotosAndVideosOptionButton: no displayed/enabled candidates yet.");
return null;
}
foreach (var c in candidates)
{
IWebElement clickable = c;
var tag = (clickable.TagName ?? string.Empty).ToLowerInvariant();
if (tag == "span" || tag == "div")
{
var parentButton = TryFindAncestorClickable(clickable);
if (parentButton != null)
{
Logger.LogTrace("FindPhotosAndVideosOptionButton: climbed from tag={Tag} to ancestor tag={AncestorTag}",
c.TagName, parentButton.TagName);
clickable = parentButton;
}
else
{
Logger.LogTrace("FindPhotosAndVideosOptionButton: no clickable ancestor found for tag={Tag}", c.TagName);
}
}
if (clickable.Displayed && clickable.Enabled)
{
Logger.LogDebug(
"FindPhotosAndVideosOptionButton: selected candidate tag={Tag} aria-label={AriaLabel} title={Title}",
clickable.TagName,
clickable.GetAttribute("aria-label"),
clickable.GetAttribute("title")
);
return clickable;
}
}
return null;
});
Logger.LogDebug("FindPhotosAndVideosOptionButton completed. found={Found}", result is not null);
return result;
}
catch (WebDriverTimeoutException ex)
{
Logger.LogError(ex,
"FindPhotosAndVideosOptionButton timed out after {Timeout}. xpath='{XPath}'",
timeout, FindPhotosAndVideosOption
);
throw;
}
}
private IWebElement? TryFindAncestorClickable(IWebElement element)
{
try
{
return element.FindElement(By.XPath(
"ancestor::*[self::button or @role='button' or @role='menuitem'][1]"
));
}
catch (Exception ex)
{
Logger.LogTrace(ex, "TryFindAncestorClickable: no clickable ancestor found.");
return null;
}
}
}
}

=== FILE: F:\Marketing\Services\Selector\Selectors.cs ===

﻿using Services.Abstractions.Selector;
namespace Services.Selector
{
internal sealed class Selectors : ISelectors
{
public string CssSelectorToFindLoggedInMarker => "div[role='textbox'][contenteditable='true']";
public string CssSelectorToFindSearchInput => "div[role='textbox'][contenteditable='true'][aria-label='Search input textbox']";
public string XpathToFindGridcellAncestor => "./ancestor::*[@role='gridcell' or @role='row' or @tabindex][1]";
}
}

=== FILE: F:\Marketing\Services\Selenium\WebDriverFacade.cs ===

﻿using OpenQA.Selenium;
using Services.Abstractions.Selenium;
namespace Services.Selenium
{
internal sealed class WebDriverFacade(IWebDriver driver) : IWebDriverFacade
{
public IReadOnlyCollection<IWebElement> FindElements(By by)
=> driver.FindElements(by);
}
}

=== FILE: F:\Marketing\Services\Url\UrlShort.cs ===

﻿using Application.TrackedLinks;
using Services.Abstractions.Url;
using System.Text;
namespace Services.Url
{
public class UrlShort(ITrackedLink trackedLink) : IUrlShort
{
private readonly ITrackedLink _trackedLink = trackedLink;
public async Task ShortenUrlAsync(
string longUrl,
string key,
CancellationToken ct = default)
{
if (string.IsNullOrWhiteSpace(longUrl))
throw new ArgumentException("longUrl is required.", nameof(longUrl));
if (string.IsNullOrWhiteSpace(key))
throw new ArgumentException("key is required.", nameof(key));
await _trackedLink.UpsertAsync(id: key,targetUrl: longUrl, ct: ct);
}
}
}

=== FILE: F:\Marketing\Services\UrlValidation\HttpValidatorBase.cs ===

﻿using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;
using System.Net;
using System.Text;
namespace Services.UrlValidation;
public abstract class HttpValidatorBase(
HttpClient httpClient,
IOptionsMonitor<UrlOptions> options) : IUrValidator
{
private readonly HttpClient _httpClient = httpClient;
private readonly IOptionsMonitor<UrlOptions> _options = options;
private const string DefaultUserAgent =
"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120 Safari/537.36";
public abstract UrlPlatform Platform { get; }
protected abstract PlatformRules Rules(UrlOptions opt);
public async Task<UrlValidationResult> ValidateAsync(string url, CancellationToken ct = default)
{
if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
return new UrlValidationResult(false, Platform, null, "Invalid absolute URL.");
var opt = _options.CurrentValue;
using var cts = CreateTimeoutCts(opt, ct);
try
{
var headerProbe = await TryFetchHeadersAsync(uri, cts.Token).ConfigureAwait(false);
if (headerProbe.DecisiveResult is not null)
return headerProbe.DecisiveResult;
return await FetchBodySnippetAndEvaluateAsync(uri, opt, cts.Token).ConfigureAwait(false);
}
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
return new UrlValidationResult(false, Platform, null, "Timeout while validating URL.");
}
catch (HttpRequestException ex)
{
return new UrlValidationResult(false, Platform, null, $"Network error: {ex.Message}");
}
catch (Exception ex)
{
return new UrlValidationResult(false, Platform, null, $"Unexpected error: {ex.Message}");
}
}
private async Task<(UrlValidationResult? DecisiveResult, int? StatusCode)> TryFetchHeadersAsync(
Uri uri,
CancellationToken ct)
{
using var headReq = new HttpRequestMessage(HttpMethod.Head, uri);
headReq.Headers.UserAgent.ParseAdd(DefaultUserAgent);
try
{
using var headResp = await _httpClient.SendAsync(
headReq,
HttpCompletionOption.ResponseHeadersRead,
ct).ConfigureAwait(false);
var status = (int)headResp.StatusCode;
if (headResp.StatusCode == HttpStatusCode.Forbidden)
return (null, status);
if ((int)headResp.StatusCode >= 400)
{
return (
new UrlValidationResult(false, Platform, status, $"HTTP {status} {headResp.ReasonPhrase}"),
status);
}
return (null, status);
}
catch (HttpRequestException)
{
return (null, null);
}
}
private async Task<UrlValidationResult> FetchBodySnippetAndEvaluateAsync(
Uri uri,
UrlOptions opt,
CancellationToken ct)
{
using var getReq = new HttpRequestMessage(HttpMethod.Get, uri);
getReq.Headers.UserAgent.ParseAdd(DefaultUserAgent);
using var resp = await _httpClient.SendAsync(
getReq,
HttpCompletionOption.ResponseHeadersRead,
ct).ConfigureAwait(false);
var status = (int)resp.StatusCode;
if ((int)resp.StatusCode >= 400 && resp.StatusCode != HttpStatusCode.Forbidden)
{
return new UrlValidationResult(false, Platform, status, $"HTTP {status} {resp.ReasonPhrase}");
}
var body = await ReadUpToCharsAsync(resp, opt.MaxBodyCharsToScan, ct).ConfigureAwait(false);
var rules = Rules(opt);
foreach (var must in rules.MustContain)
{
if (!body.Contains(must, StringComparison.OrdinalIgnoreCase))
{
return new UrlValidationResult(false, Platform, status, $"Missing required marker: '{must}'");
}
}
foreach (var bad in rules.MustNotContain)
{
if (body.Contains(bad, StringComparison.OrdinalIgnoreCase))
{
return new UrlValidationResult(
false,
Platform,
status,
$"Contains blocked marker: '{bad}'. Evidence: {TakeSnippet(body, bad)}");
}
}
return new UrlValidationResult(true, Platform, status, "Available");
}
private static CancellationTokenSource CreateTimeoutCts(UrlOptions opt, CancellationToken ct)
{
var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, opt.TimeoutSeconds)));
return cts;
}
private static async Task<string> ReadUpToCharsAsync(
HttpResponseMessage resp,
int maxChars,
CancellationToken ct)
{
await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
using var reader = new StreamReader(stream, Encoding.UTF8, true);
var sb = new StringBuilder(Math.Min(maxChars, 64_000));
var buffer = new char[8192];
while (sb.Length < maxChars)
{
var read = await reader.ReadAsync(
buffer.AsMemory(0, Math.Min(buffer.Length, maxChars - sb.Length)), ct)
.ConfigureAwait(false);
if (read <= 0) break;
sb.Append(buffer, 0, read);
}
return sb.ToString();
}
private static string? TakeSnippet(string body, string needle)
{
var idx = body.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
if (idx < 0) return null;
var start = Math.Max(0, idx - 80);
var len = Math.Min(body.Length - start, 200);
return body.Substring(start, len);
}
}

=== FILE: F:\Marketing\Services\UrlValidation\InstagramUrlValidator.cs ===

﻿using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;
namespace Services.UrlValidation
{
public sealed class InstagramUrlValidator(HttpClient httpClient, IOptionsMonitor<UrlOptions> options) : HttpValidatorBase(httpClient, options)
{
public override UrlPlatform Platform => UrlPlatform.Instagram;
protected override PlatformRules Rules(UrlOptions opt) => opt.Instagram;
}
}

=== FILE: F:\Marketing\Services\UrlValidation\TikTokUrlValidator.cs ===

﻿using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;
namespace Services.UrlValidation
{
public sealed class TikTokUrlValidator(HttpClient httpClient, IOptionsMonitor<UrlOptions> options) : HttpValidatorBase(httpClient, options)
{
public override UrlPlatform Platform => UrlPlatform.TikTok;
protected override PlatformRules Rules(UrlOptions opt) => opt.TikTok;
}
}

=== FILE: F:\Marketing\Services\UrlValidation\UrlValidationPipeline.cs ===

﻿using Services.Abstractions.UrlValidation;
namespace Services.UrlValidation
{
public sealed class UrlValidationPipeline(IUrlFactory factory) : IValidationPipeline
{
private readonly IUrlFactory _factory = factory;
public async Task<(bool AllValid, IReadOnlyList<UrlValidationResult> Results)> ValidateAllAsync(
IReadOnlyList<string> urls,
CancellationToken ct = default)
{
var results = new List<UrlValidationResult>(urls.Count);
foreach (var url in urls)
{
var validator = _factory.GetValidator(url);
var result = await validator.ValidateAsync(url, ct);
results.Add(result);
if (!result.IsValid)
return (false, results);
}
return (true, results);
}
}
}

=== FILE: F:\Marketing\Services\UrlValidation\YouTubeUrlValidator.cs ===

﻿using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;
namespace Services.UrlValidation
{
public sealed class YouTubeUrlValidator(HttpClient httpClient, IOptionsMonitor<UrlOptions> options) : HttpValidatorBase(httpClient, options)
{
public override UrlPlatform Platform => UrlPlatform.YouTube;
protected override PlatformRules Rules(UrlOptions opt) => opt.YouTube;
}
}

=== FILE: F:\Marketing\Services\WhatsApp\ScheduledMessenger.cs ===

﻿using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Configuration;
using Services.Abstractions.Login;
namespace Services.WhatsApp
{
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
var sender = scope.ServiceProvider.GetRequiredService<IMessage>();
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
if (tz.IsInvalidTime(localUnspecified))
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
}

=== FILE: F:\Marketing\Services\XPath\ChatXPathBuilder.cs ===

﻿using Services.Abstractions.XPath;
namespace Services.XPath
{
internal sealed class ChatXPathBuilder(IXPathLiteralEscaper escaper) : IChatXPathBuilder
{
public string GetXpathToFind(string needleLowerInvariant)
{
return $"
}
}
}

=== FILE: F:\Marketing\Services\XPath\XPathLiteralEscaper.cs ===

﻿using Microsoft.Extensions.Logging;
using Services.Abstractions.XPath;
using Services.Login;
namespace Services.XPath
{
internal sealed class XPathLiteralEscaper(ILogger<LoginService> logger) : IXPathLiteralEscaper
{
public string EscapeXPathLiteral(string value)
{
if (value is null)
{
throw new ArgumentNullException(nameof(value));
}
logger.LogDebug(
"EscapeXPathLiteral started. valueLength={ValueLength}",
value.Length
);
if (!value.Contains("'"))
{
logger.LogDebug("EscapeXPathLiteral: Using single-quoted XPath literal.");
return $"'{value}'";
}
if (!value.Contains("\""))
{
logger.LogDebug("EscapeXPathLiteral: Using double-quoted XPath literal.");
return $"\"{value}\"";
}
logger.LogDebug("EscapeXPathLiteral: Using concat() XPath literal strategy.");
var parts = value.Split('\'');
var partsString = string.Join(", \"'\", ", parts.Select(p => $"'{p}'"));
var result = "concat(" + partsString + ")";
logger.LogDebug(
"EscapeXPathLiteral completed. partCount={PartCount}",
parts.Length
);
return result;
}
}
}

=== FILE: F:\Marketing\Services\YouTube\YouTubeService.cs ===

﻿using Application.Result;
using Configuration.YouTube;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions.YouTube;
using System.Net;
using System.Text.Json;
namespace Services.YouTube
{
public sealed class YouTubeService(
HttpClient httpClient,
IOptions<YouTubeApiOptions> options,
IErrorHandler errorHandler,
ILogger<YouTubeService> logger
) : IYouTubeService
{
private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
private readonly YouTubeApiOptions _cfg = options?.Value ?? throw new ArgumentNullException(nameof(options));
private readonly IErrorHandler _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
private readonly ILogger<YouTubeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
private static readonly JsonSerializerOptions JsonOptions = new()
{
PropertyNameCaseInsensitive = true
};
public async Task<Operation<SearchResponse>> SearchVideosAsync(string query, SearchOptions options)
{
var requestId = Guid.NewGuid().ToString("N");
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
["Service"] = nameof(YouTubeService),
["Method"] = nameof(SearchVideosAsync),
["RequestId"] = requestId
});
_logger.LogInformation("STEP 0: Enter SearchVideosAsync. QueryLen={Len}", query?.Length ?? 0);
try
{
_logger.LogDebug("STEP 1: Validating inputs...");
if (string.IsNullOrWhiteSpace(query))
{
_logger.LogWarning("STEP 1: Validation failed. Reason=EmptyQuery");
return _errorHandler.Business<SearchResponse>("Query cannot be null or empty.");
}
if (options is null)
{
_logger.LogWarning("STEP 1: Validation failed. Reason=NullSearchOptions");
return _errorHandler.Business<SearchResponse>("SearchOptions cannot be null.");
}
if (string.IsNullOrWhiteSpace(_cfg.ApiKey))
{
_logger.LogError("STEP 1: Validation failed. Reason=MissingApiKey");
return _errorHandler.Fail<SearchResponse>(
new InvalidOperationException("YouTube API key is missing."),
"YouTube API key is not configured.");
}
_logger.LogDebug("STEP 1: Validation passed.");
_logger.LogDebug("STEP 2: Normalizing defaults...");
var maxResults = options.MaxResults <= 0 ? 10 : options.MaxResults;
var order = string.IsNullOrWhiteSpace(options.Order) ? "viewCount" : options.Order;
var safeSearch = string.IsNullOrWhiteSpace(options.SafeSearch) ? "none" : options.SafeSearch;
_logger.LogInformation(
"STEP 2: Defaults applied. MaxResults={MaxResults}, Order={Order}, SafeSearch={SafeSearch}, Region={Region}, Lang={Lang}, After={After}, Before={Before}",
maxResults,
order,
safeSearch,
options.RegionCode,
options.RelevanceLanguage,
options.PublishedAfterIso,
options.PublishedBeforeIso);
_logger.LogDebug("STEP 3: Building request URI (redacted)...");
var uri = BuildSearchUri(query, options, maxResults, order, safeSearch);
_logger.LogInformation("STEP 3: URI built (redacted). Endpoint=search?part=snippet&type=video&...");
_logger.LogInformation("STEP 4: Calling YouTube API via HttpClient...");
using var resp = await _httpClient.GetAsync(uri);
_logger.LogInformation("STEP 4: HTTP call completed. StatusCode={StatusCode} ({StatusInt})",
resp.StatusCode, (int)resp.StatusCode);
_logger.LogDebug("STEP 5: Reading response body...");
var body = await resp.Content.ReadAsStringAsync();
_logger.LogInformation("STEP 5: Body read. BodyLen={BodyLen}", body?.Length ?? 0);
if (resp.StatusCode == HttpStatusCode.Forbidden || resp.StatusCode == HttpStatusCode.Unauthorized)
{
_logger.LogWarning(
"STEP 6: Auth/quota failure detected. StatusCode={StatusCode} ({StatusInt})",
resp.StatusCode, (int)resp.StatusCode);
return _errorHandler.Fail<SearchResponse>(
new InvalidOperationException(
$"YouTube API auth/quota failure. Status={(int)resp.StatusCode}. Body={body}"),
"YouTube API rejected the request (auth/quota).");
}
if (!resp.IsSuccessStatusCode)
{
_logger.LogWarning(
"STEP 7: Non-success status detected. StatusCode={StatusCode} ({StatusInt})",
resp.StatusCode, (int)resp.StatusCode);
return _errorHandler.Fail<SearchResponse>(
new HttpRequestException(
$"YouTube API error. Status={(int)resp.StatusCode}. Body={body}"),
"YouTube API returned non-success status.");
}
_logger.LogDebug("STEP 8: Deserializing JSON response...");
YouTubeSearchListDto? dto;
try
{
dto = JsonSerializer.Deserialize<YouTubeSearchListDto>(body, JsonOptions);
}
catch (JsonException jex)
{
_logger.LogError(jex, "STEP 8: JSON deserialization failed.");
return _errorHandler.Fail<SearchResponse>(jex, "Failed to parse YouTube response.");
}
var rawItems = dto?.Items?.Count ?? 0;
_logger.LogInformation("STEP 8: JSON deserialized. RawItems={RawItems}, NextPageTokenPresent={HasToken}",
rawItems,
!string.IsNullOrWhiteSpace(dto?.NextPageToken));
_logger.LogDebug("STEP 9: Mapping DTO -> SearchResponse...");
var items =
(dto?.Items ?? [])
.Where(x => x?.Id?.VideoId is not null)
.Select(x => new SearchVideoItem(
VideoId: x!.Id!.VideoId!,
ChannelId: x.Snippet?.ChannelId ?? string.Empty,
Title: x.Snippet?.Title ?? string.Empty,
Description: x.Snippet?.Description ?? string.Empty,
PublishedAt: x.Snippet?.PublishedAt ?? DateTimeOffset.MinValue,
ThumbnailUrl: x.Snippet?.Thumbnails?.High?.Url
?? x.Snippet?.Thumbnails?.Medium?.Url
?? x.Snippet?.Thumbnails?.Default?.Url
))
.ToList();
var mapped = new SearchResponse(
Query: query,
Items: items,
NextPageToken: dto?.NextPageToken
);
_logger.LogInformation(
"STEP 9: Mapping completed. MappedItems={Count}",
mapped.Items.Count);
_logger.LogInformation("STEP 10: Returning success.");
return Operation<SearchResponse>.Success(mapped, "Search completed");
}
catch (Exception ex)
{
_logger.LogError(ex, "STEP X: Unhandled exception in SearchVideosAsync.");
return _errorHandler.Fail<SearchResponse>(ex, "SearchVideosAsync failed.");
}
}
public Task<Operation<VideoDetails>> GetVideoDetailsAsync(string videoId)
{
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
["Service"] = nameof(YouTubeService),
["Method"] = nameof(GetVideoDetailsAsync)
});
_logger.LogInformation("STEP 0: Enter GetVideoDetailsAsync. VideoIdEmpty={Empty}", string.IsNullOrWhiteSpace(videoId));
try
{
_logger.LogDebug("STEP 1: Validating inputs...");
if (string.IsNullOrWhiteSpace(videoId))
{
_logger.LogWarning("STEP 1: Validation failed. Reason=EmptyVideoId");
return Task.FromResult(_errorHandler.Business<VideoDetails>("VideoId cannot be null or empty."));
}
if (string.IsNullOrWhiteSpace(_cfg.ApiKey))
{
_logger.LogError("STEP 1: Validation failed. Reason=MissingApiKey");
return Task.FromResult(_errorHandler.Fail<VideoDetails>(
new InvalidOperationException("YouTube API key is missing."),
"YouTube API key is not configured."));
}
_logger.LogWarning("STEP 2: Not implemented.");
return Task.FromResult(_errorHandler.Business<VideoDetails>("GetVideoDetailsAsync is not implemented yet."));
}
catch (Exception ex)
{
_logger.LogError(ex, "STEP X: Unhandled exception in GetVideoDetailsAsync.");
return Task.FromResult(_errorHandler.Fail<VideoDetails>(ex, "GetVideoDetailsAsync failed."));
}
}
public Task<Operation<ChannelDetails>> GetChannelDetailsAsync(string channelId)
{
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
["Service"] = nameof(YouTubeService),
["Method"] = nameof(GetChannelDetailsAsync)
});
_logger.LogInformation("STEP 0: Enter GetChannelDetailsAsync. ChannelIdEmpty={Empty}", string.IsNullOrWhiteSpace(channelId));
try
{
_logger.LogDebug("STEP 1: Validating inputs...");
if (string.IsNullOrWhiteSpace(channelId))
{
_logger.LogWarning("STEP 1: Validation failed. Reason=EmptyChannelId");
return Task.FromResult(_errorHandler.Business<ChannelDetails>("ChannelId cannot be null or empty."));
}
if (string.IsNullOrWhiteSpace(_cfg.ApiKey))
{
_logger.LogError("STEP 1: Validation failed. Reason=MissingApiKey");
return Task.FromResult(_errorHandler.Fail<ChannelDetails>(
new InvalidOperationException("YouTube API key is missing."),
"YouTube API key is not configured."));
}
_logger.LogWarning("STEP 2: Not implemented.");
return Task.FromResult(_errorHandler.Business<ChannelDetails>("GetChannelDetailsAsync is not implemented yet."));
}
catch (Exception ex)
{
_logger.LogError(ex, "STEP X: Unhandled exception in GetChannelDetailsAsync.");
return Task.FromResult(_errorHandler.Fail<ChannelDetails>(ex, "GetChannelDetailsAsync failed."));
}
}
private string BuildSearchUri(string query, SearchOptions options, int maxResults, string order, string safeSearch)
{
return
$"search?part=snippet" +
$"&type=video" +
$"&q={Uri.EscapeDataString(query)}" +
$"&maxResults={maxResults}" +
$"&order={Uri.EscapeDataString(order)}" +
$"&safeSearch={Uri.EscapeDataString(safeSearch)}" +
$"{(string.IsNullOrWhiteSpace(options.RegionCode) ? "" : $"&regionCode={Uri.EscapeDataString(options.RegionCode)}")}" +
$"{(string.IsNullOrWhiteSpace(options.RelevanceLanguage) ? "" : $"&relevanceLanguage={Uri.EscapeDataString(options.RelevanceLanguage)}")}" +
$"{(string.IsNullOrWhiteSpace(options.PublishedAfterIso) ? "" : $"&publishedAfter={Uri.EscapeDataString(options.PublishedAfterIso)}")}" +
$"{(string.IsNullOrWhiteSpace(options.PublishedBeforeIso) ? "" : $"&publishedBefore={Uri.EscapeDataString(options.PublishedBeforeIso)}")}" +
$"&key={Uri.EscapeDataString(_cfg.ApiKey)}";
}
}
public sealed class YouTubeSearchListDto
{
public string? NextPageToken { get; set; }
public List<YouTubeSearchItemDto>? Items { get; set; }
}
public sealed class YouTubeSearchItemDto
{
public YouTubeSearchIdDto? Id { get; set; }
public YouTubeSearchSnippetDto? Snippet { get; set; }
}
public sealed class YouTubeSearchIdDto
{
public string? VideoId { get; set; }
}
public sealed class YouTubeSearchSnippetDto
{
public DateTimeOffset? PublishedAt { get; set; }
public string? ChannelId { get; set; }
public string? Title { get; set; }
public string? Description { get; set; }
public YouTubeThumbnailsDto? Thumbnails { get; set; }
}
public sealed class YouTubeThumbnailsDto
{
public YouTubeThumbnailDto? Default { get; set; }
public YouTubeThumbnailDto? Medium { get; set; }
public YouTubeThumbnailDto? High { get; set; }
}
public sealed class YouTubeThumbnailDto
{
public string? Url { get; set; }
}
}

=== FILE: F:\Marketing\Services\YouTube\YouTubeViralVideoDiscoverer.cs ===

﻿using Application.Result;
using Configuration.YouTube;
using Microsoft.Extensions.Logging;
using Services.Abstractions.UrlValidation;
using Services.Abstractions.YouTube;
namespace Services.YouTube
{
public sealed class YouTubeViralVideoDiscoverer(
IYouTubeService youTubeService,
IUrlFactory urlFactory,
ILogger<YouTubeViralVideoDiscoverer> logger
) : IYouTubeViralVideoDiscoverer
{
public async Task<Operation<List<EnrichedVideoData>>> DiscoverAsync(
string query, SearchOptions options, CancellationToken ct = default)
{
var searchOp = await youTubeService.SearchVideosAsync(query, options);
if (!searchOp.IsSuccessful || searchOp.Data is null)
return searchOp.ConvertTo<List<EnrichedVideoData>>();
var enriched = new List<EnrichedVideoData>();
foreach (var item in searchOp.Data.Items)
{
ct.ThrowIfCancellationRequested();
var url = $"https://www.youtube.com/watch?v={item.VideoId}";
var validator = urlFactory.GetValidator(url);
var validOp = await validator.ValidateAsync(url, ct);
if (validOp is not null)
{
if (!validOp.IsValid) continue;
}
var videoOp = await youTubeService.GetVideoDetailsAsync(item.VideoId);
if (!videoOp.IsSuccessful || videoOp.Data is null) continue;
var channelOp = await youTubeService.GetChannelDetailsAsync(item.ChannelId);
if (!channelOp.IsSuccessful || channelOp.Data is null) continue;
enriched.Add(Enrich(videoOp.Data, channelOp.Data, url));
}
return Operation<List<EnrichedVideoData>>.Success(enriched, "Discovery completed");
}
private static EnrichedVideoData Enrich(VideoDetails v, ChannelDetails c, string url)
{
var age = DateTimeOffset.UtcNow - v.PublishedAt;
var viewsPerHour = age.TotalHours <= 0.01 ? v.Statistics.ViewCount : v.Statistics.ViewCount / age.TotalHours;
var likeView = v.Statistics.ViewCount == 0 ? 0 : (double)v.Statistics.LikeCount / v.Statistics.ViewCount;
var commentView = v.Statistics.ViewCount == 0 ? 0 : (double)v.Statistics.CommentCount / v.Statistics.ViewCount;
var engagementScore = (likeView * 0.7) + (commentView * 0.3);
var velocityBucket =
viewsPerHour >= 50_000 ? "Exploding" :
viewsPerHour >= 10_000 ? "Rising" :
"Steady";
var channelAge = DateTimeOffset.UtcNow - c.PublishedAt;
var channelViewsPerVideo = c.VideoCount == 0 ? 0 : (double)c.ViewCount / c.VideoCount;
return new EnrichedVideoData(
v.VideoId,
url,
v.Title,
v.Description,
v.Duration,
v.PublishedAt,
v.Statistics.ViewCount,
v.Statistics.LikeCount,
v.Statistics.CommentCount,
likeView,
commentView,
engagementScore,
age,
viewsPerHour,
velocityBucket,
v.Tags,
v.CategoryId,
KeywordExtract(v.Description),
c.ChannelId,
c.Title,
c.SubscriberCount,
channelAge,
channelViewsPerVideo
);
}
private static IReadOnlyList<string> KeywordExtract(string text)
{
return [.. text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
.Select(t => t.Trim().ToLowerInvariant())
.Where(t => t.Length >= 5)
.Distinct()
.Take(25)];
}
}
}

=== FILE: F:\Marketing\Services.Abstractions\AutoIt\IAutoItRunner.cs ===

﻿using Domain;
namespace Services.Abstractions.AutoIt
{
public interface IAutoItRunner
{
public Task<AutoItRunnerResult> RunAsync(
TimeSpan timeout,
string imagePath,
bool useAutoItInterpreter = false,
CancellationToken cancellationToken = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Check\ICaptureSnapshot.cs ===

﻿namespace Services.Abstractions.Check
{
public interface ICaptureSnapshot
{
Task<string> CaptureArtifactsAsync(string executionFolder, string stage);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Check\IDirectoryCheck.cs ===

﻿namespace Services.Abstractions.Check
{
public interface IDirectoryCheck
{
void EnsureDirectoryExists(string path);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Check\ISecurityCheck.cs ===

﻿namespace Services.Abstractions.Check
{
public interface ISecurityCheck
{
bool IsSecurityCheck();
Task TryStartPuzzle();
Task HandleSecurityPage();
Task HandleUnexpectedPage();
}
}

=== FILE: F:\Marketing\Services.Abstractions\Check\IWebDriverFactory.cs ===

﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace Services.Abstractions.Check
{
public interface IWebDriverFactory
{
IWebDriver Create(bool hide = false);
IWebDriver Create(Action<ChromeOptions> configureOptions);
ChromeOptions GetDefaultOptions(string downloadFolder);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Login\ILoginService.cs ===

﻿namespace Services.Abstractions.Login
{
public interface ILoginService
{
Task LoginAsync(CancellationToken cancellationToken = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Login\ILoginStateChecker.cs ===

﻿namespace Services.Abstractions.Login
{
public interface ILoginStateChecker
{
bool IsWhatsAppLoggedIn();
}
}

=== FILE: F:\Marketing\Services.Abstractions\Login\IMessage.cs ===

﻿namespace Services.Abstractions.Login
{
public interface IMessage
{
Task LoginAsync();
Task SendMessageAsync();
}
}

=== FILE: F:\Marketing\Services.Abstractions\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Services.Abstractions\obj\Debug\net8.0\Services.Abstractions.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Services.Abstractions")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
[assembly: System.Reflection.AssemblyProductAttribute("Services.Abstractions")]
[assembly: System.Reflection.AssemblyTitleAttribute("Services.Abstractions")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Services.Abstractions\obj\Debug\net8.0\Services.Abstractions.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Services.Abstractions\OpenAI\IOpenAIClient.cs ===

﻿using Domain.OpenAI;
namespace Services.Abstractions.OpenAI
{
public interface IOpenAIClient
{
Task<string> GetChatCompletionAsync(Prompt prompt, CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\OpenAI\news\IJsonPromptRunner.cs ===

﻿namespace Services.Abstractions.OpenAI.news
{
public interface IJsonPromptRunner
{
Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\OpenAI\news\INewsHistoryStore.cs ===

﻿namespace Services.Abstractions.OpenAI.news
{
public interface INewsHistoryStore
{
Task<HashSet<string>> LoadUrlsAsync(CancellationToken ct = default);
Task AppendUsedUrlsAsync(IEnumerable<string> urls, CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\OpenAI\news\INostalgiaPromptLoader.cs ===

﻿using Domain;
using Prompts.NostalgiaRank;
namespace Services.Abstractions.OpenAI.news
{
public interface INostalgiaPromptLoader
{
Task<NostalgiaRankPrompt> LoadPromptAsync();
}
}

=== FILE: F:\Marketing\Services.Abstractions\OpenChat\IChatService.cs ===

﻿using Domain;
namespace Services.Abstractions.OpenChat
{
public interface IChatService
{
Task SendMessageAsync(
ImageMessagePayload imageMessagePayload,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default
);
}
}

=== FILE: F:\Marketing\Services.Abstractions\OpenChat\IClicker.cs ===

﻿namespace Services.Abstractions.OpenChat
{
public interface IClicker
{
Task ClickChatByTitleAsync(
string chatTitle,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\OpenChat\IOpenChat.cs ===

﻿namespace Services.Abstractions.OpenChat
{
public interface IOpenChat
{
Task OpenContactChatAsync(
string chatIdentifier,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default
);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Search\IAttachments.cs ===

﻿using OpenQA.Selenium;
namespace Services.Abstractions.Search
{
public interface IAttachments
{
IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval);
IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Search\ISearchBoxTyper.cs ===

﻿namespace Services.Abstractions.Search
{
public interface ISearchBoxTyper
{
Task TypeIntoSearchBoxAsync(
string text,
TimeSpan? timeout = null,
TimeSpan? pollInterval = null,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Selector\ISelectors.cs ===

﻿namespace Services.Abstractions.Selector
{
public interface ISelectors
{
string CssSelectorToFindLoggedInMarker { get; }
string CssSelectorToFindSearchInput { get; }
string XpathToFindGridcellAncestor { get; }
}
}

=== FILE: F:\Marketing\Services.Abstractions\Selenium\IWebDriverFacade.cs ===

﻿using OpenQA.Selenium;
namespace Services.Abstractions.Selenium
{
public interface IWebDriverFacade
{
IReadOnlyCollection<IWebElement> FindElements(By by);
}
}

=== FILE: F:\Marketing\Services.Abstractions\Url\IUrlShort.cs ===

﻿namespace Services.Abstractions.Url
{
public interface IUrlShort
{
Task ShortenUrlAsync(string longUrl, string key, CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\IPlatformResolver.cs ===

﻿namespace Services.Abstractions.UrlValidation
{
public interface IPlatformResolver
{
UrlPlatform Resolve(string url);
}
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\IUrlFactory.cs ===

﻿namespace Services.Abstractions.UrlValidation
{
public interface IUrlFactory
{
IUrValidator GetValidator(string url);
}
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\IUrValidator.cs ===

﻿namespace Services.Abstractions.UrlValidation
{
public interface IUrValidator
{
UrlPlatform Platform { get; }
Task<UrlValidationResult> ValidateAsync(string url, CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\IValidationPipeline.cs ===

﻿namespace Services.Abstractions.UrlValidation
{
public interface IValidationPipeline
{
Task<(bool AllValid, IReadOnlyList<UrlValidationResult> Results)> ValidateAllAsync(
IReadOnlyList<string> urls,
CancellationToken ct = default
);
}
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\PlatformResolver.cs ===

﻿namespace Services.Abstractions.UrlValidation
{
public sealed class PlatformResolver : IPlatformResolver
{
public UrlPlatform Resolve(string url)
{
if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
return UrlPlatform.Unknown;
var host = uri.Host.ToLowerInvariant();
if (host.Contains("youtube.com") || host.Contains("youtu.be"))
return UrlPlatform.YouTube;
if (host.Contains("tiktok.com"))
return UrlPlatform.TikTok;
if (host.Contains("instagram.com"))
return UrlPlatform.Instagram;
return UrlPlatform.Unknown;
}
}
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\UrlPlatform.cs ===

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Services.Abstractions.UrlValidation
{
public enum UrlPlatform
{
Unknown = 0,
YouTube = 1,
TikTok = 2,
Instagram = 3
}
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\UrlValidationResult.cs ===

﻿namespace Services.Abstractions.UrlValidation
{
public sealed record UrlValidationResult(
bool IsValid,
UrlPlatform Platform,
int? HttpStatusCode,
string? FailureReason,
string? EvidenceSnippet = null
);
}

=== FILE: F:\Marketing\Services.Abstractions\UrlValidation\UrlValidatorFactory.cs ===

﻿namespace Services.Abstractions.UrlValidation
{
public sealed class UrlValidatorFactory(
IPlatformResolver resolver,
IEnumerable<IUrValidator> validators) : IUrlFactory
{
private readonly IPlatformResolver _resolver = resolver;
private readonly IReadOnlyDictionary<UrlPlatform, IUrValidator> _validators = validators.ToDictionary(v => v.Platform, v => v);
public IUrValidator GetValidator(string url)
{
var platform = _resolver.Resolve(url);
if (platform == UrlPlatform.Unknown || !_validators.TryGetValue(platform, out var validator))
{
var message = $"No validator registered for platform '{platform}'.";
throw new NotSupportedException(message);
}
return validator;
}
}
}

=== FILE: F:\Marketing\Services.Abstractions\XPath\IChatXPathBuilder.cs ===

﻿namespace Services.Abstractions.XPath
{
public interface IChatXPathBuilder
{
string GetXpathToFind(string needleLowerInvariant);
}
}

=== FILE: F:\Marketing\Services.Abstractions\XPath\IXPathLiteralEscaper.cs ===

﻿namespace Services.Abstractions.XPath
{
public interface IXPathLiteralEscaper
{
string EscapeXPathLiteral(string value);
}
}

=== FILE: F:\Marketing\Services.Abstractions\YouTube\ChannelDetails.cs ===

﻿namespace Services.Abstractions.YouTube
{
public sealed record ChannelDetails(
string ChannelId,
string Title,
DateTimeOffset PublishedAt,
long SubscriberCount,
long VideoCount,
long ViewCount
);
}

=== FILE: F:\Marketing\Services.Abstractions\YouTube\EnrichedVideoData.cs ===

﻿namespace Services.Abstractions.YouTube
{
public sealed record EnrichedVideoData(
string VideoId,
string VideoUrl,
string Title,
string Description,
TimeSpan? Duration,
DateTimeOffset PublishedAt,
long Views,
long Likes,
long Comments,
double LikeViewRatio,
double CommentViewRatio,
double EngagementScore,
TimeSpan Age,
double ViewsPerHour,
string VelocityBucket,
IReadOnlyList<string> Tags,
string? CategoryId,
IReadOnlyList<string> DescriptionKeywords,
string ChannelId,
string ChannelTitle,
long SubscriberCount,
TimeSpan ChannelAge,
double ChannelViewsPerVideo
);
}

=== FILE: F:\Marketing\Services.Abstractions\YouTube\SearchOptions.cs ===

﻿using Application.Result;
using Configuration.YouTube;
namespace Services.Abstractions.YouTube
{
public interface IYouTubeService
{
Task<Operation<SearchResponse>> SearchVideosAsync(string query, SearchOptions options);
Task<Operation<VideoDetails>> GetVideoDetailsAsync(string videoId);
Task<Operation<ChannelDetails>> GetChannelDetailsAsync(string channelId);
}
public interface IYouTubeViralVideoDiscoverer
{
Task<Operation<List<EnrichedVideoData>>> DiscoverAsync(
string query,
SearchOptions options,
CancellationToken ct = default);
}
}

=== FILE: F:\Marketing\Services.Abstractions\YouTube\SearchResponse.cs ===

﻿namespace Services.Abstractions.YouTube
{
public sealed record SearchResponse(
string Query,
IReadOnlyList<SearchVideoItem> Items,
string? NextPageToken
);
public sealed record SearchVideoItem(
string VideoId,
string ChannelId,
string Title,
string Description,
DateTimeOffset PublishedAt,
string? ThumbnailUrl
);
}

=== FILE: F:\Marketing\Services.Abstractions\YouTube\VideoDetails.cs ===

﻿namespace Services.Abstractions.YouTube
{
public sealed record VideoDetails(
string VideoId,
string ChannelId,
string Title,
string Description,
DateTimeOffset PublishedAt,
TimeSpan? Duration,
IReadOnlyList<string> Tags,
string? CategoryId,
VideoStatistics Statistics
);
public sealed record VideoStatistics(
long ViewCount,
long LikeCount,
long CommentCount
);
}

=== FILE: F:\Marketing\Tools\Program.cs ===

﻿using Application.Result;
using Configuration;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Implementation;
using Persistence.Context.Interceptors;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Serilog;
using System.Data.Common;
namespace Tools
{
public sealed class Program
{
private const string ConnectionMissingMessage =
"Connection string 'DefaultConnection' is missing or empty.";
private const string OutputTemplate =
"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";
public static void Main(string[] args)
{
var builder = WebApplication.CreateBuilder(args);
var appConfig = new AppConfig();
builder.Configuration.Bind(appConfig);
builder.Services.AddSingleton(appConfig);
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
var logRoot = TryGetExecutionRunning(services) ?? context.HostingEnvironment.ContentRootPath;
var logPath = Path.Combine(logRoot, "Logs");
Directory.CreateDirectory(logPath);
loggerConfig
.MinimumLevel.Debug()
.WriteTo.Console()
.WriteTo.File(
path: Path.Combine(logPath, "Redirect-.log"),
rollingInterval: RollingInterval.Day,
fileSizeLimitBytes: 5_000_000,
retainedFileCountLimit: 7,
rollOnFileSizeLimit: true,
outputTemplate: OutputTemplate
);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
AddDbContextSQLite(builder);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());
builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
builder.Services.AddScoped<IErrorLogger, SerilogErrorLogger>();
builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
builder.Services.AddSingleton<IColumnTypes, SQLite>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<DataContext>();
if (!db.Initialize())
{
throw new Exception("Database initialization failed");
}
var context = scope.ServiceProvider.GetRequiredService<IErrorHandler>();
if (!context.Any())
{
var errorHandler = scope.ServiceProvider.GetRequiredService<IErrorHandler>();
errorHandler.LoadErrorMappings("ErrorMappings.json");
}
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
}
public static void EnsureDatabaseFolderExists(string connectionString)
{
if (string.IsNullOrWhiteSpace(connectionString))
throw new ArgumentException("Connection string is null or empty.", nameof(connectionString));
var builder = new DbConnectionStringBuilder
{
ConnectionString = connectionString
};
if (!builder.TryGetValue("Data Source", out var dataSourceObj))
throw new InvalidOperationException("Connection string does not contain 'Data Source'.");
var dbFilePath = dataSourceObj.ToString();
if (string.IsNullOrWhiteSpace(dbFilePath))
throw new InvalidOperationException("Data Source path is empty.");
var directoryPath = Path.GetDirectoryName(dbFilePath);
if (string.IsNullOrWhiteSpace(directoryPath))
throw new InvalidOperationException("Could not determine database directory path.");
if (!Directory.Exists(directoryPath))
{
Directory.CreateDirectory(directoryPath);
}
}
private static void AddDbContextSQLite(WebApplicationBuilder builder)
{
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
throw new ArgumentNullException(nameof(connectionString), ConnectionMissingMessage);
EnsureDatabaseFolderExists(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
{
options
.UseSqlite(connectionString, sqlite =>
{
var migrationsAssembly = typeof(DataContext).Assembly.GetName().Name;
sqlite.MigrationsAssembly(migrationsAssembly);
})
.AddInterceptors(new SqliteFunctionInterceptor());
});
}
private static string? TryGetExecutionRunning(IServiceProvider services)
{
try
{
var tracker = services.GetService(typeof(ExecutionTracker)) as ExecutionTracker;
return tracker?.ExecutionRunning;
}
catch
{
return null;
}
}
}
}

=== FILE: F:\Marketing\Tools\WeatherForecast.cs ===

namespace roots
{
public class WeatherForecast
{
public DateOnly Date { get; set; }
public int TemperatureC { get; set; }
public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
public string? Summary { get; set; }
}
}

=== FILE: F:\Marketing\Tools\Controllers\WeatherForecastController.cs ===

using Microsoft.AspNetCore.Mvc;
namespace roots.Controllers
{
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
private static readonly string[] Summaries = new[]
{
"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
private readonly ILogger<WeatherForecastController> _logger;
public WeatherForecastController(ILogger<WeatherForecastController> logger)
{
_logger = logger;
}
[HttpGet(Name = "GetWeatherForecast")]
public IEnumerable<WeatherForecast> Get()
{
return Enumerable.Range(1, 5).Select(index => new WeatherForecast
{
Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
TemperatureC = Random.Shared.Next(-20, 55),
Summary = Summaries[Random.Shared.Next(Summaries.Length)]
})
.ToArray();
}
}
}

=== FILE: F:\Marketing\Tools\Controllers\api\v1\Redirect\TrackedLinkController.cs ===

﻿using Application.WhatsApp.UseCases.Repository.CRUD;
using Domain.WhatsApp.Redirect;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers.v1.Redirect
{
[ApiController]
[Route("api/v1/tracked-links")]
public sealed class TrackedLinkController(
ITrackedLinkCreate trackedLinkCreate,
ITrackedLinkRead trackedLinkRead) : ControllerBase
{
private readonly ITrackedLinkCreate _create = trackedLinkCreate;
private readonly ITrackedLinkRead _read = trackedLinkRead;
[HttpPost]
[ProducesResponseType(typeof(TrackedLink), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] TrackedLink trackedLink)
{
if (!ModelState.IsValid)
return BadRequest(ModelState);
var op = await _create.CreateAsync(trackedLink);
if (!op.IsSuccessful)
return BadRequest(op.Message);
return CreatedAtAction(nameof(RedirectById), new { id = trackedLink.Id }, trackedLink);
}
[HttpGet("{id}")]
[ProducesResponseType(StatusCodes.Status302Found)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> RedirectById([FromRoute] string id)
{
if (string.IsNullOrWhiteSpace(id))
return BadRequest("id is required.");
var op = await _read.ReadAsync(id);
if (!op.IsSuccessful || op.Data is null)
return NotFound(op.Message);
var link = op.Data.FirstOrDefault();
string targetUrl = link.TargetUrl;
if (string.IsNullOrWhiteSpace(targetUrl))
return NotFound("TrackedLink has no OriginalUrl.");
return Redirect(targetUrl);
}
[HttpGet("{id}/meta")]
[ProducesResponseType(typeof(TrackedLink), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> ReadMeta([FromRoute] string id)
{
var op = await _read.ReadAsync(id);
if (!op.IsSuccessful || op.Data is null)
return NotFound(op.Message);
return Ok(op.Data);
}
}
}

=== FILE: F:\Marketing\Tools\obj\Debug\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Tools\obj\Debug\net8.0\Tools.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Tools")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+a2c56a6c80eede8b29c33340169ba755fc45ba06")]
[assembly: System.Reflection.AssemblyProductAttribute("Tools")]
[assembly: System.Reflection.AssemblyTitleAttribute("Tools")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Tools\obj\Debug\net8.0\Tools.GlobalUsings.g.cs ===

global using global::Microsoft.AspNetCore.Builder;
global using global::Microsoft.AspNetCore.Hosting;
global using global::Microsoft.AspNetCore.Http;
global using global::Microsoft.AspNetCore.Routing;
global using global::Microsoft.Extensions.Configuration;
global using global::Microsoft.Extensions.DependencyInjection;
global using global::Microsoft.Extensions.Hosting;
global using global::Microsoft.Extensions.Logging;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Net.Http.Json;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Tools\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\Tools\obj\Release\net8.0\Tools.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("Tools")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+686d4a9fd23366c79861334aafe6e4c8aa0b6377")]
[assembly: System.Reflection.AssemblyProductAttribute("Tools")]
[assembly: System.Reflection.AssemblyTitleAttribute("Tools")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\Tools\obj\Release\net8.0\Tools.GlobalUsings.g.cs ===

global using global::Microsoft.AspNetCore.Builder;
global using global::Microsoft.AspNetCore.Hosting;
global using global::Microsoft.AspNetCore.Http;
global using global::Microsoft.AspNetCore.Routing;
global using global::Microsoft.Extensions.Configuration;
global using global::Microsoft.Extensions.DependencyInjection;
global using global::Microsoft.Extensions.Hosting;
global using global::Microsoft.Extensions.Logging;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Net.Http.Json;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\Tools\obj\Release\net8.0\Tools.MvcApplicationPartsAssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartAttribute("Swashbuckle.AspNetCore.SwaggerGen")]

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
await ExecuteCommand(commands, jobArgs);
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
private static async Task ExecuteCommand(List<ICommand> commands, CommandArgs jobArgs)
{
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+502f4d15596fdae06c5415404ecfff15dae9308c")]
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

=== FILE: F:\Marketing\WhatsAppSender\obj\Release\net8.0\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\WhatsAppSender\obj\Release\net8.0\WhatsAppSender.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+686d4a9fd23366c79861334aafe6e4c8aa0b6377")]
[assembly: System.Reflection.AssemblyProductAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyTitleAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\WhatsAppSender\obj\Release\net8.0\WhatsAppSender.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

=== FILE: F:\Marketing\WhatsAppSender\obj\Release\net8.0\win-x64\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs ===

using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]

=== FILE: F:\Marketing\WhatsAppSender\obj\Release\net8.0\win-x64\WhatsAppSender.AssemblyInfo.cs ===

using System;
using System.Reflection;
[assembly: System.Reflection.AssemblyCompanyAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+943078ad1fd4a3759ac0f9160f6b41019777bb96")]
[assembly: System.Reflection.AssemblyProductAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyTitleAttribute("WhatsAppSender")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

=== FILE: F:\Marketing\WhatsAppSender\obj\Release\net8.0\win-x64\WhatsAppSender.GlobalUsings.g.cs ===

global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;