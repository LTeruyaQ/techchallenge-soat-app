using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.Entidades;
using Core.Interfaces.Presenters;

namespace Adapters.Presenters
{
    public class InsumoOSPresenter : IInsumoPresenter
    {
        public IEnumerable<InsumoOSResponse> ToResponse(IEnumerable<InsumoOS> insumos)
        {
            if (insumos == null || !insumos.Any())
                return new List<InsumoOSResponse>();

            return insumos.Select(insumo => new InsumoOSResponse
            {
                EstoqueId = insumo.EstoqueId,
                OrdemServicoId = insumo.OrdemServicoId,
                Quantidade = insumo.Quantidade,
                Estoque = insumo.Estoque != null ? new Core.DTOs.Responses.Estoque.EstoqueResponse
                {
                    Id = insumo.EstoqueId,
                    Insumo = insumo.Estoque.Insumo,
                    QuantidadeDisponivel = insumo.Estoque.QuantidadeDisponivel,
                    QuantidadeMinima = insumo.Estoque.QuantidadeMinima,
                    Preco = (double)insumo.Estoque.Preco
                } : null
            });
        }
    }
}
