
namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de recusa de orçamento
    /// </summary>
    public interface IRecusarOrcamentoHandler
    {
        /// <summary>
        /// Manipula a operação de recusa de orçamento
        /// </summary>
        /// <param name="id">ID da ordem de serviço cujo orçamento será recusado</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> Handle(Guid id);
    }
}
