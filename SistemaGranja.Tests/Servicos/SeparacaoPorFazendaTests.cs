using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    // Garante que um cliente (fazenda) nunca vê nem altera os dados de outro
    public class SeparacaoPorFazendaTests
    {
        [Fact]
        public async Task Cada_fazenda_ve_so_os_proprios_animais()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            using (var fazendaA = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A"))
                BancoDeTeste.NovoAnimal(fazendaA, "DA-FAZENDA-A");
            using (var fazendaB = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B"))
                BancoDeTeste.NovoAnimal(fazendaB, "DA-FAZENDA-B");

            using var consultaA = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A");
            var animais = await new AnimalService(consultaA).ObterTodosComRelacionamentosAsync();

            Assert.Equal("DA-FAZENDA-A", Assert.Single(animais).TagIdentificacao);
        }

        [Fact]
        public void Registro_novo_recebe_a_fazenda_automaticamente()
        {
            using var db = BancoDeTeste.Criar();

            var animal = BancoDeTeste.NovoAnimal(db, "001");

            Assert.Equal(db.FazendaAtualId, animal.FazendaId);
        }

        [Fact]
        public void Buscar_pelo_id_nao_encontra_registro_de_outra_fazenda()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            int idDaFazendaB;
            using (var fazendaB = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B"))
                idDaFazendaB = BancoDeTeste.NovoGalpao(fazendaB).Id;

            using var fazendaA = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A");

            Assert.Null(fazendaA.Galpoes.Find(idDaFazendaB));
        }

        [Fact]
        public void Nao_deixa_alterar_registro_de_outra_fazenda()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            int idDaFazendaB;
            using (var fazendaB = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B"))
                idDaFazendaB = BancoDeTeste.NovoGalpao(fazendaB, "Galpão da B").Id;

            // Simula alguém mudando o Id no formulário de edição
            using var fazendaA = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A");
            fazendaA.Galpoes.Update(new Galpao { Id = idDaFazendaB, NomeOuNumero = "Invadido", TipoInstalacao = "Engorda", CapacidadeMaxima = 1 });

            Assert.Throws<UnauthorizedAccessException>(() => fazendaA.SaveChanges());
        }

        [Fact]
        public void Nao_deixa_ligar_animal_a_galpao_de_outra_fazenda()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            int galpaoDaFazendaB;
            using (var fazendaB = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B"))
                galpaoDaFazendaB = BancoDeTeste.NovoGalpao(fazendaB).Id;

            using var fazendaA = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A");

            Assert.Throws<UnauthorizedAccessException>(() => BancoDeTeste.NovoAnimal(fazendaA, "001", galpaoId: galpaoDaFazendaB));
        }

        [Fact]
        public void Sem_fazenda_selecionada_nao_salva()
        {
            var opcoes = new DbContextOptionsBuilder<SistemaGranja.Data.AppDbContext>()
                .UseInMemoryDatabase(BancoDeTeste.NovoNomeDeBanco())
                .Options;
            using var db = new SistemaGranja.Data.AppDbContext(opcoes);
            db.Galpoes.Add(new Galpao { NomeOuNumero = "G", TipoInstalacao = "Engorda", CapacidadeMaxima = 1 });

            Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        }

        [Fact]
        public async Task Mesmo_nome_de_especie_pode_existir_em_fazendas_diferentes()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            using (var fazendaA = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A"))
                await SistemaGranja.Data.SemeadorDeDados.SemearFazendaNovaAsync(fazendaA);

            using var fazendaB = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B");
            await SistemaGranja.Data.SemeadorDeDados.SemearFazendaNovaAsync(fazendaB);

            // A fazenda B ganhou o próprio catálogo, igual ao da A
            Assert.Contains(fazendaB.Especies, e => e.Nome == "Bovino");
            Assert.NotEmpty(fazendaB.TiposVacina);
            Assert.All(fazendaB.Especies, e => Assert.Equal(fazendaB.FazendaAtualId, e.FazendaId));
        }
    }
}
