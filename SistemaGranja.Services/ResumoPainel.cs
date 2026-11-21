using System.Collections.Generic;

namespace SistemaGranja.Services
{
    // Números e alertas mostrados no painel inicial
    public class ResumoPainel
    {
        public int TotalAnimais { get; set; }
        public Dictionary<string, int> AnimaisPorEspecie { get; set; } = new();
        public Dictionary<string, int> AnimaisPorEstadoDeSaude { get; set; } = new();

        public int TotalGalpoes { get; set; }
        public int CapacidadeTotal { get; set; }
        public int AnimaisAlojados { get; set; }
        public List<string> GalpoesLotados { get; set; } = new();

        public int LotesAtivos { get; set; }
        public int FuncionariosAtivos { get; set; }

        public int VacinasAtrasadas { get; set; }
        public int VacinasProximos7Dias { get; set; }

        public List<SaldoAlimento> AlimentosComEstoqueBaixo { get; set; } = new();

        public int OcupacaoPercentual => CapacidadeTotal > 0 ? (int)System.Math.Round(AnimaisAlojados * 100.0 / CapacidadeTotal) : 0;
        public int AnimaisEmAtencao =>
            AnimaisPorEstadoDeSaude.GetValueOrDefault("Em tratamento") +
            AnimaisPorEstadoDeSaude.GetValueOrDefault("Em quarentena") +
            AnimaisPorEstadoDeSaude.GetValueOrDefault("Em observação");
    }
}
