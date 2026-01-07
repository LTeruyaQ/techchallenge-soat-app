using Core.DTOs.Entidades.Cliente;

namespace Core.DTOs.Responses.Cliente;

public class EnderecoResponse
{
    public string? Rua { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Numero { get; set; }
    public string? CEP { get; set; }
    public string? Complemento { get; set; }
    public Guid IdCliente { get; set; }
    public ClienteEntityDto Cliente { get; set; } = default!;
}