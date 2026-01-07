using Core.Entidades;

namespace Core.Interfaces.Handlers.Servicos
{
    /// <summary>
    /// Interface para o handler de obtenção de serviço por ID
    /// </summary>
    public interface IObterServicoHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de serviço por ID
        /// </summary>
        /// <param name="id">ID do serviço a ser obtido</param>
        /// <returns>Serviço encontrado</returns>
        Task<Servico> Handle(Guid id);
    }
}
