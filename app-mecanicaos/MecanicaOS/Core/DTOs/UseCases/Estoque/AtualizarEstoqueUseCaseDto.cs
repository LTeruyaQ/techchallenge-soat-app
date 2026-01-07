namespace Core.DTOs.UseCases.Estoque
{
    public class AtualizarEstoqueUseCaseDto
    {
        public string? Insumo { get; set; } = default!;
        public string? Descricao { get; set; }
        public decimal? Preco { get; set; }
        public int? QuantidadeMinima { get; set; }
        public int? QuantidadeDisponivel { get; set; }
    }
}