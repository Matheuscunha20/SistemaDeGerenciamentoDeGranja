using SistemaGranja.Domain;

namespace SistemaGranja.Tests.Dominio
{
    public class AnimalTests
    {
        private static string IdadeDe(DateTime nascimento) =>
            new Animal { DataNascimento = nascimento }.IdadeDescricao;

        [Fact]
        public void Nascido_hoje_tem_zero_dias()
        {
            Assert.Equal("0 dias", IdadeDe(DateTime.Today));
        }

        [Fact]
        public void Nascido_ontem_tem_um_dia()
        {
            Assert.Equal("1 dia", IdadeDe(DateTime.Today.AddDays(-1)));
        }

        [Fact]
        public void Mostra_meses_antes_de_completar_um_ano()
        {
            Assert.Equal("5 meses", IdadeDe(DateTime.Today.AddMonths(-5)));
        }

        [Fact]
        public void Mostra_anos_completos()
        {
            Assert.Equal("2 anos", IdadeDe(DateTime.Today.AddYears(-2)));
        }

        [Fact]
        public void Mostra_anos_e_meses()
        {
            Assert.Equal("1 ano e 3 meses", IdadeDe(DateTime.Today.AddMonths(-15)));
        }

        [Fact]
        public void Data_no_futuro_nao_tem_idade()
        {
            Assert.Equal("—", IdadeDe(DateTime.Today.AddDays(10)));
        }
    }
}
