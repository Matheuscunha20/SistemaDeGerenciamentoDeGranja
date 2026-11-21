// Formulário de vacinação: ao escolher a vacina ou mudar a data,
// calcula a próxima dose pelo intervalo de reforço e sugere a dose padrão.
(function () {
    var vacina = document.getElementById('TipoVacinaId');
    var data = document.getElementById('DataVacinacao');
    var proxima = document.getElementById('DataProximaDoseRecomendada');
    var dose = document.getElementById('DoseAplicada');
    var dica = document.getElementById('dicaProximaDose');
    if (!vacina || !data || !proxima) return;

    function somarDias(textoData, dias) {
        var partes = textoData.split('-');
        if (partes.length !== 3) return '';
        var d = new Date(Number(partes[0]), Number(partes[1]) - 1, Number(partes[2]));
        d.setDate(d.getDate() + dias);
        var mes = String(d.getMonth() + 1).padStart(2, '0');
        var dia = String(d.getDate()).padStart(2, '0');
        return d.getFullYear() + '-' + mes + '-' + dia;
    }

    function atualizar(trocouVacina) {
        var opcao = vacina.options[vacina.selectedIndex];
        if (!opcao || opcao.value === '0') return;

        var intervalo = parseInt(opcao.dataset.intervalo, 10);
        if (intervalo > 0 && data.value) {
            proxima.value = somarDias(data.value, intervalo);
            if (dica) dica.textContent = 'Reforço a cada ' + intervalo + ' dias. Você pode alterar a data.';
        } else if (trocouVacina) {
            proxima.value = '';
            if (dica) dica.textContent = 'Esta vacina não tem intervalo de reforço cadastrado (dose única ou conforme protocolo).';
        }

        if (trocouVacina && dose && opcao.dataset.dose && (!dose.value || dose.dataset.automatica === 'sim')) {
            dose.value = opcao.dataset.dose;
            dose.dataset.automatica = 'sim';
        }
    }

    if (dose) dose.addEventListener('input', function () { dose.dataset.automatica = 'nao'; });
    vacina.addEventListener('change', function () { atualizar(true); });
    data.addEventListener('change', function () { atualizar(false); });
})();
