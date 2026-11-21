using SistemaGranja.Domain;

namespace SistemaGranja.Tests.Dominio
{
    public class CpfTests
    {
        [Theory]
        [InlineData("529.982.247-25")]
        [InlineData("52998224725")]
        [InlineData("111.444.777-35")]
        [InlineData("123.456.789-09")]
        public void Cpf_com_digitos_corretos_e_valido(string cpf)
        {
            Assert.True(CpfAttribute.EhValido(cpf));
        }

        [Theory]
        [InlineData("123.456.789-00")] // dígitos verificadores errados
        [InlineData("111.111.111-11")] // todos os dígitos iguais
        [InlineData("000.000.000-00")]
        [InlineData("12345")]          // curto demais
        [InlineData("529.982.247-255")] // longo demais
        [InlineData("abc.def.ghi-jk")]
        public void Cpf_invalido_e_recusado(string cpf)
        {
            Assert.False(CpfAttribute.EhValido(cpf));
        }

        [Fact]
        public void Campo_vazio_fica_para_o_Required_validar()
        {
            var atributo = new CpfAttribute();
            Assert.True(atributo.IsValid(null));
            Assert.True(atributo.IsValid(""));
        }

        [Fact]
        public void Formata_cpf_com_pontos_e_traco()
        {
            Assert.Equal("529.982.247-25", CpfAttribute.Formatar("52998224725"));
        }

        [Fact]
        public void Remove_tudo_que_nao_e_numero()
        {
            Assert.Equal("52998224725", CpfAttribute.SomenteNumeros("529.982.247-25"));
        }
    }
}
