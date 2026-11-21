using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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
            // Mesma configuração do login usada pelo sistema: o AddDefaultIdentity define
            // tamanho 128 para as chaves das tabelas AspNetUserLogins e AspNetUserTokens
            var servicos = new ServiceCollection();
            servicos.Configure<IdentityOptions>(o => o.Stores.MaxLengthForKeys = 128);
            using var provedor = servicos.BuildServiceProvider();

            var opcoes = new DbContextOptionsBuilder<SistemaGranja.Data.AppDbContext>()
                .UseMySql("Server=localhost;Database=teste;Uid=teste;Pwd=teste",
                          new MySqlServerVersion(new Version(8, 0, 27)))
                .UseApplicationServiceProvider(provedor)
                .Options;
            using var db = new SistemaGranja.Data.AppDbContext(opcoes);

            // Mesmo cálculo do "dotnet ef migrations add", mas listando o que mudou
            IModel snapshot = db.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
            if (snapshot is IMutableModel modelo) snapshot = modelo.FinalizeModel();
            snapshot = db.GetService<IModelRuntimeInitializer>().Initialize(snapshot);
            var atual = db.GetService<IDesignTimeModel>().Model;

            var diferencas = db.GetService<IMigrationsModelDiffer>()
                .GetDifferences(snapshot.GetRelationalModel(), atual.GetRelationalModel());

            Assert.True(diferencas.Count == 0,
                "Falta migration (ou o snapshot está desatualizado):\n" + string.Join("\n", diferencas.Select(Descrever)));
        }

        private static string Descrever(MigrationOperation operacao)
        {
            string? Ler(string propriedade) =>
                operacao.GetType().GetProperty(propriedade)?.GetValue(operacao) switch
                {
                    null => null,
                    string texto => texto,
                    string[] lista => string.Join(",", lista),
                    var outro => outro.ToString()
                };

            return $"{operacao.GetType().Name} tabela={Ler("Table")} nome={Ler("Name")} colunas={Ler("Columns")}";
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
