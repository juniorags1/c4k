---
status: implemented-awaiting-uat
---

# Watched → dois próximos

Implementado recebimento de watched com IDs de episódios e intenção SQLite transacional. Um cursor por série retém maior episódio regular observado; repetição e eventos antigos não retrocedem. Worker consulta Cinemeta fora do request, ordena sequência, atravessa temporada e assegura dois sucessores lançados. Data futura/ausente aguarda 6h; erro de metadados aguarda 10min. Planos reapresentados com o mesmo episódio não reativam downloads removidos ou falhos. Ready existente não é baixado novamente.

Marcação sem episódio explícito mostra NeedsEpisode. Especiais da temporada zero não iniciam antecipação. Unwatched não altera downloads nem arquivos. Novo início de reprodução não foi implementado como gatilho. Dois episódios no mesmo hash preservam a união dos índices válidos; nome do pacote usa título da série e episódios. Publicação segue exigindo amount_left=0; exclusão de pacote compartilhado permanece bloqueada.

Painel mostra observação e resultado em Próximos episódios. 49 testes de lógica/arquivos, 53 HTTP no executável self-contained e DOM Edge aprovados. Fixtures incluem mudança de temporada, futuro, fim da lista, dedupe, lote, evento atrasado, resposta antiga, Ready e pacote compartilhado. Nenhum download real de série foi iniciado como teste.

Publicado em artifacts/win-x64 com backup dos dados pequenos em artifacts/watched-backup-*. Preservados manifest, dois filmes e automação ativada. Nenhum watched real recebido após atualização até a verificação final. UAT: usuário marca episódio específico no Stremio, confere evento com changedVideos e solicitações no painel, depois verifica download/conclusão/TV.
