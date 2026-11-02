// Samostojen primer: dotnet 05-rocni-preizkusi.cs
using System;
using System.Collections.Generic;

public class Program
{
    private static void Preveri(bool pogoj, string opis)
    {
        Console.WriteLine((pogoj ? "V redu: " : "NAPAKA: ") + opis);
    }

    public static void Main()
    {
        var g = new Graf(3);
        g.DodajPovezavo(0, 1);
        g.DodajPovezavo(1, 0);
        Preveri(g.SteviloPovezav == 1, "ponovljena povezava se šteje enkrat");

        int v = g.DodajVozlisce();
        Preveri(v == 3, "novo vozlišče ima indeks 3");
        Preveri(g.StaSoseda(0, 1), "povezava 0-1 ostane");
        Preveri(g.Stopnja(v) == 0, "novo vozlišče nima sosedov");

        try
        {
            g.DodajPovezavo(1, 1);
            Preveri(false, "zanka mora sprožiti izjemo");
        }
        catch (ArgumentException)
        {
            Preveri(true, "zanka sproži izjemo");
        }
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
