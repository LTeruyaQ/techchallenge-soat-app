using Core.Enumeradores;

namespace Core.DTOs.UseCases.Cliente
{
    public class CadastrarClienteUseCaseDto
    {
        public string Nome { get; set; } = null!;
        public string Sexo { get; set; } = null!;
        public string Documento { get; set; } = null!;
        public string DataNascimento { get; set; } = null!;
        public TipoCliente TipoCliente { get; set; }

        public string Rua { get; set; } = null!;
        public string Bairro { get; set; } = null!;
        public string Cidade { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string CEP { get; set; } = null!;
        public string? Complemento { get; set; }

        public string Email { get; set; } = null!;
        public string Telefone { get; set; } = null!;
    }
}