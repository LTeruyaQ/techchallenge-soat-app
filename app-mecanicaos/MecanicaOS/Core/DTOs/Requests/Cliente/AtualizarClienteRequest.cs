using Core.Enumeradores;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Cliente
{
    [DisplayName("Atualização de Cliente")]
    public class AtualizarClienteRequest
    {

        public Guid? Id { get; set; }

        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo {1} caracteres")]
        [DisplayName("Nome")]
        public string? Nome { get; set; }

        [StringLength(1, ErrorMessage = "O Sexo deve ter 1 caractere (M/F/O)")]
        [RegularExpression(@"^[MFOmfo]?$", ErrorMessage = "O Sexo deve ser M, F ou O")]
        [DisplayName("Sexo")]
        public string? Sexo { get; set; }

        [StringLength(20, ErrorMessage = "O Documento deve ter no máximo {1} caracteres")]
        [DisplayName("Documento")]
        public string? Documento { get; set; }

        [DisplayName("Data de Nascimento")]
        public string? DataNascimento { get; set; }

        [DisplayName("Tipo de Cliente")]
        public TipoCliente? TipoCliente { get; set; }

        [Required(ErrorMessage = "O ID do endereço é obrigatório")]
        [DisplayName("ID do Endereço")]
        public Guid EnderecoId { get; set; }

        [StringLength(200, ErrorMessage = "A Rua deve ter no máximo {1} caracteres")]
        [DisplayName("Rua")]
        public string? Rua { get; set; }

        [StringLength(100, ErrorMessage = "O Bairro deve ter no máximo {1} caracteres")]
        [DisplayName("Bairro")]
        public string? Bairro { get; set; }

        [StringLength(100, ErrorMessage = "A Cidade deve ter no máximo {1} caracteres")]
        [DisplayName("Cidade")]
        public string? Cidade { get; set; }

        [StringLength(20, ErrorMessage = "O Número deve ter no máximo {1} caracteres")]
        [DisplayName("Número")]
        public string? Numero { get; set; }

        [StringLength(9, ErrorMessage = "O CEP deve ter no máximo {1} caracteres")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Formato de CEP inválido. Use o formato: 00000-000 ou 00000000")]
        [DisplayName("CEP")]
        public string? CEP { get; set; }

        [StringLength(100, ErrorMessage = "O Complemento deve ter no máximo {1} caracteres")]
        [DisplayName("Complemento")]
        public string? Complemento { get; set; }

        [Required(ErrorMessage = "O ID do contato é obrigatório")]
        [DisplayName("ID do Contato")]
        public Guid ContatoId { get; set; }

        [EmailAddress(ErrorMessage = "O Email informado não é válido")]
        [StringLength(100, ErrorMessage = "O Email deve ter no máximo {1} caracteres")]
        [DisplayName("E-mail")]
        public string? Email { get; set; }

        [StringLength(15, ErrorMessage = "O Telefone deve ter no máximo {1} caracteres")]
        [RegularExpression(@"^\+?[0-9\s-()]*$", ErrorMessage = "Formato de telefone inválido")]
        [DisplayName("Telefone")]
        public string? Telefone { get; set; }
    }
}
