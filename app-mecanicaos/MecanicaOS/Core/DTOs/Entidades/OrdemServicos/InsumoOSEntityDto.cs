using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.Entidades.Estoque;

namespace Core.DTOs.Entidades.OrdemServicos;

public class InsumoOSEntityDto : EntityDto
{
    public Guid OrdemServicoId { get; set; }
    public OrdemServicoEntityDto OrdemServico { get; set; } = null!;
    public Guid EstoqueId { get; set; }
    public EstoqueEntityDto Estoque { get; set; } = null!;
    public int Quantidade { get; set; }
}