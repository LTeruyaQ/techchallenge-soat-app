using Core.Entidades;

namespace Core.Interfaces.Handlers.Veiculos
{
    /// <summary>
    /// Interface para o handler de obtenção de veículo por placa
    /// </summary>
    public interface IObterVeiculoPorPlacaHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de veículo por placa
        /// </summary>
        /// <param name="placa">Placa do veículo</param>
        /// <returns>Veículo encontrado ou null</returns>
        Task<Veiculo?> Handle(string placa);
    }
}
