using Core.DTOs.Requests.Usuario;
using Core.DTOs.Responses.Usuario;
using Core.DTOs.UseCases.Usuario;
using Core.Entidades;

namespace Core.Interfaces.Presenters
{
    public interface IUsuarioPresenter
    {
        CadastrarUsuarioUseCaseDto? ParaUseCaseDto(CadastrarUsuarioRequest request);
        AtualizarUsuarioUseCaseDto? ParaUseCaseDto(AtualizarUsuarioRequest request);
        UsuarioResponse? ParaResponse(Usuario usuario);
        IEnumerable<UsuarioResponse?> ParaResponse(IEnumerable<Usuario> usuarios);
    }
}
