---
phase: 01-stremio-compatibility
plan: 01
status: checkpoint
requirements_completed: []
---

# Host de compatibilidade implementado

Em 2026-11-16 foi criada a aplicação .NET 10: addon privado, painel loopback, cadastro de arquivos completos, reprodução HTTP Range e captura de eventos player/library em SQLite. Tokens separados por capacidade persistem, admin não responde na porta LAN, caminhos fora da raiz/reparse points são recusados. Não há downloads/antecipação nesta entrega.

## Verificação automatizada

- Build Release: zero warnings/errors.
- scripts/Smoke-Test.ps1: 24 verificações HTTP reais aprovadas, com reinício e preservação de pareamento/mídia/eventos.
- Publicação self-contained win-x64 em artifacts/win-x64; mesmo smoke -Published com 24 verificações aprovadas.
- PS1 usa UTF-8 com BOM para Windows PowerShell 5.1; demais arquivos UTF-8.

Corrigidos durante testes: campo opcional ausente de evento e colisão SQL de current_time com palavra especial SQLite. Eventos simulados não aprovam compatibilidade da TV.

## Checkpoint físico

Sem vídeo real informado e interação na TCL, não foram comprovados manifest HTTP LAN, 4K, áudio/legenda/HDR ou eventos Android TV. STRE-01/STRE-02/PLAY-01/PLAY-02 permanecem pendentes. Não concluir fase 1 nem avançar fase 2. Consultar 01-COMPATIBILITY-RESULTS.md e 01-VALIDATION.md.

## Decisões

Um projeto App concentra a prova; separar Core/Infrastructure quando surgir preparação. Cadastro não copia vídeo nem certifica integridade; SQLite é a primeira leitura/escrita real. Self-contained é pasta executável, instalador permanece fase 6. Nenhuma configuração de firewall/roteador ou conta Stremio foi alterada.
