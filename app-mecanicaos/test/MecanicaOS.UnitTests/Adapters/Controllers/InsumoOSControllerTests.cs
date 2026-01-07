using Adapters.Controllers;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.UseCases;
using Core.Interfaces.root;

namespace MecanicaOS.UnitTests.Adapters.Controllers
{
    /// <summary>
    /// Testes para InsumoOSController (Adapter)
    /// Cobertura: 30.3%
    /// 
    /// IMPORTÂNCIA: Controller crítico para gestão de insumos em ordens de serviço.
    /// Orquestra validações cross-domain (OrdemServico, Estoque) e atualiza estoque.
    /// Essencial para integridade do controle de estoque.
    /// </summary>
    public class InsumoOSControllerTests
    {
        [Fact]
        public void Construtor_DeveCriarInstancia()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            // Act
            var controller = new InsumoOSController(compositionRoot);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public async Task CadastrarInsumos_ComOrdemServicoValida_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            var ordemServicoId = Guid.NewGuid();
            var estoqueId = Guid.NewGuid();

            var ordemServico = new OrdemServico 
            { 
                Id = ordemServicoId, 
                ClienteId = Guid.NewGuid(), 
                ServicoId = Guid.NewGuid() 
            };

            var estoque = new Estoque 
            { 
                Id = estoqueId, 
                Insumo = "Óleo", 
                QuantidadeDisponivel = 100, 
                QuantidadeMinima = 10, 
                Preco = 50m 
            };

