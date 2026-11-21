using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class GalpaoServiceTests
    {
        [Fact]
        public async Task Conta_os_animais_de_cada_galpao()
        {
            using var db = BancoDeTeste.Criar();
            var g1 = BancoDeTeste.NovoGalpao(db, "G1");
            var g2 = BancoDeTeste.NovoGalpao(db, "G2");
            BancoDeTeste.NovoAnimal(db, "001", g1.Id);
            BancoDeTeste.NovoAnimal(db, "002", g1.Id);
            BancoDeTeste.NovoAnimal(db, "003", g2.Id);
            BancoDeTeste.NovoAnimal(db, "004"); // sem galpão

            var ocupacao = await new GalpaoService(db).ObterOcupacaoAsync();

            Assert.Equal(2, ocupacao[g1.Id]);
            Assert.Equal(1, ocupacao[g2.Id]);
        }

        [Fact]
        public async Task Contagem_pode_ignorar_o_animal_que_esta_sendo_editado()
        {
            using var db = BancoDeTeste.Criar();
            var galpao = BancoDeTeste.NovoGalpao(db);
            var animal = BancoDeTeste.NovoAnimal(db, "001", galpao.Id);
            BancoDeTeste.NovoAnimal(db, "002", galpao.Id);

            var servico = new GalpaoService(db);

            Assert.Equal(2, await servico.ContarAnimaisAsync(galpao.Id));
            Assert.Equal(1, await servico.ContarAnimaisAsync(galpao.Id, ignorarAnimalId: animal.Id));
        }

        [Fact]
        public async Task Nao_exclui_galpao_com_animais()
        {
            using var db = BancoDeTeste.Criar();
            var galpao = BancoDeTeste.NovoGalpao(db);
            BancoDeTeste.NovoAnimal(db, "001", galpao.Id);

            var removeu = await new GalpaoService(db).RemoverAsync(galpao.Id);

            Assert.False(removeu);
            Assert.Single(db.Galpoes);
        }

        [Fact]
        public async Task Exclui_galpao_vazio()
        {
            using var db = BancoDeTeste.Criar();
            var galpao = BancoDeTeste.NovoGalpao(db);

            var removeu = await new GalpaoService(db).RemoverAsync(galpao.Id);

            Assert.True(removeu);
            Assert.Empty(db.Galpoes);
        }

        [Fact]
        public async Task Nome_de_galpao_repetido_ignora_maiusculas()
        {
            using var db = BancoDeTeste.Criar();
            BancoDeTeste.NovoGalpao(db, "Galpão Norte");

            Assert.True(await new GalpaoService(db).ExisteNomeAsync("  galpão norte "));
        }
    }
}
