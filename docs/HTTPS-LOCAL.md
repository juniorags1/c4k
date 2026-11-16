# HTTPS local no C4K

O C4K pode servir HTTPS no IP privado do PC, sem domínio, túnel ou serviço externo. O certificado é assinado por uma CA local confiada no usuário Windows. A aceitação na TV depende do instalador de certificados do aparelho e da política de confiança do Stremio Android TV.

## Preparar o Windows

Com o executável atualizado, execute no mesmo usuário Windows que inicia o C4K:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Enable-LocalHttps.ps1 -Address 192.168.68.52
```

O script usa APIs nativas do Windows. Cria CA e certificado de servidor em `Cert:\CurrentUser\My`, com chaves privadas não exportáveis. Instala somente a parte pública da CA em `Cert:\CurrentUser\Root`. Não usa Docker, domínio, ngrok ou certificado de desenvolvimento do localhost.

O certificado de servidor inclui SAN para o IP selecionado, 127.0.0.1 e localhost, EKU serverAuth, validade de um ano. A CA dura cinco anos. Configura `tls.json` e o endereço preferido em `network.json` dentro de `%LOCALAPPDATA%\C4K`; guarda cópia anterior de network.json em `certificates/network-before-https.json`. O script não modifica o firewall.

Reinicie o C4K para carregar a configuração. Reserve o IP do PC no DHCP do roteador. Antes do certificado vencer, execute o script novamente e reinicie; a CA é reaproveitada enquanto válida, portanto renovar o certificado de servidor não exige reinstalar a mesma CA na TV. Mudança de IP exige nova emissão. Não há renovação agendada automática nesta versão.

## Endereços

- HTTPS addon/mídia: `https://192.168.68.52:48242`.
- HTTP anterior: `http://192.168.68.52:48240`.
- Painel: `http://127.0.0.1:48241/admin/`.
- CA pública: `http://192.168.68.52:48240/ca.crt`.

O painel mostra manifest HTTPS único para PC/TV, link para CA, validade e fingerprint SHA-256. Tokens e IDs permanecem os mesmos. Consultas ao addon por HTTPS recebem URLs HTTPS para mídia e legendas; consultas por HTTP recebem URLs HTTP, permitindo preservar o teste anterior enquanto a TV não confia na CA. O admin permanece inacessível nas portas LAN.

## Instalar a CA na TV

1. Baixe `C4K-Local-Root-CA.crt` pelo painel ou pelo endereço /ca.crt. O arquivo é somente o certificado público da CA, sem chave privada.
2. Transfira por pendrive ou use um navegador no aparelho para baixar.
3. Procure Segurança/Privacidade → Credenciais → Instalar certificado → CA. Se o menu não existir, tente abrir o .crt por um gerenciador de arquivos para verificar se há um instalador associado.
4. Instale como CA, se o sistema oferecer essa opção. Confira o nome C4K Local Root CA e a fingerprint mostrada no painel.
5. Teste o manifest HTTPS na TV e somente então substitua a instalação antiga do addon pelo manifest HTTPS do painel. Esse endereço pode ser sincronizado para o PC pela mesma conta.

Não foi comprovado um menu de instalação na TCL C6K desta sessão: usuário informou que não o encontrou. Abrir o arquivo pode não oferecer instalador. Mesmo com a CA instalada, apps Android modernos podem confiar somente nas CAs do sistema; a instalação pelo usuário não garante confiança pelo Stremio. Não considerar HTTPS na TV concluído antes da prova de manifest e reprodução. Nenhuma validação de certificado foi desativada e o aplicativo da TV não foi modificado.

## Voltar ao HTTP

No painel, configure a base `http://192.168.68.52:48240` e reinstale o manifest HTTP nos clientes afetados. Para desativar completamente o listener HTTPS, pare o host, renomeie `tls.json` fora do nome ativo e reinicie. Isso não remove certificados, dados ou vídeos. O certificado precisa continuar válido enquanto o listener estiver configurado.

## Verificação de desenvolvimento

```powershell
dotnet publish src/C4K.App -c Release -r win-x64 --self-contained true -o artifacts/c4k-https-next
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-LocalHttps.ps1 -TlsConfigPath artifacts/tls-fixture/tls.json
```

O fixture pode usar cópia de tls.json com certificados válidos no mesmo usuário Windows. Os testes usam dados/portas isoladas e validação TLS normal, sem ignorar erros de cadeia ou hostname.

Referências: [New-SelfSignedCertificate](https://learn.microsoft.com/en-us/powershell/module/pki/new-selfsignedcertificate), [Kestrel HTTPS](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints), [Android Network Security Configuration](https://developer.android.com/privacy-and-security/security-config).
