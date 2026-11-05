using CampusNet.Models;
using CampusNet.Views;

namespace CampusNet.Controllers;

/// <summary>
/// Controlador: orquesta los casos de uso, comunica el modelo con la vista
/// y captura los errores de dominio para presentarlos al usuario.
/// No imprime en consola directamente ni implementa estructuras de datos.
/// </summary>
public sealed class CampusNetController
{
    private readonly Graph _graph;
    private readonly ConsoleView _view;

    public CampusNetController(ConsoleView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _graph = new Graph();
    }

    public void Run()
    {
        _view.ShowWelcome();
        LoadSeedData();

        DemonstrateGraphConstruction();
        DemonstrateTraversals();
        DemonstrateSocialQueries();
        DemonstrateCrud();

        ShowConnectivityAnalysis();
        RunMenu();
        _view.ShowGoodbye();
    }

    // ---------------------------------------------------------------------
    // Datos semilla
    // ---------------------------------------------------------------------

    private void LoadSeedData()
    {
        var users = new (string Id, string Name, Role UserRole)[]
        {
            ("u01", "Ana Torres", Role.Student),
            ("u02", "Bruno Díaz", Role.Student),
            ("u03", "Carla Ruiz", Role.Professor),
            ("u04", "Diego Mesa", Role.Professor),
            ("u05", "Elena Vidal", Role.Graduate),
            ("u06", "Fabián Ospina", Role.Student),
            ("u07", "Gabriela Núñez", Role.Graduate),
            ("u08", "Hugo Salazar", Role.Professor),
            ("u09", "Irene Castro", Role.Student),
            ("u10", "Julián Ríos", Role.Graduate),
            ("u11", "Karina López", Role.Student),
            ("u12", "Luis Pardo", Role.Professor),
            ("u13", "Mateo Gil", Role.Student),
            ("u14", "Natalia Vega", Role.Graduate)
        };

        foreach (var user in users)
        {
            _graph.AddVertex(new Vertex(user.Id, user.Name, user.UserRole));
        }

        var edges = new (string Origin, string Destination)[]
        {
            ("u01", "u02"),
            ("u01", "u03"),
            ("u01", "u04"),
            ("u01", "u05"),
            ("u01", "u06"),
            ("u02", "u03"),
            ("u02", "u04"),
            ("u02", "u05"),
            ("u02", "u06"),
            ("u03", "u01"),
            ("u03", "u06"),
            ("u04", "u07"),
            ("u05", "u07"),
            ("u06", "u08"),
            ("u06", "u01"),
            ("u07", "u09"),
            ("u08", "u10"),
            ("u09", "u11"),
            ("u10", "u12"),
            ("u11", "u01"),
            ("u12", "u03"),
            ("u13", "u07"),
            ("u14", "u08")
        };

        foreach (var edge in edges)
        {
            _graph.AddEdge(edge.Origin, edge.Destination);
        }
    }

    // ---------------------------------------------------------------------
    // Caso 1: construcción del grafo
    // ---------------------------------------------------------------------

    private void DemonstrateGraphConstruction()
    {
        _view.ShowSection("CASO 1 · CONSTRUCCIÓN DEL GRAFO");
        _view.ShowMessage("Se creó un grafo dirigido vacío y se cargaron 14 usuarios y 23 relaciones semilla.");
        _view.ShowGraphSummary(_graph);
        _view.ShowAdjacencyList(_graph);

        var usersWithHighOutDegree = _graph.Vertices.Keys.Count(id => _graph.GetOutDegree(id) >= 4);
        var dfs = _graph.DfsFull();

        _view.ShowRequirementChecklist(
            enoughVertices: _graph.VertexCount >= 12,
            enoughEdges: _graph.EdgeCount >= 18,
            enoughActiveUsers: usersWithHighOutDegree >= 2,
            hasCycle: dfs.Cycles.Count > 0,
            enoughUsersWithoutFollowers: _graph.GetUsersWithoutFollowers().Count >= 2);
    }

