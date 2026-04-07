# Tutorial: Frontend Blazor CRUD
# Parte 6: CRUD de las 5 Tablas Restantes

En esta parte creamos las paginas CRUD para Empresa, Persona, Rol, Ruta y Usuario. Todas siguen el mismo patron de `Producto.razor` (Parte 5), adaptado a las columnas de cada tabla.

---

## 6.1 Que Cambia de una Tabla a Otra

El patron CRUD es identico para todas las tablas. Lo unico que cambia entre una pagina y otra son 4 cosas:

| Que cambia | Ejemplo Producto | Ejemplo Empresa |
|---|---|---|
| `@page` | `@page "/producto"` | `@page "/empresa"` |
| Nombre de la tabla en la API | `"producto"` | `"empresa"` |
| Campos del formulario | codigo, nombre, stock, valorunitario | codigo, nombre |
| Nombre de la clave primaria | `"codigo"` | `"codigo"` |

La estructura HTML (alerta + formulario + tabla + spinner) y la logica C# (CargarRegistros, NuevoRegistro, EditarRegistro, GuardarRegistro, EliminarRegistro, Cancelar) son las mismas.

---

## 6.2 Empresa

**Columnas:** codigo (varchar 10), nombre (varchar 200)
**Clave primaria:** codigo

Esta es la tabla mas simple: solo 2 campos.

Archivo: `Components/Pages/Empresa.razor`

**Diferencias con Producto:**
- Solo 2 campos en el formulario: `campoCodigo` y `campoNombre`
- No hay campos numericos (stock, valorunitario)
- El diccionario de datos solo tiene 2 entradas

---

## 6.3 Persona

**Columnas:** codigo (varchar 20), nombre (varchar 100), email (varchar 100), telefono (varchar 20)
**Clave primaria:** codigo

Archivo: `Components/Pages/Persona.razor`

**Diferencias con Producto:**
- 4 campos de texto: codigo, nombre, email, telefono
- Todos son `string`, no hay campos numericos
- El input de email usa `type="email"` para validacion basica del navegador

---

## 6.4 Rol

**Columnas:** id (int), nombre (varchar 100)
**Clave primaria:** id

Archivo: `Components/Pages/Rol.razor`

**Diferencias con Producto:**
- La clave primaria es `id` (int), no `codigo` (string)
- El campo `id` usa `type="number"` en el formulario
- En la API: `PUT /api/rol/id/1` y `DELETE /api/rol/id/1` (en vez de `/codigo/PR001`)
- Al llamar a `ActualizarAsync` y `EliminarAsync`, el nombre de la clave es `"id"` en lugar de `"codigo"`

---

## 6.5 Ruta

**Columnas:** ruta (varchar 100), descripcion (varchar 255)
**Clave primaria:** ruta

Archivo: `Components/Pages/Ruta.razor`

**Diferencias con Producto:**
- La clave primaria se llama `ruta` (mismo nombre que la tabla)
- Solo 2 campos: ruta y descripcion
- La pagina usa `@page "/ruta"` (nota: la ruta URL de la pagina coincide con el nombre de la tabla y de la columna, pero son cosas diferentes)

---

## 6.6 Usuario

**Columnas:** email (varchar 100), contrasena (varchar 100)
**Clave primaria:** email

Archivo: `Components/Pages/Usuario.razor`

**Diferencias con Producto:**
- La clave primaria es `email`
- El campo contrasena usa `type="password"` para ocultar los caracteres
- Al crear un usuario, la API encripta la contrasena si se le pasa el parametro `camposEncriptar`. En este CRUD basico enviamos la contrasena en texto plano (la API se encarga de encriptarla si esta configurada para eso)

---

## 6.7 Commit

```powershell
git add .
git commit -m "Agregar CRUD de Empresa, Persona, Rol, Ruta y Usuario"
git push
```

---

## Siguiente Parte

En la **Parte 7** haremos la verificacion final: correr la API y el frontend juntos, probar las 6 tablas CRUD y confirmar que todo funciona.
