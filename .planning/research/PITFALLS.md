# Riscos e verificações

| Risco | Sinal | Prevenção / prova | Fase |
|---|---|---|---|
| Fonte sem cópia completa | Job não progride | Timeout configurável, status sem progresso, troca manual; nunca garantir obtenção | 3 |
| Wi-Fi abaixo dos picos | Arquivo local também trava | Medir PC–TV e executar filme completo; PC por cabo quando possível | 1 |
| Codec/HDR/áudio incompatível | Jellyfin transcodifica ou imagem incorreta | Matriz da TV com HEVC 10-bit, HDR, áudio e legendas reais | 1 |
| Legendas forçam burn-in | CPU alta ao ativar legenda | Testar SRT e legendas embutidas; preferir alternativa compatível | 1 |
| Disco lota durante importação | Download completo, cópia falha | Reservas, limite total e checagem antes de copiar; hardlinks quando viável | 4 |
| Arquivo parcial publicado | Player para no meio | Pastas separadas e promoção somente após conclusão verificada | 3–4 |
| Duplicação após crash | Dois jobs para o mesmo desejo | Chave idempotente, reconciliação e transições transacionais | 3 |
| Seeding quebrado por rename | qBittorrent perde arquivo | Hardlink/cópia sem mover origem; contabilizar hardlinks corretamente | 4 |
| PC dorme | Biblioteca desaparece | Configuração de energia explícita; informar necessidade de PC acordado | 1, 6 |
| Addon confundido com sync | Desejo na TV não chega ao PC | Provas independentes, lista própria como fallback explícito | 5 |
| Instalador só funciona no dev | Falta runtime ou porta em conflito | VM Windows limpa, update e uninstall preservando mídia | 6 |
| Interface administrativa exposta | Acesso sem credencial na LAN | Loopback padrão, autenticação e firewall privado | 2, 6 |

Dimensionamento ilustrativo, não medição: 25 GB decimais em duas horas ≈ 27,8 Mbit/s médios; 80 GB ≈ 88,9 Mbit/s. Picos podem ser muito maiores. Meta de engenharia: throughput sustentado com margem sobre o pico observado (começar avaliando 1,5–2 vezes); 4K não determina bitrate sozinho. Ethernet 100 Mbit/s na TV pode limitar remux de alto bitrate; verificar capacidade real antes de trocar Wi-Fi por cabo.

Dez filmes de 30 GB ocupam 300 GB, além de temporários, possíveis cópias, metadados e reserva. SSD não é obrigatório para um único stream; capacidade e desempenho efetivo importam. Download de 40 GB a 50 Mbit/s leva no mínimo ~1h47, sem overhead ou falta de peers: pré-carregar significa esperar antes da sessão.

Aceitação sugerida: arquivo 4K completo na TV, reprodução contínua por 60 minutos sem rebuffer após início, três seeks recuperando em até cinco segundos; repetir com legendas habituais. Meta a validar, não resultado já obtido. Desconectar WAN preservando LAN deve manter reprodução da biblioteca local Jellyfin. Addon Stremio tem teste separado, pois catálogo/login podem depender de serviços externos.

Fonte para compatibilidade e legendas: [Jellyfin Codec Support](https://jellyfin.org/docs/general/clients/codec-support/). Cálculos e metas são propostas do projeto.
