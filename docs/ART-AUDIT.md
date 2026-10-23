# Auditoria de arte e renderização — base da edição 6

Esta auditoria descreve os problemas encontrados **antes** da revisão da edição 7. Os retângulos abaixo foram conferidos nos PNGs LPC originais, usando seus canais de transparência e componentes visíveis. Os arquivos de origem não foram alterados.

## Causas principais

1. **Objetos eram esticados para caber na colisão.** A largura e a altura da imagem eram calculadas independentemente. Uma mesa vertical de 96 × 144 virava 96 × 64; uma estante de 32 × 80 virava 96 × 32. Isso destruía suas proporções. O desenho precisa preservar a proporção, com um ponto de apoio nos pés/base; a região sólida pode ter outra altura para representar a profundidade.
2. **Alguns retângulos cortavam o desenho de origem.** A janela ocupa `(256,10,32,44)`, mas era lida como `(256,16,32,32)`: seis pixels do topo e seis da base desapareciam. O pinheiro começava em y101, mas a leitura começava em y104. O tronco com raízes tinha 64 pixels de largura e era cortado para 32.
3. **A mesma imagem era redimensionada várias vezes.** O personagem era desenhado com escala 0,78, depois ampliado pela câmera em 1,5, depois redimensionado novamente para a janela. Em 960 × 540, o fator total era 0,8775: partes da arte original eram descartadas. O filtro pontual mantém as cores, mas não recupera os pixels que foram removidos.
4. **O atlas dos retratos cortava os ombros.** Os ombros chegavam a y142, mas a célula só aceitava desenho até y125. As animações do personagem, por outro lado, cabiam nas margens do atlas: vazamento entre essas células não era a causa principal.
5. **Algumas estruturas eram desenhadas fora da ordenação por profundidade.** A fonte da praça aparecia antes de todos os personagens. Isso permitia que o corpo do personagem cobrisse a frente da fonte mesmo quando seus pés estavam atrás dela.

## Retângulos de origem verificados

Todos usam `(x, y, largura, altura)` em pixels. Eles são referências da edição 6; a implementação revisada deve consultar o manifesto de sprites.

| Arquivo | Objeto | Retângulo completo |
| --- | --- | --- |
| country.png | Cama | 8, 15, 48, 81 |
| country.png | Cadeira frontal | 67, 0, 26, 32 |
| country.png | Outra cadeira frontal | 99, 16, 26, 32 |
| country.png | Banco baixo | 131, 24, 26, 24 |
| country.png | Banco redondo | 67, 40, 26, 24 |
| country.png | Cadeiras laterais | 69, 64, 22, 32 / 69, 96, 22, 32 |
| country.png | Cadeiras inferiores | 101, 80, 22, 32 / 133, 80, 22, 32 |
| cabinets.png | Estantes completas | x = 0, 32, 64 ou 96; y = 5; 32 × 69 |
| cabinets.png | Mesa vertical grande | 0, 268, 96, 148 |
| cabinets.png | Mesa horizontal | 96, 268, 64, 46 |
| cabinets.png | Mesa vertical menor | 102, 320, 52, 90 |
| cabinets.png | Móvel estreito alto | 160, 224, 32, 64 |
| cabinets.png | Outros móveis estreitos | 160, 300, 32, 46 / 160, 352, 32, 46 |
| barrel.png | Barril isolado | 2, 10, 28, 38 |
| barrel.png | Segundo barril | 34, 26, 28, 38 |
| barrel.png | Grupo de barris | 64, 2, 48, 61 |
| chests.png | Baús pequenos fechados | 0, 72, 32, 24 / 32, 72, 32, 24 |
| tree.png | Copa arredondada A | 1, 0, 94, 80 |
| tree.png | Copa arredondada B | 97, 16, 94, 80 |
| tree.png | Pinheiro A | 4, 101, 85, 91 |
| tree.png | Pinheiro B | 102, 117, 85, 91 |
| trunk.png | Tronco e raízes A | 16, 0, 64, 73 |
| trunk.png | Tronco e raízes B | 115, 0, 59, 71 |
| house.png | Porta A | 96, 0, 32, 48 |
| house.png | Porta B | 160, 0, 32, 48 |
| house.png | Janela alta | 256, 10, 32, 44 |
| house.png | Outra janela | 226, 0, 28, 32 |
| house.png | Chaminé | 199, 11, 22, 45 |
| kitchen.png | Fogões | 0, 96, 32, 27 / 32, 96, 32, 27 |
| kitchen.png | Balde | 32, 32, 32, 28 |
| kitchen.png | Utensílios | 35, 7, 29, 20 |

O topo de `chests.png` é uma região opaca conectada de 64 × 64. A transparência sozinha não estabelece os limites de cada variante ali; os dois baús pequenos da parte inferior têm limites inequívocos e são mais seguros para o manifesto.

## Critérios de verificação da revisão

- Cada retângulo deve estar contido no PNG e ser associado a um nome, uma base e uma proporção natural.
- A galeria deve mostrar todos os objetos completos antes de montar os interiores.
- Pés/bases devem permanecer alinhados ao alternar quadros; ampliar não deve trocar sua origem.
- Telas pequenas e grandes devem usar a mesma grade de pixels, com escala inteira ou uma política explícita de apresentação, sem encolher primeiro o sprite detalhado.
- Capturas devem incluir interiores, móveis altos, janelas, árvores, retratos e o personagem caminhando em quatro direções.
- A validação deve cobrir também a transformação inversa do mouse, para que a correção da câmera não desalinhe as interações.
