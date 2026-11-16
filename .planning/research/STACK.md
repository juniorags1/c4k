# Stack vigente

| Componente | Escolha | Confian?a |
|---|---|---|
| Host/addon/m?dia/painel | C# e ASP.NET Core .NET 10 LTS | Alta documental |
| UI | Razor Pages e JS pequeno | Alta |
| Persist?ncia | SQLite e EF Core 10 | Alta; vers?o patch fixada ao implementar |
| Jobs | BackgroundService e inten??es/inbox persistidos | Proposta a testar |
| Download | qBittorrent nativo por Web API em loopback | Alta documental; vers?o negociada |
| TV | Stremio na TCL C6K Android TV 14 | Reprodu??o LAN e eventos pendentes |
| Instalador | Inno Setup, app self-contained win-x64 | Proposta; VM limpa na fase 6 |
| Jellyfin | Opcional, sem depend?ncia no caminho principal | Fora do MVP obrigat?rio |

Publica??o proposta: dotnet publish -c Release -r win-x64 --self-contained true. Single-file opcional; n?o confundir instalador com processo ?nico. N?o ativar trimming/AOT sem comprovar SQLite/EF/UI. Runtime empacotado precisa ser renovado nos updates.

Sem Node, SDK, Docker, IIS ou banco externo no PC de uso. C?digo do protocolo direto em .NET, com [exemplo oficial C#](https://github.com/Stremio/addon-helloworld-csharp) apenas como refer?ncia, sem herdar suas vers?es antigas. Evid?ncias e demais fontes em [STREMIO-SOURCES.md](STREMIO-SOURCES.md). Fontes de deploy: [Microsoft](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview), [Inno Setup](https://jrsoftware.org/isinfo.php).
