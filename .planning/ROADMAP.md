# Roadmap: C4K é addon Stremio

## Overview

Milestone v1: PC prepara próximos episódios; Stremio da TCL C6K Android TV 14 reproduz arquivos completos pela LAN. Revisão autorizada em 2026-11-15 substitui roadmap inicial de servidor de mídia. Seis fases; baseline na TV validado e biblioteca local implementada, com UAT detalhado pendente. Design: [STREMIO-DESIGN.md](STREMIO-DESIGN.md).

## Phases

- [ ] **Phase 1: Provar addon e eventos na TCL C6K** é Comprovar reprodução LAN e estabelecer modo de antecipação realmente suportado.
- [ ] **Phase 2: Assistir arquivos prontos pelo C4K** é Importar arquivo e assistir pela fonte C4K pelo servidor local.
- [ ] **Phase 3: Preparar episódios a partir de fontes** é Escolher fonte manual ou provedor e receber episódio completo no Stremio.
- [ ] **Phase 4: Antecipar próximos episódios** é Ao iniciar N, preparar N+1/N+2 conforme capacidade comprovada.
- [ ] **Phase 5: Proteger biblioteca e disco** é Manter antecipação confiável com disco limitado e falhas.
- [ ] **Phase 6: Entregar instalador standalone** é Instalar, parear e manter sem ambiente de desenvolvimento.

### Phase 1: Provar addon e eventos na TCL C6K
**Goal:** Comprovar reprodução LAN e estabelecer modo de antecipação realmente suportado.
**Mode:** mvp
**Depends on:** Nothing
**Requirements:** STRE-01, STRE-02, PLAY-01, PLAY-02
**UI hint:** no
**Success Criteria:**
1. Registrar TV/sistema e build/player exatos, instalar manifest privado e comprovar acesso da TV, tokens e transporte.
2. Executar 4K por 60 minutos sem rebuffer após início e três seeks em até cinco segundos, com áudio/legendas/HDR aplicáveis.
3. Testar matriz de start/pause/resume/seek/stop/library/watched por fonte própria e outra fonte; requests simuladas não aprovam suporte físico.
4. Provar fallback por leitura HTTP e fonte simples para próximo episódio, distinguindo HEAD e prefetch.
5. Decidir eventos completos, modo local com entrada inicial manual, ou transporte reprovado. Falha de transporte impede avançar sem resolução.
**Plans:** 01-01-PLAN.md implementado; checkpoint de compatibilidade na TV pendente.

### Phase 2: Assistir arquivos prontos pelo C4K
**Goal:** Importar arquivo e assistir pela fonte C4K pelo servidor local.
**Mode:** mvp
**Depends on:** Phase 1
**Requirements:** ADDN-01, ADDN-02, CONF-01
**UI hint:** yes
**Success Criteria:**
1. Host .NET atende manifest/catalog/meta/stream, importação mínima, SQLite e painel loopback.
2. Catálogo/fonte usam IDs canônicos e apenas Ready, com token de mídia separado do admin.
3. Verificar Range/HEAD/416, seek, revogação e rejeição de caminho fora da raiz sem carregar vídeo inteiro na RAM.
4. Testar legendas e continuidade bingeGroup no cliente real sem prometer autoplay universal.
**Plans:** 02-01-PLAN.md implementado; 02-VERIFICATION.md registra testes e UAT pendente.

### Phase 3: Preparar episódios a partir de fontes
**Goal:** Escolher fonte manual ou provedor e receber episódio completo no Stremio.
**Mode:** mvp
**Depends on:** Phase 2
**Requirements:** WISH-01, WISH-02, WISH-03, SRCE-01, SRCE-02, DOWN-01, DOWN-02, DOWN-03, DOWN-04
**UI hint:** yes
**Success Criteria:**
1. Persistir série/episódio inicial e perfil; entrada torrent/URL/arquivo funciona sem provedor externo.
2. Configurar e testar um addon de fontes real com contrato de candidatos suportados, recusados e expiração.
3. Selecionar arquivo correto em pacote; pausa/reinício/cancelamento não duplicam jobs e fonte sem progresso fica explícita.
4. Publicar download completo por caminho mínimo atômico e reproduzir na TV; rejeitar parcial e HTTP com destino proibido.
**Plans:** 03-01-PLAN.md implementado: Torrentio, magnets, qBittorrent e publicação direta. Validação real autenticada pendente; pausa/cancelamento, perfis e controles adicionais não estão concluídos. Download HTTP adiado pela revisão do usuário.

### Phase 4: Antecipar próximos episódios
**Goal:** Ao iniciar N, preparar N+1/N+2 conforme capacidade comprovada.
**Mode:** mvp
**Depends on:** Phase 3
**Requirements:** AUTO-01, AUTO-02, AUTO-03, AUTO-04
**UI hint:** yes
**Success Criteria:**
1. Início por outro addon só dispara preparação se gate de eventos passou; sem eventos, entrada inicial manual e consumo C4K ativam modo local declarado.
2. Dedupe/seek/stop ausente/reinício criam intenção única por episódio/perfil; HEAD e consulta stream não disparam aquisição.
3. Respeitar mudança de temporada, não lançados, replays, quota mínima e prioridades.
4. Janela avança no próximo episódio; progresso inferido não marca assistido nem apaga arquivos.
**Plans:** To be planned.

### Phase 5: Proteger biblioteca e disco
**Goal:** Manter antecipação confiável com disco limitado e falhas.
**Mode:** mvp
**Depends on:** Phase 4
**Requirements:** LIBR-01, LIBR-02, DISK-01, DISK-02
**UI hint:** yes
**Success Criteria:**
1. Interrupção de cópia/publicação não expõe parcial e preserva origem em seeding por hardlink/cópia.
2. Quota/reserva funcionam em mesmo volume, entre volumes e com múltiplas séries, com reconciliação após reinício.
3. Restaurar catálogo Ready, atualizar fontes e proteger arquivo em leitura contra exclusão.
4. Indisponibilidade de provedor, PC reiniciado e cache não causam loops ou crescimento ilimitado.
**Plans:** To be planned.

### Phase 6: Entregar instalador standalone
**Goal:** Instalar, parear e manter sem ambiente de desenvolvimento.
**Mode:** mvp
**Depends on:** Phase 5
**Requirements:** DIST-01, DIST-02, DIST-03
**UI hint:** yes
**Success Criteria:**
1. VM Windows x64 limpa executa app self-contained e componentes oficiais sem Docker/SDK/Node.
2. Wizard detecta qBittorrent existente, configura pastas/quota/fonte e mostra manifest/diagnóstico sem sobrescrever configuração alheia.
3. Testar início no login, firewall privado, IP alterado, conflito de porta e orientação de energia.
4. Update/uninstall preservam mídia/dados por padrão; repetir iniciar episódio → antecipar → reproduzir próximo na TV.
**Plans:** To be planned.

## Progress

| Phase | Plans Complete | Status | Completed |
|---|---|---|---|
| 1. Provar addon e eventos na TCL C6K | 0/1 | Awaiting TV verification | - |
| 2. Assistir arquivos prontos pelo C4K | 1/1 | Implemented; UAT pending | - |
| 3. Preparar episódios a partir de fontes | 0/TBD | Not started | - |
| 4. Antecipar próximos episódios | 0/TBD | Not started | - |
| 5. Proteger biblioteca e disco | 0/TBD | Not started | - |
| 6. Entregar instalador standalone | 0/TBD | Not started | - |
