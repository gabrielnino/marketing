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
            {
                return BadRequest(ModelState);
            }

            var op = await _create.CreateAsync(trackedLink); // ITrackedLinkCreate.CreateAsync(TrackedLink) :contentReference[oaicite:0]{index=0}
            if (!op.IsSuccessful)
            {
                return BadRequest(op.Message);
            }

            return CreatedAtAction(nameof(ReadById), new { id = trackedLink.Id }, trackedLink);
        }

        /// <summary>
        /// Read TrackedLink(s) by id (current repository returns a List).
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<TrackedLink>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReadById([FromRoute] string id)
        {
            var op = await _read.ReadAsync(id);
            if (!op.IsSuccessful)
            {
                return BadRequest(op.Message);
            }

            return Ok(op.Data);
        }
    }
}
