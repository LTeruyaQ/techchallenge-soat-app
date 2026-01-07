namespace Core.DTOs.Entidades.Autenticacao
{
    public abstract class EntityDto
    {
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool Ativo { get; set; }
    }
}
