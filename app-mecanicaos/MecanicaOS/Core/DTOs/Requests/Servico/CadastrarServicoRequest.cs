using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Servico
{
    [DisplayName("Cadastro de Serviço")]
    public class CadastrarServicoRequest
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo {1} caracteres")]
        [DisplayName("Nome do Serviço")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "O campo Descrição é obrigatório")]
        [StringLength(500, ErrorMessage = "A Descrição deve ter no máximo {1} caracteres")]
        [DisplayName("Descrição")]
        public required string Descricao { get; set; }

        [Required(ErrorMessage = "O campo Valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O Valor deve ser maior que zero")]
        [DisplayName("Valor")]
        public required decimal Valor { get; set; }

        [Required(ErrorMessage = "O campo Disponível é obrigatório")]
        [DisplayName("Disponível")]
        public required bool Disponivel { get; set; }
    }
}
