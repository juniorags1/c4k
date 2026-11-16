---
status: complete
---

# Pastas configuráveis no painel

Pasta de incompletos editável na configuração de automação, persistida em automation.json e enviada como downloadPath para novos torrents. Campo opcional mantém compatibilidade com clientes anteriores e preserva credenciais. Pasta de completos editável em formulário próprio, usando library.json e atualização imediata de MediaFiles.

Validação compartilhada exige pasta local absoluta abaixo da raiz do disco, sem UNC, junctions ou links. Troca da pasta de completos sincroniza com worker e cadastro, bloqueia tarefas pendentes e exige vídeos e legendas cadastrados no destino. Com arquivo original presente, verifica também tamanho. Não move arquivos ou torrents existentes; usuário ajusta torrents antigos no qBittorrent.

Verificação: 60 testes de lógica/recuperação/automação e 70 contratos HTTP aprovados. HTTP comprova alteração de raiz com biblioteca existente, rejeição de destino sem arquivos, persistência após reinício, bloqueio com tarefa pendente e campos do painel. Publicação self-contained atualizada; mesmos IDs da biblioteca, manifest e diretórios atuais comprovados após reinício. Nenhum diretório real alterado e nenhum torrent real movido. Não houve teste visual em navegador.
