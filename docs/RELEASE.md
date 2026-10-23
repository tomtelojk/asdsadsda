Vale do Sol 7.0.0 — **O Caderno das Estações**

Você herda a fazenda de Aurora e um caderno incompleto. A nova campanha tem seis capítulos: conheça os vizinhos, cultive suas primeiras colheitas, recupere as bancadas da antiga horta e organize uma mesa compartilhada. O diário mostra objetivos, entregas, contatos e recompensas. A vida na fazenda continua após o desfecho.

### Arte e apresentação

- Recortes completos e catalogados para móveis, árvores, portas e janelas, com proporções preservadas.
- Ajustes nos retratos e na posição dos pés; pixels dos personagens alinhados à câmera.
- Modo padrão **Imagem: nítida**, com ampliação inteira e margens quando necessário. **Imagem: preencher**, na pausa, permite ocupar mais espaço da janela.
- Limites dos móveis ajustados ao desenho, fonte ordenada por profundidade e estradas sem copas de árvores.
- Sete interiores acessíveis e mudanças visuais nas bancadas e na horta durante a história.

### Moradores e código

- Oito moradores com agendas por horário, dia útil, fim de semana e chuva, usando portas e caminhos do mapa.
- Tarefas próprias durante os compromissos, com pausas, poses, pequenos objetos e balões: estoque, oficina, pesca, plantas, fornadas e leitura.
- Conversas pessoais e de trabalho, falas de clima/horário/estação/amizade e assunto **O caderno [U]** para a campanha. As últimas respostas são lembradas para evitar repetição consecutiva.
- Correções de navegação com caminho vazio, uso da ferramenta pela direção do personagem, colisão com moradores e tratamento do limite de moedas ao vender.
- Simulação com passos fixos, independente da frequência de desenho, com recuperação limitada após travamentos longos.

### Download e saves

Baixe **Vale-do-Sol-CSharp-7.0.0-Linux-x86_64**, permita execução nas propriedades e abra. Alternativa: extraia o `.tar.gz` e abra `ValeDoSol`.

Requer Linux x86_64, glibc 2.38+, X11/XWayland e OpenGL 3.3, como Ubuntu 24.04. O runtime .NET está incluído.

Saves C# das edições 3–6 são importados com cópia original em `save-before-v7.json`. Recursos, cultivo e amizades são preservados; a nova campanha começa pela carta.

### Controles

WASD/setas: andar. Shift: correr. E: conversar, entrar ou examinar. Botão direito: caminhar até um ponto. I/J/R: mochila, diário e moradores. Q/T/Y/U: assuntos durante uma conversa. Feche os menus para o relógio e os moradores continuarem. Siga a estrada leste da fazenda para visitar a vila.

### Créditos e validação

Cenários LPC por Lanea Zimmerman (Sharm), CC BY-SA 3.0. PNGs originais e licença no repositório e no pacote. Código e personagens originais sob MIT; raylib e DejaVu mantêm suas licenças. Use `--licenses` para ler os avisos.

A suíte verifica cultivo/economia, progressão completa da história, entregas e recompensas, migração e limites dos saves, recortes e escalas, portas e interiores, ferramentas e colisões. Os workflows abrem as nove áreas no Xvfb. O jogo continua em desenvolvimento: ainda não há casamento, multiplayer ou música, e as culturas atuais crescem em todas as estações.

Validação desta edição: 1.217 verificações aprovadas no executável distribuído; percurso nativo com cultivo, cama, comércio e sete interiores; seis entregas da história pela interface. O GIF do README foi capturado do jogo, com os oito moradores mudando de posição ou área.
