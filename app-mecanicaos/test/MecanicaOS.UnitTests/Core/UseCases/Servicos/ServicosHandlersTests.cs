using Core.DTOs.UseCases.Servico;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Servicos.CadastrarServico;
using Core.UseCases.Servicos.EditarServico;
using Core.UseCases.Servicos.DeletarServico;
using Core.UseCases.Servicos.ObterServico;
using Core.UseCases.Servicos.ObterTodosServicos;
using Core.UseCases.Servicos.ObterServicoPorNome;
using Core.UseCases.Servicos.ObterServicosDisponiveis;

namespace MecanicaOS.UnitTests.Core.UseCases.Servicos
{
    /// <summary>
    /// Testes consolidados para todos os handlers de Serviços
    /// ROI CRÍTICO: Serviços são o core do negócio - precisam estar 100% corretos.
    /// Importância: Valida cadastro, edição, consulta e remoção de serviços.
    /// </summary>
    public class ServicosHandlersTests
    {
        private readonly IServicoGateway _servicoGateway;
        private readonly ILogGateway<CadastrarServicoHandler> _logGatewayCadastrar;
        private readonly ILogGateway<EditarServicoHandler> _logGatewayEditar;
        private readonly ILogGateway<DeletarServicoHandler> _logGatewayDeletar;
        private readonly ILogGateway<ObterServicoHandler> _logGatewayObter;
        private readonly ILogGateway<ObterTodosServicosHandler> _logGatewayTodos;
        private readonly ILogGateway<ObterServicoPorNomeHandler> _logGatewayPorNome;
        private readonly ILogGateway<ObterServicosDisponiveisHandler> _logGatewayDisponiveis;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public ServicosHandlersTests()
        {
            _servicoGateway = Substitute.For<IServicoGateway>();
            _logGatewayCadastrar = Substitute.For<ILogGateway<CadastrarServicoHandler>>();
            _logGatewayEditar = Substitute.For<ILogGateway<EditarServicoHandler>>();
            _logGatewayDeletar = Substitute.For<ILogGateway<DeletarServicoHandler>>();
            _logGatewayObter = Substitute.For<ILogGateway<ObterServicoHandler>>();
            _logGatewayTodos = Substitute.For<ILogGateway<ObterTodosServicosHandler>>();
            _logGatewayPorNome = Substitute.For<ILogGateway<ObterServicoPorNomeHandler>>();
            _logGatewayDisponiveis = Substitute.For<ILogGateway<ObterServicosDisponiveisHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        #region CadastrarServicoHandler

        [Fact]
        public async Task CadastrarServico_ComDadosValidos_DeveCadastrarComSucesso()
        {
            // Arrange
            var handler = new CadastrarServicoHandler(_servicoGateway, _logGatewayCadastrar, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarServicoUseCaseDto
            {
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };

            _servicoGateway.ObterServicosDisponiveisPorNomeAsync(request.Nome).Returns(Task.FromResult<Servico?>(null));
            _servicoGateway.CadastrarAsync(Arg.Any<Servico>()).Returns(Task.FromResult(new Servico { Nome = "Teste", Descricao = "Teste", Valor = 100m }));

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Troca de Óleo");
            resultado.Valor.Should().Be(150.00m);
            await _servicoGateway.Received(1).CadastrarAsync(Arg.Any<Servico>());
        }

        [Fact]
        public async Task CadastrarServico_ComNomeVazio_DeveLancarException()
        {
            // Arrange
            var handler = new CadastrarServicoHandler(_servicoGateway, _logGatewayCadastrar, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarServicoUseCaseDto
            {
                Nome = "",
                Descricao = "Teste",
                Valor = 100m,
                Disponivel = true
            };

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosInvalidosException>()
                .WithMessage("Nome é obrigatório");
        }

        [Fact]
        public async Task CadastrarServico_ComValorZero_DeveLancarException()
        {
            // Arrange
            var handler = new CadastrarServicoHandler(_servicoGateway, _logGatewayCadastrar, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarServicoUseCaseDto
            {
                Nome = "Serviço",
                Descricao = "Teste",
                Valor = 0m,
                Disponivel = true
            };

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosInvalidosException>()
                .WithMessage("Valor deve ser maior que zero");
        }

        [Fact]
        public async Task CadastrarServico_ComNomeDuplicado_DeveLancarException()
        {
            // Arrange
            var handler = new CadastrarServicoHandler(_servicoGateway, _logGatewayCadastrar, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarServicoUseCaseDto
            {
                Nome = "Serviço Existente",
                Descricao = "Teste",
                Valor = 100m,
                Disponivel = true
            };

            _servicoGateway.ObterServicosDisponiveisPorNomeAsync(request.Nome)
                .Returns(Task.FromResult<Servico?>(new Servico { Nome = "Serviço Existente", Descricao = "Teste", Valor = 100m }));

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosJaCadastradosException>();
        }

        #endregion

        #region EditarServicoHandler

        [Fact]
        public async Task EditarServico_ComDadosValidos_DeveAtualizarComSucesso()
        {
            // Arrange
            var handler = new EditarServicoHandler(_servicoGateway, _logGatewayEditar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var servicoExistente = new Servico
            {
                Id = servicoId,
                Nome = "Serviço Antigo",
                Descricao = "Descrição antiga",
                Valor = 100m,
                Disponivel = true
            };
            var request = new EditarServicoUseCaseDto
            {
                Nome = "Serviço Novo",
                Descricao = "Descrição nova",
                Valor = 150m,
                Disponivel = false
            };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(servicoExistente));
            _servicoGateway.EditarAsync(Arg.Any<Servico>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(servicoId, request);

            // Assert
            resultado.Nome.Should().Be("Serviço Novo");
            resultado.Valor.Should().Be(150m);
            resultado.Disponivel.Should().BeFalse();
        }

        [Fact]
        public async Task EditarServico_ComServicoInexistente_DeveLancarException()
        {
            // Arrange
            var handler = new EditarServicoHandler(_servicoGateway, _logGatewayEditar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var request = new EditarServicoUseCaseDto { Nome = "Teste", Descricao = "Teste", Valor = 100m, Disponivel = true };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(servicoId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>();
        }

        #endregion

        #region DeletarServicoHandler

        [Fact]
        public async Task DeletarServico_ComServicoExistente_DeveDeletarComSucesso()
        {
            // Arrange
            var handler = new DeletarServicoHandler(_servicoGateway, _logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var servico = new Servico { Id = servicoId, Nome = "Serviço", Descricao = "Teste", Valor = 100m };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(servico));
            _servicoGateway.DeletarAsync(Arg.Any<Servico>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(servicoId);

            // Assert
            resultado.Should().BeTrue();
            await _servicoGateway.Received(1).DeletarAsync(Arg.Any<Servico>());
        }

        [Fact]
        public async Task DeletarServico_ComServicoInexistente_DeveLancarException()
        {
            // Arrange
            var handler = new DeletarServicoHandler(_servicoGateway, _logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(servicoId))
                .Should().ThrowAsync<DadosNaoEncontradosException>();
        }

        #endregion

        #region ObterServicoHandler

        [Fact]
        public async Task ObterServico_ComIdExistente_DeveRetornarServico()
        {
            // Arrange
            var handler = new ObterServicoHandler(_servicoGateway, _logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var servico = new Servico { Id = servicoId, Nome = "Serviço Teste", Descricao = "Teste", Valor = 100m };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(servico));

            // Act
            var resultado = await handler.Handle(servicoId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(servicoId);
        }

        [Fact]
        public async Task ObterServico_ComIdInexistente_DeveLancarException()
        {
            // Arrange
            var handler = new ObterServicoHandler(_servicoGateway, _logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(servicoId))
                .Should().ThrowAsync<DadosNaoEncontradosException>();
        }

        #endregion

        #region ObterTodosServicosHandler

        [Fact]
        public async Task ObterTodosServicos_DeveRetornarListaCompleta()
        {
            // Arrange
            var handler = new ObterTodosServicosHandler(_servicoGateway, _logGatewayTodos, _udtGateway, _usuarioLogadoGateway);
            var servicos = new List<Servico>
            {
                new Servico { Nome = "Serviço 1", Descricao = "Teste 1", Valor = 100m },
                new Servico { Nome = "Serviço 2", Descricao = "Teste 2", Valor = 200m }
            };

            _servicoGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Servico>>(servicos));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObterTodosServicos_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var handler = new ObterTodosServicosHandler(_servicoGateway, _logGatewayTodos, _udtGateway, _usuarioLogadoGateway);
            _servicoGateway.ObterTodosAsync().Returns(Task.FromException<IEnumerable<Servico>>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }

        #endregion

        #region ObterServicoPorNomeHandler

        [Fact]
        public async Task ObterServicoPorNome_ComNomeExistente_DeveRetornarServico()
        {
            // Arrange
            var handler = new ObterServicoPorNomeHandler(_servicoGateway, _logGatewayPorNome, _udtGateway, _usuarioLogadoGateway);
            var servico = new Servico { Nome = "Troca de Óleo", Descricao = "Teste", Valor = 150m };

            _servicoGateway.ObterServicosDisponiveisPorNomeAsync("Troca de Óleo")
                .Returns(Task.FromResult<Servico?>(servico));

            // Act
            var resultado = await handler.Handle("Troca de Óleo");

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be("Troca de Óleo");
        }

        [Fact]
        public async Task ObterServicoPorNome_ComNomeInexistente_DeveRetornarNull()
        {
            // Arrange
            var handler = new ObterServicoPorNomeHandler(_servicoGateway, _logGatewayPorNome, _udtGateway, _usuarioLogadoGateway);

            _servicoGateway.ObterServicosDisponiveisPorNomeAsync("Serviço Inexistente")
                .Returns(Task.FromResult<Servico?>(null));

            // Act
            var resultado = await handler.Handle("Serviço Inexistente");

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObterServicoPorNome_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var handler = new ObterServicoPorNomeHandler(_servicoGateway, _logGatewayPorNome, _udtGateway, _usuarioLogadoGateway);
            _servicoGateway.ObterServicosDisponiveisPorNomeAsync("Teste").Returns(Task.FromException<Servico?>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle("Teste"));
        }

        #endregion

        #region ObterServicosDisponiveisHandler

        [Fact]
        public async Task ObterServicosDisponiveis_DeveRetornarApenasDisponiveis()
        {
            // Arrange
            var handler = new ObterServicosDisponiveisHandler(_servicoGateway, _logGatewayDisponiveis, _udtGateway, _usuarioLogadoGateway);
            var servicos = new List<Servico>
            {
                new Servico { Nome = "Disponível", Descricao = "Teste", Valor = 100m, Disponivel = true }
            };

            _servicoGateway.ObterServicoDisponivelAsync().Returns(Task.FromResult<IEnumerable<Servico>>(servicos));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(1);
            resultado.Should().AllSatisfy(s => s.Disponivel.Should().BeTrue());
        }

        [Fact]
        public async Task ObterServicosDisponiveis_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var handler = new ObterServicosDisponiveisHandler(_servicoGateway, _logGatewayDisponiveis, _udtGateway, _usuarioLogadoGateway);
            _servicoGateway.ObterServicoDisponivelAsync().Returns(Task.FromException<IEnumerable<Servico>>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }

        #endregion
    }
}
