using Core.DTOs.Entidades.Veiculo;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.Veiculo
{
    public class ObterVeiculoPorPlacaEspecificacao : EspecificacaoBase<VeiculoEntityDto>
    {
        private readonly string _placa;

        public ObterVeiculoPorPlacaEspecificacao(string placa)
        {
            _placa = placa;
            DefinirProjecao(v => new Entidades.Veiculo()
            {
                Id = v.Id,
                ClienteId = v.ClienteId,
                Placa = v.Placa,
                Marca = v.Marca,
                Modelo = v.Modelo,
                Ano = v.Ano,
                Cor = v.Cor,
                Anotacoes = v.Anotacoes,
                DataCadastro = v.DataCadastro,
                DataAtualizacao = v.DataAtualizacao,
                Ativo = v.Ativo
            });
        }

        public override Expression<Func<VeiculoEntityDto, bool>> Expressao =>
            v => v.Placa == _placa;
    }
}
