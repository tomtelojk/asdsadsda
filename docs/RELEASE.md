A edição 5 renova a arte e dá mais vida à vila, usando a referência visual enviada para orientar os novos sprites originais. O protagonista é um fazendeiro masculino de cabelo curto, chapéu de palha e macacão verde.

### Novidades

- Nova folha de sprites com animações de caminhada em quatro direções e retratos próprios para o fazendeiro e os oito moradores.
- Células separadas por margens transparentes para evitar vazamentos de imagens vizinhas. Não há rótulos ou fundo da referência dentro do jogo.
- Árvores com folhagem e sombra, arbustos floridos, cercas, troncos, pedras e três estágios visuais de crescimento das culturas.
- Casas com telhas e madeira detalhadas, banca de pães, horta de ervas, flores e bandeirinhas na vila.
- Personagens e construções ordenados pela profundidade; árvores ficam translúcidas quando encobrem o jogador.
- Dora, Miguel, Ceci e Otávio se juntam a Lia, Bento, Iara e Theo.
- Rotinas com trabalho, almoço, passeios e retorno às proximidades das casas. Chuva e fins de semana mudam os planos; os trajetos continuam sendo simulados enquanto o jogador está na fazenda.
- Lia atende no armazém das 8h às 17h, ou até 13h nos fins de semana; depois sai para a vila, sem uma cópia simultânea no balcão.
- Diálogos sobre trabalho, histórias pessoais e o dia, selecionados com Q/T/Y ou pelo mouse. Falas variam com visitas, amizade, clima e horário; presentes recebem respostas próprias.
- Menu de moradores com os oito retratos, amizades e atividades atuais.
- Importação de saves C# das edições 3 e 4, preservando recursos e amizades e criando `save-before-v5.json` antes da atualização.

### Baixar e jogar

Baixe **Vale-do-Sol-CSharp-5.0.0-Linux-x86_64**, permita execução nas propriedades e abra. Alternativa: extraia o `.tar.gz` e abra `ValeDoSol`. O runtime .NET está incluído.

Requer Linux x86_64, glibc 2.38+, X11/XWayland e OpenGL 3.3, como Ubuntu 24.04. Não precisa instalar SDK ou FUSE.

**WASD** para andar, **Shift** para correr, **1–9** para equipar, **clique/Espaço** para cultivar perto do personagem, **E** para conversar/entrar/recolher e **I/J/R** para mochila/diário/moradores. Durante conversas, **Q/T/Y** escolhem assuntos. Siga a estrada leste da fazenda para chegar à vila.

Código e arte originais em C#/.NET com raylib, sob MIT. O projeto continua em desenvolvimento: não há casamento, multiplayer ou música; menções a músicas nos diálogos fazem parte da história dos personagens. As casas decorativas da vila não têm interiores acessíveis nesta edição. As novas artes foram desenhadas no código; a imagem enviada serviu de referência, sem depender dos recortes defeituosos dela.

Validação: build sem avisos, verificações de regras, rotinas, caminhos, diálogos, margens do atlas e migração de saves, além de renderização nativa e percurso com teclado e mouse. Não foi validado em todas as distribuições/GPU Linux.
