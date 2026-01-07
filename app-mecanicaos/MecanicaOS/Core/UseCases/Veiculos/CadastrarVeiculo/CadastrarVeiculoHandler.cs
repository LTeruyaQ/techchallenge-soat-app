using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Veiculos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Veiculos.CadastrarVeiculo
{
    public class CadastrarVeiculoHandler : UseCasesHandlerAbstrato<CadastrarVeiculoHandler>, ICadastrarVeiculoHandler
    {
        private readonly IVeiculoGateway _veiculoGateway;

        public CadastrarVeiculoHandler(
            IVeiculoGateway veiculoGateway,
            ILogGateway<CadastrarVeiculoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _veiculoGateway = veiculoGateway ?? throw new ArgumentNullException(nameof(veiculoGateway));
        }

        public async Task<Veiculo> Handle(CadastrarVeiculoUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, request);

                Veiculo veiculo = new()
                {
                    Ano = request.Ano,
                    Cor = request.Cor,
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    Placa = request.Placa,
                    Anotacoes = request.Anotacoes,
                    ClienteId = request.ClienteId,
                };

                await _veiculoGateway.CadastrarAsync(veiculo);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao cadastrar veículo");

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
