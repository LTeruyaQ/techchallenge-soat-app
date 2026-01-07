using Core.Entidades;

namespace Core.Interfaces.Handlers.Clientes
{
    /// <summary>
    /// Interface para o handler de obtenção de cliente por documento
    /// </summary>
    public interface IObterClientePorDocumentoHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de cliente por documento
        /// </summary>
        /// <param name="documento">Documento do cliente a ser obtido</param>
        /// <returns>Cliente encontrado</returns>
        Task<Cliente> Handle(string documento);
    }
}
