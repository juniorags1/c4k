---
gsd_state_version: "1.0"
current_phase: 3
current_phase_name: Preparar episódios a partir de fontes
status: in_progress
stopped_at: Tela de gestão /list publicada, separada da automação; HTTP e dados preservados.
last_updated: "2026-11-17T01:18:05.988Z"
last_activity: 2026-11-16
last_activity_desc: biblioteca local 0.2 entregue; 65 checks HTTP e 7 de recuperação aprovados.
state_head: c8b1e8b8200b0c72116ac806c5e9f458371d50bc
progress:
  total_phases: 6
  completed_phases: 0
  total_plans: 2
  completed_plans: 1
  percent: 0
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-11-16)

**Core value:** Assistir título preparado no PC pela LAN sem depender de seeders durante a sessão.
**Current focus:** Phase 3 - conexão real autenticada validada após correção do HTTP 204; fluxo download/TV pendente.

## Current Position

Phase: 3 of 6 (Preparar episódios a partir de fontes)
Plan: 1 of 1 in current phase
Status: Automação inicial implementada; UAT autenticado e demais critérios amplos da fase pendentes
Last activity: 2026-11-16 - Completed quick task 261116-uvf: limpeza automática 72h após watched, cancelada por unwatched.

Progress: [░░░░░░░░░░] 0%

## Performance Metrics

Total plans completed: 1. Sem métricas de execução disponíveis.

## Accumulated Context

### Decisions

- Gestão de biblioteca movida para /list no admin loopback 48241, com busca, filtros, tamanhos, exclusão confirmada e cadastro de completos. Página principal mantém automação/fila ativa. 61 contratos HTTP e Edge real aprovados; manifest e registros preservados. Ver quick/20261116-tela-list/.

- Usuário pediu retirar HTTPS após falha na TV. Código, listener, painel e scripts de HTTPS removidos; HTTP restaurado com mesmo manifest/código, seis mídias e fila preservadas. Porta 48242 fechada e /ca.crt 404. Limpeza de certificados Windows foi bloqueada pela revisão automática sem motivo específico; certificados residuais ficam sem uso e não houve tentativa alternativa de remoção. Ver quick/20261116-remover-https/.

- Usuário autorizou certificado local sem domínio/túnel. HTTPS 48242 configurado, mesmo manifest para PC/TV e HTTP 48240 preservado. Certificado confiado no Windows, privado não exportável; CA pública disponível no painel e /ca.crt. 54 testes de lógica, 53 HTTP, 14 HTTPS e Edge real aprovados. TV ainda não validada: usuário não encontrou menu de certificado. Ver quick/20261116-https-local/ e docs/HTTPS-LOCAL.md.

- A pedido do usuário, cancelado teste The Rings of Power (tt7631058): uma tarefa removida do qBittorrent sem apagar arquivos, registro Cancelled oculto da fila e acompanhamento automático removido. Dois filmes preservados. Ver quick/20261116-cancelar-serie-teste/.

- libraryAdd de série agora prepara dois primeiros episódios regulares lançados, normalmente T01E01/T01E02. Plano persistente inicial pode ser substituído por watched explícito; repetição de adição não retrocede progresso conhecido. 54 testes de lógica e 53 HTTP aprovados; executável publicado preservando dados. Ver quick/20261116-library-primeiros-dois/.

- 2026-11-16: usuário pediu antecipação a partir de Watched. Implementado evento persistente de episódio específico → dois sucessores canônicos já lançados, com mudança de temporada, dedupe e preservação de pacotes compartilhados. Reconsulta de lançamentos em 6h e falha de metadados em 10min. Sem evento específico não presume último visto. 49 testes de lógica, 53 HTTP e DOM Edge aprovados; UAT real watched pendente. Reprodução start permanece sem antecipação.

- Erro de adição resolvido: API 5.3 devolve contadores JSON. Uma tarefa existente foi retomada por hash, sem duplicar, e entrou em downloading com progresso real. Nome Bugonia (2025) resolvido via Cinemeta e aplicado no painel/torrent; arquivos físicos preservados. Ver debug/qbittorrent-add-name.md.

- Correção de login: qBittorrent 5.3 retorna HTTP 204 vazio. Cliente agora comprova sessão via app/version; envio de magnets aceita 204. Teste real retornou Conectado: qBittorrent v5.3.0. Credenciais preservadas. Detalhes em debug/qbittorrent-login.md.

- 2026-11-16: usuário escolheu Torrentio e qBittorrent na porta 8082. new/done sem cópias; exclusão física explícita. Configuração protegida com DPAPI. Vídeo de teste migrado para referência direta em done, removido somente vínculo antigo; tokens preservados. Automação desativada até configuração no painel. Provedor respondeu manifest 0.0.15; qBittorrent respondeu 403 sem credenciais.

- Usuário confirmou que o teste na TV funcionou e pediu nome C4K. Solução, código, executável e painel renomeados; dados e tokens migrados com o mesmo manifest. Alias do catálogo anterior mantém compatibilidade.
- Após validação inicial, usuário autorizou continuar. Biblioteca 0.2 implementa importação persistente, legendas, configuração e revogação; UAT detalhado permanece separado da implementação.

