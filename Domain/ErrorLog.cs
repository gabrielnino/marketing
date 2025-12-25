using System.ComponentModel.DataAnnotations;
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
        public required string Level { get; set; }  // e.g. "Error", "Warning"

        [Required(ErrorMessage = "Message is required.")]
        [MinLength(5, ErrorMessage = "Message must be at least 3 characters long.")]
        [MaxLength(150, ErrorMessage = "Message must be maximun 150 characters long.")]
        public required string Message { get; set; }  // the human-readable message

        [Required(ErrorMessage = "ExceptionType is required.")]
        [MinLength(5, ErrorMessage = "ExceptionType must be at least 3 characters long.")]
        [MaxLength(150, ErrorMessage = "ExceptionType must be maximun 150 characters long.")]
        public string? ExceptionType { get; set; }  // ex.GetType().FullName

        [Required(ErrorMessage = "StackTrace is required.")]
        [MinLength(5, ErrorMessage = "StackTrace must be at least 3 characters long.")]
        [MaxLength(150, ErrorMessage = "StackTrace must be maximun 150 characters long.")]
        public string? StackTrace { get; set; }

        [Required(ErrorMessage = "Context is required.")]
        [MinLength(5, ErrorMessage = "Context must be at least 3 characters long.")]
        [MaxLength(150, ErrorMessage = "Context must be maximun 150 characters long.")]
        public string? Context { get; set; }  // optional JSON payload or identifier
    }
}
