using Core.Entidades;
using Core.Enumeradores;
using System.Security.Claims;

namespace Core.Interfaces.Gateways
{
    public interface IUsuarioLogadoServicoGateway
    {
        Guid? UsuarioId { get; }
        string? Email { get; }
        string Nome { get; }
        TipoUsuario? TipoUsuario { get; }
        bool EstaAutenticado { get; }
        bool EstaNaRole(string role);
        bool PossuiPermissao(string permissao);
        IEnumerable<Claim> ObterTodasClaims();
        Usuario? ObterUsuarioLogado();
    }
}
