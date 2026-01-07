using Adapters.Presenters;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.DTOs.UseCases.Estoque;
using Core.DTOs.UseCases.OrdemServico;
using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Exceptions;
using Core.Interfaces.Controllers;
using Core.Interfaces.Presenters;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class InsumoOSController : IInsumoOSController
    {
        private readonly IInsumoOSUseCases _insumoOSUseCases;
        private readonly IOrdemServicoUseCases _ordemServicoUseCases;
        private readonly IEstoqueUseCases _estoqueUseCases;
        private readonly IInsumoPresenter _insumoPresenter;

        public InsumoOSController(ICompositionRoot compositionRoot)
        {
            _insumoOSUseCases = compositionRoot.CriarInsumoOSUseCases();
            _ordemServicoUseCases = compositionRoot.CriarOrdemServicoUseCases();
            _estoqueUseCases = compositionRoot.CriarEstoqueUseCases();
            _insumoPresenter = new InsumoOSPresenter();
        }

        public async Task<IEnumerable<InsumoOSResponse>> CadastrarInsumos(Guid ordemServicoId, IEnumerable<CadastrarInsumoOSRequest> requests)
        {
            _ = await _ordemServicoUseCases.ObterPorIdUseCaseAsync(ordemServicoId)
                ?? throw new DadosNaoEncontradosException("Ordem de serviço não encontrada");

            var useCaseDtos = new List<CadastrarInsumoOSUseCaseDto>();

            foreach (var request in requests)
            {
                var estoque = await _estoqueUseCases.ObterPorIdUseCaseAsync(request.EstoqueId)
                    ?? throw new DadosNaoEncontradosException($"Estoque com ID {request.EstoqueId} não encontrado");

                if (estoque.QuantidadeDisponivel < request.Quantidade)
                    throw new DadosInvalidosException($"Estoque insuficiente para o insumo {estoque.Insumo}");

                var useCaseDto = MapearParaCadastrarInsumoOSUseCaseDto(request);
                useCaseDto.Estoque = estoque;
                useCaseDtos.Add(useCaseDto);

                estoque.QuantidadeDisponivel -= request.Quantidade;
                await _estoqueUseCases.AtualizarUseCaseAsync(estoque.Id, new AtualizarEstoqueUseCaseDto
                {
                    Insumo = estoque.Insumo,
                    QuantidadeDisponivel = estoque.QuantidadeDisponivel,
                    QuantidadeMinima = estoque.QuantidadeMinima,
                    Preco = estoque.Preco
                });
            }

            await _ordemServicoUseCases.AtualizarUseCaseAsync(ordemServicoId, new AtualizarOrdemServicoUseCaseDto
            {
                Status = Core.Enumeradores.StatusOrdemServico.EmDiagnostico
            });

            return _insumoPresenter.ToResponse(await _insumoOSUseCases.CadastrarInsumosUseCaseAsync(ordemServicoId, useCaseDtos));
        }

        internal CadastrarInsumoOSUseCaseDto MapearParaCadastrarInsumoOSUseCaseDto(CadastrarInsumoOSRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarInsumoOSUseCaseDto
            {
                EstoqueId = request.EstoqueId,
                Quantidade = request.Quantidade
            };
        }

        public async Task DevolverInsumosAoEstoque(IEnumerable<DevolverInsumoOSRequest> insumosOS)
        {
            foreach (var insumoOS in insumosOS)
            {
                var estoque = await _estoqueUseCases.ObterPorIdUseCaseAsync(insumoOS.EstoqueId)
                    ?? throw new DadosNaoEncontradosException($"Estoque com ID {insumoOS.EstoqueId} não encontrado");

                estoque.QuantidadeDisponivel += insumoOS.Quantidade;

                await _estoqueUseCases.AtualizarUseCaseAsync(estoque.Id, new AtualizarEstoqueUseCaseDto
                {
                    Insumo = estoque.Insumo,
                    QuantidadeDisponivel = estoque.QuantidadeDisponivel,
                    QuantidadeMinima = estoque.QuantidadeMinima,
                    Preco = estoque.Preco
                });
            }

            await _insumoOSUseCases.DevolverInsumosAoEstoqueUseCaseAsync(insumosOS);
        }
    }
}
