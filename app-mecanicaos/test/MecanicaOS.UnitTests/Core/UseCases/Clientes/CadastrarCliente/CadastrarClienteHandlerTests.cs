using Core.DTOs.Entidades;
using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;
using Core.UseCases.Clientes.CadastrarCliente;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Clientes.CadastrarCliente
{
    /// <summary>
    /// Testes unitários para o handler CadastrarClienteHandler
    /// </summary>
    public class CadastrarClienteHandlerTests
    {
        /// <summary>
        /// Verifica se o handler cadastra um cliente com sucesso quando os dados são válidos
        /// </summary>
        [Fact]
        public async Task Handle_ComDadosValidos_DeveCadastrarCliente()
        {
            // Arrange
            var clienteGatewayMock = ClienteHandlerFixture.CriarClienteGatewayMock();
            var clienteRepositorioMock = ClienteHandlerFixture.CriarClienteRepositorioMock();
            var unidadeDeTrabalhoMock = ClienteHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var cliente = ClienteHandlerFixture.CriarCliente();
            var clienteDto = new CadastrarClienteUseCaseDto
            {
                Nome = cliente.Nome,
                Sexo = "M",
                TipoCliente = cliente.TipoCliente,
                Documento = cliente.Documento,
                DataNascimento = "1990-01-01",
                Rua = cliente.Endereco.Rua,
                Numero = cliente.Endereco.Numero,
                Bairro = cliente.Endereco.Bairro,
                Cidade = cliente.Endereco.Cidade,
                CEP = cliente.Endereco.CEP,
                Email = cliente.Contato.Email,
                Telefone = cliente.Contato.Telefone
            };
            
            clienteGatewayMock.ObterClientePorDocumentoAsync(Arg.Any<string>()).Returns((Cliente?)null);
            clienteGatewayMock.CadastrarAsync(Arg.Any<Cliente>()).Returns(cliente);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var enderecoGatewayMock = Substitute.For<IEnderecoGateway>();
            var contatoGatewayMock = Substitute.For<IContatoGateway>();
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarClienteHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarClienteHandler(
                clienteGatewayMock,
                enderecoGatewayMock,
                contatoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(clienteDto);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Id.Should().Be(cliente.Id, "o ID deve corresponder ao cliente cadastrado");
            resultado.Nome.Should().Be(cliente.Nome, "o nome deve corresponder ao cliente cadastrado");
            resultado.Documento.Should().Be(cliente.Documento, "o documento deve corresponder ao cliente cadastrado");
            
            await clienteGatewayMock.Received(1).ObterClientePorDocumentoAsync(Arg.Is<string>(d => d == clienteDto.Documento));
            await clienteGatewayMock.Received(1).CadastrarAsync(Arg.Any<Cliente>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando tenta cadastrar um cliente com documento já existente
        /// </summary>
        [Fact]
        public async Task Handle_ComDocumentoJaCadastrado_DeveLancarDadosJaCadastradosException()
        {
            // Arrange
            var clienteGatewayMock = ClienteHandlerFixture.CriarClienteGatewayMock();
            var clienteRepositorioMock = ClienteHandlerFixture.CriarClienteRepositorioMock();
            var unidadeDeTrabalhoMock = ClienteHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var clienteExistente = ClienteHandlerFixture.CriarCliente();
            var clienteDto = ClienteHandlerFixture.CriarCadastrarClienteUseCaseDto();
            
            clienteGatewayMock.ObterClientePorDocumentoAsync(Arg.Any<string>()).Returns(clienteExistente);
            
            var enderecoGatewayMock = Substitute.For<IEnderecoGateway>();
            var contatoGatewayMock = Substitute.For<IContatoGateway>();
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarClienteHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarClienteHandler(
                clienteGatewayMock,
                enderecoGatewayMock,
                contatoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(clienteDto);
            
            // Assert
            var exception = await act.Should().ThrowAsync<DadosJaCadastradosException>()
                .WithMessage("Cliente já cadastrado");
            
            await clienteGatewayMock.Received(1).ObterClientePorDocumentoAsync(Arg.Is<string>(d => d == clienteDto.Documento));
            await clienteGatewayMock.DidNotReceive().CadastrarAsync(Arg.Any<Cliente>());
            await unidadeDeTrabalhoMock.DidNotReceive().Commit();
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando ocorre erro ao persistir os dados
        /// </summary>
        [Fact]
        public async Task Handle_QuandoOcorreErroAoPersistir_DeveLancarPersistirDadosException()
        {
            // Arrange
            var clienteGatewayMock = ClienteHandlerFixture.CriarClienteGatewayMock();
            var clienteRepositorioMock = ClienteHandlerFixture.CriarClienteRepositorioMock();
            var unidadeDeTrabalhoMock = ClienteHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var cliente = ClienteHandlerFixture.CriarCliente();
            var clienteDto = ClienteHandlerFixture.CriarCadastrarClienteUseCaseDto();
            
            clienteGatewayMock.ObterClientePorDocumentoAsync(Arg.Any<string>()).Returns((Cliente?)null);
            clienteGatewayMock.CadastrarAsync(Arg.Any<Cliente>()).Returns(cliente);
            unidadeDeTrabalhoMock.Commit().Returns(Task.FromException<bool>(new Exception("Erro ao persistir dados")));
            
            var enderecoGatewayMock = Substitute.For<IEnderecoGateway>();
            var contatoGatewayMock = Substitute.For<IContatoGateway>();
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarClienteHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarClienteHandler(
                clienteGatewayMock,
                enderecoGatewayMock,
                contatoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(clienteDto);
            
            // Assert
            var exception = await act.Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar cliente");
            exception.Which.InnerException.Message.Should().Be("Erro ao persistir dados");
            
            await clienteGatewayMock.Received(1).ObterClientePorDocumentoAsync(Arg.Is<string>(d => d == clienteDto.Documento));
            await clienteGatewayMock.Received(1).CadastrarAsync(Arg.Any<Cliente>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se o handler preserva os campos técnicos de auditoria
        /// </summary>
        [Fact]
        public async Task Handle_ComDadosValidos_DevePreservarCamposTecnicos()
        {
            // Arrange
            var clienteGatewayMock = ClienteHandlerFixture.CriarClienteGatewayMock();
            var clienteRepositorioMock = ClienteHandlerFixture.CriarClienteRepositorioMock();
            var unidadeDeTrabalhoMock = ClienteHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            Cliente clienteCadastrado = null;
            var clienteDto = ClienteHandlerFixture.CriarCadastrarClienteUseCaseDto();
            
            clienteGatewayMock.ObterClientePorDocumentoAsync(Arg.Any<string>()).Returns((Cliente?)null);
            clienteGatewayMock.CadastrarAsync(Arg.Do<Cliente>(c => clienteCadastrado = c))
                .Returns(c => c.Arg<Cliente>());
            unidadeDeTrabalhoMock.Commit().Returns(Task.FromResult(true));
            
            var enderecoGatewayMock = Substitute.For<IEnderecoGateway>();
            var contatoGatewayMock = Substitute.For<IContatoGateway>();
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarClienteHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarClienteHandler(
                clienteGatewayMock,
                enderecoGatewayMock,
                contatoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(clienteDto);
            
            // Assert
            clienteCadastrado.Should().NotBeNull("o cliente cadastrado não deve ser nulo");
            clienteCadastrado.Id.Should().NotBeEmpty("o ID deve ser gerado automaticamente");
            clienteCadastrado.DataCadastro.Should().NotBe(default(DateTime), "a data de cadastro deve ser preenchida");
            clienteCadastrado.DataAtualizacao.Should().NotBe(default(DateTime), "a data de atualização deve ser preenchida");
            clienteCadastrado.Ativo.Should().BeTrue("o cliente deve estar ativo por padrão");
        }
    }
}
