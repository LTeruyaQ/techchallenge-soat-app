
namespace Core.DTOs.UseCases.Autenticacao
{
    public class AutenticacaoUseCaseDto
    {
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public Core.Entidades.Usuario UsuarioExistente { get; set; }
    }
}
