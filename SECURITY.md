# Segurança

O C4K foi projetado para uso em rede doméstica. Não exponha as portas do servidor diretamente à internet. O painel administrativo fica em loopback; o addon e a mídia são acessíveis pela rede local.

As URLs de pareamento dão acesso ao addon e aos vídeos. Não publique essas URLs, capturas contendo os links, credenciais do qBittorrent ou o conteúdo de `%LOCALAPPDATA%\C4K`. Para revogar URLs expostas, gere novos links no painel e reinstale o addon nos clientes.

As credenciais do qBittorrent usam DPAPI do usuário Windows. Mantenha a Interface Web do qBittorrent restrita ao PC. O C4K precisa acessar essa interface para controlar tarefas e verificar se uma exclusão é segura.

Antes de compartilhar logs ou abrir uma issue, remova tokens, credenciais, caminhos pessoais e fontes privadas. Não publique detalhes de uma vulnerabilidade que exponham dados de uma instalação. Se o repositório oferecer o recurso **Report a vulnerability**, use esse canal privado.

A limpeza de assistidos exclui arquivos físicos após 72 horas de um evento `watched` recebido. Consulte os agendamentos no painel; marcar como `unwatched` cancela a limpeza pendente.
