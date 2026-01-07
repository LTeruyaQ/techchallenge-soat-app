using Adapters.Presenters;
using Core.DTOs.Requests.Veiculo;
using Core.Entidades;
using Core.Enumeradores;
using FluentAssertions;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    public class VeiculoPresenterTests
    {
        private readonly VeiculoPresenter _presenter;

        public VeiculoPresenterTests()
        {
            _presenter = new VeiculoPresenter();
        }

        [Fact]
        public void ParaResponse_ComVeiculoValido_DeveConverterCorretamente()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Preto",
                Ano = "2020",
                Anotacoes = "Veículo em bom estado",
                ClienteId = Guid.NewGuid(),
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now.AddDays(1)
            };

            // Act
            var response = _presenter.ParaResponse(veiculo);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(veiculo.Id);
            response.Placa.Should().Be(veiculo.Placa);
            response.Marca.Should().Be(veiculo.Marca);
            response.Modelo.Should().Be(veiculo.Modelo);
            response.Cor.Should().Be(veiculo.Cor);
            response.Ano.Should().Be(veiculo.Ano);
            response.Anotacoes.Should().Be(veiculo.Anotacoes);
            response.ClienteId.Should().Be(veiculo.ClienteId);
            response.ClienteNome.Should().Be(string.Empty);
            response.DataCadastro.Should().Be(veiculo.DataCadastro);
            response.DataAtualizacao.Should().Be(veiculo.DataAtualizacao);
        }

        [Fact]
        public void ParaResponse_ComVeiculoComCliente_DeveIncluirNomeDoCliente()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                DataCadastro = DateTime.Now
            };

            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Preto",
                Ano = "2020",
                ClienteId = cliente.Id,
                Cliente = cliente,
                DataCadastro = DateTime.Now
            };

            // Act
            var response = _presenter.ParaResponse(veiculo);

            // Assert
            response.Should().NotBeNull();
            response!.ClienteNome.Should().Be("João Silva");
        }

        [Fact]
        public void ParaResponse_ComVeiculoNulo_DeveRetornarNull()
        {
            // Arrange
            Veiculo veiculo = null!;

            // Act
            var response = _presenter.ParaResponse(veiculo);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void ParaResponse_ComListaDeVeiculos_DeveConverterTodos()
        {
            // Arrange
            var veiculos = new List<Veiculo>
            {
                new Veiculo
                {
                    Id = Guid.NewGuid(),
                    Placa = "ABC1234",
                    Marca = "Honda",
                    Modelo = "Civic",
                    Cor = "Preto",
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
                    Cor = "Branco",
                    Ano = "2021",
                    ClienteId = Guid.NewGuid(),
                    DataCadastro = DateTime.Now
                }
            };

            // Act
            var responses = _presenter.ParaResponse(veiculos);

            // Assert
            responses.Should().HaveCount(2);
            responses.First()!.Placa.Should().Be("ABC1234");
            responses.Last()!.Placa.Should().Be("XYZ5678");
        }

        [Fact]
        public void ParaResponse_ComListaNula_DeveRetornarListaVazia()
        {
            // Arrange
            IEnumerable<Veiculo> veiculos = null!;

            // Act
            var responses = _presenter.ParaResponse(veiculos);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarVeiculoRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new CadastrarVeiculoRequest
            {
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Preto",
                Ano = "2020",
                Anotacoes = "Veículo novo",
                ClienteId = Guid.NewGuid()
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Placa.Should().Be(request.Placa);
            dto.Marca.Should().Be(request.Marca);
            dto.Modelo.Should().Be(request.Modelo);
            dto.Cor.Should().Be(request.Cor);
            dto.Ano.Should().Be(request.Ano);
            dto.Anotacoes.Should().Be(request.Anotacoes);
            dto.ClienteId.Should().Be(request.ClienteId);
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarVeiculoRequestNulo_DeveRetornarNull()
        {
            // Arrange
            CadastrarVeiculoRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarVeiculoRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarVeiculoRequest
            {
                Placa = "XYZ5678",
                Marca = "Toyota",
                Modelo = "Corolla",
                Cor = "Branco",
                Ano = "2021",
                Anotacoes = "Veículo atualizado",
                ClienteId = Guid.NewGuid()
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Placa.Should().Be(request.Placa);
            dto.Marca.Should().Be(request.Marca);
            dto.Modelo.Should().Be(request.Modelo);
            dto.Cor.Should().Be(request.Cor);
            dto.Ano.Should().Be(request.Ano);
            dto.Anotacoes.Should().Be(request.Anotacoes);
            dto.ClienteId.Should().Be(request.ClienteId);
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarVeiculoRequestNulo_DeveRetornarNull()
        {
            // Arrange
            AtualizarVeiculoRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarVeiculoRequest_ComValoresNulos_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarVeiculoRequest
            {
                Placa = null,
                Marca = null,
                Modelo = null,
                Cor = null,
                Ano = null,
                Anotacoes = null,
                ClienteId = null
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Placa.Should().BeNull();
            dto.Marca.Should().BeNull();
            dto.Modelo.Should().BeNull();
            dto.Cor.Should().BeNull();
            dto.Ano.Should().BeNull();
            dto.Anotacoes.Should().BeNull();
            dto.ClienteId.Should().BeNull();
        }
    }
}
