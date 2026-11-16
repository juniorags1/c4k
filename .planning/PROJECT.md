# Casa4K

## What This Is

Servidor doméstico para preparar filmes e episódios no PC antes de assistir na Google TV. Uma lista de desejos alimenta uma fila persistente de downloads; somente arquivos completos são publicados na biblioteca local. O software próprio será .NET e distribuído por instalador Windows, sem Docker, com reprodução inicialmente pelo Jellyfin e integração Stremio sujeita a prova de compatibilidade.

## Core Value

Assistir na TV a um título previamente baixado no PC, pela rede doméstica, sem depender de seeders durante a sessão.

## Requirements

### Validated

Nenhum requisito implementado ou validado no hardware do usuário.

### Active

- [ ] Validar reprodução local 4K e diagnosticar Wi-Fi, codecs, áudio e legendas.
- [ ] Manter lista de desejos própria para filmes e episódios e importar fontes fornecidas pelo usuário.
- [ ] Baixar em segundo plano, recuperar estado após reinício e informar indisponibilidade de fontes.
- [ ] Publicar apenas arquivos completos no Jellyfin.
- [ ] Instalar, iniciar e atualizar sem Docker ou instalação manual de runtime .NET.
- [ ] Investigar reprodução local no Stremio e sincronização de sua biblioteca como capacidades distintas.

### Out of Scope

- Implementar player de TV, mecanismo BitTorrent ou transcodificador próprio: reutilizar componentes maduros.
- Garantir aquisição de torrent sem cópias completas disponíveis: pré-download não cria disponibilidade.
- Streaming de arquivos parciais no MVP: reintroduz dependência da origem durante a sessão.
- Acesso externo, nuvem obrigatória, assinatura de serviços de cache e aplicativo mobile nativo.
- Busca universal automática em indexadores e sincronização privada de conta Stremio no MVP.

## Context

Usuário usa Stremio na Google TV e encontra travamentos em vídeos 4K, sobretudo antigos com poucos seeders. Quer adicionar desejos, deixar baixar antecipadamente no PC e assistir pelo Wi-Fi. Pediu pesquisa e planejamento durante sua ausência; autorizou elaboração sem entrevista síncrona. As escolhas abaixo são propostas revisáveis, não respostas fornecidas pelo usuário. Pesquisa executada inline, sem subagentes.

## Constraints

- **Backend:** C# / ASP.NET Core .NET 10 LTS, patch suportado fixado ao implementar.
- **Distribuição:** Windows 11 x64 presumido; confirmar sistema do PC. Publicação self-contained e instalador; SDK, Node, Docker e banco externo não serão requisitos de execução.
- **Reprodução:** Jellyfin oficial na Google TV como caminho recomendado; continuar usando exclusivamente Stremio ainda precisa de validação.
- **Disponibilidade:** PC ligado e acordado durante downloads e reprodução; mídia fica no disco local.
- **Rede:** desempenho precisa ser medido entre PC e TV; nenhuma promessa universal de ausência de travamentos.
- **Custo:** priorizar software gratuito e equipamento existente; disco e rede dimensionados após medição.
- **Origem:** arquivos locais, URLs e torrents para os quais o usuário possua autorização de acesso; não incluir credenciais ou fontes reais no repositório.
- **Escopo desta sessão:** pesquisa, requisitos e roadmap; instalação e implementação ficam para execução das fases.

## Key Decisions

| Decision | Rationale | Outcome |
|---|---|---|
| Download integral antes de publicar | Desacoplar reprodução da internet/seeders | Proposta prioritária |
| Jellyfin para biblioteca e TV | Instalador Windows e cliente Android TV existentes | Pendente de teste real |
| qBittorrent via Web API | Evitar construir motor de download | Pendente de versão e configuração |
| .NET 10 LTS, SQLite e Razor Pages | Runtime empacotado, baixo número de processos próprios | Proposta |
| Agente em sessão de usuário com início no login | Compatível com uso doméstico e apps de bandeja | Confirmar necessidade sem login |
| Instalador coordenando componentes nativos | Standalone não significa um único processo | Pendente de licenças e empacotamento |
| Lista própria primeiro | Não presumir acesso público estável à lista Stremio | Proposta com alternativa documentada |
| Prova Stremio explícita no roadmap | Preservar preferência sem prometer integração não verificada | Pendente |

## Premissas a confirmar

Modelo da TV; Windows e CPU/GPU/RAM do PC; armazenamento livre; PC por cabo ou Wi-Fi; roteador e faixa usada; formatos/bitrate típicos; necessidade de manter somente Stremio; local desejado da lista; necessidade de executar antes do login. A primeira fase coleta esses dados e pode reduzir código próprio se ferramentas existentes forem suficientes.

## Evolution

A cada fase, revisar requisitos, mover capacidades comprovadas para Validated e atualizar decisões com evidências. Ao fechar o milestone, revisar escopo, contexto e valor central. Preservar a distinção entre pesquisa documental e testes no ambiente real.

---
*Last updated: 2026-11-15 after initialization*
