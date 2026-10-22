using static ValeDoSol.Logic;
namespace ValeDoSol;

public readonly record struct Appointment(int start, Point at, string activity, Scene scene = Scene.VILLAGE);
public static class Social
{
    public const int Count = 8;
    static int Visit(State s, int npc) { int value = s.conversations[npc]; s.conversations[npc] = Math.Min(10_000_000, value + 1); return value; }
    static Point P(int x, int y) => new(x * 32 + 16, y * 32 + 16);
    public static Appointment[] Schedule(int npc, State s)
    {
        bool weekend = (s.day - 1) % 7 >= 5;
        Point work = NPCS[npc].morning, lunch = P(10 + npc * 2, 12), afternoon = NPCS[npc].afternoon, evening = P(11 + npc * 2, 17), home = P(7 + npc * 3, 9);
        string activity = npc switch { 0 => "Atendendo no armazém", 1 => "Medindo tábuas na oficina", 2 => "Pescando no lago", 3 => "Anotando plantas", 4 => "Preparando a banca de pães", 5 => "Cuidando da horta de ervas", 6 => "Ensaiando uma canção", _ => "Lendo no jardim" };
        if (npc == 0 && weekend) return [new(360, P(7, 9), "Preparando a feira"), new(480, new(352, 184), "Atendendo no armazém", SHOP), new(780, P(10, 13), "Visitando a feira"), new(1080, evening, "Encontrando os vizinhos"), new(1170, home, "Voltando para casa")];
        if (npc == 0 && !weekend) return [new(360, P(7, 9), "Abrindo o armazém"), new(480, new(352, 184), activity, SHOP), new(1020, P(10, 12), "Passeando depois do trabalho"), new(1140, P(7, 9), "Voltando para casa")];
        if (raining(s)) { Point[] shelter = [P(7, 8), P(26, 8), P(8, 8), P(17, 7), P(35, 8), P(19, 7), P(6, 8), P(42, 10)]; return [new(360, home, "Esperando a chuva passar"), new(540, shelter[npc], "Procurando abrigo"), new(780, shelter[npc], "Conversando junto à varanda"), new(1080, home, "Voltando para casa")]; }
        if (weekend) return [new(360, home, "Começando o dia devagar"), new(540, P(10 + npc * 2, 13), "Visitando a feira"), new(750, lunch, "Almoçando na praça"), new(900, afternoon, "Passeando pelo vale"), new(1080, evening, "Encontrando os vizinhos"), new(1170, home, "Voltando para casa")];
        return [new(360, home, "Saindo de casa"), new(480, work, activity), new(720, lunch, "Fazendo uma pausa para almoço"), new(840, afternoon, activity), new(1020, evening, "Conversando na praça"), new(1140, home, "Voltando para casa")];
    }
    public static int Phase(int npc, State s) { var plan = Schedule(npc, s); int phase = 0; for (int i = 0; i < plan.Length; i++) if (s.minute >= plan[i].start) phase = i; return phase; }
    public static Appointment Now(int npc, State s) => Schedule(npc, s)[Phase(npc, s)];
    public static Point Walkable(Point desired)
    {
        for (int radius = 0; radius < 7; radius++) for (int y = -radius; y <= radius; y++) for (int x = -radius; x <= radius; x++) { if (Math.Abs(x) + Math.Abs(y) != radius) continue; Point at = new((int)(desired.x / 32) * 32 + 16 + x * 32, (int)(desired.y / 32) * 32 + 16 + y * 32); if (!blocked(VILLAGE, at.x, at.y)) return at; }
        return new(432, 384);
    }
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
    public static string Greeting(State s, int npc, bool first)
    {
        if (first) return NPCS[npc].first;
        int visit = Visit(s, npc), variant = (visit + s.day + npc) % Daily[npc].Length;
        if (visit % 4 == 0 && raining(s)) return Rain[npc];
        if (visit % 4 == 1 && s.friendship[npc] >= 4) return Close[npc];
        if (visit % 4 == 2 && s.minute >= 1080) return "O dia passou depressa. " + NPCS[npc].later[0];
        if (visit % 4 == 3 && (s.day - 1) % 7 >= 5) return "Hoje a feira me tirou da rotina. " + Daily[npc][variant];
        return Daily[npc][variant];
    }
    public static string Topic(State s, int npc, int topic)
    {
        if (topic == 0) return NPCS[npc].later[Visit(s, npc) % NPCS[npc].later.Length];
        if (topic == 1) return s.friendship[npc] >= 3 ? Close[npc] : Daily[npc][(npc + s.day) % 4];
        return "Agora estou " + Now(npc, s).activity.ToLowerInvariant() + ". " + (raining(s) ? Rain[npc] : Daily[npc][(s.day + 2) % 4]);
    }
    public static string GiftReply(int npc) => new[] { "Da sua própria horta? Vou separar para o jantar com minha avó. Obrigada por lembrar da gente!", "Puxa... você plantou isso? Vou cozinhar hoje. Já estava cansado de jantar só pão. Obrigado, amigo.", "Vou preparar para comer depois da pesca. Você sabe cuidar das pessoas tão bem quanto cuida da horta.", "Olha a cor dessas folhas! Prometo guardar uma parte para a pesquisa e comer o resto. Obrigado!", "Isso merece virar receita! Quando sentir um cheirinho bom vindo da banca, pode aparecer.", "Fresquinho assim faz bem até para o humor. Muito obrigado por compartilhar sua colheita.", "Vou inventar uma canção de agradecimento. Uma bem curtinha, para o presente não estragar enquanto eu canto!", "Um presente que cresceu com cuidado tem uma história bonita. Vou lembrar de quem o trouxe." }[npc];
}
