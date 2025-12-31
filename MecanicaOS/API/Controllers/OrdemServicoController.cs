using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.Erro;
using Core.DTOs.Responses.OrdemServico;
using Core.Enumeradores;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class OrdemServicoController : BaseApiController
    {
        private readonly IOrdemServicoController _ordemServicoController;
        private readonly IInsumoOSController _insumoOSController;

        public OrdemServicoController(ICompositionRoot compositionRoot)
        {
            _ordemServicoController = compositionRoot.CriarOrdemServicoController();
            _insumoOSController = compositionRoot.CriarInsumoOSController();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrdemServicoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterTodos()
        {
            var ordemServicos = await _ordemServicoController.ObterTodos();
            return Ok(ordemServicos);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var ordemServico = await _ordemServicoController.ObterPorId(id);
            if (ordemServico == null)
                return NotFound(new ErrorResponse { Message = "Ordem de Serviço não encontrada" });

            return Ok(ordemServico);
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<OrdemServicoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterPorStatus(StatusOrdemServico status)
        {
            var ordensServico = await _ordemServicoController.ObterPorStatus(status);
            if (ordensServico == null || !ordensServico.Any())
                return NotFound(new ErrorResponse { Message = "Nenhuma ordem de serviço encontrada com o status informado" });

            return Ok(ordensServico);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Criar([FromBody] CadastrarOrdemServicoRequest request)
        {
            var resultadoValidacao = ValidarModelState();
            if (resultadoValidacao != null) return resultadoValidacao;

            var ordemServico = await _ordemServicoController.Cadastrar(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = ordemServico.Id }, ordemServico);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarOrdemServicoRequest request)
        {
            var resultadoValidacao = ValidarModelState();
            if (resultadoValidacao != null) return resultadoValidacao;

            var ordemServicoAtualizado = await _ordemServicoController.Atualizar(id, request);
            return Ok(ordemServicoAtualizado);
        }

        [HttpPost("{ordemServicoId}/insumos")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<Core.DTOs.Entidades.OrdemServicos.InsumoOSEntityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdicionarInsumosOS(Guid ordemServicoId, List<CadastrarInsumoOSRequest> request)
        {
            var resultadoValidacao = ValidarModelState();
            if (resultadoValidacao != null) return resultadoValidacao;

            var insumosOS = await _insumoOSController.CadastrarInsumos(ordemServicoId, request);
            return Ok(insumosOS);
        }

        [HttpGet("test-error")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TestError()
        {
            var errorResponse = new ErrorResponse
            {
                Message = "Erro forçado para teste de monitoramento Datadog",
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        [HttpPatch("{id}/aceitar-orcamento")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AceitarOrcamento(Guid id)
        {
            await _ordemServicoController.AceitarOrcamento(id);
            return NoContent();
        }

        [HttpPatch("{id}/recusar-orcamento")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecusarOrcamento(Guid id)
        {
            await _ordemServicoController.RecusarOrcamento(id);
            return NoContent();
        }

        [HttpGet("ativas")]
        [ProducesResponseType(typeof(IEnumerable<OrdemServicoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarOrdensServicoAtivas()
        {
            var ordensServico = await _ordemServicoController.ListarOrdensServicoAtivas();
            return Ok(ordensServico);
        }
    }
}