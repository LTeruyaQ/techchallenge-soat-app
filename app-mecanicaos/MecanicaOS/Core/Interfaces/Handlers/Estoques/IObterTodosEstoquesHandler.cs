using Core.Entidades;

namespace Core.Interfaces.Handlers.Estoques
{
    /// <summary>
    /// Interface para o handler de obtenção de todos os estoques
    /// </summary>
    public interface IObterTodosEstoquesHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de todos os estoques
        /// </summary>
        /// <returns>Lista de estoques</returns>
        Task<IEnumerable<Estoque>> Handle();
    }
}
