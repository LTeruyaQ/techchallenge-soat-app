using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IClienteGateway
    {
        Task<Cliente> CadastrarAsync(Cliente cliente);
        Task DeletarAsync(Cliente cliente);
        Task EditarAsync(Cliente cliente);
        Task<Cliente?> ObterClienteComVeiculoPorIdAsync(Guid clienteId);
        Task<Cliente?> ObterClientePorDocumentoAsync(string documento);
        Task<IEnumerable<Cliente>> ObterClientePorNomeAsync(string nome);
        Task<Cliente?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Cliente>> ObterTodosClientesAsync();
    }
}