            var requests = new List<CadastrarInsumoOSRequest>
            {
                new CadastrarInsumoOSRequest { EstoqueId = estoqueId, Quantidade = 5 }
            };

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            ordemServicoUseCases.ObterPorIdUseCaseAsync(ordemServicoId).Returns(Task.FromResult<OrdemServico?>(ordemServico));
            estoqueUseCases.ObterPorIdUseCaseAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoque));

            var controller = new InsumoOSController(compositionRoot);

            // Act
            await controller.CadastrarInsumos(ordemServicoId, requests);

            // Assert
            await ordemServicoUseCases.Received(1).ObterPorIdUseCaseAsync(ordemServicoId);
            await estoqueUseCases.Received(1).ObterPorIdUseCaseAsync(estoqueId);
        }

        [Fact]
        public async Task DevolverInsumosAoEstoque_ComInsumosValidos_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            var estoqueId = Guid.NewGuid();
            var estoque = new Estoque 
            { 
                Id = estoqueId, 
                Insumo = "Óleo", 
                QuantidadeDisponivel = 100, 
                QuantidadeMinima = 10, 
                Preco = 50m 
            };

            var requests = new List<DevolverInsumoOSRequest>
            {
                new DevolverInsumoOSRequest { EstoqueId = estoqueId, Quantidade = 5 }
            };

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            estoqueUseCases.ObterPorIdUseCaseAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoque));

            var controller = new InsumoOSController(compositionRoot);

            // Act
            await controller.DevolverInsumosAoEstoque(requests);

            // Assert
            await estoqueUseCases.Received(1).ObterPorIdUseCaseAsync(estoqueId);
        }

        [Fact]
        public async Task CadastrarInsumos_ComOrdemServicoInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            ordemServicoUseCases.ObterPorIdUseCaseAsync(Arg.Any<Guid>()).Returns(Task.FromResult<OrdemServico?>(null));

            var controller = new InsumoOSController(compositionRoot);
            var requests = new List<CadastrarInsumoOSRequest>
            {
                new CadastrarInsumoOSRequest { EstoqueId = Guid.NewGuid(), Quantidade = 5 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<DadosNaoEncontradosException>(async () =>
                await controller.CadastrarInsumos(Guid.NewGuid(), requests));
        }

        [Fact]
        public async Task CadastrarInsumos_ComEstoqueInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            var ordemServicoId = Guid.NewGuid();
            var ordemServico = new OrdemServico 
            { 
                Id = ordemServicoId, 
                ClienteId = Guid.NewGuid(), 
                ServicoId = Guid.NewGuid() 
            };

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            ordemServicoUseCases.ObterPorIdUseCaseAsync(ordemServicoId).Returns(Task.FromResult<OrdemServico?>(ordemServico));
            estoqueUseCases.ObterPorIdUseCaseAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Estoque?>(null));

            var controller = new InsumoOSController(compositionRoot);
            var requests = new List<CadastrarInsumoOSRequest>
            {
                new CadastrarInsumoOSRequest { EstoqueId = Guid.NewGuid(), Quantidade = 5 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<DadosNaoEncontradosException>(async () =>
                await controller.CadastrarInsumos(ordemServicoId, requests));
        }

        [Fact]
        public async Task CadastrarInsumos_ComEstoqueInsuficiente_DeveLancarDadosInvalidosException()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            var ordemServicoId = Guid.NewGuid();
            var estoqueId = Guid.NewGuid();

            var ordemServico = new OrdemServico 
            { 
                Id = ordemServicoId, 
                ClienteId = Guid.NewGuid(), 
                ServicoId = Guid.NewGuid() 
            };

            var estoque = new Estoque 
            { 
                Id = estoqueId, 
                Insumo = "Óleo", 
                QuantidadeDisponivel = 5, // Quantidade insuficiente
                QuantidadeMinima = 10, 
                Preco = 50m 
            };

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            ordemServicoUseCases.ObterPorIdUseCaseAsync(ordemServicoId).Returns(Task.FromResult<OrdemServico?>(ordemServico));
            estoqueUseCases.ObterPorIdUseCaseAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoque));

            var controller = new InsumoOSController(compositionRoot);
            var requests = new List<CadastrarInsumoOSRequest>
            {
                new CadastrarInsumoOSRequest { EstoqueId = estoqueId, Quantidade = 10 } // Solicita mais que disponível
            };

            // Act & Assert
            await Assert.ThrowsAsync<DadosInvalidosException>(async () =>
                await controller.CadastrarInsumos(ordemServicoId, requests));
        }

        [Fact]
        public async Task DevolverInsumosAoEstoque_ComEstoqueInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            estoqueUseCases.ObterPorIdUseCaseAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Estoque?>(null));

            var controller = new InsumoOSController(compositionRoot);
            var requests = new List<DevolverInsumoOSRequest>
            {
                new DevolverInsumoOSRequest { EstoqueId = Guid.NewGuid(), Quantidade = 5 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<DadosNaoEncontradosException>(async () =>
                await controller.DevolverInsumosAoEstoque(requests));
        }

        [Fact]
        public void MapearParaCadastrarInsumoOSUseCaseDto_ComRequestValido_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            var controller = new InsumoOSController(compositionRoot);
            var request = new CadastrarInsumoOSRequest
            {
                EstoqueId = Guid.NewGuid(),
                Quantidade = 10
            };

            // Act
            var resultado = controller.MapearParaCadastrarInsumoOSUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.EstoqueId.Should().Be(request.EstoqueId);
            resultado.Quantidade.Should().Be(request.Quantidade);
        }

        [Fact]
        public void MapearParaCadastrarInsumoOSUseCaseDto_ComRequestNulo_DeveRetornarNull()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var insumoOSUseCases = Substitute.For<IInsumoOSUseCases>();
            var ordemServicoUseCases = Substitute.For<IOrdemServicoUseCases>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();

            compositionRoot.CriarInsumoOSUseCases().Returns(insumoOSUseCases);
            compositionRoot.CriarOrdemServicoUseCases().Returns(ordemServicoUseCases);
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            var controller = new InsumoOSController(compositionRoot);

            // Act
            var resultado = controller.MapearParaCadastrarInsumoOSUseCaseDto(null!);

            // Assert
            resultado.Should().BeNull();
        }
    }
}
