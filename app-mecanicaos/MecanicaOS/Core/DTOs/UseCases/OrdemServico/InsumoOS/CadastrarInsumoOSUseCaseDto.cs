namespace Core.DTOs.UseCases.OrdemServico.InsumoOS;

public class CadastrarInsumoOSUseCaseDto
{
    public Guid EstoqueId { get; set; }
    public int Quantidade { get; set; }
    public Core.Entidades.Estoque? Estoque { get; set; }
}
