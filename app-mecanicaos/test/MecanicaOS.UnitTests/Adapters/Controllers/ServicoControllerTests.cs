using Adapters.Controllers;
using Core.DTOs.Requests.Servico;
using Core.Entidades;
using Core.Interfaces.UseCases;
using Core.Interfaces.root;

namespace MecanicaOS.UnitTests.Adapters.Controllers
{
    /// <summary>
    /// Testes para ServicoController (Adapter)
    /// Cobertura: 75.5%
    /// </summary>
    public class ServicoControllerTests
    {
        [Fact]
        public void Construtor_DeveCriarInstancia()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var servicoUseCases = Substitute.For<IServicoUseCases>();
            compositionRoot.CriarServicoUseCases().Returns(servicoUseCases);

            // Act
            var controller = new ServicoController(compositionRoot);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterTodos_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var servicoUseCases = Substitute.For<IServicoUseCases>();
            var servicos = new List<Servico> { new Servico { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Desc", Valor = 100m } };

            compositionRoot.CriarServicoUseCases().Returns(servicoUseCases);
            servicoUseCases.ObterTodosUseCaseAsync().Returns(Task.FromResult<IEnumerable<Servico>>(servicos));

            var controller = new ServicoController(compositionRoot);

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            await servicoUseCases.Received(1).ObterTodosUseCaseAsync();
        }

        [Fact]
        public async Task ObterServicosDisponiveis_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var servicoUseCases = Substitute.For<IServicoUseCases>();
            var servicos = new List<Servico> { new Servico { Id = Guid.NewGuid(), Nome = "Disponível", Descricao = "Desc", Valor = 100m, Disponivel = true } };

            compositionRoot.CriarServicoUseCases().Returns(servicoUseCases);
            servicoUseCases.ObterServicosDisponiveisUseCaseAsync().Returns(Task.FromResult<IEnumerable<Servico>>(servicos));

            var controller = new ServicoController(compositionRoot);

            // Act
            var resultado = await controller.ObterServicosDisponiveis();

            // Assert
            await servicoUseCases.Received(1).ObterServicosDisponiveisUseCaseAsync();
        }

        [Fact]
        public async Task ObterPorId_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var servicoUseCases = Substitute.For<IServicoUseCases>();
            var id = Guid.NewGuid();
            var servico = new Servico { Id = id, Nome = "Teste", Descricao = "Desc", Valor = 100m };

            compositionRoot.CriarServicoUseCases().Returns(servicoUseCases);
            servicoUseCases.ObterServicoPorIdUseCaseAsync(id).Returns(Task.FromResult<Servico?>(servico));

            var controller = new ServicoController(compositionRoot);

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            await servicoUseCases.Received(1).ObterServicoPorIdUseCaseAsync(id);
        }

        [Fact]
        public async Task Deletar_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var servicoUseCases = Substitute.For<IServicoUseCases>();
            var id = Guid.NewGuid();

            compositionRoot.CriarServicoUseCases().Returns(servicoUseCases);

            var controller = new ServicoController(compositionRoot);

            // Act
            await controller.Deletar(id);

            // Assert
            await servicoUseCases.Received(1).DeletarServicoUseCaseAsync(id);
        }

        [Fact]
        public void MapearParaCadastrarServicoUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var servicoUseCases = Substitute.For<IServicoUseCases>();
            compositionRoot.CriarServicoUseCases().Returns(servicoUseCases);

            var controller = new ServicoController(compositionRoot);
            var request = new CadastrarServicoRequest
            {
                Nome = "Troca de Óleo",
                Descricao = "Serviço completo de troca de óleo",
                Valor = 150.00m,
                Disponivel = true
            };

            // Act
            var resultado = controller.MapearParaCadastrarServicoUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Troca de Óleo");
            resultado.Valor.Should().Be(150.00m);
        }

        [Fact]
        public void MapearParaEditarServicoUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var servicoUseCases = Substitute.For<IServicoUseCases>();
            compositionRoot.CriarServicoUseCases().Returns(servicoUseCases);

            var controller = new ServicoController(compositionRoot);
            var request = new EditarServicoRequest
            {
                Nome = "Serviço Atualizado",
                Descricao = "Nova descrição",
                Valor = 200.00m,
                Disponivel = false
            };

            // Act
            var resultado = controller.MapearParaEditarServicoUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Serviço Atualizado");
            resultado.Disponivel.Should().BeFalse();
        }
    }
}
