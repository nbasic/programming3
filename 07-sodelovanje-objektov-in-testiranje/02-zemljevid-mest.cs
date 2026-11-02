// Samostojen primer: dotnet 02-zemljevid-mest.cs
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

        Console.WriteLine(z.StaPovezani("Bled", "Kranj"));          // True
        Console.WriteLine(string.Join(", ", z.Sosedi("Ljubljana"))); // Kranj, Kamnik
        Console.WriteLine(z.SteviloCestIz("Kranj"));                // 2
        Console.WriteLine(z.KopijaGrafa()); // Graf(4, {0-1, 0-3, 1-2})

        try
        {
            z.DodajCesto("Kranj", "Maribor");
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);   // Neznano mesto: Maribor.
        }
        Console.WriteLine(z.SteviloCest);   // 3
    }
}

public class Graf
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
}
