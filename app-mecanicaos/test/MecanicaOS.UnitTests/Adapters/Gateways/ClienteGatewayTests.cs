using Adapters.Gateways;
using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.Entidades.Cliente;
using Core.DTOs.Entidades.Veiculo;
using Core.Entidades;
using Core.Enumeradores;
using Core.Especificacoes.Base.Interfaces;
using Core.Exceptions;
using Core.Interfaces.Repositorios;
using Microsoft.AspNetCore.Mvc;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    /// <summary>
    /// Testes unitários para o ClienteGateway
    /// </summary>
    public class ClienteGatewayTests
    {
        /// <summary>
        /// Verifica se o método CadastrarAsync converte corretamente a entidade para DTO e chama o repositório
        /// </summary>
        [Fact]
        public async Task CadastrarAsync_DeveConverterEntidadeParaDtoEChamarRepositorio()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            
            var cliente = ClienteFixture.CriarClienteValido();
            cliente.Id = Guid.NewGuid();
            
            ClienteEntityDto? clienteRepositoryDtoCapturado = null;
            
            repositorioMock.CadastrarAsync(Arg.Do<ClienteEntityDto>(dto => clienteRepositoryDtoCapturado = dto))
                .Returns(callInfo => callInfo.Arg<ClienteEntityDto>());
            
            var gateway = new ClienteGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.CadastrarAsync(cliente);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Id.Should().Be(cliente.Id, "o ID deve ser preservado");
            
            clienteRepositoryDtoCapturado.Should().NotBeNull("o DTO capturado não deve ser nulo");
            clienteRepositoryDtoCapturado.Id.Should().Be(cliente.Id, "o ID deve ser preservado no DTO");
            clienteRepositoryDtoCapturado.Nome.Should().Be(cliente.Nome, "o nome deve ser preservado no DTO");
            clienteRepositoryDtoCapturado.TipoCliente.Should().Be(cliente.TipoCliente, "o tipo de cliente deve ser preservado no DTO");
            clienteRepositoryDtoCapturado.Documento.Should().Be(cliente.Documento, "o documento deve ser preservado no DTO");
            clienteRepositoryDtoCapturado.Ativo.Should().Be(cliente.Ativo, "o status ativo deve ser preservado no DTO");
            clienteRepositoryDtoCapturado.DataCadastro.Should().Be(cliente.DataCadastro, "a data de cadastro deve ser preservada no DTO");
            clienteRepositoryDtoCapturado.DataAtualizacao.Should().Be(cliente.DataAtualizacao, "a data de atualização deve ser preservada no DTO");
            
            await repositorioMock.Received(1).CadastrarAsync(Arg.Any<ClienteEntityDto>());
        }

        /// <summary>
        /// Verifica se o método AtualizarAsync converte corretamente a entidade para DTO e chama o repositório
        /// </summary>
        [Fact]
        public async Task EditarAsync_DeveConverterEntidadeParaDtoEChamarRepositorio()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            
            var cliente = ClienteFixture.CriarClienteValido();
            cliente.Id = Guid.NewGuid();
            cliente.Nome = "Cliente Atualizado";
            
            ClienteEntityDto clienteRepositoryDtoCapturado = null;
            
            repositorioMock.EditarAsync(Arg.Do<ClienteEntityDto>(dto => clienteRepositoryDtoCapturado = dto))
                .Returns(Task.CompletedTask);
            
            var gateway = new ClienteGateway(repositorioMock);
            
            // Act
            await gateway.EditarAsync(cliente);
            
            // Assert
            
            clienteRepositoryDtoCapturado.Should().NotBeNull("o DTO capturado não deve ser nulo");
            clienteRepositoryDtoCapturado.Id.Should().Be(cliente.Id, "o ID deve ser preservado no DTO");
            clienteRepositoryDtoCapturado.Nome.Should().Be("Cliente Atualizado", "o nome deve ser atualizado no DTO");
            clienteRepositoryDtoCapturado.DataAtualizacao.Should().Be(cliente.DataAtualizacao, "a data de atualização deve ser preservada no DTO");
            
            await repositorioMock.Received(1).EditarAsync(Arg.Any<ClienteEntityDto>());
        }

        /// <summary>
        /// Verifica se o método ObterPorIdAsync usa especificação correta e retorna cliente
        /// </summary>
        [Fact]
        public async Task ObterPorIdAsync_DeveUsarEspecificacaoERetornarCliente()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            
            var cliente = ClienteFixture.CriarClienteValido();
            cliente.Id = Guid.NewGuid();
            
            repositorioMock.ObterUmProjetadoSemRastreamentoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>())
                .Returns(cliente);
            
            var gateway = new ClienteGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterPorIdAsync(cliente.Id);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado!.Id.Should().Be(cliente.Id, "o ID deve ser preservado");
            resultado.Nome.Should().Be(cliente.Nome, "o nome deve ser preservado");
            resultado.TipoCliente.Should().Be(cliente.TipoCliente, "o tipo de cliente deve ser preservado");
            resultado.Documento.Should().Be(cliente.Documento, "o documento deve ser preservado");
            resultado.Ativo.Should().BeTrue("o status ativo deve ser preservado");
            
            resultado.Endereco.Should().NotBeNull("o endereço não deve ser nulo");
            resultado.Contato.Should().NotBeNull("o contato não deve ser nulo");
            
            await repositorioMock.Received(1).ObterUmProjetadoSemRastreamentoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>());
        }

        /// <summary>
        /// Verifica se o método ObterClientePorDocumentoAsync usa especificação correta e retorna cliente
        /// </summary>
        [Fact]
        public async Task ObterClientePorDocumentoAsync_DeveUsarEspecificacaoERetornarCliente()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            
            var cliente = ClienteFixture.CriarClienteValido();
            var documento = "12345678900";
            cliente.Documento = documento;
            
            repositorioMock.ObterUmProjetadoSemRastreamentoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>())
                .Returns(cliente);
            
            var gateway = new ClienteGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterClientePorDocumentoAsync(documento);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado!.Id.Should().Be(cliente.Id, "o ID deve ser preservado");
            resultado.Documento.Should().Be(documento, "o documento deve ser preservado");
            
            await repositorioMock.Received(1).ObterUmProjetadoSemRastreamentoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>());
        }

        /// <summary>
        /// Verifica se o método ObterTodosAsync converte corretamente os DTOs para entidades
        /// </summary>
        [Fact]
        public async Task ObterTodosClientesAsync_DeveUsarEspecificacaoERetornarClientes()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            
            var clientes = ClienteFixture.CriarListaClientes(2);
            
            repositorioMock.ListarProjetadoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>())
                .Returns(clientes);
            
            var gateway = new ClienteGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterTodosClientesAsync();
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Should().HaveCount(2, "devem ser retornados 2 clientes");
            
            await repositorioMock.Received(1).ListarProjetadoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>());
        }

        [Fact]
        public async Task ObterClienteComVeiculoPorIdAsync_ComIdValido_DeveRetornarClienteComVeiculo()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            var clienteId = Guid.NewGuid();
            var cliente = new Cliente
            {
                Id = clienteId,
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                DataNascimento = "1990-01-01",
                DataCadastro = DateTime.Now,
                Veiculos = new List<Veiculo>
                {
                    new Veiculo
                    {
                        Id = Guid.NewGuid(),
                        Placa = "ABC1234",
                        Modelo = "Modelo Teste",
                        Marca = "Marca Teste",
                        Ano = "2020",
                        ClienteId = clienteId
                    }
                }
            };

            repositorioMock.ObterUmProjetadoSemRastreamentoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>())
                .Returns(Task.FromResult<Cliente?>(cliente));

            var gateway = new ClienteGateway(repositorioMock);

            // Act
            var resultado = await gateway.ObterClienteComVeiculoPorIdAsync(clienteId);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(clienteId);
            resultado.Veiculos.Should().NotBeNull();
            resultado.Veiculos.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObterClienteComVeiculoPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            var clienteId = Guid.NewGuid();

            repositorioMock.ObterUmProjetadoSemRastreamentoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>())
                .Returns(Task.FromResult<Cliente?>(null));

            var gateway = new ClienteGateway(repositorioMock);

            // Act
            var resultado = await gateway.ObterClienteComVeiculoPorIdAsync(clienteId);

            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se o método DeletarAsync converte corretamente a entidade para DTO e chama o repositório
        /// </summary>
        [Fact]
        public async Task DeletarAsync_DeveConverterEntidadeParaDtoEChamarRepositorio()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            
            var cliente = ClienteFixture.CriarClienteValido();
            cliente.Id = Guid.NewGuid();
            
            ClienteEntityDto? clienteRepositoryDtoCapturado = null;
            
            repositorioMock.DeletarAsync(Arg.Do<ClienteEntityDto>(dto => clienteRepositoryDtoCapturado = dto))
                .Returns(Task.CompletedTask);
            
            var gateway = new ClienteGateway(repositorioMock);
            
            // Act
            await gateway.DeletarAsync(cliente);
            
            // Assert
            clienteRepositoryDtoCapturado.Should().NotBeNull("o DTO capturado não deve ser nulo");
            clienteRepositoryDtoCapturado.Id.Should().Be(cliente.Id, "o ID deve ser preservado no DTO");
            clienteRepositoryDtoCapturado.Nome.Should().Be(cliente.Nome, "o nome deve ser preservado no DTO");
            
            await repositorioMock.Received(1).DeletarAsync(Arg.Any<ClienteEntityDto>());
        }

        /// <summary>
        /// Verifica se o método ObterClientePorNomeAsync usa especificação correta e retorna clientes
        /// </summary>
        [Fact]
        public async Task ObterClientePorNomeAsync_DeveUsarEspecificacaoERetornarClientes()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<ClienteEntityDto>>();
            
            var clientes = ClienteFixture.CriarListaClientes(2);
            var nome = "Cliente Teste";
            
            repositorioMock.ListarProjetadoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>())
                .Returns(clientes);
            
            var gateway = new ClienteGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterClientePorNomeAsync(nome);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Should().HaveCount(2, "devem ser retornados 2 clientes");
            
            await repositorioMock.Received(1).ListarProjetadoAsync<Cliente>(Arg.Any<IEspecificacao<ClienteEntityDto>>());
        }

        /// <summary>
        /// Verifica se ToDto converte corretamente cliente sem Contato e Endereco
        /// </summary>
        [Fact]
        public void ToDto_ComClienteSemContatoEEndereco_DeveConverterCorretamente()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                DataNascimento = "1990-01-01",
                Sexo = "M",
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true,
                Contato = null,
                Endereco = null
            };
            
            // Act
            var dto = ClienteGateway.ToDto(cliente);
            
            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(cliente.Id);
            dto.Nome.Should().Be(cliente.Nome);
            dto.Documento.Should().Be(cliente.Documento);
            dto.Contato.Should().BeNull("contato deve ser nulo");
            dto.Endereco.Should().BeNull("endereço deve ser nulo");
        }

        /// <summary>
        /// Verifica se FromDto retorna null quando DTO é nulo
        /// </summary>
        [Fact]
        public void FromDto_ComDtoNulo_DeveRetornarNull()
        {
            // Act
            var resultado = ClienteGateway.FromDto(null);
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se FromDto converte corretamente DTO com Veículos
        /// </summary>
        [Fact]
        public void FromDto_ComDtoComVeiculos_DeveConverterCorretamente()
        {
            // Arrange
            var dto = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                DataNascimento = "1990-01-01",
                Sexo = "M",
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true,
                Contato = new ContatoEntityDto
                {
                    Id = Guid.NewGuid(),
                    Email = "teste@teste.com",
                    Telefone = "11999999999",
                    DataCadastro = DateTime.Now,
                    Ativo = true
                },
                Endereco = new EnderecoEntityDto
                {
                    Id = Guid.NewGuid(),
                    Rua = "Rua Teste",
                    Numero = "123",
                    Bairro = "Bairro Teste",
                    Cidade = "Cidade Teste",
                    CEP = "12345-678",
                    DataCadastro = DateTime.Now,
                    Ativo = true
                },
                Veiculos = new List<VeiculoEntityDto>
                {
                    new VeiculoEntityDto
                    {
                        Id = Guid.NewGuid(),
                        Placa = "ABC1234",
                        Modelo = "Modelo Teste",
                        Marca = "Marca Teste",
                        Ano = "2020",
                        DataCadastro = DateTime.Now,
                        Ativo = true
                    }
                }
            };
            
            // Act
            var cliente = ClienteGateway.FromDto(dto);
            
            // Assert
            cliente.Should().NotBeNull();
            cliente!.Id.Should().Be(dto.Id);
            cliente.Nome.Should().Be(dto.Nome);
            cliente.Documento.Should().Be(dto.Documento);
            cliente.Contato.Should().NotBeNull();
            cliente.Endereco.Should().NotBeNull();
            cliente.Veiculos.Should().NotBeNull();
            cliente.Veiculos.Should().HaveCount(1);
            cliente.Veiculos.First().Placa.Should().Be("ABC1234");
        }

        /// <summary>
        /// Verifica se FromDto converte corretamente DTO sem Veículos
        /// </summary>
        [Fact]
        public void FromDto_ComDtoSemVeiculos_DeveRetornarListaVazia()
        {
            // Arrange
            var dto = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                DataNascimento = "1990-01-01",
                Sexo = "M",
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true,
                Veiculos = null
            };
            
            // Act
            var cliente = ClienteGateway.FromDto(dto);
            
            // Assert
            cliente.Should().NotBeNull();
            cliente!.Veiculos.Should().NotBeNull();
            cliente.Veiculos.Should().BeEmpty("lista de veículos deve estar vazia quando DTO não tem veículos");
        }
    }
}
