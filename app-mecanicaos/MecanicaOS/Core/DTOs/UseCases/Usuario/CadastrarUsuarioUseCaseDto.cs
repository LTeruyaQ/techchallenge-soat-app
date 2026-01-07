using Core.Enumeradores;

namespace Core.DTOs.UseCases.Usuario;

public class CadastrarUsuarioUseCaseDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public bool? RecebeAlertaEstoque { get; set; }
    public string? Documento { get; set; }
    public Guid? ClienteId { get; set; } // Controller resolve o ClienteId
}