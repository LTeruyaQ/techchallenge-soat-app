using Core.DTOs.Entidades.Veiculo;
using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para testes de handlers de Veículo
    /// </summary>
    public static class VeiculoHandlerFixture
    {
        /// <summary>
        /// Cria um mock de IVeiculoGateway para testes
        /// </summary>
        public static IVeiculoGateway CriarVeiculoGatewayMock()
        {
            return Substitute.For<IVeiculoGateway>();
        }

        /// <summary>
        /// Cria um mock de IRepositorio<VeiculoEntityDto> para testes
        /// </summary>
        public static IRepositorio<VeiculoEntityDto> CriarVeiculoRepositorioMock()
        {
            return Substitute.For<IRepositorio<VeiculoEntityDto>>();
        }

        /// <summary>
        /// Cria um mock de IUnidadeDeTrabalhoGateway para testes
        /// </summary>
        public static IUnidadeDeTrabalhoGateway CriarUnidadeDeTrabalhMock()
        {
            return Substitute.For<IUnidadeDeTrabalhoGateway>();
        }

        /// <summary>
        /// Cria um veículo válido para testes
        /// </summary>
        public static Veiculo CriarVeiculo()
        {
            return new Veiculo
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Placa = "ABC1234",
                Modelo = "Civic",
                Marca = "Honda",
                Ano = "2020",
                Cor = "Preto",
                Anotacoes = "Veículo em bom estado",
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Cria um DTO de cadastro de veículo para testes
        /// </summary>
        public static CadastrarVeiculoUseCaseDto CriarCadastrarVeiculoUseCaseDto()
        {
            return new CadastrarVeiculoUseCaseDto
            {
                ClienteId = Guid.NewGuid(),
                Placa = "ABC1234",
                Modelo = "Civic",
                Marca = "Honda",
                Ano = "2020",
                Cor = "Preto",
                Anotacoes = "Veículo em bom estado"
            };
        }

        /// <summary>
        /// Cria um DTO de atualização de veículo para testes
        /// </summary>
        public static AtualizarVeiculoUseCaseDto CriarAtualizarVeiculoUseCaseDto()
        {
            return new AtualizarVeiculoUseCaseDto
            {
                ClienteId = Guid.NewGuid(),
                Placa = "XYZ9876",
                Modelo = "Corolla",
                Marca = "Toyota",
                Ano = "2021",
                Cor = "Branco",
                Anotacoes = "Veículo atualizado"
            };
        }

        /// <summary>
        /// Cria um gateway real de veículo para testes de integração
        /// </summary>
        public static IVeiculoGateway CriarVeiculoGatewayReal(IRepositorio<VeiculoEntityDto> repositorio)
        {
            return new global::Adapters.Gateways.VeiculoGateway(repositorio);
        }
    }
}
