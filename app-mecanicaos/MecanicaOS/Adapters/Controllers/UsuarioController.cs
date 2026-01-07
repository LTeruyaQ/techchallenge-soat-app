using Adapters.Presenters;
using Core.DTOs.Requests.Usuario;
using Core.DTOs.Responses.Usuario;
using Core.DTOs.UseCases.Usuario;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Controllers;
using Core.Interfaces.Presenters;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class UsuarioController : IUsuarioController
    {
        private readonly IUsuarioUseCases _usuarioUseCases;
        private readonly IClienteUseCases _clienteUseCases;
        private readonly IUsuarioPresenter _usuarioPresenter;

        public UsuarioController(ICompositionRoot compositionRoot)
        {
            _usuarioUseCases = compositionRoot.CriarUsuarioUseCases();
            _clienteUseCases = compositionRoot.CriarClienteUseCases();
            _usuarioPresenter = new UsuarioPresenter();
        }

        public async Task<IEnumerable<UsuarioResponse>> ObterTodosAsync()
        {
            return _usuarioPresenter.ParaResponse(await _usuarioUseCases.ObterTodosUseCaseAsync());
        }

        public async Task<UsuarioResponse> ObterPorIdAsync(Guid id)
        {
            return _usuarioPresenter.ParaResponse(await _usuarioUseCases.ObterPorIdUseCaseAsync(id));
        }

        public async Task<UsuarioResponse> ObterPorEmailAsync(string email)
        {
            return _usuarioPresenter.ParaResponse(await _usuarioUseCases.ObterPorEmailUseCaseAsync(email));
        }

        public async Task<UsuarioResponse> CadastrarAsync(CadastrarUsuarioRequest request)
        {
            var useCaseDto = MapearParaCadastrarUsuarioUseCaseDto(request);

            if (request.TipoUsuario == TipoUsuario.Cliente)
            {
                _ = request.Documento ?? throw new DadosInvalidosException("Usuários do tipo cliente devem informar o documento.");
                var cliente = await _clienteUseCases.ObterPorDocumentoUseCaseAsync(request.Documento);
                useCaseDto.ClienteId = cliente.Id;
            }

            var resultado = await _usuarioUseCases.CadastrarUseCaseAsync(useCaseDto);
            return _usuarioPresenter.ParaResponse(resultado);
        }

        internal CadastrarUsuarioUseCaseDto MapearParaCadastrarUsuarioUseCaseDto(CadastrarUsuarioRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarUsuarioUseCaseDto
            {
                Email = request.Email,
                Senha = request.Senha,
                TipoUsuario = request.TipoUsuario,
                RecebeAlertaEstoque = request.RecebeAlertaEstoque,
                Documento = request.Documento
            };
        }

        public async Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request)
        {
            var useCaseDto = MapearParaAtualizarUsuarioUseCaseDto(request);
            var resultado = await _usuarioUseCases.AtualizarUseCaseAsync(id, useCaseDto);
            return _usuarioPresenter.ParaResponse(resultado);
        }

        internal AtualizarUsuarioUseCaseDto MapearParaAtualizarUsuarioUseCaseDto(AtualizarUsuarioRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarUsuarioUseCaseDto
            {
                Email = request.Email,
                Senha = request.Senha,
                DataUltimoAcesso = request.DataUltimoAcesso,
                TipoUsuario = request.TipoUsuario,
                RecebeAlertaEstoque = request.RecebeAlertaEstoque
            };
        }

        public async Task<bool> DeletarAsync(Guid id)
        {
            return await _usuarioUseCases.DeletarUseCaseAsync(id);
        }

        public async Task<IEnumerable<UsuarioResponse>> ObterUsuariosParaAlertaEstoque()
        {
            var usuarios = await _usuarioUseCases.ObterUsuariosParaAlertaEstoqueUseCaseAsync();
            return _usuarioPresenter.ParaResponse(usuarios)
                .Where(response => response != null)!;
        }
    }
}
