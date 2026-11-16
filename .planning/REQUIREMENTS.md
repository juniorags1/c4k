# Requirements: Casa4K

**Defined:** 2026-11-15
**Core Value:** Assistir conteúdo preparado no PC pela rede local sem depender de seeders na sessão.

Escopo proposto a partir do pedido e pesquisa, sem validação de hardware. Lista própria é o ponto de entrada de v1; lista pessoal do Stremio tem investigação explícita e não está prometida como sincronização entregue.

## v1 Requirements

### Validação doméstica

- [ ] **PLAY-01**: Usuário reproduz arquivo 4K completo no Jellyfin da Google TV, com registro de codec, áudio, legenda e modo de reprodução.
- [ ] **PLAY-02**: Usuário recebe diagnóstico PC–TV e comprova 60 minutos sem rebuffer após início, três seeks em até cinco segundos e reprodução com WAN desligada/LAN ativa, para arquivo representativo.

### Desejos e configuração

- [ ] **WISH-01**: Usuário adiciona, lista e remove desejos de filme ou série com escopo de temporada/episódio, sem duplicatas de identidade.
- [ ] **WISH-02**: Usuário associa magnet, torrent autorizado ou URL HTTP(S) permitida e importa arquivo local; desejo sem fonte mostra AguardandoFonte.
- [ ] **WISH-03**: Usuário define prioridade, preferência de qualidade/idioma e limite de tamanho; preferências não garantem disponibilidade de fonte.
- [ ] **CONF-01**: Usuário configura pastas e conexões por painel local autenticado, sem editar código; segredos não aparecem em logs.

### Preparação

- [ ] **DOWN-01**: Usuário acompanha fila persistente e progresso real via qBittorrent, com seleção explícita de arquivos para episódios em torrent multifile.
- [ ] **DOWN-02**: Usuário pausa, retoma e cancela jobs; reinício reconcilia jobs sem duplicar downloads ou perder desejos.
- [ ] **DOWN-03**: Usuário distingue sem progresso, falha, falta de espaço e fonte ausente, podendo substituir fonte sem publicação falsa de conteúdo pronto.
- [ ] **DOWN-04**: Downloads HTTP(S) retomam quando suportado pela origem, verificam conclusão e rejeitam destinos/redirects não permitidos; origem sem retomada reinicia de modo explícito.

### Biblioteca e disco

- [ ] **LIBR-01**: Usuário vê apenas arquivos completos e verificados publicados em biblioteca separada dos incompletos, com identidade e nomes adequados ao Jellyfin.
- [ ] **LIBR-02**: Usuário vê estado de indexação Jellyfin separado do download; falha de atualização permite retry sem baixar novamente.
- [ ] **DISK-01**: Usuário define teto e reserva de disco; aplicação bloqueia novos jobs/importações sem capacidade e considera cópia adicional e hardlinks.
- [ ] **DISK-02**: Usuário remove mídia por ação explícita com impacto visível em biblioteca e download/seeding; não há exclusão automática de título não assistido por padrão.

### Integração Stremio: decisão de viabilidade

- [ ] **STRE-01**: Usuário recebe prova documentada de addon local no cliente Google TV, incluindo instalação, transporte, autenticação, URL de arquivo pronto e seek; se inviável, recebe motivo e fallback Jellyfin explícitos.
- [ ] **STRE-02**: Usuário recebe decisão fundamentada sobre detectar sua lista pessoal Stremio: API/autorização suportada comprovada ou limitação documentada e alternativa por lista própria. Não equivale à sincronização implementada.

### Distribuição

- [ ] **DIST-01**: Usuário instala Casa4K em Windows x64 limpo por instalador, sem Docker, SDK ou runtime .NET pré-instalado, com detecção/configuração dos componentes nativos.
- [ ] **DIST-02**: Usuário inicia pelo menu e no login, vê estado dos componentes e orientação de energia/rede; firewall limita portas à rede privada e qBittorrent permanece em loopback.
- [ ] **DIST-03**: Usuário atualiza/desinstala sem apagar mídia e desejos por padrão; versões, procedência e licenças dos componentes são registradas, com backup de dados antes de migração.

## v2 Requirements

- **SYNC-01**: Sincronizar lista Stremio somente se STRE-02 comprovar interface estável e autorizada.
- **ADDN-01**: Entregar addon de reprodução e catálogo Stremio completo se STRE-01 passar; manter cliente Jellyfin alternativo.
- **AUTO-01**: Buscar fontes automaticamente e monitorar novos episódios por adaptadores Radarr/Sonarr ou provedores suportados.
- **META-01**: Busca de metadados com capas e identificação assistida por provedor documentado.
- **NOTF-01**: Notificar conclusão e permitir horários de download.
- **SERV-01**: Operar antes do login por serviço Windows se necessário e comprovado com dependências.

## Out of Scope

Player/transcodificador/BitTorrent próprios; garantia de torrent sem seeds; acesso remoto; redistribuição pública de mídia; serviços de cache pagos obrigatórios; streaming parcial; compra de equipamento antes de medição.

## Traceability

| Requirement | Phase | Status |
|---|---|---|
| PLAY-01 | 1 | Pending |
| PLAY-02 | 1 | Pending |
| WISH-01 | 2 | Pending |
| WISH-02 | 2 | Pending |
| WISH-03 | 2 | Pending |
| CONF-01 | 2 | Pending |
| DOWN-01 | 3 | Pending |
| DOWN-02 | 3 | Pending |
| DOWN-03 | 3 | Pending |
| DOWN-04 | 3 | Pending |
| LIBR-01 | 4 | Pending |
| LIBR-02 | 4 | Pending |
| DISK-01 | 4 | Pending |
| DISK-02 | 4 | Pending |
| STRE-01 | 5 | Pending |
| STRE-02 | 5 | Pending |
| DIST-01 | 6 | Pending |
| DIST-02 | 6 | Pending |
| DIST-03 | 6 | Pending |

**Coverage:** 19 requisitos v1, 19 mapeados, 0 sem fase. Critérios de Stremio são entregas de investigação; não contabilizar como recurso funcional implementado.

---
*Last updated: 2026-11-15 after initial definition*
