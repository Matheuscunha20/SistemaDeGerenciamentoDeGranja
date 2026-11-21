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

        // Vacinas comuns no Brasil. Os intervalos são apenas sugestões iniciais:
        // o produtor deve ajustar conforme a bula e o médico-veterinário.
        // (nome, doença, intervalo de reforço em dias; null = dose única ou conforme protocolo)
        private static readonly (string Nome, string Doenca, int? IntervaloDias)[] Vacinas =
        {
            ("Brucelose (B19)", "Brucelose - dose única em fêmeas bovinas de 3 a 8 meses", null),
            ("Raiva", "Raiva", 365),
            ("Clostridioses (polivalente)", "Carbúnculo sintomático, gangrena gasosa, enterotoxemia e outras clostridioses", 365),
            ("Leptospirose", "Leptospirose", 180),
            ("IBR/BVD", "Rinotraqueíte infecciosa bovina e diarreia viral bovina", 365),
            ("Newcastle", "Doença de Newcastle (aves)", null),
            ("Gumboro", "Doença de Gumboro - bursite infecciosa (aves)", null),
            ("Marek", "Doença de Marek (aves)", null),
            ("Circovírus suíno (PCV2)", "Circovirose suína", null),
            ("Micoplasma suíno", "Pneumonia enzoótica suína", null),
            ("Erisipela suína", "Erisipela suína", 180),
            ("Influenza equina", "Influenza equina", 365),
            ("Tétano", "Tétano", 365),
        };

        // Dados iniciais de uma fazenda recém-criada.
        // Antes de chamar, use db.DefinirFazenda(id) com a fazenda nova.
        public static async Task SemearFazendaNovaAsync(AppDbContext db)
        {
            await SemearEspeciesERacasAsync(db);
            await SemearVacinasAsync(db);
        }

        // Só cadastra as vacinas se a fazenda ainda não tiver nenhuma,
        // para não recriar uma vacina que o usuário excluiu de propósito.
        public static async Task SemearVacinasAsync(AppDbContext db)
        {
            if (await db.TiposVacina.AnyAsync()) return;

            foreach (var (nome, doenca, intervalo) in Vacinas)
            {
                db.TiposVacina.Add(new TipoVacina
                {
                    NomeVacina = nome,
                    DoencaAlvo = doenca,
                    IntervaloReforcoDias = intervalo,
                    DosagemPadraoRecomendada = "Conforme bula",
                    ViaAdministracaoComum = "Conforme bula"
                });
            }

            await db.SaveChangesAsync();
        }

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
