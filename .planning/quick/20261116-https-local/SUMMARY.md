---
status: configured-awaiting-tv-certificate-installation
---

# HTTPS local configurado

CA e servidor emitidos pelo Windows, RSA 3072/2048 SHA256, chaves não exportáveis em CurrentUser\My. Confiança pública da CA instalada em CurrentUser\Root. SAN de servidor: IP privado confirmado 192.168.68.52, 127.0.0.1 e localhost; EKU serverAuth. Servidor válido até 2027-11-16. Apenas CA pública exportada em `%LOCALAPPDATA%\C4K\certificates\C4K-Local-Root-CA.crt`, com cópia ignorada em artifacts/C4K-Local-Root-CA.crt. Nenhuma chave privada em arquivos, repo ou downloads.

Listener 48242 serve HTTPS; painel oferece manifest HTTPS igual para PC/TV, CA, expiração, fingerprint e URL HTTP de teste. HTTP 48240 continua funcionando com URLs de vídeo HTTP; HTTPS anuncia vídeos/legendas HTTPS. Admin segue somente loopback 48241. Configuração de rede validada para portas ativas e SAN do certificado. Script idempotente reutiliza CA e renova servidor quando próximo da validade; renovação requer execução explícita/reinício.

54 testes de lógica/arquivos, 53 HTTP e 14 HTTPS aprovados. HTTPS inclui validação normal Windows, CA somente pública, SAN/cadeia, CORS/OPTIONS, Range e recusa de admin pela LAN. Edge real aceitou certificado sem ignoreHTTPSErrors e confirmou manifest único e painel HTTPS. Produção: vídeo original servido HTTP 206/32 bytes tanto por HTTPS quanto por HTTP. Preservados quatro registros de mídia no momento da publicação e mesmo código/token. Backup de dados pequenos em artifacts/https-backup-*.

Firewall já tinha regra TCP liberada para o executável em artifacts/win-x64; nenhuma regra alterada e nenhum túnel/domínio usado.

Pendente: usuário informou não encontrar menu de instalar certificado na TCL. Preparado arquivo público para pendrive/abertura por gerenciador de arquivos, mas instalador e confiança pelo Stremio Android TV não foram comprovados. Não declarar HTTPS na TV concluído. Sem ADB disponível nesta estação e nenhuma modificação da TV executada. Instruções/rollback em docs/HTTPS-LOCAL.md.
