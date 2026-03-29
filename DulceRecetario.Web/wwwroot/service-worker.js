// ============================================================
// Service Worker — Cielo Recetas PWA
// Estrategia: Cache-First para shell de la app (incluye DLLs
// de Blazor en _framework/), Network-First para datos de API.
// ============================================================

const APP_VERSION = 'v3';
const CACHE_NAME = `cielo-recetas-${APP_VERSION}`;

// Recursos estáticos del "app shell" que se cachean al instalar
// Nota: los archivos de _framework/ se capturan dinámicamente
// durante la primera navegación (ver fetch handler abajo).
const CORE_ASSETS = [
    './',
    './index.html',
    './manifest.webmanifest',
    './icon-512.png',
    './css/app.css',
    './css/bootstrap/bootstrap.min.css',
];

// ============================================================
// INSTALL — Pre-cachear el shell de la app
// ============================================================
self.addEventListener('install', (event) => {
    console.log(`[SW] Instalando cache: ${CACHE_NAME}`);
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll(CORE_ASSETS);
        }).then(() => {
            self.skipWaiting();
        })
    );
});

// ============================================================
// ACTIVATE — Limpiar caches antiguas de versiones anteriores
// ============================================================
self.addEventListener('activate', (event) => {
    console.log(`[SW] Activando: ${CACHE_NAME}`);
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames
                    .filter((name) => name.startsWith('cielo-recetas-') && name !== CACHE_NAME)
                    .map((name) => {
                        console.log(`[SW] Eliminando cache antigua: ${name}`);
                        return caches.delete(name);
                    })
            );
        }).then(() => self.clients.claim())
    );
});

// ============================================================
// FETCH — Estrategia inteligente por tipo de recurso
// ============================================================
self.addEventListener('fetch', (event) => {
    // Ignorar peticiones no-GET (POST, PUT, DELETE para Supabase, etc.)
    if (event.request.method !== 'GET') return;

    const url = new URL(event.request.url);

    // ── 1. API de Supabase → Network Only (datos siempre frescos) ──
    if (url.hostname.includes('supabase.co')) {
        event.respondWith(
            fetch(event.request).catch(() => {
                // Si no hay red, devolver error sin romper el SW
                return new Response(JSON.stringify({ error: 'Sin conexión' }), {
                    status: 503,
                    headers: { 'Content-Type': 'application/json' }
                });
            })
        );
        return;
    }

    // ── 2. Google Fonts → Network con fallback a cache ──
    if (url.hostname.includes('fonts.googleapis.com') || url.hostname.includes('fonts.gstatic.com')) {
        event.respondWith(
            caches.open(CACHE_NAME).then(async (cache) => {
                const cached = await cache.match(event.request);
                if (cached) return cached;
                try {
                    const response = await fetch(event.request);
                    cache.put(event.request, response.clone());
                    return response;
                } catch {
                    return cached || new Response('', { status: 408 });
                }
            })
        );
        return;
    }

    // ── 3. Archivos de Blazor (_framework/) → Cache First ──
    // DLLs, WASM, JS del runtime. Son grandes e inmutables por versión.
    if (url.pathname.startsWith('/_framework/') || url.pathname.startsWith('/._framework/')) {
        event.respondWith(
            caches.open(CACHE_NAME).then(async (cache) => {
                const cached = await cache.match(event.request);
                if (cached) {
                    return cached;
                }
                // No está en caché: descargarlo y guardarlo
                try {
                    const response = await fetch(event.request);
                    if (response.ok) {
                        cache.put(event.request, response.clone());
                    }
                    return response;
                } catch {
                    // Sin red y sin cache: la app no puede cargar
                    return caches.match('./index.html');
                }
            })
        );
        return;
    }

    // ── 4. Demás recursos (app shell, CSS, imágenes) → Cache First ──
    event.respondWith(
        caches.open(CACHE_NAME).then(async (cache) => {
            const cached = await cache.match(event.request);
            if (cached) {
                // Devolver el recurso en caché y actualizar en segundo plano (stale-while-revalidate)
                fetch(event.request)
                    .then((freshResponse) => {
                        if (freshResponse && freshResponse.ok) {
                            cache.put(event.request, freshResponse.clone());
                        }
                    })
                    .catch(() => {}); // Ignorar errores de red en background
                return cached;
            }

            // No está en caché → intentar la red
            try {
                const response = await fetch(event.request);
                if (response.ok) {
                    cache.put(event.request, response.clone());
                }
                return response;
            } catch {
                // Offline y sin cache → fallback al index para que Blazor Router maneje el 404
                const fallback = await caches.match('./index.html');
                return fallback || new Response('Sin conexión', { status: 503 });
            }
        })
    );
});
