using Core.Entidades;

namespace Core.Interfaces.Handlers.Estoques
{
    /// <summary>
    /// Interface para o handler de obtenção de estoque por ID
    /// </summary>
    public interface IObterEstoqueHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de estoque por ID
        /// </summary>
        /// <param name="id">ID do estoque a ser obtido</param>
        /// <returns>Estoque encontrado</returns>
        Task<Estoque> Handle(Guid id);
    }
}
