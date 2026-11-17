# C4K

Addon Stremio e servidor local .NET para preparar mídia no PC e reproduzir na TV. **Versão 0.3: automação com qBittorrent**, seleção de fontes 4K e catálogo sem cópias. Antecipação por evento de reprodução permanece na próxima fase.

## Requisitos

- Windows 11 x64 para executar e testar a integração completa.
- SDK .NET indicado em `global.json` para desenvolver ou compilar. O executável publicado é self-contained e dispensa SDK, Node e Docker.
- qBittorrent 5.x com Interface Web local para downloads e verificação de exclusões.
- Stremio e PC na mesma rede; PC ligado e sem suspensão durante downloads e reprodução.

Use apenas arquivos e fontes para os quais você possua autorização de acesso. O repositório não inclui mídia, credenciais ou fontes privadas.

## Iniciar no PC

Com SDK .NET 10.0.400 instalado, na raiz do projeto:

```powershell
dotnet run --project src/C4K.App
```

Abra **http://127.0.0.1:48241/admin/**. Configure usuário e senha da Interface Web do qBittorrent 5.x em **http://127.0.0.1:8082**, salve, teste a conexão e ative a automação. O provedor padrão é `https://torrentio.strem.fun/manifest.json`. A senha fica protegida pelo Windows (DPAPI CurrentUser), fora do repositório e das respostas do painel.

A gestão dos arquivos fica em **http://127.0.0.1:48241/list**, acessível pelo link **Gerenciar biblioteca**. Essa tela reúne busca, filtro de filmes/episódios, ordenação por tamanho, confirmação de exclusão física e cadastro de vídeos já completos em done. O total representa o tamanho lógico dos arquivos cadastrados e não soma novamente caminhos repetidos. A página principal fica concentrada na automação e nas tarefas ativas/falhas; vídeos prontos aparecem na biblioteca. Ambas as telas são restritas ao PC na porta administrativa.

Quando o Stremio envia `libraryAdd`, a intenção é persistida junto com o evento. Filmes geram uma solicitação; séries novas preparam os dois primeiros episódios regulares já lançados na sequência do Cinemeta, normalmente T01E01/T01E02. Datas futuras/ausentes aguardam reconsulta. Não reinicia o acompanhamento quando já existe episódio assistido conhecido. A solicitação manual por IMDb/episódio continua individual. O resolvedor escolhe maior contagem de seeds informada entre fontes 4K/2160p com hash torrent. Isso não garante peers disponíveis. Fontes sem contagem interpretável não são escolhidas automaticamente. Solicitação manual por IMDb/episódio ou magnet está disponível no painel; não importa retroativamente toda a biblioteca Stremio.

O título e ano são consultados no Cinemeta para identificar a solicitação e o nome de exibição do torrent. Episódios incluem temporada/episódio. Essa identificação não renomeia arquivos físicos. Se o serviço de metadados estiver indisponível, o ID continua sendo usado como fallback.

Com a automação ativada, marcar um episódio de série como **Watched/assistido** assegura os dois sucessores na sequência do Cinemeta, incluindo mudança de temporada. Só enfileira episódios lançados; data ausente ou futura aguarda reconsulta em seis horas. Ao chegar ao fim da lista conhecida, não inventa uma próxima temporada. Marcação em lote usa o último episódio regular informado; especiais da temporada zero não iniciam antecipação. Marcar a série inteira sem IDs de episódios mostra aviso no painel. Marcações antigas/repetidas não retrocedem a série nem duplicam tarefas, e arquivos já cadastrados como prontos não são baixados novamente.

O painel mostra o resultado em **Próximos episódios**. Falha de consulta de metadados é reavaliada após dez minutos. Falha de download segue exigindo retry explícito; marcar como não assistido não cancela tarefas nem apaga arquivos. Dois episódios com índices válidos podem compartilhar o mesmo torrent, preservando a união dos arquivos selecionados; exclusão de arquivos em pacote compartilhado continua protegida. O gatilho depende de o Stremio enviar `watched` com `videoId`; não importa histórico antigo e não presume que iniciar a reprodução equivale a marcar como assistido.

Filmes e séries cadastrados com IMDb incluem capa HTTPS no catálogo e nos detalhes do addon. As imagens vêm do MetaHub e dependem da internet ou do cache do Stremio; o vídeo continua sendo servido pelo PC. IDs locais sem IMDb não recebem uma capa presumida. Reabra o catálogo para atualizar imagens em cache.

As pastas de incompletos e completos são configuráveis no painel e ficam salvas após reiniciar. A troca não move arquivos ou torrents existentes. Para trocar a pasta de completos, conclua as tarefas pendentes e coloque os arquivos cadastrados no destino mantendo as subpastas.

