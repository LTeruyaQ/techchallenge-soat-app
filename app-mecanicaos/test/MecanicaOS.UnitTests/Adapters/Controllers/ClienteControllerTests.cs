using Adapters.Controllers;
using Core.DTOs.Requests.Cliente;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.UseCases;
using Core.Interfaces.root;

namespace MecanicaOS.UnitTests.Adapters.Controllers
{
    /// <summary>
    /// Testes para ClienteController (Adapter)
    /// Cobertura: 85.7%
    /// </summary>
    public class ClienteControllerTests
    {
        [Fact]
        public void Construtor_DeveCriarInstancia()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);

            // Act
            var controller = new ClienteController(compositionRoot);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterTodos_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            var clientes = new List<Cliente> { new Cliente { Id = Guid.NewGuid(), Nome = "Teste", Documento = "123", TipoCliente = TipoCliente.PessoaFisica } };

            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);
            clienteUseCases.ObterTodosUseCaseAsync().Returns(Task.FromResult<IEnumerable<Cliente>>(clientes));

            var controller = new ClienteController(compositionRoot);

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            await clienteUseCases.Received(1).ObterTodosUseCaseAsync();
        }

        [Fact]
        public async Task ObterPorId_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            var id = Guid.NewGuid();
            var cliente = new Cliente { Id = id, Nome = "Teste", Documento = "123", TipoCliente = TipoCliente.PessoaFisica };

            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);
            clienteUseCases.ObterPorIdUseCaseAsync(id).Returns(Task.FromResult<Cliente?>(cliente));

            var controller = new ClienteController(compositionRoot);

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            await clienteUseCases.Received(1).ObterPorIdUseCaseAsync(id);
        }

        [Fact]
        public async Task Remover_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            var id = Guid.NewGuid();

            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);
            clienteUseCases.RemoverUseCaseAsync(id).Returns(Task.FromResult(true));

            var controller = new ClienteController(compositionRoot);

            // Act
            var resultado = await controller.Remover(id);

            // Assert
            resultado.Should().BeTrue();
            await clienteUseCases.Received(1).RemoverUseCaseAsync(id);
        }

        [Fact]
        public async Task ObterPorDocumento_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            var documento = "12345678909";
            var cliente = new Cliente { Id = Guid.NewGuid(), Nome = "Teste", Documento = documento, TipoCliente = TipoCliente.PessoaFisica };

            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);
            clienteUseCases.ObterPorDocumentoUseCaseAsync(documento).Returns(Task.FromResult<Cliente?>(cliente));

            var controller = new ClienteController(compositionRoot);

            // Act
            var resultado = await controller.ObterPorDocumento(documento);

            // Assert
            await clienteUseCases.Received(1).ObterPorDocumentoUseCaseAsync(documento);
        }

        [Fact]
        public void MapearParaCadastrarClienteUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);

            var controller = new ClienteController(compositionRoot);
            var request = new CadastrarClienteRequest
            {
                Nome = "João Silva",
                Documento = "12345678909",
                TipoCliente = TipoCliente.PessoaFisica,
                Email = "joao@email.com",
                Telefone = "11999999999"
            };

            // Act
            var resultado = controller.MapearParaCadastrarClienteUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("João Silva");
            resultado.Documento.Should().Be("12345678909");
        }

        [Fact]
        public void MapearParaAtualizarClienteUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);

            var controller = new ClienteController(compositionRoot);
            var request = new AtualizarClienteRequest
            {
                Nome = "João Silva Atualizado",
                Email = "joao.novo@email.com",
                Telefone = "11888888888"
            };

            // Act
            var resultado = controller.MapearParaAtualizarClienteUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("João Silva Atualizado");
            resultado.Email.Should().Be("joao.novo@email.com");
        }

        [Fact]
        public void MapearParaCadastrarClienteUseCaseDto_ComRequestNull_DeveRetornarNull()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);

            var controller = new ClienteController(compositionRoot);

            // Act
            var resultado = controller.MapearParaCadastrarClienteUseCaseDto(null);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public void MapearParaAtualizarClienteUseCaseDto_ComRequestNull_DeveRetornarNull()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var clienteUseCases = Substitute.For<IClienteUseCases>();
            compositionRoot.CriarClienteUseCases().Returns(clienteUseCases);

            var controller = new ClienteController(compositionRoot);

            // Act
            var resultado = controller.MapearParaAtualizarClienteUseCaseDto(null);

            // Assert
            resultado.Should().BeNull();
        }
    }
}
