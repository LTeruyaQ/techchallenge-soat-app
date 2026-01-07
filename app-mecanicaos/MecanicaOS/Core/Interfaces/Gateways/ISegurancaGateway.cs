namespace Core.Interfaces.Gateways
{
    public interface ISegurancaGateway
    {
        /// <summary>
        /// Verifica se a senha fornecida corresponde ao hash armazenado
        /// </summary>
        /// <param name="senhaPlano">Senha em texto plano</param>
        /// <param name="hashArmazenado">Hash armazenado da senha</param>
        /// <returns>True se a senha for válida, False caso contrário</returns>
        bool VerificarSenha(string senhaPlano, string hashArmazenado);

        /// <summary>
        /// Gera um token JWT com as informações do usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="email">Email do usuário</param>
        /// <param name="tipoUsuario">Tipo do usuário</param>
        /// <param name="clienteId">ID do cliente (opcional)</param>
        /// <param name="permissoes">Lista de permissões do usuário</param>
        /// <returns>Token JWT gerado</returns>
        string GerarToken(Guid usuarioId, string email, string tipoUsuario, Guid? clienteId, IEnumerable<string> permissoes);
    }
}
