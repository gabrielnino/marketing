using Application.WhatsApp.UseCases.Repository.CRUD;
using Domain.WhatsApp.Redirect;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.v1.Redirect
{
    /// <summary>
    /// Controller for managing TrackedLink entities via REST API.
    /// </summary>
    [ApiController]
    [Route("api/v1/tracked-links")]
    public sealed class TrackedLinkController(
        ITrackedLinkCreate trackedLinkCreate,
        ITrackedLinkRead trackedLinkRead) : ControllerBase
    {
        private readonly ITrackedLinkCreate _create = trackedLinkCreate;
        private readonly ITrackedLinkRead _read = trackedLinkRead;

        /// <summary>
        /// Create a new TrackedLink.
        /// </summary>
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

            // Location now points to the redirect endpoint
            return CreatedAtAction(nameof(RedirectById), new { id = trackedLink.Id }, trackedLink);
        }

        /// <summary>
        /// Redirect to the original URL for this tracked link.
        /// </summary>
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

            // If your ITrackedLinkRead returns a List<TrackedLink>, handle it safely:
            // var link = (op.Data as List<TrackedLink>)?.FirstOrDefault();
            // if (link is null) return NotFound(op.Message);
            // return Redirect(link.OriginalUrl);

            var link = op.Data.FirstOrDefault(); // assumes op.Data is TrackedLink
            string targetUrl = link.TargetUrl;
            if (string.IsNullOrWhiteSpace(targetUrl))
                return NotFound("TrackedLink has no OriginalUrl.");

            // Use 302 by default. If you want permanent redirect, use RedirectPermanent(link.OriginalUrl).
            return Redirect(targetUrl);
        }

        /// <summary>
        /// Optional: read metadata (non-redirect) for a tracked link.
        /// </summary>
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
