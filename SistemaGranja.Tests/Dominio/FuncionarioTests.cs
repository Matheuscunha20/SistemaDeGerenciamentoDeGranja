using SistemaGranja.Domain;

namespace SistemaGranja.Tests.Dominio
{
    public class FuncionarioTests
    {
        [Fact]
        public void Recem_admitido_tem_menos_de_um_mes()
        {
            var funcionario = new Funcionario { DataAdmissao = DateTime.Today };
            Assert.Equal("Menos de 1 mês", funcionario.TempoDeCasa);
        }

        [Fact]
        public void Tempo_de_casa_para_na_data_do_desligamento()
        {
            var admissao = DateTime.Today.AddYears(-5);
            var funcionario = new Funcionario
            {
                DataAdmissao = admissao,
                DataDemissao = admissao.AddYears(1)
            };

            Assert.Equal("1 ano", funcionario.TempoDeCasa);
        }

        [Fact]
        public void Mostra_o_cpf_formatado()
        {
            var funcionario = new Funcionario { Cpf = "52998224725" };
            Assert.Equal("529.982.247-25", funcionario.CpfFormatado);
        }
    }
}
