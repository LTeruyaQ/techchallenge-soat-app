using Adapters.Presenters;
using Core.DTOs.Requests.Cliente;
using Core.Entidades;
using Core.Enumeradores;
using FluentAssertions;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    public class ClientePresenterTests
    {
        private readonly ClientePresenter _presenter;

        public ClientePresenterTests()
        {
            _presenter = new ClientePresenter();
        }

        [Fact]
        public void ParaResponse_ComClienteValido_DeveConverterCorretamente()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Sexo = "M",
                Documento = "12345678900",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now.AddDays(1)
            };

            // Act
            var response = _presenter.ParaResponse(cliente);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(cliente.Id);
            response.Nome.Should().Be(cliente.Nome);
            response.Sexo.Should().Be(cliente.Sexo);
            response.Documento.Should().Be(cliente.Documento);
            response.DataNascimento.Should().Be(cliente.DataNascimento);
            response.TipoCliente.Should().Be(cliente.TipoCliente.ToString());
        }

        [Fact]
        public void ParaResponse_ComClienteComEndereco_DeveIncluirEndereco()
        {
            // Arrange
            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Centro",
                Cidade = "São Paulo",
                CEP = "01000-000",
                Complemento = "Apto 10",
                IdCliente = Guid.NewGuid(),
                DataCadastro = DateTime.Now
            };

            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                Endereco = endereco,
                DataCadastro = DateTime.Now
            };

            // Act
            var response = _presenter.ParaResponse(cliente);

            // Assert
            response.Should().NotBeNull();
            response!.Endereco.Should().NotBeNull();
            response.Endereco!.Rua.Should().Be("Rua Teste");
            response.Endereco.Numero.Should().Be("123");
            response.Endereco.Bairro.Should().Be("Centro");
            response.Endereco.Cidade.Should().Be("São Paulo");
            response.Endereco.CEP.Should().Be("01000-000");
            response.Endereco.Complemento.Should().Be("Apto 10");
        }

        [Fact]
        public void ParaResponse_ComClienteComContato_DeveIncluirContato()
        {
            // Arrange
            var contato = new Contato
            {
                Id = Guid.NewGuid(),
                Email = "joao@teste.com",
                Telefone = "(11) 98765-4321",
                IdCliente = Guid.NewGuid(),
                DataCadastro = DateTime.Now
            };

            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                Contato = contato,
                DataCadastro = DateTime.Now
            };

            // Act
            var response = _presenter.ParaResponse(cliente);

            // Assert
            response.Should().NotBeNull();
            response!.Contato.Should().NotBeNull();
            response.Contato!.Email.Should().Be("joao@teste.com");
            response.Contato.Telefone.Should().Be("(11) 98765-4321");
        }

        [Fact]
        public void ParaResponse_ComClienteComVeiculos_DeveIncluirVeiculos()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                DataCadastro = DateTime.Now,
                Veiculos = new List<Veiculo>
                {
                    new Veiculo
                    {
                        Id = Guid.NewGuid(),
                        Placa = "ABC1234",
                        Marca = "Honda",
                        Modelo = "Civic",
                        Ano = "2020",
                        ClienteId = Guid.NewGuid(),
                        DataCadastro = DateTime.Now
                    },
                    new Veiculo
                    {
                        Id = Guid.NewGuid(),
                        Placa = "XYZ5678",
                        Marca = "Toyota",
                        Modelo = "Corolla",
                        Ano = "2021",
                        ClienteId = Guid.NewGuid(),
                        DataCadastro = DateTime.Now
                    }
                }
            };

            // Act
            var response = _presenter.ParaResponse(cliente);

            // Assert
            response.Should().NotBeNull();
            response!.Veiculos.Should().HaveCount(2);
            response.Veiculos!.First().Placa.Should().Be("ABC1234");
            response.Veiculos.Last().Placa.Should().Be("XYZ5678");
        }

        [Fact]
        public void ParaResponse_ComClienteNulo_DeveRetornarNull()
        {
            // Arrange
            Cliente cliente = null!;

            // Act
            var response = _presenter.ParaResponse(cliente);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void ParaResponse_ComListaDeClientes_DeveConverterTodos()
        {
            // Arrange
            var clientes = new List<Cliente>
            {
                new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nome = "João Silva",
                    Documento = "12345678900",
                    TipoCliente = TipoCliente.PessoaFisica,
                    DataCadastro = DateTime.Now
                },
                new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nome = "Empresa XYZ",
                    Documento = "12345678000190",
                    TipoCliente = TipoCliente.PessoaJuridico,
                    DataCadastro = DateTime.Now
                }
            };

            // Act
            var responses = _presenter.ParaResponse(clientes);

            // Assert
            responses.Should().HaveCount(2);
            responses.First()!.Nome.Should().Be("João Silva");
            responses.Last()!.Nome.Should().Be("Empresa XYZ");
        }

        [Fact]
        public void ParaResponse_ComListaNula_DeveRetornarListaVazia()
        {
            // Arrange
            IEnumerable<Cliente> clientes = null!;

            // Act
            var responses = _presenter.ParaResponse(clientes);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarClienteRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new CadastrarClienteRequest
            {
                Nome = "João Silva",
                Sexo = "M",
                Documento = "12345678900",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Rua = "Rua Teste",
                Bairro = "Centro",
                Cidade = "São Paulo",
                Numero = "123",
                CEP = "01000-000",
                Complemento = "Apto 10",
                Email = "joao@teste.com",
                Telefone = "(11) 98765-4321"
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Nome.Should().Be(request.Nome);
            dto.Sexo.Should().Be(request.Sexo);
            dto.Documento.Should().Be(request.Documento);
            dto.DataNascimento.Should().Be(request.DataNascimento);
            dto.TipoCliente.Should().Be(request.TipoCliente);
            dto.Rua.Should().Be(request.Rua);
            dto.Bairro.Should().Be(request.Bairro);
            dto.Cidade.Should().Be(request.Cidade);
            dto.Numero.Should().Be(request.Numero);
            dto.CEP.Should().Be(request.CEP);
            dto.Complemento.Should().Be(request.Complemento);
            dto.Email.Should().Be(request.Email);
            dto.Telefone.Should().Be(request.Telefone);
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarClienteRequestNulo_DeveRetornarNull()
        {
            // Arrange
            CadastrarClienteRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarClienteRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarClienteRequest
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva Atualizado",
                Sexo = "M",
                Documento = "12345678900",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                EnderecoId = Guid.NewGuid(),
                Rua = "Rua Nova",
                Bairro = "Bairro Novo",
                Cidade = "Rio de Janeiro",
                Numero = "456",
                CEP = "20000-000",
                Complemento = "Casa",
                ContatoId = Guid.NewGuid(),
                Email = "joao.novo@teste.com",
                Telefone = "(21) 99999-8888"
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(request.Id);
            dto.Nome.Should().Be(request.Nome);
            dto.Sexo.Should().Be(request.Sexo);
            dto.Documento.Should().Be(request.Documento);
            dto.DataNascimento.Should().Be(request.DataNascimento);
            dto.TipoCliente.Should().Be(request.TipoCliente);
            dto.EnderecoId.Should().Be(request.EnderecoId);
            dto.Rua.Should().Be(request.Rua);
            dto.Bairro.Should().Be(request.Bairro);
            dto.Cidade.Should().Be(request.Cidade);
            dto.Numero.Should().Be(request.Numero);
            dto.CEP.Should().Be(request.CEP);
            dto.Complemento.Should().Be(request.Complemento);
            dto.ContatoId.Should().Be(request.ContatoId);
            dto.Email.Should().Be(request.Email);
            dto.Telefone.Should().Be(request.Telefone);
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarClienteRequestNulo_DeveRetornarNull()
        {
            // Arrange
            AtualizarClienteRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarClienteRequest_ComValoresNulos_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarClienteRequest
            {
                Id = null,
                Nome = null,
                Sexo = null,
                Documento = null,
                DataNascimento = null,
                TipoCliente = null,
                EnderecoId = Guid.NewGuid(),
                Rua = null,
                Bairro = null,
                Cidade = null,
                Numero = null,
                CEP = null,
                Complemento = null,
                ContatoId = Guid.NewGuid(),
                Email = null,
                Telefone = null
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Id.Should().BeNull();
            dto.Nome.Should().BeNull();
            dto.Sexo.Should().BeNull();
            dto.Documento.Should().BeNull();
            dto.DataNascimento.Should().BeNull();
            dto.TipoCliente.Should().BeNull();
        }
    }
}
