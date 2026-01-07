using Core.Entidades;

namespace Core.Interfaces.Handlers.Usuarios
{
    /// <summary>
    /// Interface para o handler de obtenção de todos os usuários
    /// </summary>
    public interface IObterTodosUsuariosHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de todos os usuários
        /// </summary>
        /// <returns>Lista de usuários</returns>
        Task<IEnumerable<Usuario>> Handle();
    }
}
