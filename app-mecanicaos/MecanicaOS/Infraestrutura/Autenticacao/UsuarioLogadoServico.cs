using Core.DTOs.Entidades.Usuarios;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Servicos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infraestrutura.Autenticacao;

public class UsuarioLogadoServico : IUsuarioLogadoServico
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepositorio<UsuarioEntityDto> _usuarioRepositorio;

    public UsuarioLogadoServico(
        IHttpContextAccessor httpContextAccessor,
        IRepositorio<UsuarioEntityDto> usuarioRepositorio)
    {
        _httpContextAccessor = httpContextAccessor;
        _usuarioRepositorio = usuarioRepositorio;
    }

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
    private ClaimsPrincipal? Usuario => HttpContext?.User;

    public Guid? UsuarioId
    {
        get
        {
            if (Usuario?.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = Usuario.FindFirst(ClaimTypes.NameIdentifier) ??
                            Usuario.FindFirst("sub") ??
                            Usuario.FindFirst("id");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            return userId;
        }
    }

    public string? Email => Usuario?.FindFirst(ClaimTypes.Email)?.Value;
    public string Nome => Usuario?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public TipoUsuario? TipoUsuario
    {
        get
        {
            if (Usuario?.Identity?.IsAuthenticated != true)
                return null;

            var tipoUsuarioClaim = Usuario.FindFirst("tipo_usuario");
            if (tipoUsuarioClaim == null || !Enum.TryParse<TipoUsuario>(tipoUsuarioClaim.Value, out var tipoUsuario))
                return null;

            return tipoUsuario;
        }
    }

    public bool EstaAutenticado => Usuario?.Identity?.IsAuthenticated ?? false;

    public bool EstaNaRole(string role)
    {
        return Usuario?.IsInRole(role) ?? false;
    }

    public bool PossuiPermissao(string permissao)
    {
        return Usuario?.HasClaim("permissao", permissao) ?? false;
    }

    public IEnumerable<Claim> ObterTodasClaims()
    {
        return Usuario?.Claims ?? Enumerable.Empty<Claim>();
    }

    public Usuario? ObterUsuarioLogado()
    {
        if (!EstaAutenticado || !UsuarioId.HasValue)
            return null;

        var usuarioDto = _usuarioRepositorio.ObterPorIdAsync(UsuarioId.Value).Result;
        if (usuarioDto == null)
            return null;

        return new Usuario
        {
            Id = usuarioDto.Id,
            Ativo = usuarioDto.Ativo,
            DataCadastro = usuarioDto.DataCadastro,
            DataAtualizacao = usuarioDto.DataAtualizacao,
            Email = usuarioDto.Email,
            Senha = usuarioDto.Senha,
            DataUltimoAcesso = usuarioDto.DataUltimoAcesso,
            TipoUsuario = usuarioDto.TipoUsuario,
            RecebeAlertaEstoque = usuarioDto.RecebeAlertaEstoque,
            ClienteId = usuarioDto.ClienteId
        };
    }
}
