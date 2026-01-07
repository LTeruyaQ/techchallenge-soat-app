using Core.DTOs.Entidades.Autenticacao;

namespace Core.DTOs.Entidades.Estoque;

public class EstoqueEntityDto : EntityDto
{
    public string Insumo { get; set; } = default!;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public int QuantidadeMinima { get; set; }
}