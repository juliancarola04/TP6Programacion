# TP6Programacion

Aplicación de gestión y venta de productos desarrollada como trabajo práctico. Incluye un panel web para la administración y las ventas presenciales, una aplicación móvil para las compras online y una API compartida.

## Accesos

| Aplicación | URL |
| --- | --- |
| Web | [tpprogramacion.runasp.net](https://tpprogramacion.runasp.net) |
| Backend | [tpprogramacion-backend.runasp.net](https://tpprogramacion-backend.runasp.net) |

## Tecnologías

- .NET 10 y C#.
- ASP.NET Core Web API.
- Blazor WebAssembly Standalone para Web.
- .NET MAUI Blazor Hybrid para Mobile, con foco en Android.
- MudBlazor para los componentes de interfaz.
- Entity Framework Core y PostgreSQL mediante Npgsql.
- JWT para autenticar las solicitudes y refresh tokens para renovar las sesiones.
- BCrypt para los hashes de contraseñas.
- QuestPDF para los comprobantes de venta.
- Serilog para el registro de eventos y errores.

## Estructura de la solución

```text
TP6Programacion.Backend   API, reglas de negocio, repositorios, base de datos y PDFs.
TP6Programacion.Web       Panel administrativo y punto de venta presencial.
TP6Programacion.Mobile    Catálogo, carrito, compras y pagos desde la aplicación móvil.
TP6Programacion.Shared    DTOs, enums y contratos compartidos.
```

El recorrido habitual de una operación es:

```text
Componente Razor
    -> Servicio del cliente
    -> Solicitud HTTP
    -> Controlador del backend
    -> Servicio de negocio
    -> Repositorio
    -> PostgreSQL
```

Los servicios de Web y Mobile se encargan de comunicarse con la API. Los servicios del backend aplican las validaciones y las reglas del negocio.

## Roles y funcionalidades

### Administrador

- Administrar productos, precios e imágenes.
- Crear, consultar, editar y eliminar categorías.
- Crear, consultar, editar y dar de baja proveedores.
- Dar de alta cuentas de Cajero o Administrador.
- Registrar ventas presenciales.
- Consultar el listado de ventas desde Web y descargar sus comprobantes.
- Registrar ingresos de mercadería para aumentar el stock, según el flujo previsto de ingresos.

La baja de proveedores es lógica: se conserva el registro con `Eliminado = true`. La eliminación de categorías es física y se rechaza si existen productos asociados.

### Cajero

- Acceder al punto de venta web.
- Consultar el catálogo para armar una venta presencial.
- Registrar productos, cantidades, medio de pago y referencia cuando corresponda.

### Cliente

- Registrarse desde Mobile e iniciar sesión.
- Agregar productos al carrito y modificar sus cantidades.
- Realizar compras online y pagar mediante la pasarela simulada.
- Consultar sus compras y abrir sus comprobantes PDF.

### Visitante

- Consultar el catálogo móvil sin iniciar sesión.
- Buscar productos, filtrar por categoría y seleccionar el orden disponible.
- Acceder al login y al registro.

El carrito, las compras y el pago requieren una cuenta con rol Cliente. La interfaz del panel web admite cuentas de Administrador y Cajero.

## Flujo general

### Preparación del catálogo

El administrador carga las categorías, los proveedores y los productos. Los productos se publican en el catálogo con su precio de venta, imagen y disponibilidad. El catálogo no expone el precio de compra.

### Ingresos de mercadería

El flujo de ingresos permite seleccionar un proveedor y cargar una lista de productos con cantidad y precio de compra unitario. Al confirmar, el backend:

1. Valida el proveedor, los productos y las cantidades.
2. Calcula los subtotales y el total.
3. Registra el ingreso y sus detalles.
4. Suma las unidades al stock de cada producto.
5. Actualiza el precio de compra del producto.
6. Registra el usuario que realizó la operación mediante el ID del JWT.

Este flujo forma parte del alcance de la aplicación. `IngresoService.Crear()` y los endpoints ya existen en Backend; la integración de la pantalla Web de ingresos está en desarrollo.

### Venta presencial

Desde `PuntoDeVenta.razor`, el Administrador o Cajero agrega productos, indica cantidades y medio de pago, y confirma la venta.

`VentaService.CrearVentaPresencial()` consulta los precios y el stock en la base, calcula el total, descuenta las unidades y registra la venta como confirmada con su pago aprobado.

El POS registra la venta a consumidor final, sin exigir datos del comprador. `ClienteId` puede ser nulo; `UsuarioId` identifica al empleado que realizó la venta. Este flujo registra el cobro presencial y no utiliza la pasarela simulada de Mobile.

### Compra online

1. El cliente consulta el catálogo y arma su carrito local.
2. Al continuar, Mobile envía a la API los IDs de productos y sus cantidades.
3. El backend identifica al cliente mediante el usuario del JWT.
4. Consulta los precios y el stock, calcula el total y guarda los precios unitarios en los detalles.
5. Descuenta las unidades como reserva y crea una venta pendiente con vencimiento de 15 minutos.
6. El cliente selecciona el medio de pago y solicita el cobro.
7. `PagoService` verifica que la venta sea suya, esté pendiente y no haya vencido.
8. La pasarela simulada devuelve el resultado y el backend actualiza la venta y el pago.
9. Cuando la venta queda confirmada, Mobile vacía el carrito.

El servidor calcula los importes a partir de la base de datos. No toma como válidos precios o totales enviados por el cliente.

La implementación actual de `PasarelaPagoSimulada` aprueba todos los pagos. Existe lógica para tratar un rechazo, cancelar la reserva y devolver el stock.

## Autenticación y autorización

### Inicio de sesión

`POST /api/auth/login` valida las credenciales y devuelve un `LoginResponse` con:

- `Token`: JWT de acceso, con ID de usuario, username, email, rol y vencimiento.
- `RefreshToken`: secreto utilizado para renovar el acceso.
- `SesionExpira`: vencimiento de la sesión asociada al refresh token.

El registro móvil también inicia una sesión y crea los registros relacionados de Usuario y Cliente. La creación administrativa de empleados no reemplaza la sesión del administrador que realiza el alta.

### Almacenamiento y envío

- Web guarda la sesión serializada en `localStorage`.
- Mobile guarda la sesión serializada en `SecureStorage`.
- Las solicitudes autenticadas envían el JWT en `Authorization: Bearer <token>`.

La API valida firma, emisor, destinatario y vencimiento antes de permitir el acceso a los endpoints protegidos. El mecanismo actual no utiliza cookies de autenticación.

### Estado de autenticación en Blazor

`EstadoAutenticacionProvider` obtiene un JWT vigente y construye un `ClaimsPrincipal` con sus claims. Blazor utiliza ese estado en `AuthorizeView` y `AuthorizeRouteView` para mostrar opciones y restringir páginas.

`App.razor` en Web y `Routes.razor` en Mobile actualizan ese estado al navegar. Leer los claims en el cliente sirve para la interfaz; no sustituye la validación y autorización que realiza la API.

### Renovación de sesión

`AutenticacionService` intenta renovar el acceso en dos situaciones:

- `ObtenerTokenVigente()` detecta que el JWT guardado venció.
- Una petición enviada mediante `EnviarAutenticado()` recibe un 401; se intenta renovar y se reenvía una vez con el nuevo token.

No se realiza una renovación por temporizador mientras la aplicación permanece inactiva.

`POST /api/auth/refresh` recibe el refresh token. `SesionService.Renovar()` calcula su hash y busca la sesión en `SesionesUsuarios`. Comprueba que no esté vencida ni revocada y que el usuario siga habilitado.

Si la sesión es válida, genera un JWT y un refresh token nuevos. En la misma fila reemplaza el hash anterior y extiende el vencimiento 30 días desde la renovación. El cliente guarda la nueva respuesta. Así, una sesión puede continuar mediante renovaciones sucesivas mientras siga siendo válida.

Aunque la propiedad de la entidad se llama `RefreshToken`, en la base se almacena su hash, no el secreto original. El refresh anterior deja de funcionar después de la rotación.

### Cierre de sesión y permisos

`POST /api/auth/logout` revoca la sesión identificada por el refresh token. El cliente elimina sus tokens locales. La revocación impide renovar; un JWT de acceso ya emitido puede seguir siendo válido hasta su vencimiento.

Los roles se utilizan tanto en la interfaz como en los endpoints mediante `[Authorize]` y `[Authorize(Roles = "...")]`. Los cambios de rol quedan reflejados al emitir un nuevo JWT. Las compras y comprobantes del cliente incluyen además la comprobación de que la venta le pertenezca.

## Carrito móvil

`CarritoService` conserva el carrito en `SecureStorage`, separado por usuario mediante una clave como `carrito_12`.

Cada `CarritoItem` guarda únicamente `ProductoId` y `Cantidad`. El carrito persiste al cerrar la aplicación y no se comparte entre cuentas del mismo dispositivo.

- Al agregar un producto existente, se incrementa su cantidad.
- La pantalla permite sumar y restar unidades.
- Si la cantidad llega a cero, se elimina el producto.
- Cada modificación se guarda localmente.
- Para mostrar el carrito se consultan los productos correspondientes en la API y se obtienen sus nombres, precios y stock actuales.
- El subtotal y el total de la pantalla son informativos: la compra se recalcula en el backend.

Agregar o modificar unidades en el carrito no reserva ni descuenta stock. La reserva ocurre al crear la compra online, y el backend vuelve a comprobar la disponibilidad.

## Comprobantes PDF

QuestPDF genera los comprobantes en el backend a partir de los datos guardados de la venta. `ReporteVenta` recibe `ComprobanteVentaResponse`, que reúne el detalle de la venta y sus pagos sin modificar el DTO general `VentaResponse`.

El documento incluye número, fecha y estado de la venta, cliente o consumidor final, productos, cantidades, precios unitarios, subtotales, total y datos de los pagos: monto, medio, referencia y estado.

| Uso | Endpoint | Acceso |
| --- | --- | --- |
| Comprobante del cliente | `GET /api/ventas/mis-compras/{id}/pdf` | Cliente propietario de la compra |
| Comprobante administrativo | `GET /api/ventas/{id}/pdf` | Administrador |

La API genera el archivo en memoria y lo devuelve con tipo `application/pdf`. No necesita guardar los comprobantes en el disco del servidor.

En Web, el servicio obtiene los bytes con una solicitud autenticada y JavaScript inicia la descarga. En Mobile, se guarda una copia temporal en la caché y `Launcher` abre una aplicación compatible con PDF. La impresión depende de las opciones del visor utilizado.

## Ejecución local

### Requisitos

- SDK de .NET 10.
- PostgreSQL y una base de datos configurada para la API.
- Visual Studio con soporte para ASP.NET y, para Mobile, .NET MAUI y Android.
- Android SDK y emulador o dispositivo compatible.
- Herramienta `dotnet-ef` compatible con EF Core 10 para aplicar migraciones.

### Configuración del backend

Configurar `ConnectionStrings:DefaultConnection` y los valores de `JwtSettings`: `Key`, `Issuer`, `Audience` y `ExpiryMinutes`. Utilizar User Secrets en desarrollo o la configuración del entorno del hosting; no publicar contraseñas ni claves de firma en el repositorio.

`Cors:Origenes` define las direcciones de Web que pueden consumir la API desde el navegador. Web configura la URL de la API en `Apis:TP6Programacion`, dentro de sus archivos `wwwroot/appsettings.*.json`.

Aplicar las migraciones desde la raíz de la solución:

```powershell
dotnet ef database update --project TP6Programacion.Backend --startup-project TP6Programacion.Backend
```

Al iniciar, `SeedDatos.CrearAdministrador()` crea la cuenta de demostración `admin` con contraseña `admin` si no existe un usuario con ese nombre. No modifica la contraseña de una cuenta ya creada.

### Backend y Web

Ejecutar en dos terminales distintas, desde la raíz de la solución:

```powershell
dotnet run --project TP6Programacion.Backend --launch-profile https
```

```powershell
dotnet run --project TP6Programacion.Web --launch-profile https
```

| Servicio | Dirección local |
| --- | --- |
| Backend HTTPS | `https://localhost:7272` |
| Backend HTTP | `http://localhost:5156` |
| Web HTTPS | `https://localhost:7248` |
| Web HTTP | `http://localhost:5215` |
| Documento OpenAPI | `https://localhost:7272/openapi/v1.json` |

OpenAPI se publica únicamente en el entorno `Development`.

### Mobile

Seleccionar `TP6Programacion.Mobile` y un emulador Android en Visual Studio. Iniciar el backend junto con Mobile cuando se utilice la configuración Debug.

`ConfiguracionApi` selecciona la API según la compilación:

| Configuración | API utilizada |
| --- | --- |
| Debug en Android Emulator | `https://10.0.2.2:7272/` |
| Debug en otras plataformas | `https://localhost:7272/` |
| Release | `https://tpprogramacion-backend.runasp.net/` |

En Android Emulator, `10.0.2.2` permite acceder al backend del equipo de desarrollo. Para ejecutar en un teléfono físico contra una API local hay que adaptar la dirección a una accesible desde ese dispositivo.

## Estado de integración

- La interfaz Web de ingresos de mercadería está en desarrollo. El flujo de reposición de stock está incluido en el alcance y cuenta con implementación en Backend.
- Las compras online vencen a los 15 minutos. Existe `VencerReserva()` para cancelar reservas y devolver stock, pero queda pendiente conectarlo a una ejecución automática; el vencimiento por sí solo no libera la reserva.
- La página Web del historial de ventas y su PDF administrativo son exclusivos del Administrador. Actualmente los endpoints GET del listado general y detalle también admiten Cajero; se debe alinear esa configuración si la consulta debe ser exclusiva del Administrador en la API.
