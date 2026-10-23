using static ValeDoSol.Logic;
namespace ValeDoSol;

public enum NpcPose { Idle, Work, Inspect, Fish, Read, Carry, Talk }
public readonly record struct NpcTaskStep(Point at, float pause, int facing, string label, NpcPose pose);

/// <summary>
/// Small, deliberate tasks while an appointment is active. Destinations are feet
/// positions, validated by the same collision model used by player navigation.
/// This replaces one shared circular wandering pattern for every occupation.
/// </summary>
public static class NpcBehavior
{
    static int Choice(int cycle, int length) => (int)((uint)cycle % (uint)length);
    static bool Has(Appointment activity, string word) => activity.activity.Contains(word, StringComparison.OrdinalIgnoreCase);
    static Point Select(int cycle, params Point[] points) => points[Choice(cycle, points.Length)];

    public static NpcTaskStep Step(State s, int npc, Appointment activity, int cycle)
    {
        if (npc < 0 || npc >= Social.Count) return new(activity.at, 3, 0, "", NpcPose.Idle);
        Point destination = activity.at;
        string label = activity.activity;
        NpcPose pose = NpcPose.Idle;
        int facing = 0;

        if (Social.FestivalDay(s) && s.minute is >= 1020 and < 1225 && activity.scene == Scene.VILLAGE && (Has(activity, "Celebrando") || Has(activity, "Compartilhando")))
        {
            // Keep gathering places distinct and leave the central fountain free.
            Point[] offsets = [new(0, 0), new(16, 0), new(0, 16), new(0, 0)];
            Point offset = offsets[Choice(cycle + npc, offsets.Length)];
            destination = new(activity.at.x + offset.x, activity.at.y + offset.y);
            return Safe(activity, destination, 3.5f + npc % 3, FaceToward(destination, new(656, 576)), "Conversando na celebração do jardim", NpcPose.Talk);
        }

        if (npc == 0 && activity.scene == Scene.SHOP)
        {
            destination = Select(cycle,
                new(352, 200), new(208, 200), new(208, 288),
                new(352, 200), new(496, 200), new(496, 288), new(352, 200));
            label = Choice(cycle, 7) switch { 1 or 2 => "Conferindo as caixas de sementes", 4 or 5 => "Separando os pedidos do armazém", _ => "Atendendo os vizinhos" };
            pose = Choice(cycle, 7) is 0 or 3 or 6 ? NpcPose.Talk : NpcPose.Carry;
            facing = destination.y <= 208 ? 0 : destination.x < 352 ? 1 : 2;
        }
        else if (npc == 1 && activity.scene == Scene.WORKSHOP)
        {
            destination = Select(cycle, new(192, 224), new(320, 176), new(496, 224), new(352, 288), new(192, 224));
            label = Choice(cycle, 5) switch { 0 or 4 => "Medindo a madeira do barco", 1 => "Escolhendo pregos e ferramentas", 2 => "Conferindo as peças do palco", _ => "Consultando o desenho da bancada" };
            pose = NpcPose.Work;
            facing = Choice(cycle, 5) == 3 ? 2 : 3;
        }
        else if (npc == 2 && activity.scene is Scene.FARM or Scene.VILLAGE && (Has(activity, "pesc") || Has(activity, "lago") || Has(activity, "margem") || Has(activity, "cais")))
        {
            (destination, facing) = Shore(activity, cycle);
            label = Choice(cycle, 4) switch { 0 => "Observando os círculos na água", 1 => "Ajustando a linha de pesca", 2 => "Procurando sombra na margem", _ => "Esperando uma fisgada" };
            pose = NpcPose.Fish;
        }
        else if (npc == 3 && activity.scene == Scene.FARM)
        {
            destination = Select(cycle, new(816, 272), new(816, 336), new(816, 432), new(864, 448));
            label = Choice(cycle, 4) switch { 0 => "Anotando o crescimento das folhas", 1 => "Comparando a terra dos canteiros", 2 => "Observando as flores da horta", _ => "Desenhando uma descoberta no caderno" };
            pose = NpcPose.Inspect; facing = 1;
        }
        else if (npc == 3 && activity.scene == Scene.GREENHOUSE)
        {
            destination = Select(cycle, new(208, 224), new(304, 192), new(432, 192), new(496, 400), new(352, 256));
            label = Choice(cycle, 5) switch { 0 => "Comparando sementes sobre a mesa", 1 or 2 => "Examinando uma muda", 3 => "Atualizando o catálogo de plantas", _ => "Conversando com Miguel sobre as ervas" };
            pose = NpcPose.Inspect; facing = 3;
        }
        else if (npc == 4 && activity.scene == Scene.BAKERY)
        {
            destination = Select(cycle, new(224, 192), new(400, 176), new(592, 176), new(224, 272), new(400, 176));
            label = Choice(cycle, 5) switch { 0 => "Conferindo a temperatura do forno", 1 or 4 => "Separando os pães da próxima entrega", 2 => "Consultando a receita da família", _ => "Preparando os pedidos do balcão" };
            pose = Choice(cycle, 5) is 1 or 4 ? NpcPose.Carry : NpcPose.Work; facing = 3;
        }
        else if (npc == 4 && activity.scene == Scene.VILLAGE && (Has(activity, "banca") || Has(activity, "feira")))
        {
            destination = Select(cycle, new(activity.at.x - 32, activity.at.y), activity.at, new(activity.at.x + 32, activity.at.y), activity.at);
            label = Choice(cycle, 4) == 3 ? "Conversando com quem passa pela banca" : "Arrumando as cestas de pão";
            pose = NpcPose.Carry; facing = 0;
        }
        else if (npc == 5 && activity.scene == Scene.GREENHOUSE)
        {
            destination = Select(cycle, new(304, 192), new(432, 192), new(592, 192), new(528, 272), new(224, 224));
            label = Choice(cycle, 5) switch { 0 => "Conferindo a umidade da terra", 1 => "Separando folhas secas", 2 => "Cuidando das ervas da janela", 3 => "Escrevendo etiquetas para as mudas", _ => "Guardando as ferramentas do jardim" };
            pose = NpcPose.Inspect; facing = 3;
        }
        else if (npc == 5 && activity.scene == Scene.VILLAGE && (Has(activity, "horta") || Has(activity, "erva")))
        {
            destination = Select(cycle, new(688, 288), new(720, 288), new(752, 288), new(784, 288), new(720, 304));
            label = Choice(cycle, 5) == 4 ? "Limpando a ferramenta antes de guardar" : "Examinando as ervas do jardim";
            pose = NpcPose.Inspect; facing = 3;
        }
        else if (npc == 6 && activity.scene == Scene.COTTAGE)
        {
            destination = Select(cycle, new(448, 224), new(352, 256), new(480, 272), new(352, 304));
            label = Choice(cycle, 4) switch { 0 => "Relendo o verso que escreveu", 1 => "Marcando o ritmo com os dedos", 2 => "Conferindo uma anotação na partitura", _ => "Procurando a próxima frase da canção" };
            pose = NpcPose.Read; facing = 3;
        }
        else if (npc == 7 && activity.scene == Scene.LIBRARY)
        {
            destination = Select(cycle, new(280, 192), new(432, 240), new(512, 336), new(432, 400), new(192, 336));
            label = Choice(cycle, 5) switch { 0 => "Procurando um volume na estante", 1 => "Consultando uma carta antiga", 2 => "Organizando os livros devolvidos", 3 => "Anotando uma história no caderno", _ => "Voltando a um trecho da leitura" };
            pose = Choice(cycle, 5) == 2 ? NpcPose.Carry : NpcPose.Read; facing = Choice(cycle, 5) == 3 ? 1 : 3;
        }
        else
        {
            // Visits stay near the host or reading spot; each resident has a
            // different route shape instead of marching around the same circle.
            Point[][] steps = [
                [new(0, 0), new(-32, 0), new(0, 16), new(32, 0)],
                [new(0, 0), new(0, -32), new(32, -32), new(0, 0)],
                [new(0, 0), new(-32, 16), new(32, 16), new(0, 0)],
                [new(0, 0), new(32, 0), new(32, 32), new(-16, 16)],
                [new(0, 0), new(-32, 0), new(32, 0), new(0, 16)],
                [new(0, 0), new(16, -32), new(32, 0), new(-16, 16)],
                [new(0, 0), new(48, 0), new(16, 32), new(-32, 0)],
                [new(0, 0), new(0, 32), new(-32, 16), new(0, 0)]
            ];
            Point offset = steps[npc][Choice(cycle, steps[npc].Length)]; destination = new(activity.at.x + offset.x, activity.at.y + offset.y);
            bool visiting = Has(activity, "Visitando") || Has(activity, "Conversando") || Has(activity, "encontro") || Has(activity, "café");
            pose = visiting ? NpcPose.Talk : npc == 7 ? NpcPose.Read : npc is 3 or 5 ? NpcPose.Inspect : NpcPose.Idle;
            facing = Map.Indoors(activity.scene) ? 3 : npc % 4;
        }
        return Safe(activity, destination, DwellSeconds(npc, activity, cycle), facing, label, pose);
    }

