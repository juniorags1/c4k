# Roadmap: Casa4K ? addon Stremio

## Overview

Milestone v1: PC prepara pr?ximos epis?dios; Stremio da TCL C6K Android TV 14 reproduz arquivos completos pela LAN. Revis?o autorizada em 2026-11-15 substitui roadmap centrado em Jellyfin. Seis fases, nenhuma executada. Design: [STREMIO-DESIGN.md](STREMIO-DESIGN.md).

## Phases

- [ ] **Phase 1: Provar addon e eventos na TCL C6K** ? Comprovar reprodu??o LAN e estabelecer modo de antecipa??o realmente suportado.
- [ ] **Phase 2: Assistir arquivos prontos pelo Casa4K** ? Importar arquivo e assistir pela fonte Casa4K sem Jellyfin obrigat?rio.
- [ ] **Phase 3: Preparar epis?dios a partir de fontes** ? Escolher fonte manual ou provedor e receber epis?dio completo no Stremio.
- [ ] **Phase 4: Antecipar pr?ximos epis?dios** ? Ao iniciar N, preparar N+1/N+2 conforme capacidade comprovada.
- [ ] **Phase 5: Proteger biblioteca e disco** ? Manter antecipa??o confi?vel com disco limitado e falhas.
- [ ] **Phase 6: Entregar instalador standalone** ? Instalar, parear e manter sem ambiente de desenvolvimento.

### Phase 1: Provar addon e eventos na TCL C6K
**Goal:** Comprovar reprodu??o LAN e estabelecer modo de antecipa??o realmente suportado.
**Mode:** mvp
**Depends on:** Nothing
**Requirements:** STRE-01, STRE-02, PLAY-01, PLAY-02
**UI hint:** no
**Success Criteria:**
1. Registrar TV/sistema e build/player exatos, instalar manifest privado e comprovar acesso da TV, tokens e transporte.
2. Executar 4K por 60 minutos sem rebuffer ap?s in?cio e tr?s seeks em at? cinco segundos, com ?udio/legendas/HDR aplic?veis.
3. Testar matriz de start/pause/resume/seek/stop/library/watched por fonte pr?pria e outra fonte; requests simuladas n?o aprovam suporte f?sico.
4. Provar fallback por leitura HTTP e fonte simples para pr?ximo epis?dio, distinguindo HEAD e prefetch.
5. Decidir eventos completos, modo local com entrada inicial manual, ou transporte reprovado. Falha de transporte impede avan?ar sem resolu??o.
**Plans:** To be planned.

### Phase 2: Assistir arquivos prontos pelo Casa4K
**Goal:** Importar arquivo e assistir pela fonte Casa4K sem Jellyfin obrigat?rio.
**Mode:** mvp
**Depends on:** Phase 1
**Requirements:** ADDN-01, ADDN-02, CONF-01
**UI hint:** yes
**Success Criteria:**
1. Host .NET atende manifest/catalog/meta/stream, importa??o m?nima, SQLite e painel loopback.
2. Cat?logo/fonte usam IDs can?nicos e apenas Ready, com token de m?dia separado do admin.
3. Verificar Range/HEAD/416, seek, revoga??o e rejei??o de caminho fora da raiz sem carregar v?deo inteiro na RAM.
4. Testar legendas e continuidade bingeGroup no cliente real sem prometer autoplay universal.
**Plans:** To be planned.

### Phase 3: Preparar epis?dios a partir de fontes
**Goal:** Escolher fonte manual ou provedor e receber epis?dio completo no Stremio.
**Mode:** mvp
**Depends on:** Phase 2
**Requirements:** WISH-01, WISH-02, WISH-03, SRCE-01, SRCE-02, DOWN-01, DOWN-02, DOWN-03, DOWN-04
**UI hint:** yes
**Success Criteria:**
1. Persistir s?rie/epis?dio inicial e perfil; entrada torrent/URL/arquivo funciona sem provedor externo.
2. Configurar e testar um addon de fontes real com contrato de candidatos suportados, recusados e expira??o.
3. Selecionar arquivo correto em pacote; pausa/rein?cio/cancelamento n?o duplicam jobs e fonte sem progresso fica expl?cita.
4. Publicar download completo por caminho m?nimo at?mico e reproduzir na TV; rejeitar parcial e HTTP com destino proibido.
**Plans:** To be planned.

### Phase 4: Antecipar pr?ximos epis?dios
**Goal:** Ao iniciar N, preparar N+1/N+2 conforme capacidade comprovada.
**Mode:** mvp
**Depends on:** Phase 3
**Requirements:** AUTO-01, AUTO-02, AUTO-03, AUTO-04
**UI hint:** yes
**Success Criteria:**
1. In?cio por outro addon s? dispara prepara??o se gate de eventos passou; sem eventos, entrada inicial manual e consumo Casa4K ativam modo local declarado.
2. Dedupe/seek/stop ausente/rein?cio criam inten??o ?nica por epis?dio/perfil; HEAD e consulta stream n?o disparam aquisi??o.
3. Respeitar mudan?a de temporada, n?o lan?ados, replays, quota m?nima e prioridades.
4. Janela avan?a no pr?ximo epis?dio; progresso inferido n?o marca assistido nem apaga arquivos.
**Plans:** To be planned.

### Phase 5: Proteger biblioteca e disco
**Goal:** Manter antecipa??o confi?vel com disco limitado e falhas.
**Mode:** mvp
**Depends on:** Phase 4
**Requirements:** LIBR-01, LIBR-02, DISK-01, DISK-02
**UI hint:** yes
**Success Criteria:**
1. Interrup??o de c?pia/publica??o n?o exp?e parcial e preserva origem em seeding por hardlink/c?pia.
2. Quota/reserva funcionam em mesmo volume, entre volumes e com m?ltiplas s?ries, com reconcilia??o ap?s rein?cio.
3. Restaurar cat?logo Ready, atualizar fontes e proteger arquivo em leitura contra exclus?o.
4. Indisponibilidade de provedor, PC reiniciado e cache n?o causam loops ou crescimento ilimitado.
**Plans:** To be planned.

### Phase 6: Entregar instalador standalone
**Goal:** Instalar, parear e manter sem ambiente de desenvolvimento.
**Mode:** mvp
**Depends on:** Phase 5
**Requirements:** DIST-01, DIST-02, DIST-03
**UI hint:** yes
**Success Criteria:**
1. VM Windows x64 limpa executa app self-contained e componentes oficiais sem Docker/SDK/Node.
2. Wizard detecta qBittorrent existente, configura pastas/quota/fonte e mostra manifest/diagn?stico sem sobrescrever configura??o alheia.
3. Testar in?cio no login, firewall privado, IP alterado, conflito de porta e orienta??o de energia.
4. Update/uninstall preservam m?dia/dados por padr?o; repetir iniciar epis?dio?antecipar?reproduzir pr?ximo na TV.
**Plans:** To be planned.

## Progress

| Phase | Plans Complete | Status | Completed |
|---|---|---|---|
| 1. Provar addon e eventos na TCL C6K | 0/TBD | Not started | - |
| 2. Assistir arquivos prontos pelo Casa4K | 0/TBD | Not started | - |
| 3. Preparar epis?dios a partir de fontes | 0/TBD | Not started | - |
| 4. Antecipar pr?ximos epis?dios | 0/TBD | Not started | - |
| 5. Proteger biblioteca e disco | 0/TBD | Not started | - |
| 6. Entregar instalador standalone | 0/TBD | Not started | - |
