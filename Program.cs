using System.Text.RegularExpressions;

class Valve
{
    public static int usefulValvesCounter = 0;
    public List<int> neighbors;

    private int usefulId;
    public int UsefulId { get => usefulId; }
    private int tag;
    public int Tag { get => tag; }
    private int pressure;
    public int Pressure { get => pressure; }
    public Valve(string tagStr, int pressure)
    {
        this.pressure = pressure;
        usefulId = -1;
        if (this.pressure > 0)
        {
            usefulId = (1 << usefulValvesCounter);
            usefulValvesCounter++;
        }
        this.tag = valveInt(tagStr);
        neighbors = new List<int>();
    }

    public void AddNeighbor(string neigh)
    {
        neighbors.Add(valveInt(neigh));
    }

    public static int valveInt(string s)
    {
        int v = (int)s[0];
        return (v << 8) + (int)s[1];
    }

    public static int valveState(int visited, int ntag)
    {
        return (ntag << 16) | visited;
    }

    public static void valveVisitedTag(int state, out int visited, out int ntag)
    {
        visited = (0xffff & state);
        ntag = (state >> 16);
    }
}

internal class Program
{

    private static void Main(string[] args)
    {
        Dictionary<int, Valve> valvesDict = new Dictionary<int, Valve>();
        string[] lines = File.ReadAllLines(@"input.txt");
        Regex rx = new Regex(@"^Valve ([A-Z]{2}) has flow rate=(\d+); tunnels? leads? to valves? ([A-Z]{2}){1}(?:, ([A-Z]{2}))*",
          RegexOptions.Compiled);

        foreach (string line in lines)
        {

            GroupCollection groups = rx.Match(line).Groups;

            Valve valve = new Valve(groups[1].Value, int.Parse(groups[2].Value));
            valvesDict[valve.Tag] = valve;
            valve.AddNeighbor(groups[3].Value);
            foreach (var s in groups[4].Captures)
            {
                valve.AddNeighbor(s.ToString());
            }

        }

        int start = Valve.valveInt("AA");
        

        Dictionary<int, long> statePressCurr, statePressPrev;
        statePressCurr = new Dictionary<int, long>();
        statePressCurr[Valve.valveState(0, start)] = 0;
        int state, visited, valveTag;
        long currPresure, newPressure;
        for (long k = 29; k > 0; k--)
        {
            statePressPrev = statePressCurr;
            statePressCurr = new Dictionary<int, long>();
            foreach (var entry in statePressPrev)
            {
                state = entry.Key;
                Valve.valveVisitedTag(state, out visited, out valveTag);
                Valve valve = valvesDict[valveTag];
                newPressure = entry.Value;
                foreach (int neighTag in valve.neighbors)
                {
                    state = Valve.valveState(visited, neighTag);
                    if (!statePressCurr.TryGetValue(state, out currPresure) || newPressure > currPresure)
                    {
                        statePressCurr[state] = newPressure;
                    }
                }

                if (valve.UsefulId != -1 && (valve.UsefulId & visited) == 0)
                {
                    state = Valve.valveState(valve.UsefulId | visited, valveTag);
                    newPressure = entry.Value + k * valve.Pressure;
                }
                else
                {
                    state = entry.Key;
                    newPressure = entry.Value;
                }

                if (!statePressCurr.TryGetValue(state, out currPresure) || newPressure > currPresure)
                {
                    statePressCurr[state] = newPressure;
                }


            }
        }

        long maxVal = -1;
        foreach (var entry in statePressCurr)
        {
            if (entry.Value > maxVal)
                maxVal = entry.Value;
        }

        Console.WriteLine(maxVal);

        return;

    }
}