- Usuário pediu elaboração durante ausência: investigação inline e premissas revisáveis, sem entrevista bloqueante.
- Usuário escolheu addon Stremio principal; host .NET 10 LTS serve mídia, qBittorrent baixa.
- TV confirmada: TCL C6K Android TV 14, Stremio atual sem build exata. Manter fonte manual e addon configurável.
- Eventos player/library têm documentação e código públicos; prova na TV vem primeiro. Fallback por consumo HTTP só cobre mídia servida pelo C4K.
- Janela padrão proposta de dois episódios. Evento não informa origem da mídia; resolvedor de fontes separado.
- Execução por usuário/início no login proposta; sem exigência presumida de serviço antes do login.
- Em 2026-11-16 usuário autorizou iniciar implementação. Host da fase 1 entregue; não avançar à fase 2 antes da prova de compatibilidade na TV.

### Pending Todos

Informar credenciais somente no painel, testar conexão, ativar automação e validar libraryAdd → download → TV. Validar legendas e continuidade na TCL. Provedor já escolhido: Torrentio.

### Blockers/Concerns

Instalação no PC via loopback e reprodução na TV via LAN confirmadas pelo usuário; diagnóstico de Mixed Content documentado.

Usuário confirmou reprodução básica na TCL. Duração, seek, áudio/legendas e eventos específicos não foram detalhados. Downloader inicial implementado e testado com API simulada; teste real depende da autenticação local. Antecipação por reprodução ainda não implementada.

## Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|---|---|---|---|
| 20261116-tela-list | Separar gestão de biblioteca na tela /list | 2026-11-16 | ver histórico Git | [tela-list](./quick/20261116-tela-list/) |
| 20261116-remover-https | Remover HTTPS e restaurar operação HTTP | 2026-11-16 | ver histórico Git | [remover-https](./quick/20261116-remover-https/) |
| 20261116-https-local | Preparar certificado e HTTPS local para teste PC/TV | 2026-11-16 | ver histórico Git | [https-local](./quick/20261116-https-local/) |
| 20261116-library-primeiros-dois | Baixar primeiros dois episódios ao adicionar série à biblioteca | 2026-11-16 | ver histórico Git | [library-primeiros-dois](./quick/20261116-library-primeiros-dois/) |
| 20261116-watched-proximos-dois | Preparar próximos dois episódios ao marcar Watched | 2026-11-16 | ver histórico Git | [watched-proximos-dois](./quick/20261116-watched-proximos-dois/) |
| 20261116-recadastro-filme | Substituir cadastro de teste por IMDb usando o mesmo vídeo em done | 2026-11-16 | ver histórico Git | [recadastro-filme](./quick/20261116-recadastro-filme/) |
| 261115-unh | Investigar e planejar addon Stremio com pre-download de episodios | 2026-11-15 | 7d1d9a0 | [261115-unh-investigar-e-planejar-addon-stremio-com-](./quick/261115-unh-investigar-e-planejar-addon-stremio-com-/) |
| 261116-d16 | Limpar referencias a integracao descartada em toda a documentacao | 2026-11-16 | e8a73e1 | [261116-d16-limpar-referencias-a-integracao-descarta](./quick/261116-d16-limpar-referencias-a-integracao-descarta/) |
| 261116-d9k | Restaurar acentos nos documentos e configurar UTF-8 | 2026-11-16 | 256aa91 | [261116-d9k-corrigir-acentuacao-e-preservar-utf-8-no](./quick/261116-d9k-corrigir-acentuacao-e-preservar-utf-8-no/) |
| 261116-ey7 | Criar URL curta de teste local para a TV | 2026-11-16 | dad03f0 | [261116-ey7-criar-url-curta-privada-para-teste-local](./quick/261116-ey7-criar-url-curta-privada-para-teste-local/) |
| 261116-gic | Renomear projeto para C4K e preservar dados locais | 2026-11-16 | 32213b1 | [261116-gic-renomear-projeto-para-c4k-e-preservar-da](./quick/261116-gic-renomear-projeto-para-c4k-e-preservar-da/) |
| 261116-slz | Configurar pastas de incompletos e completos pelo painel | 2026-11-16 | 436922e | [261116-slz-configurar-diretorios-de-downloads-incom](./quick/261116-slz-configurar-diretorios-de-downloads-incom/) |
| 261116-uvf | Limpar filmes e episodios 72 horas apos watched, cancelando por unwatched | 2026-11-16 | c8b1e8b | [261116-uvf-limpar-filmes-e-episodios-tres-dias-apos](./quick/261116-uvf-limpar-filmes-e-episodios-tres-dias-apos/) |

## Session Continuity

Last session: 2026-11-16
Stopped at: C4K 0.2 em execução, com importação local e legendas. Fase 3 ainda não iniciada; UAT detalhado permanece pendente.
Resume file: None


### Quick task 2026-11-16: capas-addon

Capas HTTPS publicadas para IMDb no catalogo e detalhes. 51 checks HTTP aprovados; manifest preservado. Catalogo real inclui Bugonia (2025) concluido, com poster, e o video local anterior. Reproducao desse novo filme na TV ainda nao confirmada. Ver quick/20261116-capas-addon/SUMMARY.md.
