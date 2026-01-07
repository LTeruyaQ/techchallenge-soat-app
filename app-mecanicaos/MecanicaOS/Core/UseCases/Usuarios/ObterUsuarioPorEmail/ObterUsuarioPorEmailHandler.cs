using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Usuarios;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Usuarios.ObterUsuarioPorEmail
{
    public class ObterUsuarioPorEmailHandler : UseCasesHandlerAbstrato<ObterUsuarioPorEmailHandler>, IObterUsuarioPorEmailHandler
    {
        private readonly IUsuarioGateway _usuarioGateway;

        public ObterUsuarioPorEmailHandler(
            IUsuarioGateway usuarioGateway,
            ILogGateway<ObterUsuarioPorEmailHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _usuarioGateway = usuarioGateway ?? throw new ArgumentNullException(nameof(usuarioGateway));
        }

        public async Task<Usuario?> Handle(string email)
        {
            var metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, email);

                var usuario = await _usuarioGateway.ObterPorEmailAsync(email);

                LogFim(metodo, usuario);

                return usuario;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
