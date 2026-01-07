using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.AtualizarVeiculo;
using Core.UseCases.Veiculos.CadastrarVeiculo;
using Core.UseCases.Veiculos.DeletarVeiculo;
using Core.UseCases.Veiculos.ObterTodosVeiculos;
using Core.UseCases.Veiculos.ObterVeiculo;
using Core.UseCases.Veiculos.ObterVeiculoPorCliente;
using Core.UseCases.Veiculos.ObterVeiculoPorPlaca;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos
{
    /// <summary>
    /// Testes para handlers de Veículos
    /// ROI ALTO: Testa ~500 linhas de código crítico para gestão de veículos.
    /// </summary>
    public class VeiculosHandlersTests
    {
        private readonly IVeiculoGateway _veiculoGateway;
        private readonly ILogGateway<CadastrarVeiculoHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public VeiculosHandlersTests()
        {
            _veiculoGateway = Substitute.For<IVeiculoGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarVeiculoHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        [Fact]
        public async Task CadastrarVeiculo_ComDadosValidos_DeveCadastrarComSucesso()
        {
            var handler = new CadastrarVeiculoHandler(_veiculoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarVeiculoUseCaseDto
            {
                Placa = "ABC1234",
                Modelo = "Civic",
                Marca = "Honda",
                Ano = "2020",
                ClienteId = Guid.NewGuid()
            };

            _veiculoGateway.ObterVeiculoPorPlacaAsync(request.Placa).Returns(Task.FromResult<IEnumerable<Veiculo>>(new List<Veiculo>()));
            _veiculoGateway.CadastrarAsync(Arg.Any<Veiculo>()).Returns(Task.FromResult(new Veiculo()));

            var resultado = await handler.Handle(request);

            resultado.Should().NotBeNull();
            await _veiculoGateway.Received(1).CadastrarAsync(Arg.Any<Veiculo>());
        }

        [Fact]
        public async Task CadastrarVeiculo_ComFalhaNoCommit_DeveLancarExcecao()
        {
            var handler = new CadastrarVeiculoHandler(_veiculoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarVeiculoUseCaseDto { Placa = "ABC1234", Modelo = "Civic", Marca = "Honda", Ano = "2020", ClienteId = Guid.NewGuid() };

            _veiculoGateway.ObterVeiculoPorPlacaAsync(request.Placa).Returns(Task.FromResult<IEnumerable<Veiculo>>(new List<Veiculo>()));
            _udtGateway.Commit().Returns(Task.FromResult(false));

            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar veículo");
        }

        [Fact]
        public async Task DeletarVeiculo_ComVeiculoExistente_DeveDeletarComSucesso()
        {
            var logGatewayDeletar = Substitute.For<ILogGateway<DeletarVeiculoHandler>>();
            var handler = new DeletarVeiculoHandler(_veiculoGateway, logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);
            var veiculoId = Guid.NewGuid();

            _veiculoGateway.ObterPorIdAsync(veiculoId).Returns(Task.FromResult<Veiculo?>(new Veiculo { Id = veiculoId }));

            var resultado = await handler.Handle(veiculoId);

            resultado.Should().BeTrue();
            await _veiculoGateway.Received(1).DeletarAsync(Arg.Any<Veiculo>());
        }
    }
}
