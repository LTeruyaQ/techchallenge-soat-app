using Core.DTOs.Entidades.Veiculo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestrutura.Dados.Mapeamentos;

public class VeiculoMapeamento : IEntityTypeConfiguration<VeiculoEntityDto>
{
    public void Configure(EntityTypeBuilder<VeiculoEntityDto> builder)
    {
        builder.ToTable("Veiculos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Placa)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(v => v.Marca)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Modelo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Cor)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(v => v.Ano)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(v => v.Anotacoes)
            .HasMaxLength(500);

        builder.Property(v => v.DataCadastro)
            .IsRequired();

        builder.Property(v => v.DataAtualizacao);


        builder.Property(v => v.ClienteId);

        builder.HasOne(v => v.Cliente)
            .WithMany(c => c.Veiculos)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
