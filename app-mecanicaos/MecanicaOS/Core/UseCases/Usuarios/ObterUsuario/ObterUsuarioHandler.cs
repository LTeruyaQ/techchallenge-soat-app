using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Usuarios;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Usuarios.ObterUsuario
{
    public class ObterUsuarioHandler : UseCasesHandlerAbstrato<ObterUsuarioHandler>, IObterUsuarioHandler
    {
        private readonly IUsuarioGateway _usuarioGateway;

        public ObterUsuarioHandler(
            IUsuarioGateway usuarioGateway,
            ILogGateway<ObterUsuarioHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _usuarioGateway = usuarioGateway ?? throw new ArgumentNullException(nameof(usuarioGateway));
        }

        public async Task<Usuario?> Handle(Guid id)
        {
            var metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var usuario = await _usuarioGateway.ObterPorIdAsync(id);

                if (usuario is not null)
                    IsNotGetSenha(usuario);

                LogFim(metodo, usuario);

                return usuario;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }

        private static void IsNotGetSenha(Usuario usuario)
        {
            usuario.Senha = string.Empty;
        }
    }
}
