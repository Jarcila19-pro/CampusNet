namespace CampusNet.Models;

/// <summary>
/// Vértice del grafo: representa un usuario académico.
/// El Id es inmutable porque identifica al vértice dentro del grafo.
/// </summary>
public sealed class Vertex
{
    public string Id { get; }
    public string Name { get; private set; }
    public Role Role { get; private set; }

    public Vertex(string id, string name, Role role)
    {
        Id = NormalizeId(id);
        Name = NormalizeName(name);
        Role = ValidateRole(role);
    }

    /// <summary>Actualiza los datos editables del usuario (nombre y rol).</summary>
    public void Update(string name, Role role)
    {
        Name = NormalizeName(name);
        Role = ValidateRole(role);
    }

    public static string NormalizeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("El Id del usuario no puede estar vacío.");
        }

        return id.Trim();
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del usuario no puede estar vacío.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length > 60)
        {
            throw new ArgumentException("El nombre del usuario no puede superar los 60 caracteres.");
        }

        return trimmed;
    }

    private static Role ValidateRole(Role role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentException("El rol indicado no es válido.");
        }

        return role;
    }
}
