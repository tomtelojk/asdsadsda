# Vale do Sol 🌱

Jogo de fazenda para **Linux em C# e .NET 10**, com raylib para janela e gráficos. A edição 4 traz exploração em tela inteira, câmera que acompanha o personagem e interface de RPG sobre o cenário.

![Fazenda e câmera da edição 4](docs/fazenda-csharp.png)

## Baixar e jogar

**[Downloads da versão atual](https://github.com/tomtelojk/asdsadsda/releases/latest)**

Baixe `Vale-do-Sol-CSharp-4.0.0-Linux-x86_64`, marque **Permitir executar como programa** nas propriedades e abra. O `.tar.gz` contém o mesmo executável e as instruções.

```sh
chmod +x Vale-do-Sol-CSharp-4.0.0-Linux-x86_64
./Vale-do-Sol-CSharp-4.0.0-Linux-x86_64
```

O runtime .NET e a biblioteca gráfica estão incluídos. Não usa navegador, HTML ou Electron; não precisa de SDK ou FUSE para jogar. Requer Linux x86_64 com glibc 2.38+, sessão X11/XWayland e OpenGL 3.3, por exemplo Ubuntu 24.04. O .NET extrai bibliotecas em `~/.net/` na primeira execução.

## Uma vida no campo

- Explore fazenda e vila com mapas de 2048 × 1408, bosques, estradas e lagos.
- Caminhe com câmera seguindo o personagem; corra com Shift.
- Equipe ferramentas na barra e cultive os canteiros ao seu alcance.
- Plante cenouras, nabos e morangos; regue, colha e venda.
- Entre na sua casa, durma na cama e acorde para um novo dia.
- Entre no armazém e converse com Lia no balcão para comprar e vender.
- Conheça Lia, Bento, Iara e Theo; converse, dê presentes e complete missões.
- Melhore o regador, pesque e recolha madeira e pedras.
- Acompanhe dia/noite, chuva e o calendário sazonal.

Mochila, diário e relações ficam em menus abertos por teclas. Os diálogos têm retratos e texto gradual. Arte e personagens originais, inspirados no gênero de RPGs de fazenda.

É uma edição pequena em desenvolvimento. Ainda não há casamento, multiplayer ou música. As estações mudam no calendário; as culturas atuais podem crescer o ano todo. Os pacotes não incluem atualizador automático.

## Controles

| Ação | Controle |
| --- | --- |
| Caminhar / correr | WASD ou setas / Shift |
| Equipar ferramentas | 1–5 |
| Equipar cenoura, nabo, morango e vara | 6, 7, 8, 9 |
| Cultivar ao alcance do personagem | Clique ou Espaço |
| Caminhar até um ponto | Botão direito; clique com Explorar selecionado |
| Conversar, entrar, recolher ou usar a cama | E perto do alvo |
| Pescar / puxar na fisgada | F / Espaço |
| Mochila / diário / moradores | I ou Tab / J / R |
| Ajuda / pausa | H / Escape |
| Salvar / tela cheia | F5 / F11 |

Siga a estrada à direita da fazenda até o limite do mapa para chegar à vila. Volte pela saída à esquerda da vila. Entre em portas com E; para sair de um interior, caminhe pela porta na parte de baixo. O tempo para durante menus/diálogos e perda de foco. Às 22h começa um novo dia na sua casa.

## Editar e testar em C#

Requer .NET SDK 10 e bibliotecas gráficas do Linux. O instalador opcional coloca o SDK em uma pasta local do projeto, sem sudo:

```sh
./scripts/setup-dotnet.sh
./scripts/run.sh
```

Edite os arquivos `.cs`, feche o jogo e execute `./scripts/run.sh` para recompilar e ver as mudanças. O primeiro build baixa raylib do repositório oficial e confere o SHA-256. O SDK local tem SHA-512 verificado.

```sh
./scripts/test.sh       # regras, câmera, colisões, navegação, interiores e saves
./scripts/build.sh      # compilar com avisos tratados como erros
./scripts/package.sh    # gerar executável Linux com .NET incluído em release/
```

`ValeDoSol.slnx` pode ser aberto em uma IDE com suporte a C#/.NET 10. Bibliotecas Linux: `libgl1`, `libx11-6`, `libxrandr2`, `libxi6`, `libxcursor1`, `libxinerama1`.

## Organização

- `State.cs`: estado, cultivo, economia, amizades, missões, colisões e busca de caminho.
- `Game.cs`: controles, câmera, interações, cenas e passagem dos dias.
- `World.cs`: mapas, interiores, personagens e animações.
- `Ui.cs`: HUD, ferramentas, menus e diálogos.
- `Ray.cs`: interop explícito com a biblioteca gráfica raylib.
- `SelfTests.cs`: verificações executáveis sem display.

Todos estão em `src/ValeDoSol/`. SDK, dependências e saídas de build ficam fora do Git.

## Saves

Pasta padrão: `~/.local/share/vale-do-sol-csharp/`, ou `$XDG_DATA_HOME/vale-do-sol-csharp/` se configurado. O save principal é `save.json`. A mochila oferece **Criar backup** e **Restaurar** usando `backup.json` nessa pasta.

Os saves C# da edição 3 continuam carregando. Antes da primeira atualização, o save antigo é copiado para `save-before-v4.json`. Para transferir, copie `backup.json` para a pasta de saves no destino e use Restaurar. Os saves do antigo navegador/Electron continuam separados e não são importados.

```sh
./ValeDoSol --save-dir /caminho/para/uma/partida
```

## Validação gráfica

`--self-test` executa as regras sem abrir janela. `--smoke` renderiza 60 quadros, salva e fecha; `--village`, `--home` e `--shop` selecionam áreas para esse teste. `--screenshot arquivo.png` salva uma captura no final do smoke test. Os workflows usam Xvfb, o que não substitui testes em todas as distribuições/GPU.

Código e arte originais sob MIT. raylib: zlib/libpng. A fonte DejaVu Sans e sua licença estão incluídas. O executável apresenta os avisos com `--licenses`.
