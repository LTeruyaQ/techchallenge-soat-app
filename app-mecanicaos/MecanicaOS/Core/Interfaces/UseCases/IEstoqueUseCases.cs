using Core.DTOs.UseCases.Estoque;
using Core.Entidades;

namespace Core.Interfaces.UseCases
{
    public interface IEstoqueUseCases
    {
        Task<Estoque> AtualizarUseCaseAsync(Guid id, AtualizarEstoqueUseCaseDto request);
        Task<Estoque> CadastrarUseCaseAsync(CadastrarEstoqueUseCaseDto request);
        Task<bool> DeletarUseCaseAsync(Guid id);
        Task<Estoque> ObterPorIdUseCaseAsync(Guid id);
        Task<IEnumerable<Estoque>> ObterTodosUseCaseAsync();
        Task<IEnumerable<Estoque>> ObterEstoqueCriticoUseCaseAsync();
    }
}