using Microsoft.AspNetCore.Mvc;
using Moq.Business.Service;
using Moq.DB.Context;

namespace Moq.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _service;
        private readonly ILogger<CandidatesController> _logger;

        public CandidatesController(ICandidateService service, ILogger<CandidatesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCandidate([FromBody] Candidate candidate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _service.AddOrUpdateCandidateAsync(candidate);
                return Ok();
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "An error occurred while adding or updating a candidate with email {Email}", candidate.Email);
                return StatusCode(500, "An error occurred while processing your request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding or updating a candidate with email {Email}", candidate.Email);
                return StatusCode(500, "An unexpected error occurred while processing your request.");
            }
        }
    }
}
