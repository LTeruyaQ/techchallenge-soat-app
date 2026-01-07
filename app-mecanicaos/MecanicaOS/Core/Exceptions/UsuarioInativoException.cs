namespace Core.Exceptions;

public class UsuarioInativoException : Exception
{
    public UsuarioInativoException() : base("Usuário inativo") { }
    public UsuarioInativoException(string mensagem) : base(mensagem) { }
}
