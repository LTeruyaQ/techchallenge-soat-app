using Core.DTOs.Entidades.OrdemServicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestrutura.Dados.Mapeamentos;

public class OrdemServicoMapeamento : IEntityTypeConfiguration<OrdemServicoEntityDto>
{
    public void Configure(EntityTypeBuilder<OrdemServicoEntityDto> builder)
    {
        builder.Property(o => o.Descricao).HasMaxLength(1000);

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(80);
    }
}