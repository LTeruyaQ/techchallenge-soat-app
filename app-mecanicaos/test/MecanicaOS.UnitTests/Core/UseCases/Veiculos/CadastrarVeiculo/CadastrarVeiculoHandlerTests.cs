using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.CadastrarVeiculo;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos.CadastrarVeiculo
{
    /// <summary>
    /// Testes unitários para o handler CadastrarVeiculoHandler
    /// Importância: Valida a criação de veículos no sistema, garantindo que todos os campos sejam preenchidos corretamente
    /// e que a persistência seja bem-sucedida. Contribui para a robustez ao testar cenários de sucesso e falha.
    /// </summary>
    public class CadastrarVeiculoHandlerTests
    {
        /// <summary>
        /// Verifica se o handler cadastra um veículo com sucesso quando os dados são válidos
        /// Importância: Testa o fluxo principal de cadastro, garantindo que o veículo seja persistido corretamente
        /// </summary>
        [Fact]
        public async Task Handle_ComDadosValidos_DeveCadastrarVeiculo()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculo = VeiculoHandlerFixture.CriarVeiculo();
            var veiculoDto = VeiculoHandlerFixture.CriarCadastrarVeiculoUseCaseDto();
            
            veiculoGatewayMock.CadastrarAsync(Arg.Any<Veiculo>()).Returns(Task.CompletedTask);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(veiculoDto);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Id.Should().NotBeEmpty("o ID deve ser gerado");
            resultado.Placa.Should().Be(veiculoDto.Placa, "a placa deve corresponder");
            resultado.Modelo.Should().Be(veiculoDto.Modelo, "o modelo deve corresponder");
            resultado.Marca.Should().Be(veiculoDto.Marca, "a marca deve corresponder");
            resultado.ClienteId.Should().Be(veiculoDto.ClienteId, "o ClienteId deve corresponder");
            
            await veiculoGatewayMock.Received(1).CadastrarAsync(Arg.Any<Veiculo>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando o commit falha
        /// Importância: Garante que falhas de persistência sejam tratadas adequadamente
        /// </summary>
        [Fact]
        public async Task Handle_QuandoCommitFalha_DeveLancarPersistirDadosException()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculo = VeiculoHandlerFixture.CriarVeiculo();
            var veiculoDto = VeiculoHandlerFixture.CriarCadastrarVeiculoUseCaseDto();
            
            veiculoGatewayMock.CadastrarAsync(Arg.Any<Veiculo>()).Returns(Task.CompletedTask);
            unidadeDeTrabalhoMock.Commit().Returns(false);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(veiculoDto);
            
            // Assert
            await act.Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar veículo");
            
            await veiculoGatewayMock.Received(1).CadastrarAsync(Arg.Any<Veiculo>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se o handler preserva anotações opcionais
        /// Importância: Valida que campos opcionais são tratados corretamente
        /// </summary>
        [Fact]
        public async Task Handle_ComAnotacoesNulas_DeveCadastrarSemAnotacoes()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculo = VeiculoHandlerFixture.CriarVeiculo();
            var veiculoDto = VeiculoHandlerFixture.CriarCadastrarVeiculoUseCaseDto();
            veiculoDto.Anotacoes = null;
            
            veiculoGatewayMock.CadastrarAsync(Arg.Any<Veiculo>()).Returns(Task.CompletedTask);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(veiculoDto);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            await veiculoGatewayMock.Received(1).CadastrarAsync(Arg.Any<Veiculo>());
        }

        [Fact]
        public void Construtor_ComVeiculoGatewayNulo_DeveLancarArgumentNullException()
        {
            // Arrange
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarVeiculoHandler>>();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CadastrarVeiculoHandler(
                    null!,
                    logGatewayMock,
                    unidadeDeTrabalhoMock,
                    usuarioLogadoServicoGatewayMock));

            exception.ParamName.Should().Be("veiculoGateway");
        }
    }
}
