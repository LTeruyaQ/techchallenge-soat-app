using Core.DTOs.Requests.Veiculo;
using Core.DTOs.Responses.Veiculo;
using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Interfaces.Presenters;

namespace Adapters.Presenters
{
    public class VeiculoPresenter : IVeiculoPresenter
    {
        public CadastrarVeiculoUseCaseDto? ParaUseCaseDto(CadastrarVeiculoRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarVeiculoUseCaseDto
            {
                Placa = request.Placa,
                Marca = request.Marca,
                Modelo = request.Modelo,
                Cor = request.Cor,
                Ano = request.Ano,
                Anotacoes = request.Anotacoes,
                ClienteId = request.ClienteId
            };
        }

        public AtualizarVeiculoUseCaseDto? ParaUseCaseDto(AtualizarVeiculoRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarVeiculoUseCaseDto
            {
                Placa = request.Placa,
                Marca = request.Marca,
                Modelo = request.Modelo,
                Cor = request.Cor,
                Ano = request.Ano,
                Anotacoes = request.Anotacoes,
                ClienteId = request.ClienteId
            };
        }

        public VeiculoResponse? ParaResponse(Veiculo veiculo)
        {
            if (veiculo is null)
                return null;

            return new VeiculoResponse
            {
                Id = veiculo.Id,
                Placa = veiculo.Placa,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Cor = veiculo.Cor,
                Ano = veiculo.Ano,
                Anotacoes = veiculo.Anotacoes,
                ClienteId = veiculo.ClienteId,
                ClienteNome = veiculo.Cliente?.Nome ?? string.Empty,
                DataCadastro = veiculo.DataCadastro,
                DataAtualizacao = veiculo.DataAtualizacao
            };
        }

        public IEnumerable<VeiculoResponse?> ParaResponse(IEnumerable<Veiculo> veiculos)
        {
            if (veiculos is null)
                return [];

            return veiculos.Select(ParaResponse);
        }
    }
}
