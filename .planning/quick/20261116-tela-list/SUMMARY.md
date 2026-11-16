---
status: complete
---

# Biblioteca separada em /list

Criada LibraryList.html servida em /list na porta administrativa loopback 48241. Página principal contém navegação para biblioteca e não renderiza listagem/cadastro local ou tarefas Ready/Removed como fila ativa. Nova tela reúne capa, título, episódio, nome/tamanho/disponibilidade, busca, filtro, ordenação, exclusão confirmada e cadastro direto sem cópia.

API /admin/api/library informa somente dados da biblioteca, sem tokens/configurações. Total lógico deduplica caminhos e trata arquivo indisponível. Middleware protege /list por porta, loopback e Host, com API exigindo header/origem existentes. Exclusão reutiliza backend e proteções anteriores, sem alterações de lógica destrutiva.

Build Release e 61 contratos HTTP aprovados. Edge real comprovou navegação, tamanhos, busca/episódio, filtros/ordenação, títulos seguros via textContent, cadastro e confirmação/cancelamento da exclusão. Exclusão visual testada com API simulada, sem tocar mídia real. Publicação preservou manifest e todos os registros existentes. Cadastro/exclusão movidos para /list, mantendo automação em /admin/.
