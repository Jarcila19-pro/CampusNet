namespace CampusNet.Models;

/// <summary>
/// Arista dirigida: Origin → Destination significa que Origin sigue a Destination.
/// La igualdad se define por el par ordenado, lo que ayuda a detectar duplicados.
/// </summary>
public sealed record Edge(string Origin, string Destination);
