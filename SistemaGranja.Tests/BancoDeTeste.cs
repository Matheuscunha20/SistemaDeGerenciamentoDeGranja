using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Tests
{
    // Cria um banco em memória novo para cada teste, assim um teste não interfere no outro
    public static class BancoDeTeste
    {
        // Banco novo com uma fazenda já criada e selecionada
        public static AppDbContext Criar() => CriarParaFazenda(NovoNomeDeBanco(), "Fazenda de teste");

        public static string NovoNomeDeBanco() => $"agroarca-testes-{Guid.NewGuid()}";

        // Abre o banco "nomeDoBanco" como usuário da fazenda "nomeDaFazenda" (cria a fazenda se não existir).
        // Duas chamadas com o mesmo banco e fazendas diferentes simulam dois clientes no mesmo servidor.
        public static AppDbContext CriarParaFazenda(string nomeDoBanco, string nomeDaFazenda)
        {
            var opcoes = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(nomeDoBanco)
                .Options;
            var db = new AppDbContext(opcoes);

            var fazenda = db.Fazendas.FirstOrDefault(f => f.Nome == nomeDaFazenda);
            if (fazenda == null)
            {
                fazenda = new Fazenda { Nome = nomeDaFazenda };
                db.Fazendas.Add(fazenda);
                db.SaveChanges();
            }

            db.DefinirFazenda(fazenda.Id);
            return db;
        }

        public static Galpao NovoGalpao(AppDbContext db, string nome = "Galpão 1", int capacidade = 10)
        {
            var galpao = new Galpao { NomeOuNumero = nome, TipoInstalacao = "Engorda", CapacidadeMaxima = capacidade };
            db.Galpoes.Add(galpao);
            db.SaveChanges();
            return galpao;
        }

        public static Animal NovoAnimal(AppDbContext db, string tag, int? galpaoId = null, int? loteId = null, string estado = "Saudável")
        {
            var animal = new Animal
            {
                TagIdentificacao = tag,
                Sexo = "Fêmea",
                StatusSaude = estado,
                DataNascimento = DateTime.Today.AddYears(-1),
                GalpaoId = galpaoId,
                LoteId = loteId
            };
            db.Animais.Add(animal);
            db.SaveChanges();
            return animal;
        }

        public static TipoAlimento NovoAlimento(AppDbContext db, string nome = "Milho", double? estoqueMinimo = null)
        {
            var alimento = new TipoAlimento { Nome = nome, UnidadeMedidaPadrao = "Kg", EstoqueMinimo = estoqueMinimo };
            db.TiposAlimento.Add(alimento);
            db.SaveChanges();
            return alimento;
        }
    }
}