Magnets são enviados pela API, por padrão com arquivos incompletos em `C:\media\new` e finalizados em `C:\media\done`. Não é necessário gravar magnets como arquivos na pasta monitorada. Caminhos são definidos por nova tarefa; tarefas antigas e preferências globais do qBittorrent não são alteradas. O catálogo referencia diretamente o arquivo completo de done, após confirmar conclusão e ausência de movimentação/verificação. Pacotes exigem índice de vídeo válido ou um único vídeo; nenhuma escolha implícita pelo maior arquivo. Episódios no mesmo pacote mantêm a união dos índices solicitados. A publicação aguarda conclusão de todos os arquivos selecionados no torrent.

**Apagar vídeo e retirar do catálogo exclui fisicamente o vídeo de done**, mediante confirmação. Não exclui diretórios nem outros arquivos do pacote. A tarefa própria é removida do qBittorrent com `deleteFiles=false` antes da exclusão exata; tarefas externas, compartilhamento e arquivos em uso bloqueiam a operação. A conexão com qBittorrent é necessária para verificar essa proteção. O cadastro manual aceita somente arquivos já completos na raiz done, sem copiar. Importações antigas com cópia não são retomadas pela versão 0.3. MP4, MKV, WEBM e M4V são aceitos; não há transcodificação.

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

Por padrão, `%LOCALAPPDATA%\C4K` contém `secrets.json`, `automation.json`, `library.json` e `c4k.db`; vídeos ficam em `C:\media\done`. Proteja esse diretório como dados privados. Não o coloque no Git. `secrets.json` mantém dois tokens separados (addon/mídia), fora das URLs de log e independentes do painel. Para revogar o pareamento, use o botão de gerar novos links.

A renomeação para C4K preservou os dados desta instalação, tokens e URL do manifest. O catálogo anterior segue como alias para clientes já instalados. Se o Stremio mantiver o nome antigo em cache, atualize/reinstale o addon com a mesma URL para carregar o nome C4K.

Variáveis opcionais: `C4K_DATA_DIR`, `C4K_MEDIA_ROOT`, `C4K_ADDON_PORT`, `C4K_ADMIN_PORT`, `C4K_PUBLIC_URL`. O cadastro direto recusa caminhos fora da raiz, links simbólicos/junctions e pastas de rede. Não altere arquivos durante reprodução.

Em **Rede e links de acesso**, salve o IP deste PC para persistir em `network.json`. Uma variável `C4K_PUBLIC_URL` explícita tem prioridade. Gerar novos links revoga os anteriores, preservando arquivos e catálogo; será necessário reinstalar o addon. `library.json` mantém a raiz de completos; instalações antigas precisam migrar referências antes de mudar a raiz. Trocar configuração não move arquivos automaticamente.

O painel mostra os últimos 100 eventos recebidos; banco retém até 5.000. A presença de `libraryAdd` ou `start` só demonstra um evento recebido: requests simuladas não aprovam compatibilidade com Stremio Android TV. Não há importação de biblioteca antiga ou replay garantido. Reprodução por player externo pode mudar os eventos disponíveis.

## Limpeza e cancelamento

Novos eventos `watched` de filmes ou episódios individuais agendam a exclusão física e a remoção do catálogo após 72 horas, mesmo com downloads automáticos desativados. `unwatched` cancela a limpeza pendente. Os agendamentos sobrevivem ao reinício e aparecem no painel. Não há limpeza retroativa do histórico antigo. Arquivos em uso ou ligados a tarefas externas são preservados; pacotes compartilhados aguardam os demais itens cadastrados vencerem. Falhas são tentadas novamente após uma hora. O evento ainda depende do suporte do cliente Stremio.

Downloads com falha oferecem **Tentar novamente** e **Cancelar**. Cancelar remove o item da fila exibida, sem apagar arquivos ou alterar a tarefa existente no qBittorrent. Eventos automáticos não reativam esse item; uma solicitação manual explícita pode adicioná-lo novamente.

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

## Estrutura do repositório

- `src/C4K.App`: servidor, addon, painel e integração com qBittorrent.
- `tests/C4K.LibraryTests`: verificações de persistência, automação e arquivos.
- `scripts`: inicialização, encerramento e testes HTTP.
- `.planning`: histórico de decisões e limites de validação; documentos antigos podem descrever abordagens substituídas.

Veja [CONTRIBUTING.md](CONTRIBUTING.md) para desenvolver e [SECURITY.md](SECURITY.md) para cuidados com dados e acesso. O GitHub Actions valida o projeto em Windows a cada push e pull request.

## Primeiro envio ao GitHub

Crie um repositório vazio na sua conta, sem gerar README, licença ou `.gitignore` pelo site. Na raiz deste projeto, substitua `SEU-USUARIO` pelo seu usuário:

```powershell
git remote add origin https://github.com/SEU-USUARIO/c4k.git
git push -u origin main
```

O projeto ainda não define uma licença de distribuição. O titular deve escolher uma antes de autorizar reutilização por terceiros.
