// Formulário de receita/despesa: mostra só as categorias do tipo escolhido
(function () {
    var select = document.querySelector('[data-categorias]');
    var opcoesTipo = document.querySelectorAll('[data-tipo-lancamento]');
    if (!select || !opcoesTipo.length) return;

    function tipoEscolhido() {
        var marcado = document.querySelector('[data-tipo-lancamento]:checked');
        return marcado ? marcado.value : '';
    }

    function filtrar() {
        var tipo = tipoEscolhido();
        Array.prototype.forEach.call(select.options, function (opcao) {
            if (!opcao.dataset.tipo) return; // "-- Selecione --"
            var mostrar = opcao.dataset.tipo === tipo;
            opcao.hidden = !mostrar;
            opcao.disabled = !mostrar;
        });
        var atual = select.options[select.selectedIndex];
        if (atual && atual.disabled) select.value = '';

        var titulo = document.querySelector('.page-header h1');
        if (titulo && /^Nova /.test(titulo.textContent)) {
            titulo.textContent = tipo === 'Receita' ? 'Nova receita' : 'Nova despesa';
        }
    }

    opcoesTipo.forEach(function (opcao) { opcao.addEventListener('change', filtrar); });
    filtrar();
})();
