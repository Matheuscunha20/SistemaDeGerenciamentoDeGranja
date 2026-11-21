// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Campos decimais (peso, quantidade, preço): no padrão brasileiro o ponto separa milhar,
// então "12.5" seria lido como 125. Se a pessoa digitar um único ponto como decimal,
// troca por vírgula ao sair do campo: "12.5" vira "12,5".
// Com exatamente 3 dígitos depois do ponto ("1.250") é milhar no padrão brasileiro, e fica como está
// (exceto "0.125", que só pode ser decimal).
document.addEventListener('change', function (evento) {
    var campo = evento.target;
    if (!campo.matches || !campo.matches('input[inputmode="decimal"]')) return;

    var valor = campo.value.trim();
    if (/^-?(0\.\d+|\d+\.(\d{1,2}|\d{4,}))$/.test(valor)) {
        campo.value = valor.replace('.', ',');
    }
});

// Modo escuro: o botão com data-alternar-tema troca entre claro e escuro.
// A escolha fica salva neste aparelho; sem escolha, segue o tema do celular/computador.
document.addEventListener('click', function (evento) {
    var botao = evento.target.closest && evento.target.closest('[data-alternar-tema]');
    if (!botao) return;

    var raiz = document.documentElement;
    var novo = raiz.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
    raiz.setAttribute('data-bs-theme', novo);
    try { localStorage.setItem('agroarca-tema', novo); } catch (e) { }

    var cor = document.querySelector('meta[name="theme-color"]');
    if (cor) cor.setAttribute('content', novo === 'dark' ? '#131B16' : '#1F3A26');
});

// Listas em cartões no celular: cada célula recebe o nome da sua coluna (data-rotulo),
// que o CSS mostra ao lado do valor. Vale para toda <table class="tabela-cartoes">.
document.querySelectorAll('table.tabela-cartoes').forEach(function (tabela) {
    var titulos = Array.prototype.map.call(tabela.querySelectorAll('thead th'), function (th) {
        return th.textContent.trim();
    });
    tabela.querySelectorAll('tbody tr').forEach(function (linha) {
        Array.prototype.forEach.call(linha.children, function (celula, indice) {
            if (!celula.hasAttribute('data-rotulo')) celula.setAttribute('data-rotulo', titulos[indice] || '');
        });
    });
});
