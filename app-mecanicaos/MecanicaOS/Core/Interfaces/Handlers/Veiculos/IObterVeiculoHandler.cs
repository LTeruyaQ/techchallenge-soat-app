using Core.Entidades;

namespace Core.Interfaces.Handlers.Veiculos
{
    /// <summary>
    /// Interface para o handler de obtenção de veículo por ID
    /// </summary>
    public interface IObterVeiculoHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de veículo por ID
        /// </summary>
        /// <param name="id">ID do veículo a ser obtido</param>
        /// <returns>Veículo encontrado</returns>
        Task<Veiculo> Handle(Guid id);
    }
}
