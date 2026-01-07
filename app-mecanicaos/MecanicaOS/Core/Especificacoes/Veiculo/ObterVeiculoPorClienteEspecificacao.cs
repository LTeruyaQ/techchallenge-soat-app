using Core.DTOs.Entidades.Veiculo;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.Veiculo
{
    public class ObterVeiculoPorClienteEspecificacao : EspecificacaoBase<VeiculoEntityDto>
    {
        private readonly Guid _clienteId;

        public ObterVeiculoPorClienteEspecificacao(Guid clienteId)
        {
            _clienteId = clienteId;
            AdicionarInclusao(v => v.Cliente);

            DefinirProjecao(v => new Entidades.Veiculo()
            {
                Id = v.Id,
                ClienteId = v.ClienteId,
                Cliente = new Entidades.Cliente()
                {
                    Id = v.Cliente.Id,
                    Nome = v.Cliente.Nome,
                    Documento = v.Cliente.Documento,
                    DataAtualizacao = v.Cliente.DataAtualizacao,
                    DataCadastro = v.Cliente.DataCadastro,
                    DataNascimento = v.Cliente.DataNascimento,
                    Sexo = v.Cliente.Sexo,
                    TipoCliente = v.Cliente.TipoCliente,
                    Ativo = v.Cliente.Ativo
                },
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
            v => v.ClienteId == _clienteId;
    }
}