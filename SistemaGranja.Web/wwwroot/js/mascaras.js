// Máscaras simples de digitação: <input data-mascara="cpf"> ou <input data-mascara="telefone">
(function () {
    function somenteNumeros(valor) { return valor.replace(/\D/g, ''); }

    var mascaras = {
        cpf: function (v) {
            v = somenteNumeros(v).slice(0, 11);
            return v
                .replace(/^(\d{3})(\d)/, '$1.$2')
                .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
                .replace(/\.(\d{3})(\d{1,2})$/, '.$1-$2');
        },
        telefone: function (v) {
            v = somenteNumeros(v).slice(0, 11);
            if (v.length <= 2) return v.length ? '(' + v : v;
            if (v.length <= 6) return '(' + v.slice(0, 2) + ') ' + v.slice(2);
            if (v.length <= 10) return '(' + v.slice(0, 2) + ') ' + v.slice(2, 6) + '-' + v.slice(6);
            return '(' + v.slice(0, 2) + ') ' + v.slice(2, 7) + '-' + v.slice(7);
        }
    };

    document.querySelectorAll('[data-mascara]').forEach(function (campo) {
        var aplicar = mascaras[campo.dataset.mascara];
        if (!aplicar) return;
        campo.value = aplicar(campo.value);
        campo.addEventListener('input', function () { campo.value = aplicar(campo.value); });
    });
})();
