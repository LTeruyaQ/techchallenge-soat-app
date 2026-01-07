using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IServicoGateway
    {
        Task<Servico> CadastrarAsync(Servico servico);
        Task DeletarAsync(Servico servico);
        Task EditarAsync(Servico servico);
        Task<Servico?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Servico>> ObterServicoDisponivelAsync();
        Task<Servico?> ObterServicosDisponiveisPorNomeAsync(string nome);
        Task<IEnumerable<Servico>> ObterTodosAsync();
    }
}
