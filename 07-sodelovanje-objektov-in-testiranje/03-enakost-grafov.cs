// Samostojen primer: dotnet 03-enakost-grafov.cs
using System;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        var a = new Graf(3);
        a.DodajPovezave(new (int, int)[] { (0, 1), (1, 2) });
        var b = a.Kopija();
        Console.WriteLine(ReferenceEquals(a, b)); // False
        Console.WriteLine(a.Equals(b));           // True
        Console.WriteLine(a == b);                // True
        b.DodajPovezavo(0, 2);
        Console.WriteLine(a == b);                // False

        var mnozica = new HashSet<Graf>();
        Console.WriteLine(mnozica.Add(a));          // True
        Console.WriteLine(mnozica.Add(a.Kopija())); // False
        Console.WriteLine(mnozica.Count);           // 1

        a.DodajPovezavo(0, 2);  // Spremenimo graf, ki je že v množici.
        Console.WriteLine(mnozica.Contains(a));     // False
        Console.WriteLine(mnozica.Contains(b));     // False
    }
}

public class Graf : IEquatable<Graf>
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
}
