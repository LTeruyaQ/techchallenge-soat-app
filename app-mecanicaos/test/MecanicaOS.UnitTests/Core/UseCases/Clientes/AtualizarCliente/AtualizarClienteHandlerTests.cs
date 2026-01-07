using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Clientes.AtualizarCliente;

namespace MecanicaOS.UnitTests.Core.UseCases.Clientes.AtualizarCliente
{
    /// <summary>
    /// Testes para AtualizarClienteHandler
    /// ROI ALTO: Atualização de clientes é operação frequente - dados precisam estar corretos.
    /// Importância: Valida atualização de cliente, endereço e contato mantendo integridade.
    /// </summary>
    public class AtualizarClienteHandlerTests
    {
        private readonly IClienteGateway _clienteGateway;
        private readonly IEnderecoGateway _enderecoGateway;
        private readonly IContatoGateway _contatoGateway;
        private readonly ILogGateway<AtualizarClienteHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public AtualizarClienteHandlerTests()
        {
            _clienteGateway = Substitute.For<IClienteGateway>();
            _enderecoGateway = Substitute.For<IEnderecoGateway>();
            _contatoGateway = Substitute.For<IContatoGateway>();
            _logGateway = Substitute.For<ILogGateway<AtualizarClienteHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        /// <summary>
        /// Verifica se atualiza cliente com dados válidos
        /// Importância: ALTA - Operação principal do handler
        /// Contribuição: Garante que dados do cliente são atualizados corretamente
        /// </summary>
        [Fact]
        public async Task Handle_ComDadosValidos_DeveAtualizarCliente()
        {
            // Arrange
            var handler = new AtualizarClienteHandler(_clienteGateway, _enderecoGateway, _contatoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var clienteExistente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente Original",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                Sexo = "M"
            };
            var request = new AtualizarClienteUseCaseDto
            {
                Nome = "Cliente Atualizado",
                Sexo = "F",
                TipoCliente = TipoCliente.PessoaFisica,
                DataNascimento = "01/01/1990",
                EnderecoId = Guid.Empty,
                ContatoId = Guid.Empty
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(clienteExistente));
            _clienteGateway.EditarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(clienteId, request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Cliente Atualizado");
            resultado.Sexo.Should().Be("F");
            await _clienteGateway.Received(1).EditarAsync(Arg.Any<Cliente>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se lança exceção quando cliente não existe
        /// Importância: ALTA - Validação de existência obrigatória
        /// Contribuição: Previne atualização de dados inexistentes
        /// </summary>
        [Fact]
        public async Task Handle_ComClienteInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var handler = new AtualizarClienteHandler(_clienteGateway, _enderecoGateway, _contatoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var request = new AtualizarClienteUseCaseDto
            {
                Nome = "Cliente Teste",
                EnderecoId = Guid.Empty,
                ContatoId = Guid.Empty
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(clienteId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("cliente não encontrado");
        }

        /// <summary>
        /// Verifica se atualiza endereço quando informado
        /// Importância: ALTA - Atualização de endereço é comum
        /// Contribuição: Garante que endereço é atualizado junto com cliente
        /// </summary>
        [Fact]
        public async Task Handle_ComEnderecoInformado_DeveAtualizarEndereco()
        {
            // Arrange
            var handler = new AtualizarClienteHandler(_clienteGateway, _enderecoGateway, _contatoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var clienteExistente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };
            var enderecoExistente = new Endereco
            {
                Id = enderecoId,
                Rua = "Rua Antiga",
                Numero = "100",
                Bairro = "Bairro Antigo",
                Cidade = "Cidade Antiga",
                CEP = "00000-000"
            };
            var request = new AtualizarClienteUseCaseDto
            {
                Nome = "Cliente Teste",
                EnderecoId = enderecoId,
                Rua = "Rua Nova",
                Numero = "200",
                Bairro = "Bairro Novo",
                Cidade = "Cidade Nova",
                CEP = "11111-111",
                Complemento = "Apto 10",
                ContatoId = Guid.Empty
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(clienteExistente));
            _enderecoGateway.ObterPorIdAsync(enderecoId).Returns(Task.FromResult<Endereco?>(enderecoExistente));
            _clienteGateway.EditarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);
            _enderecoGateway.EditarAsync(Arg.Any<Endereco>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(clienteId, request);

            // Assert
            await _enderecoGateway.Received(1).EditarAsync(Arg.Is<Endereco>(e =>
                e.Rua == "Rua Nova" &&
                e.Numero == "200" &&
                e.Bairro == "Bairro Novo" &&
                e.Cidade == "Cidade Nova" &&
                e.CEP == "11111-111" &&
                e.Complemento == "Apto 10"
            ));
        }

        /// <summary>
        /// Verifica se atualiza contato quando informado
        /// Importância: ALTA - Atualização de contato é comum
        /// Contribuição: Garante que contato é atualizado junto com cliente
        /// </summary>
        [Fact]
        public async Task Handle_ComContatoInformado_DeveAtualizarContato()
        {
            // Arrange
            var handler = new AtualizarClienteHandler(_clienteGateway, _enderecoGateway, _contatoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var contatoId = Guid.NewGuid();
            var clienteExistente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };
            var contatoExistente = new Contato
            {
                Id = contatoId,
                Email = "antigo@teste.com",
                Telefone = "(11) 1111-1111",
                IdCliente = clienteId
            };
            var request = new AtualizarClienteUseCaseDto
            {
                Id = clienteId,
                Nome = "Cliente Teste",
                ContatoId = contatoId,
                Email = "novo@teste.com",
                Telefone = "(11) 9999-9999",
                EnderecoId = Guid.Empty
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(clienteExistente));
            _contatoGateway.ObterPorIdAsync(contatoId).Returns(Task.FromResult<Contato?>(contatoExistente));
            _clienteGateway.EditarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);
            _contatoGateway.EditarAsync(Arg.Any<Contato>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(clienteId, request);

            // Assert
            await _contatoGateway.Received(1).EditarAsync(Arg.Is<Contato>(c =>
                c.Email == "novo@teste.com" &&
                c.Telefone == "(11) 9999-9999" &&
                c.IdCliente == clienteId
            ));
        }

        /// <summary>
        /// Verifica se lança exceção quando endereço não existe
        /// Importância: MÉDIA - Validação de integridade
        /// Contribuição: Previne atualização de endereço inexistente
        /// </summary>
        [Fact]
        public async Task Handle_ComEnderecoInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var handler = new AtualizarClienteHandler(_clienteGateway, _enderecoGateway, _contatoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var enderecoId = Guid.NewGuid();
            var clienteExistente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };
            var request = new AtualizarClienteUseCaseDto
            {
                Nome = "Cliente Teste",
                EnderecoId = enderecoId,
                Rua = "Rua Teste",
                ContatoId = Guid.Empty
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(clienteExistente));
            _enderecoGateway.ObterPorIdAsync(enderecoId).Returns(Task.FromResult<Endereco?>(null));
            _clienteGateway.EditarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);

            // Act & Assert
            await handler.Invoking(h => h.Handle(clienteId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Endereço inexistente");
        }

        /// <summary>
        /// Verifica se lança exceção quando commit falha
        /// Importância: ALTA - Garantia de persistência
        /// Contribuição: Previne perda de dados
        /// </summary>
        [Fact]
        public async Task Handle_QuandoCommitFalha_DeveLancarPersistirDadosException()
        {
            // Arrange
            var handler = new AtualizarClienteHandler(_clienteGateway, _enderecoGateway, _contatoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var clienteId = Guid.NewGuid();
            var clienteExistente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };
            var request = new AtualizarClienteUseCaseDto
            {
                Nome = "Cliente Atualizado",
                EnderecoId = Guid.Empty,
                ContatoId = Guid.Empty
            };

            _clienteGateway.ObterPorIdAsync(clienteId).Returns(Task.FromResult<Cliente?>(clienteExistente));
            _clienteGateway.EditarAsync(Arg.Any<Cliente>()).Returns(Task.CompletedTask);
            _udtGateway.Commit().Returns(Task.FromResult(false));

            // Act & Assert
            await handler.Invoking(h => h.Handle(clienteId, request))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao atualizar cliente");
        }
    }
}
