using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class LoteServiceTests
    {
        private static Lote NovoLote(Data.AppDbContext db, string codigo, bool ativo = true)
        {
            var lote = new Lote { CodigoLote = codigo, Ativo = ativo };
            db.Lotes.Add(lote);
            db.SaveChanges();
            return lote;
        }

        [Fact]
        public async Task Adiciona_varios_animais_e_move_de_outro_lote()
        {
            using var db = BancoDeTeste.Criar();
            var loteAntigo = NovoLote(db, "ANTIGO");
            var loteNovo = NovoLote(db, "NOVO");
            var a1 = BancoDeTeste.NovoAnimal(db, "001", loteId: loteAntigo.Id);
            var a2 = BancoDeTeste.NovoAnimal(db, "002");

            var quantidade = await new LoteService(db).AdicionarAnimaisAsync(loteNovo.Id, new[] { a1.Id, a2.Id, 9999 });

            Assert.Equal(2, quantidade); // o Id 9999 não existe e é ignorado
            Assert.All(db.Animais, a => Assert.Equal(loteNovo.Id, a.LoteId));
        }

        [Fact]
        public async Task Retirar_do_lote_nao_exclui_o_animal()
        {
            using var db = BancoDeTeste.Criar();
            var lote = NovoLote(db, "L1");
            var animal = BancoDeTeste.NovoAnimal(db, "001", loteId: lote.Id);

            await new LoteService(db).RemoverAnimalDoLoteAsync(lote.Id, animal.Id);

            Assert.Null(db.Animais.Single().LoteId);
        }

        [Fact]
        public async Task Lista_de_ativos_nao_traz_lote_encerrado()
        {
            using var db = BancoDeTeste.Criar();
            NovoLote(db, "ATIVO");
            NovoLote(db, "ENCERRADO", ativo: false);

            var ativos = await new LoteService(db).ObterAtivosAsync();

            Assert.Equal("ATIVO", Assert.Single(ativos).CodigoLote);
        }

        [Fact]
        public async Task Codigo_do_lote_e_salvo_em_maiusculas()
        {
            using var db = BancoDeTeste.Criar();
            var lote = new Lote { CodigoLote = "  bezerras-maio " };

            await new LoteService(db).AdicionarAsync(lote);

            Assert.Equal("BEZERRAS-MAIO", lote.CodigoLote);
        }

        [Fact]
        public async Task Nao_exclui_lote_com_animais()
        {
            using var db = BancoDeTeste.Criar();
            var lote = NovoLote(db, "L1");
            BancoDeTeste.NovoAnimal(db, "001", loteId: lote.Id);

            Assert.False(await new LoteService(db).RemoverAsync(lote.Id));
        }
    }
}
