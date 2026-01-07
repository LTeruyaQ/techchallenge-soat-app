namespace Core.Exceptions
{
    [Serializable]
    public class DadosJaCadastradosException : Exception
    {
        public DadosJaCadastradosException()
        {
        }

        public DadosJaCadastradosException(string? message) : base(message)
        {
        }

        public DadosJaCadastradosException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}