namespace CampusNet.Models;

/// <summary>Resultado de un recorrido en anchura (BFS).</summary>
public sealed record BfsResult(
    string Start,
    IReadOnlyList<string> VisitOrder,
    IReadOnlyDictionary<string, int> Levels,
    IReadOnlyDictionary<string, string?> Parents)
{
    public int ReachedCount => VisitOrder.Count;
}

/// <summary>Resultado de un recorrido en profundidad (DFS) completo.</summary>
public sealed record DfsResult(
    IReadOnlyList<string> DiscoveryOrder,
    IReadOnlyList<IReadOnlyList<string>> Cycles);

/// <summary>Entrada de un ranking por grado (entrada o salida).</summary>
public sealed record DegreeEntry(Vertex Vertex, int Degree);
