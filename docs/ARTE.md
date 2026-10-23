# Arte e personagens

A imagem enviada pelo jogador orienta as cores e o clima visual. A edição 8 acrescenta retratos ilustrados gerados para o projeto e novos detalhes nos sprites dos moradores. O jogo não recorta os personagens da imagem de referência. O fazendeiro continua masculino.

## Retratos da edição 8

![Retratos dos moradores no jogo](retratos-csharp.png)

Lia, Bento, Iara, Theo, Dora, Miguel, Ceci e Otávio têm rostos, penteados e roupas distintos. Os retratos são arquivos individuais em `src/ValeDoSol/Resources/Characters`; `Portraits.cs` carrega e apresenta cada imagem inteira, preservando proporção e transparência. A apresentação reduz as ilustrações com filtragem própria, separada da pixel art do mundo, para evitar que detalhes finos sejam descartados de forma irregular.

Os arquivos de origem são preservados. Os avisos de procedência ficam junto dos assets. Um atlas conceitual não serve como fonte de recortes de personagens: imagens individuais evitam partes de retratos vizinhos ou cabeças cortadas nos limites das células.

## Sprites de caminhada e atlas original

![Folha de sprites do jogo](sprites-csharp.png)

`Art.cs` desenha os sprites originais do mundo. O atlas mede 2048 × 1536 pixels, com 16 colunas e 12 linhas de células de 128 × 128. Cada célula reserva dois pixels transparentes nas bordas para evitar vazamento de quadros vizinhos.

- Células 0–15: fazendeiro masculino.
- Células 16–143: Lia, Bento, Iara, Theo, Dora, Miguel, Ceci e Otávio; 16 quadros por personagem.
- Cada conjunto de 16 quadros tem quatro direções: baixo, esquerda, direita e costas, com quatro quadros por direção.
- Células 144–149: árvore, arbusto, flores, pedra, tronco e cerca.
- Células 150–158: três estágios de cenoura, nabo e morango.
- Células 176–184: retratos originais do fazendeiro e dos moradores; os novos retratos dos NPCs são carregados separadamente.

Os índices começam em zero. As células restantes são reservadas. `--export-art arquivo.png` exporta o atlas; editar essa imagem exportada não altera o jogo, pois a fonte dos desenhos é `Art.cs`.

## Recortes e escala

`SpriteCatalog.cs` registra os retângulos completos dos objetos LPC e o ponto de base de cada um. Camas, estantes, baús, portas, janelas, chaminés, árvores e utensílios mantêm a proporção da fonte. Móveis largos são compostos por peças inteiras.

Os personagens usam escala de 0,5 no mundo e câmera de 2: cada pixel de origem ocupa um pixel no alvo de 1280 × 720. Posições e câmera são alinhadas a essa grade, com filtro de ponto. Os retratos ilustrados têm tratamento próprio e não substituem os quadros de caminhada.

A apresentação final usa o mesmo retângulo para desenhar e interpretar o mouse. No modo padrão **Imagem: nítida**, janelas grandes usam ampliação inteira e margens quando necessário. Por exemplo, uma janela de 1920 × 1080 apresenta o alvo em 1280 × 720 centralizado. **Imagem: preencher**, na pausa, permite a escala de 1,5 nesse exemplo. Janelas menores que o alvo precisam de redução uniforme.

A [auditoria de arte](ART-AUDIT.md) registra os problemas encontrados nas versões anteriores. Os PNGs LPC permanecem originais; os recortes e a composição são definidos no jogo.

## Horta, vila e profundidade

`Garden.cs` define as 48 parcelas, oito canteiros elevados, corredores, cercas e quatro portões. `World.cs` desenha a horta a partir desse modelo, e `Map.Solids` usa os mesmos retângulos para colisão. Os canteiros ficam ao sul da estrada e podem ser cultivados pela lateral.

`VillageProps.cs` separa a base física e a área visual de bancos, floreiras, lampiões, quadro de avisos, poço, banca e placa. `TownArt.cs` desenha esses objetos e a prefeitura, que tem torre de relógio e uma fachada própria. `Map.cs` define também o interior acessível da prefeitura, os móveis e os caminhos.

`Physics.cs` usa a área dos pés e passos curtos; a navegação considera a largura do personagem. O desenho é ordenado pela base de cada objeto e pela posição dos pés. Árvores que ocultariam o jogador ficam translúcidas, e a distribuição de árvores reserva os acessos, a horta e os novos detalhes da vila.

Agendas e falas ficam em `Social.cs`; `NpcBehavior.cs` define percursos curtos de trabalho, pausas e poses, incluindo visitas à prefeitura. Objetos de atividade e balões acompanham as animações. O save preserva posições, áreas, amizades, presentes e escolhas recentes de conversa.

## Créditos e novos assets

Construções, chão, água, árvores e móveis usam PNGs do Liberated Pixel Cup, por **Lanea Zimmerman (Sharm)**, sob **CC BY-SA 3.0**. Os originais, a procedência e a licença estão em `Resources/Lpc`, separados do código MIT e da pixel art original dos personagens. Os novos retratos foram gerados para o projeto e têm seus arquivos e avisos em `Resources/Characters`.

Para acrescentar arte, use PNGs transparentes com dimensões e licença conhecidas. Mantenha o ponto dos pés alinhado entre os quadros e registre os recortes completos no catálogo. Para retratos, preserve a imagem inteira e sua proporção ao ajustar a área de exibição.
