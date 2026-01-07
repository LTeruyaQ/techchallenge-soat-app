using Core.DTOs.Entidades.Cliente;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestrutura.Dados.Mapeamentos;

public class ContatoMapeamento : IEntityTypeConfiguration<ContatoEntityDto>
{
    public void Configure(EntityTypeBuilder<ContatoEntityDto> builder)
    {
        builder.Property(e => e.DataAtualizacao)
            .IsRequired(false);
    }
}