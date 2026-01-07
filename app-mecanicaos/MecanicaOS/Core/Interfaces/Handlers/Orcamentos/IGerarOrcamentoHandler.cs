
using Core.DTOs.UseCases.Orcamento;

namespace Core.Interfaces.Handlers.Orcamentos
{
    /// <summary>
    /// Interface para o handler de geração de orçamentos
    /// </summary>
    public interface IGerarOrcamentoHandler
    {
        /// <summary>
        /// Manipula a operação de geração de orçamento
        /// </summary>
        /// <param name="useCase">Dados para geração do orçamento</param>
        /// <returns>Valor do orçamento</returns>
        decimal Handle(GerarOrcamentoUseCase useCase);
    }
}
