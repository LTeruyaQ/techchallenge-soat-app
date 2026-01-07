using Core.Entidades.Abstratos;

namespace Core.Entidades;

public class AlertaEstoque : Entidade
{
    public Guid EstoqueId { get; set; }
    public Estoque Estoque { get; set; } = null!;
}