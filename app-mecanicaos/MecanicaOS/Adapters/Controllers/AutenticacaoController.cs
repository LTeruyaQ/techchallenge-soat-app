using Core.DTOs.Requests.Autenticacao;
using Core.DTOs.Responses.Autenticacao;
using Core.DTOs.UseCases.Autenticacao;
using Core.Exceptions;
using Core.Interfaces.Controllers;
using Core.Interfaces.Presenters;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class AutenticacaoController : IAutenticacaoController
    {
        private readonly IAutenticacaoUseCases _autenticacaoUseCases;
        private readonly IUsuarioUseCases _usuarioUseCases;
        private readonly IAutenticacaoPresenter _autenticacaoPresenter;

        public AutenticacaoController(ICompositionRoot compositionRoot)
        {
            _autenticacaoUseCases = compositionRoot.CriarAutenticacaoUseCases();
            _usuarioUseCases = compositionRoot.CriarUsuarioUseCases();
            _autenticacaoPresenter = compositionRoot.CriarAutenticacaoPresenter();
        }

        public async Task<AutenticacaoResponse> AutenticarAsync(AutenticacaoRequest autenticacaoRequest)
        {
            var usuario = await _usuarioUseCases.ObterPorEmailUseCaseAsync(autenticacaoRequest.Email) ?? throw new DadosInvalidosException("Usuário ou senha inválidos");

            var useCaseDto = MapearParaAutenticacaoUseCaseDto(autenticacaoRequest, usuario);
            var autenticacaoDto = await _autenticacaoUseCases.AutenticarUseCaseAsync(useCaseDto);

            return _autenticacaoPresenter.ParaResponse(autenticacaoDto);
        }

        public AutenticacaoUseCaseDto MapearParaAutenticacaoUseCaseDto(AutenticacaoRequest request, Core.Entidades.Usuario usuario)
        {
            if (request is null)
                return null;

            return new AutenticacaoUseCaseDto
            {
                Email = request.Email,
                Senha = request.Senha,
                UsuarioExistente = usuario
            };
        }
    }
}
