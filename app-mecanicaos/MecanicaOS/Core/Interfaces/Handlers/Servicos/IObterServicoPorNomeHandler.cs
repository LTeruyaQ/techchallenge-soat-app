using Core.Entidades;

namespace Core.Interfaces.Handlers.Servicos
{
    /// <summary>
    /// Interface para o handler de obtenção de serviço por nome
    /// </summary>
    public interface IObterServicoPorNomeHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de serviço por nome
        /// </summary>
        /// <param name="nome">Nome do serviço a ser obtido</param>
        /// <returns>Serviço encontrado ou null se não encontrado</returns>
        Task<Servico?> Handle(string nome);
    }
}
