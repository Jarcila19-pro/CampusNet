# CampusNet · Red social académica dirigida (MVC en C#)

Módulo de consola en C# que modela una red social académica dirigida. Cada vértice
representa un usuario (id, nombre y rol) y cada arista `A → B` significa que A sigue
a B; la relación no es recíproca.

Taller Integrador de Programación III · Universidad de Manizales.

## Integrantes y responsabilidades

| Integrante | GitHub | Responsabilidad |
|---|---|---|
| Juan Esteban Arcila Espitia | [@Jarcila19-pro](https://github.com/Jarcila19-pro) | Modelo: `Vertex`, `Edge`, `Graph`, BFS/DFS, ciclos, métricas y conectividad. |
| Juan Alejandro Cardona | [@alejandrocardo1001](https://github.com/alejandrocardo1001) | Vista: `ConsoleView`, impresión en consola y menú. |
| Juan Martin Lopez | [@tincito1](https://github.com/tincito1) | Controlador: casos de uso, datos semilla, CRUD y manejo de errores. |

Trabajo desarrollado de forma grupal y en llamada, como la primera tarea. Los commits
de los tres integrantes son verificables en el historial del repositorio.

## Arquitectura MVC

```text
Program.cs              → solo invoca al Controller
Models/                 → Vertex, Edge, Graph (lista de adyacencia) y resultados
Views/ConsoleView.cs    → toda la entrada y salida de consola
Controllers/            → casos de uso y comunicación Model ↔ View
```

El modelo no usa `Console`, la vista no contiene reglas de negocio y `Program.cs` no
tiene lógica: solo crea la vista, el controlador y ejecuta `Run()`.

## Ejecución

Requisitos: **.NET SDK 8.0 o superior**. No requiere paquetes externos.

```bash
git clone https://github.com/Jarcila19-pro/CampusNet.git
cd CampusNet
dotnet run
```

Al iniciar se ejecutan automáticamente los cuatro casos de uso y el análisis final;
después se abre un menú interactivo. La opción `0` sale del programa.

## Evidencia de ejecución

Salida completa: [`docs/evidencia.txt`](docs/evidencia.txt).

Datos semilla: **14 usuarios** y **23 relaciones** (mínimo exigido: 12 y 18), con
`u01` y `u02` de grado de salida 5 y 4, el ciclo `u01 → u02 → u03 → u01` y los
usuarios `u13` y `u14` sin seguidores.

Lista de adyacencia (fragmento):

```text
u01 | Ana Torres [Estudiante] (entrada: 3, salida: 5)
    → u02 (Bruno Díaz), u03 (Carla Ruiz), u04 (Diego Mesa), u05 (Elena Vidal), u06 (Fabián Ospina)
u13 | Mateo Gil [Estudiante] (entrada: 0, salida: 1)
    → u07 (Gabriela Núñez)
u14 | Natalia Vega [Egresado] (entrada: 0, salida: 1)
    → u08 (Hugo Salazar)
Usuarios: 14 | Relaciones: 23 | Densidad: 12,64%
```

Recorridos:

| BFS desde | Alcanzados | No alcanzados |
|---|---|---|
| u01 | 12 de 14 | u13, u14 |
| u05 | 12 de 14 | u13, u14 |
| u13 | 13 de 14 | u14 |

DFS completo (orden de descubrimiento):

```text
u01 → u02 → u03 → u06 → u08 → u10 → u12 → u04 → u07 → u09 → u11 → u05 → u13 → u14
```

Ciclos detectados:

```text
u01 → u02 → u03 → u01
u01 → u02 → u03 → u06 → u01
u03 → u06 → u08 → u10 → u12 → u03
u01 → u02 → u04 → u07 → u09 → u11 → u01
```

Consultas y CRUD (extracto):

```text
Usuarios sin seguidores: u13 (Mateo Gil), u14 (Natalia Vega)
Influyentes (entrada = 3): u01, u03, u06, u07
Más activo (salida = 5): u01
¿u01 puede llegar a u12? SÍ → u01 → u06 → u08 → u10 → u12
¿u01 puede llegar a u14? NO

[CRUD] Agregar usuario u15: ✔
[CRUD] Agregar u15 duplicado: ✖ Ya existe un usuario con Id 'u15'
[CRUD] Actualizar u15: Sofía Herrera [Estudiante] → Sofía Herrera Gómez [Egresado]
[CRUD] Agregar relación u15 → u01: ✔
[CRUD] Repetir u15 → u01: ✖ La relación ya existe
[CRUD] Eliminar relación u01 → u15: ✔
[CRUD] Eliminar usuario u15: ✔ (elimina u15 → u01)
```

## Análisis final

- **Débilmente conexo, no fuertemente conexo:** hay una sola componente débil, pero
  tres componentes fuertes: `u01…u12`, `u13` y `u14`.
- **u13 y u14 no son alcanzables desde u01:** nadie los sigue (grado de entrada 0),
  aunque ellos sí pueden llegar a la comunidad principal (`u13→u07`, `u14→u08`).
- **u01 es el más activo** (salida 5) y alcanza 12 de 14 usuarios; no llega a u13 ni
  a u14 porque las aristas van en sentido contrario.
- **Influyentes y ciclos:** u01, u03, u06 y u07 empatan con 3 seguidores. Los cuatro
  ciclos detectados explican que `u01…u12` sea una componente fuertemente conexa.
- **Densidad 12,64%:** 23 de 182 aristas posibles; red dispersa con actividad
  concentrada en u01 y u02.
