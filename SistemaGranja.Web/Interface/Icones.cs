using Microsoft.AspNetCore.Html;

namespace SistemaGranja.Web.Interface
{
    // Ícones de traço simples (SVG), usados no menu e nos botões.
    // Uso na tela: @Icones.Svg("animais")
    public static class Icones
    {
        private static readonly Dictionary<string, string> Desenhos = new()
        {
            ["marca"] = "<path d=\"M3.5 13.5h17l-2.5 5.5h-12z\"/><path d=\"M12 3.5v10\"/><path d=\"M12 4.5l5.5 5H12\"/>",
            ["painel"] = "<path d=\"M3 10.5 12 3l9 7.5V20a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1z\"/>",
            ["animais"] = "<path d=\"M20.6 13.4 13.4 20.6a2 2 0 0 1-2.8 0L3 13V3h10l7.6 7.6a2 2 0 0 1 0 2.8z\"/><circle cx=\"7.5\" cy=\"7.5\" r=\"1.5\"/>",
            ["lotes"] = "<rect x=\"3\" y=\"4\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"14\" y=\"4\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"3\" y=\"15\" width=\"7\" height=\"5\" rx=\"1.5\"/><rect x=\"14\" y=\"15\" width=\"7\" height=\"5\" rx=\"1.5\"/>",
            ["galpoes"] = "<path d=\"M3 21V9l9-6 9 6v12\"/><path d=\"M9 21v-7h6v7\"/>",
            ["sanidade"] = "<path d=\"M12 3 4 6v6c0 5 3.5 8 8 9 4.5-1 8-4 8-9V6z\"/><path d=\"M12 9v6M9 12h6\"/>",
            ["saude"] = "<path d=\"M20.8 8.6a5 5 0 0 0-8.8-3.2 5 5 0 0 0-8.8 3.2c0 5.4 8.8 11.4 8.8 11.4s8.8-6 8.8-11.4z\"/>",
            ["estoque"] = "<path d=\"M21 8 12 3 3 8v8l9 5 9-5z\"/><path d=\"M3 8l9 5 9-5M12 13v8\"/>",
            ["financeiro"] = "<rect x=\"3\" y=\"6\" width=\"18\" height=\"13\" rx=\"2\"/><path d=\"M3 10h18M16 14.5h2\"/>",
            ["funcionarios"] = "<circle cx=\"9\" cy=\"8\" r=\"3.5\"/><path d=\"M2.5 20c.8-3.5 3.4-5.5 6.5-5.5s5.7 2 6.5 5.5\"/><path d=\"M16 4.5a3.5 3.5 0 0 1 0 7M18 14.5c1.9.6 3.1 2.5 3.5 5.5\"/>",
            ["cadastros"] = "<path d=\"M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01\"/>",
            ["equipe"] = "<path d=\"M4 6h10M18 6h2M4 12h4M12 12h8M4 18h12\"/><circle cx=\"16\" cy=\"6\" r=\"2\"/><circle cx=\"10\" cy=\"12\" r=\"2\"/><circle cx=\"18\" cy=\"18\" r=\"2\"/>",
            ["conta"] = "<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 21c1-4 4-6 8-6s7 2 8 6\"/>",
            ["sair"] = "<path d=\"M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3\"/><path d=\"M10 17l5-5-5-5M15 12H3\"/>",
            ["buscar"] = "<circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"m20 20-3.5-3.5\"/>",
            ["mais"] = "<path d=\"M12 5v14M5 12h14\"/>",
            ["menu"] = "<path d=\"M4 6h16M4 12h16M4 18h16\"/>",
            ["lua"] = "<path d=\"M20 14.5A8 8 0 0 1 9.5 4 8 8 0 1 0 20 14.5z\"/>",
            ["sol"] = "<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4\"/>",
            ["seta"] = "<path d=\"m9 6 6 6-6 6\"/>",
            ["vacina"] = "<path d=\"m18 2 4 4M17 7l3-3M19 9 8.7 19.3a2.4 2.4 0 0 1-3.4 0l-.6-.6a2.4 2.4 0 0 1 0-3.4L15 5M9 11l4 4M5 19l-3 3M14 4l6 6\"/>",
            ["grupo"] = "<circle cx=\"7\" cy=\"8\" r=\"3\"/><circle cx=\"17\" cy=\"8\" r=\"3\"/><path d=\"M2 20c.6-3 2.5-5 5-5s4.4 2 5 5M12 20c.6-3 2.5-5 5-5s4.4 2 5 5\"/>",
        };

        public static IHtmlContent Svg(string nome, int tamanho = 20)
        {
            var desenho = Desenhos.TryGetValue(nome, out var d) ? d : string.Empty;
            return new HtmlString(
                $"<svg class=\"icone\" width=\"{tamanho}\" height=\"{tamanho}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
                $"stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\" focusable=\"false\">{desenho}</svg>");
        }
    }
}
