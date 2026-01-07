using Core.DTOs.Requests.Cliente;
using Core.DTOs.Responses.Cliente;
using Core.DTOs.UseCases.Cliente;
using Core.Entidades;

namespace Core.Interfaces.Presenters
{
    public interface IClientePresenter
    {
        CadastrarClienteUseCaseDto? ParaUseCaseDto(CadastrarClienteRequest request);
        AtualizarClienteUseCaseDto? ParaUseCaseDto(AtualizarClienteRequest request);
        ClienteResponse? ParaResponse(Cliente cliente);
        IEnumerable<ClienteResponse?> ParaResponse(IEnumerable<Cliente> clientes);
    }
}
