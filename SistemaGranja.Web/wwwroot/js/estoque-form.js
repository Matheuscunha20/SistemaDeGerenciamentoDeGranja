// Formulário de estoque: mostra a unidade e o saldo do alimento, o valor total da compra,
// e esconde os campos de compra quando a movimentação é uma saída ou ajuste.
(function () {
    var tipo = document.getElementById('TipoMovimentacao');
    var alimento = document.getElementById('TipoAlimentoId');
    var quantidade = document.getElementById('Quantidade');
    var preco = document.getElementById('PrecoUnitario');
    var camposCompra = document.getElementById('camposCompra');
    var unidade = document.getElementById('unidadeQuantidade');
    var saldo = document.getElementById('saldoDisponivel');
    var valorTotal = document.getElementById('valorTotal');
    var rotulo = document.getElementById('rotuloFornecedorDestino');
    if (!tipo || !alimento) return;

    var ENTRADA_COMPRA = '1';
    var numero = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

    // Aceita "1.250,50" (padrão brasileiro) e também "12.5"
    function lerNumero(texto) {
        var valor = String(texto || '').trim();
        if (valor.indexOf(',') !== -1) valor = valor.replace(/\./g, '').replace(',', '.');
        return parseFloat(valor) || 0;
    }

    function atualizar() {
        var opcao = alimento.options[alimento.selectedIndex];
        var temAlimento = opcao && opcao.value !== '0';
        unidade.textContent = temAlimento ? (opcao.dataset.unidade || '—') : '—';
        saldo.textContent = temAlimento
            ? 'Saldo atual: ' + numero.format(parseFloat(opcao.dataset.saldo || '0')) + ' ' + (opcao.dataset.unidade || '')
            : '';

        var ehCompra = tipo.value === ENTRADA_COMPRA;
        camposCompra.style.display = ehCompra ? '' : 'none';
        var blocoFinanceiro = document.getElementById('lancarNoFinanceiroBloco');
        if (blocoFinanceiro) blocoFinanceiro.style.display = ehCompra ? '' : 'none';
        if (rotulo) rotulo.textContent = ehCompra ? 'Fornecedor' : 'Destino (galpão, lote...)';

        if (valorTotal) {
            var total = lerNumero(quantidade.value) * lerNumero(preco.value);
            valorTotal.textContent = ehCompra && total > 0 ? 'Valor total: ' + moeda.format(total) : '';
        }
    }

    [tipo, alimento].forEach(function (c) { c.addEventListener('change', atualizar); });
    [quantidade, preco].forEach(function (c) { if (c) c.addEventListener('input', atualizar); });
    atualizar();
})();
