namespace SistemaGranja.Domain
{
    // Marca as entidades que pertencem a uma fazenda.
    // O AppDbContext usa esta interface para filtrar as consultas pela fazenda de quem está logado
    // e para preencher a fazenda automaticamente ao salvar.
    public interface IPertenceAFazenda
    {
        int FazendaId { get; set; }
    }
}
