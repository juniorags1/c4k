# Casa4K

## What This Is

Addon Stremio e servidor .NET que prepara filmes e episódios no PC para assistir na TCL C6K com Android TV 14. Ao iniciar um episódio, assegura os dois seguintes na fila e publica somente arquivos completos como fontes locais no Stremio. Instalador Windows sem Docker.

## Core Value

Assistir na TV a um título previamente baixado no PC, pela rede doméstica, sem depender de seeders durante a sessão.

## Requirements

### Validated

Nenhum requisito implementado ou validado no hardware do usuário.

### Active

- [ ] Validar reprodução local 4K e diagnosticar Wi-Fi, codecs, áudio e legendas.
- [ ] Manter lista de desejos própria para filmes e episódios e importar fontes fornecidas pelo usuário.
- [ ] Baixar em segundo plano, recuperar estado após reinício e informar indisponibilidade de fontes.
- [ ] Publicar arquivos completos como fonte Casa4K e catálogo no Stremio.
- [ ] Instalar, iniciar e atualizar sem Docker ou instalação manual de runtime .NET.
- [ ] Investigar reprodução local no Stremio e sincronização de sua biblioteca como capacidades distintas.

### Out of Scope

- Implementar player de TV, mecanismo BitTorrent ou transcodificador próprio: reutilizar componentes maduros.
- Garantir aquisição de torrent sem cópias completas disponíveis: pré-download não cria disponibilidade.
- Streaming de arquivos parciais no MVP: reintroduz dependência da origem durante a sessão.
- Acesso externo, nuvem obrigatória, assinatura de serviços de cache e aplicativo mobile nativo.
- Busca universal em indexadores, extração de senha da conta e reconstrução automática de histórico anterior à instalação.

## Context

Usuário usa Stremio na Google TV e encontra travamentos em vídeos 4K, sobretudo antigos com poucos seeders. Quer adicionar desejos, deixar baixar antecipadamente no PC e assistir pelo Wi-Fi. Pediu pesquisa e planejamento durante sua ausência; autorizou elaboração sem entrevista síncrona. As escolhas abaixo são propostas revisáveis, não respostas fornecidas pelo usuário. Pesquisa executada inline, sem subagentes.

## Constraints

Revisão autorizada em 2026-11-15: usuário escolheu Stremio principal, informou TCL C6K Android TV 14 e Stremio atual sem build numérica. Deseja ambas as opções de fontes, addon configurado e entrada manual. Eventos têm documentação e implementação pública; suporte na TV ainda precisa de prova. Design vigente: [STREMIO-DESIGN.md](STREMIO-DESIGN.md).

- **Backend:** C# / ASP.NET Core .NET 10 LTS, patch suportado fixado ao implementar.
- **Distribuição:** Windows 11 x64 presumido; confirmar sistema do PC. Publicação self-contained e instalador; SDK, Node, Docker e banco externo não serão requisitos de execução.
- **Reprodução:** Stremio na TCL C6K Android TV 14 como cliente principal. Registrar build/player no teste.
- **Disponibilidade:** PC ligado e acordado durante downloads e reprodução; mídia fica no disco local.
- **Rede:** desempenho precisa ser medido entre PC e TV; nenhuma promessa universal de ausência de travamentos.
- **Custo:** priorizar software gratuito e equipamento existente; disco e rede dimensionados após medição.
- **Origem:** arquivos locais, URLs e torrents para os quais o usuário possua autorização de acesso; não incluir credenciais ou fontes reais no repositório.
- **Escopo atual:** usuário autorizou iniciar implementação em 2026-11-16. Fase 1 entrega ferramenta de prova; conclusão exige evidência física da TV.

## Key Decisions

| Decision | Rationale | Outcome |
|---|---|---|
| Download integral antes de publicar | Desacoplar reprodução da internet/seeders | Proposta prioritária |
| Stremio como cliente principal | Preferência explícita do usuário na revisão | Escolhido; gate de transporte/player na fase 1 |
| qBittorrent via Web API | Evitar construir motor de download | Pendente de versão e configuração |
| .NET 10 LTS, SQLite e Razor Pages | Runtime empacotado, baixo número de processos próprios | Proposta |
| Agente em sessão de usuário com início no login | Compatível com uso doméstico e apps de bandeja | Confirmar necessidade sem login |
| Instalador coordenando componentes nativos | Standalone não significa um único processo | Pendente de licenças e empacotamento |
| Eventos player/library e observação HTTP local | Antecipar sem ler credenciais da conta | Gate de eventos e fallback na fase 1 |
| Janela padrão de dois episódios | Dar tempo de preparação com limite de disco | Proposta configurável |
| Addon de fontes e entrada manual | Usuário pediu manter ambas | Escolhido; provedor real ainda aberto |

## Premissas a confirmar

Windows e hardware do PC; armazenamento livre; PC por cabo ou Wi-Fi; roteador; formatos/bitrate; build/player Stremio; addon de fontes; necessidade antes do login. TV confirmada: TCL C6K Android TV 14. Primeira fase prova transporte, player e eventos.

## Evolution

A cada fase, revisar requisitos, mover capacidades comprovadas para Validated e atualizar decisões com evidências. Ao fechar o milestone, revisar escopo, contexto e valor central. Preservar a distinção entre pesquisa documental e testes no ambiente real.

---
*Last updated: 2026-11-15 after Stremio-first research revision*
