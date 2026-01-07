using Core.DTOs.Entidades.Autenticacao;

namespace Core.DTOs.Entidades.Cliente;

public class ContatoEntityDto : EntityDto
{
    public Guid IdCliente { get; set; }
    public ClienteEntityDto Cliente { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
}
