using Core.DTOs.Requests.Servico;
using Core.DTOs.Responses.Servico;

namespace Core.Interfaces.Controllers
{
    public interface IServicoController
    {
        Task<ServicoResponse> Atualizar(Guid id, EditarServicoRequest atualizarServicoRequest);
        Task<ServicoResponse> Criar(CadastrarServicoRequest cadastrarServicoRequest);
        Task Deletar(Guid id);
        Task<ServicoResponse> ObterPorId(Guid id);
        Task<IEnumerable<ServicoResponse>> ObterServicosDisponiveis();
        Task<IEnumerable<ServicoResponse>> ObterTodos();
    }
}