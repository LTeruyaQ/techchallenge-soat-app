using Core.Entidades.Abstratos;

namespace Core.Entidades;

public class Endereco : Entidade
{
    public string? Rua { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Numero { get; set; }
    public string? CEP { get; set; }
    public string? Complemento { get; set; }
    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = default!;
}