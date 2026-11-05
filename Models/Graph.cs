namespace CampusNet.Models;

/// <summary>
/// Grafo dirigido con lista de adyacencia.
/// Toda la lógica de negocio y las validaciones de integridad viven aquí;
/// esta clase no imprime nada en consola.
/// </summary>
public sealed class Graph
{
    private readonly Dictionary<string, Vertex> _vertices = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _adjacency = new(StringComparer.Ordinal);
    private readonly List<Edge> _edges = new();

    private enum VisitColor
    {
        White,
        Gray,
        Black
    }

    public IReadOnlyDictionary<string, Vertex> Vertices => _vertices;
    public IReadOnlyDictionary<string, List<string>> Adjacency => _adjacency;
    public IReadOnlyList<Edge> Edges => _edges;
    public int VertexCount => _vertices.Count;
    public int EdgeCount => _edges.Count;

    public bool ContainsVertex(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && _vertices.ContainsKey(id.Trim());
    }

    public bool ContainsEdge(string origin, string destination)
    {
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
        {
            return false;
        }

        var from = origin.Trim();
        var to = destination.Trim();
        return _adjacency.TryGetValue(from, out var neighbors) && neighbors.Contains(to);
    }

    public Vertex GetVertex(string id)
    {
        var key = Vertex.NormalizeId(id);
        if (!_vertices.TryGetValue(key, out var vertex))
        {
            throw new KeyNotFoundException($"No existe un usuario con Id '{key}'.");
        }

        return vertex;
    }

    // ---------------------------------------------------------------------
    // CRUD de vértices
    // ---------------------------------------------------------------------

    /// <summary>Agrega un usuario. Rechaza Id duplicados.</summary>
    public void AddVertex(Vertex vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (_vertices.ContainsKey(vertex.Id))
        {
            throw new InvalidOperationException(
                $"Ya existe un usuario con Id '{vertex.Id}'. Los Id deben ser únicos.");
        }

        _vertices.Add(vertex.Id, vertex);
        _adjacency.Add(vertex.Id, new List<string>());
    }

    /// <summary>
    /// Elimina un usuario y todas sus relaciones incidentes (entrantes y salientes).
    /// Devuelve las aristas eliminadas para poder reportar el cambio.
    /// </summary>
    public IReadOnlyList<Edge> RemoveVertex(string id)
    {
        var key = Vertex.NormalizeId(id);
        if (!_vertices.ContainsKey(key))
        {
            throw new KeyNotFoundException($"No existe un usuario con Id '{key}'.");
        }

        var removedEdges = _edges
            .Where(edge => edge.Origin == key || edge.Destination == key)
            .ToList();

        foreach (var edge in removedEdges)
        {
            if (_adjacency.TryGetValue(edge.Origin, out var neighbors))
            {
                neighbors.Remove(edge.Destination);
            }
        }

        _edges.RemoveAll(edge => edge.Origin == key || edge.Destination == key);
        _vertices.Remove(key);
        _adjacency.Remove(key);

        return removedEdges;
    }

    /// <summary>
    /// Actualiza nombre y/o rol de un usuario. Si un parámetro es null se conserva.
    /// </summary>
    public Vertex UpdateVertex(string id, string? name = null, Role? role = null)
    {
        var vertex = GetVertex(id);
        var updatedName = name ?? vertex.Name;
        var updatedRole = role ?? vertex.Role;
        vertex.Update(updatedName, updatedRole);
        return vertex;
    }

    // ---------------------------------------------------------------------
    // CRUD de aristas
    // ---------------------------------------------------------------------

    /// <summary>Agrega una relación dirigida. Rechaza duplicados y auto-seguimiento.</summary>
    public void AddEdge(string origin, string destination)
    {
        var from = Vertex.NormalizeId(origin);
        var to = Vertex.NormalizeId(destination);

        if (!_vertices.ContainsKey(from))
        {
            throw new KeyNotFoundException($"No existe el usuario de origen '{from}'.");
        }

        if (!_vertices.ContainsKey(to))
        {
            throw new KeyNotFoundException($"No existe el usuario de destino '{to}'.");
        }

        if (from == to)
        {
            throw new InvalidOperationException(
                "Un usuario no puede seguirse a sí mismo (no se permiten bucles).");
        }

        if (_adjacency[from].Contains(to))
        {
            throw new InvalidOperationException($"La relación {from} → {to} ya existe.");
        }

        _adjacency[from].Add(to);
        _edges.Add(new Edge(from, to));
    }

    /// <summary>Elimina una relación dirigida existente.</summary>
    public void RemoveEdge(string origin, string destination)
    {
        var from = Vertex.NormalizeId(origin);
        var to = Vertex.NormalizeId(destination);

        if (!_vertices.ContainsKey(from))
        {
            throw new KeyNotFoundException($"No existe el usuario de origen '{from}'.");
        }

        if (!_vertices.ContainsKey(to))
        {
            throw new KeyNotFoundException($"No existe el usuario de destino '{to}'.");
        }

        if (!_adjacency[from].Contains(to))
        {
            throw new InvalidOperationException($"La relación {from} → {to} no existe.");
        }

        _adjacency[from].Remove(to);
        _edges.RemoveAll(edge => edge.Origin == from && edge.Destination == to);
    }