    // ---------------------------------------------------------------------
    // Caso 2: recorridos
    // ---------------------------------------------------------------------

    private void DemonstrateTraversals()
    {
        _view.ShowSection("CASO 2 · RECORRIDOS");
        _view.ShowMessage("Se ejecuta BFS desde tres usuarios distintos (u01, u05 y u13).");

        foreach (var start in new[] { "u01", "u05", "u13" })
        {
            _view.ShowBfs(_graph.Bfs(start), _graph);
        }

        _view.ShowMessage("Se ejecuta ahora un DFS completo sobre todo el grafo.");
        _view.ShowDfs(_graph.DfsFull(), _graph);
    }

    // ---------------------------------------------------------------------
    // Caso 3: consultas sociales
    // ---------------------------------------------------------------------

    private void DemonstrateSocialQueries()
    {
        _view.ShowSection("CASO 3 · CONSULTAS SOCIALES");

        ShowSocialQueries();

        var reachableToU12 = _graph.CanReach("u01", "u12", out var pathToU12);
        _view.ShowReachability("u01", "u12", reachableToU12, pathToU12, _graph);

        var reachableToU14 = _graph.CanReach("u01", "u14", out var pathToU14);
        _view.ShowReachability("u01", "u14", reachableToU14, pathToU14, _graph);
    }

    private void ShowSocialQueries()
    {
        _view.ShowUsersWithoutFollowers(_graph.GetUsersWithoutFollowers(), _graph);
        _view.ShowDegreeRanking(
            "Usuarios influyentes (mayor grado de entrada)",
            _graph.GetMostInfluential(),
            "entrada");
        _view.ShowDegreeRanking(
            "Usuarios más activos (mayor grado de salida)",
            _graph.GetMostActive(),
            "salida");
    }

    // ---------------------------------------------------------------------
    // Caso 4: operaciones CRUD
    // ---------------------------------------------------------------------

    private void DemonstrateCrud()
    {
        const string newId = "u15";

        _view.ShowSection("CASO 4 · OPERACIONES CRUD");
        _view.ShowMessage("Cada operación se valida en el modelo y se reporta el cambio en consola.");

        _view.ShowStep(1, "Agregar usuario u15");
        ExecuteSafely(() =>
        {
            _graph.AddVertex(new Vertex(newId, "Sofía Herrera", Role.Student));
            _view.ShowSuccess("Usuario agregado correctamente.");
            _view.ShowEdgesOf(_graph, newId);
            _view.ShowGraphSummary(_graph);
        });

        _view.ShowStep(2, "Intentar agregar un usuario duplicado (u15)");
        ExecuteSafely(() =>
        {
            _graph.AddVertex(new Vertex(newId, "Sofía Duplicada", Role.Student));
            _view.ShowSuccess("Esto no debería mostrarse: el modelo debía rechazar el duplicado.");
        });
        _view.ShowMessage("El modelo rechazó el duplicado; la lista de usuarios no cambió.");

        _view.ShowStep(3, "Actualizar nombre y rol de u15");
        ExecuteSafely(() =>
        {
            var before = _graph.GetVertex(newId);
            _view.ShowMessage($"Antes:  {before.Name} [{before.Role.ToDisplayName()}]");

            _graph.UpdateVertex(newId, "Sofía Herrera Gómez", Role.Graduate);

            var after = _graph.GetVertex(newId);
            _view.ShowSuccess($"Después: {after.Name} [{after.Role.ToDisplayName()}]");
        });

        _view.ShowStep(4, "Agregar relación u15 → u01");
        ExecuteSafely(() =>
        {
            _graph.AddEdge(newId, "u01");
            _view.ShowSuccess("Relación agregada correctamente.");
            _view.ShowEdgesOf(_graph, newId);
            _view.ShowGraphSummary(_graph);
        });

        _view.ShowStep(5, "Intentar agregar la relación duplicada u15 → u01");
        ExecuteSafely(() =>
        {
            _graph.AddEdge(newId, "u01");
            _view.ShowSuccess("Esto no debería mostrarse: el modelo debía rechazar la relación duplicada.");
        });
        _view.ShowMessage("El modelo rechazó la relación duplicada; el grafo no cambió.");

        _view.ShowStep(6, "Agregar relación inversa u01 → u15");
        ExecuteSafely(() =>
        {
            _graph.AddEdge("u01", newId);
            _view.ShowSuccess("Relación agregada correctamente.");
            _view.ShowEdgesOf(_graph, "u01");
        });

        _view.ShowStep(7, "Eliminar relación u01 → u15");
        ExecuteSafely(() =>
        {
            _graph.RemoveEdge("u01", newId);
            _view.ShowSuccess("Relación eliminada correctamente.");
            _view.ShowEdgesOf(_graph, "u01");
            _view.ShowGraphSummary(_graph);
        });

        _view.ShowStep(8, "Eliminar usuario u15 (y sus relaciones incidentes)");
        ExecuteSafely(() =>
        {
            var removedVertex = _graph.GetVertex(newId);
            var removedEdges = _graph.RemoveVertex(newId);
            _view.ShowSuccess($"Usuario {removedVertex.Id} ({removedVertex.Name}) eliminado.");
            _view.ShowRemovedEdges(removedEdges, _graph);
            _view.ShowGraphSummary(_graph);
        });

        _view.ShowStep(9, "Intentar eliminar un usuario inexistente (u99)");
        ExecuteSafely(() => _graph.RemoveVertex("u99"));
    }

