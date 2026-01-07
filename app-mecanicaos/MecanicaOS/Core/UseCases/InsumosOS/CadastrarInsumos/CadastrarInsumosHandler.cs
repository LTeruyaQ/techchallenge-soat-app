using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.InsumosOS;
using Core.UseCases.Abstrato;

namespace Core.UseCases.InsumosOS.CadastrarInsumos
{
    public class CadastrarInsumosHandler : UseCasesHandlerAbstrato<CadastrarInsumosHandler>, ICadastrarInsumosHandler
    {
        private readonly IVerificarEstoqueJobGateway _verificarEstoqueJobGateway;
        private readonly IInsumosGateway _insumosGateway;

        public CadastrarInsumosHandler(
            ILogGateway<CadastrarInsumosHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway,
            IVerificarEstoqueJobGateway verificarEstoqueJobGateway,
            IInsumosGateway insumosGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _verificarEstoqueJobGateway = verificarEstoqueJobGateway ?? throw new ArgumentNullException(nameof(verificarEstoqueJobGateway));
            _insumosGateway = insumosGateway;
        }

        public async Task<List<InsumoOS>> Handle(Guid ordemServicoId, List<CadastrarInsumoOSUseCaseDto> request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, new { ordemServicoId, InsumosCount = request.Count });

                List<InsumoOS> insumosOS = [];

                foreach (var insumoDto in request)
                {
                    var insumoOS = new InsumoOS
                    {
                        OrdemServicoId = ordemServicoId,
                        EstoqueId = insumoDto.EstoqueId,
                        Quantidade = insumoDto.Quantidade
                    };

                    insumosOS.Add(insumoOS);

                    if (insumoDto.Estoque != null &&
                        (insumoDto.Estoque.QuantidadeDisponivel - insumoDto.Quantidade) <= insumoDto.Estoque.QuantidadeMinima)
                    {
                        await _verificarEstoqueJobGateway.VerificarEstoqueAsync();
                    }
                }

                await _insumosGateway.CadastrarVariosAsync(insumosOS);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao cadastrar insumos na ordem de serviço");

                LogFim(metodo, insumosOS);

                return [.. insumosOS];
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
