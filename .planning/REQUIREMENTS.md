# Requirements: C4K é addon Stremio

**Revised:** 2026-11-15

**Core Value:** Assistir conteúdo preparado no PC pelo Stremio sem depender de seeders durante a sessão.

Alvo confirmado: TCL C6K Android TV 14, Stremio atual sem build numérica. Usuário escolheu manter addon de fontes e entrada manual. Nenhuma capacidade implementada. IDs anteriores preservados com escopo atualizado; Design vigente: [STREMIO-DESIGN.md](STREMIO-DESIGN.md).

## v1 Requirements

### Compatibilidade e addon

- [ ] **STRE-01**: Usuário pode instalar manifest privado e reproduzir mídia pronta pelo addon na TCL C6K Android TV 14; registrar build/player exatos, transporte LAN e autenticação.
- [ ] **STRE-02**: Usuário pode receber matriz real dos eventos player/library do cliente alvo e modo de atividade comprovado, sem presumir suporte por documentação.
- [ ] **PLAY-01**: Usuário pode assistir arquivo 4K completo via PC no Stremio, registrando codec, áudio, HDR quando aplicável e legendas habituais.
- [ ] **PLAY-02**: Usuário pode validar diagnóstico PC–TV, sessão de 60 minutos sem rebuffer após início, três seeks em até cinco segundos e dependência WAN separada da leitura local.
- [ ] **ADDN-01**: Usuário pode encontrar arquivos Ready como fonte C4K nos IDs canônicos e catálogo de prontos, sem stream parcial ou fictício.
- [ ] **ADDN-02**: Usuário pode fazer seek por HTTP Range autenticado; HEAD, intervalo inválido e token revogado têm comportamento correto.
- [ ] **CONF-01**: Usuário pode configurar e parear por painel local autenticado, sem edição de código ou segredos em logs.

### Intenção e fontes

- [ ] **WISH-01**: Usuário pode acompanhar filmes/séries e definir episódio inicial no painel sem depender de histórico privado da conta.
- [ ] **WISH-02**: Usuário pode fornecer torrent/magnet autorizado, URL HTTP(S) suportada ou arquivo local, com associação explícita de episódio em pacotes.
- [ ] **WISH-03**: Usuário pode configurar qualidade, idioma, tamanho, prioridade e janela de um a cinco episódios; fallback de qualidade exige opção explícita.
- [ ] **SRCE-01**: Usuário pode configurar um addon de fontes e consultar próximos episódios com candidatos suportados; comprovar um provedor real em teste.
- [ ] **SRCE-02**: Usuário pode distinguir fonte ausente, incompatível, expirada e indisponível; fluxo manual funciona sem addon de fontes.

### Downloads

- [ ] **DOWN-01**: Usuário pode acompanhar qBittorrent por API com seleção do arquivo correto, sem escolher maior arquivo implicitamente em pacotes ambíguos.
- [ ] **DOWN-02**: Usuário pode pausar/retomar/cancelar jobs; reinício reconcilia estado antes de reenviar tarefas.
- [ ] **DOWN-03**: Usuário pode ver sem progresso, falha e falta de espaço, com retries limitados e nenhuma publicação falsa de pronto.
- [ ] **DOWN-04**: Usuário pode preparar HTTP(S) integral com retomada quando suportada, validação de redirects/destinos e recusa de esquemas não suportados.

### Antecipação

- [ ] **AUTO-01**: Usuário pode início confirmado assegura intenções únicas dos próximos K episódios elegíveis, padrão proposto K=2, respeitando quota e jobs existentes.
- [ ] **AUTO-02**: Usuário pode sem eventos, consumo HTTP local pode antecipar próximos episódios; consulta de fontes e HEAD não significam reprodução nem marcam assistido.
- [ ] **AUTO-03**: Usuário pode respeitar sequência canônica, fim de temporada, especiais configurados, não lançados, replays e cancelamentos sem regressão por observação atrasada.
- [ ] **AUTO-04**: Usuário pode tolerar eventos duplicados/seek e reinícios sem duplicar downloads; perda de eventos e incerteza de progresso são explícitas.

### Biblioteca e disco

- [ ] **LIBR-01**: Usuário pode publicar só arquivo completo verificado, sem mover origem ativa de seeding ou exibir cópia parcial.
- [ ] **LIBR-02**: Usuário pode restaurar Ready/identidades após reinício e atualizar lista de fontes com cache curto sem baixar novamente.
- [ ] **DISK-01**: Usuário pode aplicar quota e reserva considerando cópia adicional, hardlinks e múltiplas séries.
- [ ] **DISK-02**: Usuário pode remover mídia manualmente com impacto explícito e proteção a arquivo em leitura; não apagar por progresso inferido.

### Distribuição

- [ ] **DIST-01**: Usuário pode instalar .NET self-contained e dependências nativas em Windows x64 limpo sem Docker, SDK ou runtime compartilhado obrigatório.
- [ ] **DIST-02**: Usuário pode iniciar no login e diagnosticar rede/energia; admin e qBittorrent em loopback, addon/mídia apenas LAN privada.
- [ ] **DIST-03**: Usuário pode preservar mídia/dados em update/uninstall por padrão; backup de migração, procedência, versões e licenças registrados.

## v2 Requirements

- **SYNC-01**: Importar histórico/biblioteca anterior e recuperar eventos perdidos por interface estável autorizada, se comprovada.
- **NOTF-01**: Notificações e horários de download.
- **SERV-01**: Serviço antes do login se necessário e validado.
- **PROF-01**: Perfis distintos com identidade comprovada; MVP usa perfil doméstico por token.

## Out of Scope

Player/transcodificador/BitTorrent próprios; streaming parcial; downloader de DRM/YouTube/externalUrl/manifest adaptativo; captura de senha Stremio; herdar automaticamente addons/segredos da conta; acesso externo obrigatório; garantir conteúdo raro disponível.

## Traceability

| Requirement | Phase | Status |
|---|---|---|
| STRE-01 | 1 | Pending |
| STRE-02 | 1 | Pending |
| PLAY-01 | 1 | Pending |
| PLAY-02 | 1 | Pending |
| ADDN-01 | 2 | Pending |
| ADDN-02 | 2 | Pending |
| CONF-01 | 2 | Pending |
| WISH-01 | 3 | Pending |
| WISH-02 | 3 | Pending |
| WISH-03 | 3 | Pending |
| SRCE-01 | 3 | Pending |
| SRCE-02 | 3 | Pending |
| DOWN-01 | 3 | Pending |
| DOWN-02 | 3 | Pending |
| DOWN-03 | 3 | Pending |
| DOWN-04 | 3 | Pending |
| AUTO-01 | 4 | Pending |
| AUTO-02 | 4 | Pending |
| AUTO-03 | 4 | Pending |
| AUTO-04 | 4 | Pending |
| LIBR-01 | 5 | Pending |
| LIBR-02 | 5 | Pending |
| DISK-01 | 5 | Pending |
| DISK-02 | 5 | Pending |
| DIST-01 | 6 | Pending |
| DIST-02 | 6 | Pending |
| DIST-03 | 6 | Pending |

**Coverage:** 27 requisitos v1, todos mapeados uma vez. Fase 2 usa importação manual mínima; fase 3 inclui publicação mínima atômica; fase 5 endurece publicação/quota já necessárias ao fluxo.
