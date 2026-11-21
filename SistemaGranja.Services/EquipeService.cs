using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    public class EquipeService : IEquipeService
    {
        private readonly AppDbContext _context;

        public EquipeService(AppDbContext context)
        {
            _context = context;
        }

        // Acesso removido = bloqueio "para sempre" no login.
        // (Não usa DateTimeOffset.MaxValue porque o MySQL não guarda frações além de microssegundos.)
        public static readonly DateTimeOffset BloqueioPermanente = new(9999, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static bool AcessoRemovido(ApplicationUser usuario) =>
            usuario.LockoutEnd.HasValue && usuario.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(100);

        public async Task<List<ApplicationUser>> ObterMembrosAsync()
        {
            var fazendaId = _context.FazendaAtualId;
            return await _context.Users
                .Where(u => u.FazendaId != null && u.FazendaId == fazendaId)
                .OrderBy(u => u.Perfil)
                .ThenBy(u => u.Email)
                .ToListAsync();
        }

        public async Task<List<Convite>> ObterConvitesPendentesAsync()
        {
            var agora = DateTime.UtcNow;
            return await _context.Convites
                .Where(c => c.AceitoEm == null && c.ExpiraEm > agora)
                .OrderByDescending(c => c.CriadoEm)
                .ToListAsync();
        }

        public async Task<(Convite? Convite, string? Erro)> CriarConviteAsync(string email, PerfilUsuario perfil)
        {
            if (!Enum.IsDefined(perfil))
                return (null, "Perfil inválido.");

            email = (email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email))
                return (null, "Informe o e-mail da pessoa.");

            var emailNormalizado = email.ToUpperInvariant();
            if (await _context.Users.AnyAsync(u => u.NormalizedEmail == emailNormalizado))
                return (null, "Este e-mail já tem uma conta no AgroArca. Por enquanto, cada conta participa de uma única fazenda.");

            // Se já havia convite pendente para este e-mail, o antigo deixa de valer
            var agora = DateTime.UtcNow;
            var antigos = await _context.Convites
                .Where(c => c.AceitoEm == null && c.ExpiraEm > agora && c.Email.ToUpper() == emailNormalizado)
                .ToListAsync();
            foreach (var antigo in antigos)
                antigo.ExpiraEm = agora;

            var convite = new Convite
            {
                Email = email,
                Perfil = perfil,
                Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32))
            };
            _context.Convites.Add(convite);
            await _context.SaveChangesAsync();
            return (convite, null);
        }

        public async Task<bool> CancelarConviteAsync(int conviteId)
        {
            var convite = await _context.Convites.FindAsync(conviteId);
            if (convite == null || convite.AceitoEm != null) return false;

            _context.Convites.Remove(convite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Convite?> ObterConviteValidoAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            // Quem abre o link ainda não está logado: busca em todas as fazendas
            var convite = await _context.Convites
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Token == token);

            return convite != null && convite.Pendente ? convite : null;
        }

        public async Task MarcarConviteAceitoAsync(Convite convite)
        {
            _context.DefinirFazenda(convite.FazendaId);
            convite.AceitoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<string?> AlterarPerfilAsync(string usuarioId, PerfilUsuario novoPerfil, string usuarioLogadoId)
        {
            if (!Enum.IsDefined(novoPerfil)) return "Perfil inválido.";
            if (usuarioId == usuarioLogadoId) return "Você não pode alterar o seu próprio perfil.";

            var usuario = await ObterMembroAsync(usuarioId);
            if (usuario == null) return "Pessoa não encontrada na equipe.";
            if (usuario.Perfil == novoPerfil) return null;

            if (usuario.Perfil == PerfilUsuario.Dono && !await ExisteOutroDonoAtivoAsync(usuario.Id))
                return "A fazenda precisa ter pelo menos um dono.";

            usuario.Perfil = novoPerfil;
            usuario.ConcurrencyStamp = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> RemoverAcessoAsync(string usuarioId, string usuarioLogadoId)
        {
            if (usuarioId == usuarioLogadoId) return "Você não pode remover o seu próprio acesso.";

            var usuario = await ObterMembroAsync(usuarioId);
            if (usuario == null) return "Pessoa não encontrada na equipe.";

            if (usuario.Perfil == PerfilUsuario.Dono && !await ExisteOutroDonoAtivoAsync(usuario.Id))
                return "A fazenda precisa ter pelo menos um dono.";

            usuario.LockoutEnabled = true;
            usuario.LockoutEnd = BloqueioPermanente;
            // Trocar o carimbo de segurança derruba a sessão que estiver aberta
            usuario.SecurityStamp = Guid.NewGuid().ToString();
            usuario.ConcurrencyStamp = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> ReativarAcessoAsync(string usuarioId)
        {
            var usuario = await ObterMembroAsync(usuarioId);
            if (usuario == null) return "Pessoa não encontrada na equipe.";

            usuario.LockoutEnd = null;
            usuario.AccessFailedCount = 0;
            usuario.ConcurrencyStamp = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();
            return null;
        }

        private async Task<ApplicationUser?> ObterMembroAsync(string usuarioId)
        {
            var fazendaId = _context.FazendaAtualId;
            if (fazendaId == null) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == usuarioId && u.FazendaId == fazendaId);
        }

        private async Task<bool> ExisteOutroDonoAtivoAsync(string ignorarUsuarioId)
        {
            var fazendaId = _context.FazendaAtualId;
            var limite = DateTimeOffset.UtcNow;
            var donos = await _context.Users
                .Where(u => u.FazendaId == fazendaId && u.Perfil == PerfilUsuario.Dono && u.Id != ignorarUsuarioId)
                .ToListAsync();
            return donos.Any(u => u.LockoutEnd == null || u.LockoutEnd <= limite);
        }
    }
}
