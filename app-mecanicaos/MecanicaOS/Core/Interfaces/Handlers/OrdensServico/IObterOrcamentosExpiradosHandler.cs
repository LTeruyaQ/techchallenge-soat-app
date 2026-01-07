using Core.Entidades;

namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de obtenção de ordens de serviço com orçamento expirado
    /// </summary>
    public interface IObterOrcamentosExpiradosHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de ordens de serviço com orçamento expirado
        /// (Status AguardandoAprovacao com mais de 3 dias desde envio do orçamento)
        /// </summary>
        /// <returns>Lista de ordens de serviço com orçamento expirado</returns>
        Task<IEnumerable<OrdemServico>> Handle();
    }
}
