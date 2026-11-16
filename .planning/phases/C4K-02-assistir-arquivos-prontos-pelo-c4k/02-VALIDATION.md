# Validação

Automatizado:

- `dotnet build C4K.slnx -c Release`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-Test.ps1`
- `dotnet run --project tests/C4K.LibraryTests -c Release`
- Publicar app e repetir smoke com `-Published -PublishedDirectory artifacts/c4k-library-next`.

Manual na TV: importar vídeo completo com SRT/VTT, reabrir fontes, escolher legenda e conferir sincronização/seek. Para série, registrar dois episódios consecutivos e testar continuidade no player preferido. Não presumir eventos player/library ou autoplay por simulação HTTP. Não girar links apenas para testar a atualização; isso exige reinstalação.
