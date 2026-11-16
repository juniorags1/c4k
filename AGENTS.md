<!-- GSD:project-start source:PROJECT.md -->

## Project

**C4K**

Addon Stremio e servidor .NET que prepara filmes e episódios no PC para assistir na TCL C6K com Android TV 14. Ao iniciar um episódio, assegura os dois seguintes na fila e publica somente arquivos completos como fontes locais no Stremio. Instalador Windows sem Docker.

**Core Value:** Assistir na TV a um título previamente baixado no PC, pela rede doméstica, sem depender de seeders durante a sessão.

### Constraints

Revisão autorizada em 2026-11-15: usuário escolheu Stremio principal, informou TCL C6K Android TV 14 e Stremio atual sem build numérica. Deseja ambas as opções de fontes, addon configurado e entrada manual. Eventos têm documentação e implementação pública; suporte na TV ainda precisa de prova. Design vigente: [STREMIO-DESIGN.md](STREMIO-DESIGN.md).

- **Backend:** C# / ASP.NET Core .NET 10 LTS, patch suportado fixado ao implementar.
- **Distribuição:** Windows 11 x64 presumido; confirmar sistema do PC. Publicação self-contained e instalador; SDK, Node, Docker e banco externo não serão requisitos de execução.
- **Reprodução:** Stremio na TCL C6K Android TV 14 como cliente principal. Registrar build/player no teste.
- **Disponibilidade:** PC ligado e acordado durante downloads e reprodução; mídia fica no disco local.
- **Rede:** desempenho precisa ser medido entre PC e TV; nenhuma promessa universal de ausência de travamentos.
- **Custo:** priorizar software gratuito e equipamento existente; disco e rede dimensionados após medição.
- **Origem:** arquivos locais, URLs e torrents para os quais o usuário possua autorização de acesso; não incluir credenciais ou fontes reais no repositório.
- **Escopo atual:** usuário autorizou continuar após reprodução básica validada na TCL. Biblioteca local 0.2 implementada; UAT de legendas/autoplay e critérios detalhados da fase 1 seguem pendentes.

<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->

## Technology Stack

| Componente | Escolha | Confiança |
|---|---|---|
| Host/addon/mídia/painel | C# e ASP.NET Core .NET 10 LTS | Alta documental |
| UI | HTML/CSS/JS servido pelo ASP.NET Core | Implementado |
| Persistência | SQLite e Microsoft.Data.Sqlite 10.0.15 | Implementado; migrações aditivas |
| Jobs | BackgroundService e intenções/inbox persistidos | Proposta a testar |
| Download | qBittorrent nativo por Web API em loopback | Alta documental; versão negociada |
| TV | Stremio na TCL C6K Android TV 14 | Reprodução LAN e eventos pendentes |
| Instalador | Inno Setup, app self-contained win-x64 | Proposta; VM limpa na fase 6 |
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->

## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->

## Architecture

Architecture: [.planning/STREMIO-DESIGN.md](.planning/STREMIO-DESIGN.md). The host in src/C4K.App implements local library imports, SQLite jobs and sidecar subtitles. Run scripts/Smoke-Test.ps1 for HTTP contracts and tests/C4K.LibraryTests for migration/recovery. Downloading and episode preloading are not implemented yet.
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->

## Project Skills

Project GSD skills are in .codex/skills/. Read the selected SKILL.md before its workflow.
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
