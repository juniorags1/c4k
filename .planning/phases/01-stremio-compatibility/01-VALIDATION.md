# Validação proposta — compatibilidade Stremio

**Estado:** não executado. Protocolo de teste, ainda sem PLAN.md de implementação ou evidência de UAT.

Ambiente informado: TCL C6K Android TV 14; Stremio atual sem build numérica. PC Windows x64 presumido; hardware/rede/disco não informados. Fonte manual e addon configurável permanecem no escopo.

Na execução, criar host .NET mínimo com manifest privado, mídia completa autorizada e logs sanitizados. Não construir fila completa antes de validar transporte/player. Testar na TV física e registrar build exata, player e rede; simulação HTTP valida contrato apenas.

| Caso | Procedimento | Evidência esperada | Estado |
|---|---|---|---|
| A1 | Instalar manifest HTTP LAN na TV | Instalação e acesso reais com token sanitizado | Pendente |
| A2 | Se falhar, separar instalação de acesso; verificar TLS | Erro real e alternativa sem infraestrutura oculta | Pendente |
| A3 | Reproduzir arquivo e fazer três seeks | Range correto, recuperação até cinco segundos | Pendente |
| A4 | Reproduzir 4K por 60 minutos com áudio/legendas usuais | Zero rebuffer após início como meta, codec/HDR registrados | Pendente |
| A5 | Cortar WAN mantendo LAN | Separar leitura de mídia de catálogo/auth/cache | Pendente |
| B1 | Start/pause/resume/seek/stop pela fonte Casa4K | Eventos recebidos ou ausência e campos | Pendente |
| B2 | Repetir por outra fonte | Confirmar escopo de eventos entre addons | Pendente |
| B3 | Add/remove library e watched/unwatched | IDs/ações recebidos, sem supor histórico anterior | Pendente |
| B4 | Encerrar app abruptamente e PC indisponível | Ausência de stop/replay e recuperação declarada | Pendente |
| C1 | Desativar eventos e consumir mídia local | Observação limitada de consumo | Pendente |
| C2 | HEAD, listar fontes e probes | Sem jobs por HEAD/listagem; risco de prefetch medido | Pendente |
| D1 | Fonte manual e um addon escolhido para próximo episódio | Identidade/candidato corretos sem credenciais da conta | Pendente |
| D2 | Pacote, URL expirada e fonte ausente | Seleção explícita ou limitação e erro honesto | Pendente |

Gate: A aprovado; B ou C define modo suportado; ao menos uma fonte D demonstrada. B reprovado permite modo local, mas impede prometer detecção de reprodução por outras fontes. A reprovado exige resolver transporte/player antes de desenvolver fases seguintes. Sem TV acessível, gate permanece pendente.

Entregável futuro: `01-COMPATIBILITY-RESULTS.md`, com versões, matriz preenchida, logs sanitizados e decisão. Não registrar os casos acima como executados por mera existência deste arquivo.
