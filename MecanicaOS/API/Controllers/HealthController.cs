using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller simples para verificacao de saude da aplicacao.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HealthController : BaseApiController
    {
        /// <summary>
        /// Endpoint que retorna um status "ok" para indicar que a aplicacao esta rodando.
        /// Nao possui dependencias externas para garantir uma resposta rapida e confiavel.
        /// </summary>
        /// <returns>HTTP 200 OK com { "status": "ok" }</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult GetHealth()
        {
            return Ok(new { status = "ok" });
        }
    }
}
