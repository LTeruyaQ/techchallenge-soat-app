using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IContatoGateway
    {
        Task CadastrarAsync(Contato contato);
        Task EditarAsync(Contato contato);
        Task<Contato?> ObterPorIdAsync(Guid contatoId);
    }
}
