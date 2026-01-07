using API.Notificacoes.OS;
using Core.DTOs.Responses.Cliente;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.Responses.Servico;
using Core.DTOs.Responses.Veiculo;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MecanicaOS.UnitTests.API.Notificacoes.OS
{
    /// <summary>
    /// Testes para OrdemServicoFinalizadaHandler
    /// 
    /// IMPORTÂNCIA: Handler crítico que processa eventos de finalização de OS.
    /// Responsável por notificar cliente sobre a conclusão do serviço.
    /// 
    /// COBERTURA: Testa todos os cenários do handler de notificação.
    /// Valida integração com controllers, serviço de email e geração de conteúdo.
    /// </summary>
    public class OrdemServicoFinalizadaHandlerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly ILogServico<OrdemServicoFinalizadaHandler> _logServico;
        private readonly IServicoEmail _emailServico;
        private readonly IOrdemServicoController _ordemServicoController;
        private readonly OrdemServicoFinalizadaHandler _handler;

        public OrdemServicoFinalizadaHandlerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _logServico = Substitute.For<ILogServico<OrdemServicoFinalizadaHandler>>();
            _emailServico = Substitute.For<IServicoEmail>();
            _ordemServicoController = Substitute.For<IOrdemServicoController>();

            _compositionRoot.CriarLogService<OrdemServicoFinalizadaHandler>().Returns(_logServico);
            _compositionRoot.CriarServicoEmail().Returns(_emailServico);
            _compositionRoot.CriarOrdemServicoController().Returns(_ordemServicoController);

            _handler = new OrdemServicoFinalizadaHandler(_compositionRoot);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var logServico = Substitute.For<ILogServico<OrdemServicoFinalizadaHandler>>();
            var emailServico = Substitute.For<IServicoEmail>();
            var ordemServicoController = Substitute.For<IOrdemServicoController>();

            compositionRoot.CriarLogService<OrdemServicoFinalizadaHandler>().Returns(logServico);
            compositionRoot.CriarServicoEmail().Returns(emailServico);
            compositionRoot.CriarOrdemServicoController().Returns(ordemServicoController);

            // Act
            var handler = new OrdemServicoFinalizadaHandler(compositionRoot);

            // Assert
            handler.Should().NotBeNull();
            compositionRoot.Received(1).CriarLogService<OrdemServicoFinalizadaHandler>();
            compositionRoot.Received(1).CriarServicoEmail();
            compositionRoot.Received(1).CriarOrdemServicoController();
        }

        [Fact]
        public async Task Handle_ComEventoValido_DeveProcessarOrdemServicoEEnviarEmail()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoFinalizadaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoResponse(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _ordemServicoController.Received(1).ObterPorId(ordemServicoId);
            await _emailServico.Received(1).EnviarAsync(
                Arg.Is<IEnumerable<string>>(emails => emails.Contains(ordemServico.Cliente.Contato.Email)),
                "Serviço Finalizado",
                Arg.Any<string>());
            _logServico.Received(1).LogFim(nameof(_handler.Handle));
        }

        [Fact]
        public async Task Handle_ComOrdemServicoNula_DeveRetornarSemProcessar()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoFinalizadaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            _ordemServicoController.ObterPorId(ordemServicoId).Returns((OrdemServicoResponse?)null);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _ordemServicoController.Received(1).ObterPorId(ordemServicoId);
            await _emailServico.DidNotReceive().EnviarAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<string>(), Arg.Any<string>());
            _logServico.DidNotReceive().LogFim(Arg.Any<string>());
        }

        [Fact]
        public async Task Handle_ComExcecaoNoObterOrdemServico_DeveLogarErroEReLancar()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoFinalizadaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var exception = new Exception("Erro ao obter ordem de serviço");
            _ordemServicoController.ObterPorId(ordemServicoId)
                .Throws(exception);

            // Act
            var act = async () => await _handler.Handle(notification, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao obter ordem de serviço");
            
            _logServico.Received(1).LogErro(nameof(_handler.Handle), exception);
        }

        [Fact]
        public async Task Handle_ComExcecaoNoEnvioEmail_DeveLogarErroEReLancar()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoFinalizadaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoResponse(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            var exception = new Exception("Erro ao enviar email");
            _emailServico.EnviarAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(exception);

            // Act
            var act = async () => await _handler.Handle(notification, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao enviar email");
            
            _logServico.Received(1).LogErro(nameof(_handler.Handle), exception);
        }

        [Fact]
        public async Task Handle_ComOrdemServicoValida_DeveGerarConteudoCorreto()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoFinalizadaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoResponse(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            await _emailServico.Received(1).EnviarAsync(
                Arg.Is<IEnumerable<string>>(emails => emails.Contains("cliente@teste.com")),
                "Serviço Finalizado",
                Arg.Any<string>());
        }

        [Fact]
        public async Task Handle_ComDiferentesClientes_DeveEnviarEmailCorreto()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoFinalizadaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoResponse(ordemServicoId);
            ordemServico.Cliente.Contato.Email = "outro@cliente.com";
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            await _emailServico.Received(1).EnviarAsync(
                Arg.Is<IEnumerable<string>>(emails => emails.Contains("outro@cliente.com")),
                "Serviço Finalizado",
                Arg.Any<string>());
        }

        private static OrdemServicoResponse CriarOrdemServicoResponse(Guid ordemServicoId)
        {
            return new OrdemServicoResponse
            {
                Id = ordemServicoId,
                Cliente = new ClienteResponse
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente Teste",
                    Contato = new ContatoResponse
                    {
                        Email = "cliente@teste.com",
                        Telefone = "(11) 99999-9999"
                    }
                },
                Servico = new ServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Nome = "Troca de Óleo",
                    Valor = 50.00m
                },
                Veiculo = new VeiculoResponse
                {
                    Id = Guid.NewGuid(),
                    Modelo = "Honda Civic",
                    Placa = "ABC-1234"
                }
            };
        }
    }
}
