using Core.Interfaces.Gateways;

namespace Core.UseCases.Abstrato
{
    public abstract class UseCasesHandlerAbstrato<T> where T : class
    {
        private readonly ILogGateway<T> _logServicoGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        protected readonly IUsuarioLogadoServicoGateway _usuarioLogadoServicoGateway;

        protected UseCasesHandlerAbstrato(ILogGateway<T> logServicoGateway, IUnidadeDeTrabalhoGateway udtGateway, IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
        {
            _logServicoGateway = logServicoGateway ?? throw new ArgumentNullException(nameof(logServicoGateway));
            _udtGateway = udtGateway ?? throw new ArgumentNullException(nameof(udtGateway));
            _usuarioLogadoServicoGateway = usuarioLogadoServicoGateway ?? throw new ArgumentNullException(nameof(usuarioLogadoServicoGateway));
        }

        protected async Task<bool> Commit()
        {
            return await _udtGateway.Commit();
        }

        #region Logs
        protected void LogInicio(string metodo, object? props = null)
           => _logServicoGateway.LogInicio(metodo, props);

        protected void LogFim(string metodo, object? retorno = null)
            => _logServicoGateway.LogFim(metodo, retorno);

        protected void LogErro(string metodo, Exception ex)
            => _logServicoGateway.LogErro(metodo, ex);
        #endregion
    }
}
