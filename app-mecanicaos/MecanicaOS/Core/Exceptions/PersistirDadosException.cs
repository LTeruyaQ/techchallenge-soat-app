namespace Core.Exceptions;

public class PersistirDadosException : Exception
{
    public PersistirDadosException(string mensagem) : base(mensagem) { }
    public PersistirDadosException(string mensagem, Exception innerException) : base(mensagem, innerException) { }
}

