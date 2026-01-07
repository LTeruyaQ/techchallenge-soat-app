using Core.DTOs.UseCases.Servico;
using Core.Entidades;

namespace Core.Interfaces.Handlers.Servicos
{
    /// <summary>
    /// Interface para o handler de cadastro de serviços
    /// </summary>
    public interface ICadastrarServicoHandler
    {
        /// <summary>
        /// Manipula a operação de cadastro de serviço
        /// </summary>
        /// <param name="request">DTO com os dados do serviço a ser cadastrado</param>
        /// <returns>Serviço cadastrado</returns>
        Task<Servico> Handle(CadastrarServicoUseCaseDto request);
    }
}
