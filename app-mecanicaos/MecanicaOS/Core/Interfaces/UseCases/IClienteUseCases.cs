using Core.DTOs.UseCases.Cliente;
using Core.Entidades;

namespace Core.Interfaces.UseCases
{
    public interface IClienteUseCases
    {
        Task<Cliente> AtualizarUseCaseAsync(Guid id, AtualizarClienteUseCaseDto request);
        Task<Cliente> CadastrarUseCaseAsync(CadastrarClienteUseCaseDto request);
        Task<Cliente> ObterPorDocumentoUseCaseAsync(string documento);
        Task<IEnumerable<Cliente>> ObterPorNomeUseCaseAsync(string nome);
        Task<Cliente> ObterPorIdUseCaseAsync(Guid id);
        Task<IEnumerable<Cliente>> ObterTodosUseCaseAsync();
        Task<bool> RemoverUseCaseAsync(Guid id);
    }
}