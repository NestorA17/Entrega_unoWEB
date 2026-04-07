# Tutorial: Frontend Blazor CRUD
# Parte 7: Verificacion Final

En esta ultima parte vamos a correr la API y el frontend juntos, probar las 6 tablas CRUD y confirmar que todo funciona.

---

## 7.1 Requisitos Previos

Antes de empezar necesitamos:

1. **SQL Server** corriendo con la base de datos `bdfacturas_sqlserver_local`
2. **La API `ApiGenericaCsharp`** lista para correr (puerto 5034)
3. **El frontend `FrontBlazor_AppiGenericaCsharp`** listo para correr (puerto 5100)

Si no tienes datos en las tablas, no hay problema. El frontend mostrara "No se encontraron registros" y podras crear registros desde el formulario.

---

## 7.2 Paso 1: Correr la API

Abrimos una terminal de PowerShell y ejecutamos:

```powershell
cd ApiGenericaCsharp
dotnet run
```

Debemos ver algo como:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5034
```

**No cerrar esta terminal.** La API debe seguir corriendo mientras probamos el frontend.

Para verificar que la API responde, podemos abrir en el navegador:

```
http://localhost:5034/swagger
```

Si vemos la interfaz de Swagger, la API esta funcionando correctamente.

---

## 7.3 Paso 2: Correr el Frontend

Abrimos **otra terminal** de PowerShell (la anterior debe seguir abierta con la API) y ejecutamos:

```powershell
cd FrontBlazor_AppiGenericaCsharp
dotnet run
```

Debemos ver algo como:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5100
```

Ahora abrimos el navegador en:

```
http://localhost:5100
```

Debemos ver la pagina de inicio con el mensaje de bienvenida.

---

## 7.4 Paso 3: Probar Cada Tabla

Vamos a probar las 3 operaciones basicas en cada tabla: **Crear**, **Editar** y **Eliminar**.

### 7.4.1 Probar Producto

1. Click en **Producto** en el menu lateral
2. Click en **Nuevo Producto**
3. Llenar: Codigo=`PR001`, Nombre=`Laptop`, Stock=`10`, Valor Unitario=`1500.50`
4. Click en **Guardar** → debe aparecer alerta verde "Registro creado"
5. El registro aparece en la tabla
6. Click en **Editar** → cambiar Nombre a `Laptop Pro`
7. Click en **Guardar** → alerta verde "Registro actualizado"
8. Click en **Eliminar** → alerta verde "Registro eliminado"

### 7.4.2 Probar Empresa

1. Click en **Empresa** en el menu
2. Crear: Codigo=`E001`, Nombre=`Mi Empresa`
3. Editar: cambiar Nombre a `Mi Empresa S.A.`
4. Eliminar el registro

### 7.4.3 Probar Persona

1. Click en **Persona** en el menu
2. Crear: Codigo=`P001`, Nombre=`Juan`, Email=`juan@test.com`, Telefono=`555-1234`
3. Editar: cambiar Email a `juan@empresa.com`
4. Eliminar el registro

### 7.4.4 Probar Rol

1. Click en **Rol** en el menu
2. Crear: ID=`1`, Nombre=`Administrador`
3. Editar: cambiar Nombre a `Admin`
4. Eliminar el registro

**Nota:** En esta tabla la clave primaria es `id` (numerico), no `codigo` (texto).

### 7.4.5 Probar Ruta

1. Click en **Ruta** en el menu
2. Crear: Ruta=`/api/productos`, Descripcion=`Endpoint de productos`
3. Editar: cambiar Descripcion a `Endpoint CRUD de productos`
4. Eliminar el registro

### 7.4.6 Probar Usuario

1. Click en **Usuario** en el menu
2. Crear: Email=`admin@test.com`, Contrasena=`123456`
3. Editar: cambiar Contrasena a `nueva123`
4. Eliminar el registro

**Nota:** El campo contrasena se muestra con `type="password"` en el formulario (caracteres ocultos), pero en la tabla se muestra el valor tal como lo devuelve la API.

---

## 7.5 Posibles Errores y Soluciones

### Error: "No se pudo conectar con la API"

- **Causa:** La API no esta corriendo o esta en otro puerto
- **Solucion:** Verificar que la terminal de la API muestre `Now listening on: http://localhost:5034`

### Error: "No se encontraron registros" (cuando si deberia haber)

