using Core.Entidades.Abstratos;

namespace Core.Entidades;

public class Servico : Entidade
{
    public required string Nome { get; set; }
    public required string Descricao { get; set; }
    public decimal Valor { get; set; }
    public bool Disponivel { get; set; }


    public void Atualizar(string nome, string descricao, decimal? valor, bool? disponivel)
    {
        if (!string.IsNullOrEmpty(nome)) Nome = nome;
        if (!string.IsNullOrEmpty(descricao)) Descricao = descricao;
        if (valor.HasValue) Valor = valor.Value;
        if (disponivel.HasValue) Disponivel = disponivel.Value;
    }
}