using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IInsumosGateway
    {
        Task<IEnumerable<InsumoOS>> CadastrarVariosAsync(IEnumerable<InsumoOS> insumosOS);
        Task<IEnumerable<InsumoOS>> ObterInsumosOSPorOSAsync(Guid ordemServicoId);
    }
}
