using Core.Entidades;

namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de listagem de ordens de serviço ativas
    /// </summary>
    public interface IListarOrdensServicoAtivasHandler
    {
        /// <summary>
        /// Lista ordens de serviço ativas (excluindo finalizadas, entregues, canceladas e orçamento expirado)
        /// ordenadas por prioridade de status e data de cadastro
        /// </summary>
        /// <returns>Lista de ordens de serviço ativas ordenadas</returns>
        Task<IEnumerable<OrdemServico>> Handle();
    }
}
