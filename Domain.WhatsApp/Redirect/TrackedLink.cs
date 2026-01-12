using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Domain.WhatsApp.Redirect
{
    [method: SetsRequiredMembers]
    public sealed class TrackedLink(string id, string targeUrl) : Entity(id)
    {
        [Required(ErrorMessage = "A destination URL is required.")]
        [MinLength(5, ErrorMessage = "The destination URL is too short.")]
        [MaxLength(150, ErrorMessage = "The destination URL is too long (maximum 150 characters).")]
        public required string TargetUrl { get; init; } = targeUrl;

        public long VisitCount { get; private set; }

        public void RegisterVisit()
        {
            VisitCount++;
        }
    }
}
