using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.Entidades.Cliente;

namespace Core.DTOs.Entidades.Veiculo;

public class VeiculoEntityDto : EntityDto
{
    public string Placa { get; set; } = default!;
    public string Marca { get; set; } = default!;
    public string Modelo { get; set; } = default!;
    public string Cor { get; set; } = default!;
    public string Ano { get; set; } = default!;
    public string? Anotacoes { get; set; }

    public Guid? ClienteId { get; set; }
    public ClienteEntityDto Cliente { get; set; } = default!;
}
