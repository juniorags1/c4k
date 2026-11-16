# Arquitetura vigente

Stremio TV ? host Casa4K .NET ? m?dia completa no PC. O mesmo host atende manifest/catalog/meta/stream, eventos condicionais, painel loopback e workers. SQLite persiste inten??es; qBittorrent em loopback transfere. 

Eventos e consumo HTTP local acionam planejador idempotente. Resolvedor busca fonte por addon configurado ou entrada manual; n?o recebe segredos de outros addons. Biblioteca separa incompletos de Ready, com promo??o at?mica. HTTP Range permite seek sem transcodificador pr?prio.

Especifica??o can?nica com diagrama, APIs, entidades, transi??es, limites e gates: [STREMIO-DESIGN.md](../STREMIO-DESIGN.md). Fontes: [STREMIO-SOURCES.md](STREMIO-SOURCES.md). Proposta anterior preservada em archive/INITIAL-ARCHITECTURE.md.