    // ---------------------------------------------------------------------
    // Análisis final
    // ---------------------------------------------------------------------

    private void ShowConnectivityAnalysis()
    {
        _view.ShowSection("ANÁLISIS FINAL DE CONECTIVIDAD");

        var weakComponents = _graph.GetWeakComponents();
        var strongComponents = _graph.GetStrongComponents();
        _view.ShowConnectivity(_graph, weakComponents, strongComponents);

        var bfsFromU01 = _graph.Bfs("u01");
        var unreachableFromU01 = _graph.Vertices.Keys
            .Where(id => !bfsFromU01.Levels.ContainsKey(id))
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        _view.ShowUnreachable("u01", unreachableFromU01, _graph);

        _view.ShowSubsection("Interpretación");
        _view.ShowMessage(weakComponents.Count == 1
            ? "El grafo es débilmente conexo: todos los usuarios están conectados si se ignoran las direcciones."
            : $"El grafo tiene {weakComponents.Count} componentes débilmente conexas: no todos los usuarios están conectados.");

        _view.ShowMessage(strongComponents.Count == 1
            ? "El grafo es fuertemente conexo: todos los usuarios pueden alcanzarse entre sí."
            : $"El grafo no es fuertemente conexo ({strongComponents.Count} componentes): " +
              "existen usuarios que no pueden alcanzarse mutuamente.");

        if (unreachableFromU01.Count > 0)
        {
            _view.ShowMessage(
                $"Aunque u01 es el usuario más activo, no puede llegar a {string.Join(", ", unreachableFromU01)}: " +
                "tienen relaciones salientes, pero nadie los sigue.");
        }
    }

    // ---------------------------------------------------------------------
    // Menú interactivo
    // ---------------------------------------------------------------------

    private void RunMenu()
    {
        while (true)
        {
            _view.ShowMenu();
            var option = _view.ReadMenuOption();

            if (option == 0)
            {
                return;
            }

            switch (option)
            {
                case 1:
                    _view.ShowAdjacencyList(_graph);
                    break;

                case 2:
                    _view.ShowUsers(_graph);
                    break;

                case 3:
                    AddUserInteractive();
                    break;

                case 4:
                    RemoveUserInteractive();
                    break;

                case 5:
                    UpdateUserInteractive();
                    break;

                case 6:
                    AddEdgeInteractive();
                    break;

                case 7:
                    RemoveEdgeInteractive();
                    break;

                case 8:
                    RunBfsInteractive();
                    break;

                case 9:
                    _view.ShowDfs(_graph.DfsFull(), _graph);
                    break;

                case 10:
                    ShowSocialQueries();
                    break;

                case 11:
                    CheckReachabilityInteractive();
                    break;

                case 12:
                    ShowConnectivityAnalysis();
                    break;
            }
        }
    }

