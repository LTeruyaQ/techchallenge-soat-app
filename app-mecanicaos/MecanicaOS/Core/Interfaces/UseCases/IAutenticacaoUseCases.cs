using Core.DTOs.UseCases.Autenticacao;

namespace Core.Interfaces.UseCases
{
    public interface IAutenticacaoUseCases
    {
        Task<AutenticacaoDto> AutenticarUseCaseAsync(AutenticacaoUseCaseDto request);
    }
}