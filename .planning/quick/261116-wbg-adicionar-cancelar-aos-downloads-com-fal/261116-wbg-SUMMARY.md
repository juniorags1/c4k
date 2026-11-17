---
status: complete
---

# Cancelar falhas da fila

Botão Cancelar ao lado de Tentar novamente para downloads Failed. Endpoint POST /admin/api/downloads/cancel exige proteções administrativas existentes e sincroniza com worker. Persiste Cancelled, limpa erro e oculta item da fila, mantendo arquivos e tarefas do qBittorrent. Idempotente para Cancelled; rejeita tarefa ativa e inexistente. Eventos automáticos não reativam o item, enquanto pedido manual explícito pode reenfileirar.

78 verificações de lógica e 78 contratos HTTP aprovados, incluindo testes no executável publicado. Cobertura: persistência após reinício, dedupe, proteção de tarefas ativas, solicitação manual, autenticação do endpoint e presença do botão. Sem teste visual em navegador. Publicação atualizada preservando manifest, IDs de biblioteca e estados de todos os downloads reais; nenhum cancelamento real aplicado.
