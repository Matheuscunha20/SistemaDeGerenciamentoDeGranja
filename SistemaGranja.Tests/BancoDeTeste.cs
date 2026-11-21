using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Tests
{
    // Cria um banco em memória novo para cada teste, assim um teste não interfere no outro
    public static class BancoDeTeste
    {
        public static AppDbContext Criar()
        {
            var opcoes = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"agroarca-testes-{Guid.NewGuid()}")
                .Options;
            return new AppDbContext(opcoes);
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
