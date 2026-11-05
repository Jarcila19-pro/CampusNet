namespace CampusNet.Models;

/// <summary>Roles disponibles para un usuario de CampusNet.</summary>
public enum Role
{
    Student,
    Professor,
    Graduate
}

public static class RoleExtensions
{
    /// <summary>Nombre legible del rol para mostrar en consola.</summary>
    public static string ToDisplayName(this Role role) => role switch
    {
        Role.Student => "Estudiante",
        Role.Professor => "Profesor",
        Role.Graduate => "Egresado",
        _ => "Desconocido"
    };
}
