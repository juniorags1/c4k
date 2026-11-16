---
status: awaiting_client_retest
trigger: Failed to fetch ao colar manifest na instalação do addon no Stremio do PC
created: 2026-11-16
updated: 2026-11-16
---

# Diagnóstico inline: instalação do manifest

## Sintomas

Usuário confirmou erro ao colar manifest, não ao abrir vídeo ou botão. Esperado: instalar addon LAN. Stremio shell instalado 5.0.27; WebView2 local 157.0.4341.46. Nenhum token/conta/histórico do Stremio foi lido ou modificado.

## Evidências

- HTTP sem proxy: LAN e loopback retornam 200 com CORS e JSON válido.
- Reproduzido em contexto Edge isolado, carregando web.stremio.com: fetch LAN e loopback falham com permissão negada para address space local/loopback.
- No mesmo contexto de teste, conceder permissões de rede via CDP transformou a resposta LAN em 200 e manifest local.casa4k.probe. Não foram alteradas permissões do navegador/aplicativo real do usuário.
- Bug adicional identificado por código oficial: ShellOpenHandler transforma stremio://host em https://host. O botão Casa4K perdia o esquema HTTP. Esse bug não explica sozinho o erro manual relatado.

## Tratativa

Atualização: reproduzida segunda falha independente no browser com LNA concedido: GET com Content-Type application/json era bloqueado pelo preflight, pois Access-Control-Allow-Headers não incluía Content-Type. Header permitido explicitamente e regressão adicionada. Não comprova que o app real usou esse header; corrige um caso real reproduzido do servidor. Ainda não foi observado o console do processo Stremio do usuário.

Host devolve links de instalação que preservam URL HTTP na rota interna do Stremio, incluindo opção pelo navegador. Painel/README orientam concessão de rede local por site, sem desabilitar segurança global. Dois testes regressivos verificam roundtrip dos links. Nenhum header CORS pode substituir uma permissão de rede negada pelo navegador.

## Limite e próximo passo

Verificação da tratativa: build sem warnings/errors; 26 checks HTTP aprovados tanto em desenvolvimento quanto no binário self-contained. UI real no Edge confirmou que ambos os links preservam HTTP e que a mídia cadastrada permanece disponível. Host atualizado e reiniciado. O teste de permissão foi somente em contexto isolado e não alterou o perfil real do Stremio.

Repetir instalação pelo navegador com permissão de rede local e conta desejada, depois conferir sincronização e teste real no app/TV. Não declarar instalação no cliente do usuário corrigida antes desse reteste.

Fontes: https://github.com/Stremio/stremio-web/blob/development/src/App/ShellOpenHandler.ts ; https://github.com/MicrosoftEdge/WebView2Announcements/issues/126 ; https://support.microsoft.com/en-us/edge/control-a-website-s-access-to-the-local-network-in-microsoft-edge .
