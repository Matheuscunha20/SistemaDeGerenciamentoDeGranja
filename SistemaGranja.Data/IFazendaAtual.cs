namespace SistemaGranja.Data
{
    // Informa qual é a fazenda de quem está usando o sistema agora.
    // Na aplicação web, vem do login (claim "FazendaId").
    public interface IFazendaAtual
    {
        int? FazendaId { get; }
    }
}