    private void AddUserInteractive()
    {
        var id = _view.ReadRequiredText("Id del nuevo usuario: ");
        var name = _view.ReadRequiredText("Nombre completo: ");
        var role = _view.ReadRole() ?? Role.Student;

        ExecuteSafely(() =>
        {
            _graph.AddVertex(new Vertex(id, name, role));
            _view.ShowSuccess($"Usuario {id} agregado correctamente.");
            _view.ShowEdgesOf(_graph, id);
            _view.ShowGraphSummary(_graph);
        });
    }

    private void RemoveUserInteractive()
    {
        var id = _view.ReadRequiredText("Id del usuario a eliminar: ");

        ExecuteSafely(() =>
        {
            var vertex = _graph.GetVertex(id);
            var removedEdges = _graph.RemoveVertex(id);
            _view.ShowSuccess($"Usuario {vertex.Id} ({vertex.Name}) eliminado.");
            _view.ShowRemovedEdges(removedEdges, _graph);
            _view.ShowGraphSummary(_graph);
        });
    }

    private void UpdateUserInteractive()
    {
        var id = _view.ReadRequiredText("Id del usuario a actualizar: ");

        ExecuteSafely(() =>
        {
            var vertex = _graph.GetVertex(id);
            _view.ShowMessage($"Datos actuales: {vertex.Name} [{vertex.Role.ToDisplayName()}]");

            var newName = _view.ReadOptionalText("Nuevo nombre (Enter para conservar): ");
            var newRole = _view.ReadRole(allowKeep: true);

            _graph.UpdateVertex(id, newName, newRole);
            var updated = _graph.GetVertex(id);
            _view.ShowSuccess($"Usuario actualizado: {updated.Name} [{updated.Role.ToDisplayName()}]");
        });
    }

    private void AddEdgeInteractive()
    {
        var origin = _view.ReadRequiredText("Id del usuario que sigue: ");
        var destination = _view.ReadRequiredText("Id del usuario seguido: ");

        ExecuteSafely(() =>
        {
            _graph.AddEdge(origin, destination);
            _view.ShowSuccess($"Relación agregada: {origin} → {destination}.");
            _view.ShowEdgesOf(_graph, origin);
            _view.ShowGraphSummary(_graph);
        });
    }

    private void RemoveEdgeInteractive()
    {
        var origin = _view.ReadRequiredText("Id del usuario que sigue: ");
        var destination = _view.ReadRequiredText("Id del usuario seguido: ");

        ExecuteSafely(() =>
        {
            _graph.RemoveEdge(origin, destination);
            _view.ShowSuccess($"Relación eliminada: {origin} → {destination}.");
            _view.ShowEdgesOf(_graph, origin);
            _view.ShowGraphSummary(_graph);
        });
    }

    private void RunBfsInteractive()
    {
        var start = _view.ReadRequiredText("Id del usuario inicial: ");
        ExecuteSafely(() => _view.ShowBfs(_graph.Bfs(start), _graph));
    }

    private void CheckReachabilityInteractive()
    {
        var origin = _view.ReadRequiredText("Id del usuario de origen: ");
        var destination = _view.ReadRequiredText("Id del usuario de destino: ");

        ExecuteSafely(() =>
        {
            var reachable = _graph.CanReach(origin, destination, out var path);
            _view.ShowReachability(origin, destination, reachable, path, _graph);
        });
    }

    // ---------------------------------------------------------------------
    // Manejo de errores de dominio
    // ---------------------------------------------------------------------

    private void ExecuteSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            _view.ShowError(ex.Message);
        }
    }
}
