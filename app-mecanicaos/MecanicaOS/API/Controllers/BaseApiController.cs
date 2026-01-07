using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ValidarModelState()
    {
        if (!ModelState.IsValid)
        {
            var mensagensErro = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrEmpty(m))
                .ToArray();

            var mensagem = mensagensErro.Length > 0
                ? string.Join("; ", mensagensErro)
                : "Dados inválidos";

            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = mensagem
            });
        }

        return null;
    }
}
