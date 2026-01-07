using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Veiculos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Veiculos.AtualizarVeiculo
{
    public class AtualizarVeiculoHandler : UseCasesHandlerAbstrato<AtualizarVeiculoHandler>, IAtualizarVeiculoHandler
    {
        private readonly IVeiculoGateway _veiculoGateway;

        public AtualizarVeiculoHandler(
            IVeiculoGateway veiculoGateway,
            ILogGateway<AtualizarVeiculoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _veiculoGateway = veiculoGateway ?? throw new ArgumentNullException(nameof(veiculoGateway));
        }

        public async Task<Veiculo> Handle(Guid id, AtualizarVeiculoUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, new { id, request });

                var veiculo = await _veiculoGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Veículo não encontrado");

                if (request.Placa != null) veiculo.Placa = request.Placa;
                if (request.Marca != null) veiculo.Marca = request.Marca;
                if (request.Modelo != null) veiculo.Modelo = request.Modelo;
                if (request.Cor != null) veiculo.Cor = request.Cor;
                if (request.Ano != null) veiculo.Ano = request.Ano;
                if (request.Anotacoes != null) veiculo.Anotacoes = request.Anotacoes;
                if (request.ClienteId.HasValue) veiculo.ClienteId = request.ClienteId.Value;

                veiculo.DataAtualizacao = DateTime.UtcNow;

                await _veiculoGateway.EditarAsync(veiculo);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao atualizar veículo");

                LogFim(metodo, veiculo);

                return veiculo;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
