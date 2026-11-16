# Riscos vigentes

| Risco | Controle | Fase |
|---|---|---|
| Documento do core não comprova Android TV | Prova real em TCL C6K com build/player registrados | 1 |
| Manifest não instala por transporte/TLS | Testar HTTP LAN primeiro e declarar requisitos extras | 1 |
| Evento perde-se com PC desligado | Sem garantia de replay; entrada manual e modo local explícitos | 1, 4 |
| Consultar stream é confundido com assistir | Nenhum job por consulta/HEAD; observar mídia com confiança limitada | 2, 4 |
| Evento não revela fonte | Resolvedor por provedor cadastrado ou entrada manual | 3 |
| Pacote associa arquivo errado | Mapear fileIndex ao episódio, pedir seleção se ambíguo | 3 |
| Poucos seeders impedem conclusão | Stalled/NoSource e troca limitada, sem prometer obtenção | 3 |
| Repetição/seek cria fila ilimitada | Inbox, intenção única e quotas | 4, 5 |
| Inferência HTTP apaga mídia assistida parcialmente | Nenhuma exclusão automática por inferência | 5 |
| Wi-Fi/codecs/legendas causam travamento | Teste físico 4K, seek e perfil compatível | 1 |
| Tokens/caminhos administrativos vazam | Capacidades separadas, logs sanitizados, admin loopback e SSRF/path guards | 2, 3, 6 |
| App só funciona no PC dev | VM Windows limpa e atualização preservando dados | 6 |

Cálculos ilustrativos: 80 GB decimais em duas horas são cerca de 88,9 Mbit/s médios, sem picos. 40 GB a 50 Mbit/s levam pelo menos 1h47; iniciar próximo download nos créditos pode ser tarde. Quota deve contar reserva e possível cópia adicional. Nada disso é benchmark da rede do usuário.

Detalhes e metas de teste em [STREMIO-DESIGN.md](../STREMIO-DESIGN.md); evidências em [STREMIO-SOURCES.md](STREMIO-SOURCES.md).
