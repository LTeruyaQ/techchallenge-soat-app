using Core.DTOs.Entidades.Autenticacao;

namespace Core.DTOs.Entidades.Servico;

public class ServicoEntityDto : EntityDto
{
    public required string Nome { get; set; }
    public required string Descricao { get; set; }
    public decimal Valor { get; set; }
    public bool Disponivel { get; set; }
}