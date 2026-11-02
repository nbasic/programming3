// Samostojen primer: dotnet 04-urejanje.cs
using System;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        var z = new ZemljevidMest();
        z.DodajMesto("Ljubljana");
        z.DodajMesto("Kranj");
        z.DodajMesto("Bled");
        z.DodajMesto("Kamnik");
        z.DodajCesto("Ljubljana", "Kranj");
        z.DodajCesto("Kranj", "Bled");
        z.DodajCesto("Ljubljana", "Kamnik");

        List<StopnjaMesta> mesta = z.MestaPoStopnji();
        Console.WriteLine(string.Join(", ", mesta));
        // Kranj (2), Ljubljana (2), Bled (1), Kamnik (1)
        mesta.Sort(StopnjaMesta.PoImenu);
        Console.WriteLine(string.Join(", ", mesta));
        // Bled (1), Kamnik (1), Kranj (2), Ljubljana (2)

        var grafi = new List<Graf>();
        for (int a = 0; a <= 1; a++)
            for (int b = 0; b <= 1; b++)
                for (int c = 0; c <= 1; c++)
                {
                    var g = new Graf(3);
                    if (a == 1) g.DodajPovezavo(1, 2);
                    if (b == 1) g.DodajPovezavo(0, 2);
                    if (c == 1) g.DodajPovezavo(0, 1);
                    grafi.Add(g);
                }
        grafi.Sort();
        foreach (Graf g in grafi)
            Console.WriteLine(g);
    }
}

public class Graf : IEquatable<Graf>, IComparable<Graf>
{
    private bool[,] sosednost;
    public int SteviloVozlisc => sosednost.GetLength(0);
    public int SteviloPovezav { get; private set; }

    public Graf(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        sosednost = new bool[n, n];
    }

    public int DodajVozlisce()
    {
        int n = SteviloVozlisc;
        var nova = new bool[n + 1, n + 1];
        for (int u = 0; u < n; u++)
            for (int v = 0; v < n; v++)
                nova[u, v] = sosednost[u, v];
        sosednost = nova;
        return n;
    }

    private void PreveriVozlisce(int v)
    {
        if (v < 0 || v >= SteviloVozlisc)
            throw new ArgumentOutOfRangeException(nameof(v));
    }

    private void PreveriPovezavo(int u, int v)
    {
        PreveriVozlisce(u);
        PreveriVozlisce(v);
        if (u == v)
            throw new ArgumentException("Zanke niso dovoljene.");
    }

    public void DodajPovezavo(int u, int v)
    {
        PreveriPovezavo(u, v);
        if (sosednost[u, v]) return;
        sosednost[u, v] = sosednost[v, u] = true;
        SteviloPovezav++;
    }

    public void DodajPovezave((int U, int V)[] povezave)
    {
        foreach (var (u, v) in povezave)
            PreveriPovezavo(u, v);
        foreach (var (u, v) in povezave)
            DodajPovezavo(u, v);
    }

    public bool StaSoseda(int u, int v)
    {
        PreveriVozlisce(u);
        PreveriVozlisce(v);
        return sosednost[u, v];
    }

    public int Stopnja(int v)
    {
        PreveriVozlisce(v);
        int stopnja = 0;
        for (int u = 0; u < SteviloVozlisc; u++)
            if (sosednost[v, u]) stopnja++;
        return stopnja;
    }

    public List<int> Sosedi(int v)
    {
        PreveriVozlisce(v);
        var sosedi = new List<int>();
        for (int u = 0; u < SteviloVozlisc; u++)
            if (sosednost[v, u]) sosedi.Add(u);
        return sosedi;
    }

    public void OdstraniPovezavo(int u, int v)
    {
        PreveriPovezavo(u, v);
        if (!sosednost[u, v]) return;
        sosednost[u, v] = sosednost[v, u] = false;
        SteviloPovezav--;
    }

    public bool JePovezan()
    {
        if (SteviloVozlisc == 0) return false;
        bool[] obiskano = new bool[SteviloVozlisc];
        var vrsta = new Queue<int>();
        obiskano[0] = true;
        vrsta.Enqueue(0);
        int dosezenih = 1;

        while (vrsta.Count > 0)
        {
            int v = vrsta.Dequeue();
            foreach (int u in Sosedi(v))
            {
                if (obiskano[u]) continue;
                obiskano[u] = true;
                dosezenih++;
                vrsta.Enqueue(u);
            }
        }
        return dosezenih == SteviloVozlisc;
    }

    public Graf Kopija()
    {
        var kopija = new Graf(SteviloVozlisc);
        for (int u = 0; u < SteviloVozlisc; u++)
            for (int v = u + 1; v < SteviloVozlisc; v++)
                if (sosednost[u, v]) kopija.DodajPovezavo(u, v);
        return kopija;
    }

