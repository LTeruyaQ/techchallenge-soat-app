using Core.Entidades;

namespace Core.Interfaces.Handlers.Clientes
{
    /// <summary>
    /// Interface para o handler de obtenção de cliente por nome
    /// </summary>
    public interface IObterClientePorNomeHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de cliente por nome
        /// </summary>
        /// <param name="nome">Nome do cliente a ser obtido</param>
        /// <returns>Lista de clientes encontrados</returns>
        Task<IEnumerable<Cliente>> Handle(string nome);
    }
}
