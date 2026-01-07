using Core.DTOs.UseCases.Usuario;
using Core.Entidades;

namespace Core.Interfaces.UseCases
{
    public interface IUsuarioUseCases
    {
        Task<Usuario> AtualizarUseCaseAsync(Guid id, AtualizarUsuarioUseCaseDto request);
        Task<Usuario> CadastrarUseCaseAsync(CadastrarUsuarioUseCaseDto request);
        Task<bool> DeletarUseCaseAsync(Guid id);
        Task<Usuario?> ObterPorEmailUseCaseAsync(string email);
        Task<Usuario?> ObterPorIdUseCaseAsync(Guid id);
        Task<IEnumerable<Usuario>> ObterTodosUseCaseAsync();
        Task<IEnumerable<Usuario>> ObterUsuariosParaAlertaEstoqueUseCaseAsync();
    }
}