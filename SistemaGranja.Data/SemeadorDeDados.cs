using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;

namespace SistemaGranja.Data
{
    // Cadastra as espécies e raças mais comuns no Brasil.
    // Roda toda vez que o sistema inicia, mas só insere o que ainda não existe,
    // então nunca duplica dados nem apaga o que o usuário cadastrou.
    public static class SemeadorDeDados
    {
        private static readonly Dictionary<string, string[]> EspeciesERacas = new()
        {
            ["Bovino"] = new[]
            {
                "Nelore", "Angus", "Brahman", "Brangus", "Gir", "Girolando", "Guzerá",
                "Hereford", "Holandês", "Jersey", "Senepol", "Simental", "Tabapuã", "Mestiço", "Outra"
            },
            ["Suíno"] = new[]
            {
                "Large White", "Landrace", "Duroc", "Pietrain", "Hampshire", "Moura", "Mestiço", "Outra"
            },
            ["Ave"] = new[]
            {
                "Cobb", "Ross", "Hubbard", "Embrapa 051", "Isa Brown", "Hy-Line", "Caipira", "Outra"
            },
            ["Ovino"] = new[]
            {
                "Santa Inês", "Dorper", "Texel", "Suffolk", "Morada Nova", "Mestiço", "Outra"
            },
            ["Caprino"] = new[]
            {
                "Boer", "Saanen", "Anglo-Nubiano", "Parda Alpina", "Toggenburg", "Mestiço", "Outra"
            },
            ["Equino"] = new[]
            {
                "Mangalarga Marchador", "Quarto de Milha", "Crioulo", "Campolina", "Puro-Sangue Inglês", "Mestiço", "Outra"
            },
            ["Bubalino"] = new[]
            {
                "Murrah", "Mediterrâneo", "Jafarabadi", "Carabao", "Mestiço", "Outra"
            },
        };

        public static async Task SemearEspeciesERacasAsync(AppDbContext db)
        {
            var especiesExistentes = await db.Especies
                .Include(e => e.Racas)
                .ToDictionaryAsync(e => e.Nome);

            foreach (var (nomeEspecie, racas) in EspeciesERacas)
            {
                if (!especiesExistentes.TryGetValue(nomeEspecie, out var especie))
                {
                    especie = new Especie { Nome = nomeEspecie };
                    db.Especies.Add(especie);
                }

                var racasExistentes = especie.Racas.Select(r => r.Nome).ToHashSet();
                foreach (var nomeRaca in racas.Where(r => !racasExistentes.Contains(r)))
                {
                    especie.Racas.Add(new Raca { Nome = nomeRaca });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
