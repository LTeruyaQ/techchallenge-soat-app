using Core.DTOs.Requests.Autenticacao;
using Core.DTOs.Requests.Usuario;
using Core.DTOs.Responses.Autenticacao;
using Core.DTOs.Responses.Erro;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AutenticacaoController : BaseApiController
    {
        private readonly IAutenticacaoController _autenticacaoController;
        private readonly IUsuarioController _usuarioController;

        public AutenticacaoController(
            ICompositionRoot compositionRoot)
        {
            _autenticacaoController = compositionRoot.CriarAutenticacaoController();
            _usuarioController = compositionRoot.CriarUsuarioController();
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AutenticacaoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AutenticacaoRequest request)
        {
            return ValidarModelState() ?? Ok(await _autenticacaoController.AutenticarAsync(request));
        }

        [HttpPost("Registrar")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Registrar(CadastrarUsuarioRequest request)
        {
            var usuario = await _usuarioController.CadastrarAsync(request);
            return CreatedAtAction(nameof(Login), new AutenticacaoRequest { Email = usuario.Email, Senha = usuario.Senha }, usuario);
        }

        [HttpGet("Validar-Token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ValidarToken()
        {
            return Ok(new { mensagem = "Token válido" });
        }
    }
}
