# Arte e personagens

A imagem enviada pelo jogador orientou as cores e o desenho dos novos personagens. Os personagens desta versão são desenhados em `src/ValeDoSol/Art.cs`; o jogo não recorta os personagens diretamente da imagem de referência.

![Folha de sprites original](sprites-csharp.png)

O atlas mede 2048 × 1536 pixels, com 16 colunas e 12 linhas de células de 128 × 128. Cada célula reserva dois pixels transparentes nas bordas. Essa separação evita carregar partes de quadros vizinhos. O filtro de textura preserva os pixels ao ampliar.

- Células 0–15: fazendeiro masculino.
- Células 16–143: Lia, Bento, Iara, Theo, Dora, Miguel, Ceci e Otávio; 16 quadros por personagem.
- Cada conjunto de 16 quadros tem quatro direções: baixo, esquerda, direita e costas, com quatro quadros por direção.
- Células 144–149: árvore, arbusto, flores, pedra, tronco e cerca.
- Células 150–158: três estágios de cenoura, nabo e morango.
- Células 176–184: retratos do fazendeiro e dos oito moradores.

Os índices começam em zero. As células restantes são reservadas. `--export-art arquivo.png` exporta o atlas para inspeção; editar a imagem exportada não altera o jogo, pois a fonte dos desenhos nesta edição é `Art.cs`.

Os retratos e as animações partilham as paletas de cada personagem. Construções, chão, água, árvores e interiores usam os PNGs originais do Liberated Pixel Cup (Lanea Zimmerman / Sharm, CC BY-SA 3.0), montados em `Scenery.cs` e `World.cs`. Os arquivos e a licença estão em `Resources/Lpc`. O código MIT e os desenhos originais dos personagens mantêm suas licenças separadas. O modelo `Map.cs` define tanto os limites de desenho quanto as colisões. `Physics.cs` usa a área dos pés e passos curtos, e a navegação considera a largura do personagem. A ordenação por profundidade usa a posição dos pés. Árvores à frente do personagem ficam translúcidas.

As rotinas e falas ficam em `Social.cs`. Há compromissos por horário, planos diferentes para chuva e fim de semana e pequenos deslocamentos durante o trabalho. Os caminhos respeitam obstáculos. Ao reabrir a partida, as posições e áreas dos moradores são preservadas, assim como amizades, presentes e o contador de conversas. As agendas têm horários e destinos próprios para cada morador, com transições pelas portas e visitas à fazenda.

Para uma futura revisão visual, os materiais mais úteis são referências com indicação do que agradou, ou sprites PNG transparentes com dimensões e licença conhecidas. Novas animações devem manter o ponto dos pés alinhado entre os quadros.
