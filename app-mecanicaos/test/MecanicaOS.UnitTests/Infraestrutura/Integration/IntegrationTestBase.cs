using Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace MecanicaOS.UnitTests.Infraestrutura.Integration
{
    /// <summary>
    /// Classe base para testes de integração com EF InMemory
    /// Importância: Fornece infraestrutura comum para testes de integração
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected MecanicaContexto Context { get; private set; }

        protected IntegrationTestBase()
        {
            var options = new DbContextOptionsBuilder<MecanicaContexto>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new MecanicaContexto(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
