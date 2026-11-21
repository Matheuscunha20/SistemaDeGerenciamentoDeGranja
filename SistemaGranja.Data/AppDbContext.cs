using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SistemaGranja.Domain;

namespace SistemaGranja.Data
{
    // Herda de IdentityDbContext para incluir as tabelas de login (AspNetUsers etc.)
    // IDataProtectionKeyContext: guarda no banco as chaves que protegem o login e os links de e-mail
    // (em hospedagens sem disco permanente, como o Render grátis, elas se perderiam a cada reinício)
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        private readonly IFazendaAtual? _fazendaAtual;
        private int? _fazendaDefinida;

        public AppDbContext(DbContextOptions<AppDbContext> options, IFazendaAtual? fazendaAtual = null) : base(options)
        {
            _fazendaAtual = fazendaAtual;
        }

        // Fazenda usada para filtrar e salvar os dados.
        // Vem do usuário logado, ou de DefinirFazenda (cadastro de conta, testes).
        public int? FazendaAtualId => _fazendaDefinida ?? _fazendaAtual?.FazendaId;

        public void DefinirFazenda(int fazendaId) => _fazendaDefinida = fazendaId;

        public DbSet<Fazenda> Fazendas { get; set; }
        public DbSet<Animal> Animais { get; set; }
        public DbSet<Galpao> Galpoes { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<TipoAlimento> TiposAlimento { get; set; }
        public DbSet<MovimentacaoEstoqueAlimento> MovimentacoesEstoqueAlimento { get; set; }
        public DbSet<TipoVacina> TiposVacina { get; set; }
        public DbSet<RegistroVacinacao> RegistrosVacinacao { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<RegistroSaude> RegistrosSaude { get; set; }
        public DbSet<Especie> Especies { get; set; }
        public DbSet<Raca> Racas { get; set; }
        public DbSet<Convite> Convites { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<CategoriaFinanceira> CategoriasFinanceiras { get; set; }
        public DbSet<LancamentoFinanceiro> LancamentosFinanceiros { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // ESSENCIAL ao usar IdentityDbContext

            // Usuário pertence a uma fazenda
            builder.Entity<ApplicationUser>()
                .HasOne<Fazenda>()
                .WithMany()
                .HasForeignKey(u => u.FazendaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Convite: o token do link é único
            builder.Entity<Convite>()
                .HasIndex(c => c.Token)
                .IsUnique();

            // Espécie: nome único dentro da fazenda
            builder.Entity<Especie>()
                .HasIndex(e => new { e.FazendaId, e.Nome })
                .IsUnique();

            // Raça: pertence a uma espécie; nome único dentro da espécie.
            // Restrict: não deixa excluir uma espécie que ainda tem raças.
            builder.Entity<Raca>()
                .HasOne(r => r.Especie)
                .WithMany(e => e.Racas)
                .HasForeignKey(r => r.EspecieId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Raca>()
                .HasIndex(r => new { r.EspecieId, r.Nome })
                .IsUnique();

            // Animal: raça opcional no banco (animais antigos), obrigatória só no formulário.
            // O [Required] da entidade faria o EF tratar a raça como obrigatória no banco e
            // esconder das listas os animais sem raça (INNER JOIN). IsRequired(false) evita isso.
            // Restrict: não deixa excluir uma raça que ainda tem animais.
            builder.Entity<Animal>()
                .Property(a => a.RacaId)
                .IsRequired(false);

            builder.Entity<Animal>()
                .HasOne(a => a.Raca)
                .WithMany(r => r.Animais)
                .HasForeignKey(a => a.RacaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Financeiro: nome da categoria único por fazenda e tipo (receita/despesa)
            builder.Entity<CategoriaFinanceira>()
                .HasIndex(c => new { c.FazendaId, c.Tipo, c.Nome })
                .IsUnique();

            // Restrict: categoria com lançamentos não é excluída (pode ser desativada)
            builder.Entity<LancamentoFinanceiro>()
                .HasOne(l => l.Categoria)
                .WithMany()
                .HasForeignKey(l => l.CategoriaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Se o lote for excluído, o lançamento continua, só sem o lote
            builder.Entity<LancamentoFinanceiro>()
                .HasOne(l => l.Lote)
                .WithMany()
                .HasForeignKey(l => l.LoteId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Despesa criada por uma compra no estoque: se a compra for excluída, o vínculo some
            builder.Entity<LancamentoFinanceiro>()
                .HasOne(l => l.MovimentacaoEstoque)
                .WithMany()
                .HasForeignKey(l => l.MovimentacaoEstoqueId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            ConfigurarSeparacaoPorFazenda(builder);
        }

        // Para cada entidade que pertence a uma fazenda:
        // - liga à tabela Fazendas (sem exclusão em cascata)
        // - aplica o filtro automático: toda consulta só traz dados da fazenda atual
        private void ConfigurarSeparacaoPorFazenda(ModelBuilder builder)
        {
            var tipos = builder.Model.GetEntityTypes()
                .Where(t => typeof(IPertenceAFazenda).IsAssignableFrom(t.ClrType))
                .Select(t => t.ClrType)
                .ToList();

            foreach (var tipo in tipos)
            {
                builder.Entity(tipo)
                    .HasOne(typeof(Fazenda))
                    .WithMany()
                    .HasForeignKey(nameof(IPertenceAFazenda.FazendaId))
                    .OnDelete(DeleteBehavior.Restrict);

                // e => e.FazendaId == this.FazendaAtualId
                // O EF reconhece "this" e usa a fazenda do contexto de cada requisição.
                var entidade = Expression.Parameter(tipo, "e");
                var fazendaDaEntidade = Expression.Convert(
                    Expression.Property(entidade, nameof(IPertenceAFazenda.FazendaId)), typeof(int?));
                var fazendaAtual = Expression.Property(Expression.Constant(this), nameof(FazendaAtualId));
                var filtro = Expression.Lambda(Expression.Equal(fazendaDaEntidade, fazendaAtual), entidade);

                builder.Entity(tipo).HasQueryFilter(filtro);
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AplicarRegrasDaFazenda();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AplicarRegrasDaFazenda();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        // Antes de salvar:
        // - registros novos recebem a fazenda atual
        // - registros alterados ou excluídos precisam ser da fazenda atual
        // - ligações (galpão, lote, raça, vacina...) precisam apontar para registros da mesma fazenda
        // Isso protege contra alguém alterar o formulário para mexer em dados de outra fazenda.
        private void AplicarRegrasDaFazenda()
        {
            var entradas = ChangeTracker.Entries<IPertenceAFazenda>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            if (entradas.Count == 0) return;

            var fazendaId = FazendaAtualId
                ?? throw new InvalidOperationException("Nenhuma fazenda selecionada para salvar os dados.");

            foreach (var entrada in entradas)
            {
                switch (entrada.State)
                {
                    case EntityState.Added:
                        entrada.Entity.FazendaId = fazendaId;
                        break;

                    case EntityState.Modified:
                    case EntityState.Deleted:
                        GarantirQuePertenceAFazenda(entrada, fazendaId);
                        break;
                }

                if (entrada.State is EntityState.Added or EntityState.Modified)
                    GarantirLigacoesDaMesmaFazenda(entrada);
            }
        }

        private void GarantirQuePertenceAFazenda(EntityEntry<IPertenceAFazenda> entrada, int fazendaId)
        {
            // Lê do banco a fazenda verdadeira do registro (não confia no que veio do formulário)
            var valoresNoBanco = entrada.GetDatabaseValues();
            var fazendaNoBanco = valoresNoBanco?.GetValue<int>(nameof(IPertenceAFazenda.FazendaId));

            if (fazendaNoBanco != fazendaId)
                throw new UnauthorizedAccessException("Este registro não pertence à sua fazenda.");

            // O formulário não envia a fazenda; mantém a que está no banco e não deixa alterá-la
            var propriedade = entrada.Property(e => e.FazendaId);
            propriedade.CurrentValue = fazendaId;
            propriedade.OriginalValue = fazendaId;
            propriedade.IsModified = false;
        }

        private void GarantirLigacoesDaMesmaFazenda(EntityEntry<IPertenceAFazenda> entrada)
        {
            foreach (var chaveEstrangeira in entrada.Metadata.GetForeignKeys())
            {
                var tipoPrincipal = chaveEstrangeira.PrincipalEntityType.ClrType;
                if (!typeof(IPertenceAFazenda).IsAssignableFrom(tipoPrincipal)) continue;

                var valor = entrada.Property(chaveEstrangeira.Properties[0].Name).CurrentValue;
                if (valor == null) continue;

                // Find respeita o filtro da fazenda: registro de outra fazenda volta null
                if (Find(tipoPrincipal, valor) == null)
                    throw new UnauthorizedAccessException(
                        $"O registro ligado ({tipoPrincipal.Name} {valor}) não pertence à sua fazenda.");
            }
        }
    }
}
