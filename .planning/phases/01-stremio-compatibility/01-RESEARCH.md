# Pesquisa aplicada à prova

SDK .NET 10.0.400 e runtime 10.0.11 já presentes no PC Windows x64. Feed oficial NuGet consultado: Microsoft.Data.Sqlite 10.0.15 disponível e fixado para persistência local. Host Minimal API único com listener LAN para addon/mídia e loopback separado para painel.

Protocolo oficial consultado em 2026-11-16: https://github.com/Stremio/stremio-addon-sdk/blob/master/docs/protocol.md. Implementar manifest/catalog/meta/stream e entradas player/library. Eventos do cliente não são garantidos nem recuperam biblioteca anterior. O app só registra observações para medir suporte da TV.

Respostas de arquivo usam Results.File com enableRangeProcessing e validação de caminhos: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0. Registrar arquivo completo local não significa implementar aquisição ou transcode.

Validação automatizada: build, HTTP real em processo local, Range/HEAD/416, autenticação, isolamento de porta admin, JSON/CORS, eventos inválidos, persistência após reinício. Validação manual separada: instalação de manifest e 4K/eventos na TV. Nenhuma simulação aprova suporte Android TV.
