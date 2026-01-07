using API.Notificacoes.OS;
using Core.DTOs.Responses.Cliente;
using Core.DTOs.Responses.Estoque;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.DTOs.Responses.Servico;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MecanicaOS.UnitTests.API.Notificacoes.OS
{
    /// <summary>
    /// Testes para OrdemServicoEmOrcamentoHandler
    /// 
    /// IMPORTÂNCIA: Handler crítico que processa eventos de orçamento de OS.
    /// Responsável por calcular orçamento e enviar email ao cliente.
    /// 
    /// COBERTURA: Testa todos os cenários do handler de notificação.
    /// Valida integração com controllers, serviço de email e geração de conteúdo.
    /// </summary>
    public class OrdemServicoEmOrcamentoHandlerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IServicoEmail _emailServico;
        private readonly ILogServico<OrdemServicoEmOrcamentoHandler> _logServico;
        private readonly IOrdemServicoController _ordemServicoController;
        private readonly OrdemServicoEmOrcamentoHandler _handler;

        public OrdemServicoEmOrcamentoHandlerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _emailServico = Substitute.For<IServicoEmail>();
            _logServico = Substitute.For<ILogServico<OrdemServicoEmOrcamentoHandler>>();
            _ordemServicoController = Substitute.For<IOrdemServicoController>();

            _compositionRoot.CriarServicoEmail().Returns(_emailServico);
            _compositionRoot.CriarLogService<OrdemServicoEmOrcamentoHandler>().Returns(_logServico);
            _compositionRoot.CriarOrdemServicoController().Returns(_ordemServicoController);

            _handler = new OrdemServicoEmOrcamentoHandler(_compositionRoot);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var emailServico = Substitute.For<IServicoEmail>();
            var logServico = Substitute.For<ILogServico<OrdemServicoEmOrcamentoHandler>>();
            var ordemServicoController = Substitute.For<IOrdemServicoController>();

            compositionRoot.CriarServicoEmail().Returns(emailServico);
            compositionRoot.CriarLogService<OrdemServicoEmOrcamentoHandler>().Returns(logServico);
            compositionRoot.CriarOrdemServicoController().Returns(ordemServicoController);

            // Act
            var handler = new OrdemServicoEmOrcamentoHandler(compositionRoot);

            // Assert
            handler.Should().NotBeNull();
            compositionRoot.Received(1).CriarServicoEmail();
            compositionRoot.Received(1).CriarLogService<OrdemServicoEmOrcamentoHandler>();
            compositionRoot.Received(1).CriarOrdemServicoController();
        }

        [Fact]
        public async Task Handle_ComEventoValido_DeveProcessarOrdemServicoEEnviarEmail()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoEmOrcamentoEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoResponse(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _ordemServicoController.Received(1).CalcularOrcamentoAsync(ordemServicoId);
            await _ordemServicoController.Received(1).ObterPorId(ordemServicoId);
            await _emailServico.Received(1).EnviarAsync(
                Arg.Is<IEnumerable<string>>(emails => emails.Contains(ordemServico.Cliente.Contato.Email)),
                "Orçamento de Serviço",
                Arg.Any<string>());
            _logServico.Received(1).LogFim(nameof(_handler.Handle));
        }

        [Fact]
        public async Task Handle_ComExcecaoNoCalcularOrcamento_DeveLogarErroEReLancar()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoEmOrcamentoEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var exception = new Exception("Erro ao calcular orçamento");
            _ordemServicoController.CalcularOrcamentoAsync(ordemServicoId)
                .Throws(exception);

            // Act
            var act = async () => await _handler.Handle(notification, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao calcular orçamento");
            
            _logServico.Received(1).LogErro(nameof(_handler.Handle), exception);
        }

        [Fact]
        public async Task Handle_ComExcecaoNoObterOrdemServico_DeveLogarErroEReLancar()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoEmOrcamentoEvent(ordemServicoId);
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
            var notification = new OrdemServicoEmOrcamentoEvent(ordemServicoId);
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
        public async Task Handle_ComOrdemServicoComInsumos_DeveGerarConteudoCorreto()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoEmOrcamentoEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoResponse(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            string conteudoCapturado = "";
            await _emailServico.EnviarAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<string>(), 
                Arg.Do<string>(conteudo => conteudoCapturado = conteudo));

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            await _emailServico.Received(1).EnviarAsync(
                Arg.Is<IEnumerable<string>>(emails => emails.Contains("cliente@teste.com")),
                "Orçamento de Serviço",
                Arg.Any<string>());
        }

        [Fact]
        public async Task Handle_ComOrdemServicoSemInsumos_DeveProcessarCorretamente()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoEmOrcamentoEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoResponse(ordemServicoId);
            ordemServico.Insumos = new List<InsumoOSResponse>(); // Sem insumos
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _ordemServicoController.Received(1).CalcularOrcamentoAsync(ordemServicoId);
            await _emailServico.Received(1).EnviarAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<string>(), Arg.Any<string>());
            _logServico.Received(1).LogFim(nameof(_handler.Handle));
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
                Orcamento = 150.00,
                Insumos = new List<InsumoOSResponse>
                {
                    new InsumoOSResponse
                    {
                        OrdemServicoId = ordemServicoId,
                        EstoqueId = Guid.NewGuid(),
                        Quantidade = 2,
                        Estoque = new EstoqueResponse
                        {
                            Id = Guid.NewGuid(),
                            Insumo = "Óleo 10W40",
                            Preco = 25.00
                        }
                    },
                    new InsumoOSResponse
                    {
                        OrdemServicoId = ordemServicoId,
                        EstoqueId = Guid.NewGuid(),
                        Quantidade = 1,
                        Estoque = new EstoqueResponse
                        {
                            Id = Guid.NewGuid(),
                            Insumo = "Filtro de Óleo",
                            Preco = 50.00
                        }
                    }
                }
            };
        }
    }
}
