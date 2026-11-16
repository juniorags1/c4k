# Pesquisa aplicada

Host existente .NET 10 e SQLite já atendem addon/Range. Evoluir sem trocar banco ou dependências: tabelas aditivas para fila de importação e legendas; BackgroundService singleton com conexões SQLite curtas por operação.

Importação deve deixar o arquivo original intacto, usar hardlink quando possível e cópia em background caso contrário, publicando apenas após conclusão. Registrar jobs antes de responder ao painel e reconciliar estado em reinício. Não seguir junctions/symlinks ou abrir UNC. Legendas externas limitadas a SRT/VTT UTF-8, servidas por token de mídia separado da administração.

Fontes primárias consultadas em 2026-11-16: https://learn.microsoft.com/en-us/dotnet/core/extensions/scoped-service ; https://github.com/Stremio/stremio-addon-sdk/blob/master/docs/api/responses/stream.md ; https://github.com/Stremio/stremio-addon-sdk/blob/master/docs/api/responses/subtitles.md .

Teste de regressão: HTTP real, SQLite migrado e reinício. Novo teste de importação usa arquivos sintéticos completos, sem baixar mídia real. UAT de legenda/autoplay na TV segue separado.
