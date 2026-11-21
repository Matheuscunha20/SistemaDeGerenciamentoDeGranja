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

        [Fact]
        public void Especie_da_raca_continua_obrigatoria()
        {
            using var db = BancoDeTeste.Criar();
            var propriedade = db.Model.FindEntityType(typeof(Raca))!.FindProperty(nameof(Raca.EspecieId))!;

            Assert.False(propriedade.IsNullable);
        }
    }
}
