---
status: complete
---

# Limpeza automática de assistidos

Migração aditiva watched_cleanup registra novos eventos library watched individuais para filmes e episódios, independentemente do toggle de downloads. Prazo de 72h UTC desde a primeira marcação; duplicatas não adiam. Unwatched remove marcação/agendamento; novo watched inicia novo prazo. Série genérica e IDs de outra série não causam exclusão. Sem importação retroativa de eventos antigos.

Worker verifica pendências no ciclo de 5 segundos, inclusive após reiniciar, reutilizando exclusão física protegida e removendo catálogo/tarefa gerenciada, sem deleteFiles global no qBittorrent. Legendas locais cadastradas e não compartilhadas também são removidas. Eventos e exclusões usam o mesmo semáforo. Erros persistem com nova tentativa em 1h, sem impedir downloads. Arquivo em uso, tarefa externa e arquivo compartilhado são preservados. Pacote de vários episódios aguarda todos os outros itens gerenciados estarem Ready e com prazo vencido, antes de remover tarefa sem apagar demais arquivos.

Painel explica regra e exibe prazos e falhas. Funciona somente quando o cliente envia eventos individuais; não presume assistido a partir de reprodução ou série genérica. Não houve prova de evento watched na TV nesta tarefa. Agendamento pode existir antes do arquivo chegar; exclusão aguarda cadastro local.

Validação: 73 checks de lógica, migração, automação e arquivos com fixtures isoladas; 74 contratos HTTP, também no self-contained publicado. Inclui prazo exato, antes do vencimento, dedupe, unwatched, reinício, independência de automação, arquivo ocupado e repetição, preservação de não assistidos e pacote compartilhado. Publicação preservou IDs da biblioteca, manifest e diretórios; nenhuma exclusão real ou agendamento retroativo executado. Sem teste visual em navegador.
