using Core.DTOs.Entidades.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestrutura.Dados.Mapeamentos;

public partial class EstoqueMapeamento : IEntityTypeConfiguration<EstoqueEntityDto>
{
    public void Configure(EntityTypeBuilder<EstoqueEntityDto> builder)
    {
        builder.Property(e => e.Insumo).HasMaxLength(100);

        builder.Property(e => e.Descricao).HasMaxLength(500);
    }
}
