using Adapters.Gateways;
using Core.DTOs.Entidades.Estoque;
using Core.Entidades;
using Core.Interfaces.Repositorios;
using Core.Especificacoes.Base.Interfaces;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    /// <summary>
    /// Testes para AlertaEstoqueGateway
    /// Importância: Valida sistema de alertas de estoque crítico.
    /// Garante que alertas sejam cadastrados e consultados corretamente.
    /// </summary>
    public class AlertaEstoqueGatewayTests
    {
        /// <summary>
        /// Verifica se o gateway cadastra vários alertas em lote
        /// Importância: Valida operação em lote de alertas
        /// </summary>
        [Fact]
        public async Task CadastrarVariosAsync_DeveCadastrarAlertas()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<AlertaEstoqueEntityDto>>();
            
            var alertas = new List<AlertaEstoque>
            {
                new AlertaEstoque
                {
                    Id = Guid.NewGuid(),
                    EstoqueId = Guid.NewGuid(),
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                },
                new AlertaEstoque
                {
                    Id = Guid.NewGuid(),
                    EstoqueId = Guid.NewGuid(),
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                }
            };
            
            var alertasDtos = alertas.Select(AlertaEstoqueGateway.ToDto).ToList();
            
            repositorioMock.CadastrarVariosAsync(Arg.Any<IEnumerable<AlertaEstoqueEntityDto>>())
                .Returns(alertasDtos);
            
            var gateway = new AlertaEstoqueGateway(repositorioMock);
            
            // Act
            await gateway.CadastrarVariosAsync(alertas);
            
            // Assert
            await repositorioMock.Received(1).CadastrarVariosAsync(Arg.Is<IEnumerable<AlertaEstoqueEntityDto>>(
                dtos => dtos.Count() == 2));
        }

        /// <summary>
        /// Verifica se o gateway obtém alertas do dia por estoque
        /// Importância: Valida filtro de alertas por data e estoque
        /// </summary>
        [Fact]
        public async Task ObterAlertaDoDiaPorEstoqueAsync_DeveUsarEspecificacao()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<AlertaEstoqueEntityDto>>();
            
            var estoqueId = Guid.NewGuid();
            var dataAtual = DateTime.UtcNow.Date;
            
            var alertas = new List<AlertaEstoque>
            {
                new AlertaEstoque
                {
                    Id = Guid.NewGuid(),
                    EstoqueId = estoqueId,
                    DataCadastro = dataAtual
                }
            };
            
            repositorioMock.ListarProjetadoAsync<AlertaEstoque>(Arg.Any<IEspecificacao<AlertaEstoqueEntityDto>>())
                .Returns(alertas);
            
            var gateway = new AlertaEstoqueGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterAlertaDoDiaPorEstoqueAsync(estoqueId, dataAtual);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.First().EstoqueId.Should().Be(estoqueId);
            
            await repositorioMock.Received(1).ListarProjetadoAsync<AlertaEstoque>(Arg.Any<IEspecificacao<AlertaEstoqueEntityDto>>());
        }

        /// <summary>
        /// Verifica se conversão ToDto preserva todos os campos
        /// Importância: Valida integridade da conversão
        /// </summary>
        [Fact]
        public void ToDto_SemEstoque_DeveConverterCamposBasicos()
        {
            // Arrange
            var alerta = new AlertaEstoque
            {
                Id = Guid.NewGuid(),
                EstoqueId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                Estoque = null
            };
            
            // Act
            var dto = AlertaEstoqueGateway.ToDto(alerta);
            
            // Assert
            dto.Id.Should().Be(alerta.Id);
            dto.EstoqueId.Should().Be(alerta.EstoqueId);
            dto.Ativo.Should().BeTrue();
            dto.Estoque.Should().BeNull();
        }

        /// <summary>
        /// Verifica se conversão ToDto preserva relacionamento com Estoque
        /// Importância: Valida conversão de relacionamentos
        /// </summary>
        [Fact]
        public void ToDto_ComEstoque_DeveConverterRelacionamento()
        {
            // Arrange
            var alerta = new AlertaEstoque
            {
                Id = Guid.NewGuid(),
                EstoqueId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                Estoque = new Estoque
                {
                    Id = Guid.NewGuid(),
                    Insumo = "Óleo 5W30",
                    QuantidadeDisponivel = 2,
                    QuantidadeMinima = 10,
                    Preco = 50.00m,
                    Descricao = "Óleo sintético"
                }
            };
            
            // Act
            var dto = AlertaEstoqueGateway.ToDto(alerta);
            
            // Assert
            dto.Estoque.Should().NotBeNull();
            dto.Estoque!.Insumo.Should().Be("Óleo 5W30");
            dto.Estoque.QuantidadeDisponivel.Should().Be(2);
            dto.Estoque.QuantidadeMinima.Should().Be(10);
        }
    }
}
