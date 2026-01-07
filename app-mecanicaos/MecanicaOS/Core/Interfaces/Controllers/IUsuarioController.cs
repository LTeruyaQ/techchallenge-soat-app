using Core.DTOs.Requests.Usuario;
using Core.DTOs.Responses.Usuario;

namespace Core.Interfaces.Controllers
{
    public interface IUsuarioController
    {
        Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request);
        Task<UsuarioResponse> CadastrarAsync(CadastrarUsuarioRequest request);
        Task<bool> DeletarAsync(Guid id);
        Task<UsuarioResponse> ObterPorEmailAsync(string email);
        Task<UsuarioResponse> ObterPorIdAsync(Guid id);
        Task<IEnumerable<UsuarioResponse>> ObterTodosAsync();
        Task<IEnumerable<UsuarioResponse>> ObterUsuariosParaAlertaEstoque();
    }
}