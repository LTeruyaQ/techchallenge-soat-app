using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IEnderecoGateway
    {
        Task CadastrarAsync(Endereco endereco);
        Task EditarAsync(Endereco endereco);
        Task<Endereco?> ObterPorIdAsync(Guid enderecoId);
    }
}
