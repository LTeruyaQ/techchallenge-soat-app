using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Estoque
{
    public class AtualizarEstoqueRequest
    {
        [MaxLength(100, ErrorMessage = "O campo deve ter no máximo 100 caracteres.")]
        public string? Insumo { get; set; } = default!;

        [MaxLength(500, ErrorMessage = "O campo deve ter no máximo 500 caracteres.")]
        public string? Descricao { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O valor deve ser no mínimo 1.")]
        public decimal? Preco { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O valor deve ser no mínimo 1.")]
        public int? QuantidadeMinima { get; set; }

        public int? QuantidadeDisponivel { get; set; }
    }
}