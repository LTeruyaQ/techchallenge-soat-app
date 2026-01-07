namespace Core.DTOs.Responses.Estoque
{
    public class EstoqueResponse
    {
        public Guid Id { get; set; }
        public string Insumo { get; set; } = default!;
        public string? Descricao { get; set; }
        public double Preco { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeMinima { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
