
namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de aceitação de orçamento
    /// </summary>
    public interface IAceitarOrcamentoHandler
    {
        /// <summary>
        /// Manipula a operação de aceitação de orçamento
        /// </summary>
        /// <param name="id">ID da ordem de serviço cujo orçamento será aceito</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> Handle(Guid id);
    }
}
