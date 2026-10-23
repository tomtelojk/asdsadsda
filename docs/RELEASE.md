Vale do Sol 8.0.0 — **A vila floresce**

A horta saiu da estrada, os moradores receberam novas artes e a Vila das Flores ganhou uma prefeitura para visitar.

### Horta e física

- Oito canteiros elevados ao sul da estrada, com 48 parcelas e corredores entre eles.
- Canteiros e cercas sólidos, com quatro portões; todas as plantas podem ser cuidadas pela lateral.
- Desenho, seleção de parcelas, colisões e navegação usam o mesmo modelo. O caminho de acesso não atravessa os canteiros nem a água.
- O canteiro comunitário, bancos, floreiras, postes e outros objetos da vila também têm bases físicas.

### Moradores e arte

- Oito novos retratos ilustrados, gerados para o projeto, com rostos, cabelos e roupas distintos.
- Imagens individuais desenhadas por inteiro, mantendo a proporção e a transparência; sprites de caminhada atualizados para acompanhar os personagens.
- Filtro de ponto para o mundo e filtragem própria para reduzir as ilustrações dos retratos.
- Agendas incluem visitas à prefeitura; os botânicos observam a nova posição da horta.

### Prefeitura e vila

- Prefeitura com fachada própria, torre de relógio e interior acessível: atendimento, arquivos e mesa de reunião.
- Novos bancos, floreiras, lampiões, poço, banca, placas e quadro de avisos.
- Quadro consultável com **E**, na vila ou dentro da prefeitura: avisos, mapa e localização atual dos moradores.
- **M** abre o mapa de qualquer área. Alterne entre fazenda e vila, encontre as construções e identifique os pontos dos moradores com o mouse.
- Dez áreas no total, incluindo oito interiores acessíveis. Os acessos às construções e à horta ficam livres de árvores.

A campanha **O Caderno das Estações**, em seis capítulos, continua disponível. Cultive, desenvolva amizades e ajude a recuperar a antiga horta comunitária. O diário J mostra objetivos, contatos e recompensas; a vida na fazenda continua depois do desfecho.

### Download e saves

Baixe **Vale-do-Sol-CSharp-8.0.0-Linux-x86_64**, permita execução nas propriedades e abra. Alternativa: extraia o `.tar.gz` e abra `ValeDoSol`.

Requer Linux x86_64, glibc 2.38+, X11/XWayland e OpenGL 3.3, como Ubuntu 24.04. O runtime .NET está incluído.

Saves C# das edições 3–7 são importados com cópia original em `save-before-v8.json`. As 48 parcelas mantêm plantas e crescimento ao mudar de lugar. Recursos, amizades e a campanha existente são preservados; posições ocupadas por novos obstáculos são ajustadas para um ponto livre próximo.

### Controles

WASD/setas: andar. Shift: correr. E: conversar, entrar, examinar ou consultar o quadro. Botão direito: caminhar até um ponto. I/J/R: mochila, diário e moradores. M: mapa. Q/T/Y/U: assuntos durante a conversa. Feche os menus para o relógio e os moradores continuarem.

### Créditos e validação

Cenários LPC por Lanea Zimmerman (Sharm), CC BY-SA 3.0. PNGs originais e licença no repositório e no pacote. Código e pixel art original sob MIT; novos retratos gerados para o projeto, com arquivos e avisos em Resources/Characters. raylib e DejaVu mantêm suas licenças. Use `--licenses` para ler os avisos.

A suíte verifica o ciclo de cultivo em todas as 48 parcelas pela lateral, canteiros fora da estrada e da água, colisões de canteiros e cercas, passagem pelos portões, acesso à prefeitura, objetos da vila, história e migração dos saves. Os workflows abrem as dez áreas no Xvfb.

O jogo continua em desenvolvimento: ainda não há casamento, multiplayer ou música, e as três culturas crescem em todas as estações.

Validação desta edição: 1.961 verificações aprovadas no executável Linux, incluindo todas as 48 parcelas, navegação por mouse nos corredores e os oito PNGs distribuídos. O percurso com janela também confirmou cultivo pela lateral, bloqueio dos canteiros, portões, prefeitura, quadros, mapa e galeria.
