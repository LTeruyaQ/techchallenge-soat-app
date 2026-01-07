using Core.DTOs.UseCases.Autenticacao;
using Core.Interfaces.Handlers.Autenticacao;
using Core.Interfaces.UseCases;

namespace Core.UseCases.Autenticacao
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IAutenticacaoUseCases
    /// enquanto utiliza os novos casos de uso individuais
    /// </summary>
    public class AutenticacaoUseCasesFacade : IAutenticacaoUseCases
    {
        private readonly IAutenticarUsuarioHandler _autenticarUsuarioHandler;

        public AutenticacaoUseCasesFacade(IAutenticarUsuarioHandler autenticarUsuarioHandler)
        {
            _autenticarUsuarioHandler = autenticarUsuarioHandler ?? throw new ArgumentNullException(nameof(autenticarUsuarioHandler));
        }

        public async Task<AutenticacaoDto> AutenticarUseCaseAsync(AutenticacaoUseCaseDto request)
        {
            return await _autenticarUsuarioHandler.Handle(request);
        }
    }
}
