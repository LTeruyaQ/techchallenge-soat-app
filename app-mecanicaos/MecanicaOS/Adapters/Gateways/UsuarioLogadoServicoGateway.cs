using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.Interfaces.Servicos;
using System.Security.Claims;

namespace Adapters.Gateways
{
    public class UsuarioLogadoServicoGateway : IUsuarioLogadoServicoGateway
    {
        private readonly IUsuarioLogadoServico _usuarioLogadoServico;

        public UsuarioLogadoServicoGateway(IUsuarioLogadoServico usuarioLogadoServico)
        {
            _usuarioLogadoServico = usuarioLogadoServico ?? throw new ArgumentNullException(nameof(usuarioLogadoServico));
        }

        public Guid? UsuarioId => _usuarioLogadoServico.UsuarioId;

        public string? Email => _usuarioLogadoServico.Email;

        public string Nome => _usuarioLogadoServico.Nome;

        public TipoUsuario? TipoUsuario => _usuarioLogadoServico.TipoUsuario;

        public bool EstaAutenticado => _usuarioLogadoServico.EstaAutenticado;

        public bool EstaNaRole(string role)
        {
            return _usuarioLogadoServico.EstaNaRole(role);
        }

        public bool PossuiPermissao(string permissao)
        {
            return _usuarioLogadoServico.PossuiPermissao(permissao);
        }

        public IEnumerable<Claim> ObterTodasClaims()
        {
            return _usuarioLogadoServico.ObterTodasClaims();
        }

        public Usuario? ObterUsuarioLogado()
        {
            return _usuarioLogadoServico.ObterUsuarioLogado();
        }
    }
}
