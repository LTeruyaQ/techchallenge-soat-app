using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IEstoqueGateway
    {
        Task CadastrarAsync(Estoque estoque);
        Task DeletarAsync(Estoque estoque);
        Task EditarAsync(Estoque estoque);
        Task<IEnumerable<Estoque>> ObterEstoqueCriticoAsync();
        Task<Estoque?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Estoque>> ObterTodosAsync();
    }
}
