using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Estoque
{
    public class CadastrarEstoqueRequest
    {
        [Required]
        [MaxLength(100, ErrorMessage = "O campo deve ter no máximo 100 caracteres.")]
        public string Insumo { get; set; } = default!;

        [MaxLength(500, ErrorMessage = "O campo deve ter no máximo 500 caracteres.")]
        public string? Descricao { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "O valor deve ser no mínimo 1.")]
        public double Preco { get; set; }

        [Required]
        public int QuantidadeDisponivel { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "O valor deve ser no mínimo 1.")]
        public int QuantidadeMinima { get; set; }
    }
}