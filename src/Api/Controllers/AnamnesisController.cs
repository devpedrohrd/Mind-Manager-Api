using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mind_Manager.src.Application.Services;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager.src.Api.Controllers
{
    [Route("api/anamnesis")]
    [ApiController]
    public class AnamnesisController(IAnamneseService anamneseService, IUserLoggedHandller userLoggedHandller) : ControllerBase
    {
        private readonly IAnamneseService _anamneseService = anamneseService;
        private readonly IUserLoggedHandller _userLoggedHandller = userLoggedHandller;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> CreateAnamnesis([FromBody] CreateAnamnesisCommand command)
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.CreateAnamnesisAsync(command, userRole == UserRole.Psychologist.ToString());
            return Ok(result);
        }

        [HttpGet("{anamnesisId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> GetAnamnesisById(Guid anamnesisId)
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.GetAnamnesisByIdAsync(anamnesisId, userRole == UserRole.Psychologist.ToString());
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        [HttpPatch("{anamnesisId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> UpdateAnamnesis(Guid anamnesisId, [FromBody] UpdateAnamnesisCommand command)
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.UpdateAnamnesisAsync(anamnesisId, command, userRole == UserRole.Psychologist.ToString());
            if (!result)
                return NotFound();
            return Ok("Anamnesis updated successfully.");
        }

        [HttpDelete("{anamnesisId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> DeleteAnamnesis(Guid anamnesisId)
        {
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.DeleteAnamnesisAsync(anamnesisId, userRole == UserRole.Psychologist.ToString());
            if (!result)
                return NotFound();
            return Ok("Anamnesis deleted successfully.");
        }

    }
}
