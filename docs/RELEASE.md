O Vale do Sol agora é um jogo de desktop em **C# com .NET 10**. A lógica, a interface e os saves foram reescritos em C#. raylib fornece os gráficos e a janela; não há HTML ou Electron nesta edição.

### Como jogar

Baixe **Vale-do-Sol-CSharp-3.0.0-Linux-x86_64**, marque “Permitir executar como programa” nas propriedades e abra. O arquivo já inclui o runtime .NET e a biblioteca gráfica. Não é necessário instalar SDK, navegador ou FUSE.

Alternativa: extraia o `.tar.gz` e abra `ValeDoSol` dentro da pasta.

**Sistema:** Linux x86_64, glibc 2.38 ou superior, sessão X11/XWayland e OpenGL 3.3; por exemplo Ubuntu 24.04. Aplicativo sem assinatura e sem atualizador automático.

### Conteúdo

Fazenda com três culturas, vila com quatro NPCs, diálogos, amizade, presentes, três missões, loja, regador melhorável, pesca, coleta, chuva e passagem dos dias. Saves locais e backup/restauração.

**Controles:** WASD/setas para andar, 1–5 para ferramentas, clique para cultivar, E para conversar/recolher, F e Espaço para pescar, H para ajuda e F11 para tela cheia.

**Saves:** `~/.local/share/vale-do-sol-csharp/`. A versão antiga fica preservada na release 2.0. Os saves do navegador/Electron não são importados nesta edição.

O código C# está na branch main. Para editar e testar: `./scripts/run.sh`. O script compila novamente antes de abrir o jogo.

Validação: build sem avisos, testes de regras e saves, abertura gráfica da fazenda e da vila com Xvfb, e verificação do executável com runtime incluído. Não foi testado em todas as distribuições/GPU Linux.
