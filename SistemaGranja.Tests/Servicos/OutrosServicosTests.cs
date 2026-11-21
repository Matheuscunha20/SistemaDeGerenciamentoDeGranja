using SistemaGranja.Data;
using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class SaudeServiceTests
    {
        [Fact]
        public async Task Registrar_ocorrencia_pode_mudar_o_estado_do_animal()
        {
            using var db = BancoDeTeste.Criar();
            var animal = BancoDeTeste.NovoAnimal(db, "001");

            await new SaudeService(db).AdicionarAsync(new RegistroSaude
            {
                AnimalId = animal.Id,
                TipoOcorrencia = TipoOcorrenciaSaude.DoencaDiagnosticada,
                DescricaoDetalhada = "Febre e falta de apetite"
            }, novoEstadoDoAnimal: "Em tratamento");

            Assert.Equal("Em tratamento", db.Animais.Single().StatusSaude);
            Assert.Single(db.RegistrosSaude);
        }

        [Fact]
        public async Task Sem_novo_estado_o_animal_continua_como_estava()
        {
            using var db = BancoDeTeste.Criar();
            var animal = BancoDeTeste.NovoAnimal(db, "001");

            await new SaudeService(db).AdicionarAsync(new RegistroSaude
            {
                AnimalId = animal.Id,
                TipoOcorrencia = TipoOcorrenciaSaude.ObservacaoRotina,
                DescricaoDetalhada = "Tudo normal"
            }, novoEstadoDoAnimal: null);

            Assert.Equal("Saudável", db.Animais.Single().StatusSaude);
        }
    }

    public class FuncionarioServiceTests
    {
        [Fact]
        public async Task Cpf_e_salvo_so_com_numeros_e_nao_pode_repetir()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new FuncionarioService(db);

            await servico.AdicionarAsync(new Funcionario { NomeCompleto = " Maria ", Cpf = "529.982.247-25", Cargo = "Tratador" });

            var salvo = db.Funcionarios.Single();
            Assert.Equal("52998224725", salvo.Cpf);
            Assert.Equal("Maria", salvo.NomeCompleto);
            Assert.True(await servico.ExisteCpfAsync("529.982.247-25"));
            Assert.False(await servico.ExisteCpfAsync("529.982.247-25", ignorarId: salvo.Id));
        }

        [Fact]
        public async Task Desligar_mantem_o_cadastro_e_marca_a_data()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new FuncionarioService(db);
            var funcionario = new Funcionario { NomeCompleto = "João", Cpf = "52998224725", Cargo = "Vaqueiro", DataAdmissao = DateTime.Today.AddYears(-2) };
            await servico.AdicionarAsync(funcionario);

            await servico.DesligarAsync(funcionario.Id, DateTime.Today);

            var salvo = db.Funcionarios.Single();
            Assert.False(salvo.Ativo);
            Assert.Equal(DateTime.Today, salvo.DataDemissao);
        }

        [Fact]
        public async Task Nao_exclui_funcionario_que_ja_registrou_algo()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new FuncionarioService(db);
            var funcionario = new Funcionario { NomeCompleto = "Ana", Cpf = "11144477735", Cargo = "Veterinário" };
            await servico.AdicionarAsync(funcionario);
            var animal = BancoDeTeste.NovoAnimal(db, "001");
            db.RegistrosSaude.Add(new RegistroSaude { AnimalId = animal.Id, FuncionarioId = funcionario.Id, DescricaoDetalhada = "Consulta" });
            db.SaveChanges();

            Assert.False(await servico.RemoverAsync(funcionario.Id));
        }
    }

    public class AnimalServiceTests
    {
        [Fact]
        public async Task Brinco_repetido_ignora_maiusculas_e_espacos()
        {
            using var db = BancoDeTeste.Criar();
            BancoDeTeste.NovoAnimal(db, "AB-01");

            Assert.True(await new AnimalService(db).ExisteTagAsync(" ab-01 "));
        }

        [Fact]
        public async Task Filtra_por_estado_de_saude_e_por_busca()
        {
            using var db = BancoDeTeste.Criar();
            BancoDeTeste.NovoAnimal(db, "001", estado: "Em tratamento");
            BancoDeTeste.NovoAnimal(db, "002");
            BancoDeTeste.NovoAnimal(db, "103");
            var servico = new AnimalService(db);

            Assert.Equal("001", Assert.Single(await servico.ObterFiltradosAsync(null, null, null, "Em tratamento")).TagIdentificacao);
            Assert.Equal(2, (await servico.ObterFiltradosAsync("00", null, null, null)).Count());
        }
    }

    public class SemeadorDeDadosTests
    {
        [Fact]
        public async Task Rodar_varias_vezes_nao_duplica_especies_e_racas()
        {
            using var db = BancoDeTeste.Criar();

            await SemeadorDeDados.SemearEspeciesERacasAsync(db);
            var especies = db.Especies.Count();
            var racas = db.Racas.Count();
            await SemeadorDeDados.SemearEspeciesERacasAsync(db);

            Assert.True(especies >= 7);
            Assert.Equal(especies, db.Especies.Count());
            Assert.Equal(racas, db.Racas.Count());
        }

        [Fact]
        public async Task Vacina_excluida_pelo_usuario_nao_volta()
        {
            using var db = BancoDeTeste.Criar();
            await SemeadorDeDados.SemearVacinasAsync(db);
            var total = db.TiposVacina.Count();

            db.TiposVacina.Remove(db.TiposVacina.First());
            db.SaveChanges();
            await SemeadorDeDados.SemearVacinasAsync(db);

            Assert.Equal(total - 1, db.TiposVacina.Count());
        }
    }

    public class PainelServiceTests
    {
        [Fact]
        public async Task Resumo_junta_os_numeros_dos_modulos()
        {
            using var db = BancoDeTeste.Criar();
            var galpao = BancoDeTeste.NovoGalpao(db, capacidade: 2);
            BancoDeTeste.NovoAnimal(db, "001", galpao.Id);
            BancoDeTeste.NovoAnimal(db, "002", galpao.Id, estado: "Em tratamento");
            var milho = BancoDeTeste.NovoAlimento(db, estoqueMinimo: 50);
            var estoque = new EstoqueService(db);
            await estoque.AdicionarAsync(new MovimentacaoEstoqueAlimento
            {
                TipoAlimentoId = milho.Id,
                TipoMovimentacao = TipoMovimentacaoEstoque.EntradaCompra,
                Quantidade = 20
            });

            var painel = new PainelService(db, new GalpaoService(db), new VacinacaoService(db), estoque);
            var resumo = await painel.ObterResumoAsync();

            Assert.Equal(2, resumo.TotalAnimais);
            Assert.Equal(100, resumo.OcupacaoPercentual);
            Assert.Contains("Galpão 1", resumo.GalpoesLotados);
            Assert.Equal(1, resumo.AnimaisEmAtencao);
            Assert.Equal("Milho", Assert.Single(resumo.AlimentosComEstoqueBaixo).Alimento.Nome);
        }
    }
}
