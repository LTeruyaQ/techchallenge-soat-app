using Core.Exceptions;

namespace MecanicaOS.UnitTests.Core.Exceptions
{
    /// <summary>
    /// Testes para Exceptions customizadas
    /// ROI MÉDIO: Valida que exceptions são criadas corretamente e têm mensagens adequadas.
    /// </summary>
    public class ExceptionsTests
    {
        [Fact]
        public void CredenciaisInvalidasException_ComMensagem_DeveCriarCorretamente()
        {
            var mensagem = "Credenciais inválidas";
            var exception = new CredenciaisInvalidasException(mensagem);
            exception.Message.Should().Be(mensagem);
        }

        [Fact]
        public void DadosJaCadastradosException_ComMensagem_DeveCriarCorretamente()
        {
            var mensagem = "Dados já cadastrados";
            var exception = new DadosJaCadastradosException(mensagem);
            exception.Message.Should().Be(mensagem);
        }

        [Fact]
        public void InsumoApagadoException_ComMensagem_DeveCriarCorretamente()
        {
            var mensagem = "Insumo foi apagado";
            var exception = new InsumoApagadoException(mensagem);
            exception.Message.Should().Be(mensagem);
        }

        [Fact]
        public void InsumosIndisponiveisException_ComMensagem_DeveCriarCorretamente()
        {
            var mensagem = "Insumos indisponíveis";
            var exception = new InsumosIndisponiveisException(mensagem);
            exception.Message.Should().Be(mensagem);
        }

        [Fact]
        public void ServicoIndisponivelException_ComMensagem_DeveCriarCorretamente()
        {
            var mensagem = "Serviço indisponível";
            var exception = new ServicoIndisponivelException(mensagem);
            exception.Message.Should().Be(mensagem);
        }

        [Fact]
        public void UsuarioInativoException_ComMensagem_DeveCriarCorretamente()
        {
            var mensagem = "Usuário inativo";
            var exception = new UsuarioInativoException(mensagem);
            exception.Message.Should().Be(mensagem);
        }

        [Fact]
        public void OrcamentoExpiradoException_ComMensagem_DeveCriarCorretamente()
        {
            var mensagem = "Orçamento expirado";
            var exception = new OrcamentoExpiradoException(mensagem);
            exception.Message.Should().Be(mensagem);
        }

        // Testes para construtores sem parâmetros
        [Fact]
        public void DadosJaCadastradosException_SemParametros_DeveCriarCorretamente()
        {
            var exception = new DadosJaCadastradosException();
            exception.Should().NotBeNull();
        }

        [Fact]
        public void CredenciaisInvalidasException_SemParametros_DeveCriarCorretamente()
        {
            var exception = new CredenciaisInvalidasException();
            exception.Should().NotBeNull();
        }

        [Fact]
        public void UsuarioInativoException_SemParametros_DeveCriarCorretamente()
        {
            var exception = new UsuarioInativoException();
            exception.Should().NotBeNull();
        }

        // Testes para construtores com InnerException
        [Fact]
        public void DadosJaCadastradosException_ComInnerException_DeveCriarCorretamente()
        {
            var mensagem = "Dados já cadastrados";
            var innerException = new InvalidOperationException("Erro interno");
            var exception = new DadosJaCadastradosException(mensagem, innerException);
            
            exception.Message.Should().Be(mensagem);
            exception.InnerException.Should().Be(innerException);
        }

        // CredenciaisInvalidasException e UsuarioInativoException não possuem construtor com InnerException
    }
}
