# Tela dedicada de biblioteca /list

GSD quick inline. Mover listagem, exclusão e cadastro de arquivos completos da página principal para /list na porta administrativa loopback. Manter links de navegação entre automação e biblioteca. Reutilizar API de exclusão física e proteções existentes; não alterar mídia ou dados na implantação.

API administrativa somente de biblioteca fornece tamanho/disponibilidade/capa e total lógico de arquivos, deduplicando caminhos iguais no total. UI com busca, filtro filme/série, ordenação e confirmação explícita de exclusão; títulos por textContent. Página principal mostra tarefas ativas/falhas, não repete biblioteca pronta. Validar proteção de /list na LAN, contratos HTTP e navegação/formulário/confirmar/cancelar no Edge com fixtures. Publicar preservando manifest e biblioteca.
