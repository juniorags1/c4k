# Recomendação executiva

Começar com qBittorrent e Jellyfin instalados nativamente no PC, com biblioteca de arquivos completos, e o app Jellyfin na Google TV. Validar um vídeo representativo antes de desenvolver. Construir Casa4K .NET apenas para simplificar desejos, fila, disco e instalação. Se Radarr/Sonarr atenderem ao fluxo desejado com configuração aceitável, adotá-los pode evitar código próprio.

Fluxo alvo: adicionar desejo no painel → informar fonte ou aguardar uma → baixar no PC → verificar → publicar no Jellyfin → assistir pela LAN. Poucos seeders deixam de afetar a sessão depois da conclusão; ainda podem atrasar ou impedir a preparação. Wi-Fi, codecs, legendas e PC acordado continuam sendo condições.

Stremio tem duas necessidades independentes: reproduzir uma URL local por addon e ler a lista pessoal da conta. A primeira tem suporte documental de protocolo, mas rede/autenticação precisam ser provadas na TV; a segunda não foi comprovada nas APIs de addons consultadas. Não vender a lista própria como sincronização já resolvida. Roadmap inclui investigação explícita e decisão documentada.

Instalador Windows inclui aplicação self-contained e coordena componentes nativos; não implica que Jellyfin e qBittorrent estejam embutidos num único executável .NET. Antes de redistribuir binários, registrar versões, licenças, procedência, hashes e avisos; alternativa é instalador obter artefatos oficiais verificados. Operação local não exige internet após preparação; instalação inicial e aquisição podem exigir.

Confiança alta em download completo + Jellyfin nativo; média na experiência final do instalador; pendente em hardware/Wi-Fi e integração de conta Stremio. Nenhum teste de TV ou instalação foi executado nesta sessão.

Detalhes e fontes: [STACK.md](STACK.md), [FEATURES.md](FEATURES.md), [ARCHITECTURE.md](ARCHITECTURE.md), [PITFALLS.md](PITFALLS.md).
