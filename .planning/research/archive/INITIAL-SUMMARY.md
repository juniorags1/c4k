> Histórico da primeira proposta, substituído pelo design Stremio principal em 2026-11-15. Não usar como escopo vigente.

# Recomendação executiva

Começar com arquivos completos no PC e validar reprodução pelo Stremio na Google TV. Casa4K .NET administra desejos, fila, disco, instalação e publicação local; qBittorrent realiza downloads.

Fluxo alvo: adicionar desejo no painel → informar fonte ou aguardar uma → baixar no PC → verificar → publicar no servidor local → assistir pela LAN. Poucos seeders deixam de afetar a sessão depois da conclusão; ainda podem atrasar ou impedir a preparação. Wi-Fi, codecs, legendas e PC acordado continuam sendo condições.

Stremio tem duas necessidades independentes: reproduzir uma URL local por addon e ler a lista pessoal da conta. A primeira tem suporte documental de protocolo, mas rede/autenticação precisam ser provadas na TV; a segunda não foi comprovada nas APIs de addons consultadas. Não vender a lista própria como sincronização já resolvida. Roadmap inclui investigação explícita e decisão documentada.

Instalador Windows inclui aplicação self-contained e coordena componentes nativos; não implica que servidor local e qBittorrent estejam embutidos num único executável .NET. Antes de redistribuir binários, registrar versões, licenças, procedência, hashes e avisos; alternativa é instalador obter artefatos oficiais verificados. Operação local não exige internet após preparação; instalação inicial e aquisição podem exigir.

Confiança alta em download completo + servidor local nativo; média na experiência final do instalador; pendente em hardware/Wi-Fi e integração de conta Stremio. Nenhum teste de TV ou instalação foi executado nesta sessão.

Detalhes e fontes: [STACK.md](STACK.md), [FEATURES.md](FEATURES.md), [ARCHITECTURE.md](ARCHITECTURE.md), [PITFALLS.md](PITFALLS.md).
