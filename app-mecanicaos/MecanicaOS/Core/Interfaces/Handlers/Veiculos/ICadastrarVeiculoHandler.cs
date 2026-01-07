using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;

namespace Core.Interfaces.Handlers.Veiculos
{
    /// <summary>
    /// Interface para o handler de cadastro de veículos
    /// </summary>
    public interface ICadastrarVeiculoHandler
    {
        /// <summary>
        /// Manipula a operação de cadastro de veículo
        /// </summary>
        /// <param name="request">DTO com os dados do veículo a ser cadastrado</param>
        /// <returns>Veículo cadastrado</returns>
        Task<Veiculo> Handle(CadastrarVeiculoUseCaseDto request);
    }
}
