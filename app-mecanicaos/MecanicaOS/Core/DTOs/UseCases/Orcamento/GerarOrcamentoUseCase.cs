namespace Core.DTOs.UseCases.Orcamento
{
    /// <summary>
    /// Classe de caso de uso para geração de orçamento
    /// </summary>
    public class GerarOrcamentoUseCase
    {
        /// <summary>
        /// Ordem de serviço para a qual será gerado o orçamento
        /// </summary>
        public Core.Entidades.OrdemServico OrdemServico { get; }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="ordemServico">Ordem de serviço para a qual será gerado o orçamento</param>
        public GerarOrcamentoUseCase(Core.Entidades.OrdemServico ordemServico)
        {
            OrdemServico = ordemServico ?? throw new ArgumentNullException(nameof(ordemServico));
        }
    }
}
