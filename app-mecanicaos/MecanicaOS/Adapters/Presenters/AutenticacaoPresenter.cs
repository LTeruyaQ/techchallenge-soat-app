using Core.DTOs.Responses.Autenticacao;
using Core.DTOs.UseCases.Autenticacao;
using Core.Interfaces.Presenters;

namespace Adapters.Presenters
{

    public class AutenticacaoPresenter : IAutenticacaoPresenter
    {
        public AutenticacaoResponse? ParaResponse(AutenticacaoDto autenticacaoUseCaseDto)
        {
            if (autenticacaoUseCaseDto is null)
                return null;
            return new AutenticacaoResponse()
            {
                Token = autenticacaoUseCaseDto.Token
            };
        }
    }
}
