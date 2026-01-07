using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.Entidades.Veiculo;
using Core.Enumeradores;

namespace Core.DTOs.Entidades.Cliente;

public class ClienteEntityDto : EntityDto
{
    public string Nome { get; set; } = default!;
    public string? Sexo { get; set; }
    public string Documento { get; set; } = default!;
    public string DataNascimento { get; set; } = default!;
    public TipoCliente TipoCliente { get; set; }
    public EnderecoEntityDto Endereco { get; set; } = null!;
    public ContatoEntityDto Contato { get; set; } = null!;
    public IEnumerable<VeiculoEntityDto> Veiculos { get; set; } = [];
}