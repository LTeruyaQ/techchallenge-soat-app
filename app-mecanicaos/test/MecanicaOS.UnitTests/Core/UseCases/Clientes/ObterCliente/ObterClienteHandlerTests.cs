using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Clientes.ObterCliente;
using MecanicaOS.UnitTests.Fixtures;
using NSubstitute;

namespace MecanicaOS.UnitTests.Core.UseCases.Clientes.ObterCliente
{
    /// <summary>
    /// Testes unitários para o handler ObterClienteHandler
    /// </summary>
    public class ObterClienteHandlerTests
    {
        /// <summary>
        /// Verifica se o handler retorna o cliente quando encontrado pelo ID
        /// </summary>
        [Fact]
        public async Task Handle_ComIdExistente_DeveRetornarCliente()
        {
            // Arrange
            var clienteGatewayMock = Substitute.For<IClienteGateway>();
            var cliente = ClienteFixture.CriarClienteValido();
            var clienteId = cliente.Id;
            
            clienteGatewayMock.ObterPorIdAsync(clienteId).Returns(cliente);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterClienteHandler>>();
            var unidadeDeTrabalhoMock = Substitute.For<IUnidadeDeTrabalhoGateway>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterClienteHandler(
                clienteGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(clienteId);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Id.Should().Be(clienteId, "o ID deve corresponder ao cliente buscado");
            resultado.Nome.Should().Be(cliente.Nome, "o nome deve corresponder ao cliente buscado");
            resultado.Documento.Should().Be(cliente.Documento, "o documento deve corresponder ao cliente buscado");
            
            await clienteGatewayMock.Received(1).ObterPorIdAsync(clienteId);
        }

        /// <summary>
        /// Verifica se o handler retorna null quando o cliente não é encontrado pelo ID
        /// </summary>
        [Fact]
        public async Task Handle_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var clienteGatewayMock = Substitute.For<IClienteGateway>();
            var clienteId = Guid.NewGuid();
            
            clienteGatewayMock.ObterPorIdAsync(clienteId).Returns((Cliente)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterClienteHandler>>();
            var unidadeDeTrabalhoMock = Substitute.For<IUnidadeDeTrabalhoGateway>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterClienteHandler(
                clienteGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(clienteId);
            
            // Assert
            resultado.Should().BeNull("o resultado deve ser nulo quando o cliente não é encontrado");
            
            await clienteGatewayMock.Received(1).ObterPorIdAsync(clienteId);
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando ocorre erro ao buscar o cliente
        /// </summary>
        [Fact]
        public async Task Handle_QuandoOcorreErro_DeveLancarException()
        {
            // Arrange
            var clienteGatewayMock = Substitute.For<IClienteGateway>();
            var clienteId = Guid.NewGuid();
            
            clienteGatewayMock.ObterPorIdAsync(clienteId)
                .Returns(Task.FromException<Cliente>(new Exception("Erro ao buscar cliente")));
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterClienteHandler>>();
            var unidadeDeTrabalhoMock = Substitute.For<IUnidadeDeTrabalhoGateway>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterClienteHandler(
                clienteGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(clienteId);
            
            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao buscar cliente");
            
            await clienteGatewayMock.Received(1).ObterPorIdAsync(clienteId);
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando o ID é vazio
        /// </summary>
        [Fact]
        public async Task Handle_ComIdVazio_DeveLancarDadosInvalidosException()
        {
            // Arrange
            var clienteGatewayMock = Substitute.For<IClienteGateway>();
            var clienteId = Guid.Empty;
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterClienteHandler>>();
            var unidadeDeTrabalhoMock = Substitute.For<IUnidadeDeTrabalhoGateway>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterClienteHandler(
                clienteGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(clienteId);
            
            // Assert
            await act.Should().ThrowAsync<DadosInvalidosException>()
                .WithMessage("ID do cliente inválido");
            
            await clienteGatewayMock.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>());
        }
    }
}
