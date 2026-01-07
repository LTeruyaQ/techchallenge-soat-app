namespace Core.Interfaces.Servicos
{
    public interface IServicoJwt
    {
        string GerarToken(
            Guid usuarioId,
            string email,
            string tipoUsuario,
            string? nome = null,
            IEnumerable<string>? permissoes = null);

        bool ValidarToken(string token);
    }
}
