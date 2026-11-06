using System.Text;
using CampusNet.Models;

namespace CampusNet.Views;

/// <summary>
/// Vista de consola. Contiene todo el acceso a Console (lectura y escritura),
/// pero ninguna regla de negocio: solo formatea y muestra los datos que recibe.
/// </summary>
public sealed class ConsoleView
{
    private const int SeparatorWidth = 78;

    public ConsoleView()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }

    // ---------------------------------------------------------------------
    // Mensajes generales
    // ---------------------------------------------------------------------

    public void ShowWelcome()
    {
        Console.WriteLine();
        WriteSeparator('═');
        Console.WriteLine("  CAMPUSNET · RED SOCIAL ACADÉMICA DIRIGIDA");
        Console.WriteLine("  Universidad de Manizales · Programación III");
        Console.WriteLine("  Arquitectura MVC · Grafo dirigido con lista de adyacencia");
        WriteSeparator('═');
        Console.WriteLine("  Convención: A → B significa que el usuario A sigue al usuario B.");
    }

    public void ShowGoodbye()
    {
        Console.WriteLine();
        WriteSeparator('═');
        Console.WriteLine("  Fin de la ejecución de CampusNet. ¡Hasta pronto!");
        WriteSeparator('═');
    }

    public void ShowSection(string title)
    {
        Console.WriteLine();
        WriteSeparator('═');
        Console.WriteLine($"  {title}");
        WriteSeparator('═');
    }

    public void ShowSubsection(string title)
    {
        Console.WriteLine();
        WriteSeparator('─');
        Console.WriteLine($"  {title}");
        WriteSeparator('─');
    }

    public void ShowStep(int number, string title)
    {
        Console.WriteLine();
        Console.WriteLine($"  [Paso {number}] {title}");
    }

    public void ShowMessage(string message) => Console.WriteLine($"  {message}");

    public void ShowSuccess(string message) => WriteColored($"  ✔ {message}", ConsoleColor.Green);

    public void ShowError(string message) => WriteColored($"  ✖ {message}", ConsoleColor.Red);

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }

    // ---------------------------------------------------------------------
    // Grafo
    // ---------------------------------------------------------------------

    public void ShowGraphSummary(Graph graph)
    {
        Console.WriteLine(
            $"  Usuarios: {graph.VertexCount} | Relaciones: {graph.EdgeCount} | " +
            $"Densidad: {graph.GetDensity() * 100:F2}%");
    }

    public void ShowAdjacencyList(Graph graph)
    {
        ShowSubsection("Lista de adyacencia");

        foreach (var vertex in graph.Vertices.Values.OrderBy(v => v.Id, StringComparer.Ordinal))
        {
            var neighbors = graph.Adjacency[vertex.Id];

            Console.WriteLine(
                $"  {vertex.Id} | {vertex.Name} [{vertex.Role.ToDisplayName()}] " +
                $"(entrada: {graph.GetInDegree(vertex.Id)}, salida: {neighbors.Count})");

            Console.WriteLine(neighbors.Count == 0
                ? "      → (sin relaciones de salida)"
                : "      → " + string.Join(", ", neighbors
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .Select(id => Describe(graph, id))));
        }

        Console.WriteLine();
        ShowGraphSummary(graph);
    }

    public void ShowUsers(Graph graph)
    {
        ShowSubsection("Usuarios registrados");
        Console.WriteLine($"  {"ID",-5} {"NOMBRE",-26} {"ROL",-12} {"ENTRADA",8} {"SALIDA",7}");

        foreach (var vertex in graph.Vertices.Values.OrderBy(v => v.Id, StringComparer.Ordinal))
        {
            Console.WriteLine(
                $"  {vertex.Id,-5} {Truncate(vertex.Name, 26),-26} {vertex.Role.ToDisplayName(),-12} " +
                $"{graph.GetInDegree(vertex.Id),8} {graph.GetOutDegree(vertex.Id),7}");
        }
    }

    public void ShowRequirementChecklist(
        bool enoughVertices,
        bool enoughEdges,
        bool enoughActiveUsers,
        bool hasCycle,
        bool enoughUsersWithoutFollowers)
    {
        ShowSubsection("Verificación de condiciones mínimas del taller");
        PrintCheck("Al menos 12 usuarios registrados", enoughVertices);
        PrintCheck("Al menos 18 relaciones de seguimiento", enoughEdges);
        PrintCheck("Al menos 2 usuarios con grado de salida ≥ 4", enoughActiveUsers);
        PrintCheck("Al menos 1 ciclo dirigido detectable", hasCycle);
        PrintCheck("Al menos 2 usuarios sin seguidores (grado de entrada 0)", enoughUsersWithoutFollowers);
    }

    public void ShowEdgesOf(Graph graph, string id)
    {
        var outgoing = graph.Edges.Where(edge => edge.Origin == id).ToList();
        var incoming = graph.Edges.Where(edge => edge.Destination == id).ToList();

        Console.WriteLine($"  Relaciones de {id}: salida {outgoing.Count}, entrada {incoming.Count}");
        Console.WriteLine("    Sigue a: " + (outgoing.Count == 0
            ? "(nadie)"
            : string.Join(", ", outgoing.Select(edge => Describe(graph, edge.Destination)))));
        Console.WriteLine("    Seguido por: " + (incoming.Count == 0
            ? "(nadie)"
            : string.Join(", ", incoming.Select(edge => Describe(graph, edge.Origin)))));
    }

    public void ShowRemovedEdges(IReadOnlyList<Edge> edges, Graph graph)
    {
        if (edges.Count == 0)
        {
            Console.WriteLine("  (No se eliminaron relaciones.)");
            return;
        }

        Console.WriteLine("  Relaciones eliminadas:");
        foreach (var edge in edges)
        {
            Console.WriteLine($"    {Describe(graph, edge.Origin)} → {Describe(graph, edge.Destination)}");
        }
    }

    // ---------------------------------------------------------------------
    // Recorridos
    // ---------------------------------------------------------------------

    public void ShowBfs(BfsResult result, Graph graph)
    {
        ShowSubsection($"BFS desde {Describe(graph, result.Start)}");
        Console.WriteLine("  Orden de visita:");

        for (var i = 0; i < result.VisitOrder.Count; i++)
        {
            var id = result.VisitOrder[i];
            Console.WriteLine($"    {i + 1,2}. {Describe(graph, id)} (nivel {result.Levels[id]})");
        }

        Console.WriteLine($"  Vértices alcanzados: {result.ReachedCount} de {graph.VertexCount}.");

        var unreached = graph.Vertices.Keys
            .Where(id => !result.Levels.ContainsKey(id))
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        Console.WriteLine(unreached.Count == 0
            ? "  No hay vértices inalcanzables desde este origen."
            : "  No alcanzados: " + string.Join(", ", unreached) + ".");
    }

    public void ShowDfs(DfsResult result, Graph graph)
    {
        ShowSubsection("DFS completo · orden de descubrimiento");
        Console.WriteLine("  " + string.Join(" → ", result.DiscoveryOrder));
        Console.WriteLine($"  Vértices descubiertos: {result.DiscoveryOrder.Count} de {graph.VertexCount}.");

        ShowSubsection("Ciclos dirigidos detectados por DFS");

        if (result.Cycles.Count == 0)
        {
            Console.WriteLine("  No se detectaron ciclos dirigidos.");
            return;
        }

        foreach (var cycle in result.Cycles)
        {
            Console.WriteLine($"  • {string.Join(" → ", cycle)} → {cycle[0]}");
        }

        Console.WriteLine($"  Total de ciclos detectados: {result.Cycles.Count}.");
    }

    // ---------------------------------------------------------------------
    // Consultas sociales
    // ---------------------------------------------------------------------

    public void ShowUsersWithoutFollowers(IReadOnlyList<Vertex> users, Graph graph)
    {
        ShowSubsection("Usuarios sin seguidores (grado de entrada = 0)");

        if (users.Count == 0)
        {
            Console.WriteLine("  Todos los usuarios tienen al menos un seguidor.");
            return;
        }

        foreach (var user in users)
        {
            Console.WriteLine(
                $"  • {user.Id} | {user.Name} | {user.Role.ToDisplayName()} | " +
                $"salida: {graph.GetOutDegree(user.Id)}");
        }

        Console.WriteLine($"  Total: {users.Count}.");
    }

    public void ShowUsersByRole(Graph graph, Role role)
    {
        ShowSubsection($"Usuarios con rol {role.ToDisplayName()}");

        var users = graph.Vertices.Values
            .Where(vertex => vertex.Role == role)
            .OrderBy(vertex => vertex.Id, StringComparer.Ordinal)
            .ToList();

        if (users.Count == 0)
        {
            Console.WriteLine("  No hay usuarios con este rol.");
            return;
        }

        foreach (var user in users)
        {
            Console.WriteLine(
                $"  • {user.Id} | {user.Name} | entrada: {graph.GetInDegree(user.Id)}, " +
                $"salida: {graph.GetOutDegree(user.Id)}");
        }

        Console.WriteLine($"  Total: {users.Count}.");
    }

    public void ShowDegreeRanking(string title, IReadOnlyList<DegreeEntry> entries, string degreeLabel)
    {
        ShowSubsection(title);

        if (entries.Count == 0)
        {
            Console.WriteLine("  No hay usuarios registrados.");
            return;
        }

        foreach (var entry in entries)
        {
            Console.WriteLine(
                $"  • {entry.Vertex.Id} | {entry.Vertex.Name} | " +
                $"{entry.Vertex.Role.ToDisplayName()} | {degreeLabel}: {entry.Degree}");
        }

        if (entries.Count > 1)
        {
            Console.WriteLine("  (Empate entre los usuarios mostrados).");
        }
    }

    public void ShowReachability(
        string origin,
        string destination,
        bool reachable,
        IReadOnlyList<string> path,
        Graph graph)
    {
        ShowSubsection($"Alcanzabilidad: ¿{origin} puede llegar a {destination}?");

        if (!reachable)
        {
            Console.WriteLine(
                $"  Resultado: NO. No existe un camino dirigido de " +
                $"{Describe(graph, origin)} a {Describe(graph, destination)}.");
            return;
        }

        Console.WriteLine("  Resultado: SÍ. Camino más corto encontrado con BFS:");
        Console.WriteLine("    " + string.Join(" → ", path.Select(id => Describe(graph, id))));
        Console.WriteLine($"  Longitud: {path.Count - 1} arista(s).");
    }

    // ---------------------------------------------------------------------
    // Conectividad
    // ---------------------------------------------------------------------

    public void ShowConnectivity(
        Graph graph,
        IReadOnlyList<IReadOnlyList<string>> weakComponents,
        IReadOnlyList<IReadOnlyList<string>> strongComponents)
    {
        ShowSubsection("Resumen estructural");
        Console.WriteLine($"  Vértices: {graph.VertexCount} | Aristas: {graph.EdgeCount}" +
                          $" | Densidad: {graph.GetDensity() * 100:F2}%");

        ShowSubsection($"Componentes débilmente conexas: {weakComponents.Count}");
        foreach (var component in weakComponents)
        {
            Console.WriteLine($"  • {component.Count} usuario(s): {string.Join(", ", component)}");
        }

        ShowSubsection($"Componentes fuertemente conexas: {strongComponents.Count}");
        foreach (var component in strongComponents)
        {
            var label = component.Count == 1 ? "componente trivial" : "grupo";
            Console.WriteLine($"  • {component.Count} usuario(s) [{label}]: {string.Join(", ", component)}");
        }
    }

    public void ShowUnreachable(string origin, IReadOnlyList<string> unreachable, Graph graph)
    {
        if (unreachable.Count == 0)
        {
            Console.WriteLine($"  Desde {origin} se alcanzan todos los usuarios.");
            return;
        }

        Console.WriteLine(
            $"  Usuarios NO alcanzables desde {Describe(graph, origin)}: " +
            string.Join(", ", unreachable) + ".");
    }

    // ---------------------------------------------------------------------
    // Menú interactivo
    // ---------------------------------------------------------------------

    public void ShowMenu()
    {
        Console.WriteLine();
        WriteSeparator('─');
        Console.WriteLine("  MENÚ INTERACTIVO");
        WriteSeparator('─');
        Console.WriteLine("   1. Ver lista de adyacencia");
        Console.WriteLine("   2. Ver usuarios registrados");
        Console.WriteLine("   3. Agregar usuario");
        Console.WriteLine("   4. Eliminar usuario");
        Console.WriteLine("   5. Actualizar nombre o rol");
        Console.WriteLine("   6. Agregar relación (seguir)");
        Console.WriteLine("   7. Eliminar relación (dejar de seguir)");
        Console.WriteLine("   8. Ejecutar BFS desde un usuario");
        Console.WriteLine("   9. Ejecutar DFS completo");
        Console.WriteLine("  10. Consultas sociales");
        Console.WriteLine("  11. Verificar alcanzabilidad");
        Console.WriteLine("  12. Análisis de conectividad");
        Console.WriteLine("  13. Ver usuarios por rol");
        Console.WriteLine("   0. Salir");
    }

    public int ReadMenuOption()
    {
        while (true)
        {
            Console.Write("  Seleccione una opción: ");
            var input = Console.ReadLine();

            if (input is null)
            {
                return 0;
            }

            if (int.TryParse(input.Trim(), out var option) && option is >= 0 and <= 13)
            {
                return option;
            }

            ShowError("Opción inválida. Ingrese un número entre 0 y 13.");
        }
    }

    public string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}");
            var input = Console.ReadLine();

            if (input is null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            ShowError("El valor no puede estar vacío.");
        }
    }

    public string? ReadOptionalText(string prompt)
    {
        Console.Write($"  {prompt}");
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    public Role? ReadRole(bool allowKeep = false)
    {
        while (true)
        {
            Console.WriteLine(allowKeep
                ? "  Rol: 0 = conservar | 1 = Estudiante | 2 = Profesor | 3 = Egresado"
                : "  Rol: 1 = Estudiante | 2 = Profesor | 3 = Egresado");
            Console.Write("  Seleccione el rol: ");

            var input = Console.ReadLine();
            if (input is null)
            {
                return allowKeep ? null : Role.Student;
            }

            switch (input.Trim())
            {
                case "0" when allowKeep:
                    return null;
                case "1":
                    return Role.Student;
                case "2":
                    return Role.Professor;
                case "3":
                    return Role.Graduate;
                default:
                    ShowError("Rol inválido.");
                    break;
            }
        }
    }

    // ---------------------------------------------------------------------
    // Utilidades de formato
    // ---------------------------------------------------------------------

    private void WriteSeparator(char character)
    {
        Console.WriteLine(new string(character, SeparatorWidth));
    }

    private static void PrintCheck(string label, bool passed)
    {
        Console.WriteLine($"  {(passed ? "✔" : "✖")} {label}");
    }

    private static string Describe(Graph graph, string id)
    {
        return graph.Vertices.TryGetValue(id, out var vertex)
            ? $"{id} ({vertex.Name})"
            : id;
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
    }
}
