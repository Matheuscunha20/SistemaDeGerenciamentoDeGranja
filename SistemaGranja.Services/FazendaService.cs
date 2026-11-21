using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    public class FazendaService : IFazendaService
    {
        private readonly AppDbContext _context;

        public FazendaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Fazenda?> ObterAtualAsync()
        {
            var id = _context.FazendaAtualId;
            return id == null ? null : await _context.Fazendas.FindAsync(id.Value);
        }

        // Retorna null se deu certo, ou a mensagem de erro
        public async Task<string?> RenomearAsync(string novoNome)
        {
            novoNome = (novoNome ?? string.Empty).Trim();
            if (novoNome.Length == 0) return "Informe o nome da fazenda.";
            if (novoNome.Length > 150) return "Use no máximo 150 caracteres.";

            var fazenda = await ObterAtualAsync();
            if (fazenda == null) return "Fazenda não encontrada.";

            fazenda.Nome = novoNome;
            await _context.SaveChangesAsync();
            return null;
        }
    }
}
