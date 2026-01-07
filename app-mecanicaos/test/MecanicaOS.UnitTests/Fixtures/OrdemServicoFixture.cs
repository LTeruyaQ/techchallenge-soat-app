using Core.Entidades;
using Core.Enumeradores;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para criação de objetos OrdemServico para testes
    /// </summary>
    public static class OrdemServicoFixture
    {
        /// <summary>
        /// Cria uma ordem de serviço válida para testes
        /// </summary>
        /// <param name="clienteId">ID do cliente</param>
        /// <param name="veiculoId">ID do veículo</param>
        /// <param name="servicoId">ID do serviço</param>
        /// <returns>Uma instância de OrdemServico com dados válidos</returns>
        public static OrdemServico CriarOrdemServicoValida(
            Guid? clienteId = null, 
            Guid? veiculoId = null, 
            Guid? servicoId = null)
        {
            return new OrdemServico
            {
                ClienteId = clienteId ?? Guid.NewGuid(),
                VeiculoId = veiculoId ?? Guid.NewGuid(),
                ServicoId = servicoId ?? Guid.NewGuid(),
                Descricao = "Manutenção preventiva",
                Status = StatusOrdemServico.Recebida,
            };
        }

        /// <summary>
        /// Cria uma ordem de serviço com status específico para testes
        /// </summary>
        /// <param name="status">Status da ordem de serviço</param>
        /// <returns>Uma instância de OrdemServico com o status especificado</returns>
        public static OrdemServico CriarOrdemServicoComStatus(StatusOrdemServico status)
        {
            return new OrdemServico
            {
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Descricao = "Manutenção preventiva",
                Status = status,
            };
        }

        /// <summary>
        /// Cria uma lista de ordens de serviço para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de ordens a serem criadas</param>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Lista de ordens de serviço</returns>
        public static List<OrdemServico> CriarListaOrdensServico(int quantidade = 3, Guid? clienteId = null)
        {
            var ordens = new List<OrdemServico>();
            var clienteIdReal = clienteId ?? Guid.NewGuid();
            
            for (int i = 0; i < quantidade; i++)
            {
                var ordem = new OrdemServico
                {
                    ClienteId = clienteIdReal,
                    VeiculoId = Guid.NewGuid(),
                    ServicoId = Guid.NewGuid(),
                    Descricao = $"Descrição da ordem {i}",
                    Status = (StatusOrdemServico)(i % 6), // Alterna entre os status disponíveis
                };
                
                ordens.Add(ordem);
            }
            
            return ordens;
        }

        /// <summary>
        /// Cria uma lista de ordens de serviço com status específico para testes
        /// </summary>
        /// <param name="status">Status das ordens de serviço</param>
        /// <param name="quantidade">Quantidade de ordens a serem criadas</param>
        /// <returns>Lista de ordens de serviço com o status especificado</returns>
        public static List<OrdemServico> CriarListaOrdensServicoComStatus(StatusOrdemServico status, int quantidade = 3)
        {
            var ordens = new List<OrdemServico>();
            
            for (int i = 0; i < quantidade; i++)
            {
                var ordem = new OrdemServico
                {
                    ClienteId = Guid.NewGuid(),
                    VeiculoId = Guid.NewGuid(),
                    ServicoId = Guid.NewGuid(),
                    Descricao = $"Descrição da ordem {i}",
                    Status = status,
                };
                
                ordens.Add(ordem);
            }
            
            return ordens;
        }
    }
}
