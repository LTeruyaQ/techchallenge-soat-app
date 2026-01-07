using Core.DTOs.Requests.Usuario;
using Core.DTOs.Responses.Usuario;
using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Interfaces.Presenters;

namespace Adapters.Presenters
{
    public class UsuarioPresenter : IUsuarioPresenter
    {
        public CadastrarUsuarioUseCaseDto? ParaUseCaseDto(CadastrarUsuarioRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarUsuarioUseCaseDto
            {
                Email = request.Email,
                Senha = request.Senha,
                TipoUsuario = request.TipoUsuario,
                RecebeAlertaEstoque = request.RecebeAlertaEstoque,
                Documento = request.Documento
            };
        }

        public AtualizarUsuarioUseCaseDto? ParaUseCaseDto(AtualizarUsuarioRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarUsuarioUseCaseDto
            {
                Email = request.Email,
                Senha = request.Senha,
                DataUltimoAcesso = request.DataUltimoAcesso,
                TipoUsuario = request.TipoUsuario,
                RecebeAlertaEstoque = request.RecebeAlertaEstoque
            };
        }

        public UsuarioResponse? ParaResponse(Usuario usuario)
        {
            if (usuario is null)
                return null;

            return new UsuarioResponse
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Senha = usuario.Senha,
                DataUltimoAcesso = usuario.DataUltimoAcesso,
                TipoUsuario = usuario.TipoUsuario,
                RecebeAlertaEstoque = usuario.RecebeAlertaEstoque,
                Ativo = usuario.Ativo,
                DataCadastro = usuario.DataCadastro,
                DataAtualizacao = usuario.DataAtualizacao
            };
        }

        public IEnumerable<UsuarioResponse?> ParaResponse(IEnumerable<Usuario> usuarios)
        {
            if (usuarios is null)
                return [];

            return usuarios.Select(ParaResponse);
        }
    }
}
