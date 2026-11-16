---
status: human_needed
---

# Verificação de implementação

| Capacidade | Evidência |
|---|---|
| Importação → catálogo/stream | HTTP real e formulário no navegador |
| Arquivo completo e original preservado | Hardlink/cópia e falha sem publicação nos testes |
| Retomada após interrupção | Novo worker recupera job Importing e substitui parcial |
| Legenda/token/UTF-8 | HTTP real, token inválido, UTF-8 inválido e retry |
| Revogação de links | Tokens novos persistem; anteriores retornam 404 |
| Configuração de rede/pasta | Persistência em reinício, recusando origem externa/UNC e biblioteca ocupada |
| Dados da instalação existente | Migração SQLite aditiva e verificação após atualização local |
| Legenda/autoplay na TCL | Pendente de teste do usuário |

65 verificações HTTP + 7 de migração/recuperação aprovadas. Revisão inline, sem alegar auditoria independente. Implementação entregue; conclusão de UAT da fase permanece aberta.