    public override string ToString()
    {
        var povezave = new List<string>();
        for (int u = 0; u < SteviloVozlisc; u++)
            for (int v = u + 1; v < SteviloVozlisc; v++)
                if (sosednost[u, v]) povezave.Add(u + "-" + v);
        return "Graf(" + SteviloVozlisc + ", {" + string.Join(", ", povezave) + "})";
    }

    public bool Equals(Graf? drugi)
    {
        if (drugi is null) return false;
        if (ReferenceEquals(this, drugi)) return true;
        if (SteviloVozlisc != drugi.SteviloVozlisc) return false;
        if (SteviloPovezav != drugi.SteviloPovezav) return false;
        for (int u = 0; u < SteviloVozlisc; u++)
            for (int v = u + 1; v < SteviloVozlisc; v++)
                if (sosednost[u, v] != drugi.sosednost[u, v]) return false;
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Graf drugi && Equals(drugi);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(SteviloVozlisc);
        for (int u = 0; u < SteviloVozlisc; u++)
            for (int v = u + 1; v < SteviloVozlisc; v++)
                if (sosednost[u, v])
                {
                    hash.Add(u);
                    hash.Add(v);
                }
        return hash.ToHashCode();
    }

    public static bool operator ==(Graf? a, Graf? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(Graf? a, Graf? b)
    {
        return !(a == b);
    }

    public int CompareTo(Graf? drugi)
    {
        if (drugi is null) return 1;
        int primerjava = SteviloVozlisc.CompareTo(drugi.SteviloVozlisc);
        if (primerjava != 0) return primerjava;
        primerjava = SteviloPovezav.CompareTo(drugi.SteviloPovezav);
        if (primerjava != 0) return primerjava;
        for (int u = 0; u < SteviloVozlisc; u++)
            for (int v = u + 1; v < SteviloVozlisc; v++)
            {
                primerjava = sosednost[u, v].CompareTo(drugi.sosednost[u, v]);
                if (primerjava != 0) return primerjava;
            }
        return 0;
    }
}

public class StopnjaMesta : IComparable<StopnjaMesta>
{
    public string Ime { get; }
    public int Stopnja { get; }

    public StopnjaMesta(string ime, int stopnja)
    {
        Ime = ime;
        Stopnja = stopnja;
    }

    public int CompareTo(StopnjaMesta? drugo)
    {
        if (drugo is null) return 1;
        int primerjava = drugo.Stopnja.CompareTo(Stopnja); // padajoče
        if (primerjava != 0) return primerjava;
        return string.CompareOrdinal(Ime, drugo.Ime);
    }

    public static int PoImenu(StopnjaMesta a, StopnjaMesta b)
    {
        return string.CompareOrdinal(a.Ime, b.Ime);
    }

    public override string ToString()
    {
        return $"{Ime} ({Stopnja})";
    }
}

public class ZemljevidMest
{
    private readonly Graf graf = new Graf(0);
    private readonly List<string> imena = new List<string>();
    private readonly Dictionary<string, int> indeksi =
        new Dictionary<string, int>();

    public int SteviloMest => imena.Count;
    public int SteviloCest => graf.SteviloPovezav;

    public void DodajMesto(string ime)
    {
        if (string.IsNullOrWhiteSpace(ime))
            throw new ArgumentException("Ime mesta ne sme biti prazno.");
        if (indeksi.ContainsKey(ime))
            throw new ArgumentException($"Mesto {ime} že obstaja.");

        int v = graf.DodajVozlisce();
        imena.Add(ime);
        indeksi.Add(ime, v);
    }

    private int Indeks(string ime)
    {
        if (!indeksi.TryGetValue(ime, out int v))
            throw new ArgumentException($"Neznano mesto: {ime}.");
        return v;
    }

    public void DodajCesto(string a, string b)
    {
        graf.DodajPovezavo(Indeks(a), Indeks(b));
    }

    public bool StaPovezani(string a, string b)
    {
        return graf.StaSoseda(Indeks(a), Indeks(b));
    }

    public int SteviloCestIz(string ime)
    {
        return graf.Stopnja(Indeks(ime));
    }

    public List<string> Sosedi(string ime)
    {
        var sosedi = new List<string>();
        foreach (int u in graf.Sosedi(Indeks(ime)))
            sosedi.Add(imena[u]);
        return sosedi;
    }

    public bool JePovezan()
    {
        return graf.JePovezan();
    }

    public Graf KopijaGrafa()
    {
        return graf.Kopija();
    }

    public List<StopnjaMesta> MestaPoStopnji()
    {
        var seznam = new List<StopnjaMesta>();
        for (int v = 0; v < imena.Count; v++)
            seznam.Add(new StopnjaMesta(imena[v], graf.Stopnja(v)));
        seznam.Sort();
        return seznam;
    }
}
