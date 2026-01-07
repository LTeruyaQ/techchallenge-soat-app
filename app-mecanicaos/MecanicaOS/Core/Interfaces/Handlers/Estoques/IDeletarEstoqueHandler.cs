
namespace Core.Interfaces.Handlers.Estoques
{
    /// <summary>
    /// Interface para o handler de deleção de estoque
    /// </summary>
    public interface IDeletarEstoqueHandler
    {
        /// <summary>
        /// Manipula a operação de deleção de estoque
        /// </summary>
        /// <param name="id">ID do estoque a ser deletado</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> Handle(Guid id);
    }
}
