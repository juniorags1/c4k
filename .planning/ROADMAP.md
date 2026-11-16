# Roadmap: Casa4K

## Overview

Milestone v1: preparar arquivos completos no PC e assistir pela LAN com instalação doméstica simples. Seis fases propostas, nenhuma executada. Validar rede e player antes de investir em automação. Integração Stremio tem decisão de viabilidade independente do caminho Jellyfin.

## Phases

- [ ] **Phase 1: Provar 4K local na Google TV** — eliminar incerteza de rede e codecs.
- [ ] **Phase 2: Gerenciar desejos no PC** — painel .NET e fontes explícitas.
- [ ] **Phase 3: Baixar com retomada e estados confiáveis** — preparar sem supervisão constante.
- [ ] **Phase 4: Publicar biblioteca pronta e controlar disco** — fechar fluxo desejo→TV.
- [ ] **Phase 5: Provar integração com Stremio** — decidir addon e sincronização separadamente.
- [ ] **Phase 6: Entregar instalador standalone** — validar fora da máquina de desenvolvimento.

### Phase 1: Provar 4K local na Google TV
**Goal:** Usuário assiste um arquivo representativo inteiro via LAN e identifica gargalos reais.
**Mode:** mvp
**Depends on:** Nothing
**Requirements:** PLAY-01, PLAY-02
**UI hint:** no
**Success Criteria:**
1. Inventário de PC, disco, TV e rede registrado; Jellyfin nativo e app de TV configurados com arquivo autorizado.
2. Resultado de codec, áudio, legenda, HDR quando aplicável e direct play/transcode documentado, sem presumir suporte por resolução.
3. Sessão de 60 minutos e seeks medidos; reprodução Jellyfin continua sem WAN. Se falhar, causa e ajuste comprovados antes de avançar.
4. Comparação prática com Radarr/Sonarr confirma se código próprio traz benefício ao usuário; ajustar fases seguintes se ferramentas existentes bastarem.
**Plans:** To be planned with `$gsd-plan-phase 1`.

### Phase 2: Gerenciar desejos no PC
**Goal:** Usuário mantém desejos e configura fontes por navegador local.
**Mode:** mvp
**Depends on:** Phase 1
**Requirements:** WISH-01, WISH-02, WISH-03, CONF-01
**UI hint:** yes
**Success Criteria:**
1. Filme e episódio podem ser adicionados e removidos, mantendo estado após reinício e rejeitando duplicação.
2. Desejo sem fonte permanece AguardandoFonte; magnet, torrent, URL e importação local entram por caminhos explícitos.
3. Preferências e prioridade ficam salvas; painel e segredos têm proteção desde a primeira versão.
**Plans:** To be planned.

### Phase 3: Baixar com retomada e estados confiáveis
**Goal:** Usuário deixa o PC preparando arquivos e acompanha progresso honesto.
**Mode:** mvp
**Depends on:** Phase 2
**Requirements:** DOWN-01, DOWN-02, DOWN-03, DOWN-04
**UI hint:** yes
**Success Criteria:**
1. Download autorizado torrent e HTTP chega a conclusão; torrent multifile respeita episódio selecionado.
2. Pausa/retomada/cancelamento e reinício não criam jobs duplicados.
3. Fonte sem progresso e falhas ficam visíveis com recuperação; nenhum parcial é anunciado como pronto.
4. HTTP com e sem suporte a retomada e redirects proibidos têm comportamento verificado.
**Plans:** To be planned.

### Phase 4: Publicar biblioteca pronta e controlar disco
**Goal:** Desejo concluído aparece na TV sem gerenciamento manual de arquivos.
**Mode:** mvp
**Depends on:** Phase 3
**Requirements:** LIBR-01, LIBR-02, DISK-01, DISK-02
**UI hint:** yes
**Success Criteria:**
1. Arquivo completo é importado e indexado; cópia interrompida não aparece na biblioteca.
2. Biblioteca permanece reproduzível com Casa4K parado; Jellyfin continua servindo arquivos independentes.
3. Cenários de espaço insuficiente, hardlink e cópia entre volumes preservam dados e seeding.
4. Exclusão explicita impacto e não apaga desejos ou mídia inadvertidamente.
**Plans:** To be planned.

### Phase 5: Provar integração com Stremio
**Goal:** Usuário conhece exatamente o que poderá continuar fazendo dentro do Stremio.
**Mode:** mvp
**Depends on:** Phase 4
**Requirements:** STRE-01, STRE-02
**UI hint:** no
**Success Criteria:**
1. Prova de addon usa URL LAN de arquivo pronto, nunca infoHash para reprodução; testar instalação, auth, HTTP/HTTPS e seek na Google TV real.
2. Evidência identifica se existe caminho suportado para ler mudanças na lista pessoal, sem confundir consultas de catálogo com eventos de desejo.
3. Relatório decide seguir com addon/sync ou manter lista própria/Jellyfin, deixando necessidades não atendidas explícitas. Implementação completa promovida separadamente a v2.
**Plans:** To be planned.

### Phase 6: Entregar instalador standalone
**Goal:** Usuário instala e mantém solução doméstica sem ambiente de desenvolvimento.
**Mode:** mvp
**Depends on:** Phase 4, Phase 5
**Requirements:** DIST-01, DIST-02, DIST-03
**UI hint:** yes
**Success Criteria:**
1. Instalação em VM Windows limpa executa build self-contained sem Docker, SDK, Node ou runtime compartilhado obrigatório.
2. Componentes existentes são detectados; artefatos novos têm procedência/versão verificadas e não sobrescrevem configuração alheia.
3. Início no login, portas em conflito, firewall privado e PC em suspensão têm comportamento explicado e testado.
4. Atualização preserva SQLite/configuração/mídia; uninstall preserva dados por padrão. Instalação final repete desejo→download→TV.
**Plans:** To be planned.

## Progress

| Phase | Plans Complete | Status | Completed |
|---|---|---|---|
| 1. Provar 4K local | 0/TBD | Not started | - |
| 2. Gerenciar desejos | 0/TBD | Not started | - |
| 3. Downloads confiáveis | 0/TBD | Not started | - |
| 4. Biblioteca e disco | 0/TBD | Not started | - |
| 5. Viabilidade Stremio | 0/TBD | Not started | - |
| 6. Instalador standalone | 0/TBD | Not started | - |
