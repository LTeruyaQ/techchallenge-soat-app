using FluentAssertions;
using Infraestrutura.Servicos;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace MecanicaOS.UnitTests.Infraestrutura.Servicos
{
    /// <summary>
    /// Testes para ServicoEmail (Infraestrutura)
    /// 
    /// IMPORTÂNCIA: Serviço crítico para comunicação com clientes via email.
    /// Utilizado para notificações de estoque crítico, confirmações de ordens de serviço, etc.
    /// 
    /// COBERTURA: Valida inicialização do serviço e configuração da API Key.
    /// Nota: Testes de envio real requerem mock do SendGridClient (não implementado aqui para evitar dependências externas).
    /// 
    /// LIMITAÇÃO: Testes completos de envio de email requerem integração com SendGrid ou mocks avançados.
    /// Estes testes focam na configuração e estrutura básica do serviço.
    /// </summary>
    public class ServicoEmailTests
    {
        [Fact]
        public void Construtor_ComConfigurationValida_DeveCriarInstancia()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            configuration["SENDGRID_APIKEY"].Returns("test-api-key-123");

            // Act
            var servico = new ServicoEmail(configuration);

            // Assert
            servico.Should().NotBeNull("Serviço deve ser criado com sucesso");
        }

        [Fact]
        public void Construtor_DeveObterApiKeyDaConfiguration()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            var apiKey = "sendgrid-api-key-test";
            configuration["SENDGRID_APIKEY"].Returns(apiKey);

            // Act
            var servico = new ServicoEmail(configuration);

            // Assert
            var receivedKey = configuration.Received(1)["SENDGRID_APIKEY"];
            servico.Should().NotBeNull();
        }

        [Fact]
        public void Construtor_ComDiferentesApiKeys_DeveCriarInstancias()
        {
            // Arrange
            var config1 = Substitute.For<IConfiguration>();
            config1["SENDGRID_APIKEY"].Returns("key-1");

            var config2 = Substitute.For<IConfiguration>();
            config2["SENDGRID_APIKEY"].Returns("key-2");

            // Act
            var servico1 = new ServicoEmail(config1);
            var servico2 = new ServicoEmail(config2);

            // Assert
            servico1.Should().NotBeNull();
            servico2.Should().NotBeNull();
            servico1.Should().NotBeSameAs(servico2, "Cada instância deve ser independente");
        }

        [Fact]
        public void EnviarAsync_DeveSerMetodoPublico()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            configuration["SENDGRID_APIKEY"].Returns("test-key");
            var servico = new ServicoEmail(configuration);

            // Act
            var metodo = servico.GetType().GetMethod("EnviarAsync");

            // Assert
            metodo.Should().NotBeNull("Método EnviarAsync deve existir");
            metodo!.IsPublic.Should().BeTrue("Método EnviarAsync deve ser público");
            metodo.ReturnType.Should().Be(typeof(Task), "Método deve retornar Task");
        }

        [Fact]
        public void EnviarAsync_DeveAceitarParametrosCorretos()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            configuration["SENDGRID_APIKEY"].Returns("test-key");
            var servico = new ServicoEmail(configuration);

            // Act
            var metodo = servico.GetType().GetMethod("EnviarAsync");
            var parametros = metodo!.GetParameters();

            // Assert
            parametros.Should().HaveCount(3, "Método deve ter 3 parâmetros");
            parametros[0].Name.Should().Be("emailsDestino");
            parametros[0].ParameterType.Should().Be(typeof(IEnumerable<string>));
            parametros[1].Name.Should().Be("assunto");
            parametros[1].ParameterType.Should().Be(typeof(string));
            parametros[2].Name.Should().Be("conteudo");
            parametros[2].ParameterType.Should().Be(typeof(string));
        }

        [Fact]
        public void ServicoEmail_DeveImplementarIServicoEmail()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            configuration["SENDGRID_APIKEY"].Returns("test-key");

            // Act
            var servico = new ServicoEmail(configuration);

            // Assert
            servico.Should().BeAssignableTo<global::Core.Interfaces.Servicos.IServicoEmail>(
                "ServicoEmail deve implementar a interface IServicoEmail");
        }

        [Fact]
        public void Construtor_ComApiKeyVazia_DeveCriarInstancia()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            configuration["SENDGRID_APIKEY"].Returns("");

            // Act
            var servico = new ServicoEmail(configuration);

            // Assert
            servico.Should().NotBeNull("Serviço deve ser criado mesmo com API key vazia");
        }

        [Fact]
        public void Construtor_ComApiKeyNula_DeveCriarInstancia()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            configuration["SENDGRID_APIKEY"].Returns((string?)null);

            // Act & Assert
            // O operador ! força a conversão, mas em runtime pode causar NullReferenceException
            // Este teste valida que o construtor aceita a configuração
            var action = () => new ServicoEmail(configuration);
            action.Should().NotThrow("Construtor deve aceitar configuration mesmo com API key nula");
        }

        [Fact]
        public void ServicoEmail_DeveSerClassePublica()
        {
            // Arrange & Act
            var tipo = typeof(ServicoEmail);

            // Assert
            tipo.IsPublic.Should().BeTrue("ServicoEmail deve ser uma classe pública");
            tipo.IsClass.Should().BeTrue("ServicoEmail deve ser uma classe");
            tipo.IsAbstract.Should().BeFalse("ServicoEmail não deve ser abstrata");
        }

        [Fact]
        public void ServicoEmail_DeveTerConstrutorPublico()
        {
            // Arrange & Act
            var construtores = typeof(ServicoEmail).GetConstructors();

            // Assert
            construtores.Should().HaveCount(1, "Deve ter exatamente um construtor público");
            construtores[0].IsPublic.Should().BeTrue("Construtor deve ser público");
            
            var parametros = construtores[0].GetParameters();
            parametros.Should().HaveCount(1, "Construtor deve ter 1 parâmetro");
            parametros[0].ParameterType.Should().Be(typeof(IConfiguration));
        }
    }
}
