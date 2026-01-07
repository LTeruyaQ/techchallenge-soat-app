using Core.Interfaces.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class PermissaoNecessariaAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _permissao;

    public PermissaoNecessariaAttribute(string permissao)
    {
        _permissao = permissao;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuarioLogadoServico = context.HttpContext.RequestServices.GetService<IUsuarioLogadoServico>();

        if (usuarioLogadoServico == null || !usuarioLogadoServico.EstaAutenticado)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!usuarioLogadoServico.PossuiPermissao(_permissao))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
