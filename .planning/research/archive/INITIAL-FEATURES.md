> Histórico da primeira proposta, substituído pelo design Stremio principal em 2026-11-15. Não usar como escopo vigente.

# Opções e escopo funcional

## Comparação

| Caminho | Trabalho próprio | Instalação | Adequação |
|---|---|---|---|
| qBittorrent + servidor local | Nenhum | Instaladores nativos separados | Melhor prova inicial: baixar e assistir localmente |
| Radarr + Sonarr + qBittorrent + servidor local | Pouco ou nenhum | Componentes nativos Windows | Bom se aceitar painéis separados e configurar provedores; pode dispensar C4K |
| C4K .NET + qBittorrent + servidor local | Moderado | Instalador coordenado | Escolhido para lista unificada, status e fluxo doméstico simples |
| C4K + addon Stremio | Maior | Mesmo PC, instalação do addon na conta/TV | Preserva player preferido se teste LAN e protocolo passar |
| Cache remoto/debrid | Pouco | Depende de terceiro e conta | Não atende plenamente a servidor local standalone; cache não garante título raro |

Radarr já oferece seleção/perfis de filmes; Sonarr cobre séries e episódios. Ambos oferecem distribuição Windows. Não acrescentar esses dois como dependências obrigatórias se a versão inicial aceitar fontes explícitas e apenas coordenar os downloads. Se busca automática completa virar prioridade, preferir seus adaptadores a reimplementar indexadores.

## Essencial para v1

Lista própria com identidade de filme ou série/temporada/episódio; fonte fornecida; perfis de preferência e limite de tamanho; fila com retomada; estados honestos; importação integral; publicação servidor local; proteção de espaço; configuração por navegador; instalador e diagnóstico. Um desejo sem fonte permanece aguardando fonte, nunca simula download automático.

## Diferenciais posteriores

Catálogo de prontos no Stremio; fonte local associada a IMDb/episódio; monitoramento de novos episódios; integração com listas externas documentadas; notificações; múltiplos perfis e retenção por histórico.

## Não construir inicialmente

Player, transcodificador, indexador universal, cliente BitTorrent, sincronização de credenciais Stremio por engenharia reversa, promessa de recuperar torrent morto. Lista de desejos de conta Stremio não é equivalente a catálogo de addon: documentação consultada descreve recursos de catálogo e stream, não comprovou eventos de alteração da lista pessoal.

Fontes: [Radarr](https://radarr.video/), [Sonarr](https://sonarr.tv/), [catálogo Stremio](https://stremio.github.io/stremio-addon-sdk/api/requests/defineCatalogHandler.html), [streams Stremio](https://stremio.github.io/stremio-addon-sdk/api/requests/defineStreamHandler.html). Comparações de esforço são avaliação de arquitetura, não benchmarks.
