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
