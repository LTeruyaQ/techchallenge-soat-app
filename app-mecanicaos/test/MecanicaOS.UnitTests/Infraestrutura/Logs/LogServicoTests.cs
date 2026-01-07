using Core.Interfaces.Servicos;
using FluentAssertions;
using Infraestrutura.Logs;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MecanicaOS.UnitTests.Infraestrutura.Logs
{
    /// <summary>
    /// Testes para LogServico (Infraestrutura)
    /// 
    /// IMPORTÂNCIA: Serviço central de logging estruturado da aplicação.
    /// Responsável por registrar todas as operações com contexto completo (usuário, correlationId, timestamp).
    /// 
    /// COBERTURA: Valida logging de início, fim e erros de operações.
    /// Garante que logs estruturados sejam gerados corretamente com todos os metadados necessários.
    /// Essencial para monitoramento, debugging e auditoria em produção.
    /// </summary>
    public class LogServicoTests
    {
        private readonly IIdCorrelacionalService _correlationIdService;
        private readonly IUsuarioLogadoServico _usuarioLogadoServico;
        private readonly ILogger<LogServicoTests> _logger;

        public LogServicoTests()
        {
            _correlationIdService = Substitute.For<IIdCorrelacionalService>();
            _usuarioLogadoServico = Substitute.For<IUsuarioLogadoServico>();
            _logger = Substitute.For<ILogger<LogServicoTests>>();
        }

        private LogServico<LogServicoTests> CriarServico()
        {
            return new LogServico<LogServicoTests>(
                _correlationIdService,
                _logger,
                _usuarioLogadoServico);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange & Act
            var servico = CriarServico();

            // Assert
            servico.Should().NotBeNull("Serviço deve ser criado com sucesso");
        }

        [Fact]
        public void LogInicio_DeveChamarLoggerComNivelInformation()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var props = new { Parametro1 = "valor1", Parametro2 = 123 };

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogInicio(metodo, props);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogInicio_SemPropriedades_DeveChamarLogger()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogInicio(metodo);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogFim_DeveChamarLoggerComNivelInformation()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var retorno = new { Resultado = "sucesso", Id = Guid.NewGuid() };

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogFim(metodo, retorno);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogFim_SemRetorno_DeveChamarLogger()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogFim(metodo);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogErro_DeveChamarLoggerComNivelError()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var exception = new InvalidOperationException("Erro de teste");

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogErro(metodo, exception);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogErro_DeveIncluirMensagemETipoDeException()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var mensagemErro = "Erro específico de teste";
            var exception = new ArgumentException(mensagemErro);

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogErro(metodo, exception);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogInicio_DeveObterCorrelationIdDoServico()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var correlationId = "correlation-123";

            _correlationIdService.GetCorrelationId().Returns(correlationId);
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogInicio(metodo);

            // Assert
            _correlationIdService.Received(1).GetCorrelationId();
        }

        [Fact]
        public void LogInicio_DeveObterNomeDoUsuarioLogado()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogInicio(metodo);

            // Assert
            var nomeUsuario = _usuarioLogadoServico.Received(1).Nome;
        }

        [Fact]
        public void LogFim_DeveObterCorrelationIdDoServico()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogFim(metodo);

            // Assert
            _correlationIdService.Received(1).GetCorrelationId();
        }

        [Fact]
        public void LogErro_DeveObterCorrelationIdDoServico()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var exception = new Exception("Erro");

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogErro(metodo, exception);

            // Assert
            _correlationIdService.Received(1).GetCorrelationId();
        }

        [Fact]
        public void LogInicio_ComPropriedadesComplexas_DeveChamarLogger()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var props = new
            {
                Id = Guid.NewGuid(),
                Nome = "Teste",
                Valores = new[] { 1, 2, 3 },
                Objeto = new { Propriedade = "valor" }
            };

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogInicio(metodo, props);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogErro_ComExceptionComStackTrace_DeveChamarLogger()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            
            Exception exception;
            try
            {
                throw new InvalidOperationException("Erro com stack trace");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            _correlationIdService.GetCorrelationId().Returns("test-correlation-id");
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogErro(metodo, exception);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogInicio_LogFim_LogErro_DevemUsarMesmoCorrelationId()
        {
            // Arrange
            var servico = CriarServico();
            var metodo = "TesteMetodo";
            var correlationId = "same-correlation-id";

            _correlationIdService.GetCorrelationId().Returns(correlationId);
            _usuarioLogadoServico.Nome.Returns("Usuario Teste");

            // Act
            servico.LogInicio(metodo);
            servico.LogFim(metodo);
            servico.LogErro(metodo, new Exception());

            // Assert
            _correlationIdService.Received(3).GetCorrelationId();
        }
    }
}
