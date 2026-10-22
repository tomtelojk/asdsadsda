A edição 4 muda a apresentação e a exploração do Vale do Sol: o cenário ocupa toda a janela e a câmera acompanha o personagem, com interface de jogo sobre o mapa.

### Novidades

- Mapas ampliados para 2048 × 1408, com estradas, bosques e lagos.
- Câmera próxima, movimento em quatro direções e corrida com Shift.
- Barra de ferramentas de nove posições e HUD com relógio, moedas e energia.
- Mochila, missões e moradores em menus abertos por teclas.
- Cultivo por proximidade e animação do uso das ferramentas.
- Interior da casa com cama para dormir e interior do armazém com atendimento no balcão.
- Retratos nos diálogos e texto que aparece gradualmente.
- Saves C# da edição 3 preservados, com cópia de segurança antes da atualização.

Mantém as três culturas, quatro NPCs, amizade, presentes, missões, pesca, loja e chuva. Arte e personagens originais. Continua em C#/.NET com raylib, sem HTML ou Electron.

### Baixar

Baixe **Vale-do-Sol-CSharp-4.0.0-Linux-x86_64**, permita execução nas propriedades e abra. Alternativa: extraia o `.tar.gz` e abra `ValeDoSol`. O runtime .NET está incluído.

Requer Linux x86_64, glibc 2.38+, X11/XWayland e OpenGL 3.3, como Ubuntu 24.04. Não precisa instalar SDK ou FUSE.

### Controles principais

**WASD** para andar, **Shift** para correr, **1–9** para equipar, **clique/Espaço** para usar ferramenta perto do personagem, **E** para conversar/entrar/recolher, **I/J/R** para os menus, **Escape** para pausa, **F11** para tela cheia. Siga a estrada leste da fazenda para chegar à vila. Dentro de casa, aproxime-se da cama e aperte E para dormir.

Código atualizado na branch main. Para editar e testar, execute `./scripts/run.sh`.

Validação: build sem avisos, verificações de regras/câmera/colisões/saves, renderização da fazenda, vila e interiores, e percurso real com teclado e mouse. Não foi validado em todas as distribuições/GPU Linux. Ainda não há multiplayer ou casamento.
