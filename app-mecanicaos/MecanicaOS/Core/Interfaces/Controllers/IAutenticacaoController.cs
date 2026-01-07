using Core.DTOs.Requests.Autenticacao;
using Core.DTOs.Responses.Autenticacao;

namespace Core.Interfaces.Controllers
{
    public interface IAutenticacaoController
    {
        Task<AutenticacaoResponse> AutenticarAsync(AutenticacaoRequest autenticacaoRequest);
    }
}