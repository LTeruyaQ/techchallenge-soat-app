using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.UseCases.Clientes.ObterTodosClientes;

namespace MecanicaOS.UnitTests.Core.UseCases.Clientes
{
    /// <summary>
    /// Testes para ObterTodosClientesHandler
    /// ROI ALTO: Valida listagem de clientes, operação fundamental para gestão.
    /// Importância: Clientes são a base do sistema - listagem é operação frequente.
    /// </summary>
    public class ObterTodosClientesHandlerTests
    {
        private readonly IClienteGateway _clienteGateway;
        private readonly ILogGateway<ObterTodosClientesHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public ObterTodosClientesHandlerTests()
        {
            _clienteGateway = Substitute.For<IClienteGateway>();
            _logGateway = Substitute.For<ILogGateway<ObterTodosClientesHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
        }

        /// <summary>
        /// Verifica se ObterTodosClientes retorna lista vazia quando não há clientes
        /// Importância: MÉDIA - Valida comportamento esperado sem dados
        /// Contribuição: Previne erros de null reference em telas de listagem
        /// </summary>
        [Fact]
        public async Task ObterTodosClientes_SemClientes_DeveRetornarListaVazia()
        {
            // Arrange
            var handler = new ObterTodosClientesHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            _clienteGateway.ObterTodosClientesAsync().Returns(Task.FromResult<IEnumerable<Cliente>>(new List<Cliente>()));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().BeEmpty();
            await _clienteGateway.Received(1).ObterTodosClientesAsync();
        }

        /// <summary>
        /// Verifica se ObterTodosClientes retorna lista com múltiplos clientes
        /// Importância: ALTA - Valida listagem básica, operação mais comum
        /// Contribuição: Garante que sistema pode listar todos os clientes cadastrados
        /// </summary>
        [Fact]
        public async Task ObterTodosClientes_ComVariosClientes_DeveRetornarListaCompleta()
        {
            // Arrange
            var handler = new ObterTodosClientesHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clientes = new List<Cliente>
            {
                new Cliente { Id = Guid.NewGuid(), Nome = "Cliente 1", Documento = "12345678900", TipoCliente = TipoCliente.PessoaFisica },
                new Cliente { Id = Guid.NewGuid(), Nome = "Cliente 2", Documento = "98765432100", TipoCliente = TipoCliente.PessoaFisica },
                new Cliente { Id = Guid.NewGuid(), Nome = "Empresa 1", Documento = "12345678000190", TipoCliente = TipoCliente.PessoaJuridico }
            };
            _clienteGateway.ObterTodosClientesAsync().Returns(Task.FromResult<IEnumerable<Cliente>>(clientes));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(3);
            resultado.Should().Contain(c => c.Nome == "Cliente 1");
            resultado.Should().Contain(c => c.Nome == "Cliente 2");
            resultado.Should().Contain(c => c.Nome == "Empresa 1");
            resultado.Should().Contain(c => c.TipoCliente == TipoCliente.PessoaJuridico);
        }

        /// <summary>
        /// Verifica se ObterTodosClientes retorna diferentes tipos de clientes
        /// Importância: ALTA - Valida que PF e PJ são listados corretamente
        /// Contribuição: Garante suporte a ambos os tipos de cliente no sistema
        /// </summary>
        [Fact]
        public async Task ObterTodosClientes_ComPessoaFisicaEJuridica_DeveRetornarAmbos()
        {
            // Arrange
            var handler = new ObterTodosClientesHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clientes = new List<Cliente>
            {
                new Cliente { Id = Guid.NewGuid(), Nome = "João Silva", Documento = "12345678900", TipoCliente = TipoCliente.PessoaFisica },
                new Cliente { Id = Guid.NewGuid(), Nome = "Empresa XYZ Ltda", Documento = "12345678000190", TipoCliente = TipoCliente.PessoaJuridico }
            };
            _clienteGateway.ObterTodosClientesAsync().Returns(Task.FromResult<IEnumerable<Cliente>>(clientes));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Count(c => c.TipoCliente == TipoCliente.PessoaFisica).Should().Be(1);
            resultado.Count(c => c.TipoCliente == TipoCliente.PessoaJuridico).Should().Be(1);
        }

        /// <summary>
        /// Verifica se ObterTodosClientes propaga exceção quando gateway falha
        /// Importância: ALTA - Valida tratamento de erros
        /// Contribuição: Garante que exceções sejam propagadas corretamente
        /// </summary>
        [Fact]
        public async Task ObterTodosClientes_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var handler = new ObterTodosClientesHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            _clienteGateway.ObterTodosClientesAsync().Returns(Task.FromException<IEnumerable<Cliente>>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }
    }
}
