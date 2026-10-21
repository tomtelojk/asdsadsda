# Vale do Sol 🌻

Um jogo de fazenda original em português, com aplicativo de desktop para **Linux x86_64** e versão de navegador. Explore a Fazenda Girassol e a Vila das Flores, cultive três tipos de vegetais e conheça seus novos vizinhos.

## Baixar e jogar no Linux

Os arquivos ficam em **[Releases](https://github.com/tomtelojk/asdsadsda/releases/latest)**.

### AppImage

Baixe `Vale-do-Sol-Linux-2.0.0.AppImage`, abra suas propriedades e marque **Permitir executar como programa**. Depois abra o arquivo. Pelo terminal:

```sh
chmod +x Vale-do-Sol-Linux-2.0.0.AppImage
./Vale-do-Sol-Linux-2.0.0.AppImage
```

### Alternativa sem FUSE

Extraia `Vale-do-Sol-Linux-2.0.0.tar.gz` e abra o executável `vale-do-sol` dentro da pasta extraída. Mantenha todos os arquivos dessa pasta juntos. Não é necessário instalar Node.js, Electron ou abrir um navegador.

Os pacotes dependem das bibliotecas de desktop da sua distribuição e do suporte a sandbox do Chromium. AppImage também requer FUSE 2; use o tar.gz se FUSE não estiver disponível. Não execute como root. Os pacotes desta versão não são assinados e não incluem atualizações automáticas.

## O que há para fazer

- Cultivar cenouras, nabos e morangos, com energia, rega diária, colheita e venda.
- Caminhar por dois mapas, com colisões e busca de caminho ao clicar.
- Conversar com Lia, Bento, Iara e Theo. Os moradores mudam de lugar à tarde.
- Construir amizades, dar um vegetal de presente por dia e completar três missões.
- Comprar sementes com Lia, melhorar o regador com Bento e entregar cenouras a Theo.
- Ganhar uma vara de Iara e pescar reagindo à fisgada.
- Recolher madeira e pedras, descansar e aproveitar a chuva a cada quatro dias.
- Exportar/importar saves, ativar efeitos sonoros e jogar em tela cheia.

É um jogo pequeno em evolução. Não há multiplayer, interiores de casas ou sistema de casamento. As quatro estações mudam no calendário a cada 28 dias; as culturas atuais podem ser plantadas em qualquer estação.

## Controles

| Ação | Controle |
| --- | --- |
| Andar | WASD ou setas; clique no chão |
| Enxada, sementes, regador, cesta, explorar | 1, 2, 3, 4, 5 |
| Usar ferramenta | Clique no canteiro; o personagem caminha até ele |
| Conversar, recolher, entrar na loja | E perto do alvo ou clique nele |
| Pescar | F perto do lago, depois Espaço quando aparecer AGORA |
| Trocar sementes | Aba Mochila → Plantar |
| Descansar | Botão Descansar ou E na porta de casa |
| Fechar/cancelar | Escape |
| Ajuda | H |
| Tela cheia no desktop | F11 |

O relógio para durante diálogos, lojas e ajuda. Às 22h o personagem volta para casa e começa um novo dia.

## Saves

O desktop salva localmente no perfil do aplicativo (`~/.config/vale-do-sol/` em instalações Linux convencionais), separado dos saves do navegador. Use **Exportar save** para fazer backup e **Importar** para transferir o progresso entre desktop e navegador. Saves da versão 1 são migrados automaticamente quando encontrados no mesmo armazenamento. A importação de arquivos aceita apenas saves validados da versão 2. Não mova ou remova o perfil para atualizar o jogo.

## Versão offline de navegador

Baixe `vale-do-sol.zip`, extraia e abra `vale-do-sol.html`. Não precisa de servidor ou conexão. Alguns navegadores restringem saves em arquivos locais; nesse caso, use Exportar save. O GitHub Pages precisa ser habilitado pelo administrador do repositório para existir um endereço jogável online.

## Desenvolvimento

Requer Node.js 22.16+ ou 24 e npm. Electron 45 inclui seu próprio runtime para a versão distribuída.

```sh
npm ci
npm run dev
```

```sh
npm test
npm run build
npm run desktop
npm run package:linux
```

O build gera `dist/` e atualiza o HTML offline. Os pacotes de desktop ficam em `release/`. O Electron permanece com sandbox, isolamento de contexto e integração de Node desativada na janela do jogo. Não há acesso remoto ou login.

Para testes de navegador:

```sh
npx playwright-core install chromium
npm run dev
# em outro terminal:
npm run test:browser
```

Use `CHROMIUM_PATH=/caminho/para/chromium` se preferir um navegador instalado. O workflow **Verificar jogo** roda os testes e o build. O workflow manual **Publicar desktop Linux** recria os downloads da versão indicada no package.json.

## Estrutura

- `game/state.js`: estado, economia, cultivo, missões, amizade e validação de saves.
- `game/world.js`: mapas, obstáculos e busca de caminho A*.
- `game/render.js`: arte pixelada original e animação em Canvas.
- `src.js`: controles, interface, diálogos e ciclo do jogo.
- `desktop/main.cjs`: aplicativo Electron e menus nativos.
- `tests/`: regras de jogo e fluxos reais no navegador.

Arte e personagens originais. Código sob licença MIT.
