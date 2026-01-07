using Core.Entidades;

namespace Core.Interfaces.Handlers.Estoques
{
    /// <summary>
    /// Interface para o handler de obtenção de estoques críticos
    /// </summary>
    public interface IObterEstoqueCriticoHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de estoques críticos
        /// </summary>
        /// <returns>Lista de estoques críticos</returns>
        Task<IEnumerable<Estoque>> Handle();
    }
}
