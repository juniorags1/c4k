# Evidências da investigação Stremio

Consultadas em 2026-11-15. Documentação/código remoto não constituem teste na TV. Repositórios usam branches mutáveis; registrar novas revisões ao implementar.

| Ref | Fonte primária | Evidência e limite |
|---|---|---|
| S1 | [Protocolo oficial](https://github.com/Stremio/stremio-addon-sdk/blob/master/docs/protocol.md) | Define recursos de dados e eventos player/library. Eventos não fornecem leitura inicial da biblioteca nem recuperação de histórico perdido. |
| S2 | [Implementação de eventos no core](https://github.com/Stremio/stremio-core/blob/development/src/models/common/addon_events.rs) | Constrói eventos e os despacha aos addons compatíveis. Não comprova publicação em todos os aplicativos. |
| S3 | [Commit de introdução](https://github.com/Stremio/stremio-core/commit/f31d53e682fbb892119c587e4cdf257d340bc23b) | GitHub API atribui introdução em 2026-09-29; correções posteriores em outubro. Código recente no core, sem versão Android TV comprovada. |
| S4 | [Player no core](https://github.com/Stremio/stremio-core/blob/development/src/models/player.rs) | Usa addon_events_effects; estado interno não equivale a API pública da conta. |
| S5 | [Stream Object](https://github.com/Stremio/stremio-addon-sdk/blob/master/docs/api/responses/stream.md) | URL direta, dicas de reprodução, continuidade e identificação para legendas. Suporte varia por cliente. |
| S6 | [Manifest](https://github.com/Stremio/stremio-addon-sdk/blob/master/docs/api/responses/manifest.md) | Declara recursos e filtros por tipo/ID; base para addon direto sem Node. |
| S7 | [Uso avançado](https://github.com/Stremio/stremio-addon-sdk/blob/master/docs/advanced.md#proxying-other-addons) | Addon pode consultar endpoints de outro addon. Não descobre automaticamente todos os addons/segredos da conta. |
| S8 | [Exemplo oficial C#](https://github.com/Stremio/addon-helloworld-csharp) | Demonstra implementação fora do SDK JavaScript. Não adotar versões antigas do exemplo como stack atual. |
| S9 | [Addon oficial de arquivos locais](https://github.com/Stremio/stremio-local-addon/blob/master/lib/manifest.js) | Precedente de integração local por catálogo e streams. Não comprova roteamento LAN/HTTPS nesta TV. |
| S10 | [API qBittorrent 5.x](https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0)) | Integração de downloads e seleção de arquivos; negociar versão da API antes de usar operações. |
| S11 | [ASP.NET Core PhysicalFile](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.physicalfile) | Suporte a enableRangeProcessing para servir arquivos com seek. Validar headers e autenticação. |

Registro GitHub API para S2: f31d53e (2026-09-29, introdução); 42738c9 (2026-10-02, evitar eventos unchanged); 1e45149 (2026-10-02, lotes watched). A busca pública não identificou evidência suficiente para fixar versão mínima Android TV. Não concluir ausência de recurso apenas por essa falta de evidência.

## Correção das premissas anteriores

A primeira pesquisa dizia que o protocolo consultado não comprovava eventos. A leitura aprofundada encontrou suporte documental e implementação pública. A resposta seguinte destacou essa possibilidade; a conclusão precisa permanecer condicional à versão real do cliente. O novo plano substitui a arquitetura centrada em Jellyfin e trata o addon como entrega principal, com prova de reprodução e eventos na primeira fase.
