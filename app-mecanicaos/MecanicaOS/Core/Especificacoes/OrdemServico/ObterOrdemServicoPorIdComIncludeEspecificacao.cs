using Core.DTOs.Entidades.OrdemServicos;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.OrdemServico;

public class ObterOrdemServicoPorIdComIncludeEspecificacao : EspecificacaoBase<OrdemServicoEntityDto>
{
    private readonly Guid _id;

    public ObterOrdemServicoPorIdComIncludeEspecificacao(Guid id)
    {
        _id = id;
        AdicionarInclusao(os => os.Servico);
        AdicionarInclusao(os => os.Veiculo);
        AdicionarInclusao(os => os.Cliente.Contato);
        AdicionarInclusao(os => os.InsumosOS, io => io.Estoque);

        DefinirProjecao(os => new Entidades.OrdemServico
        {
            Id = os.Id,
            Ativo = os.Ativo,
            DataCadastro = os.DataCadastro,
            DataAtualizacao = os.DataAtualizacao,
            ClienteId = os.ClienteId,
            ServicoId = os.ServicoId,
            Orcamento = os.Orcamento,
            DataEnvioOrcamento = os.DataEnvioOrcamento,
            Descricao = os.Descricao,
            Status = os.Status,
            Servico = new Entidades.Servico()
            {
                Id = os.Servico.Id,
                Nome = os.Servico.Nome,
                Descricao = os.Servico.Descricao,
                Valor = os.Servico.Valor,
                Ativo = os.Servico.Ativo,
                DataCadastro = os.Servico.DataCadastro,
                DataAtualizacao = os.Servico.DataAtualizacao
            },
            VeiculoId = os.VeiculoId,
            Veiculo = new Entidades.Veiculo()
            {
                Id = os.Veiculo.Id,
                Placa = os.Veiculo.Placa,
                Modelo = os.Veiculo.Modelo,
                Marca = os.Veiculo.Marca,
                Ano = os.Veiculo.Ano,
                ClienteId = os.Veiculo.ClienteId,
                Ativo = os.Veiculo.Ativo,
                DataCadastro = os.Veiculo.DataCadastro,
                DataAtualizacao = os.Veiculo.DataAtualizacao
            },
            Cliente = new()
            {
                Id = os.Cliente.Id,
                Nome = os.Cliente.Nome,
                Documento = os.Cliente.Documento,
                Contato = new()
                {
                    Id = os.Cliente.Contato.Id,
                    Email = os.Cliente.Contato.Email,
                    Telefone = os.Cliente.Contato.Telefone,
                    IdCliente = os.Cliente.Contato.IdCliente
                }
            },
            InsumosOS = os.InsumosOS.Select(io => new Entidades.InsumoOS
            {
                Id = io.Id,
                OrdemServicoId = io.OrdemServicoId,
                Quantidade = io.Quantidade,
                DataAtualizacao = io.DataAtualizacao,
                DataCadastro = io.DataCadastro,
                EstoqueId = io.EstoqueId,
                Estoque = new()
                {
                    Id = io.Estoque.Id,
                    Insumo = io.Estoque.Insumo,
                    QuantidadeDisponivel = io.Estoque.QuantidadeDisponivel,
                    Preco = io.Estoque.Preco,
                    Descricao = io.Estoque.Descricao,
                }
            }).ToList()
        });
    }

    public override Expression<Func<OrdemServicoEntityDto, bool>> Expressao => os => os.Id == _id;
}
