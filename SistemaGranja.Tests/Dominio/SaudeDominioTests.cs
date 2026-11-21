using SistemaGranja.Domain;

namespace SistemaGranja.Tests.Dominio
{
    public class SaudeDominioTests
    {
        [Theory]
        [InlineData(TipoOcorrenciaSaude.DoencaDiagnosticada, "Doença diagnosticada")]
        [InlineData(TipoOcorrenciaSaude.TratamentoConcluido, "Fim de tratamento")]
        [InlineData(TipoOcorrenciaSaude.Outro, "Outro")]
        public void Tipos_de_ocorrencia_tem_nome_em_portugues(TipoOcorrenciaSaude tipo, string esperado)
        {
            Assert.Equal(esperado, tipo.NomeExibicao());
        }

        [Fact]
        public void Todos_os_tipos_tem_o_atributo_Display()
        {
            foreach (var tipo in Enum.GetValues<TipoOcorrenciaSaude>())
            {
                var campo = typeof(TipoOcorrenciaSaude).GetField(tipo.ToString())!;
                var display = campo.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false);
                Assert.Single(display);
            }
        }
    }
}
