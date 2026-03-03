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

        /// <summary>
        /// Criar uma nova anamnese para um paciente
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <remarks>
        /// Somente psicólogos podem criar anamneses. Pacientes e administradores não têm permissão para criar anamneses.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> CreateAnamnesis([FromBody] CreateAnamnesisCommand command)
        {
            var (userId, _) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.CreateAnamnesisAsync(command, userId ?? Guid.Empty);
            return Ok(result);
        }

        /// <summary>
        /// Buscar uma anamnese por ID
        /// </summary>
        /// <param name="anamnesisId">ID da anamnese a ser buscada</param>
        /// <returns>Retorna a anamnese encontrada ou erro</returns>
        /// <remarks>
        /// Somente o psicólogo criador da anamnese pode acessá-la.
        /// </remarks>
        [HttpGet("{anamnesisId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> GetAnamnesisById(Guid anamnesisId)
        {
            var (userId, _) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.GetAnamnesisByIdAsync(anamnesisId, userId ?? Guid.Empty);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Atualizar uma anamnese existente por ID
        /// </summary>
        /// <param name="anamnesisId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <remarks>
        /// Somente o psicólogo criador da anamnese pode atualizá-la.
        /// </remarks>
        [HttpPatch("{anamnesisId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> UpdateAnamnesis(Guid anamnesisId, [FromBody] UpdateAnamnesisCommand command)
        {
            var (userId, _) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.UpdateAnamnesisAsync(anamnesisId, command, userId ?? Guid.Empty);
            if (!result)
                return NotFound();
            return Ok("Anamnesis updated successfully.");
        }
        /// <summary>
        /// Excluir uma anamnese por ID
        /// </summary>
        /// <param name="anamnesisId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Somente o psicólogo criador da anamnese pode excluí-la.
        /// </remarks>
        [HttpDelete("{anamnesisId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> DeleteAnamnesis(Guid anamnesisId)
        {
            var (userId, _) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.DeleteAnamnesisAsync(anamnesisId, userId ?? Guid.Empty);
            if (!result)
                return NotFound();
            return Ok("Anamnesis deleted successfully.");
        }

    }
}
