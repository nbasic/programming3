// Samostojen primer: dotnet 01-dodajanje-vozlisc.cs
using System;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        var g = new Graf(3);
        g.DodajPovezave(new (int, int)[] { (0, 1), (1, 2) });
        Console.WriteLine(g);              // Graf(3, {0-1, 1-2})

        int v = g.DodajVozlisce();
        g.DodajPovezavo(v, 0);
        Console.WriteLine(v);              // 3
        Console.WriteLine(g);              // Graf(4, {0-1, 0-3, 1-2})
        Console.WriteLine(g.Stopnja(0));   // 2
        Console.WriteLine(g.JePovezan());  // True

        var prazen = new Graf(0);
        Console.WriteLine(prazen.DodajVozlisce()); // 0
        Console.WriteLine(prazen);         // Graf(1, {})
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
