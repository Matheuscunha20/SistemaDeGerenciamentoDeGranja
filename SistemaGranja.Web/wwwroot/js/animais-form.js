// Formulário de animais: ao trocar a espécie, recarrega a lista de raças
(function () {
    var selectEspecie = document.getElementById('especieId');
    var selectRaca = document.getElementById('RacaId');
    if (!selectEspecie || !selectRaca) return;

    function preencherRacas(racas, textoVazio) {
        selectRaca.innerHTML = '';
        var vazio = document.createElement('option');
        vazio.value = '';
        vazio.textContent = textoVazio;
        selectRaca.appendChild(vazio);

        racas.forEach(function (raca) {
            var opcao = document.createElement('option');
            opcao.value = raca.id;
            opcao.textContent = raca.nome;
            selectRaca.appendChild(opcao);
        });
    }

    selectEspecie.addEventListener('change', function () {
        var especieId = selectEspecie.value;
        if (!especieId) {
            preencherRacas([], '-- Selecione a espécie primeiro --');
            return;
        }

        selectRaca.disabled = true;
        var url = selectEspecie.dataset.urlRacas + '?especieId=' + encodeURIComponent(especieId);

        fetch(url)
            .then(function (resposta) { return resposta.json(); })
            .then(function (racas) { preencherRacas(racas, '-- Selecione --'); })
            .catch(function () { preencherRacas([], 'Erro ao carregar raças'); })
            .finally(function () { selectRaca.disabled = false; });
    });
})();
