# C4K

Addon Stremio e servidor local .NET para preparar mídia no PC e reproduzir na TV. **Versão 0.2: biblioteca local**, com importação persistente, legendas e diagnóstico de eventos. Download, busca de fontes e antecipação de episódios serão implementados nas próximas fases.

## Iniciar no PC

Com SDK .NET 10.0.400 instalado, na raiz do projeto:

```powershell
dotnet run --project src/C4K.App
```

Abra **http://127.0.0.1:48241/admin/**. Informe o caminho completo de um vídeo já finalizado, título e tipo. Use o IMDb da obra para associar às fontes normais do Stremio; vazio cria uma identificação local no catálogo C4K. Para séries, informe temporada e episódio. MP4, MKV, WEBM e M4V são aceitos; não há transcodificação ou certificação de integridade.

O painel acompanha importações. No mesmo disco, o app tenta hardlink sem duplicar os dados; origem permanece intacta, mas alterações posteriores no conteúdo do original também afetam esse vínculo. Escolha **cópia independente** se quiser separar o conteúdo. Caso hardlink não seja possível, o app copia em segundo plano e só publica o arquivo após concluir. O original nunca é excluído. Importações que falham mostram erro e permitem tentar novamente; após reinício, tarefas em andamento são reprocessadas sem publicar arquivos parciais. Retirar um item do catálogo não apaga os arquivos nem libera espaço automaticamente.

Opcionalmente, informe legenda SRT/VTT em UTF-8 com até 5 MB e selecione idioma. Ela é oferecida no stream por acesso de mídia próprio. Seleção/sincronização e autoplay entre episódios ainda dependem do cliente real. Não use caminhos UNC ou arquivos em download. A API anterior `/admin/api/media` segue aceitando cadastro relativo à raiz para compatibilidade.

Copie o manifest privado pelo painel e instale diretamente no Stremio. No catálogo C4K, abra a amostra cadastrada. Para associar arquivo a uma obra existente, use o IMDb correto; episódios usam o ID da série seguido de `:temporada:episódio`. Primeiro teste filme com ID local `c4k:amostra` para separar a prova de rede da associação de metadados.

Para facilitar digitação na TV, o painel fornece `http://IP-DO-PC:48240/CODIGO/manifest.json`, com código de dez caracteres. O alias atende manifest e recursos diretamente, sem redirecionar. O endereço longo anterior continua funcionando. Esse código é uma chave para teste doméstico na LAN, sem exposição à internet; trocar secrets.json também revoga o endereço curto. Permissões de rede local do navegador continuam necessárias.

PC e TV devem estar na mesma LAN e o PC precisa permanecer ligado. O host escuta na porta **48240** para addon/mídia; painel e dados administrativos só na porta loopback **48241**. Se o Windows solicitar liberação de rede, permita apenas rede privada. O programa não altera firewall nem roteador. Não encaminhe portas para a internet.

O endereço LAN é detectado automaticamente por interface ativa com gateway. Se houver VPN ou endereço incorreto, configure antes de iniciar:

```powershell
$env:C4K_PUBLIC_URL = 'http://192.168.1.50:48240'
dotnet run --project src/C4K.App
```

Use o IP real do PC, preferencialmente reservado no DHCP. O link de instalação contém uma chave de acesso; não publique nem compartilhe. HTTP doméstico não cifra tráfego. Suporte a instalação HTTP LAN precisa ser confirmado na TV. Nenhum certificado ou túnel é exigido ou instalado automaticamente.

### “Failed to fetch” ao instalar no PC

O Stremio Windows usa WebView2 para carregar a interface web. Versões atuais do motor podem bloquear acesso de `https://web.stremio.com` a endereços locais se a permissão de rede local estiver negada. Resposta 200 no PowerShell não comprova que o navegador tenha essa permissão; trocar IP LAN por localhost também pode continuar bloqueado.

