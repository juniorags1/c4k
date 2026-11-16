# Stack vigente

| Componente | Escolha | Confiança |
|---|---|---|
| Host/addon/mídia/painel | C# e ASP.NET Core .NET 10 LTS | Alta documental |
| UI | HTML/CSS/JS servido pelo ASP.NET Core | Implementado na biblioteca 0.2 |
| Persistência | SQLite e Microsoft.Data.Sqlite 10.0.15 | Implementado; migrações aditivas verificadas |
| Jobs | BackgroundService e intenções/inbox persistidos | Proposta a testar |
| Download | qBittorrent nativo por Web API em loopback | Alta documental; versão negociada |
| TV | Stremio na TCL C6K Android TV 14 | Reprodução LAN e eventos pendentes |
| Instalador | Inno Setup, app self-contained win-x64 | Proposta; VM limpa na fase 6 |

Publicação proposta: dotnet publish -c Release -r win-x64 --self-contained true. Single-file opcional; não confundir instalador com processo único. Não ativar trimming/AOT sem comprovar SQLite/EF/UI. Runtime empacotado precisa ser renovado nos updates.

Sem Node, SDK, Docker, IIS ou banco externo no PC de uso. Código do protocolo direto em .NET, com [exemplo oficial C#](https://github.com/Stremio/addon-helloworld-csharp) apenas como referência, sem herdar suas versões antigas. Evidências e demais fontes em [STREMIO-SOURCES.md](STREMIO-SOURCES.md). Fontes de deploy: [Microsoft](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview), [Inno Setup](https://jrsoftware.org/isinfo.php).
