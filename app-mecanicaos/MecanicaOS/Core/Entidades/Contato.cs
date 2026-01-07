using Core.Entidades.Abstratos;

namespace Core.Entidades;

public class Contato : Entidade
{
    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
}