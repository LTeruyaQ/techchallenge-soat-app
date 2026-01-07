using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Responses.OrdemServico;
using Core.Enumeradores;

namespace Core.Interfaces.Controllers
{
    public interface IOrdemServicoController
    {
        Task AceitarOrcamento(Guid id);
        Task RecusarOrcamento(Guid id);
        Task<IEnumerable<OrdemServicoResponse>> ObterOrcamentosExpirados();
        Task<OrdemServicoResponse> Atualizar(Guid id, AtualizarOrdemServicoRequest request);
        Task<OrdemServicoResponse> Cadastrar(CadastrarOrdemServicoRequest request);
        Task CalcularOrcamentoAsync(Guid ordemServicoId);
        Task<OrdemServicoResponse> ObterPorId(Guid id);
        Task<IEnumerable<OrdemServicoResponse>> ObterPorStatus(StatusOrdemServico status);
        Task<IEnumerable<OrdemServicoResponse>> ObterTodos();
        Task<IEnumerable<OrdemServicoResponse>> ListarOrdensServicoAtivas();
    }
}