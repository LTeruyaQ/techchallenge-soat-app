using Adapters.Gateways;
using Core.DTOs.Entidades.OrdemServicos;
using Core.Entidades;
using Core.Interfaces.Repositorios;
using Core.Especificacoes.Base.Interfaces;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    /// <summary>
    /// Testes para InsumosGateway
    /// Importância: Valida o trânsito de dados entre UseCase e Repositório para Insumos de OS.
    /// Garante que conversões ToDto/FromDto preservem todos os campos técnicos de auditoria.
    /// </summary>
    public class InsumosGatewayTests
    {
        /// <summary>
        /// Verifica se o gateway cadastra vários insumos e preserva campos de auditoria
        /// Importância: Valida operação em lote e integridade dos dados
        /// </summary>
        [Fact]
        public async Task CadastrarVariosAsync_DevePreservarCamposTecnicos()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<InsumoOSEntityDto>>();
            
            var ordemServicoId = Guid.NewGuid();
            var estoqueId = Guid.NewGuid();
            
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    Id = Guid.NewGuid(),
                    OrdemServicoId = ordemServicoId,
                    EstoqueId = estoqueId,
                    Quantidade = 5,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                },
                new InsumoOS
                {
                    Id = Guid.NewGuid(),
                    OrdemServicoId = ordemServicoId,
                    EstoqueId = Guid.NewGuid(),
                    Quantidade = 10,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                }
            };
            
            var insumosDtos = insumos.Select(InsumosGateway.ToDto).ToList();
            
            repositorioMock.CadastrarVariosAsync(Arg.Any<IEnumerable<InsumoOSEntityDto>>())
                .Returns(insumosDtos);
            
            var gateway = new InsumosGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.CadastrarVariosAsync(insumos);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);
            
            var listaResultado = resultado.ToList();
            listaResultado[0].Id.Should().Be(insumos[0].Id, "o ID deve ser preservado");
            listaResultado[0].OrdemServicoId.Should().Be(ordemServicoId, "o OrdemServicoId deve ser preservado");
            listaResultado[0].Quantidade.Should().Be(5, "a quantidade deve ser preservada");
            listaResultado[0].Ativo.Should().BeTrue("o status ativo deve ser preservado");
            
            await repositorioMock.Received(1).CadastrarVariosAsync(Arg.Any<IEnumerable<InsumoOSEntityDto>>());
        }

        /// <summary>
        /// Verifica se o gateway obtém insumos por ordem de serviço usando especificação
        /// Importância: Valida filtro por ordem de serviço
        /// </summary>
        [Fact]
        public async Task ObterInsumosOSPorOSAsync_DeveUsarEspecificacaoERetornarInsumos()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<InsumoOSEntityDto>>();
            
            var ordemServicoId = Guid.NewGuid();
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    Id = Guid.NewGuid(),
                    OrdemServicoId = ordemServicoId,
                    EstoqueId = Guid.NewGuid(),
                    Quantidade = 3,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                }
            };
            
            repositorioMock.ListarProjetadoAsync<InsumoOS>(Arg.Any<IEspecificacao<InsumoOSEntityDto>>())
                .Returns(insumos);
            
            var gateway = new InsumosGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterInsumosOSPorOSAsync(ordemServicoId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.First().OrdemServicoId.Should().Be(ordemServicoId);
            
            await repositorioMock.Received(1).ListarProjetadoAsync<InsumoOS>(Arg.Any<IEspecificacao<InsumoOSEntityDto>>());
        }

        /// <summary>
        /// Verifica se conversão ToDto preserva todos os campos
        /// Importância: Valida integridade da conversão de entidade para DTO
        /// </summary>
        [Fact]
        public void ToDto_DeveConverterTodosOsCampos()
        {
            // Arrange
            var insumo = new InsumoOS
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = Guid.NewGuid(),
                EstoqueId = Guid.NewGuid(),
                Quantidade = 7,
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow.AddHours(1)
            };
            
            // Act
            var dto = InsumosGateway.ToDto(insumo);
            
            // Assert
            dto.Id.Should().Be(insumo.Id);
            dto.OrdemServicoId.Should().Be(insumo.OrdemServicoId);
            dto.EstoqueId.Should().Be(insumo.EstoqueId);
            dto.Quantidade.Should().Be(insumo.Quantidade);
            dto.Ativo.Should().Be(insumo.Ativo);
            dto.DataCadastro.Should().Be(insumo.DataCadastro);
            dto.DataAtualizacao.Should().Be(insumo.DataAtualizacao);
        }

        /// <summary>
        /// Verifica se conversão FromDto preserva todos os campos
        /// Importância: Valida integridade da conversão de DTO para entidade
        /// </summary>
        [Fact]
        public void FromDto_DeveConverterTodosOsCampos()
        {
            // Arrange
            var dto = new InsumoOSEntityDto
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = Guid.NewGuid(),
                EstoqueId = Guid.NewGuid(),
                Quantidade = 7,
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow.AddHours(1)
            };
            
            // Act
            var insumo = InsumosGateway.FromDto(dto);
            
            // Assert
            insumo.Id.Should().Be(dto.Id);
            insumo.OrdemServicoId.Should().Be(dto.OrdemServicoId);
            insumo.EstoqueId.Should().Be(dto.EstoqueId);
            insumo.Quantidade.Should().Be(dto.Quantidade);
            insumo.Ativo.Should().Be(dto.Ativo);
            insumo.DataCadastro.Should().Be(dto.DataCadastro);
            insumo.DataAtualizacao.Should().Be(dto.DataAtualizacao);
        }
    }
}
