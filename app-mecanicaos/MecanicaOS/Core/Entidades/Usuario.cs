using Core.Entidades.Abstratos;
using Core.Enumeradores;

namespace Core.Entidades;

public class Usuario : Entidade
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
    public DateTime? DataUltimoAcesso { get; set; }
    public TipoUsuario TipoUsuario { get; set; }
    public bool RecebeAlertaEstoque { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public Usuario() { }

    public Usuario(string email, string senha, TipoUsuario tipoUsuario, Guid clienteId)
    {
        Email = email;
        Senha = senha;
        TipoUsuario = tipoUsuario;
        ClienteId = clienteId;
    }

    public void Atualizar(string? email = null, string? senha = null, DateTime? dataUltimoAcesso = null, TipoUsuario? tipoUsuario = null, bool? recebeAlertaEstoque = null, bool? ativo = null)
    {
        if (!string.IsNullOrEmpty(email)) Email = email;
        if (!string.IsNullOrEmpty(senha)) Senha = senha;
        if (dataUltimoAcesso != null) DataUltimoAcesso = dataUltimoAcesso;
        if (tipoUsuario != null) TipoUsuario = tipoUsuario.Value;
        if (recebeAlertaEstoque != null) RecebeAlertaEstoque = recebeAlertaEstoque.Value;
        if (ativo != null) Ativo = ativo.Value;
    }

    public void AtualizarUltimoAcesso()
    {
        DataUltimoAcesso = DateTime.UtcNow;
    }

    public void AtualizarUltimoAcesso(DateTime dataAcesso)
    {
        DataUltimoAcesso = dataAcesso;
    }
}