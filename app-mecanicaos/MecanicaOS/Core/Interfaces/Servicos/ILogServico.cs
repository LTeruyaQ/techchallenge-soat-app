namespace Core.Interfaces.Servicos
{
    public interface ILogServico<T>
    {
        public void LogInicio(string metodo, object? props = null);

        public void LogFim(string metodo, object? retorno = null);

        public void LogErro(string metodo, Exception ex);
    }
}
