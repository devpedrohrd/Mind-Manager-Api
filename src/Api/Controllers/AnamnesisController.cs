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
            var (userId, userRole) = _userLoggedHandller.GetUserIdAndRole(User);
            var result = await _anamneseService.CreateAnamnesisAsync(command, userRole == UserRole.Psychologist.ToString());
            return Ok(result);
        }

        /// <summary>
        /// Buscar uma anamnese por ID
        /// </summary>
        /// <param name="anamnesisId">ID da anamnese a ser buscada</param>
        /// <returns>Retorna a anamnese encontrada ou erro</returns>
        /// <remarks>
        /// Psicólogos, administradores e pacientes podem buscar anamneses por ID. Pacientes só podem acessar anamneses relacionadas a eles.
        /// </remarks>
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

        /// <summary>
        /// Atualizar uma anamnese existente por ID
        /// </summary>
        /// <param name="anamnesisId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <remarks>
        /// Somente psicólogos podem atualizar anamneses. Pacientes e administradores não têm permissão para atualizar anamneses.
        /// </remarks>
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
        /// <summary>
        /// Excluir uma anamnese por ID
        /// </summary>
        /// <param name="anamnesisId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Somente psicólogos podem excluir anamneses. Pacientes e administradores não têm permissão para excluir anamneses.
        /// </remarks>
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
