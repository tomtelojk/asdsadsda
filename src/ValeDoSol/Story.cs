using System.Numerics;
using static ValeDoSol.Logic;
namespace ValeDoSol;

public readonly record struct StoryProgress(int Stage, string Title, string Summary, string Objective, string Progress, bool Ready, string Reward, int ContactNpc, string Contact);

/// <summary>A small, complete campaign built from the farming and friendship rules.
/// It does not award progress for opening a dialogue, spend items before checking
/// every requirement, or discard rewards when a saved inventory is at its limit.</summary>
public static class Story
{
    public const string Title = "O Caderno das Estações";
    public const int ChapterCount = 6;
    public const uint LetterRead = 1u << 6;
    public const uint GreenhouseRepaired = 1u << 2;
    public const uint CommunityGarden = 1u << 3;
    public const uint FestivalPrepared = 1u << 4;
    public const uint FestivalFinished = 1u << 5;
    const int CoinLimit = 1_000_000_000, ItemLimit = 10_000_000;
    static uint CastMask => (1u << NPCS.Length) - 1;
    static readonly int[] Contacts = [7, 3, 1, 5, 4, 6];
    static readonly string[] Titles = ["Os nomes do vale", "A primeira página", "Madeira que volta a cantar", "Um canteiro de muitas mãos", "Uma mesa para o vale", "A página em branco"];
    static readonly string[] Summaries =
    [
        "Você herdou a Fazenda Girassol e um caderno incompleto de sua avó Aurora. Otávio reconhece o desenho da capa: a antiga horta comunitária nasceu ali.",
        "Uma anotação de Aurora diz: 'Antes de prometer uma colheita ao vale, aprenda a cuidar de uma pequena parte dele.' Theo propõe começar com cenouras.",
        "A ala antiga da casa das flores guarda os canteiros do projeto de Aurora. Bento consegue recuperar as bancadas e preparar o espaço, se todos ajudarem com os materiais.",
        "Miguel encontra páginas com as letras de vários moradores. A horta não pertencia a uma pessoa: cada vizinho trazia uma muda e ensinava algo que sabia.",
        "Dora lembra da última mesa compartilhada da vila. Iara oferece peixe, Lia separa os pratos e Ceci quer reunir os moradores de novo. Sua horta pode completar a mesa.",
        "A comida está separada. Falta convidar quem ajudou a escrever esse recomeço. Encontre os moradores e volte à praça, na tarde de um dia seguinte."
    ];
    static readonly string[] Milestones =
    [
        "Otávio reconhece a letra de Aurora e devolve uma página que guardou durante anos. Há nomes nas margens, manchas de terra e espaço para as suas primeiras anotações.",
        "Theo organiza as cenouras sobre a mesa. Entre duas páginas do caderno, vocês encontram a planta da velha horta: um lugar para cultivar e dividir o que se aprende.",
        "Bento ajusta a última bancada. A ala antiga da casa das flores pode receber mudas outra vez. Na madeira, ele encontra o nome de Aurora e deixa um pequeno espaço ao lado.",
        "Miguel leva as primeiras mudas para o canteiro compartilhado. Cada espécie ganha uma etiqueta com o nome de quem a trouxe. A etiqueta da Fazenda Girassol é sua.",
        "Dora guarda os vegetais e Iara separa os peixes. Os convites estão prontos. Amanhã, depois das 17h, a praça pode receber a mesa que o vale esperou por tanto tempo.",
        "Na praça, o caderno passa de mão em mão. Otávio lê os primeiros nomes; Ceci recita um verso e Bento acrescenta o seu. Aurora deixou uma página em branco para quem viesse depois. Agora ela tem uma história."
    ];
    public const string IntroLetter = "Meu querido neto,\n\nSe você abriu esta carta, a chave da Fazenda Girassol já encontrou suas mãos. Não deixei uma fazenda pronta. Deixei terra para experimentar, uma casa para voltar e pessoas que ainda sabem meu nome.\n\nNo baú há um caderno. As páginas não contam só o que plantei: contam quem me ajudou. Eu queria ver a velha horta da vila cheia outra vez. Faltaram dias para terminar, mas talvez não falte gente.\n\nConverse com Otávio. Ele guardou uma parte dessa história. E, quando a terra não responder como você esperava, descanse. Amanhã também é tempo de começar.\n\nCom amor,\nVó Aurora";
    public const string Ending = "O Caderno das Estações\n\nA mesa na praça não resolve todos os dias difíceis. Ela lembra por que vale a pena atravessá-los em companhia. As páginas de Aurora agora estão entre as suas: uma cenoura que cresceu torta, uma cadeira recuperada, um vizinho que virou amigo.\n\nNo fim da tarde, Otávio devolve o caderno. A próxima página continua em branco. Desta vez, você sabe o que escrever nela.\n\nA história principal foi concluída. Sua vida no vale continua: plante, pesque, converse e cuide do que vocês construíram juntos.";

