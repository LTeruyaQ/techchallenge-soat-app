namespace Core.Interfaces.Gateways
{
    public interface ILogGateway<T> where T : class
    {
        void LogInicio(string metodo, object? props = null);
        void LogFim(string metodo, object? retorno = null);
        void LogErro(string metodo, Exception ex);
    }
}
