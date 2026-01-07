using Core.Entidades;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para criação de objetos Veiculo para testes
    /// </summary>
    public static class VeiculoFixture
    {
        /// <summary>
        /// Cria um veículo válido para testes
        /// </summary>
        /// <param name="clienteId">ID do cliente proprietário do veículo</param>
        /// <returns>Uma instância de Veiculo com dados válidos</returns>
        public static Veiculo CriarVeiculoValido(Guid? clienteId = null)
        {
            return new Veiculo
            {
                Placa = "ABC1234",
                Marca = "Toyota",
                Modelo = "Corolla",
                Ano = "2022",
                Cor = "Prata",
                Anotacoes = "Veículo em bom estado",
                ClienteId = clienteId ?? Guid.NewGuid()
            };
        }

        /// <summary>
        /// Cria uma lista de veículos para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de veículos a serem criados</param>
        /// <param name="clienteId">ID do cliente proprietário dos veículos</param>
        /// <returns>Lista de veículos</returns>
        public static List<Veiculo> CriarListaVeiculos(int quantidade = 3, Guid? clienteId = null)
        {
            var veiculos = new List<Veiculo>();
            var clienteIdReal = clienteId ?? Guid.NewGuid();
            
            for (int i = 0; i < quantidade; i++)
            {
                var veiculo = new Veiculo
                {
                    Placa = $"ABC{1000 + i}",
                    Marca = $"Marca {i}",
                    Modelo = $"Modelo {i}",
                    Ano = $"{2020 + i}",
                    Cor = $"Cor {i}",
                    Anotacoes = $"Anotações do veículo {i}",
                    ClienteId = clienteIdReal
                };
                
                veiculos.Add(veiculo);
            }
            
            return veiculos;
        }

        /// <summary>
        /// Cria um veículo com placa específica para testes
        /// </summary>
        /// <param name="placa">Placa do veículo</param>
        /// <param name="clienteId">ID do cliente proprietário do veículo</param>
        /// <returns>Uma instância de Veiculo com a placa especificada</returns>
        public static Veiculo CriarVeiculoComPlaca(string placa, Guid? clienteId = null)
        {
            return new Veiculo
            {
                Placa = placa,
                Marca = "Toyota",
                Modelo = "Corolla",
                Ano = "2022",
                Cor = "Prata",
                Anotacoes = "Veículo em bom estado",
                ClienteId = clienteId ?? Guid.NewGuid()
            };
        }
    }
}
