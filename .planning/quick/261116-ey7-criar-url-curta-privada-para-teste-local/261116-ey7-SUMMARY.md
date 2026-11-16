---
status: complete
---

# URL curta entregue

Manifest do painel agora usa endereço LAN seguido de código de dez caracteres e manifest.json. Catálogo, metadados, streams e eventos usam a mesma base curta; URL longa continua válida. Token de mídia e administração permanecem separados. Alias depende do segredo já persistido e é revogado junto com ele. Uso previsto: teste doméstico LAN, sem exposição à internet.

Build sem warnings/errors. Smoke com 29 checks valida código inválido, endereço anterior e todos os fluxos através do alias; corrigida ordem do roteamento para aplicar reescrita antes de resolver endpoint. Testes de instalação/reprodução na TV continuam pendentes. Código real não registrado em documentos versionados.
