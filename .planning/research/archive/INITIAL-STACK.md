> Hist?rico da primeira proposta, substitu?do pelo design Stremio principal em 2026-11-15. N?o usar como escopo vigente.

# Stack recomendada

Pesquisa em 2026-11-15. Versões de terceiros serão fixadas por release verificada na implementação; não são dependências já instaladas.

| Componente | Escolha | Por quê | Confiança |
|---|---|---|---|
| Aplicação própria | C#, ASP.NET Core .NET 10 LTS | Backend pedido e distribuição self-contained | Alta |
| Interface | Razor Pages, HTML/CSS e JS pequeno | Interface no navegador, sem runtime frontend separado | Alta |
| Persistência | SQLite + EF Core 10 | Estado local transacional e migrações | Alta |
| Trabalho assíncrono | BackgroundService com jobs persistidos | Poucos downloads; não exige broker ou serviço externo | Alta |
| Downloads torrent | qBittorrent nativo, Web API 5.x compatível | Pausa, retomada, progresso e estado consolidados | Alta documental; versão a fixar |
| Biblioteca/TV | Jellyfin nativo Windows + cliente Android TV | Evita player e transcodificação próprios | Alta documental; TV a validar |
| Instalador | Inno Setup, versão estável a fixar | Atalhos, desinstalação e coordenação de processos | Alta |
| Stremio | Adaptador HTTP implementado em .NET | Protocolo JSON pode ser atendido sem SDK Node | Média; rede/cliente a provar |

Publicação proposta: `dotnet publish -c Release -r win-x64 --self-contained true`. Single-file é opcional: um instalador com diretório de arquivos atende ao pedido e simplifica bibliotecas nativas SQLite. Não ativar trimming/AOT sem testes de compatibilidade. Atualizações self-contained devem renovar também o runtime empacotado.

Aplicação própria inicialmente por usuário, iniciada no login, com navegador como painel. Serviço Windows é alternativa se houver necessidade antes do login, com conta, permissões e processos de terceiros testados. A documentação Jellyfin Windows sinaliza que execução como serviço não é recomendada; não forçar esse modo por padrão.

Não exigir Docker, IIS, SQL Server, Redis, Node ou SDK no computador de uso. Componentes de terceiros continuam sendo processos separados. Primeira versão do instalador pode detectar instalações existentes e orientar configuração; critério final exige experiência coordenada e verificável. Não sobrescrever configurações preexistentes.

Fontes primárias:
- [.NET e suporte LTS](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)
- [Publicação single-file e self-contained](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)
- [ASP.NET Core como serviço Windows](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service)
- [Jellyfin Windows](https://jellyfin.org/docs/general/installation/windows/)
- [Clientes Jellyfin](https://jellyfin.org/downloads.html/)
- [API qBittorrent 5.0+](https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0))
- [Inno Setup](https://jrsoftware.org/isinfo.php)
