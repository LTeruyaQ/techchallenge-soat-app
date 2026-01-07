using Core.DTOs.Requests.Cliente;
using Core.DTOs.Responses.Cliente;

namespace Core.Interfaces.Controllers
{
    public interface IClienteController
    {
        Task<ClienteResponse> Atualizar(Guid id, AtualizarClienteRequest request);
        Task<ClienteResponse> Cadastrar(CadastrarClienteRequest request);
        Task<ClienteResponse> ObterPorDocumento(string documento);
        Task<IEnumerable<ClienteResponse>> ObterPorNome(string nome);
        Task<ClienteResponse> ObterPorId(Guid id);
        Task<IEnumerable<ClienteResponse>> ObterTodos();
        Task<bool> Remover(Guid id);
    }
}