using Adapters.Gateways;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class LogGatewayTests
    {
        private readonly ILogServico<TestClass> _logServico;
        private readonly LogGateway<TestClass> _gateway;

        public LogGatewayTests()
        {
            _logServico = Substitute.For<ILogServico<TestClass>>();
            _gateway = new LogGateway<TestClass>(_logServico);
        }

        [Fact]
        public void Construtor_ComLogServicoNulo_DeveLancarArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new LogGateway<TestClass>(null!));
            exception.ParamName.Should().Be("logServico");
        }

        [Fact]
        public void LogInicio_SemProps_DeveChamarLogServico()
        {
            // Arrange
            var metodo = "TesteMetodo";

            // Act
            _gateway.LogInicio(metodo);

            // Assert
            _logServico.Received(1).LogInicio(metodo, null);
        }

        [Fact]
        public void LogInicio_ComProps_DeveChamarLogServicoComProps()
        {
            // Arrange
            var metodo = "TesteMetodo";
            var props = new { Id = 1, Nome = "Teste" };

            // Act
            _gateway.LogInicio(metodo, props);

            // Assert
            _logServico.Received(1).LogInicio(metodo, props);
        }

        [Fact]
        public void LogFim_SemRetorno_DeveChamarLogServico()
        {
            // Arrange
            var metodo = "TesteMetodo";

            // Act
            _gateway.LogFim(metodo);

            // Assert
            _logServico.Received(1).LogFim(metodo, null);
        }

        [Fact]
        public void LogFim_ComRetorno_DeveChamarLogServicoComRetorno()
        {
            // Arrange
            var metodo = "TesteMetodo";
            var retorno = new { Sucesso = true };

            // Act
            _gateway.LogFim(metodo, retorno);

            // Assert
            _logServico.Received(1).LogFim(metodo, retorno);
        }

        [Fact]
        public void LogErro_DeveChamarLogServicoComExcecao()
        {
            // Arrange
            var metodo = "TesteMetodo";
            var excecao = new Exception("Erro de teste");

            // Act
            _gateway.LogErro(metodo, excecao);

            // Assert
            _logServico.Received(1).LogErro(metodo, excecao);
        }

        [Fact]
        public void LogErro_ComDiferentesTiposDeExcecao_DeveChamarLogServico()
        {
            // Arrange
            var metodo = "TesteMetodo";
            var excecao1 = new InvalidOperationException("Operação inválida");
            var excecao2 = new ArgumentException("Argumento inválido");

            // Act
            _gateway.LogErro(metodo, excecao1);
            _gateway.LogErro(metodo, excecao2);

            // Assert
            _logServico.Received(1).LogErro(metodo, excecao1);
            _logServico.Received(1).LogErro(metodo, excecao2);
        }

        [Fact]
        public void LogInicio_LogFim_LogErro_DevemSerIndependentes()
        {
            // Arrange
            var metodo = "TesteMetodo";
            var props = new { Id = 1 };
            var retorno = new { Sucesso = true };
            var excecao = new Exception("Erro");

            // Act
            _gateway.LogInicio(metodo, props);
            _gateway.LogFim(metodo, retorno);
            _gateway.LogErro(metodo, excecao);

            // Assert
            _logServico.Received(1).LogInicio(metodo, props);
            _logServico.Received(1).LogFim(metodo, retorno);
            _logServico.Received(1).LogErro(metodo, excecao);
        }

        // Classe auxiliar para testes genéricos
        public class TestClass
        {
            public int Id { get; set; }
            public string? Nome { get; set; }
        }
    }
}
