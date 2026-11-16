---
status: implemented-awaiting-uat
---

# Dois primeiros episódios ao adicionar série

libraryAdd de série usa intenção persistente FromBeginning e resolvedor canônico para preparar dois primeiros episódios regulares, normalmente T01E01/T01E02. Não inclui especiais da temporada zero nem envia episódios futuros/data desconhecida. Reconsulta de lançamentos e falhas aproveita fluxo watched. Filmes seguem individuais; pedido manual também permanece individual.

Watched específico substitui o início e mantém avanço; repetir libraryAdd não reinicia série com episódio assistido conhecido. Resposta inicial atrasada não sobrescreve novo avanço. Plano anterior sem episódio identificado pode iniciar após libraryAdd explícito. Dedupe e proteção a Ready/removidos/falhas preservados.

54 testes de lógica/arquivos e 53 contratos HTTP aprovados. Publicado executável, mesmos tokens/manifest, filmes e registros de download. Sem downloads reais extras durante teste. Falta usuário adicionar série no Stremio e comprovar as duas solicitações reais; não há sincronização retroativa de biblioteca antiga.
