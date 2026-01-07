using Core.DTOs.Requests.Estoque;
using Core.DTOs.Responses.Estoque;

namespace Core.Interfaces.Controllers
{
    public interface IEstoqueController
    {
        Task<EstoqueResponse> Atualizar(Guid id, AtualizarEstoqueRequest request);
        Task<EstoqueResponse> Cadastrar(CadastrarEstoqueRequest request);
        Task<bool> Deletar(Guid id);
        Task<EstoqueResponse> ObterPorId(Guid id);
        Task<IEnumerable<EstoqueResponse>> ObterTodos();
        Task<IEnumerable<EstoqueResponse>> ObterEstoqueCritico();
    }
}