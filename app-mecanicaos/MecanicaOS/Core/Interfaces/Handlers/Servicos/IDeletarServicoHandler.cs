
namespace Core.Interfaces.Handlers.Servicos
{
    /// <summary>
    /// Interface para o handler de deleção de serviços
    /// </summary>
    public interface IDeletarServicoHandler
    {
        /// <summary>
        /// Manipula a operação de deleção de serviço
        /// </summary>
        /// <param name="id">ID do serviço a ser deletado</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> Handle(Guid id);
    }
}