- **Causa:** El nombre de la tabla no coincide con la base de datos
- **Solucion:** Verificar en Swagger que `GET /api/{tabla}` devuelva datos. Si la tabla no existe, la API devuelve un error

### Error: La pagina no responde a clicks

- **Causa:** Falta `@rendermode InteractiveServer` en la pagina
- **Solucion:** Verificar que la directiva este presente en la linea 2 de cada `.razor`

### Error: "405 Method Not Allowed" o "CORS"

- **Causa:** La API no tiene CORS habilitado
- **Solucion:** Verificar que el `Program.cs` de la API tenga `builder.Services.AddCors(...)` y `app.UseCors("PermitirTodo")`

---

## 7.6 Resumen del Proyecto Completo

### Estructura de Archivos

```
FrontBlazor_AppiGenericaCsharp/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor      ← Layout principal
│   │   ├── MainLayout.razor.css
│   │   └── NavMenu.razor          ← Menu lateral con 6 links
│   ├── Pages/
│   │   ├── Home.razor             ← Pagina de inicio
│   │   ├── Empresa.razor          ← CRUD Empresa
│   │   ├── Persona.razor          ← CRUD Persona
│   │   ├── Producto.razor         ← CRUD Producto
│   │   ├── Rol.razor              ← CRUD Rol
│   │   ├── Ruta.razor             ← CRUD Ruta
│   │   └── Usuario.razor          ← CRUD Usuario
│   ├── _Imports.razor
│   ├── App.razor
│   └── Routes.razor
├── Services/
│   └── ApiService.cs              ← Servicio generico para la API
├── Properties/
│   └── launchSettings.json        ← Puerto 5100
├── Program.cs                     ← HttpClient + DI
├── Parte1_ConceptosFundamentales.md
├── Parte2_CrearProyectoYConfiguracion.md
├── Parte3_ApiService.md
├── Parte4_LayoutYNavegacion.md
├── Parte5_CrudProducto.md
├── Parte6_CrudDemasTablas.md
└── Parte7_VerificacionFinal.md    ← Este archivo
```

### Resumen por Parte

| Parte | Que Hicimos |
|---|---|
| 1 | Conceptos fundamentales de Blazor |
| 2 | Crear proyecto, configurar puerto 5100, HttpClient a API 5034, git init |
| 3 | ApiService: servicio generico con ListarAsync, CrearAsync, ActualizarAsync, EliminarAsync |
| 4 | Layout y navegacion: NavMenu con 6 links, MainLayout limpio |
| 5 | CRUD completo de Producto (patron base) |
| 6 | CRUD de las 5 tablas restantes (Empresa, Persona, Rol, Ruta, Usuario) |
| 7 | Verificacion final: correr API + frontend, probar todo |

### Las 6 Tablas

| Tabla | Campos | Clave Primaria | Tipo PK |
|---|---|---|---|
| empresa | codigo, nombre | codigo | string |
| persona | codigo, nombre, email, telefono | codigo | string |
| producto | codigo, nombre, stock, valorunitario | codigo | string |
| rol | id, nombre | id | int |
| ruta | ruta, descripcion | ruta | string |
| usuario | email, contrasena | email | string |

### Tecnologias Utilizadas

- **Blazor Server** (.NET 9) con `InteractiveServer` render mode
- **HttpClient** configurado en DI apuntando a la API
- **Bootstrap 5** para estilos (tablas, formularios, alertas, cards, spinner)
- **ApiService** generico usando `Dictionary<string, object?>` (sin modelos tipados)
- **API GenericaCsharp** como backend (CRUD generico por nombre de tabla)

---

## 7.7 Que Sigue (Ideas para Mejorar)

Este tutorial cubre un CRUD basico funcional. Algunas mejoras posibles para el futuro:

1. **Confirmacion antes de eliminar:** Agregar un modal "Esta seguro?" antes de borrar
2. **Validacion de formularios:** Usar `EditForm` con `DataAnnotations` de Blazor
3. **Tablas con FK:** Crear paginas para tablas con relaciones (dropdown para seleccionar la FK)
4. **Autenticacion:** Implementar login con JWT usando la tabla `usuario`
5. **Paginacion:** Para tablas con muchos registros
6. **Busqueda/filtrado:** Agregar un campo para filtrar registros en la tabla

---

## 7.8 Commit Final

```powershell
git add .
git commit -m "Agregar Parte 7: verificacion final y resumen del proyecto"
git push
```

Con esto completamos el tutorial. El proyecto esta funcionando y publicado en GitHub.
