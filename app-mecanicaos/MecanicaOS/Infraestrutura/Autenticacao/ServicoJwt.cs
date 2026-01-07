using Core.DTOs.Config;
using Core.Interfaces.Servicos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infraestrutura.Autenticacao;

public class ServicoJwt : IServicoJwt
{
    private readonly ConfiguracaoJwt _configuracaoJwt;

    public ServicoJwt(ConfiguracaoJwt configuracaoJwt)
    {
        _configuracaoJwt = configuracaoJwt;
    }

    public string GerarToken(Guid usuarioId, string email, string tipoUsuario, string? nome = null, IEnumerable<string>? permissoes = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.OutboundClaimTypeMap.Clear();
        var key = Encoding.ASCII.GetBytes(_configuracaoJwt.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, tipoUsuario),
            new Claim("tipo_usuario", tipoUsuario),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(nome))
        {
            claims.Add(new Claim(ClaimTypes.Name, nome));
        }

        if (permissoes != null)
        {
            claims.AddRange(permissoes.Select(permissao => new Claim("permissao", permissao)));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_configuracaoJwt.ExpiryInMinutes),
            Issuer = _configuracaoJwt.Issuer,
            Audience = _configuracaoJwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidarToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuracaoJwt.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuracaoJwt.Issuer,
                ValidAudience = _configuracaoJwt.Audience,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
