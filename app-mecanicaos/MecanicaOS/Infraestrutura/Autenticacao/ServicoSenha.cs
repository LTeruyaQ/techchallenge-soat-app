using BCrypt.Net;
using Core.Interfaces.Servicos;

namespace Infraestrutura.Autenticacao;

public class ServicoSenha : IServicoSenha
{
    public string CriptografarSenha(string senha)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(senha, HashType.SHA512, 12);
    }

    public bool VerificarSenha(string senha, string hashSenha)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(senha, hashSenha, HashType.SHA512);
    }
}
