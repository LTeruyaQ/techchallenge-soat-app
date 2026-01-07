using Core.DTOs.UseCases.Orcamento;
using Core.Entidades;
using Core.Interfaces.Handlers.Orcamentos;
using Core.Interfaces.UseCases;

namespace Core.UseCases.Orcamentos
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IOrcamentoUseCases
    /// enquanto utiliza os novos casos de uso individuais
    /// </summary>
    public class OrcamentoUseCasesFacade : IOrcamentoUseCases
    {
        private readonly IGerarOrcamentoHandler _gerarOrcamentoHandler;

        public OrcamentoUseCasesFacade(IGerarOrcamentoHandler gerarOrcamentoHandler)
        {
            _gerarOrcamentoHandler = gerarOrcamentoHandler ?? throw new ArgumentNullException(nameof(gerarOrcamentoHandler));
        }

        public decimal GerarOrcamentoUseCase(OrdemServico ordemServico)
        {
            var useCase = new GerarOrcamentoUseCase(ordemServico);
            return _gerarOrcamentoHandler.Handle(useCase);
        }
    }
}
