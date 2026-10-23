using static ValeDoSol.Logic;
namespace ValeDoSol;

public readonly record struct Appointment(int start, Point at, string activity, Scene scene = Scene.VILLAGE);
public static class Social
{
    public const int Count = 8;
    static int Visit(State s, int npc) { int value = s.conversations[npc]; s.conversations[npc] = Math.Min(10_000_000, value + 1); return value; }
    static (int day, int stage, int festivalDay, bool gathering) cachedCalendar;
    public static bool FestivalDay(State s) => s.story_day > 0 && (s.story_stage == 5 && s.day > s.story_day && s.friendship.All(n => n >= 2) || s.story_stage == 6 && s.day >= s.story_day && s.day <= s.story_day + 1);
    static readonly Dictionary<int, Appointment[]> plans = [];
    public static Appointment[] Schedule(int npc, State s) { var calendar = (s.day, s.story_stage, s.story_day, FestivalDay(s)); if (cachedCalendar != calendar) { plans.Clear(); cachedCalendar = calendar; } if (!plans.TryGetValue(npc, out var plan)) plans[npc] = plan = BuildSchedule(npc, s); return plan; }
    static Appointment[] BuildSchedule(int npc, State s)
    {
        bool weekend = (s.day - 1) % 7 >= 5;
        Appointment A(int time, Scene scene, float x, float y, string activity) => new(time, Physics.Nearest(scene, new(x, y)), activity, scene);
        Appointment[] plan = npc switch
        {
            0 => [A(360, Scene.SHOP, 352, 192, "Arrumando os produtos"), A(480, Scene.SHOP, 352, 200, "Atendendo no armazém"), A(weekend ? 780 : 1020, Scene.VILLAGE, 400, 448, "Tomando café com Dora"), A(1080, Scene.LIBRARY, 432, 240, "Visitando Otávio"), A(1170, Scene.SHOP, 512, 336, "Conferindo o estoque")],
            1 => [A(360, Scene.WORKSHOP, 352, 288, "Preparando as ferramentas"), A(480, Scene.WORKSHOP, 192, 224, "Trabalhando no barco da Iara"), A(500, Scene.VILLAGE, 848, 336, "Buscando madeira na varanda"), A(560, Scene.FARM, 336, 400, "Visitando o novo fazendeiro"), A(690, Scene.BAKERY, 224, 336, "Buscando o almoço da Dora"), A(800, Scene.WORKSHOP, 496, 224, "Construindo o palco da Ceci"), A(1030, Scene.VILLAGE, 656, 576, "Encontrando Ceci na praça"), A(1180, Scene.WORKSHOP, 352, 320, "Guardando as ferramentas")],
            2 => [A(360, Scene.COTTAGE, 448, 272, "Separando as iscas"), A(450, Scene.VILLAGE, 880, 688, "Procurando peixes na margem sul"), A(530, Scene.VILLAGE, 784, 496, "Pescando no cais"), A(650, Scene.VILLAGE, 1040, 688, "Trocando o ponto de pesca"), A(760, Scene.BAKERY, 432, 320, "Almoçando com Dora"), A(890, Scene.VILLAGE, 1360, 1120, "Explorando o lago do bosque"), A(1080, Scene.VILLAGE, 400, 592, "Contando histórias para Ceci"), A(1200, Scene.COTTAGE, 480, 256, "Limpando a vara de pesca")],
            3 => [A(360, Scene.GREENHOUSE, 352, 256, "Organizando o caderno"), A(480, Scene.GREENHOUSE, 208, 224, "Estudando as sementes"), A(505, Scene.VILLAGE, 688, 272, "Examinando as mudas de Miguel"), A(600, Scene.FARM, 864, 448, "Observando a horta da fazenda"), A(770, Scene.VILLAGE, 464, 592, "Comparando notas no jardim"), A(880, Scene.LIBRARY, 384, 240, "Pesquisando os mapas antigos"), A(1040, Scene.VILLAGE, 1200, 816, "Catalogando flores do bosque"), A(1160, Scene.GREENHOUSE, 400, 256, "Guardando as amostras")],
            4 => [A(360, Scene.BAKERY, 224, 192, "Assando os primeiros pães"), A(480, Scene.BAKERY, 400, 176, "Tirando uma fornada do forno"), A(492, Scene.VILLAGE, 1168, 336, "Abrindo a banca de pães"), A(590, Scene.SHOP, 512, 304, "Entregando broas para Lia"), A(720, Scene.VILLAGE, 416, 448, "Dividindo o almoço na praça"), A(850, Scene.BAKERY, 352, 304, "Preparando massa e biscoitos"), A(1035, Scene.LIBRARY, 480, 240, "Levando chá para Otávio"), A(1150, Scene.BAKERY, 224, 272, "Fechando a cozinha")],
            5 => [A(360, Scene.GREENHOUSE, 512, 240, "Preparando as ervas"), A(465, Scene.VILLAGE, 720, 272, "Regando a horta de ervas"), A(550, Scene.VILLAGE, 432, 688, "Colhendo folhas no jardim"), A(690, Scene.COTTAGE, 352, 272, "Levando chá para Iara"), A(840, Scene.GREENHOUSE, 448, 256, "Separando mudas e etiquetas"), A(950, Scene.VILLAGE, 736, 720, "Cuidando das flores da margem"), A(1090, Scene.VILLAGE, 656, 592, "Descansando no banco da praça"), A(1190, Scene.GREENHOUSE, 352, 320, "Preparando chá para amanhã")],
            6 => [A(360, Scene.COTTAGE, 352, 256, "Escrevendo uma melodia"), A(470, Scene.VILLAGE, 464, 432, "Escutando a vila acordar"), A(515, Scene.VILLAGE, 656, 592, "Ensaiando na praça"), A(650, Scene.WORKSHOP, 352, 288, "Planejando o palco com Bento"), A(780, Scene.VILLAGE, 1120, 336, "Visitando a banca da Dora"), A(920, Scene.VILLAGE, 400, 592, "Compondo à sombra das árvores"), A(1070, Scene.VILLAGE, 672, 576, "Tocando para os vizinhos"), A(1200, Scene.COTTAGE, 480, 272, "Guardando as partituras")],
            _ => [A(360, Scene.LIBRARY, 352, 240, "Separando os livros do dia"), A(475, Scene.VILLAGE, 400, 544, "Lendo no jardim"), A(540, Scene.LIBRARY, 384, 240, "Restaurando um mapa do vale"), A(675, Scene.SHOP, 512, 320, "Conversando sobre as novidades"), A(820, Scene.VILLAGE, 1440, 816, "Caminhando pelo bosque"), A(960, Scene.LIBRARY, 480, 304, "Organizando cartas antigas"), A(1080, Scene.VILLAGE, 608, 608, "Ouvindo as histórias dos vizinhos"), A(1175, Scene.LIBRARY, 352, 240, "Escolhendo a leitura da noite")]
        };
        if (weekend)
        {
            switch (npc)
            {
                case 1: plan[5] = A(800, Scene.COTTAGE, 432, 272, "Visitando Iara para combinar o barco"); break;
                case 2: plan[2] = A(530, Scene.FARM, 816, 496, "Pescando perto da fazenda"); break;
                case 3: plan[2] = A(505, Scene.GREENHOUSE, 400, 240, "Recebendo visitas para trocar sementes"); break;
                case 4: plan[3] = A(590, Scene.VILLAGE, 1136, 336, "Dividindo broas na feira"); break;
                case 5: plan[2] = A(550, Scene.FARM, 864, 448, "Visitando a horta da fazenda"); break;
                case 6: plan[2] = A(515, Scene.BAKERY, 224, 336, "Buscando café antes do ensaio"); plan[4] = A(780, Scene.VILLAGE, 688, 592, "Preparando o encontro da praça"); break;
                case 7: plan[2] = A(540, Scene.VILLAGE, 432, 592, "Lendo um poema para os vizinhos"); plan[4] = A(820, Scene.FARM, 336, 400, "Conferindo o velho caminho do pomar"); break;
            }
        }
        else if (npc > 0)
        {
            // Visits change by weekday, not only by a small offset at one spot.
            int weekday = (s.day - 1) % 7;
            if (npc == 1 && weekday is 1 or 3) plan[3] = A(560, Scene.LIBRARY, 432, 272, "Consertando a cadeira de Otávio");
            if (npc == 3 && weekday == 2) plan[3] = A(600, Scene.VILLAGE, 1200, 816, "Registrando flores antes da feira");
            if (npc == 4 && weekday == 4) plan[3] = A(590, Scene.COTTAGE, 352, 288, "Entregando uma broa para as amigas");
            if (npc == 5 && weekday is 0 or 3) plan[3] = A(690, Scene.LIBRARY, 432, 240, "Levando chá e buscando um livro de ervas");
            if (npc == 6 && weekday == 1) plan[3] = A(650, Scene.LIBRARY, 432, 272, "Escolhendo um poema com Otávio");
            if (npc == 7 && weekday == 4) plan[3] = A(675, Scene.BAKERY, 224, 336, "Anotando uma receita antiga da Dora");
        }
        if (raining(s) && npc != 0)
        {
            for (int i = 1; i < plan.Length - 1; i++)
            {
                if (plan[i].scene is not (Scene.VILLAGE or Scene.FARM)) continue;
                Scene room = npc switch { 1 => Scene.WORKSHOP, 2 or 6 => Scene.COTTAGE, 3 or 5 => Scene.GREENHOUSE, 4 => Scene.BAKERY, _ => Scene.LIBRARY };
                if (i % 2 == 0) room = npc % 2 == 0 ? Scene.BAKERY : Scene.LIBRARY;
                plan[i] = A(plan[i].start, room, 256 + npc % 3 * 96, 288, i % 2 == 0 ? "Visitando um amigo enquanto chove" : "Trabalhando abrigado da chuva");
            }
        }
        if (FestivalDay(s))
        {
            Point[] gathering = [new(432, 448), new(656, 576), new(432, 592), new(688, 448), new(464, 640), new(720, 592), new(672, 608), new(608, 640)];
            Appointment home = plan[^1];
            plan = plan.Where(a => a.start < 1020).Append(A(1020 + npc * 3, Scene.VILLAGE, gathering[npc].x, gathering[npc].y, npc == 6 ? "Compartilhando a canção do jardim" : "Celebrando o jardim com os vizinhos")).Append(home with { start = 1200 + npc * 3 }).ToArray();
        }
        return plan;
    }
    public static int Phase(int npc, State s) { var plan = Schedule(npc, s); int phase = 0; for (int i = 0; i < plan.Length; i++) if (s.minute >= plan[i].start) phase = i; return phase; }
    public static Appointment Now(int npc, State s) => Schedule(npc, s)[Phase(npc, s)];
    public static Point Walkable(Point desired) => Physics.Nearest(Scene.VILLAGE, desired);
    static readonly string[][] Daily = [
        ["Separei sementes novas hoje. Minha avó guardava as melhores em envelopes com desenhos, porque vivia perdendo os óculos.", "Bento consertou a porta do armazém. Ela parou de ranger, mas agora sinto falta do aviso de que alguém chegou!", "Ceci quer tocar na feira. Estou tentando convencer Otávio a ler um poema entre as músicas.", "Dora me ensinou um pão de cenoura. Se a sua horta render, talvez a gente invente uma receita só da vila."],
        ["Estou restaurando um barco para Iara. Ela pediu que flutuasse; eu achei uma exigência bastante razoável.", "Otávio trouxe uma cadeira quebrada. Disse que ela tem quarenta anos de histórias. Vou tentar salvar todas.", "Meu primeiro trabalho foi a placa da vila. A letra torta é charme, segundo Lia. Eu aceito essa versão.", "Ceci pediu um palco. Fiz um desenho pequeno e ela devolveu um desenho enorme. Artistas, viu?"],
        ["De manhã vi uma garça perto do lago. Fiquei tão quieta para não assustá-la que perdi a fisgada.", "Bento está cuidando do meu barco. Enquanto isso, pesco da margem e finjo que gosto mais assim.", "Theo anda medindo a água. Eu perguntei a ele se os peixes estavam crescendo. Ele anotou a pergunta a sério.", "Quando eu era menina, minha mãe reconhecia a chuva pelo cheiro do vento. Estou aprendendo a fazer isso também."],
        ["Encontrei uma flor crescendo entre duas pedras. Anotei: as plantas não pedem permissão para começar de novo.", "Miguel me ajuda a separar plantas medicinais das que só têm cheiro de remédio. A diferença é bem importante.", "Iara sabe onde cada planta cresce na margem. Meu caderno melhorou muito depois que comecei a escutá-la.", "Quero organizar uma troca de sementes. Sua fazenda pode virar parte da história das plantas deste vale."],
        ["A fornada de hoje saiu com a casca perfeita. O segredo foi esperar. O difícil é explicar isso ao meu estômago.", "Lia e eu estamos tentando recuperar a receita de broa da mãe dela. A anotação diz apenas: farinha até dar ponto.", "Ceci ensaiou perto da banca e vendi todos os pães. Vou começar a pagar o cachê dela em biscoitos.", "Otávio conhece uma história para cada receita. O bolo de laranja dele quase vira uma trilogia."],
        ["Reservei um cantinho para hortelã. Theo disse que ela toma conta de tudo. Acho que ele está preocupado comigo.", "Dora trouxe pão para quem passou a manhã na horta. Cuidar de uma vila também é lembrar que as pessoas precisam almoçar.", "Aprendi a fazer chá com minha avó. Hoje cultivo as mesmas ervas para ter um pouco daquela casa por perto.", "Se passar o dia inteiro na lavoura, faça pausas. Sua energia faz parte da colheita também."],
        ["Estou tentando transformar o barulho do regador em ritmo. Até agora minha melhor plateia é um pardal.", "Bento prometeu um palco para a feira. Eu prometi uma canção que ele consiga assobiar enquanto trabalha.", "Iara diz que o rio tem voz. Fiquei ouvindo um tempão e voltei com o começo de uma melodia.", "Quero escrever uma música sobre quem chega ao vale. Ainda não decidi a rima para fazendeiro."],
        ["Encontrei um mapa antigo da vila. Onde fica a praça havia um pomar. Talvez as raízes ainda lembrem.", "Bento recuperou minha cadeira de leitura. Alguns objetos ficam mais bonitos quando alguém cuida deles.", "Ceci me pediu um poema para a feira. Estou escolhendo um curto. Quero que sobre tempo para dançar.", "Anoto pequenas histórias dos moradores. Um dia alguém vai querer saber como era viver aqui, num dia comum como este."]
    ];
    static readonly string[] Rain = ["Gosto do cheiro de chuva entrando pela porta do armazém. Minha avó dizia que era dia de fazer café extra para as visitas.", "Madeira molhada pede paciência. Hoje vou afiar as ferramentas e deixar o telhado fazer a música.", "A chuva embaralha os círculos na água. Fica difícil ver a fisgada, mas os peixes chegam mais perto da margem.", "Repare nas folhas depois da chuva. Elas parecem recém-pintadas. Minha pesquisa pode esperar cinco minutos.", "Hoje o pão demora mais a crescer. Aproveito para fazer biscoitos e escutar a chuva na janela.", "Deixei as ervas recolhidas sob a varanda. Um chá quente combina com esse tempo, não acha?", "Essa chuva está em compasso de três. Se eu conseguir guardar o ritmo, sai uma valsa.", "Dias assim me lembram a biblioteca antiga. Chovia lá dentro também, mas só na seção de geografia."];
    static readonly string[] Close = ["Quando você chegou, fiquei torcendo para a fazenda voltar a ter luz à noite. Agora encontro um amigo quando olho para aquele caminho.", "Não sou bom de conversa, mas já deixei uma caneca sua na oficina. Isso deve contar como uma frase bonita.", "Eu costumava pescar sozinha para pensar. Agora às vezes guardo uma história para te contar antes de voltar.", "Sua horta me ensinou coisas que meu caderno não sabia. Obrigado por levar minhas pequenas descobertas a sério.", "Separei o último pão para você. Pode chamar de privilégio de amigo; eu chamo de não deixar ninguém ir embora com fome.", "É bom ver você cuidando de um lugar e de si mesmo. O vale tem sorte de ter você por perto.", "Terminei o primeiro verso da música da fazenda. Ainda tenho vergonha de tocar, mas você vai ser o primeiro a ouvir.", "Já escrevi seu nome no caderno das histórias do vale. Não se preocupe: deixei muitas páginas em branco depois dele."];
    static readonly string[][] Work = [
        ["Eu separo as sementes pela época e confiro os preços antes de abrir. Parece só uma loja, mas cada caixinha veio de algum vizinho.", "Compro sua colheita e os peixes. O que você vende aqui acaba na mesa de alguém da vila; gosto de saber de onde vem cada coisa.", "Nos dias úteis, atendo até as cinco. No fim de semana, fecho à uma para visitar a feira. Também preciso lembrar como é o lado de fora do balcão."],
        ["A madeira avisa quando está pronta: muda o cheiro, o peso, o som. Meu trabalho é prestar atenção antes de usar a serra.", "O barco da Iara e o palco da Ceci disputam espaço na oficina. Um precisa enfrentar a água; o outro, a ansiedade da artista.", "Se quiser o regador de cobre, preciso de oito madeiras e sessenta moedas. Prefiro lhe entregar uma ferramenta boa a prometer uma ferramenta rápida."],
        ["Não pesco sempre no mesmo ponto. Procuro sombra, correnteza e pequenos círculos na superfície. Às vezes volto só com uma boa observação.", "Na fisgada, espere aparecer AGORA e puxe com Espaço. A parte mais difícil da pesca é aprender que esperar também é fazer alguma coisa.", "Separei uma vara para você porque ninguém deveria começar sem companhia. A minha mãe fez o mesmo quando me trouxe ao lago."],
        ["O meu caderno tem números, desenhos e perguntas sem resposta. Uma horta de verdade ensina coisas que uma planta num vaso não mostra.", "Três cenouras da sua horta ajudam na pesquisa. Quando eu tiver as medidas, quero explicar os resultados de um jeito que seja útil para você.", "Miguel cuida das ervas e eu registro as sementes. Às vezes discordamos de um nome e descobrimos que estávamos falando da mesma planta."],
        ["A massa muda com o tempo. Hoje ela pede uma pausa mais longa. Eu podia olhar só o relógio, mas prefiro olhar o pão.", "Levo broas para Lia, pão para a praça e alguma coisa para quem esqueceu de almoçar. As entregas acabam virando a minha volta pela vila.", "Ainda faço a primeira fornada com a receita do meu pai. Ajustei o forno, a farinha e as medidas; a mania de provar a massa ficou igual."],
        ["Identifico as ervas antes de cortar. Algumas plantas parecem iguais de longe, por isso nunca peço a alguém que prove uma folha desconhecida.", "Gosto de deixar as ferramentas limpas e as mudas identificadas. Theo ri das minhas etiquetas, até precisar encontrar uma planta no escuro.", "Passo pela margem e pelo jardim para observar a umidade. Cada cantinho do vale segura a água de um jeito diferente."],
        ["Eu anoto ritmos que encontro andando: os passos na ponte, as ferramentas do Bento, a chuva. Depois tento descobrir o que combina com o quê.", "Pedir um palco ao Bento foi fácil. Difícil foi admitir que o espaço que eu queria era maior que a minha coragem.", "Quando pareço estar passeando sem rumo, quase sempre estou procurando o fim de um verso. Quase sempre."],
        ["Uma biblioteca também precisa de alguém que escute. Muita história chega primeiro como conversa e só depois vira uma página.", "Catalogo cartas, mapas e receitas. Um endereço pode mudar de nome, mas a receita continua levando a gente até uma família.", "Não conserto os rasgos para esconder que existiram. Uso papel e paciência para que o mapa consiga contar a história por mais tempo."]
    ];
    static readonly string[][] Personal = [
        ["Eu queria viajar quando era pequena. Depois percebi que podia conhecer gente diferente sem sair daqui: bastava manter a porta aberta.", "Minha avó me ensinou a guardar troco e a oferecer café. Só descobri adulta que a segunda lição era a mais importante.", "Fico nervosa quando a loja está silenciosa demais. É estranho sentir falta até das reclamações sobre o preço do açúcar."],
        ["Eu era aprendiz quando fiz a primeira placa da vila. A letra saiu torta. Deixaram a placa lá por anos e eu passei a gostar dela.", "Não sei falar bonito na hora certa. Às vezes percebo o que devia ter dito quando já estou desmontando uma cadeira.", "Meu pai dizia que ninguém aprende carpintaria sem errar medidas. Eu achei que fosse uma desculpa dele, até começar a trabalhar."],
        ["Aprendi a nadar antes de aprender o caminho da escola. Minha mãe dizia que eu conversava com o rio; eu achava que todo mundo fazia isso.", "Guardo uma pedra lisa de cada lugar em que pesquei. Não são troféus. Só me ajudam a lembrar de quem estava comigo.", "Ceci voltou para casa com uma ideia de música sobre o lago. Eu pedi que o peixe que escapei não aparecesse na letra."],
        ["Eu ficava sem jeito de perguntar o nome das plantas. Anotava a cor e o lugar, depois passava horas tentando adivinhar. Hoje pergunto mais.", "Vim procurar respostas e acabei aprendendo a gostar das perguntas. O vale é bom nisso: sempre aparece uma folha que eu nunca vi direito.", "Tenho um desenho antigo da primeira flor que cultivei. Ela ficou torta no papel e na terra. Guardei os dois resultados com carinho."],
        ["Meu pai nunca escreveu as receitas completas. Onde faltava uma medida, quase sempre havia o nome de alguém que sabia explicar.", "Quando a padaria fica cheia, eu reclamo que não tenho tempo de sentar. Quando fica vazia, arrumo as cadeiras para parecer que alguém vem.", "Quero deixar uma receita minha no caderno da família. Ainda estou decidindo se começo pela massa ou pela história que vai ao lado."],
        ["A minha avó tinha um jardim pequeno. Eu achava enorme porque cada vaso tinha uma história e quase todos tinham o nome de um amigo.", "Demorei a aprender a descansar sem achar que estava abandonando alguma coisa. As plantas sobreviveram às minhas primeiras tardes de folga.", "Gosto de gente que faz perguntas simples. Geralmente são essas perguntas que mostram o que eu vinha complicando sem perceber."],
        ["Antes de vir para cá, eu tinha parado de escrever. Foi uma semana ouvindo o vento na janela e as palavras começaram a aparecer de novo.", "Eu ensaio as apresentações enquanto caminho. Quando você me vê falando sozinha, pode fingir que não percebeu a parte em que erro meu nome.", "Iara deixa um cantinho da mesa livre para minhas partituras. Às vezes uma música começa com esse tipo de gentileza."],
        ["Eu desenhava mapas quando era menino. Acrescentava lugares que não existiam, só para poder imaginar uma viagem maior que a estrada da vila.", "Já pensei que guardar uma lembrança era proteger uma coisa do tempo. Hoje acho que é dar a ela mais alguém com quem seguir.", "Deixo páginas em branco entre as histórias antigas. A vila não acabou quando eu envelheci, e o caderno precisa saber disso."]
    ];
    static readonly string[] NewRain = [
        "Deixei uma toalha perto da entrada. Pode entrar sem cerimônia; o chão seca depois, mas a conversa não precisa ficar esperando.",
        "Hoje eu trabalho mais perto da janela. O som da chuva ajuda a perceber quando uma ferramenta começa a vibrar do jeito errado.",
        "Se eu voltar cedo do lago, vou fazer chá com Ceci. Alguns dias de pesca terminam melhor na cozinha.",
        "Vou deixar a caminhada pelo bosque para amanhã. Por hoje, o livro do Otávio tem folhas suficientes para eu estudar.",
        "Está um bom dia para biscoitos. Eles ficam prontos antes que alguém resolva sair correndo para escapar da chuva.",
        "Recolhi as etiquetas de papel antes que molhassem. As plantas conhecem seus nomes, mas eu ainda preciso de uma ajudinha.",
        "Anotei o ritmo das gotas no telhado. Se amanhã eu não entender nada do rabisco, vou dizer que era uma composição experimental.",
        "Separei uma mesa longe das goteiras. Foi o primeiro reparo que pedi ao Bento quando resolvi cuidar desta casa."
    ];
    static readonly string[] NewClose = [
        "Já sei reconhecer seus passos perto da porta. Se eu começar a passar o café antes de você pedir, não é adivinhação. É costume bom.",
        "Guardei uma sobra de madeira boa para quando você precisar. Não é favor com cobrança; só gosto de saber que posso ajudar um amigo.",
        "Quando vejo uma coisa bonita no lago, penso em como vou lhe contar depois. Isso mudou bastante os meus dias.",
        "Tenho menos vergonha de mostrar os desenhos incompletos do caderno agora. Você sempre encontra alguma coisa para observar junto comigo.",
        "O pão que separei para você ficou com a ponta mais dourada. É a minha parte favorita. Pode considerar uma declaração de amizade.",
        "Se um dia não quiser conversar, pode sentar aqui mesmo assim. Às vezes fazer companhia já é uma conversa inteira.",
        "A canção que comecei quando você chegou está quase pronta. Agora ela fala mais sobre ficar do que sobre chegar.",
        "No começo eu anotava o que você fazia pela fazenda. Agora descubro histórias suas nas conversas dos outros moradores. Você criou raízes."
    ];
    static readonly string[][] Seasons = [
        ["As sementes de primavera chegam em envelopes leves. Engraçado como um começo inteiro cabe numa caixinha.", "O armazém esquenta cedo no verão. Deixo a água perto da porta para quem vem da lavoura.", "No outono começo a conferir as reservas. Minha avó dizia que planejar também era uma forma de cuidar.", "No inverno, café e conversa demoram um pouco mais. Não tenho pressa de fechar a porta para as visitas."],
        ["A primavera enche a oficina de pedidos. Todo mundo encontra uma cerca que precisa de cuidado.", "No calor, trabalho nas peças menores antes do sol subir. A madeira fica mais agradável na sombra.", "Gosto da cor das tábuas na luz do outono. Às vezes paro só para olhar e finjo que estou medindo.", "No inverno separo as ferramentas e recupero peças antigas. Consertar também é preparar o próximo começo."],
        ["Repare na margem quando as primeiras folhas aparecem. O lago muda antes que a gente perceba.", "No verão procuro a sombra das árvores. Os peixes costumam ter a mesma ideia.", "As folhas de outono confundem um pouco a superfície. Tenho que observar com mais calma.", "Quando o frio aperta, reduzo a pescaria. A volta para casa fica mais interessante que a espera na margem."],
        ["A primavera parece uma biblioteca que abriu todos os livros ao mesmo tempo. Tento registrar uma página por dia.", "No verão observo as plantas cedo. Mais tarde, elas e eu preferimos uma sombra.", "No outono guardo sementes e reviso os nomes. Meu caderno sempre ganha alguma correção.", "O inverno é bom para comparar os registros. Descubro perguntas que nem percebi que estava fazendo."],
        ["Começo a primavera procurando uma receita para cada colheita que imagino chegando.", "No verão deixo a massa descansar longe da janela. O sol já quer assar o pão antes de mim.", "O outono pede broa e café comprido. É a estação em que mais gente esquece a pressa na minha mesa.", "No inverno o forno vira ponto de encontro. Não reclamo: gosto de casa com vozes."],
        ["A primavera me faz rever cada etiqueta. Sempre aparece uma muda que eu não esperava.", "No verão, rego as ervas cedo e confiro a terra com a mão antes de pegar o regador.", "No outono recolho folhas secas e guardo as sementes com cuidado. Nada precisa ser apressado.", "No inverno deixo as mudas mais protegidas. Também aproveito para ouvir as histórias do Otávio."],
        ["Estou tentando escrever uma melodia de primavera sem usar a palavra flor. Por enquanto, perdi para as flores.", "O verão tem barulho até no silêncio. Minha janela fica aberta e o caderno acaba cheio.", "No outono gosto de caminhar antes de terminar os versos. O som das folhas ajuda a encontrar o ritmo.", "No inverno ensaio mais dentro de casa. Iara já sabe em qual compasso vou errar."],
        ["Na primavera, comparo o jardim com os mapas antigos. Algumas flores parecem insistir nos mesmos lugares.", "O verão me lembra as tardes em que eu lia escondido do trabalho. Hoje leio à vista de todo mundo.", "No outono penso mais nas cartas antigas. Talvez seja o jeito que a luz cai sobre o papel.", "O inverno convida a uma leitura demorada. Sempre deixo outra cadeira perto da mesa."]
    ];
    static readonly string[] Evening = [
        "Já estou pensando no caminho de volta. Antes de fechar o dia, gosto de saber se todo mundo conseguiu o que precisava.", "A oficina está quase quieta. Vou guardar as ferramentas antes que a vontade de fazer só mais um ajuste me faça perder a hora.", "Daqui a pouco volto para casa. A melhor história de pesca de hoje ainda é a que o peixe me deixou inventar.", "Vou passar as notas a limpo antes de esquecer os detalhes. Depois tento lembrar que também preciso jantar.", "Sobrou um cheirinho de pão na roupa. É assim que percebo que o trabalho veio comigo para casa.", "Já conferi as mudas. Agora vou deixar que a noite cuide de ser noite e procurar um pouco de descanso.", "Esse é meu horário favorito para ouvir a praça. Cada pessoa vai levando o dia para uma direção diferente.", "Vou escolher a última leitura do dia. Se o livro for bom demais, amanhã a culpa das minhas olheiras será dele."
    ];
    readonly record struct Line(int id, string text);
    static Line Pick(State s, int npc, IEnumerable<Line> source, bool topic, int visit)
    {
        Line[] lines = source.ToArray(); int previous = topic ? s.npc_last_topic[npc] : s.npc_last_line[npc];
        int index = (int)(((long)visit + s.day * 3L + npc * 5L + s.minute / 120) % lines.Length);
        if (lines[index].id == previous) index = (index + 1) % lines.Length;
        Line line = lines[index];
        if (line.id == previous) line = new(100 + (visit + npc + 1) % Daily[npc].Length, Daily[npc][(visit + npc + 1) % Daily[npc].Length]);
        if (topic) s.npc_last_topic[npc] = line.id; else s.npc_last_line[npc] = line.id;
        return line;
    }
    static IEnumerable<Line> Bank(string[] lines, int firstId) => lines.Select((text, index) => new Line(firstId + index, text));
    public static string Greeting(State s, int npc, bool first)
    {
        if (npc < 0 || npc >= Count) return "";
        if (first) return NPCS[npc].first;
        int visit = Visit(s, npc), season = (s.day - 1) / 28 % 4;
        IEnumerable<Line> lines = Bank(Daily[npc], 100);
        if (raining(s) && visit % 3 == 0) lines = [new(500, Rain[npc]), new(501, NewRain[npc])];
        else if (s.friendship[npc] >= 4 && visit % 3 == 1) lines = [new(600, Close[npc]), new(601, NewClose[npc])];
        else if (s.minute >= 1080 && visit % 3 == 2) lines = [new(800, Evening[npc]), new(700 + season, Seasons[npc][season])];
        else if (visit % 3 == 2) lines = Bank(Personal[npc], 400).Append(new(700 + season, Seasons[npc][season]));
        return Pick(s, npc, lines, false, visit).text;
    }
    public static string Topic(State s, int npc, int topic)
    {
        if (npc < 0 || npc >= Count) return "";
        int visit = Visit(s, npc), season = (s.day - 1) / 28 % 4;
        IEnumerable<Line> lines;
        if (topic == 0) lines = Bank(Work[npc], 300);
        else if (topic == 1) { lines = Bank(Personal[npc], 400); if (s.friendship[npc] >= 3) lines = lines.Append(new(600, Close[npc])).Append(new(601, NewClose[npc])); }
        else
        {
            Appointment activity = Now(npc, s); string context = "Meu plano agora é: " + activity.activity.ToLowerInvariant() + ".";
            lines = [new(1100 + Phase(npc, s), context), new(700 + season, Seasons[npc][season]), new(800, s.minute >= 1080 ? Evening[npc] : Daily[npc][(s.day + npc) % Daily[npc].Length])];
            if (raining(s)) lines = lines.Append(new(500, Rain[npc])).Append(new(501, NewRain[npc]));
        }
        return Pick(s, npc, lines, true, visit).text;
    }
    public static int FavoriteCrop(int npc) => new[] { 1, 0, 1, 0, 2, 1, 2, 0 }[Math.Clamp(npc, 0, Count - 1)];
    public static string GiftReply(int npc) => new[] { "Da sua própria horta? Vou separar para o jantar com minha avó. Obrigada por lembrar da gente!", "Puxa... você plantou isso? Vou cozinhar hoje. Já estava cansado de jantar só pão. Obrigado, amigo.", "Vou preparar para comer depois da pesca. Você sabe cuidar das pessoas tão bem quanto cuida da horta.", "Olha a cor dessas folhas! Prometo guardar uma parte para a pesquisa e comer o resto. Obrigado!", "Isso merece virar receita! Quando sentir um cheirinho bom vindo da banca, pode aparecer.", "Fresquinho assim faz bem até para o humor. Muito obrigado por compartilhar sua colheita.", "Vou inventar uma canção de agradecimento. Uma bem curtinha, para o presente não estragar enquanto eu canto!", "Um presente que cresceu com cuidado tem uma história bonita. Vou lembrar de quem o trouxe." }[Math.Clamp(npc, 0, Count - 1)];
    public static string GiftReply(State s, int npc, int crop)
    {
        if (npc < 0 || npc >= Count) return "";
        if (crop != FavoriteCrop(npc)) return GiftReply(npc);
        string[] favorites = ["Nabo! Minha avó faz uma sopa que me lembra a infância. Vou levar para ela e contar de quem veio.", "Cenoura! Vou tentar a receita da Dora hoje. Se o resultado parecer uma peça de madeira, volto para pedir ajuda.", "Nabo é o meu favorito depois de uma manhã no lago. Você prestou atenção em mim. Obrigada.", "Cenoura da sua horta! É a melhor mistura de pesquisa e almoço que eu poderia receber.", "Morangos! Já estou pensando numa torta. Se eu conseguir deixar uma fatia de lado, ela vai ter seu nome.", "Nabo fresquinho. Gosto de cozinhar devagar no fim do dia; você acabou de melhorar meu jantar.", "Morangos! Vou dividir com Iara. Ela diz que fruta boa faz o ensaio parecer mais curto.", "Cenoura! Minha mãe fazia um ensopado com ela. Engraçado um presente conseguir trazer uma casa inteira de volta."];
        return favorites[npc];
    }
}
