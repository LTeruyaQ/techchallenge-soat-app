namespace Core.DTOs.UseCases.Autenticacao
{
    public class AutenticacaoDto
    {
        public string? Token { get; set; }
        public Core.Entidades.Usuario? Usuario { get; set; }
        public List<string>? Permissoes { get; set; }
    }
}
