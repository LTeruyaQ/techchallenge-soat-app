using Core.Entidades.Abstratos;
using Core.Enumeradores;

namespace Core.Entidades;

public class Cliente : Entidade
{
    public string Nome { get; set; } = default!;
    public string? Sexo { get; set; }
    public string Documento { get; set; } = default!;
    public string DataNascimento { get; set; } = default!;
    public TipoCliente TipoCliente { get; set; }
    public Endereco Endereco { get; set; } = null!;
    public Contato Contato { get; set; } = null!;
    public IEnumerable<Veiculo> Veiculos { get; set; } = [];

    public void Atualizar(string? nome, string? sexo, TipoCliente? tipoCliente, string? dtNascimento)
    {
        if (!string.IsNullOrEmpty(nome)) Nome = nome;
        if (!string.IsNullOrEmpty(sexo)) Sexo = sexo;
        if (tipoCliente.HasValue) TipoCliente = tipoCliente.Value;
        if (!string.IsNullOrEmpty(dtNascimento)) DataNascimento = dtNascimento;
    }
}