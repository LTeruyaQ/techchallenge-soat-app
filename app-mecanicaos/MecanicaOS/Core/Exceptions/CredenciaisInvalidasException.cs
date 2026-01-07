namespace Core.Exceptions;

public class CredenciaisInvalidasException : Exception
{
    public CredenciaisInvalidasException() : base("Credenciais inválidas") { }
    public CredenciaisInvalidasException(string mensagem) : base(mensagem) { }
}
