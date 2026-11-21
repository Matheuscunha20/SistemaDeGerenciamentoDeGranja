using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;

namespace SistemaGranja.Tests.Servicos
{
    // Confere decisões do mapeamento do banco que já causaram problema
    public class MapeamentoTests
    {
        [Fact]
        public void Raca_do_animal_e_opcional_no_banco()
        {
            using var db = BancoDeTeste.Criar();
            var propriedade = db.Model.FindEntityType(typeof(Animal))!.FindProperty(nameof(Animal.RacaId))!;

            // Se ficar obrigatória, animais sem raça somem das listas
            Assert.True(propriedade.IsNullable);
        }

        [Fact]
        public async Task Animal_sem_raca_aparece_na_lista()
        {
            using var db = BancoDeTeste.Criar();
            BancoDeTeste.NovoAnimal(db, "SEM-RACA");

            var animais = await new SistemaGranja.Services.AnimalService(db).ObterTodosComRelacionamentosAsync();

            Assert.Equal("SEM-RACA", Assert.Single(animais).TagIdentificacao);
        }

        // As migrations deste projeto foram escritas à mão. Este teste compara o "snapshot"
        // das migrations com o modelo atual: se faltar alguma mudança no banco, ele falha.
        [Fact]
        public void Migrations_estao_em_dia_com_as_entidades()
        {
            var opcoes = new DbContextOptionsBuilder<SistemaGranja.Data.AppDbContext>()
                .UseMySql("Server=localhost;Database=teste;Uid=teste;Pwd=teste",
                          new MySqlServerVersion(new Version(8, 0, 27)))
                .Options;
            using var db = new SistemaGranja.Data.AppDbContext(opcoes);

            Assert.False(db.Database.HasPendingModelChanges(),
                "As entidades mudaram e falta uma migration (ou o snapshot está desatualizado).");
        }

        [Fact]
        public void Especie_da_raca_continua_obrigatoria()
        {
            using var db = BancoDeTeste.Criar();
            var propriedade = db.Model.FindEntityType(typeof(Raca))!.FindProperty(nameof(Raca.EspecieId))!;

            Assert.False(propriedade.IsNullable);
        }
    }
}
