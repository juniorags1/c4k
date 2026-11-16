# Requirements: Casa4K ? addon Stremio

**Revised:** 2026-11-15

**Core Value:** Assistir conte?do preparado no PC pelo Stremio sem depender de seeders durante a sess?o.

Alvo confirmado: TCL C6K Android TV 14, Stremio atual sem build num?rica. Usu?rio escolheu manter addon de fontes e entrada manual. Nenhuma capacidade implementada. IDs anteriores preservados com escopo atualizado; Design vigente: [STREMIO-DESIGN.md](STREMIO-DESIGN.md).

## v1 Requirements

### Compatibilidade e addon

- [ ] **STRE-01**: Usu?rio pode instalar manifest privado e reproduzir m?dia pronta pelo addon na TCL C6K Android TV 14; registrar build/player exatos, transporte LAN e autentica??o.
- [ ] **STRE-02**: Usu?rio pode receber matriz real dos eventos player/library do cliente alvo e modo de atividade comprovado, sem presumir suporte por documenta??o.
- [ ] **PLAY-01**: Usu?rio pode assistir arquivo 4K completo via PC no Stremio, registrando codec, ?udio, HDR quando aplic?vel e legendas habituais.
- [ ] **PLAY-02**: Usu?rio pode validar diagn?stico PC?TV, sess?o de 60 minutos sem rebuffer ap?s in?cio, tr?s seeks em at? cinco segundos e depend?ncia WAN separada da leitura local.
- [ ] **ADDN-01**: Usu?rio pode encontrar arquivos Ready como fonte Casa4K nos IDs can?nicos e cat?logo de prontos, sem stream parcial ou fict?cio.
- [ ] **ADDN-02**: Usu?rio pode fazer seek por HTTP Range autenticado; HEAD, intervalo inv?lido e token revogado t?m comportamento correto.
- [ ] **CONF-01**: Usu?rio pode configurar e parear por painel local autenticado, sem edi??o de c?digo ou segredos em logs.

### Inten??o e fontes

- [ ] **WISH-01**: Usu?rio pode acompanhar filmes/s?ries e definir epis?dio inicial no painel sem depender de hist?rico privado da conta.
- [ ] **WISH-02**: Usu?rio pode fornecer torrent/magnet autorizado, URL HTTP(S) suportada ou arquivo local, com associa??o expl?cita de epis?dio em pacotes.
- [ ] **WISH-03**: Usu?rio pode configurar qualidade, idioma, tamanho, prioridade e janela de um a cinco epis?dios; fallback de qualidade exige op??o expl?cita.
- [ ] **SRCE-01**: Usu?rio pode configurar um addon de fontes e consultar pr?ximos epis?dios com candidatos suportados; comprovar um provedor real em teste.
- [ ] **SRCE-02**: Usu?rio pode distinguir fonte ausente, incompat?vel, expirada e indispon?vel; fluxo manual funciona sem addon de fontes.

### Downloads

- [ ] **DOWN-01**: Usu?rio pode acompanhar qBittorrent por API com sele??o do arquivo correto, sem escolher maior arquivo implicitamente em pacotes amb?guos.
- [ ] **DOWN-02**: Usu?rio pode pausar/retomar/cancelar jobs; rein?cio reconcilia estado antes de reenviar tarefas.
- [ ] **DOWN-03**: Usu?rio pode ver sem progresso, falha e falta de espa?o, com retries limitados e nenhuma publica??o falsa de pronto.
- [ ] **DOWN-04**: Usu?rio pode preparar HTTP(S) integral com retomada quando suportada, valida??o de redirects/destinos e recusa de esquemas n?o suportados.

### Antecipa??o

- [ ] **AUTO-01**: Usu?rio pode in?cio confirmado assegura inten??es ?nicas dos pr?ximos K epis?dios eleg?veis, padr?o proposto K=2, respeitando quota e jobs existentes.
- [ ] **AUTO-02**: Usu?rio pode sem eventos, consumo HTTP local pode antecipar pr?ximos epis?dios; consulta de fontes e HEAD n?o significam reprodu??o nem marcam assistido.
- [ ] **AUTO-03**: Usu?rio pode respeitar sequ?ncia can?nica, fim de temporada, especiais configurados, n?o lan?ados, replays e cancelamentos sem regress?o por observa??o atrasada.
- [ ] **AUTO-04**: Usu?rio pode tolerar eventos duplicados/seek e rein?cios sem duplicar downloads; perda de eventos e incerteza de progresso s?o expl?citas.

### Biblioteca e disco

- [ ] **LIBR-01**: Usu?rio pode publicar s? arquivo completo verificado, sem mover origem ativa de seeding ou exibir c?pia parcial.
- [ ] **LIBR-02**: Usu?rio pode restaurar Ready/identidades ap?s rein?cio e atualizar lista de fontes com cache curto sem baixar novamente.
- [ ] **DISK-01**: Usu?rio pode aplicar quota e reserva considerando c?pia adicional, hardlinks e m?ltiplas s?ries.
- [ ] **DISK-02**: Usu?rio pode remover m?dia manualmente com impacto expl?cito e prote??o a arquivo em leitura; n?o apagar por progresso inferido.

### Distribui??o

- [ ] **DIST-01**: Usu?rio pode instalar .NET self-contained e depend?ncias nativas em Windows x64 limpo sem Docker, SDK ou runtime compartilhado obrigat?rio.
- [ ] **DIST-02**: Usu?rio pode iniciar no login e diagnosticar rede/energia; admin e qBittorrent em loopback, addon/m?dia apenas LAN privada.
- [ ] **DIST-03**: Usu?rio pode preservar m?dia/dados em update/uninstall por padr?o; backup de migra??o, proced?ncia, vers?es e licen?as registrados.

## v2 Requirements

- **SYNC-01**: Importar hist?rico/biblioteca anterior e recuperar eventos perdidos por interface est?vel autorizada, se comprovada.
- **NOTF-01**: Notifica??es e hor?rios de download.
- **SERV-01**: Servi?o antes do login se necess?rio e validado.
- **PROF-01**: Perfis distintos com identidade comprovada; MVP usa perfil dom?stico por token.

## Out of Scope

Player/transcodificador/BitTorrent pr?prios; streaming parcial; downloader de DRM/YouTube/externalUrl/manifest adaptativo; captura de senha Stremio; herdar automaticamente addons/segredos da conta; acesso externo obrigat?rio; garantir conte?do raro dispon?vel.

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

**Coverage:** 27 requisitos v1, todos mapeados uma vez. Fase 2 usa importa??o manual m?nima; fase 3 inclui publica??o m?nima at?mica; fase 5 endurece publica??o/quota j? necess?rias ao fluxo.