    // ---------------------------------------------------------------------
    // Métricas de grado
    // ---------------------------------------------------------------------

    public int GetInDegree(string id)
    {
        var key = GetVertex(id).Id;
        return _edges.Count(edge => edge.Destination == key);
    }

    public int GetOutDegree(string id)
    {
        var key = GetVertex(id).Id;
        return _adjacency[key].Count;
    }

    // ---------------------------------------------------------------------
    // Recorridos
    // ---------------------------------------------------------------------

    /// <summary>Recorrido en anchura desde un vértice, con niveles y padres.</summary>
    public BfsResult Bfs(string start)
    {
        var origin = Vertex.NormalizeId(start);
        if (!_vertices.ContainsKey(origin))
        {
            throw new KeyNotFoundException($"No existe un usuario con Id '{origin}'.");
        }

        var visitOrder = new List<string>();
        var levels = new Dictionary<string, int>(StringComparer.Ordinal) { [origin] = 0 };
        var parents = new Dictionary<string, string?>(StringComparer.Ordinal) { [origin] = null };
        var queue = new Queue<string>();
        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visitOrder.Add(current);

            foreach (var neighbor in _adjacency[current].OrderBy(id => id, StringComparer.Ordinal))
            {
                if (levels.ContainsKey(neighbor))
                {
                    continue;
                }

                levels[neighbor] = levels[current] + 1;
                parents[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        return new BfsResult(origin, visitOrder, levels, parents);
    }

    /// <summary>
    /// DFS completo sobre todo el grafo (bosque). Devuelve el orden de
    /// descubrimiento y los ciclos dirigidos encontrados mediante aristas de retroceso.
    /// </summary>
    public DfsResult DfsFull()
    {
        var color = _vertices.Keys.ToDictionary(id => id, _ => VisitColor.White, StringComparer.Ordinal);
        var discoveryOrder = new List<string>();
        var cycles = new List<IReadOnlyList<string>>();
        var seenCycles = new HashSet<string>(StringComparer.Ordinal);
        var stack = new List<string>();

        void Visit(string id)
        {
            color[id] = VisitColor.Gray;
            discoveryOrder.Add(id);
            stack.Add(id);

            foreach (var neighbor in _adjacency[id].OrderBy(x => x, StringComparer.Ordinal))
            {
                if (color[neighbor] == VisitColor.White)
                {
                    Visit(neighbor);
                }
                else if (color[neighbor] == VisitColor.Gray)
                {
                    // Arista de retroceso: se reconstruye el ciclo desde la pila.
                    var startIndex = stack.IndexOf(neighbor);
                    var cycle = stack.Skip(startIndex).ToList();
                    if (seenCycles.Add(CanonicalCycleKey(cycle)))
                    {
                        cycles.Add(cycle);
                    }
                }
            }

            stack.RemoveAt(stack.Count - 1);
            color[id] = VisitColor.Black;
        }

        foreach (var id in _vertices.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            if (color[id] == VisitColor.White)
            {
                Visit(id);
            }
        }

        return new DfsResult(discoveryOrder, cycles);
    }

    /// <summary>Comprueba si existe un camino dirigido entre dos usuarios.</summary>
    public bool CanReach(string origin, string destination, out IReadOnlyList<string> path)
    {
        var from = Vertex.NormalizeId(origin);
        var to = Vertex.NormalizeId(destination);

        if (!_vertices.ContainsKey(from))
        {
            throw new KeyNotFoundException($"No existe el usuario de origen '{from}'.");
        }

        if (!_vertices.ContainsKey(to))
        {
            throw new KeyNotFoundException($"No existe el usuario de destino '{to}'.");
        }

        var bfs = Bfs(from);
        if (!bfs.Levels.ContainsKey(to))
        {
            path = Array.Empty<string>();
            return false;
        }

        var reversePath = new List<string>();
        string? current = to;
        while (current is not null)
        {
            reversePath.Add(current);
            current = bfs.Parents[current];
        }

        reversePath.Reverse();
        path = reversePath;
        return true;
    }

    // ---------------------------------------------------------------------
    // Consultas sociales
    // ---------------------------------------------------------------------

    /// <summary>Usuarios con grado de entrada 0.</summary>
    public IReadOnlyList<Vertex> GetUsersWithoutFollowers()
    {
        var followed = _edges.Select(edge => edge.Destination).ToHashSet(StringComparer.Ordinal);

        return _vertices.Values
            .Where(vertex => !followed.Contains(vertex.Id))
            .OrderBy(vertex => vertex.Id, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Usuarios con el mayor grado de entrada (puede haber empates).</summary>
    public IReadOnlyList<DegreeEntry> GetMostInfluential()
    {
        return GetTopByDegree(vertex => GetInDegree(vertex.Id));
    }

    /// <summary>Usuarios con el mayor grado de salida (puede haber empates).</summary>
    public IReadOnlyList<DegreeEntry> GetMostActive()
    {
        return GetTopByDegree(vertex => GetOutDegree(vertex.Id));
    }

    private IReadOnlyList<DegreeEntry> GetTopByDegree(Func<Vertex, int> degreeSelector)
    {
        if (_vertices.Count == 0)
        {
            return Array.Empty<DegreeEntry>();
        }

        var entries = _vertices.Values
            .Select(vertex => new DegreeEntry(vertex, degreeSelector(vertex)))
            .ToList();

        var maxDegree = entries.Max(entry => entry.Degree);

        return entries
            .Where(entry => entry.Degree == maxDegree)
            .OrderBy(entry => entry.Vertex.Id, StringComparer.Ordinal)
            .ToList();
    }

    // ---------------------------------------------------------------------
    // Conectividad
    // ---------------------------------------------------------------------

    /// <summary>Densidad del grafo dirigido: aristas / (V * (V - 1)).</summary>
    public double GetDensity()
    {
        if (_vertices.Count < 2)
        {
            return 0;
        }

        return (double)_edges.Count / (_vertices.Count * (_vertices.Count - 1));
    }

    /// <summary>Componentes débilmente conexas (se ignoran las direcciones).</summary>
    public IReadOnlyList<IReadOnlyList<string>> GetWeakComponents()
    {
        var undirected = _vertices.Keys.ToDictionary(
            id => id,
            _ => new List<string>(),
            StringComparer.Ordinal);

        foreach (var edge in _edges)
        {
            undirected[edge.Origin].Add(edge.Destination);
            undirected[edge.Destination].Add(edge.Origin);
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var components = new List<IReadOnlyList<string>>();

        foreach (var id in _vertices.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            if (!visited.Add(id))
            {
                continue;
            }

            var component = new List<string>();
            var queue = new Queue<string>();
            queue.Enqueue(id);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                component.Add(current);

                foreach (var neighbor in undirected[current].OrderBy(x => x, StringComparer.Ordinal))
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            component.Sort(StringComparer.Ordinal);
            components.Add(component);
        }

        return components.OrderBy(component => component[0], StringComparer.Ordinal).ToList();
    }

    /// <summary>Componentes fuertemente conexas usando el algoritmo de Kosaraju.</summary>
    public IReadOnlyList<IReadOnlyList<string>> GetStrongComponents()
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var finishOrder = new List<string>();

        void FillFinishOrder(string id)
        {
            visited.Add(id);
            foreach (var neighbor in _adjacency[id].OrderBy(x => x, StringComparer.Ordinal))
            {
                if (!visited.Contains(neighbor))
                {
                    FillFinishOrder(neighbor);
                }
            }

            finishOrder.Add(id);
        }

        foreach (var id in _vertices.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            if (!visited.Contains(id))
            {
                FillFinishOrder(id);
            }
        }

        var reverse = _vertices.Keys.ToDictionary(
            id => id,
            _ => new List<string>(),
            StringComparer.Ordinal);

        foreach (var edge in _edges)
        {
            reverse[edge.Destination].Add(edge.Origin);
        }

        var assigned = new HashSet<string>(StringComparer.Ordinal);
        var components = new List<IReadOnlyList<string>>();

        void CollectComponent(string id, List<string> component)
        {
            assigned.Add(id);
            component.Add(id);

            foreach (var neighbor in reverse[id].OrderBy(x => x, StringComparer.Ordinal))
            {
                if (!assigned.Contains(neighbor))
                {
                    CollectComponent(neighbor, component);
                }
            }
        }

        for (var i = finishOrder.Count - 1; i >= 0; i--)
        {
            var id = finishOrder[i];
            if (assigned.Contains(id))
            {
                continue;
            }

            var component = new List<string>();
            CollectComponent(id, component);
            component.Sort(StringComparer.Ordinal);
            components.Add(component);
        }

        return components.OrderBy(component => component[0], StringComparer.Ordinal).ToList();
    }

    // ---------------------------------------------------------------------
    // Utilidades internas
    // ---------------------------------------------------------------------

    /// <summary>
    /// Normaliza un ciclo para poder deduplicarlo: lo rota para que empiece
    /// por el Id menor y lo une con "->".
    /// </summary>
    private static string CanonicalCycleKey(IReadOnlyList<string> cycle)
    {
        var minIndex = 0;
        for (var i = 1; i < cycle.Count; i++)
        {
            if (string.CompareOrdinal(cycle[i], cycle[minIndex]) < 0)
            {
                minIndex = i;
            }
        }

        var rotated = cycle.Skip(minIndex).Concat(cycle.Take(minIndex));
        return string.Join("->", rotated);
    }
}