    static NpcTaskStep Safe(Appointment activity, Point desired, float pause, int facing, string label, NpcPose pose)
    {
        Point safe = Physics.Nearest(activity.scene, desired);
        // Never let a small task turn into an unrelated tour of the whole room.
        if (Physics.Distance(safe, desired) > 96) safe = Physics.Nearest(activity.scene, activity.at);
        return new(safe, pause, facing, label, pose);
    }

    static (Point at, int facing) Shore(Appointment activity, int cycle)
    {
        Rectangle[] ponds = water_bodies(activity.scene);
        if (ponds.Length == 0) return (activity.at, 0);
        Rectangle pond = ponds.MinBy(p => Physics.Distance(activity.at, new(Math.Clamp(activity.at.x, p.x, p.x + p.width), Math.Clamp(activity.at.y, p.y, p.y + p.height))));
        int offset = new[] { 0, -32, 32, -16, 16 }[Choice(cycle, 5)];
        Point anchor = activity.at;
        if (anchor.y >= pond.y + pond.height) return (new(Math.Clamp(anchor.x + offset, pond.x + 24, pond.x + pond.width - 24), pond.y + pond.height + 24), 3);
        if (anchor.y <= pond.y) return (new(Math.Clamp(anchor.x + offset, pond.x + 24, pond.x + pond.width - 24), pond.y - 24), 0);
        if (anchor.x < pond.x) return (new(pond.x - 24, Math.Clamp(anchor.y + offset, pond.y + 24, pond.y + pond.height - 24)), 2);
        return (new(pond.x + pond.width + 24, Math.Clamp(anchor.y + offset, pond.y + 24, pond.y + pond.height - 24)), 1);
    }

