using Core.DTOs.UseCases.Autenticacao;

namespace Core.Interfaces.Handlers.Autenticacao
{
    /// <summary>
    /// Interface para o handler de autenticação de usuário
    /// </summary>
    public interface IAutenticarUsuarioHandler
    {
        /// <summary>
        /// Manipula a operação de autenticação de usuário
        /// </summary>
        /// <param name="request">DTO com os dados de autenticação</param>
        /// <returns>Dados de autenticação e token</returns>
        Task<AutenticacaoDto> Handle(AutenticacaoUseCaseDto request);
    }
}
