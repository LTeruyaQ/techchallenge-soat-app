using Core.Entidades;

namespace Core.Interfaces.UseCases
{
    public interface IOrcamentoUseCases
    {
        decimal GerarOrcamentoUseCase(OrdemServico ordemServico);
    }
}