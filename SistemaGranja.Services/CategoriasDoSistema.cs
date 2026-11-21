using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    // Categorias usadas pelos lançamentos automáticos (compra no estoque, venda de animais)
    public static class CategoriasDoSistema
    {
        public const string CompraDeAlimento = "Ração e alimentação";
        public const string VendaDeAnimais = "Venda de animais";

        // Procura a categoria pelo nome; se a fazenda tiver apagado ou renomeado, cria de novo
        public static async Task<CategoriaFinanceira> ObterOuCriarAsync(AppDbContext db, TipoLancamento tipo, string nome)
        {
            await SemeadorDeDados.SemearCategoriasFinanceirasAsync(db);

            var nomeMinusculo = nome.ToLower();
            var categoria = await db.CategoriasFinanceiras
                .FirstOrDefaultAsync(c => c.Tipo == tipo && c.Nome.ToLower() == nomeMinusculo);
            if (categoria != null) return categoria;

            categoria = new CategoriaFinanceira { Tipo = tipo, Nome = nome };
            db.CategoriasFinanceiras.Add(categoria);
            await db.SaveChangesAsync();
            return categoria;
        }
    }
}
