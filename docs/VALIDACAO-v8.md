# Validação da edição 8

- Build Release sem avisos e 1.961 verificações aprovadas: cultivo, campanha, rotinas, arte, física, interface e saves.
- Todas as 48 parcelas estão fora das estradas e da água. Cada uma foi alcançada por um trajeto livre e cultivada pela lateral, incluindo plantar, regar, crescer e colher.
- Movimento rápido não atravessa os oito canteiros nem as cercas. Os quatro portões permitem entrar e sair.
- A navegação por mouse preserva pontos nos corredores de 16 pixels. A antiga seleção em blocos de 32 pixels impedia escolher alguns desses lugares; isso foi corrigido e coberto pelos testes.
- Os 20 objetos da vila usam bases físicas; casas, torre da prefeitura, objetos e jardins reservam espaço contra as copas das árvores.
- Migração da edição 7 preserva exatamente as 48 plantas, estágios e recursos. O arquivo original recebe backup antes da edição 8.
- Os oito retratos são PNGs individuais e diferentes. Testes verificam os bytes realmente embutidos, dimensões, nomes, recortes completos, proporções e avisos.
- Percurso no executável Linux com janela e entradas XTest: cultivo ao lado do canteiro, colisão durante corrida, passagem pelo portão, três destinos de mouse nos corredores, entrada/saída da prefeitura, quadro externo e interno, abas do mapa e galeria dos oito moradores.
- As dez áreas foram abertas no executável distribuído no Xvfb/Mesa, por 60 quadros cada.
- O GIF foi capturado da edição 8; os oito moradores mudaram de posição ou área durante a captura.

Os retratos completos foram gerados para o projeto e integrados sem recortar cabelos, mãos ou vizinhos de um atlas. A galeria e os diálogos apresentados no README são capturas do jogo. As cenas de cultivo da demonstração usam uma partida preparada com plantas maduras para tornar o layout visível.

Os workflows verificam novamente o código e o executável publicado. Os testes gráficos usam Linux/Mesa e não substituem a verificação em todas as distribuições, placas e monitores.
