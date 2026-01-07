using Core.Entidades;

namespace Core.Interfaces.Handlers.Usuarios
{
    /// <summary>
    /// Interface para o handler de obtenção de usuários para alerta de estoque
    /// </summary>
    public interface IObterUsuariosParaAlertaEstoqueHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de usuários que devem receber alertas de estoque crítico
        /// </summary>
        /// <returns>Lista de usuários configurados para receber alertas</returns>
        Task<IEnumerable<Usuario>> Handle();
    }
}
