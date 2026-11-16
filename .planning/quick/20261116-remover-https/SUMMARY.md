---
status: complete-with-certificate-cleanup-limitation
---

# C4K somente HTTP

Removidos LocalHttps, listener TLS, suporte HTTPS em PublicUrl/configuração de rede, seção do painel, endpoint /ca.crt, scripts de emissão/teste e documentação operacional de HTTPS. Base restaurada para HTTP 192.168.68.52:48240; endereço de teste PC retorna ao loopback. Histórico de planejamento mantido.

54 testes de lógica/arquivos e 53 HTTP aprovados. Publicado executável sem HTTPS, seis registros de mídia e mesma fila preservados, mesmo token/código. Manifest HTTP 200; vídeo real Range 206 com 32 bytes; /ca.crt 404; porta 48242 sem listener.

A revisão automática bloqueou o comando que incluía remoção dos certificados CurrentUser criados no teste; não forneceu motivo específico. Não houve nova tentativa de remover certificados por outro mecanismo. Eles e os arquivos residuais de configuração/exportação permanecem, sem uso pelo host atual; não foi executada limpeza no repositório de confiança do Windows. Isso não reativa HTTPS: o código/listener foram removidos. Nenhum certificado ou arquivo foi apagado pela tentativa bloqueada. A atualização HTTP foi executada separadamente sem tocar nesses certificados.
