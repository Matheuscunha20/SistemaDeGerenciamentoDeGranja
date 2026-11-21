using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: AppDbContext.cs (no projeto SistemaGranja.Data)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Para IdentityDbContext
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain; // Para acessar suas entidades de domínio
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;



namespace SistemaGranja.Data
{
    // Herda de IdentityDbContext para incluir as tabelas do ASP.NET Core Identity
    // e passamos nossa classe ApplicationUser personalizada.
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Construtor que recebe as opções de configuração do DbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet para cada uma das suas entidades de domínio
        // Isso diz ao EF Core para criar uma tabela para cada uma dessas classes.
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

        // Adicione aqui DbSets para as entidades que criamos na conversa anterior,
        // quando você decidir adicioná-las ao seu projeto Domain:
        // public DbSet<CategoriaFinanceira> CategoriasFinanceiras { get; set; }
        // public DbSet<TransacaoFinanceira> TransacoesFinanceiras { get; set; }
        // public DbSet<VendaAnimal> VendasAnimais { get; set; }
        // public DbSet<GranjaInfo> GranjaInfos { get; set; }
        // public DbSet<Cliente> Clientes { get; set; }
        // public DbSet<Fornecedor> Fornecedores { get; set; }


        // O IdentityDbContext já inclui DbSets para:
        // Users (AspNetUsers), Roles (AspNetRoles), UserRoles (AspNetUserRoles), etc.

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // ESSENCIAL ao usar IdentityDbContext

            // Espécie: nome único (não pode haver dois "Bovino")
            builder.Entity<Especie>()
                .HasIndex(e => e.Nome)
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
        }
    }
}