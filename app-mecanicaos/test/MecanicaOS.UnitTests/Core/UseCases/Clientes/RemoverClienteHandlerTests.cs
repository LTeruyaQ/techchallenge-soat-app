using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Clientes.RemoverCliente;

namespace MecanicaOS.UnitTests.Core.UseCases.Clientes
{
    /// <summary>
    /// Testes para RemoverClienteHandler
    /// ROI ALTO: Valida exclusão de clientes, operação crítica para integridade.
    /// Importância: Exclusão deve ser controlada para evitar perda de dados relacionados.
    /// </summary>
    public class RemoverClienteHandlerTests
    {
        private readonly IClienteGateway _clienteGateway;
        private readonly ILogGateway<RemoverClienteHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public RemoverClienteHandlerTests()
        {
            _clienteGateway = Substitute.For<IClienteGateway>();
            _logGateway = Substitute.For<ILogGateway<RemoverClienteHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        /// <summary>
        /// Verifica se RemoverCliente remove cliente existente com sucesso
        /// Importância: ALTA - Operação de exclusão deve funcionar corretamente
        /// Contribuição: Garante que clientes podem ser removidos quando necessário
        /// </summary>
        [Fact]
        public async Task RemoverCliente_ComClienteExistente_DeveRemoverComSucesso()
        {
            // Arrange
            var handler = new RemoverClienteHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var cliente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente a Remover",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(cliente));
            _clienteGateway.DeletarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);

            // Act
            await handler.Handle(clienteId);

            // Assert
            await _clienteGateway.Received(1).ObterPorIdAsync(clienteId);
            await _clienteGateway.Received(1).DeletarAsync(Arg.Is<Cliente>(c => c.Id == clienteId));
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se RemoverCliente lança exceção para cliente inexistente
        /// Importância: CRÍTICA - Previne exclusão de registros inválidos
        /// Contribuição: Garante integridade e tratamento adequado de erros
        /// </summary>
        [Fact]
        public async Task RemoverCliente_ComClienteInexistente_DeveLancarExcecao()
        {
            // Arrange
            var handler = new RemoverClienteHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(clienteId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("cliente não encontrado");

            await _clienteGateway.Received(1).ObterPorIdAsync(clienteId);
            await _clienteGateway.DidNotReceive().DeletarAsync(Arg.Any<Cliente>());
        }

        /// <summary>
        /// Verifica se RemoverCliente lança exceção quando commit falha
        /// Importância: ALTA - Valida tratamento de erro de persistência
        /// Contribuição: Garante que falhas de banco são tratadas adequadamente
        /// </summary>
        [Fact]
        public async Task RemoverCliente_ComFalhaNoCommit_DeveLancarExcecao()
        {
            // Arrange
            var handler = new RemoverClienteHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var cliente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(cliente));
            _clienteGateway.DeletarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);
            _udtGateway.Commit().Returns(Task.FromResult(false)); // Simula falha no commit

            // Act & Assert
            await handler.Invoking(h => h.Handle(clienteId))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao remover cliente");

            await _clienteGateway.Received(1).DeletarAsync(Arg.Any<Cliente>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se RemoverCliente funciona para diferentes tipos de cliente
        /// Importância: MÉDIA - Valida que PF e PJ podem ser removidos
        /// Contribuição: Garante suporte a ambos os tipos de cliente
        /// </summary>
        [Theory]
        [InlineData(TipoCliente.PessoaFisica, "12345678900")]
        [InlineData(TipoCliente.PessoaJuridico, "12345678000190")]
        public async Task RemoverCliente_ComDiferentesTipos_DeveRemoverCorretamente(TipoCliente tipo, string documento)
        {
            // Arrange
            var handler = new RemoverClienteHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var cliente = new Cliente
            {
                Id = clienteId,
                Nome = tipo == TipoCliente.PessoaFisica ? "João Silva" : "Empresa XYZ",
                Documento = documento,
                TipoCliente = tipo
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(cliente));
            _clienteGateway.DeletarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);

            // Act
            await handler.Handle(clienteId);

            // Assert
            await _clienteGateway.Received(1).DeletarAsync(Arg.Is<Cliente>(c => 
                c.Id == clienteId && 
                c.TipoCliente == tipo &&
                c.Documento == documento
            ));
        }
    }
}
