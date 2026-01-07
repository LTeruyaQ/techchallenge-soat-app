using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IAlertaEstoqueGateway
    {
        Task CadastrarVariosAsync(IEnumerable<AlertaEstoque> alertas);
        Task<IEnumerable<AlertaEstoque>> ObterAlertaDoDiaPorEstoqueAsync(Guid insumoId, DateTime dataAtual);
    }
}
