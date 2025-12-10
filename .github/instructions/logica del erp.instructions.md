---
applyTo: '**'
---
Directivas de Arquitectura y Programación para el ERP AtelierPro
Este documento establece las directrices, patrones y estándares de programación que deben seguirse rigurosamente para garantizar que el ERP AtelierPro sea robusto, escalable, seguro y mantenible. Dada la naturaleza crítica del sistema (manejo de alta volumetría de datos y más de 200 usuarios concurrentes), la adherencia a estos principios no es negociable.

1. Filosofía y Principios Fundamentales
Clean Architecture (Arquitectura Limpia): Se aplicará una estricta separación de responsabilidades. La lógica de negocio no debe depender de detalles de implementación como la base de datos o la interfaz de usuario. Las dependencias siempre apuntan hacia el núcleo del negocio.
SOLID:
S (Single Responsibility): Cada clase tiene una única razón para cambiar. Un servicio maneja la lógica de un dominio (ej. ComprasService), un controlador expone una API, un componente Razor renderiza UI.
O (Open/Closed): Las entidades deben estar abiertas a la extensión pero cerradas a la modificación. Usar interfaces y clases base para permitir nuevos comportamientos sin alterar el código existente.
L (Liskov Substitution): Las clases derivadas deben ser sustituibles por sus clases base sin alterar la lógica del programa.
I (Interface Segregation): Crear interfaces específicas para cada cliente en lugar de una interfaz monolítica. No obligar a una clase a implementar métodos que no usa.
D (Dependency Inversion): Los módulos de alto nivel no deben depender de los de bajo nivel. Ambos deben depender de abstracciones (interfaces). Usar Inyección de Dependencias (DI) en todo el sistema.
Seguridad por Defecto: La seguridad no es una característica adicional, sino un pilar fundamental del diseño. Cada nueva pieza de código debe ser segura desde su concepción.
Asincronía Total: Toda operación de I/O (base de datos, red, sistema de archivos) debe ser asíncrona (async/await) para garantizar la escalabilidad y la capacidad de respuesta del sistema bajo carga.
2. Estructura de la Arquitectura (Capas)
La solución se organiza en las siguientes capas, y la comunicación entre ellas debe seguir este flujo:

Capa de Presentación (UI - AtelierPro/Pages, AtelierPro/Shared):

Tecnología: Blazor Server.
Responsabilidades: Exclusivamente lógica de UI, renderizado, y manejo de eventos del usuario.
Reglas:
NO debe contener lógica de negocio.
NO debe acceder directamente al DbContext.
DEBE inyectar (@inject) y consumir servicios de la capa de aplicación para obtener datos y ejecutar acciones.
DEBE manejar estados de carga (ej. BusyService) y mostrar retroalimentación clara al usuario (éxito, error).
Capa de Aplicación (Servicios - AtelierPro/Services):

Responsabilidades: Orquestar la lógica de negocio. Es el corazón funcional del sistema.
Reglas:
Contiene la lógica de validación de datos de entrada, reglas de negocio complejas y flujos de trabajo.
Actúa como intermediario entre la UI y la capa de datos.
DEBE gestionar la unidad de trabajo (transacciones). Cada método de servicio que modifica datos debe garantizar la consistencia.
Inyecta repositorios o el DbContext para interactuar con los datos.
Capa de Dominio (Modelos - AtelierPro/Models):

