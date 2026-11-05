using CampusNet.Controllers;
using CampusNet.Views;

namespace CampusNet;

/// <summary>
/// Punto de entrada. No contiene lógica de negocio ni acceso a consola:
/// únicamente conecta la vista con el controlador y lo ejecuta.
/// </summary>
public static class Program
{
    public static void Main()
    {
        var view = new ConsoleView();
        var controller = new CampusNetController(view);
        controller.Run();
    }
}
