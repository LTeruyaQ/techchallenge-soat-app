using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Usuarios;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Usuarios.ObterUsuariosParaAlertaEstoque
{
    public class ObterUsuariosParaAlertaEstoqueHandler : UseCasesHandlerAbstrato<ObterUsuariosParaAlertaEstoqueHandler>, IObterUsuariosParaAlertaEstoqueHandler
    {
        private readonly IUsuarioGateway _usuarioGateway;

        public ObterUsuariosParaAlertaEstoqueHandler(
            IUsuarioGateway usuarioGateway,
            ILogGateway<ObterUsuariosParaAlertaEstoqueHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _usuarioGateway = usuarioGateway ?? throw new ArgumentNullException(nameof(usuarioGateway));
        }

        public async Task<IEnumerable<Usuario>> Handle()
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo);

                var usuarios = await _usuarioGateway.ObterUsuarioParaAlertaEstoqueAsync();

                LogFim(metodo, usuarios);

                return usuarios;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
