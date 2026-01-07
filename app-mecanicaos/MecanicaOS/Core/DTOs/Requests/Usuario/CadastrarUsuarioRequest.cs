using Core.Enumeradores;
using Core.Validacoes.AtributosValidacao;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Usuario;

[DisplayName("Cadastro de Usuário")]
public class CadastrarUsuarioRequest
{
    [Required(ErrorMessage = "O campo E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "O Email informado não é válido")]
    [StringLength(240, ErrorMessage = "O Email deve ter no máximo {1} caracteres")]
    [DisplayName("E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo Senha é obrigatório")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A Senha deve ter entre {2} e {1} caracteres")]
    [DataType(DataType.Password)]
    [DisplayName("Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo Tipo de Usuário é obrigatório")]
    [DisplayName("Tipo de Usuário")]
    [EnumDataType(typeof(TipoUsuario), ErrorMessage = "Tipo de usuário inválido")]
    public TipoUsuario TipoUsuario { get; set; }

    [DisplayName("Recebe Alerta de Estoque")]
    public bool? RecebeAlertaEstoque { get; set; }

    [StringLength(20, ErrorMessage = "O Documento deve ter no máximo {1} caracteres")]
    [DisplayName("Documento")]
    [CpfOuCnpj(ErrorMessage = "Documento inválido")]
    public string? Documento { get; set; }
}