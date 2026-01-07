using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Veiculo
{
    [DisplayName("Cadastro de Veículo")]
    public class CadastrarVeiculoRequest
    {
        [Required(ErrorMessage = "O campo Placa é obrigatório")]
        [StringLength(10, ErrorMessage = "A Placa deve ter no máximo {1} caracteres")]
        [RegularExpression(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$|^[A-Z]{3}[0-9]{4}$",
            ErrorMessage = "Formato de placa inválido. Use o formato: ABC1D23 ou ABC1234")]
        [DisplayName("Placa")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Marca é obrigatório")]
        [StringLength(50, ErrorMessage = "A Marca deve ter no máximo {1} caracteres")]
        [DisplayName("Marca")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Modelo é obrigatório")]
        [StringLength(100, ErrorMessage = "O Modelo deve ter no máximo {1} caracteres")]
        [DisplayName("Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Cor é obrigatório")]
        [StringLength(30, ErrorMessage = "A Cor deve ter no máximo {1} caracteres")]
        [DisplayName("Cor")]
        public string Cor { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Ano é obrigatório")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "O Ano deve ter 4 dígitos")]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "O Ano deve conter apenas 4 dígitos")]
        [DisplayName("Ano")]
        public string Ano { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "As Anotações devem ter no máximo {1} caracteres")]
        [DisplayName("Anotações")]
        public string? Anotacoes { get; set; }

        [Required(ErrorMessage = "O ID do Cliente é obrigatório")]
        [DisplayName("ID do Cliente")]
        public Guid ClienteId { get; set; }
    }
}
