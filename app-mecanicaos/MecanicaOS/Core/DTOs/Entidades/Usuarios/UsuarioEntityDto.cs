using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.Entidades.Cliente;
using Core.Enumeradores;

namespace Core.DTOs.Entidades.Usuarios;

public class UsuarioEntityDto : EntityDto
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
    public DateTime? DataUltimoAcesso { get; set; }
    public TipoUsuario TipoUsuario { get; set; }
    public bool RecebeAlertaEstoque { get; set; }

    public Guid? ClienteId { get; set; }
    public ClienteEntityDto? Cliente { get; set; }
}