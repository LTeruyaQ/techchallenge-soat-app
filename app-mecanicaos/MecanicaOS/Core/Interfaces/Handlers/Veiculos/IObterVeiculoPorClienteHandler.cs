using Core.Entidades;

namespace Core.Interfaces.Handlers.Veiculos
{
    /// <summary>
    /// Interface para o handler de obtenção de veículos por cliente
    /// </summary>
    public interface IObterVeiculoPorClienteHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de veículos por cliente
        /// </summary>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Lista de veículos do cliente</returns>
        Task<IEnumerable<Veiculo>> Handle(Guid clienteId);
    }
}
