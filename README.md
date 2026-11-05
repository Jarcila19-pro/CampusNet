# CampusNet · Red social académica dirigida (MVC en C#)

Módulo de consola que modela una red social dirigida de usuarios académicos. Cada
vértice representa un usuario y cada arista dirigida `A → B` significa que **A sigue a B**.
La relación no es recíproca: que A siga a B no implica que B siga a A.

El proyecto implementa un **grafo dirigido con lista de adyacencia**, recorridos BFS/DFS,
detección de ciclos, métricas de influencia y conectividad, además de un CRUD completo,
bajo una arquitectura **MVC estricta**.

---

## Integrantes y responsabilidades

| Integrante | Responsabilidad principal |
|---|---|
| **1. _(completar nombre completo)_** | **Modelo:** clases `Vertex`, `Edge` y `Graph`, lista de adyacencia, validación de duplicados, BFS, DFS, ciclos, grados y componentes conexas. |
| **2. _(completar nombre completo)_** | **Vista:** `ConsoleView`, formato de la lista de adyacencia, resultados de recorridos, consultas, operaciones CRUD y menú interactivo. |
| **3. _(completar nombre completo)_** | **Controlador:** `CampusNetController`, casos de uso, datos semilla, manejo de errores de dominio, análisis final y documentación. |

> Reemplazar los nombres por los datos reales del equipo antes de la entrega.

---

## Arquitectura MVC

```
CampusNet.csproj
Program.cs                        → solo conecta la vista con el controlador
Models/
  Role.cs                         → enum de roles y nombre legible
  Vertex.cs                       → usuario (Id único, nombre, rol)
  Edge.cs                         → arista dirigida (origen, destino)
  Graph.cs                        → lista de adyacencia y toda la lógica de negocio
  TraversalResults.cs             → resultados de BFS, DFS y rankings
Views/
  ConsoleView.cs                  → todo el acceso a Console (lectura y escritura)
Controllers/
  CampusNetController.cs          → casos de uso y comunicación Model ↔ View
```

Responsabilidades respetadas:

- **Model:** no imprime en consola. Valida la integridad del grafo (Ids únicos,
  aristas duplicadas, vértices inexistentes, auto-seguimiento).
- **View:** no contiene reglas de negocio. Solo lee datos y presenta resultados.
- **Controller:** no imprime ni manipula estructuras de datos; decide el orden de los
  casos de uso y captura los errores del modelo para mostrarlos.
- **Program.cs:** únicamente crea la vista, crea el controlador y ejecuta `Run()`.

---

## Requisitos y ejecución

- **.NET SDK 8.0** (o superior).
- No requiere paquetes NuGet ni configuración adicional.

```bash
git clone https://github.com/Jarcila19-pro/CampusNet.git
cd CampusNet

# Desde la raíz del repositorio
dotnet build
dotnet run
```

Al ejecutar, el programa:

1. Carga automáticamente los datos semilla y demuestra los cuatro casos de uso.
2. Imprime el análisis final de conectividad.
3. Abre un **menú interactivo** para probar el CRUD manualmente.
4. Se cierra con la opción `0`.

La salida completa de una ejecución real está en
[`docs/evidencia.txt`](docs/evidencia.txt).

---

## Modelo del grafo

- **Tipo:** dirigido.
- **Identificador:** `string` único (`u01`, `u02`, …).
- **Estructura:** `Dictionary<string, List<string>>` para la lista de adyacencia y
  `Dictionary<string, Vertex>` para los datos de cada usuario.
- **Duplicados:** prohibidos tanto en vértices como en aristas; el modelo lanza una
  excepción de dominio y el controlador la reporta.
- **Auto-seguimiento:** prohibido (`A → A`).
- **Eliminación de usuarios:** elimina en cascada todas sus aristas entrantes y salientes.
- **Vecinos ordenados:** las salidas son deterministas, lo que facilita la verificación.

---

## Datos semilla

14 usuarios con roles variados:

| Id | Nombre | Rol |
|---|---|---|
| u01 | Ana Torres | Estudiante |
| u02 | Bruno Díaz | Estudiante |
| u03 | Carla Ruiz | Profesor |
| u04 | Diego Mesa | Profesor |
| u05 | Elena Vidal | Egresado |
| u06 | Fabián Ospina | Estudiante |
| u07 | Gabriela Núñez | Egresado |
| u08 | Hugo Salazar | Profesor |
| u09 | Irene Castro | Estudiante |
| u10 | Julián Ríos | Egresado |
| u11 | Karina López | Estudiante |
| u12 | Luis Pardo | Profesor |
| u13 | Mateo Gil | Estudiante |
| u14 | Natalia Vega | Egresado |

