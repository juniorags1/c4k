<!-- GSD:project-start source:PROJECT.md -->

## Project

**Casa4K**

Servidor doméstico para preparar filmes e episódios no PC antes de assistir na Google TV. Uma lista de desejos alimenta uma fila persistente de downloads; somente arquivos completos são publicados na biblioteca local. O software próprio será .NET e distribuído por instalador Windows, sem Docker, com reprodução inicialmente pelo Jellyfin e integração Stremio sujeita a prova de compatibilidade.

**Core Value:** Assistir na TV a um título previamente baixado no PC, pela rede doméstica, sem depender de seeders durante a sessão.

### Constraints

- **Backend:** C# / ASP.NET Core .NET 10 LTS, patch suportado fixado ao implementar.
- **Distribuição:** Windows 11 x64 presumido; confirmar sistema do PC. Publicação self-contained e instalador; SDK, Node, Docker e banco externo não serão requisitos de execução.
- **Reprodução:** Jellyfin oficial na Google TV como caminho recomendado; continuar usando exclusivamente Stremio ainda precisa de validação.
- **Disponibilidade:** PC ligado e acordado durante downloads e reprodução; mídia fica no disco local.
- **Rede:** desempenho precisa ser medido entre PC e TV; nenhuma promessa universal de ausência de travamentos.
- **Custo:** priorizar software gratuito e equipamento existente; disco e rede dimensionados após medição.
- **Origem:** arquivos locais, URLs e torrents para os quais o usuário possua autorização de acesso; não incluir credenciais ou fontes reais no repositório.
- **Escopo desta sessão:** pesquisa, requisitos e roadmap; instalação e implementação ficam para execução das fases.

<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->

## Technology Stack

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
- [.NET e suporte LTS](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)
- [Publicação single-file e self-contained](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)
- [ASP.NET Core como serviço Windows](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service)
- [Jellyfin Windows](https://jellyfin.org/docs/general/installation/windows/)
- [Clientes Jellyfin](https://jellyfin.org/downloads.html/)
- [API qBittorrent 5.0+](https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0))
- [Inno Setup](https://jrsoftware.org/isinfo.php)

<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->

## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->

## Architecture

Proposed architecture: [.planning/research/ARCHITECTURE.md](.planning/research/ARCHITECTURE.md). No application code exists yet.
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->

## Project Skills

Project GSD skills are available in `.codex/skills/`; read the selected skill's `SKILL.md` before invoking its workflow.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->

## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `$gsd-fast` for a trivial task inline, with no subagents and no PLAN.md
- `$gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `$gsd-debug` for investigation and bug fixing
- `$gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->

<!-- GSD:profile-start -->

## Developer Profile

> Profile not yet configured. Run `$gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
