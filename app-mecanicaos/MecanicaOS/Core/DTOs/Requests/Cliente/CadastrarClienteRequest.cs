using Core.Enumeradores;
using Core.Validacoes.AtributosValidacao;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Cliente
{
    [DisplayName("Cadastro de Cliente")]
    public class CadastrarClienteRequest
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo {1} caracteres")]
        [DisplayName("Nome Completo")]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "O campo Sexo é obrigatório")]
        [StringLength(1, ErrorMessage = "O Sexo deve ter 1 caractere (M/F/O)")]
        [RegularExpression(@"^[MFOmfo]$", ErrorMessage = "O Sexo deve ser M, F ou O")]
        [DisplayName("Sexo")]
        public string Sexo { get; set; } = null!;

        [Required(ErrorMessage = "O campo Documento é obrigatório")]
        [StringLength(20, ErrorMessage = "O Documento deve ter no máximo {1} caracteres")]
        [DisplayName("Documento")]
        [CpfOuCnpj(ErrorMessage = "Documento inválido")]
        public string Documento { get; set; } = null!;

        [Required(ErrorMessage = "O campo Data de Nascimento é obrigatório")]
        [DisplayName("Data de Nascimento")]
        public string DataNascimento { get; set; } = null!;

        [Required(ErrorMessage = "O campo Tipo de Cliente é obrigatório")]
        [DisplayName("Tipo de Cliente")]
        public TipoCliente TipoCliente { get; set; }

        [Required(ErrorMessage = "O campo Rua é obrigatório")]
        [StringLength(200, ErrorMessage = "A Rua deve ter no máximo {1} caracteres")]
        [DisplayName("Rua")]
        public string Rua { get; set; } = null!;

        [Required(ErrorMessage = "O campo Bairro é obrigatório")]
        [StringLength(100, ErrorMessage = "O Bairro deve ter no máximo {1} caracteres")]
        [DisplayName("Bairro")]
        public string Bairro { get; set; } = null!;

        [Required(ErrorMessage = "O campo Cidade é obrigatório")]
        [StringLength(100, ErrorMessage = "A Cidade deve ter no máximo {1} caracteres")]
        [DisplayName("Cidade")]
        public string Cidade { get; set; } = null!;

        [Required(ErrorMessage = "O campo Número é obrigatório")]
        [StringLength(20, ErrorMessage = "O Número deve ter no máximo {1} caracteres")]
        [DisplayName("Número")]
        public string Numero { get; set; } = null!;

        [Required(ErrorMessage = "O campo CEP é obrigatório")]
        [StringLength(9, ErrorMessage = "O CEP deve ter no máximo {1} caracteres")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Formato de CEP inválido. Use o formato: 00000-000 ou 00000000")]
        [DisplayName("CEP")]
        public string CEP { get; set; } = null!;

        [StringLength(100, ErrorMessage = "O Complemento deve ter no máximo {1} caracteres")]
        [DisplayName("Complemento")]
        public string? Complemento { get; set; }

        [Required(ErrorMessage = "O campo E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "O Email informado não é válido")]
        [StringLength(100, ErrorMessage = "O Email deve ter no máximo {1} caracteres")]
        [DisplayName("E-mail")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O Telefone deve ter no máximo {1} caracteres")]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?9?\d{4}[-\s]?\d{4}$", ErrorMessage = "Formato de telefone inválido")]
        [DisplayName("Telefone")]
        public string Telefone { get; set; } = null!;
    }
}