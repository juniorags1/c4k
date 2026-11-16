# Pesquisa

- API qBittorrent 5.x: https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0)
- Implementação 5.3.0 verificada: https://github.com/qbittorrent/qBittorrent/blob/release-5.3.0/src/webui/api/torrentscontroller.cpp
- Add aceita savepath, downloadPath, useDownloadPath e stopped. Torrents novos recebem caminhos individuais, sem modificar preferências globais. Magnets seguem pela API; pasta monitorada não equivale a leitor de arquivos texto contendo magnets.
- Login usa SID e Referer da mesma origem. Porta 8082 respondeu 403 sem autenticação nesta máquina. Não tentar senhas nem reiniciar qBittorrent.
- Torrentio fornece streams com infoHash e fileIdx. Seeds e resolução vêm dos rótulos; selecionar apenas contagens interpretáveis. Índice identifica o vídeo de um pacote, não supor maior arquivo.
- Arquivos só entram no catálogo após confirmação de conclusão e caminho final dentro de done. Validação de caminhos existente recusa traversal e reparse points.

Pesquisa, planejamento e revisão executados inline conforme adaptador Codex da skill; sem revisão independente por subagente.
