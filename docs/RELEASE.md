A edição 6 refaz o mapa, a física e os cenários do Vale do Sol. Casas, portas, móveis e colisões passam a usar as mesmas medidas; árvores e copas ficam fora das estradas e entradas.

### Cenários e interiores

- Texturas de pixel art do Liberated Pixel Cup em casas, vegetação, pisos e móveis, com créditos e licença incluídos.
- Telhados contínuos, portas alinhadas aos acessos, sombras, janelas e jardins.
- Sete interiores acessíveis: sua casa, armazém, oficina, padaria, biblioteca, casa das flores e casa do lago.
- Móveis com colisão e objetos que contam pequenas histórias ao examinar com E.
- Fazendeiro masculino, com sprites originais e sombreamento revisado.

### Movimento e moradores

- Colisão pela área dos pés, pequenos passos para impedir atravessar obstáculos em alta velocidade, deslizamento junto às paredes e navegação que respeita a largura do personagem.
- Agendas individuais para os oito moradores, com horários e destinos distintos; visitas à fazenda, compras, refeições, encontros e locais diferentes de pesca.
- Entrada e saída pelas portas, pequenos deslocamentos durante as atividades e planos diferentes em dias de chuva e fins de semana.
- Posições e áreas dos moradores gravadas no save, evitando que todos retornem ao mesmo lugar ao reabrir o jogo.
- A simulação continua quando o jogador está em outra área. Menus e diálogos pausam o tempo.

### Download

Baixe **Vale-do-Sol-CSharp-6.0.0-Linux-x86_64**, permita execução nas propriedades e abra. Alternativa: extraia o `.tar.gz` e abra `ValeDoSol`.

Requer Linux x86_64, glibc 2.38+, X11/XWayland e OpenGL 3.3, como Ubuntu 24.04. O runtime .NET está incluído.

Saves C# das edições 3–5 são importados com cópia original em `save-before-v6.json`. Recursos, cultivo e amizades são preservados.

### Controles

WASD/setas: andar. Shift: correr. E: conversar, entrar ou examinar. Botão direito: caminhar até um ponto. I/J/R: mochila, diário e moradores. Q/T/Y: assuntos durante uma conversa. Feche os menus para o relógio e os moradores continuarem. Siga a estrada leste da fazenda para visitar a vila.

### Créditos e validação

Cenários LPC por Lanea Zimmerman (Sharm), CC BY-SA 3.0. PNGs originais e licença no repositório e no pacote. Código e personagens originais sob MIT; raylib e DejaVu mantêm suas licenças. Use `--licenses` para ler os avisos.

Validação inclui regras de cultivo/economia, migração de saves, portas e interiores, movimento rápido e diagonal, acesso aos móveis e uma simulação de um dia com os oito moradores. Também foram executados testes gráficos e interações com teclado e mouse. Ainda não foi validado em todas as distribuições/GPU. O jogo segue em desenvolvimento, sem casamento ou multiplayer.
