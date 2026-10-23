# Vale do Sol 🌱

RPG de fazenda para Linux, em **C# / .NET 10 / raylib**. A edição 7 traz a campanha **O Caderno das Estações**, novas tarefas para os oito moradores e uma revisão dos recortes, proporções e apresentação da pixel art.

![Fazenda da edição 7](docs/fazenda-csharp.png)

**[Baixar o executável Linux 7.0.0](https://github.com/tomtelojk/asdsadsda/releases/download/v7.0.0/Vale-do-Sol-CSharp-7.0.0-Linux-x86_64)** · [Todos os downloads](https://github.com/tomtelojk/asdsadsda/releases/latest)

Permita execução nas propriedades do arquivo e abra. O `.tar.gz` contém o mesmo jogo, instruções e os arquivos de arte/licença LPC.

```sh
chmod +x Vale-do-Sol-CSharp-7.0.0-Linux-x86_64
./Vale-do-Sol-CSharp-7.0.0-Linux-x86_64
```

O .NET e a biblioteca gráfica estão incluídos. Requer Linux x86_64 com glibc 2.38+, X11/XWayland e OpenGL 3.3, como Ubuntu 24.04. Não precisa instalar SDK ou FUSE. Bibliotecas nativas são extraídas em `~/.net/` ao abrir pela primeira vez.

## O Caderno das Estações

Você é o neto de Aurora e acaba de herdar a Fazenda Girassol. Junto da chave, ela deixou uma carta e um caderno incompleto sobre a antiga horta comunitária da vila. Otávio guardou uma página; os outros moradores guardaram suas próprias lembranças.

A campanha tem **seis capítulos**, com cultivo, materiais, pesca e amizades. Recupere as bancadas, forme um canteiro compartilhado e prepare uma mesa para reunir a vila. Os objetivos, requisitos, contato e recompensas aparecem no diário **J**. Entregue os materiais perto do morador indicado; o último encontro acontece na vila entre 17h e 21h, em um dia posterior aos preparativos. Não há prazo para concluir os capítulos, e o jogo continua após o desfecho.

A carta pode ser relida pelo diário. Cada morador tem comentários próprios sobre cada etapa, disponíveis no assunto **O caderno [U]**. Veja [a história e seus capítulos](docs/HISTORIA.md) para os detalhes, incluindo spoilers.

## Arte, mapa e movimento

- Recortes completos de portas, janelas, camas, árvores e móveis, com dimensões e pontos de base catalogados. Os objetos mantêm a proporção da imagem original.
- Personagens com pixels alinhados à câmera e filtro de ponto. O modo padrão **Imagem: nítida** amplia a tela por fatores inteiros; pode deixar margens em monitores maiores. A pausa oferece **Imagem: preencher**, que ocupa mais espaço e permite escala fracionária.
- Casas, portas, móveis e colisões usam o mesmo modelo de mapa; árvores e copas ficam fora das estradas e acessos.
- Colisão pela área dos pés, passos curtos, deslizamento junto às paredes e caminhos que consideram a largura do personagem. A animação do corpo não determina a colisão.
- Sete interiores acessíveis: sua casa, armazém, oficina, padaria, biblioteca, casa das flores e casa do lago. Os móveis podem ser examinados com E.
- A recuperação das bancadas e da horta deixa marcas visíveis no cenário durante a campanha.

![Interior da casa](docs/casa-csharp.png)

![Biblioteca de Otávio](docs/biblioteca-csharp.png)

## Moradores em movimento

**Lia, Bento, Iara, Theo, Dora, Miguel, Ceci e Otávio** têm agendas próprias. Entram e saem pelas portas, visitam a fazenda e outros estabelecimentos, fazem refeições e encontram os vizinhos. Chuva, dias úteis e fins de semana mudam os destinos.

Durante um compromisso, os moradores executam pequenas tarefas ligadas ao trabalho: Lia confere pedidos, Bento consulta e mede peças, Iara procura pontos na margem, Theo e Miguel examinam mudas, Dora prepara fornadas, Ceci consulta suas anotações e Otávio organiza leituras. Os percursos alternam deslocamentos e pausas; poses, objetos e balões ajudam a mostrar o que fazem.

![Movimento dos moradores no jogo](docs/vila-em-movimento.gif)

As posições e áreas dos moradores são gravadas. A simulação continua quando o jogador está em outra área; menus e diálogos pausam o tempo. O menu **R** mostra as atividades atuais.

Os diálogos oferecem assuntos de trabalho, vida pessoal, dia e história principal. Clima, horário, estação e amizade influenciam as falas. O save lembra as últimas escolhas para evitar a mesma resposta consecutiva; presentes recebem comentários pessoais.

![Moradores e retratos](docs/moradores-csharp.png)

## Cultivar e jogar

Prepare a terra, plante cenouras, nabos e morangos, regue, durma e colha. Visite Lia para comprar sementes e vender produtos, melhore o regador com Bento e converse com Iara para ganhar uma vara de pesca. O fazendeiro é masculino.

| Ação | Controle |
| --- | --- |
| Caminhar / correr | WASD ou setas / Shift |
| Ferramentas / sementes / vara | 1–5 / 6–8 / 9 |
| Cultivar perto do personagem | Clique; Espaço usa o canteiro à frente |
| Caminhar até um ponto | Botão direito |
| Conversar, entrar, examinar, recolher | E |
| Pescar / puxar na fisgada | F / Espaço |
| Mochila / diário / moradores | I ou Tab / J / R |
| Assuntos da conversa | Q / T / Y / U |
| Ajuda / pausa / salvar / tela cheia | H / Escape / F5 / F11 |

A estrada à direita da fazenda leva à vila; volte pela saída à esquerda. Dentro das casas, caminhe até a porta inferior para sair. Durma ao lado da cama da sua casa. O dia termina às 22h. Lia atende no armazém das 8h às 17h em dias úteis e até 13h nos fins de semana.

Ainda é um jogo pequeno em desenvolvimento. Não há casamento, multiplayer ou música; referências a músicas nas falas fazem parte da história de Ceci. As três culturas crescem em todas as estações.

## Editar o projeto

```sh
./scripts/setup-dotnet.sh  # opcional: SDK local, sem sudo
./scripts/run.sh          # recompila e abre o jogo
./scripts/test.sh         # regras, história, física, rotinas e saves
./scripts/package.sh      # executável Linux com runtime em release/
```

Abra `ValeDoSol.slnx` em uma IDE com suporte a C#/.NET 10. O primeiro build baixa raylib e verifica SHA-256. O SDK local tem SHA-512 verificado.

Em `src/ValeDoSol/`: `Map.cs` define o mapa e os móveis; `Physics.cs` contém colisão e navegação; `Social.cs` define agendas e falas; `NpcBehavior.cs` define tarefas; `Story.cs` contém a campanha; `SpriteCatalog.cs` registra os recortes; `Scenery.cs` monta os cenários LPC; `Art.cs` desenha os personagens originais. `Game.cs` controla os atores e as interações, e `State.cs` cuida de cultivo, economia e saves. `Viewport.cs` mantém a apresentação e o mouse na mesma escala.

## Saves e validação

A pasta padrão é `~/.local/share/vale-do-sol-csharp/`, ou `$XDG_DATA_HOME/vale-do-sol-csharp/`. O arquivo principal é `save.json`. A mochila oferece backup/restauração em `backup.json`. Ao importar um save das edições 3–6, a edição 7 guarda o original em `save-before-v7.json`. Cultivo, recursos e amizades são preservados; a nova história começa pela carta. Se um móvel ocupar uma posição anterior, o personagem é colocado em um ponto livre próximo.

`--save-dir DIRETORIO` escolhe outra partida. `--self-test` verifica as regras sem display. `--smoke --scene BAKERY --screenshot imagem.png` abre uma área por 60 quadros e salva uma captura. Áreas disponíveis: `FARM`, `VILLAGE`, `HOME`, `SHOP`, `WORKSHOP`, `BAKERY`, `LIBRARY`, `GREENHOUSE`, `COTTAGE`. Os flags antigos `--village`, `--home` e `--shop` continuam disponíveis nos testes.

A suíte cobre cultivo e economia, a progressão da campanha, recompensas e entregas, migração e validação de saves, recortes e escalas, ferramentas por direção, portas, móveis e colisões. Inclui trajetos dos moradores e simulação ao longo do dia. Os workflows também abrem todas as áreas no Xvfb. Esses testes não cobrem todas as distribuições e GPUs.

## Créditos

Código e personagens originais: MIT. Cenários LPC: **Lanea Zimmerman (Sharm)**, [Liberated Pixel Cup](https://github.com/OpenGameArt/LiberatedPixelCup), **CC BY-SA 3.0**. Os PNGs originais, os caminhos de origem e os avisos estão em [Resources/Lpc](src/ValeDoSol/Resources/Lpc). As artes LPC mantêm sua licença, separada do código. raylib: zlib/libpng. Fonte DejaVu Sans: licença incluída. O executável apresenta os avisos com `--licenses`.
