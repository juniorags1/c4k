# Arquitetura vigente

Stremio TV é host C4K .NET é mídia completa no PC. O mesmo host atende manifest/catalog/meta/stream, eventos condicionais, painel loopback e workers. SQLite persiste intenções; qBittorrent em loopback transfere.

Eventos e consumo HTTP local acionam planejador idempotente. Resolvedor busca fonte por addon configurado ou entrada manual; não recebe segredos de outros addons. Biblioteca separa incompletos de Ready, com promoção atômica. HTTP Range permite seek sem transcodificador próprio.

Especificação canônica com diagrama, APIs, entidades, transições, limites e gates: [STREMIO-DESIGN.md](../STREMIO-DESIGN.md). Fontes: [STREMIO-SOURCES.md](STREMIO-SOURCES.md). Proposta anterior preservada em archive/INITIAL-ARCHITECTURE.md.