    public static bool IsValid(State s)
    {
        if (s.story_stage < 0 || s.story_stage > ChapterCount || (s.story_flags & ~127u) != 0) return false;
        uint completed = (1u << s.story_stage) - 1;
        if ((s.story_flags & 63u) != completed) return false;
        return s.story_stage == 0 ? s.story_day == 0 : (s.story_flags & LetterRead) != 0 && s.story_day >= 1 && s.story_day <= s.day;
    }
    public static void ReadLetter(State s) { if (IsValid(s)) s.story_flags |= LetterRead; }
    public static bool Complete(State s) => IsValid(s) && s.story_stage == ChapterCount;
    static int Met(State s) => BitOperations.PopCount(s.met & CastMask);
    static int Friends(State s, int level) => s.friendship.Count(n => n >= level);
    static int GrowingKinds(State s) => s.plots.Where(p => p.crop >= 0).Select(p => p.crop).Distinct().Count();
    static bool Requirements(State s) => s.story_stage switch
    {
        0 => (s.story_flags & LetterRead) != 0 && (s.met & CastMask) == CastMask,
        1 => s.harvested >= 5 && s.inventory[0] >= 3,
        2 => s.wood >= 12 && s.stone >= 6 && s.coins >= 80,
        3 => GrowingKinds(s) >= 3 && Friends(s, 2) >= 3,
        4 => s.inventory.All(n => n >= 2) && s.fish >= 2 && Friends(s, 3) >= 4,
        5 => s.day > s.story_day && Friends(s, 2) >= NPCS.Length && s.scene == VILLAGE && s.minute is >= 1020 and < 1260,
        _ => false
    };
    static (int coins, int[] seeds, int energy) Reward(int stage) => stage switch
    {
        0 => (40, [5, 0, 0], 0),
        1 => (50, [0, 5, 0], 0),
        2 => (0, [0, 0, 5], 10),
        3 => (80, [5, 5, 5], 0),
        4 => (150, [0, 0, 0], 0),
        5 => (200, [0, 0, 5], 10),
        _ => (0, [0, 0, 0], 0)
    };
    static bool RewardFits(State s)
    {
        var reward = Reward(s.story_stage); int spentCoins = s.story_stage == 2 ? 80 : 0;
        if ((long)s.coins - spentCoins + reward.coins > CoinLimit) return false;
        for (int i = 0; i < 3; i++) if ((long)s.seeds[i] + reward.seeds[i] > ItemLimit) return false;
        return true;
    }
    public static StoryProgress Current(State s)
    {
        if (!IsValid(s)) return new(s.story_stage, "Caderno indisponível", "Os dados desta história precisam ser recuperados de um backup válido.", "Restaure um backup da partida.", "Progresso inválido", false, "", 7, NPCS[7].name);
        int stage = s.story_stage;
        if (stage == ChapterCount) return new(stage, "O vale continua", "O caderno de Aurora agora guarda a história que vocês escreveram juntos.", "Continue cultivando, explorando e encontrando os moradores.", "História principal concluída", false, "Releia o desfecho quando quiser.", 7, NPCS[7].name);
        string objective = stage switch
        {
            0 => $"Leia a carta de Aurora e conheça os {NPCS.Length} moradores.",
            1 => "Colha 5 vegetais e entregue 3 cenouras a Theo.",
            2 => "Entregue 12 madeiras, 6 pedras e 80 moedas para recuperar as bancadas.",
            3 => "Mantenha cenoura, nabo e morango plantados e alcance 2 corações com 3 moradores.",
            4 => "Entregue 2 cenouras, 2 nabos, 2 morangos e 2 peixes. Alcance 3 corações com 4 moradores.",
            _ => $"Alcance 2 corações com os {NPCS.Length} moradores. Em um dia seguinte, vá à vila entre 17h e 21h."
        };
        string progress = stage switch
        {
            0 => $"Carta: {((s.story_flags & LetterRead) != 0 ? "lida" : "pendente")} | Moradores: {Met(s)}/{NPCS.Length}",
            1 => $"Colheitas: {Math.Min(5, s.harvested)}/5 | Cenouras na mochila: {Math.Min(3, s.inventory[0])}/3",
            2 => $"Madeira: {Math.Min(12, s.wood)}/12 | Pedra: {Math.Min(6, s.stone)}/6 | Moedas: {Math.Min(80, s.coins)}/80",
            3 => $"Culturas plantadas: {GrowingKinds(s)}/3 | Amigos com 2 corações: {Math.Min(3, Friends(s, 2))}/3",
            4 => $"Cenoura {Math.Min(2, s.inventory[0])}/2 · Nabo {Math.Min(2, s.inventory[1])}/2 · Morango {Math.Min(2, s.inventory[2])}/2 · Peixe {Math.Min(2, s.fish)}/2 | Amigos: {Math.Min(4, Friends(s, 3))}/4",
            _ => $"Amigos: {Friends(s, 2)}/{NPCS.Length} | A partir do dia {s.story_day + 1}, na vila, das 17h às 21h"
        };
        string reward = stage switch
        {
            0 => "40 G e 5 sementes de cenoura",
            1 => "50 G e 5 sementes de nabo",
            2 => "5 sementes de morango e +10 de energia máxima",
            3 => "80 G e 5 sementes de cada cultura",
            4 => "150 G e os preparativos do encontro",
            _ => "200 G, 5 sementes de morango e +10 de energia máxima"
        };
        bool requirements = Requirements(s), fits = RewardFits(s);
        if (requirements && !fits) progress = "Objetivo concluído. Use sementes ou gaste moedas para receber toda a recompensa.";
        int contact = Contacts[stage];
        return new(stage, Titles[stage], Summaries[stage], objective, progress, requirements && fits, reward, contact, NPCS[contact].name);
    }
    public static bool TryAdvance(State s, out string message)
    {
        StoryProgress progress = Current(s);
        if (!progress.Ready) { message = Complete(s) ? "Essa história já foi concluída. O vale continua esperando por você." : progress.Progress; return false; }
        int stage = s.story_stage; var reward = Reward(stage);
        // All requirements and reward capacity were checked before any mutation.
        if (stage == 1) s.inventory[0] -= 3;
        if (stage == 2) { s.wood -= 12; s.stone -= 6; s.coins -= 80; }
        if (stage == 4) { for (int i = 0; i < 3; i++) s.inventory[i] -= 2; s.fish -= 2; }
        s.coins += reward.coins;
        for (int i = 0; i < 3; i++) s.seeds[i] += reward.seeds[i];
        s.max_energy = Math.Min(200, s.max_energy + reward.energy);
        s.energy = Math.Min(s.max_energy, s.energy + reward.energy);
        s.story_flags |= 1u << stage; s.story_stage++; s.story_day = s.day;
        message = Milestones[stage]; return true;
    }
    public static string Milestone(int completedStage) => completedStage is >= 0 and < ChapterCount ? Milestones[completedStage] : "";
    public static string NpcLine(State s, int npc)
    {
        if (npc < 0 || npc >= 8 || !IsValid(s)) return "";
        int chapter = Math.Min(s.story_stage, ChapterCount);
        return Lines[chapter][npc];
    }
    static readonly string[][] Lines =
    [
        ["Aurora comprava sementes soltas e saía com três visitas combinadas. Quando terminar de conhecer o pessoal, leve o caderno ao Otávio.", "Sua avó me ensinou a consertar bancos sem apagar as marcas de quem sentou neles. A fazenda ainda tem bastante história debaixo da tinta.", "A gente levava peixe para a horta e voltava com legumes. Ninguém anotava o que devia. Aurora só perguntava se tinha dado para todo mundo.", "Ouvi falar de um caderno que mistura receitas, sementes e nomes. Se encontrou esse caderno na fazenda, você trouxe uma parte importante do vale de volta.", "Aurora assinava as receitas com uma mancha de farinha. Se encontrar uma página assim, eu provavelmente sei de que bolo ela estava falando.", "A velha horta ainda cabe na casa das flores. Antes de pensar em reformar, conheça quem guardou uma lembrança dela.", "Eu não conheci Aurora, mas conheço as histórias que deixam os olhos da vila diferentes quando alguém conta. Quero ouvir a sua também.", "Reconheço essa capa. Guardei uma página para sua avó e nunca consegui devolver. Conheça os vizinhos; depois sentamos para completar os nomes."],
        ["Theo está animado com sua primeira colheita. Guarde três cenouras para ele; as outras podem ajudar com as despesas da fazenda.", "Antes das bancadas, vem a terra. Sua avó insistiria para você provar que o canteiro recebe cuidado todos os dias.", "Minha vara está com você. Não precisa aprender tudo de uma vez: hoje a horta, amanhã o lago. O vale não vai sair do lugar.", "Colha cinco vegetais e traga três cenouras. Não estou procurando perfeição: quero ver o solo da Fazenda Girassol respondendo às suas mãos.", "Cenoura torta também faz bolo bom. Se Theo começar a medir cada uma, lembre a ele que algumas precisam virar almoço.", "Observe as folhas antes de pegar o regador. Cuidar bem de uma planta também é aprender quando ela já recebeu o que precisava.", "Anote o dia da sua primeira colheita. Parece uma coisa pequena agora; depois vira aquele começo que dá vontade de contar.", "Na página que guardei, Aurora escreveu: 'Primeiro, uma coisa viva sob os nossos cuidados.' Acho que sua horta é uma boa resposta."],
        ["Posso separar os pregos e ajudar com o material da reforma. Bento precisa de doze madeiras, seis pedras e oitenta moedas para preparar as bancadas.", "As bancadas ainda têm conserto. Traga doze madeiras, seis pedras e oitenta moedas. Vamos recuperar o espaço sem jogar fora a história dele.", "Quando a horta voltar, posso mostrar ao pessoal as plantas da margem. Minha contribuição começa antes de qualquer peixe chegar à mesa.", "O desenho das bancadas está no caderno. A ala antiga da casa das flores pode abrigar as primeiras mudas quando Bento terminar o trabalho.", "Vou levar pão para o pessoal da reforma. Ainda não aprendi a segurar uma tábua e um martelo ao mesmo tempo, mas disso eu entendo.", "Enquanto vocês cuidam da madeira, vou limpar os vasos e separar as etiquetas antigas. Algumas ainda têm a letra de Aurora.", "Bento diz que madeira canta quando recebe uma boa ferramenta. Estou tentando descobrir se ele falou de propósito ou só gosta de assobiar.", "Repare no que ficou e no que precisou ser trocado. Restaurar não é prender o vale no passado; é dar a ele condições de seguir."],
        ["Separei envelopes para a troca de sementes. Plante as três culturas e encontre tempo para visitar os amigos: a horta nasceu das duas coisas.", "Prontinho. Deixei um lugar para cada muda. Madeira boa segura peso; amizade boa faz alguém aparecer quando falta uma mão.", "Aurora sempre plantava alguma coisa que outra pessoa gostava. Talvez a próxima visita à sua horta comece com essa pergunta.", "Cenoura, nabo e morango crescem em ritmos diferentes. Ter os três no canteiro é um jeito de aprender a prestar atenção em cada um.", "Miguel contou que você está organizando as mudas. Guarde um pedacinho de tempo para conversar também. Planta não é a única coisa que precisa de cuidado.", "Mantenha cenoura, nabo e morango plantados e crie laços com três vizinhos. Quero que cada canteiro tenha uma pessoa que volte para vê-lo crescer.", "Meu vaso quase virou um cemitério de sementes. Miguel veio me ensinar e ficou para conversar. Talvez fosse esse o plano da horta desde o começo.", "O caderno tem várias letras na mesma página. As respostas de Aurora ficavam espremidas nas margens, onde ainda cabia uma pergunta de alguém."],
        ["Os pratos e as toalhas estão separados. Se Dora receber sua colheita, podemos preparar a mesa sem deixar ninguém carregando tudo sozinho.", "A mesa da praça não precisa ser nova. Apertei os parafusos e tirei as farpas. Agora falta o que vai por cima dela e quem vai sentar.", "Separe dois peixes da pesca. Com dois de cada vegetal e a ajuda da Dora, teremos bastante assunto para uma mesa inteira.", "Minha pesquisa pode esperar até amanhã. Hoje quero ver as plantas virando comida e os números do caderno virando gente reunida.", "Preciso de dois de cada vegetal e dois peixes. Cultive também suas amizades: uma mesa só vira encontro quando há pessoas querendo voltar a ela.", "Deixei as mudas protegidas. Vou levar ervas para o chá. Você e Dora não precisam cuidar do encontro inteiro; cada um traz uma parte.", "Estou escolhendo um verso curto para abrir o encontro. Pode deixar: ninguém vai precisar esperar a comida esfriar para eu terminar.", "Achei o convite da última mesa compartilhada. Não tem data para repetir. Só diz: 'Quando houver vontade, arrume uma cadeira a mais.'"],
        ["Os convites estão no balcão. Visite quem ainda falta e volte à vila amanhã ou depois, entre cinco e nove da noite. Vai ter lugar para você.", "Nunca achei que faria outra mesa dessas. Chegue à praça depois das cinco, num dia seguinte. Quero te mostrar o espaço que deixei para o seu nome.", "Os peixes estão separados com a Dora. Vá conversar com o pessoal. Às vezes o convite precisa chegar pela voz de um amigo.", "A horta trouxe as plantas. As visitas trouxeram as pessoas. Agora falta você voltar à praça depois das cinco, em um dia seguinte.", "Está tudo guardado. Passe um dia cuidando da fazenda e visitando os amigos. Num dia seguinte, depois das cinco, a mesa pode ir para a praça.", "Quero que todos se sintam convidados. Crie pelo menos dois corações com cada morador e volte à praça no fim da tarde de um próximo dia.", "Tenho o verso, Otávio tem o caderno e Dora tem a comida. Faça os últimos convites e venha à praça em um dia seguinte, entre dezessete e vinte e uma horas.", "A página em branco não era um erro. Aurora sabia que algumas histórias só ficam completas quando outra pessoa chega para continuá-las."],
        ["Hoje alguém entrou no armazém e perguntou pela horta como quem pergunta por um vizinho. Acho que é assim que um lugar volta a fazer parte da vila.", "O seu nome ficou um pouquinho torto na madeira. Eu disse que era charme. Lia disse que eu finalmente aprendi a aceitar essa desculpa.", "Gostei de ver a mesa cheia. Agora podemos voltar ao lago com mais histórias do que antes. Da próxima vez, você conta a primeira.", "Coloquei uma etiqueta nova no caderno: 'Aprendido em companhia'. Ela serve para quase tudo que aconteceu desde que você chegou.", "Já estão perguntando quando vamos arrumar a mesa de novo. Deixei a receita em branco no final, para as próximas colheitas sugerirem alguma coisa.", "A horta não ficou pronta para sempre. Amanhã vai precisar de água e cuidado outra vez. A diferença é que agora mais gente sabe onde ajudar.", "O verso ficou melhor quando ouvi os nomes de todo mundo juntos. Seu começo no vale já não cabe em uma canção só.", "Guarde o caderno na fazenda. Ele passou tempo demais esperando uma última página. Prefiro que agora tenha sempre uma próxima."]
    ];
}
