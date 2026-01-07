using Core.DTOs.Entidades.Autenticacao;

namespace Core.DTOs.Entidades.Estoque;

public class AlertaEstoqueEntityDto : EntityDto
{
    public Guid EstoqueId { get; set; }
    public EstoqueEntityDto Estoque { get; set; } = null!;
}