using System.Collections.Generic;
using SistemaGranja.Domain;

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

        // Lista curta para o painel: as próximas doses (atrasadas primeiro)
        public List<RegistroVacinacao> ProximasDoses { get; set; } = new();

        // Financeiro do mês (só aparece para quem vê o financeiro)
        public decimal RecebidoNoMes { get; set; }
        public decimal PagoNoMes { get; set; }
        public decimal SaldoDoMes => RecebidoNoMes - PagoNoMes;
        public decimal AReceberProximos7Dias { get; set; }
        public decimal APagarProximos7Dias { get; set; }

        // Contas a pagar (só aparece para quem vê o financeiro)
        public int ContasAPagarVencidas { get; set; }
        public decimal ValorContasAPagarVencidas { get; set; }
        public int ContasAPagarProximos7Dias { get; set; }

        // Alimentos que acabam primeiro, no ritmo atual de consumo
        public List<SaldoAlimento> EstoqueAcabandoPrimeiro { get; set; } = new();

        public int OcupacaoPercentual => CapacidadeTotal > 0 ? (int)System.Math.Round(AnimaisAlojados * 100.0 / CapacidadeTotal) : 0;
        public int AnimaisEmAtencao =>
            AnimaisPorEstadoDeSaude.GetValueOrDefault("Em tratamento") +
            AnimaisPorEstadoDeSaude.GetValueOrDefault("Em quarentena") +
            AnimaisPorEstadoDeSaude.GetValueOrDefault("Em observação");
    }
}