23 relaciones de seguimiento:

```text
u01→u02  u01→u03  u01→u04  u01→u05  u01→u06
u02→u03  u02→u04  u02→u05  u02→u06
u03→u01  u03→u06
u04→u07  u05→u07
u06→u08  u06→u01
u07→u09  u08→u10  u09→u11  u10→u12  u11→u01  u12→u03
u13→u07  u14→u08
```

Condiciones exigidas por el taller:

| Condición | Cumplimiento |
|---|---|
| ≥ 12 usuarios | **14** |
| ≥ 18 relaciones | **23** |
| ≥ 2 usuarios con grado de salida ≥ 4 | **u01 = 5**, **u02 = 4** |
| ≥ 1 ciclo dirigido | `u01 → u02 → u03 → u01` (y otros) |
| ≥ 2 usuarios sin seguidores | **u13** y **u14** (grado de entrada 0) |

---

## Casos de uso implementados

### 1. Construcción del grafo

- Grafo dirigido vacío creado por el controlador.
- Carga de 14 usuarios y 23 relaciones.
- Impresión de la lista de adyacencia con grados de entrada/salida.
- Verificación automática de las condiciones mínimas.

### 2. Recorridos

- **BFS** desde `u01`, `u05` y `u13`: orden de visita, nivel de cada vértice,
  cantidad de vértices alcanzados y listado de no alcanzados.
- **DFS completo** (bosque): orden de descubrimiento y detección de ciclos mediante
  colores (blanco/gris/negro) y reconstrucción del ciclo a partir de aristas de retroceso.

### 3. Consultas sociales

- Usuarios sin seguidores (grado de entrada 0).
- Usuarios más influyentes (mayor grado de entrada, con empates).
- Usuarios más activos (mayor grado de salida).
- Alcanzabilidad entre dos usuarios usando BFS, mostrando el camino más corto.

### 4. Operaciones CRUD

- Agregar y eliminar usuarios.
- Agregar y eliminar relaciones dirigidas.
- Actualizar nombre y rol.
- Impresión del cambio después de cada operación.
- Intentos de duplicado rechazados y evidenciados en consola.

### 5. Análisis de conectividad (valor agregado)

- Densidad del grafo.
- Componentes débilmente conexas.
- Componentes fuertemente conexas con el algoritmo de **Kosaraju**.
- Usuarios inalcanzables desde el usuario más activo.

---

## Evidencia de ejecución

Salida completa: [`docs/evidencia.txt`](docs/evidencia.txt). Extractos:

### Lista de adyacencia (fragmento)

```text
u01 | Ana Torres [Estudiante] (entrada: 3, salida: 5)
    → u02 (Bruno Díaz), u03 (Carla Ruiz), u04 (Diego Mesa), u05 (Elena Vidal), u06 (Fabián Ospina)
u02 | Bruno Díaz [Estudiante] (entrada: 1, salida: 4)
    → u03 (Carla Ruiz), u04 (Diego Mesa), u05 (Elena Vidal), u06 (Fabián Ospina)
u13 | Mateo Gil [Estudiante] (entrada: 0, salida: 1)
    → u07 (Gabriela Núñez)
u14 | Natalia Vega [Egresado] (entrada: 0, salida: 1)
    → u08 (Hugo Salazar)
Usuarios: 14 | Relaciones: 23 | Densidad: 12,64%
```

### Recorridos

| BFS desde | Alcanzados | No alcanzados |
|---|---|---|
| `u01` (Ana Torres) | 12 de 14 | u13, u14 |
| `u05` (Elena Vidal) | 12 de 14 | u13, u14 |
| `u13` (Mateo Gil) | 13 de 14 | u14 |

Orden de descubrimiento del DFS completo:

```text
u01 → u02 → u03 → u06 → u08 → u10 → u12 → u04 → u07 → u09 → u11 → u05 → u13 → u14
```

Ciclos dirigidos detectados:

```text
u01 → u02 → u03 → u01
u01 → u02 → u03 → u06 → u01
u03 → u06 → u08 → u10 → u12 → u03
u01 → u02 → u04 → u07 → u09 → u11 → u01
```

### Consultas sociales

