using Core.DTOs.Entidades.Cliente;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestrutura.Dados.Mapeamentos;

public class ClienteMapeamento : IEntityTypeConfiguration<ClienteEntityDto>
{
    public void Configure(EntityTypeBuilder<ClienteEntityDto> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .HasColumnType("varchar(200)");

        builder.Property(c => c.Sexo)
            .HasColumnType("varchar(10)");

        builder.Property(c => c.Documento)
            .HasColumnType("varchar(20)");

        builder.Property(c => c.DataNascimento)
            .HasColumnType("varchar(10)");

        builder.Property(c => c.TipoCliente)
           .HasConversion<string>()
           .HasMaxLength(60);

        // Relacionamento 1:1 com Contato
        builder.HasOne(c => c.Contato)
            .WithOne(c => c.Cliente)
            .HasForeignKey<ContatoEntityDto>(c => c.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento 1:1 com Endereco
        builder.HasOne(c => c.Endereco)
            .WithOne(e => e.Cliente)
            .HasForeignKey<EnderecoEntityDto>(e => e.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento 1:N com Veiculos
        builder.HasMany(c => c.Veiculos)
            .WithOne(v => v.Cliente)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => c.Documento)
            .IsUnique();

        builder.HasIndex(c => c.Nome);
    }
}
