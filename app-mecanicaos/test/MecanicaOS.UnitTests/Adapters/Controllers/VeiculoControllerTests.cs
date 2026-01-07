using Adapters.Controllers;
using Core.DTOs.Requests.Veiculo;
using Core.Entidades;
using Core.Interfaces.UseCases;
using Core.Interfaces.root;

namespace MecanicaOS.UnitTests.Adapters.Controllers
{
    /// <summary>
    /// Testes para VeiculoController (Adapter)
    /// Cobertura: 79.3%
    /// 
    /// IMPORTÂNCIA: Controller crítico para gestão de veículos dos clientes.
    /// Valida orquestração entre VeiculoUseCases e apresentação de dados.
    /// </summary>
    public class VeiculoControllerTests
    {
        [Fact]
        public void Construtor_DeveCriarInstancia()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);

            // Act
            var controller = new VeiculoController(compositionRoot);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterTodos_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            var veiculos = new List<Veiculo> 
            { 
                new Veiculo 
                { 
                    Id = Guid.NewGuid(), 
                    Placa = "ABC1234", 
                    Marca = "Toyota", 
                    Modelo = "Corolla", 
                    Ano = "2020",
                    ClienteId = Guid.NewGuid()
                } 
            };

            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);
            veiculoUseCases.ObterTodosUseCaseAsync().Returns(Task.FromResult<IEnumerable<Veiculo>>(veiculos));

            var controller = new VeiculoController(compositionRoot);

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            await veiculoUseCases.Received(1).ObterTodosUseCaseAsync();
        }

        [Fact]
        public async Task ObterPorId_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            var id = Guid.NewGuid();
            var veiculo = new Veiculo 
            { 
                Id = id, 
                Placa = "ABC1234", 
                Marca = "Toyota", 
                Modelo = "Corolla", 
                Ano = "2020",
                ClienteId = Guid.NewGuid()
            };

            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);
            veiculoUseCases.ObterPorIdUseCaseAsync(id).Returns(Task.FromResult<Veiculo?>(veiculo));

            var controller = new VeiculoController(compositionRoot);

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            await veiculoUseCases.Received(1).ObterPorIdUseCaseAsync(id);
        }

        [Fact]
        public async Task ObterPorPlaca_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            var placa = "ABC1234";
            var veiculo = new Veiculo 
            { 
                Id = Guid.NewGuid(), 
                Placa = placa, 
                Marca = "Toyota", 
                Modelo = "Corolla", 
                Ano = "2020",
                ClienteId = Guid.NewGuid()
            };

            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);
            veiculoUseCases.ObterPorPlacaUseCaseAsync(placa).Returns(Task.FromResult<Veiculo?>(veiculo));

            var controller = new VeiculoController(compositionRoot);

            // Act
            var resultado = await controller.ObterPorPlaca(placa);

            // Assert
            await veiculoUseCases.Received(1).ObterPorPlacaUseCaseAsync(placa);
        }

        [Fact]
        public async Task ObterPorCliente_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            var clienteId = Guid.NewGuid();
            var veiculos = new List<Veiculo> 
            { 
                new Veiculo 
                { 
                    Id = Guid.NewGuid(), 
                    Placa = "ABC1234", 
                    Marca = "Toyota", 
                    Modelo = "Corolla", 
                    Ano = "2020",
                    ClienteId = clienteId
                } 
            };

            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);
            veiculoUseCases.ObterPorClienteUseCaseAsync(clienteId).Returns(Task.FromResult<IEnumerable<Veiculo>>(veiculos));

            var controller = new VeiculoController(compositionRoot);

            // Act
            var resultado = await controller.ObterPorCliente(clienteId);

            // Assert
            await veiculoUseCases.Received(1).ObterPorClienteUseCaseAsync(clienteId);
        }

        [Fact]
        public async Task Deletar_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            var id = Guid.NewGuid();

            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);
            veiculoUseCases.DeletarUseCaseAsync(id).Returns(Task.FromResult(true));

            var controller = new VeiculoController(compositionRoot);

            // Act
            var resultado = await controller.Deletar(id);

            // Assert
            resultado.Should().BeTrue();
            await veiculoUseCases.Received(1).DeletarUseCaseAsync(id);
        }

        [Fact]
        public void MapearParaCadastrarVeiculoUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);

            var controller = new VeiculoController(compositionRoot);
            var request = new CadastrarVeiculoRequest
            {
                Placa = "ABC1234",
                Marca = "Toyota",
                Modelo = "Corolla",
                Ano = "2020",
                ClienteId = Guid.NewGuid()
            };

            // Act
            var resultado = controller.MapearParaCadastrarVeiculoUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Placa.Should().Be("ABC1234");
            resultado.Marca.Should().Be("Toyota");
        }

        [Fact]
        public void MapearParaAtualizarVeiculoUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var veiculoUseCases = Substitute.For<IVeiculoUseCases>();
            compositionRoot.CriarVeiculoUseCases().Returns(veiculoUseCases);

            var controller = new VeiculoController(compositionRoot);
            var request = new AtualizarVeiculoRequest
            {
                Placa = "UPD1234",
                Marca = "Ford",
                Modelo = "Focus",
                Ano = "2021"
            };

            // Act
            var resultado = controller.MapearParaAtualizarVeiculoUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Placa.Should().Be("UPD1234");
            resultado.Marca.Should().Be("Ford");
        }
    }
}
