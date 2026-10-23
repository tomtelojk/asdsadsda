# O Caderno das Estações

A campanha de **Vale do Sol** começa quando um jovem fazendeiro herda a Fazenda Girassol de sua avó Aurora. Junto da chave, ela deixou uma carta e um caderno incompleto. O caderno mistura instruções de cultivo, receitas e nomes de moradores: registros de uma antiga horta comunitária que ocupava uma ala da casa das flores.

Aurora não deixou uma dívida, um prazo ou uma fazenda pronta. Deixou um projeto interrompido e pessoas que guardaram partes dele. O objetivo do jogador é recuperar esse espaço e reconstruir os vínculos que faziam a horta funcionar. Cada capítulo combina ações existentes no jogo com uma conversa e uma pequena mudança na história.

## Capítulos jogáveis

| Capítulo | Objetivo | Participação e recompensa |
| --- | --- | --- |
| Os nomes do vale | Ler a carta e conhecer todos os moradores. | Otávio reconhece o caderno e devolve uma página. 40 moedas e 5 sementes de cenoura. |
| A primeira página | Colher 5 vegetais e entregar 3 cenouras. | Theo encontra a planta da velha horta entre as páginas. 50 moedas e 5 sementes de nabo. |
| Madeira que volta a cantar | Entregar 12 madeiras, 6 pedras e 80 moedas. | Bento prepara as bancadas da ala antiga. 5 sementes de morango e 10 pontos de energia máxima. |
| Um canteiro de muitas mãos | Manter as três culturas plantadas e ter 2 corações com 3 moradores. | Miguel organiza as mudas e as etiquetas de quem ajudou. 80 moedas e 5 sementes de cada cultura. |
| Uma mesa para o vale | Entregar 2 cenouras, 2 nabos, 2 morangos e 2 peixes; ter 3 corações com 4 moradores. | Dora e Iara guardam os alimentos; Lia ajuda com os convites. 150 moedas e o encontro preparado. |
| A página em branco | Ter 2 corações com todos e voltar à vila entre 17h e 21h, em um dia posterior aos preparativos. | Ceci e Otávio conduzem o encerramento. 200 moedas, 5 sementes de morango e 10 pontos de energia máxima. |

Os materiais de uma entrega são consumidos apenas quando todos os requisitos estão prontos e há espaço para receber a recompensa. Não há prazo para cumprir capítulos. O jogo continua após o desfecho, com cultivo, pesca, exploração e amizades.

## Os personagens na história

- **Lia** preserva a tradição de compartilhar sementes e organiza os convites. Seu armazém liga a produção da fazenda à vida cotidiana da vila.
- **Bento** recupera as bancadas e a mesa, valorizando as marcas de uso. Aprendeu com Aurora que consertar não exige apagar o passado.
- **Iara** lembra das trocas de peixe e vegetais sem caderno de dívidas. Sua contribuição para a mesa é também uma maneira de recuperar essa confiança.
- **Theo** começa tratando a horta como pesquisa e aprende a reconhecer o que os números não registram: o valor de cultivar em companhia.
- **Dora** guarda receitas e associa memória a gestos concretos. Ela transforma a colheita em uma refeição compartilhada.
- **Miguel** cuida das mudas e aproxima os moradores. Para ele, o projeto só funciona quando as pessoas querem voltar para cuidar dele.
- **Ceci** chegou depois de Aurora. Sua perspectiva mostra que preservar uma história também exige abrir espaço para quem não estava no começo.
- **Otávio** guardou uma página que não conseguiu devolver. Ao entregá-la ao neto de Aurora, deixa de tratar o caderno como lembrança parada e passa a ajudá-lo a continuar.

Cada morador tem uma fala própria para cada fase da campanha, inclusive depois do encerramento. Essas falas complementam as conversas cotidianas, presentes e atividades da vila.

## Tom e limites

A história é acolhedora, com saudade, humor e trabalho em comunidade. A herança oferece um recomeço; ela não pune o jogador por jogar devagar. O final reconhece que cuidar de um lugar é uma prática contínua.

A campanha não depende de itens escondidos que ainda não existem, de personagens adicionais ou de novos minijogos. A reforma e o encontro têm marcos narrativos próprios. A implementação visual pode usar os marcos de progresso para mostrar as bancadas preparadas, o canteiro e os enfeites do encontro. Não há promessa de uma animação musical ou de música gravada: o encerramento é apresentado em texto.

## Estado e integração

`Story.cs` usa três campos persistentes em `State`: `story_stage`, `story_flags` e `story_day`, inicialmente zero. `story_stage` varia de 0 a 6; os bits 0–5 de `story_flags` registram os capítulos concluídos e o bit 6 registra a leitura da carta. `story_day` guarda o dia do último capítulo concluído.

`Story.Current` fornece título, resumo, objetivo, progresso, recompensa e contato do capítulo atual. `Story.TryAdvance` verifica requisitos, entrega materiais e concede uma recompensa uma única vez. `Story.NpcLine` fornece a fala de cada morador na fase atual. A carta e o desfecho podem ser relidos sem conceder prêmios ou mudar o estado.