No painel C4K, use **Instalar pelo navegador** e permita o acesso à rede local para `web.stremio.com` quando solicitado. Se já foi negado, reveja a permissão desse site no Edge/Chrome. Entre na mesma conta do Stremio da TV para sincronizar o addon; isso não comprova reprodução na TV ou acesso posterior no aplicativo Windows. Não desative segurança global do navegador. [Orientação Microsoft](https://support.microsoft.com/en-us/edge/control-a-website-s-access-to-the-local-network-in-microsoft-edge).

O botão do aplicativo usa `stremio:///addons?addon=...` para preservar o `http://` completo. A forma simples `stremio://host/manifest.json` pode ser reconstruída como HTTPS pelo Stremio atual, incompatível com este host HTTP. [Código oficial do cliente](https://github.com/Stremio/stremio-web/blob/development/src/App/ShellOpenHandler.ts).

## Dados locais e diagnóstico

Por padrão, `%LOCALAPPDATA%\C4K` contém `secrets.json`, `c4k.db` e a pasta `media`. Proteja esse diretório como dados privados. Não o coloque no Git. `secrets.json` mantém dois tokens separados (addon/mídia), fora das URLs de log e independentes do painel. Para revogar o pareamento nesta prova, pare o host, remova somente `secrets.json` e reinicie; será necessário reinstalar o novo manifest.

A renomeação para C4K preservou os dados desta instalação, tokens e URL do manifest. O catálogo anterior segue como alias para clientes já instalados. Se o Stremio mantiver o nome antigo em cache, atualize/reinstale o addon com a mesma URL para carregar o nome C4K.

Variáveis opcionais: `C4K_DATA_DIR`, `C4K_MEDIA_ROOT`, `C4K_ADDON_PORT`, `C4K_ADMIN_PORT`, `C4K_PUBLIC_URL`. A importação lê arquivos locais de outras pastas e publica na raiz gerenciada, recusando links simbólicos/junctions e pastas de rede. O cadastro direto antigo continua restrito à raiz. Não altere arquivos durante reprodução.

Em **Pastas, endereço da rede e acesso**, salve o IP deste PC para persistir em `network.json`. Uma variável `C4K_PUBLIC_URL` explícita tem prioridade e bloqueia alteração pelo painel. Gerar novos links revoga os anteriores de addon, vídeo e legenda, preservando arquivos e catálogo; será necessário reinstalar o addon. A pasta da biblioteca pode ser configurada pelo painel apenas quando estiver vazia e sem importações em andamento, persistindo em `library.json`; uma variável `C4K_MEDIA_ROOT` explícita tem prioridade. Trocar pasta não move nem exclui arquivos antigos.

O painel mostra os últimos 100 eventos recebidos; banco retém até 5.000. A presença de `libraryAdd` ou `start` só demonstra um evento recebido: requests simuladas não aprovam compatibilidade com Stremio Android TV. Não há importação de biblioteca antiga ou replay garantido. Reprodução por player externo pode mudar os eventos disponíveis.

## Verificação automatizada

```powershell
dotnet build src/C4K.App -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-Test.ps1
dotnet run --project tests/C4K.LibraryTests -c Release
dotnet publish src/C4K.App -c Release -r win-x64 --self-contained true -o artifacts/win-x64
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-Test.ps1 -Published
```

Após publicação, execute `artifacts\win-x64\C4K.App.exe` sem SDK. Ainda é uma pasta de distribuição, não o instalador definitivo. O teste automatizado usa fixture binária para contratos HTTP e não substitui vídeo 4K real.

Para iniciar em segundo plano: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-C4K.ps1 -Background`. Para encerrar esse processo: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Stop-C4K.ps1`. O launcher registra apenas o PID em artifacts e não abre janelas.

## Testar na TCL C6K

1. Registrar versão exata do Stremio e player usado, além de Android TV 14.
2. Instalar o manifest; reproduzir vídeo completo, testar áudio/legendas, três seeks e 60 minutos de sessão.
3. Adicionar/remover obra na biblioteca e iniciar/pausar/encerrar vídeos por fonte C4K e outra fonte; consultar eventos no painel.
4. Reprodução básica na TCL foi confirmada pelo usuário. Registrar resultados detalhados conforme [.planning/phases/01-stremio-compatibility/01-VALIDATION.md](.planning/phases/01-stremio-compatibility/01-VALIDATION.md); duração, legendas, autoplay e eventos específicos permanecem como verificações separadas.
