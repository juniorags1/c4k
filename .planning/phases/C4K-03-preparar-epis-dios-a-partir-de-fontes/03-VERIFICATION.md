# Verificação 0.3

- Build Release: sem warnings/erros.
- Console tests: 24 checks, incluindo DPAPI, dedupe/reinício, ranking 4K, caminhos por tarefa, seleção de pacote, parciais/moving e exclusão física de fixture.
- Smoke-Test.ps1 -Published -PublishedDirectory artifacts/c4k-automation-next: 46 contratos HTTP reais.
- Edge headless: painel carrega, porta 8082, salvar credenciais limpa senha, fila manual, cadastro direto e cancelamento da confirmação de exclusão.
- Produção local: health 0.3.0, raiz C:\media\done, um vídeo preservado com mesmo ID, mesmo manifest, Range 206/32 bytes. Vínculo privado antigo removido após validar original.
- Provedor real: manifest com.stremio.torrentio.addon 0.0.15 respondeu. Código oficial do Torrentio confirma rótulo 👤 N e fileIdx.
- qBittorrent real: 5.3.0 detectado; API 8082 responde 403 sem autenticação. Não tentadas senhas nem iniciado torrent real.

## UAT pendente

1. Preencher credenciais no painel, salvar, testar e ativar.
2. Adicionar um novo item à biblioteca Stremio; confirmar libraryAdd no diagnóstico. Se ausente, usar entrada manual e registrar limitação do cliente.
3. Conferir fonte, seeds informados e tarefa C4K com incompletos em new e final em done.
4. Aguardar conclusão, atualizar fontes Stremio e reproduzir na TV.
5. Testar exclusão somente com arquivo dispensável; confirmar remoção física sem apagar outros arquivos.
