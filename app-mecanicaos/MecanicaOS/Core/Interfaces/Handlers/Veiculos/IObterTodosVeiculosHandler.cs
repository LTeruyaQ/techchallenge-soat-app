using Core.Entidades;

namespace Core.Interfaces.Handlers.Veiculos
{
    /// <summary>
    /// Interface para o handler de obtenção de todos os veículos
    /// </summary>
    public interface IObterTodosVeiculosHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de todos os veículos
        /// </summary>
        /// <returns>Lista de veículos</returns>
        Task<IEnumerable<Veiculo>> Handle();
    }
}
