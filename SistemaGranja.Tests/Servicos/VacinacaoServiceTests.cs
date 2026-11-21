using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class VacinacaoServiceTests
    {
        private static TipoVacina NovaVacina(Data.AppDbContext db, string nome, int? intervalo)
        {
            var vacina = new TipoVacina { NomeVacina = nome, DoencaAlvo = nome, IntervaloReforcoDias = intervalo };
            db.TiposVacina.Add(vacina);
            db.SaveChanges();
            return vacina;
        }

        [Fact]
        public async Task Proxima_dose_soma_o_intervalo_da_vacina()
        {
            using var db = BancoDeTeste.Criar();
            var raiva = NovaVacina(db, "Raiva", 365);

            var proxima = await new VacinacaoService(db).CalcularProximaDoseAsync(raiva.Id, new DateTime(2026, 1, 10));

            Assert.Equal(new DateTime(2027, 1, 10), proxima);
        }

        [Fact]
        public async Task Vacina_de_dose_unica_nao_tem_proxima_dose()
        {
            using var db = BancoDeTeste.Criar();
            var brucelose = NovaVacina(db, "Brucelose", null);

            Assert.Null(await new VacinacaoService(db).CalcularProximaDoseAsync(brucelose.Id, DateTime.Today));
        }

        [Fact]
        public async Task Agenda_mostra_dose_atrasada_e_some_quando_o_reforco_e_aplicado()
        {
            using var db = BancoDeTeste.Criar();
            var raiva = NovaVacina(db, "Raiva", 365);
            var animal = BancoDeTeste.NovoAnimal(db, "001");
            var servico = new VacinacaoService(db);

            // Vacinado há 2 anos: o reforço venceu há 1 ano
            await servico.AdicionarAsync(new RegistroVacinacao
            {
                AnimalId = animal.Id,
                TipoVacinaId = raiva.Id,
                DataVacinacao = DateTime.Today.AddYears(-2),
                DataProximaDoseRecomendada = DateTime.Today.AddYears(-1)
            });

            var atrasada = Assert.Single(await servico.ObterProximasDosesAsync());
            Assert.True(atrasada.DataProximaDoseRecomendada < DateTime.Today);

            // Reforço aplicado hoje, próxima dose daqui a 1 ano
            await servico.AdicionarAsync(new RegistroVacinacao
            {
                AnimalId = animal.Id,
                TipoVacinaId = raiva.Id,
                DataVacinacao = DateTime.Today,
                DataProximaDoseRecomendada = DateTime.Today.AddYears(1)
            });

            Assert.Empty(await servico.ObterProximasDosesAsync(30));
        }

        [Fact]
        public async Task Vacinacao_em_grupo_cria_um_registro_por_animal()
        {
            using var db = BancoDeTeste.Criar();
            var raiva = NovaVacina(db, "Raiva", 365);
            var a1 = BancoDeTeste.NovoAnimal(db, "001");
            var a2 = BancoDeTeste.NovoAnimal(db, "002");

            var modelo = new RegistroVacinacao { TipoVacinaId = raiva.Id, DataVacinacao = DateTime.Today, DoseAplicada = "2 ml" };
            var quantidade = await new VacinacaoService(db).AdicionarEmGrupoAsync(modelo, new[] { a1.Id, a2.Id, a2.Id, 9999 });

            Assert.Equal(2, quantidade); // repetidos e inexistentes são ignorados
            Assert.Equal(2, db.RegistrosVacinacao.Count());
            Assert.All(db.RegistrosVacinacao, r => Assert.Equal("2 ml", r.DoseAplicada));
        }
    }
}
