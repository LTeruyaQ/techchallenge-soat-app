using Core.Entidades;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para criação de objetos Servico para testes
    /// </summary>
    public static class ServicoFixture
    {
        /// <summary>
        /// Cria um serviço válido para testes
        /// </summary>
        /// <returns>Uma instância de Servico com dados válidos</returns>
        public static Servico CriarServicoValido()
        {
            return new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor e filtro",
                Valor = 120.00m,
                Disponivel = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true
            };
        }

        /// <summary>
        /// Cria um serviço indisponível para testes
        /// </summary>
        /// <returns>Uma instância de Servico com disponibilidade false</returns>
        public static Servico CriarServicoIndisponivel()
        {
            return new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Alinhamento e Balanceamento",
                Descricao = "Alinhamento e balanceamento das rodas",
                Valor = 80.00m,
                Disponivel = false,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true
            };
        }

        /// <summary>
        /// Cria uma lista de serviços para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de serviços a serem criados</param>
        /// <returns>Lista de serviços</returns>
        public static List<Servico> CriarListaServicos(int quantidade = 3)
        {
            var servicos = new List<Servico>();
            
            for (int i = 0; i < quantidade; i++)
            {
                var servico = new Servico
                {
                    Id = Guid.NewGuid(),
                    Nome = $"Serviço {i}",
                    Descricao = $"Descrição do serviço {i}",
                    Valor = 50.00m * (i + 1),
                    Disponivel = i % 2 == 0, // Alterna entre disponível e indisponível
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now,
                    Ativo = true
                };
                
                servicos.Add(servico);
            }
            
            return servicos;
        }

        /// <summary>
        /// Cria uma lista de serviços disponíveis para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de serviços a serem criados</param>
        /// <returns>Lista de serviços disponíveis</returns>
        public static List<Servico> CriarListaServicosDisponiveis(int quantidade = 3)
        {
            var servicos = new List<Servico>();
            
            for (int i = 0; i < quantidade; i++)
            {
                var servico = new Servico
                {
                    Id = Guid.NewGuid(),
                    Nome = $"Serviço Disponível {i}",
                    Descricao = $"Descrição do serviço disponível {i}",
                    Valor = 50.00m * (i + 1),
                    Disponivel = true,
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now,
                    Ativo = true
                };
                
                servicos.Add(servico);
            }
            
            return servicos;
        }
    }
}
