using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class EquipeServiceTests
    {
        private static ApplicationUser NovoUsuario(AppDbContext db, string email, PerfilUsuario perfil)
        {
            var usuario = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                NormalizedUserName = email.ToUpperInvariant(),
                FazendaId = db.FazendaAtualId,
                Perfil = perfil,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            db.Users.Add(usuario);
            db.SaveChanges();
            return usuario;
        }

        [Fact]
        public async Task Cria_convite_com_link_unico_e_validade_de_7_dias()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new EquipeService(db);

            var (convite, erro) = await servico.CriarConviteAsync(" joao@fazenda.com ", PerfilUsuario.Funcionario);

            Assert.Null(erro);
            Assert.NotNull(convite);
            Assert.Equal("joao@fazenda.com", convite!.Email);
            Assert.Equal(64, convite.Token.Length);
            Assert.Equal(db.FazendaAtualId, convite.FazendaId);
            Assert.True(convite.ExpiraEm > DateTime.UtcNow.AddDays(6));
            Assert.Single(await servico.ObterConvitesPendentesAsync());
        }

        [Fact]
        public async Task Nao_convida_email_que_ja_tem_conta()
        {
            using var db = BancoDeTeste.Criar();
            NovoUsuario(db, "maria@fazenda.com", PerfilUsuario.Gerente);

            var (convite, erro) = await new EquipeService(db).CriarConviteAsync("MARIA@fazenda.com", PerfilUsuario.Funcionario);

            Assert.Null(convite);
            Assert.NotNull(erro);
        }

        [Fact]
        public async Task Novo_convite_para_o_mesmo_email_invalida_o_anterior()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new EquipeService(db);

            var (primeiro, _) = await servico.CriarConviteAsync("ana@fazenda.com", PerfilUsuario.Funcionario);
            var (segundo, _) = await servico.CriarConviteAsync("ana@fazenda.com", PerfilUsuario.Gerente);

            Assert.Null(await servico.ObterConviteValidoAsync(primeiro!.Token));
            Assert.NotNull(await servico.ObterConviteValidoAsync(segundo!.Token));
        }

        [Fact]
        public async Task Link_do_convite_funciona_sem_login_e_aceite_grava_a_data()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            string token;
            using (var dono = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A"))
                token = (await new EquipeService(dono).CriarConviteAsync("pedro@fazenda.com", PerfilUsuario.Funcionario)).Convite!.Token;

            // Quem abre o link ainda não tem fazenda selecionada
            var opcoes = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(banco).Options;
            using (var visitante = new AppDbContext(opcoes))
            {
                var servico = new EquipeService(visitante);
                var convite = await servico.ObterConviteValidoAsync(token);
                Assert.NotNull(convite);

                await servico.MarcarConviteAceitoAsync(convite!);
            }

            using var depois = new AppDbContext(opcoes);
            Assert.Null(await new EquipeService(depois).ObterConviteValidoAsync(token));
        }

        [Fact]
        public async Task Convite_expirado_ou_token_errado_nao_vale()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new EquipeService(db);
            var (convite, _) = await servico.CriarConviteAsync("velho@fazenda.com", PerfilUsuario.Funcionario);
            convite!.ExpiraEm = DateTime.UtcNow.AddMinutes(-1);
            db.SaveChanges();

            Assert.Null(await servico.ObterConviteValidoAsync(convite.Token));
            Assert.Null(await servico.ObterConviteValidoAsync("token-que-nao-existe"));
            Assert.Null(await servico.ObterConviteValidoAsync(null));
        }

        [Fact]
        public async Task Cancelar_convite_remove_o_link()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new EquipeService(db);
            var (convite, _) = await servico.CriarConviteAsync("x@fazenda.com", PerfilUsuario.Funcionario);

            Assert.True(await servico.CancelarConviteAsync(convite!.Id));
            Assert.Null(await servico.ObterConviteValidoAsync(convite.Token));
        }

        [Fact]
        public async Task Membros_sao_so_da_propria_fazenda()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            using (var b = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B"))
                NovoUsuario(b, "de-b@fazenda.com", PerfilUsuario.Dono);

            using var a = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A");
            NovoUsuario(a, "de-a@fazenda.com", PerfilUsuario.Dono);

            var membros = await new EquipeService(a).ObterMembrosAsync();

            Assert.Equal("de-a@fazenda.com", Assert.Single(membros).Email);
        }

        [Fact]
        public async Task Dono_altera_perfil_de_outra_pessoa_mas_nao_o_proprio()
        {
            using var db = BancoDeTeste.Criar();
            var dono = NovoUsuario(db, "dono@fazenda.com", PerfilUsuario.Dono);
            var func = NovoUsuario(db, "func@fazenda.com", PerfilUsuario.Funcionario);
            var servico = new EquipeService(db);

            Assert.Null(await servico.AlterarPerfilAsync(func.Id, PerfilUsuario.Gerente, dono.Id));
            Assert.Equal(PerfilUsuario.Gerente, db.Users.Find(func.Id)!.Perfil);

            Assert.NotNull(await servico.AlterarPerfilAsync(dono.Id, PerfilUsuario.Funcionario, dono.Id));
            Assert.Equal(PerfilUsuario.Dono, db.Users.Find(dono.Id)!.Perfil);
        }

        [Fact]
        public async Task Fazenda_nunca_fica_sem_dono_ativo()
        {
            using var db = BancoDeTeste.Criar();
            var donoA = NovoUsuario(db, "a@fazenda.com", PerfilUsuario.Dono);
            var donoB = NovoUsuario(db, "b@fazenda.com", PerfilUsuario.Dono);
            var servico = new EquipeService(db);

            // B remove A: ok, B continua dono
            Assert.Null(await servico.RemoverAcessoAsync(donoA.Id, donoB.Id));

            // Uma terceira pessoa (gerente) tentando rebaixar B: B é o único dono ativo
            var gerente = NovoUsuario(db, "g@fazenda.com", PerfilUsuario.Gerente);
            Assert.NotNull(await servico.AlterarPerfilAsync(donoB.Id, PerfilUsuario.Gerente, gerente.Id));
        }

        [Fact]
        public async Task Remover_acesso_bloqueia_e_reativar_libera()
        {
            using var db = BancoDeTeste.Criar();
            var dono = NovoUsuario(db, "dono@fazenda.com", PerfilUsuario.Dono);
            var func = NovoUsuario(db, "func@fazenda.com", PerfilUsuario.Funcionario);
            var carimboAntes = func.SecurityStamp;
            var servico = new EquipeService(db);

            Assert.Null(await servico.RemoverAcessoAsync(func.Id, dono.Id));
            var removido = db.Users.Find(func.Id)!;
            Assert.True(EquipeService.AcessoRemovido(removido));
            Assert.NotEqual(carimboAntes, removido.SecurityStamp);

            Assert.Null(await servico.ReativarAcessoAsync(func.Id));
            Assert.False(EquipeService.AcessoRemovido(db.Users.Find(func.Id)!));
        }

        [Fact]
        public async Task Nao_remove_o_proprio_acesso_nem_pessoa_de_outra_fazenda()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            string idDeOutraFazenda;
            using (var b = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B"))
                idDeOutraFazenda = NovoUsuario(b, "de-b@fazenda.com", PerfilUsuario.Funcionario).Id;

            using var a = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A");
            var dono = NovoUsuario(a, "dono@fazenda.com", PerfilUsuario.Dono);
            var servico = new EquipeService(a);

            Assert.NotNull(await servico.RemoverAcessoAsync(dono.Id, dono.Id));
            Assert.NotNull(await servico.RemoverAcessoAsync(idDeOutraFazenda, dono.Id));
            Assert.False(EquipeService.AcessoRemovido(a.Users.Find(idDeOutraFazenda)!));
        }

        [Fact]
        public async Task Renomeia_a_fazenda()
        {
            using var db = BancoDeTeste.Criar();
            var servico = new FazendaService(db);

            Assert.Null(await servico.RenomearAsync("  Fazenda Boa Vista "));
            Assert.Equal("Fazenda Boa Vista", (await servico.ObterAtualAsync())!.Nome);
            Assert.NotNull(await servico.RenomearAsync("   "));
        }
    }
}
