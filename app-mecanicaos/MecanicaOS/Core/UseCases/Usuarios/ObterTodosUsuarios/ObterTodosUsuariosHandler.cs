using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Usuarios;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Usuarios.ObterTodosUsuarios
{
    public class ObterTodosUsuariosHandler : UseCasesHandlerAbstrato<ObterTodosUsuariosHandler>, IObterTodosUsuariosHandler
    {
        private readonly IUsuarioGateway _usuarioGateway;

        public ObterTodosUsuariosHandler(
            IUsuarioGateway usuarioGateway,
            ILogGateway<ObterTodosUsuariosHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _usuarioGateway = usuarioGateway ?? throw new ArgumentNullException(nameof(usuarioGateway));
        }

        public async Task<IEnumerable<Usuario>> Handle()
        {
            var metodo = nameof(Handle);

            try
            {
                LogInicio(metodo);

                var usuarios = await _usuarioGateway.ObterTodosAsync();

                foreach (var usuario in usuarios)
                {
                    IsNotGetSenha(usuario);
                }

                LogFim(metodo, usuarios);

                return usuarios;
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
