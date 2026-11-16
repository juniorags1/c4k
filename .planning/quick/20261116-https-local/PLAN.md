# HTTPS local para teste na TCL

GSD quick inline. Criar CA local e certificado de servidor no repositório CurrentUser do Windows, com chave privada não exportável, SAN IP 192.168.68.52/127.0.0.1/localhost e EKU serverAuth. Exportar somente CA pública DER para importação na TV. Usuário autorizou configuração local e teste; não alterar certificados da TV remotamente nem remover validação TLS.

Implementar listener HTTPS opcional 48242 configurado em tls.json fora do repo. HTTP 48240 e admin loopback 48241 permanecem; URLs de mídia acompanham o transporte da consulta para preservar teste HTTP anterior. Manifest HTTPS será único para PC/TV, com mesmos tokens. Expor somente certificado público em /ca.crt, informações de expiração/fingerprint e instruções no painel. Validar certificado/cadeia, headers/CORS/Range e bloqueio de admin via HTTPS. Firewall somente rede privada/sub-rede local se houver permissão.

Testes isolados e rollback sem apagar arquivos/certificados existentes; backup de configurações. Instalação e aceitação da CA no Stremio Android TV dependem de ação e UAT do usuário. Fontes: Microsoft New-SelfSignedCertificate/Kestrel, Android Network Security Configuration.
