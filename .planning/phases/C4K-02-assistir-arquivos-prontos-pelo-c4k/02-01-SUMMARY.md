---
phase: 02-assistir-arquivos-prontos-pelo-c4k
plan: 01
status: complete
requirements_completed: [ADDN-01, ADDN-02, CONF-01]
---

# C4K 0.2 — biblioteca local

Implementada importação persistente de arquivos completos de outras pastas, com worker, hardlink no mesmo volume e fallback/cópia independente em segundo plano. Arquivo temporário não entra no catálogo; conclusão publica mídia e estado em transação. Repetição da mesma solicitação é idempotente. Falha tem mensagem/retry; reinício reconcilia tarefas interrompidas.

Legendas SRT/VTT UTF-8 de até 5 MB são oferecidas no stream por token de mídia. Painel registra filmes/episódios, acompanha importações, retira itens sem excluir originais e configura endereço da rede, pasta de biblioteca vazia e rotação de acesso. Endereços PC/TV ficam separados. Cadastro/rota antigos continuam funcionando.

## Evidências

- Build da solução sem warnings/errors.
- Smoke HTTP: 65 verificações aprovadas em desenvolvimento e no binário self-contained.
- Testes de migração/recuperação: 7 verificações com SQLite legado, arquivo parcial e novo worker, incluindo preservação do original e cópia independente.
- DOM real no Edge: formulário de importação, campos de série, publicação, links PC/TV e retirada sem apagar origem.
- Atualização local com backup dos arquivos pequenos do banco/configuração; servidor 0.2.0 iniciado, mesma URL do manifest e vídeo anterior preservados. Nenhum link real foi revogado durante atualização.

## Limites

UAT de legendas e autoplay na TV continua pendente. Reprodução básica na TCL já foi confirmada pelo usuário. A conexão aqui configurada é de rede local; integrações com download/provedores permanecem fase 3. Não há busca de fontes, download remoto, antecipação nem limpeza automática de disco nesta versão. Pasta só pode mudar com biblioteca vazia e sem jobs ativos; não mover arquivos grandes implicitamente.