```text
Usuarios sin seguidores: u13 (Mateo Gil), u14 (Natalia Vega)

Usuarios influyentes (entrada = 3, empate):
  u01 Ana Torres · u03 Carla Ruiz · u06 Fabián Ospina · u07 Gabriela Núñez

Usuarios más activos (salida = 5):
  u01 Ana Torres

¿u01 puede llegar a u12? SÍ
  u01 → u06 → u08 → u10 → u12 (4 aristas)

¿u01 puede llegar a u14? NO
```

### CRUD (extracto)

```text
[Paso 1] Agregar usuario u15
✔ Usuario agregado correctamente.
Usuarios: 15 | Relaciones: 23

[Paso 2] Intentar agregar un usuario duplicado (u15)
✖ Ya existe un usuario con Id 'u15'. Los Id deben ser únicos.

[Paso 3] Actualizar nombre y rol de u15
Antes:  Sofía Herrera [Estudiante]
Después: Sofía Herrera Gómez [Egresado]

[Paso 4] Agregar relación u15 → u01
✔ Relación agregada correctamente.

[Paso 5] Intentar agregar la relación duplicada u15 → u01
✖ La relación u15 → u01 ya existe.

[Paso 7] Eliminar relación u01 → u15
✔ Relación eliminada correctamente.

[Paso 8] Eliminar usuario u15
✔ Usuario u15 (Sofía Herrera Gómez) eliminado.
Relaciones eliminadas: u15 → u01
Usuarios: 14 | Relaciones: 23
```

---

## Análisis de resultados

1. **El grafo es débilmente conexo pero no fuertemente conexo.**
   Existe una sola componente débilmente conexa (los 14 usuarios están vinculados si se
   ignoran las direcciones), pero hay 3 componentes fuertemente conexas: el grupo
   `u01…u12` y las componentes triviales `u13` y `u14`. Por lo tanto, no todos los
   usuarios pueden alcanzarse mutuamente siguiendo las flechas.

2. **`u13` y `u14` no son alcanzables desde la comunidad principal.**
   Ambos tienen grado de entrada 0: nadie los sigue. Sin embargo, sí tienen relaciones
   salientes (`u13→u07` y `u14→u08`), por lo que ellos sí pueden llegar a la comunidad
   principal. Esto explica que el grafo sea débilmente conexo pero no fuertemente conexo.

3. **`u01` es el usuario más activo (salida 5).**
   Sigue a `u02`, `u03`, `u04`, `u05` y `u06`, y desde esos usuarios se alcanza a toda la
   comunidad principal (12 de 14 vértices). Aun así, `u01` no puede llegar a `u13` ni a
   `u14`, porque las aristas van en la dirección contraria.

4. **Hay un empate en influencia (grado de entrada 3).**
   `u01`, `u03`, `u06` y `u07` reciben tres seguidores cada uno. La influencia no depende
   de la actividad: `u01` es el más activo, pero comparte el primer lugar en seguidores.

5. **Los ciclos son frecuentes en la comunidad principal.**
   El DFS detectó cuatro ciclos, entre ellos el ciclo mínimo `u01 → u02 → u03 → u01` y
   ciclos más largos que pasan por `u07`, `u09` y `u11`. La existencia de ciclos es la
   razón por la que el grupo `u01…u12` forma una componente fuertemente conexa: desde
   cualquier usuario del grupo se puede regresar a `u01` y desde allí salir a los demás.

6. **La densidad es baja (12,64%).**
   Con 23 aristas de 182 posibles (`14 × 13`), la red es dispersa, aunque concentra la
   actividad en `u01` y `u02`. Esto es coherente con una red social académica donde unos
   pocos usuarios siguen a muchos y la mayoría tiene pocas relaciones.

---

## Checklist de pruebas mínimas

| Prueba | Estado |
|---|---|
| Grafo dirigido con ≥ 12 vértices y ≥ 18 aristas | ✔ 14 vértices, 23 aristas |
| Impresión de lista de adyacencia | ✔ |
| 3 BFS completos | ✔ desde u01, u05 y u13 |
| 1 DFS completo | ✔ 14 vértices descubiertos |
| Consultas de influyentes, activos y sin seguidores | ✔ |
| Al menos 1 ciclo dirigido detectable | ✔ 4 ciclos reportados |
| CRUD: agregar, eliminar y actualizar | ✔ 9 pasos demostrados |
| Duplicados rechazados | ✔ vértices y aristas |
| `Program.cs` sin lógica de negocio | ✔ solo invoca al controlador |
| Consola fuera del modelo | ✔ |
