using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;

namespace Core.Interfaces.Handlers.Veiculos
{
    /// <summary>
    /// Interface para o handler de atualização de veículos
    /// </summary>
    public interface IAtualizarVeiculoHandler
    {
        /// <summary>
        /// Manipula a operação de atualização de veículo
        /// </summary>
        /// <param name="id">ID do veículo a ser atualizado</param>
        /// <param name="request">DTO com os dados atualizados do veículo</param>
        /// <returns>Veículo atualizado</returns>
        Task<Veiculo> Handle(Guid id, AtualizarVeiculoUseCaseDto request);
    }
}