Responsabilidades: Definir las entidades del negocio (ej. OrdenCompra, Cliente, Producto).
Reglas:
Deben ser clases POCO (Plain Old C# Objects) con propiedades y, opcionalmente, métodos que contengan lógica intrínseca a la entidad (ej. CalcularTotal()).
NO deben tener dependencias de frameworks (EF Core, Blazor, etc.). Son el núcleo puro del negocio.
Capa de Infraestructura (Datos y Servicios Externos - AtelierPro/Data, AtelierPro/Services/FinditPartsService.cs):

Responsabilidades: Implementar la persistencia de datos y la comunicación con servicios externos.
Reglas:
Contiene el AtelierProDbContext, configuraciones de EF Core y migraciones.
Patrón Repositorio (Opcional pero Recomendado): Para lógica de acceso a datos compleja, se puede implementar un repositorio (IOrdenCompraRepository) para abstraer las consultas de EF Core de los servicios de aplicación.
Clientes para APIs externas (como FinditPartsService) residen aquí.
3. Lógica de Negocio y Transaccionalidad
Precisión Lógica: Toda la lógica de negocio debe estar contenida en la Capa de Aplicación. Evitar lógica dispersa en controladores o componentes Blazor.
Unidad de Trabajo (Unit of Work): DbContext implementa este patrón. Una única llamada a await _context.SaveChangesAsync() al final de una operación de servicio escribe todos los cambios (adds, updates, deletes) en una transacción atómica. Para operaciones que abarcan múltiples métodos o servicios, se debe usar IDbContextTransaction para controlar explícitamente el Commit y Rollback.
4. Acceso y Manipulación de Datos (Rendimiento para +200 Usuarios)
Consultas de Lectura: Para consultas que solo muestran datos (listas, reportes), usar siempre AsNoTracking() en EF Core. Esto deshabilita el seguimiento de cambios y mejora drásticamente el rendimiento.
var clientes = await _context.Clientes.AsNoTracking().ToListAsync();
Evitar Problemas N+1: Al cargar entidades y sus relaciones, usar Include() y ThenInclude() para obtener todos los datos en una sola consulta.
// MALO: Causa N+1 consultas
var ordenes = await _context.OrdenesCompra.ToListAsync();
foreach(var orden in ordenes) { var proveedor = orden.Proveedor; /*...*/ }

// BUENO: Una sola consulta
var ordenes = await _context.OrdenesCompra.Include(o => o.Proveedor).ToListAsync();
Proyecciones (Select): Nunca hacer SELECT * (ToList()) si solo se necesitan algunas columnas. Usar Select para proyectar los datos en un DTO (Data Transfer Object) o un modelo anónimo. Esto minimiza el tráfico de datos entre la BD y la aplicación.
Concurrencia Optimista: Para evitar que un usuario sobrescriba los cambios de otro, las entidades críticas (ej. Producto con su stock) deben tener un campo de versión de fila.
[] // Atributo Data Annotation
public byte[] RowVersion { get; set; }
// En Fluent API: .IsRowVersion()
EF Core detectará automáticamente conflictos de concurrencia al guardar y lanzará una DbUpdateConcurrencyException, que debe ser manejada en la capa de servicio para notificar al usuario.
Indexación: Asegurar que todas las claves foráneas (FK) y las columnas usadas frecuentemente en cláusulas WHERE (ej. Cliente.Nombre, Producto.SKU) tengan índices en la base de datos. Esto es crítico para el rendimiento.
5. Seguridad Avanzada
Autenticación y Autorización:
Usar ASP.NET Core Identity como base.
La autorización debe ser granular, basada en Roles ([Authorize(Roles = \"Administrador,Vendedor\")]) y, para casos más complejos, en Políticas/Claims.
Proteger cada Controller y página Blazor sensible con el atributo [Authorize].
Prevención de Vulnerabilidades:
SQL Injection: Totalmente prevenido al usar EF Core y consultas parametrizadas. Nunca concatenar strings para formar consultas SQL.
Cross-Site Scripting (XSS): Blazor codifica la salida de forma predeterminada, pero se debe tener cuidado al usar MarkupString o interoperar con JS. Validar toda entrada del usuario.
Cross-Site Request Forgery (CSRF): Los formularios de Blazor Server (<EditForm>) incluyen protección anti-CSRF por defecto.
Secrets Management: NUNCA almacenar contraseñas, API keys o connection strings en appsettings.json en producción. Usar el servicio de "User Secrets" en desarrollo y "Azure Key Vault" (o un sistema equivalente) en producción.
6. Pruebas
Pruebas Unitarias (AtelierPro.Tests): Probar la lógica de los servicios de la capa de aplicación en aislamiento. Usar un framework de mocking (como Moq) para simular dependencias (DbContext, otros servicios).
Pruebas de Integración: Crear pruebas que usen un proveedor de base de datos en memoria (como Microsoft.EntityFrameworkCore.InMemory) o SQLite para verificar que la lógica del servicio y las consultas de EF Core funcionan correctamente contra una base de datos real..