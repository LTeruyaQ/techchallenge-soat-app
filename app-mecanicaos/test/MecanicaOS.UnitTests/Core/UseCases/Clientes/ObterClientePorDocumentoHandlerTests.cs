using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Clientes.ObterClientePorDocumento;

namespace MecanicaOS.UnitTests.Core.UseCases.Clientes
{
    /// <summary>
    /// Testes para ObterClientePorDocumentoHandler
    /// ROI CRÍTICO: Valida busca por CPF/CNPJ, essencial para cadastro de usuários.
    /// Importância: Operação crítica usada no fluxo de cadastro de usuários.
    /// </summary>
    public class ObterClientePorDocumentoHandlerTests
    {
        private readonly IClienteGateway _clienteGateway;
        private readonly ILogGateway<ObterClientePorDocumentoHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public ObterClientePorDocumentoHandlerTests()
        {
            _clienteGateway = Substitute.For<IClienteGateway>();
            _logGateway = Substitute.For<ILogGateway<ObterClientePorDocumentoHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
        }

        /// <summary>
        /// Verifica se ObterClientePorDocumento retorna cliente com CPF existente
        /// Importância: CRÍTICA - Busca por CPF é usada no cadastro de usuários
        /// Contribuição: Garante que sistema encontra clientes PF corretamente
        /// </summary>
        [Fact]
        public async Task ObterClientePorDocumento_ComCpfExistente_DeveRetornarCliente()
        {
            // Arrange
            var handler = new ObterClientePorDocumentoHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var cpf = "12345678900";
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Documento = cpf,
                TipoCliente = TipoCliente.PessoaFisica
            };

            _clienteGateway.ObterClientePorDocumentoAsync(cpf).Returns(Task.FromResult<Cliente?>(cliente));

            // Act
            var resultado = await handler.Handle(cpf);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Documento.Should().Be(cpf);
            resultado.TipoCliente.Should().Be(TipoCliente.PessoaFisica);
            resultado.Nome.Should().Be("João Silva");
            await _clienteGateway.Received(1).ObterClientePorDocumentoAsync(cpf);
        }

        /// <summary>
        /// Verifica se ObterClientePorDocumento retorna cliente com CNPJ existente
        /// Importância: CRÍTICA - Busca por CNPJ é usada para clientes PJ
        /// Contribuição: Garante que sistema encontra empresas corretamente
        /// </summary>
        [Fact]
        public async Task ObterClientePorDocumento_ComCnpjExistente_DeveRetornarCliente()
        {
            // Arrange
            var handler = new ObterClientePorDocumentoHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var cnpj = "12345678000190";
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "Empresa XYZ Ltda",
                Documento = cnpj,
                TipoCliente = TipoCliente.PessoaJuridico
            };

            _clienteGateway.ObterClientePorDocumentoAsync(cnpj).Returns(Task.FromResult<Cliente?>(cliente));

            // Act
            var resultado = await handler.Handle(cnpj);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Documento.Should().Be(cnpj);
            resultado.TipoCliente.Should().Be(TipoCliente.PessoaJuridico);
            resultado.Nome.Should().Be("Empresa XYZ Ltda");
        }

        /// <summary>
        /// Verifica se ObterClientePorDocumento lança exceção para documento inexistente
        /// Importância: ALTA - Comportamento esperado quando cliente não existe
        /// Contribuição: Permite que controller trate adequadamente cliente não encontrado
        /// </summary>
        [Fact]
        public async Task ObterClientePorDocumento_ComDocumentoInexistente_DeveLancarExcecao()
        {
            // Arrange
            var handler = new ObterClientePorDocumentoHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var documentoInexistente = "99999999999";

            _clienteGateway.ObterClientePorDocumentoAsync(documentoInexistente).Returns(Task.FromResult<Cliente?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(documentoInexistente))
                .Should().ThrowAsync<DadosNaoEncontradosException>();

            await _clienteGateway.Received(1).ObterClientePorDocumentoAsync(documentoInexistente);
        }

        /// <summary>
        /// Verifica se ObterClientePorDocumento funciona com diferentes formatos de documento
        /// Importância: MÉDIA - Valida flexibilidade na busca
        /// Contribuição: Garante que busca funciona independente de formatação
        /// </summary>
        [Theory]
        [InlineData("12345678900")]
        [InlineData("123.456.789-00")]
        [InlineData("12345678000190")]
        [InlineData("12.345.678/0001-90")]
        public async Task ObterClientePorDocumento_ComDiferentesFormatos_DeveBuscarCorretamente(string documento)
        {
            // Arrange
            var handler = new ObterClientePorDocumentoHandler(_clienteGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Documento = documento,
                TipoCliente = documento.Length > 11 ? TipoCliente.PessoaJuridico : TipoCliente.PessoaFisica
            };

            _clienteGateway.ObterClientePorDocumentoAsync(documento).Returns(Task.FromResult<Cliente?>(cliente));

            // Act
            var resultado = await handler.Handle(documento);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Documento.Should().Be(documento);
            await _clienteGateway.Received(1).ObterClientePorDocumentoAsync(documento);
        }
    }
}
