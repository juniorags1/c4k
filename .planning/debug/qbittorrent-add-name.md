---
status: resolved
trigger: Filme falhou com qBittorrent recusou o magnet; mostrar título em vez de IMDb
---

libraryAdd de tt12300742 recebido. Fonte resolvida, hash persistido, estado Failed antes da seleção. Investigar resposta de torrents/add e existência por hash antes de reenviar. Resolver título público e usar nome da obra no painel e nome de exibição do torrent sem renomear arquivos físicos. Fluxo gsd-debug inline conforme adaptador Codex.

## Causa e correção

qBittorrent 5.3 retorna JSON com success_count, failure_count, pending_count e added_torrent_ids em torrents/add. O cliente ainda esperava Ok. ou 204. A tarefa existia com nome igual ao hash e estado stoppedDL, mas o C4K havia marcado Failed.

Cliente agora aceita contadores positivos de sucesso ou pendência, mantendo rejeição de resultado sem sucesso. Resolve título público pelo Cinemeta com timeout e fallback ao identificador se indisponível. Nome segue em dn, rename no add e rename de tarefa própria existente; não altera os arquivos físicos. Séries incluem temporada/episódio no nome de exibição. Estado de obtenção de metadados fica explícito também após retry.

## Validação

37 verificações de lógica/arquivos e 46 HTTP aprovadas. Título real: Bugonia (2025). Publicado executável, preservado manifest e retomada solicitação existente por hash. Consulta real confirmou exatamente uma tarefa C4K, nome Bugonia (2025), estado downloading. C4K: Downloading, selected=true, sem erro, progresso 0,0044357 (0,44%). Conclusão e reprodução dessa transferência na TV ainda não foram verificadas.
