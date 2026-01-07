using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.Cliente;
using Core.DTOs.Responses.Estoque;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.DTOs.Responses.Servico;
using Core.DTOs.Responses.Veiculo;
using Core.DTOs.UseCases.OrdemServico;
using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Presenters;

namespace Adapters.Presenters
{
    public class OrdemServicoPresenter : IOrdemServicoPresenter
    {
        public CadastrarOrdemServicoUseCaseDto? ParaUseCaseDto(CadastrarOrdemServicoRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarOrdemServicoUseCaseDto
            {
                ClienteId = request.ClienteId,
                VeiculoId = request.VeiculoId,
                ServicoId = request.ServicoId,
                Descricao = request.Descricao
            };
        }

        public AtualizarOrdemServicoUseCaseDto? ParaUseCaseDto(AtualizarOrdemServicoRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarOrdemServicoUseCaseDto
            {
                ClienteId = request.ClienteId,
                VeiculoId = request.VeiculoId,
                ServicoId = request.ServicoId,
                Descricao = request.Descricao,
                Status = request.Status
            };
        }

        public CadastrarInsumoOSUseCaseDto? ParaUseCaseDto(CadastrarInsumoOSRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarInsumoOSUseCaseDto
            {
                EstoqueId = request.EstoqueId,
                Quantidade = request.Quantidade
            };
        }

        public OrdemServicoResponse? ParaResponse(OrdemServico ordemServico)
        {
            if (ordemServico is null)
                return null;

            return new OrdemServicoResponse
            {
                Id = ordemServico.Id,
                ClienteId = ordemServico.ClienteId,
                VeiculoId = ordemServico.VeiculoId,
                ServicoId = ordemServico.ServicoId,
                Orcamento = (double?)ordemServico.Orcamento,
                Descricao = ordemServico.Descricao,
                Status = ordemServico.Status,
                DataEnvioOrcamento = ordemServico.DataEnvioOrcamento,
                Cliente = ordemServico.Cliente == null ? null : new ClienteResponse
                {
                    Id = ordemServico.ClienteId,
                    Nome = ordemServico.Cliente.Nome,
                    Sexo = ordemServico.Cliente.Sexo,
                    Documento = ordemServico.Cliente.Documento,
                    DataNascimento = ordemServico.Cliente?.DataNascimento,
                    TipoCliente = ordemServico.Cliente!.TipoCliente.ToString(),
                    EnderecoId = ordemServico.Cliente!.Endereco?.Id,
                    ContatoId = ordemServico.Cliente.Contato?.Id,
                    DataCadastro = ordemServico.Cliente.DataCadastro.ToString(),
                    DataAtualizacao = ordemServico.Cliente.DataAtualizacao.ToString(),
                    Contato = new ContatoResponse
                    {
                        Email = ordemServico.Cliente.Contato!.Email,
                        Telefone = ordemServico.Cliente.Contato.Telefone
                    }
                },
                Servico = ordemServico.Servico == null ? null : new ServicoResponse
                {
                    Id = ordemServico.ServicoId,
                    Nome = ordemServico.Servico.Nome,
                    Valor = ordemServico.Servico.Valor,
                    Descricao = ordemServico.Servico.Descricao,
                    Disponivel = ordemServico.Servico.Disponivel,
                    DataAtualizacao = ordemServico.Servico.DataAtualizacao,
                    DataCadastro = ordemServico.Servico.DataCadastro
                },
                Veiculo = ordemServico.Veiculo == null ? null : new VeiculoResponse
                {
                    Id = ordemServico.VeiculoId,
                    Placa = ordemServico.Veiculo.Placa,
                    Modelo = ordemServico.Veiculo.Modelo,
                    Marca = ordemServico.Veiculo.Marca,
                    Ano = ordemServico.Veiculo.Ano,
                    Anotacoes = ordemServico.Veiculo.Anotacoes,
                    Cor = ordemServico.Veiculo.Cor,
                    DataAtualizacao = ordemServico.Veiculo.DataAtualizacao,
                    DataCadastro = ordemServico.Veiculo.DataCadastro
                },
                Insumos = ordemServico.InsumosOS?.Select(i => new InsumoOSResponse
                {
                    OrdemServicoId = i.OrdemServicoId,
                    EstoqueId = i.EstoqueId,
                    Estoque = i.Estoque == null ? null : new EstoqueResponse
                    {
                        Id = i.EstoqueId,
                        Insumo = i.Estoque.Insumo,
                        Preco = Convert.ToDouble(i.Estoque.Preco),
                        Descricao = i.Estoque.Descricao,
                        QuantidadeDisponivel = i.Estoque.QuantidadeDisponivel,
                        QuantidadeMinima = i.Estoque.QuantidadeMinima,
                        DataAtualizacao = i.Estoque.DataAtualizacao,
                        DataCadastro = i.Estoque.DataCadastro
                    },
                    Quantidade = i.Quantidade
                })
            };
        }

        public IEnumerable<OrdemServicoResponse?> ParaResponse(IEnumerable<OrdemServico> ordensServico)
        {
            if (ordensServico is null)
                return [];

            return ordensServico.Select(ParaResponse);
        }
    }
}
