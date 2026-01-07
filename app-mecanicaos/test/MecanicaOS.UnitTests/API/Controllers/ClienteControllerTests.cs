using API.Controllers;
using Core.DTOs.Requests.Cliente;
using Core.DTOs.Responses.Cliente;
using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;
using Microsoft.AspNetCore.Mvc;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes unitários para o ClienteController
    /// </summary>
    public class ClienteControllerTests
    {
        /// <summary>
        /// Verifica se o método Cadastrar retorna Created quando o cliente é cadastrado com sucesso
        /// </summary>
        [Fact]
        public async Task Cadastrar_QuandoClienteCadastradoComSucesso_DeveRetornarCreated()
        {
            // Arrange
            var cliente = ClienteFixture.CriarClienteValido();
            
            var request = new CadastrarClienteRequest
            {
                Nome = cliente.Nome,
                TipoCliente = cliente.TipoCliente,
                Documento = cliente.Documento,
                Rua = cliente.Endereco.Rua,
                Numero = cliente.Endereco.Numero,
                Bairro = cliente.Endereco.Bairro,
                Cidade = cliente.Endereco.Cidade,
                CEP = cliente.Endereco.CEP,
                Email = cliente.Contato.Email,
                Telefone = cliente.Contato.Telefone
            };
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.Cadastrar(Arg.Any<CadastrarClienteRequest>()).Returns(new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Documento = cliente.Documento
            });
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act
            var resultado = await controller.Criar(request);
            
            // Assert
            resultado.Should().BeOfType<CreatedAtActionResult>("deve retornar Created (201)");
            
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(ClienteController.ObterPorId), "deve apontar para a action ObterPorId");
            createdResult.RouteValues!["id"].Should().Be(cliente.Id, "deve incluir o ID do cliente criado");
            
            var response = createdResult.Value as ClienteResponse;
            response.Should().NotBeNull("a resposta não deve ser nula");
            response!.Id.Should().Be(cliente.Id, "o ID deve corresponder ao cliente cadastrado");
            response.Nome.Should().Be(cliente.Nome, "o nome deve corresponder ao cliente cadastrado");
            response.Documento.Should().Be(cliente.Documento, "o documento deve corresponder ao cliente cadastrado");
            
            await clienteControllerMock.Received(1).Cadastrar(Arg.Any<CadastrarClienteRequest>());
        }

        /// <summary>
        /// Verifica se o método Cadastrar propaga exceção quando ocorre DadosJaCadastradosException
        /// A exceção será tratada pelo GlobalExceptionHandlerMiddleware
        /// </summary>
        [Fact]
        public async Task Cadastrar_QuandoOcorreDadosJaCadastradosException_DevePropagarExcecao()
        {
            // Arrange
            var request = new CadastrarClienteRequest
            {
                Nome = "Cliente Teste",
                TipoCliente = TipoCliente.PessoaFisica,
                Documento = "12345678900",
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Bairro Teste",
                Cidade = "São Paulo",
                CEP = "01234567",
                Email = "cliente@teste.com",
                Telefone = "11999999999"
            };
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.Cadastrar(Arg.Any<CadastrarClienteRequest>())
                .Returns<ClienteResponse>(x => throw new DadosJaCadastradosException("Cliente com documento já cadastrado"));
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act & Assert
            await FluentActions.Invoking(async () => await controller.Criar(request))
                .Should().ThrowAsync<DadosJaCadastradosException>()
                .WithMessage("Cliente com documento já cadastrado");
            
            await clienteControllerMock.Received(1).Cadastrar(Arg.Any<CadastrarClienteRequest>());
        }

        /// <summary>
        /// Verifica se o método ObterPorId retorna Ok quando o cliente é encontrado
        /// </summary>
        [Fact]
        public async Task ObterPorId_QuandoClienteEncontrado_DeveRetornarOk()
        {
            // Arrange
            var cliente = ClienteFixture.CriarClienteValido();
            cliente.Id = Guid.NewGuid();
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.ObterPorId(cliente.Id).Returns(new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Documento = cliente.Documento
            });
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act
            var resultado = await controller.ObterPorId(cliente.Id);
            
            // Assert
            resultado.Should().BeOfType<OkObjectResult>("deve retornar Ok (200)");
            
            var okResult = resultado as OkObjectResult;
            var response = okResult!.Value as ClienteResponse;
            
            response.Should().NotBeNull("a resposta não deve ser nula");
            response!.Id.Should().Be(cliente.Id, "o ID deve corresponder ao cliente encontrado");
            response.Nome.Should().Be(cliente.Nome, "o nome deve corresponder ao cliente encontrado");
            
            await clienteControllerMock.Received(1).ObterPorId(cliente.Id);
        }

        /// <summary>
        /// Verifica se o método ObterPorId propaga exceção quando o cliente não é encontrado
        /// A exceção será tratada pelo GlobalExceptionHandlerMiddleware
        /// </summary>
        [Fact]
        public async Task ObterPorId_QuandoClienteNaoEncontrado_DevePropagarExcecao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.ObterPorId(clienteId)
                .Returns<ClienteResponse>(x => throw new DadosNaoEncontradosException("Cliente não encontrado"));
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act & Assert
            await FluentActions.Invoking(async () => await controller.ObterPorId(clienteId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Cliente não encontrado");
            
            await clienteControllerMock.Received(1).ObterPorId(clienteId);
        }

        /// <summary>
        /// Verifica se o método ObterTodos retorna Ok com a lista de clientes
        /// </summary>
        [Fact]
        public async Task ObterTodos_DeveRetornarOkComListaDeClientes()
        {
            // Arrange
            var clientes = ClienteFixture.CriarListaClientes(3);
            var clientesResponse = clientes.Select(c => new ClienteResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Documento = c.Documento
            });
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.ObterTodos().Returns(clientesResponse);
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act
            var resultado = await controller.ObterTodos();
            
            // Assert
            resultado.Should().BeOfType<OkObjectResult>("deve retornar Ok (200)");
            
            var okResult = resultado as OkObjectResult;
            var response = okResult!.Value as IEnumerable<ClienteResponse>;
            
            response.Should().NotBeNull("a resposta não deve ser nula");
            response.Should().HaveCount(3, "devem ser retornados 3 clientes");
            
            await clienteControllerMock.Received(1).ObterTodos();
        }

        /// <summary>
        /// Verifica se o método Atualizar retorna Ok quando o cliente é atualizado com sucesso
        /// </summary>
        [Fact]
        public async Task Atualizar_QuandoClienteAtualizadoComSucesso_DeveRetornarOk()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var request = new AtualizarClienteRequest
            {
                Nome = "Cliente Atualizado",
                EnderecoId = Guid.NewGuid(),
                Rua = "Rua Atualizada",
                Numero = "456",
                Bairro = "Bairro Atualizado",
                Cidade = "Rio de Janeiro",
                CEP = "98765432",
                Complemento = "Apto 123",
                ContatoId = Guid.NewGuid(),
                Email = "atualizado@teste.com",
                Telefone = "21888888888"
            };
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.Atualizar(clienteId, Arg.Any<AtualizarClienteRequest>())
                .Returns(new ClienteResponse
                {
                    Id = clienteId,
                    Nome = "Cliente Atualizado",
                    Documento = "12345678900"
                });
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act
            var resultado = await controller.Atualizar(clienteId, request);
            
            // Assert
            resultado.Should().BeOfType<OkObjectResult>("deve retornar Ok (200)");
            
            var okResult = resultado as OkObjectResult;
            var response = okResult!.Value as ClienteResponse;
            
            response.Should().NotBeNull("a resposta não deve ser nula");
            response!.Id.Should().Be(clienteId, "o ID deve corresponder ao cliente atualizado");
            response.Nome.Should().Be("Cliente Atualizado", "o nome deve corresponder ao cliente atualizado");
            
            await clienteControllerMock.Received(1).Atualizar(clienteId, Arg.Any<AtualizarClienteRequest>());
        }

        /// <summary>
        /// Verifica se o método Atualizar propaga exceção quando o cliente não é encontrado
        /// A exceção será tratada pelo GlobalExceptionHandlerMiddleware
        /// </summary>
        [Fact]
        public async Task Atualizar_QuandoClienteNaoEncontrado_DevePropagarExcecao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var request = new AtualizarClienteRequest
            {
                Nome = "Cliente Atualizado",
                EnderecoId = Guid.NewGuid(),
                Rua = "Rua Atualizada",
                Numero = "456",
                Bairro = "Bairro Atualizado",
                Cidade = "Rio de Janeiro",
                CEP = "98765432",
                Complemento = "Apto 123",
                ContatoId = Guid.NewGuid(),
                Email = "atualizado@teste.com",
                Telefone = "21888888888"
            };
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.Atualizar(clienteId, Arg.Any<AtualizarClienteRequest>())
                .Returns<ClienteResponse>(x => throw new DadosNaoEncontradosException("Cliente não encontrado"));
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act & Assert
            await FluentActions.Invoking(async () => await controller.Atualizar(clienteId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Cliente não encontrado");
            
            await clienteControllerMock.Received(1).Atualizar(clienteId, Arg.Any<AtualizarClienteRequest>());
        }

        /// <summary>
        /// Verifica se o método Remover retorna NoContent quando o cliente é removido com sucesso
        /// </summary>
        [Fact]
        public async Task Remover_QuandoClienteRemovidoComSucesso_DeveRetornarNoContent()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.Remover(clienteId).Returns(true);
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act
            var resultado = await controller.Remover(clienteId);
            
            // Assert
            resultado.Should().BeOfType<NoContentResult>("deve retornar NoContent (204)");
            
            await clienteControllerMock.Received(1).Remover(clienteId);
        }

        /// <summary>
        /// Verifica se o método Remover propaga exceção quando o cliente não é encontrado
        /// A exceção será tratada pelo GlobalExceptionHandlerMiddleware
        /// </summary>
        [Fact]
        public async Task Remover_QuandoClienteNaoEncontrado_DevePropagarExcecao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            
            clienteControllerMock.Remover(clienteId)
                .Returns<bool>(x => throw new DadosNaoEncontradosException("Cliente não encontrado"));
            
            var controller = new ClienteController(compositionRootMock);
            
            // Act & Assert
            await FluentActions.Invoking(async () => await controller.Remover(clienteId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Cliente não encontrado");
            
            await clienteControllerMock.Received(1).Remover(clienteId);
        }

        [Fact]
        public async Task ObterPorDocumento_ComDocumentoValido_DeveRetornarCliente()
        {
            // Arrange
            var documento = "12345678901";
            var clienteResponse = new ClienteResponse
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Documento = documento
            };

            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var clienteControllerMock = Substitute.For<IClienteController>();
            
            compositionRootMock.CriarClienteController().Returns(clienteControllerMock);
            clienteControllerMock.ObterPorDocumento(documento).Returns(clienteResponse);

            var controller = new ClienteController(compositionRootMock);

            // Act
            var resultado = await controller.ObterPorDocumento(documento);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().Be(clienteResponse);

            await clienteControllerMock.Received(1).ObterPorDocumento(documento);
        }
    }
}
