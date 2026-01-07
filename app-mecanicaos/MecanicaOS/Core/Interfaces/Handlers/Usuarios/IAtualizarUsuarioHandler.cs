using Core.DTOs.UseCases.Usuario;
using Core.Entidades;

namespace Core.Interfaces.Handlers.Usuarios
{
    /// <summary>
    /// Interface para o handler de atualização de usuários
    /// </summary>
    public interface IAtualizarUsuarioHandler
    {
        /// <summary>
        /// Manipula a operação de atualização de usuário
        /// </summary>
        /// <param name="id">ID do usuário a ser atualizado</param>
        /// <param name="request">DTO com os dados atualizados do usuário</param>
        /// <returns>Usuário atualizado</returns>
        Task<Usuario> Handle(Guid id, AtualizarUsuarioUseCaseDto request);
    }
}
