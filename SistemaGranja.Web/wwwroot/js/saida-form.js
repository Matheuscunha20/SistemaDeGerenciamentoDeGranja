// Saída do rebanho: os campos de valor e pagamento só aparecem quando o motivo é Venda
(function () {
    var motivo = document.querySelector('[data-motivo-saida]');
    var camposVenda = document.querySelector('[data-campos-venda]');
    if (!motivo || !camposVenda) return;

    function atualizar() {
        camposVenda.style.display = motivo.value === '1' ? '' : 'none';
    }
    motivo.addEventListener('change', atualizar);
    atualizar();
})();

// Venda de lote: marcar/desmarcar todos os animais
(function () {
    var todos = document.querySelector('[data-marcar-todos]');
    if (!todos) return;
    todos.addEventListener('change', function () {
        document.querySelectorAll('[data-animal-venda]').forEach(function (caixa) { caixa.checked = todos.checked; });
    });
})();
