// Service worker do AgroArca: deixa o app instalável e mostra uma tela amigável sem internet.
// Só guarda arquivos públicos (estilos, scripts, ícones e a tela offline).
// As páginas com dados da fazenda NUNCA ficam salvas no aparelho.
const VERSAO = 'agroarca-v3';
const ARQUIVOS = [
    '/offline.html',
    '/css/site.css',
    '/js/site.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/lib/jquery/dist/jquery.min.js',
    '/icones/icone-192.png',
    '/icones/icone.svg'
];

self.addEventListener('install', (evento) => {
    evento.waitUntil(caches.open(VERSAO).then((cache) => cache.addAll(ARQUIVOS)));
    self.skipWaiting();
});

// Ao atualizar o app, apaga os arquivos da versão anterior
self.addEventListener('activate', (evento) => {
    evento.waitUntil(
        caches.keys()
            .then((nomes) => Promise.all(nomes.filter((n) => n !== VERSAO).map((n) => caches.delete(n))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (evento) => {
    const pedido = evento.request;
    if (pedido.method !== 'GET') return;

    const url = new URL(pedido.url);
    if (url.origin !== self.location.origin) return;

    // Páginas: sempre da internet; sem conexão, mostra a tela offline
    if (pedido.mode === 'navigate') {
        evento.respondWith(fetch(pedido).catch(() => caches.match('/offline.html')));
        return;
    }

    // Arquivos estáticos: da internet quando possível (pega a versão nova), do aparelho quando offline
    const estatico = /^\/(css|js|lib|icones)\//.test(url.pathname);
    if (estatico) {
        evento.respondWith(
            fetch(pedido)
                .then((resposta) => {
                    if (resposta.ok) {
                        const copia = resposta.clone();
                        caches.open(VERSAO).then((cache) => cache.put(pedido, copia));
                    }
                    return resposta;
                })
                .catch(() => caches.match(pedido, { ignoreSearch: true }))
        );
    }
});
