using Core.DTOs.UseCases.Orcamento;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Orcamentos;

namespace Core.UseCases.Orcamentos.GerarOrcamento
{
    public class GerarOrcamentoHandler : IGerarOrcamentoHandler
    {
        private readonly ILogGateway<GerarOrcamentoHandler> _logServicoGateway;

        public GerarOrcamentoHandler(
            ILogGateway<GerarOrcamentoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
        {
            _logServicoGateway = logServicoGateway ?? throw new ArgumentNullException(nameof(logServicoGateway));
        }

        public decimal Handle(GerarOrcamentoUseCase useCase)
        {
            try
            {
                decimal precoServico = useCase.OrdemServico.Servico!.Valor;
                decimal precoInsumos = useCase.OrdemServico.InsumosOS.Sum(i =>
                    i.Quantidade * i.Estoque.Preco);

                decimal valorTotal = precoServico + precoInsumos;

                return valorTotal;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
