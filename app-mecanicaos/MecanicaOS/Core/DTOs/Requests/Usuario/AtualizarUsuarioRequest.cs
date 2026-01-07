using Core.Enumeradores;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Usuario;

[DisplayName("Atualização de Usuário")]
public class AtualizarUsuarioRequest
{
    [EmailAddress(ErrorMessage = "O Email informado não é válido")]
    [StringLength(240, ErrorMessage = "O Email deve ter no máximo {1} caracteres")]
    [DisplayName("E-mail")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "A Senha deve ter entre {2} e {1} caracteres")]
    [DisplayName("Senha")]
    public string? Senha { get; set; }

    [DisplayName("Data do Último Acesso")]
    public DateTime? DataUltimoAcesso { get; set; }

    [DisplayName("Tipo de Usuário")]
    [EnumDataType(typeof(TipoUsuario), ErrorMessage = "Tipo de usuário inválido")]
    public TipoUsuario? TipoUsuario { get; set; }

    [DisplayName("Recebe Alerta de Estoque")]
    public bool? RecebeAlertaEstoque { get; set; }
}