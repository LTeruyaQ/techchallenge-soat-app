using Core.DTOs.Requests.Veiculo;
using Core.DTOs.Responses.Veiculo;

namespace Core.Interfaces.Controllers
{
    public interface IVeiculoController
    {
        Task<VeiculoResponse> Atualizar(Guid id, AtualizarVeiculoRequest request);
        Task<VeiculoResponse> Cadastrar(CadastrarVeiculoRequest request);
        Task<bool> Deletar(Guid id);
        Task<IEnumerable<VeiculoResponse>> ObterPorCliente(Guid clienteId);
        Task<VeiculoResponse> ObterPorId(Guid id);
        Task<VeiculoResponse> ObterPorPlaca(string placa);
        Task<IEnumerable<VeiculoResponse>> ObterTodos();
    }
}