    public static float DwellSeconds(int npc, Appointment activity, int cycle)
    {
        float variation = Choice(cycle + npc, 3) * .65f;
        return npc switch { 0 => 2.8f + variation, 1 => 3.6f + variation, 2 => 5.2f + variation, 3 => 2.8f + variation, 4 => 2.4f + variation, 5 => 3.2f + variation, 6 => 2.2f + variation, _ => 4.4f + variation };
    }

    public static int FaceToward(Point from, Point to)
    {
        float dx = to.x - from.x, dy = to.y - from.y;
        return Math.Abs(dx) > Math.Abs(dy) ? dx < 0 ? 1 : 2 : dy < 0 ? 3 : 0;
    }

    public static string ProximityGreeting(State s, int npc, Scene scene, Point player, Point resident)
    {
        if (npc < 0 || npc >= Social.Count || (s.met & (1u << npc)) == 0 || s.talked_day[npc] == s.day || Physics.Distance(player, resident) > 72) return "";
        return npc switch
        {
            0 => scene == Scene.SHOP ? "Escolhi sementes novas hoje!" : raining(s) ? "Entre, tem café quentinho!" : "Passe no armazém quando puder!",
            1 => "Ei, fazendeiro! Como vai a cerca?",
            2 => "Hoje o lago está cheio de histórias.",
            3 => "Encontrei uma coisa para lhe mostrar!",
            4 => scene == Scene.BAKERY ? "Saiu uma fornada agora!" : s.minute < 720 ? "Já tomou café da manhã?" : "Separei pão para quem vai passar.",
            5 => "Lembre de fazer uma pausa, amigo.",
            6 => "Chegou bem na hora do intervalo!",
            _ => "Bom encontrar você pelo caminho."
        };
    }

    public static string SmallTalk(State s, int npc)
    {
        string[] rain = ["O chão seca depois. Entre!", "Vou ouvir a chuva da oficina.", "Guardei as iscas da umidade.", "Hoje estudo as folhas dos livros.", "Dia de forno aceso e visitas.", "As mudas estão protegidas.", "Esse telhado tem um bom ritmo.", "Um capítulo e outra xícara de chá."];
        string[] morning = ["Só falta conferir as sementes.", "Primeiro, uma medida bem feita.", "Devagar, para não assustar os peixes.", "Onde deixei a etiqueta desta muda?", "Essa fornada está quase pronta.", "A terra ainda guarda a água de ontem.", "Acho que encontrei o começo do verso.", "Vou marcar esta página para depois."];
        string[] evening = ["Vou levar uma história para casa.", "Chega de só mais um ajuste por hoje.", "Ceci vai gostar de ouvir esta.", "Preciso passar as notas a limpo.", "Um pão para quem passa por último.", "Agora as plantas descansam também.", "Mais um verso antes de guardar.", "Ainda cabe uma pequena leitura."];
        npc = Math.Clamp(npc, 0, Social.Count - 1);
        return raining(s) ? rain[npc] : s.minute >= 1050 ? evening[npc] : morning[npc];
    }
}
