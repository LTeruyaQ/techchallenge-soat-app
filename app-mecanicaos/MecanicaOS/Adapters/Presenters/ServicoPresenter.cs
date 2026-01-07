using Core.DTOs.Requests.Servico;
using Core.DTOs.Responses.Servico;
using Core.DTOs.UseCases.Servico;
using Core.Entidades;
using Core.Interfaces.Presenters;

namespace Adapters.Presenters
{
    public class ServicoPresenter : IServicoPresenter
    {
        public IEnumerable<ServicoResponse> ParaResponse(IEnumerable<Servico> enumerable)
        {
            if (enumerable is null)
                return [];

            return enumerable.Select(ParaResponse);
        }

        public ServicoResponse ParaResponse(Servico servico)
        {
            return new ServicoResponse
            {
                Id = servico.Id,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                Valor = servico.Valor,
                Disponivel = servico.Disponivel,
                DataCadastro = servico.DataCadastro,
                DataAtualizacao = servico.DataAtualizacao
            };
        }

        public CadastrarServicoUseCaseDto? ParaUseCaseDto(CadastrarServicoRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarServicoUseCaseDto
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                Valor = request.Valor,
                Disponivel = request.Disponivel
            };
        }

        public EditarServicoUseCaseDto? ParaUseCaseDto(EditarServicoRequest request)
        {
            if (request is null)
                return null;

            return new EditarServicoUseCaseDto
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                Valor = request.Valor,
                Disponivel = request.Disponivel
            };
        }
    }
}
