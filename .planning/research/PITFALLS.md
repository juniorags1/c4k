# Riscos vigentes

| Risco | Controle | Fase |
|---|---|---|
| Documento do core n?o comprova Android TV | Prova real em TCL C6K com build/player registrados | 1 |
| Manifest n?o instala por transporte/TLS | Testar HTTP LAN primeiro e declarar requisitos extras | 1 |
| Evento perde-se com PC desligado | Sem garantia de replay; entrada manual e modo local expl?citos | 1, 4 |
| Consultar stream ? confundido com assistir | Nenhum job por consulta/HEAD; observar m?dia com confian?a limitada | 2, 4 |
| Evento n?o revela fonte | Resolvedor por provedor cadastrado ou entrada manual | 3 |
| Pacote associa arquivo errado | Mapear fileIndex ao epis?dio, pedir sele??o se amb?guo | 3 |
| Poucos seeders impedem conclus?o | Stalled/NoSource e troca limitada, sem prometer obten??o | 3 |
| Repeti??o/seek cria fila ilimitada | Inbox, inten??o ?nica e quotas | 4, 5 |
| Infer?ncia HTTP apaga m?dia assistida parcialmente | Nenhuma exclus?o autom?tica por infer?ncia | 5 |
| Wi-Fi/codecs/legendas causam travamento | Teste f?sico 4K, seek e perfil compat?vel | 1 |
| Tokens/caminhos administrativos vazam | Capacidades separadas, logs sanitizados, admin loopback e SSRF/path guards | 2, 3, 6 |
| App s? funciona no PC dev | VM Windows limpa e atualiza??o preservando dados | 6 |

C?lculos ilustrativos: 80 GB decimais em duas horas s?o cerca de 88,9 Mbit/s m?dios, sem picos. 40 GB a 50 Mbit/s levam pelo menos 1h47; iniciar pr?ximo download nos cr?ditos pode ser tarde. Quota deve contar reserva e poss?vel c?pia adicional. Nada disso ? benchmark da rede do usu?rio.

Detalhes e metas de teste em [STREMIO-DESIGN.md](../STREMIO-DESIGN.md); evid?ncias em [STREMIO-SOURCES.md](STREMIO-SOURCES.md).
