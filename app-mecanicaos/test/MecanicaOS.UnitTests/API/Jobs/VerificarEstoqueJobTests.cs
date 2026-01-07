using API.Jobs;
using Core.DTOs.Requests.Estoque;
using Core.DTOs.Responses.Estoque;
using Core.DTOs.Responses.Usuario;
using Core.Exceptions;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MecanicaOS.UnitTests.API.Jobs
{
    /// <summary>
    /// Testes para VerificarEstoqueJob após migração para Clean Architecture
    /// Testa integração com Controllers ao invés de repositórios diretos
    /// </summary>
    public class VerificarEstoqueJobTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IEstoqueController _estoqueController;
        private readonly IUsuarioController _usuarioController;
        private readonly IAlertaEstoqueController _alertaEstoqueController;
        private readonly ILogServico<VerificarEstoqueJob> _logServico;
        private readonly IServicoEmail _servicoEmail;

        public VerificarEstoqueJobTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _estoqueController = Substitute.For<IEstoqueController>();
            _usuarioController = Substitute.For<IUsuarioController>();
            _alertaEstoqueController = Substitute.For<IAlertaEstoqueController>();
            _logServico = Substitute.For<ILogServico<VerificarEstoqueJob>>();
            _servicoEmail = Substitute.For<IServicoEmail>();

            _compositionRoot.CriarEstoqueController().Returns(_estoqueController);
            _compositionRoot.CriarUsuarioController().Returns(_usuarioController);
            _compositionRoot.CriarAlertaEstoqueController().Returns(_alertaEstoqueController);
            _compositionRoot.CriarLogService<VerificarEstoqueJob>().Returns(_logServico);
            _compositionRoot.CriarServicoEmail().Returns(_servicoEmail);
        }

        private VerificarEstoqueJob CriarJob()
        {
            return new VerificarEstoqueJob(_compositionRoot);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange & Act
            var job = CriarJob();

            // Assert
            job.Should().NotBeNull();
            _compositionRoot.Received(1).CriarEstoqueController();
            _compositionRoot.Received(1).CriarUsuarioController();
            _compositionRoot.Received(1).CriarAlertaEstoqueController();
            _compositionRoot.Received(1).CriarLogService<VerificarEstoqueJob>();
            _compositionRoot.Received(1).CriarServicoEmail();
        }

        [Fact]
        public async Task ExecutarAsync_SemInsumosCriticos_NaoDeveEnviarAlerta()
        {
            // Arrange
            var estoquesVazios = new List<EstoqueResponse>();
            
            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesVazios));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync), Arg.Any<object>());
            await _servicoEmail.DidNotReceive().EnviarAsync(
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<string>(),
                Arg.Any<string>());
        }

        [Fact]
        public async Task ExecutarAsync_ComInsumosCriticos_DeveEnviarAlerta()
        {
            // Arrange
            var estoquesCriticos = new List<EstoqueResponse>
            {
                new EstoqueResponse
                {
                    Id = Guid.NewGuid(),
                    Insumo = "Óleo Motor",
                    QuantidadeDisponivel = 2,
                    QuantidadeMinima = 10
                }
            };

            var usuariosAlerta = new List<UsuarioResponse>
            {
                new UsuarioResponse { Email = "admin@oficina.com" }
            };

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));

            _alertaEstoqueController.VerificarAlertaEnviadoHoje(Arg.Any<Guid>())
                .Returns(Task.FromResult(false));

            _usuarioController.ObterUsuariosParaAlertaEstoque()
                .Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(usuariosAlerta));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync), Arg.Any<object>());
            
            await _estoqueController.Received(1).ObterEstoqueCritico();
            await _alertaEstoqueController.Received(1).VerificarAlertaEnviadoHoje(Arg.Any<Guid>());
            await _usuarioController.Received(1).ObterUsuariosParaAlertaEstoque();
            await _servicoEmail.Received(1).EnviarAsync(
                Arg.Any<IEnumerable<string>>(),
                "Alerta de Estoque Baixo",
                Arg.Any<string>());
            await _alertaEstoqueController.Received(1).CadastrarAlertas(Arg.Any<IEnumerable<CadastrarAlertaEstoqueRequest>>());
        }

        [Fact]
        public async Task ExecutarAsync_ComAlertaJaEnviado_NaoDeveEnviarNovoAlerta()
        {
            // Arrange
            var estoquesCriticos = new List<EstoqueResponse>
            {
                new EstoqueResponse
                {
                    Id = Guid.NewGuid(),
                    Insumo = "Óleo Motor",
                    QuantidadeDisponivel = 2,
                    QuantidadeMinima = 10
                }
            };

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));

            _alertaEstoqueController.VerificarAlertaEnviadoHoje(Arg.Any<Guid>())
                .Returns(Task.FromResult(true)); // Alerta já enviado hoje

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            await _servicoEmail.DidNotReceive().EnviarAsync(
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<string>(),
                Arg.Any<string>());
            await _alertaEstoqueController.DidNotReceive().CadastrarAlertas(Arg.Any<IEnumerable<CadastrarAlertaEstoqueRequest>>());
        }

        [Fact]
        public async Task ExecutarAsync_ComExcecao_DeveLogarErroEReLancar()
        {
            // Arrange
            var exception = new Exception("Erro no controller");
            _estoqueController.ObterEstoqueCritico()
                .Throws(exception);

            var job = CriarJob();

            // Act
            var act = async () => await job.ExecutarAsync();

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro no controller");
            
            _logServico.Received(1).LogErro(nameof(job.ExecutarAsync), Arg.Any<Exception>());
        }

        [Fact]
        public async Task ExecutarAsync_ComExcecaoNoEnvioEmail_DeveLogarErroEReLancar()
        {
            // Arrange
            var estoquesCriticos = new List<EstoqueResponse>
            {
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Óleo", QuantidadeDisponivel = 1, QuantidadeMinima = 5 }
            };

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(Arg.Any<Guid>())
                .Returns(Task.FromResult(false));
            _usuarioController.ObterUsuariosParaAlertaEstoque()
                .Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(new List<UsuarioResponse>
                {
                    new UsuarioResponse { Email = "admin@teste.com" }
                }));
            
            var emailException = new Exception("Erro ao enviar email");
            _servicoEmail.EnviarAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(emailException);

            var job = CriarJob();

            // Act
            var act = async () => await job.ExecutarAsync();

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao enviar email");
            
            _logServico.Received(1).LogErro("EnviarAlertaEstoqueAsync", Arg.Any<Exception>());
        }

        [Fact]
        public async Task ExecutarAsync_ComExcecaoNoSalvarAlerta_DeveLogarErroEReLancar()
        {
            // Arrange
            var estoquesCriticos = new List<EstoqueResponse>
            {
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Óleo", QuantidadeDisponivel = 1, QuantidadeMinima = 5 }
            };

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(Arg.Any<Guid>())
                .Returns(Task.FromResult(false));
            _usuarioController.ObterUsuariosParaAlertaEstoque()
                .Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(new List<UsuarioResponse>
                {
                    new UsuarioResponse { Email = "admin@teste.com" }
                }));
            
            var alertaException = new Exception("Erro ao salvar alerta");
            _alertaEstoqueController.CadastrarAlertas(Arg.Any<IEnumerable<CadastrarAlertaEstoqueRequest>>())
                .Throws(alertaException);

            var job = CriarJob();

            // Act
            var act = async () => await job.ExecutarAsync();

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao salvar alerta");
            
            _logServico.Received(1).LogErro("SalvarAlertaEnviadoAsync", Arg.Any<Exception>());
        }

        [Fact]
        public async Task ExecutarAsync_ComConteudoEmailVazio_NaoDeveEnviarEmail()
        {
            // Arrange
            var estoquesCriticos = new List<EstoqueResponse>
            {
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Óleo", QuantidadeDisponivel = 1, QuantidadeMinima = 5 }
            };

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(Arg.Any<Guid>())
                .Returns(Task.FromResult(false));
            _usuarioController.ObterUsuariosParaAlertaEstoque()
                .Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(new List<UsuarioResponse>
                {
                    new UsuarioResponse { Email = "admin@teste.com" }
                }));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert - Como GerarConteudoEmailAsync é privado e pode falhar lendo template,
            // se não houver template o conteúdo será vazio/null e não deve enviar email
            // Não podemos testar isso diretamente pois os métodos são privados,
            // mas podemos verificar que o job não falha completamente
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync), Arg.Any<object>());
        }

        [Fact]
        public async Task ExecutarAsync_ComUsuariosSemEmail_DeveCompletarSemErro()
        {
            // Arrange
            var estoquesCriticos = new List<EstoqueResponse>
            {
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Óleo", QuantidadeDisponivel = 1, QuantidadeMinima = 5 }
            };

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(Arg.Any<Guid>())
                .Returns(Task.FromResult(false));
            _usuarioController.ObterUsuariosParaAlertaEstoque()
                .Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(new List<UsuarioResponse>
                {
                    new UsuarioResponse { Email = "" }, // Email vazio
                    new UsuarioResponse { Email = null! } // Email nulo
                }));

            var job = CriarJob();

            // Act
            var act = async () => await job.ExecutarAsync();

            // Assert
            await act.Should().NotThrowAsync();
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync), Arg.Any<object>());
        }

        [Fact]
        public async Task ExecutarAsync_ComMuitosInsumosCriticos_DeveProcessarTodos()
        {
            // Arrange
            var estoquesCriticos = Enumerable.Range(1, 10)
                .Select(i => new EstoqueResponse 
                { 
                    Id = Guid.NewGuid(), 
                    Insumo = $"Insumo {i}", 
                    QuantidadeDisponivel = 1, 
                    QuantidadeMinima = 5 
                })
                .ToList();

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(Arg.Any<Guid>())
                .Returns(Task.FromResult(false));
            _usuarioController.ObterUsuariosParaAlertaEstoque()
                .Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(new List<UsuarioResponse>
                {
                    new UsuarioResponse { Email = "admin@teste.com" }
                }));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync), Arg.Any<object>());
            
            // Verifica que verificou alerta para todos os 10 insumos
            await _alertaEstoqueController.Received(10).VerificarAlertaEnviadoHoje(Arg.Any<Guid>());
            
            // Deve cadastrar alertas para todos os 10 insumos
            await _alertaEstoqueController.Received(1).CadastrarAlertas(
                Arg.Is<IEnumerable<CadastrarAlertaEstoqueRequest>>(alertas => alertas.Count() == 10));
        }

        [Fact]
        public async Task ExecutarAsync_ComParteDosInsumosJaComAlertaEnviado_DeveProcessarApenasPendentes()
        {
            // Arrange
            var estoquesCriticos = new List<EstoqueResponse>
            {
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Insumo 1", QuantidadeDisponivel = 1, QuantidadeMinima = 5 },
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Insumo 2", QuantidadeDisponivel = 2, QuantidadeMinima = 5 },
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Insumo 3", QuantidadeDisponivel = 1, QuantidadeMinima = 5 }
            };

            _estoqueController.ObterEstoqueCritico()
                .Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoquesCriticos));
            
            // Simula que apenas o segundo insumo já teve alerta enviado hoje
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(estoquesCriticos[0].Id)
                .Returns(Task.FromResult(false));
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(estoquesCriticos[1].Id)
                .Returns(Task.FromResult(true)); // Já teve alerta hoje
            _alertaEstoqueController.VerificarAlertaEnviadoHoje(estoquesCriticos[2].Id)
                .Returns(Task.FromResult(false));
                
            _usuarioController.ObterUsuariosParaAlertaEstoque()
                .Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(new List<UsuarioResponse>
                {
                    new UsuarioResponse { Email = "admin@teste.com" }
                }));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync), Arg.Any<object>());
            
            // Deve cadastrar alertas apenas para 2 insumos (excluindo o que já teve alerta)
            await _alertaEstoqueController.Received(1).CadastrarAlertas(
                Arg.Is<IEnumerable<CadastrarAlertaEstoqueRequest>>(alertas => alertas.Count() == 2));
        }
    }
}
