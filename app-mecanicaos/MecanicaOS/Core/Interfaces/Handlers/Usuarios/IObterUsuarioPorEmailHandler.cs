using Core.Entidades;

namespace Core.Interfaces.Handlers.Usuarios
{
    /// <summary>
    /// Interface para o handler de obtenção de usuário por email
    /// </summary>
    public interface IObterUsuarioPorEmailHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de usuário por email
        /// </summary>
        /// <param name="email">Email do usuário a ser obtido</param>
        /// <returns>Usuário encontrado ou null</returns>
        Task<Usuario?> Handle(string email);
    }
}
