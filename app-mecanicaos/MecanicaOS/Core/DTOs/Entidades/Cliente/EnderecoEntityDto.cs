using Core.DTOs.Entidades.Autenticacao;

namespace Core.DTOs.Entidades.Cliente;

public class EnderecoEntityDto : EntityDto
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
