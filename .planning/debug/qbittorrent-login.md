---
status: resolved
trigger: Login do qBittorrent recusado apesar de configuração salva
---

# Diagnóstico

Usuário ativou WebUI na porta 8082 e salvou credenciais no painel. C4K apresenta recusa genérica. Não registrar nem alterar credenciais.

Hipóteses: credenciais erradas; bloqueio temporário; proteção de origem; resposta de sucesso diferente de `Ok.` no qBittorrent 5.3.

Evidência: log oficial registra WebAPI login success em loopback nos horários de testes recentes. Código oficial authcontroller.cpp 5.3 usa setStatus(APIStatus::Ok). Investigar corpo de resposta e sessão antes de atribuir falha à senha.

Fluxo gsd-debug inline conforme adaptador Codex; sem subagentes.

## Causa comprovada

Uma tentativa com credenciais já salvas respondeu HTTP 204 vazio, seguida de GET autenticado app/version HTTP 200. Nenhuma senha exibida ou alterada. C4K exigia corpo `Ok.` e rejeitava um login bem-sucedido. Mesmo pressuposto existia em torrents/add.

## Correção

Login passa a comprovar sessão consultando app/version após resposta de sucesso. Add aceita HTTP 204 ou resposta legada 200/Ok. Mantém recusa de 200/Fails e mensagens distintas para bloqueio HTTP 403.

## Validação e entrega

29 testes de lógica/arquivos aprovados, incluindo login 204, login legado e recusa de 200/Fails. 46 contratos HTTP aprovados no executável publicado. Atualizado host mantendo configuração, tokens e um vídeo. Endpoint real /admin/api/automation/test retornou `Conectado: qBittorrent v5.3.0` usando credenciais já salvas, sem exibi-las ou alterá-las. Ajustado Stop-C4K para aguardar saída antes de atualizar binários, após identificar corrida de encerramento na publicação.
