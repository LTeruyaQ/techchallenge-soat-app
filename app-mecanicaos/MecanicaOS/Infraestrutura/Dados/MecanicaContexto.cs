
using Core.DTOs.Entidades.Cliente;
using Core.DTOs.Entidades.Estoque;
using Core.DTOs.Entidades.OrdemServicos;
using Core.DTOs.Entidades.Servico;
using Core.DTOs.Entidades.Usuarios;
using Core.DTOs.Entidades.Veiculo;
using Infraestrutura.Dados.Mapeamentos;
using Microsoft.EntityFrameworkCore;

namespace Infraestrutura.Dados;

public class MecanicaContexto : DbContext
{
    public MecanicaContexto(DbContextOptions<MecanicaContexto> options) : base(options) { }

    public DbSet<ServicoEntityDto> Servicos { get; set; }
    public DbSet<EstoqueEntityDto> Estoques { get; set; }
    public DbSet<VeiculoEntityDto> Veiculos { get; set; }
    public DbSet<ClienteEntityDto> Clientes { get; set; }
    public DbSet<EnderecoEntityDto> Enderecos { get; set; }
    public DbSet<ContatoEntityDto> Contatos { get; set; }
    public DbSet<UsuarioEntityDto> Usuarios { get; set; }
    public DbSet<AlertaEstoqueEntityDto> AlertasEstoque { get; set; }
    public DbSet<OrdemServicoEntityDto> OrdensSevico { get; set; }
    public DbSet<InsumoOSEntityDto> InsumosOrdemServico { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("DataCadastro") != null))
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("DataCadastro").CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("DataCadastro").IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EstoqueMapeamento());
        modelBuilder.ApplyConfiguration(new VeiculoMapeamento());
        modelBuilder.ApplyConfiguration(new ClienteMapeamento());
        modelBuilder.ApplyConfiguration(new ContatoMapeamento());
        modelBuilder.ApplyConfiguration(new UsuarioMapeamento());
        modelBuilder.ApplyConfiguration(new OrdemServicoMapeamento());

        base.OnModelCreating(modelBuilder);
    }
}
