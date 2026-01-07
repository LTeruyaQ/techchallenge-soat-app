using Core.DTOs.Entidades.OrdemServicos;
using Core.Entidades;
using Core.Enumeradores;
using Core.Especificacoes.OrdemServico;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class OrdemServicoGateway : IOrdemServicoGateway
    {
        private readonly IRepositorio<OrdemServicoEntityDto> _repositorioOrdemServico;

        public OrdemServicoGateway(IRepositorio<OrdemServicoEntityDto> repositorioOrdemServico)
        {
            _repositorioOrdemServico = repositorioOrdemServico;
        }

        public async Task<OrdemServico> CadastrarAsync(OrdemServico ordemServico)
        {
            var dto = ToDto(ordemServico);
            var result = await _repositorioOrdemServico.CadastrarAsync(dto);
            return FromDto(result);
        }

        public async Task EditarAsync(OrdemServico ordemServico)
        {
            await _repositorioOrdemServico.EditarAsync(ToDto(ordemServico));
        }

        public async Task EditarVariosAsync(IEnumerable<OrdemServico> ordensServico)
        {
            var dtos = ordensServico.Select(ToDto);
            await _repositorioOrdemServico.EditarVariosAsync(dtos);
        }

        public async Task<IEnumerable<OrdemServico>> ListarOSOrcamentoExpiradoAsync()
        {
            var especificacao = new ObterOSOrcamentoExpiradoProjetadoEspecificacao();
            return await _repositorioOrdemServico.ListarProjetadoAsync<OrdemServico>(especificacao);
        }

        public async Task<OrdemServico?> ObterOrdemServicoPorIdComInsumos(Guid id)
        {
            var especificacao = new ObterOrdemServicoPorIdComInsumosEspecificacao(id);
            return await _repositorioOrdemServico.ObterUmProjetadoSemRastreamentoAsync<OrdemServico>(especificacao);
        }

        public async Task<IEnumerable<OrdemServico>> ObterOrdemServicoPorStatusAsync(StatusOrdemServico status)
        {
            var especificacao = new ObterOrdemServicoPorStatusEspecificacao(status);
            return await _repositorioOrdemServico.ListarProjetadoAsync<OrdemServico>(especificacao);
        }

        public async Task<OrdemServico?> ObterPorIdAsync(Guid id)
        {
            var especificacao = new ObterOrdemServicoPorIdComIncludeEspecificacao(id);
            return await _repositorioOrdemServico.ObterUmProjetadoSemRastreamentoAsync<OrdemServico>(especificacao);
        }

        public async Task<IEnumerable<OrdemServico>> ObterTodosAsync()
        {
            var dtos = await _repositorioOrdemServico.ObterTodosAsync();
            return dtos.Select(FromDto);
        }

        public async Task<IEnumerable<OrdemServico>> ObterOrdensServicoAtivasAsync()
        {
            var especificacao = new ObterOrdensServicoAtivasComIncludeEspecificacao();
            return await _repositorioOrdemServico.ListarProjetadoAsync<OrdemServico>(especificacao);
        }

        public static OrdemServicoEntityDto ToDto(OrdemServico ordemServico)
        {
            return new OrdemServicoEntityDto
            {
                Id = ordemServico.Id,
                Ativo = ordemServico.Ativo,
                DataCadastro = ordemServico.DataCadastro,
                DataAtualizacao = ordemServico.DataAtualizacao,
                ClienteId = ordemServico.ClienteId,
                VeiculoId = ordemServico.VeiculoId,
                ServicoId = ordemServico.ServicoId,
                Orcamento = ordemServico.Orcamento,
                DataEnvioOrcamento = ordemServico.DataEnvioOrcamento,
                Descricao = ordemServico.Descricao,
                Status = ordemServico.Status,
                InsumosOS = ordemServico.InsumosOS.Select(insumo => new InsumoOSEntityDto
                {
                    Id = insumo.Id,
                    Ativo = insumo.Ativo,
                    DataCadastro = insumo.DataCadastro,
                    DataAtualizacao = insumo.DataAtualizacao,
                    OrdemServicoId = insumo.OrdemServicoId,
                    EstoqueId = insumo.EstoqueId,
                    Quantidade = insumo.Quantidade
                }).ToList()
            };
        }

        public static OrdemServico FromDto(OrdemServicoEntityDto dto)
        {
            var ordemServico = new OrdemServico
            {
                Id = dto.Id,
                Ativo = dto.Ativo,
                DataCadastro = dto.DataCadastro,
                DataAtualizacao = dto.DataAtualizacao,
                ClienteId = dto.ClienteId,
                VeiculoId = dto.VeiculoId,
                ServicoId = dto.ServicoId,
                Orcamento = dto.Orcamento,
                DataEnvioOrcamento = dto.DataEnvioOrcamento,
                Descricao = dto.Descricao,
                Status = dto.Status,
                InsumosOS = dto.InsumosOS.Select(insumoDto => new InsumoOS
                {
                    Id = insumoDto.Id,
                    Ativo = insumoDto.Ativo,
                    DataCadastro = insumoDto.DataCadastro,
                    DataAtualizacao = insumoDto.DataAtualizacao,
                    OrdemServicoId = insumoDto.OrdemServicoId,
                    EstoqueId = insumoDto.EstoqueId,
                    Quantidade = insumoDto.Quantidade
                }).ToList()
            };

            if (dto.InsumosOS.Any())
                ordemServico.InsumosOS = dto.InsumosOS.Select(insumoDto =>
                {
                    var insumo = new InsumoOS
                    {
                        Id = insumoDto.Id,
                        Ativo = insumoDto.Ativo,
                        DataCadastro = insumoDto.DataCadastro,
                        DataAtualizacao = insumoDto.DataAtualizacao,
                        OrdemServicoId = insumoDto.OrdemServicoId,
                        EstoqueId = insumoDto.EstoqueId,
                        Quantidade = insumoDto.Quantidade
                    };

                    if (insumoDto.Estoque is not null)
                        insumo.Estoque = new Estoque
                        {
                            Id = insumoDto.Estoque.Id,
                            Ativo = insumoDto.Estoque.Ativo,
                            DataCadastro = insumoDto.Estoque.DataCadastro,
                            DataAtualizacao = insumoDto.Estoque.DataAtualizacao,
                            Insumo = insumoDto.Estoque.Insumo,
                            QuantidadeMinima = insumoDto.Estoque.QuantidadeMinima,
                            QuantidadeDisponivel = insumoDto.Estoque.QuantidadeDisponivel,
                            Preco = insumoDto.Estoque.Preco,
                            Descricao = insumoDto.Estoque.Descricao,
                        };

                    return insumo;
                });

            if (dto.Cliente is not null)
            {
                ordemServico.Cliente = new Cliente
                {
                    Id = dto.Cliente.Id,
                    Ativo = dto.Cliente.Ativo,
                    DataCadastro = dto.Cliente.DataCadastro,
                    DataAtualizacao = dto.Cliente.DataAtualizacao,
                    Nome = dto.Cliente.Nome,
                    Documento = dto.Cliente.Documento
                };

                if (dto.Cliente.Contato is not null)
                {
                    ordemServico.Cliente.Contato = new Contato
                    {
                        Id = dto.Cliente.Contato.Id,
                        Ativo = dto.Cliente.Contato.Ativo,
                        DataCadastro = dto.Cliente.Contato.DataCadastro,
                        DataAtualizacao = dto.Cliente.Contato.DataAtualizacao,
                        Email = dto.Cliente.Contato.Email,
                        Telefone = dto.Cliente.Contato.Telefone,
                        IdCliente = dto.Cliente.Contato.IdCliente
                    };
                }
            }

            return ordemServico;
        }
    }
}
