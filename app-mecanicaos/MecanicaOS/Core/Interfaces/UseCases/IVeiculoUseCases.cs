using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;

namespace Core.Interfaces.UseCases
{
    public interface IVeiculoUseCases
    {
        Task<Veiculo> AtualizarUseCaseAsync(Guid id, AtualizarVeiculoUseCaseDto request);
        Task<Veiculo> CadastrarUseCaseAsync(CadastrarVeiculoUseCaseDto request);
        Task<bool> DeletarUseCaseAsync(Guid id);
        Task<IEnumerable<Veiculo>> ObterPorClienteUseCaseAsync(Guid clienteId);
        Task<Veiculo> ObterPorIdUseCaseAsync(Guid id);
        Task<Veiculo?> ObterPorPlacaUseCaseAsync(string placa);
        Task<IEnumerable<Veiculo>> ObterTodosUseCaseAsync();
    }
}