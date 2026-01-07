using Core.DTOs.UseCases.Servico;
using Core.Entidades;

namespace Core.Interfaces.UseCases
{
    public interface IServicoUseCases
    {
        Task<Servico> CadastrarServicoUseCaseAsync(CadastrarServicoUseCaseDto request);
        Task<bool> DeletarServicoUseCaseAsync(Guid id);
        Task<Servico> EditarServicoUseCaseAsync(Guid id, EditarServicoUseCaseDto request);
        Task<Servico> ObterServicoPorIdUseCaseAsync(Guid id);
        Task<Servico?> ObterServicoPorNomeUseCaseAsync(string nome);
        Task<IEnumerable<Servico>> ObterServicosDisponiveisUseCaseAsync();
        Task<IEnumerable<Servico>> ObterTodosUseCaseAsync();
    }
}