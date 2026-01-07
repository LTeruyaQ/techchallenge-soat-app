using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IVeiculoGateway
    {
        Task CadastrarAsync(object veiculo);
        Task DeletarAsync(Veiculo veiculo);
        Task EditarAsync(object veiculo);
        Task<Veiculo?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Veiculo>> ObterTodosAsync();
        Task<IEnumerable<Veiculo>> ObterVeiculoPorClienteAsync(Guid clienteId);
        Task<IEnumerable<Veiculo>> ObterVeiculoPorPlacaAsync(string placa);
    }
}
