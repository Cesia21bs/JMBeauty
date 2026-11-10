# JM-BEAUTY CLAROS (Academia – Salón)
## Sistema Integral de Gestión Empresarial en 4 Capas (.NET 8, PostgreSQL, DevExpress)

---

## 1. Estructura del Proyecto

El sistema está estructurado bajo una **arquitectura en 4 capas estrictamente desacopladas y unidireccionales**:

```
JMBeauty.sln
│
├── JMBeauty.Entidades/       (Capa 1: Modelos de dominio POCO puros, cero dependencias)
│   ├── Enums.cs              (EstadoCita, EstadoDocumento, TipoPago)
│   ├── Rol.cs & Empleado.cs  (Datos de personal, comisiones % y color de agenda)
│   ├── Cliente.cs            (Ficha técnica, historial de tintes y alergias)
│   ├── Servicio.cs           (Duración en minutos, tarifas y categorías)
│   ├── Producto.cs           (Inventario, existencias mínimas e insumos internos)
│   ├── Cita.cs               (Horarios de agendamiento y servicios asignados)
│   ├── Venta.cs              (Punto de venta, facturación y desglose de comisiones)
│   ├── Compra.cs             (Compras a proveedores e insumos técnicos)
│   └── Reportes.cs           (Inventario actual y comisiones diarias)
│
├── JMBeauty.Datos/           (Capa 2: Persistencia ADO.NET de alto rendimiento con Npgsql)
│   ├── Conexion.cs           (NpgsqlDataSource con connection pooling y variable JMBEAUTY_DB_PASSWORD)
│   ├── EmpleadoDatos.cs      (CRUD de personal y roles)
│   ├── ClienteDatos.cs       (Directorio de clientes y ficha de belleza)
│   ├── ServicioDatos.cs      (Catálogo de servicios y categorías)
│   ├── ProductoDatos.cs      (Inventario, kardex y vista vw_inventario_actual)
│   ├── CitaDatos.cs          (Prevención estricta de solapamiento de horario)
│   ├── VentaDatos.cs         (Transacción atómica: venta, comisiones, stock y cierre de cita)
│   └── CompraDatos.cs        (Registro transaccional de compras a proveedores)
│
├── JMBeauty.Negocio/         (Capa 3: Reglas de negocio y orquestación)
│   ├── ConexionNegocio.cs    (Diagnóstico y validación previa de credenciales)
│   ├── EmpleadoNegocio.cs    (Validaciones de comisiones 0-100% y datos obligatorios)
│   ├── ClienteNegocio.cs     (Reglas de contacto y fichas técnicas)
│   ├── ServicioNegocio.cs    (Validaciones de duración y precios)
│   ├── ProductoNegocio.cs    (Validaciones de márgenes y existencias mínimas)
│   ├── CitaNegocio.cs        (Comprobación de agenda y duración de servicios)
│   ├── VentaNegocio.cs       (Generación de comprobantes y validaciones de importes)
│   └── CompraNegocio.cs      (Validaciones de cantidades y costos)
│
├── JMBeauty.Presentacion/    (Capa 4: Interfaz WinForms + DevExpress)
│   ├── TemaJMBeauty.cs       (Paleta de colores: Crema, Dorado, Rosa Nude, Rosa Pastel, Rosa Claro)
│   ├── FormPrincipal.cs      (Tablero de control ejecutivo con logo y navegación)
│   └── Recursos/Svg/         (Iconos vectoriales SVG adaptados a la paleta de la marca)
│       ├── citas.svg
│       ├── servicios.svg
│       ├── productos.svg
│       ├── clientes.svg
│       ├── empleados.svg
│       ├── ventas_caja.svg
│       ├── inventario_compras.svg
│       └── comisiones.svg
│
└── scripts/                  (Scripts SQL para PostgreSQL)
    ├── 00_JMBeauty_PostgreSQL_CrearBase.sql
    ├── 01_JMBeauty_PostgreSQL_Estructura.sql
    └── 02_JMBeauty_PostgreSQL_DatosIniciales.sql
```

---

## 2. Puesta en Marcha Rápida

### Paso 1: Configurar la Variable de Entorno en Windows
Abra una terminal PowerShell o CMD y configure la contraseña de su PostgreSQL local (sin comillas si no tiene espacios):
```powershell
[System.Environment]::SetEnvironmentVariable('JMBEAUTY_DB_PASSWORD', 'su_contraseña_aqui', 'User')
```

### Paso 2: Ejecutar los Scripts SQL en PostgreSQL
1. En **pgAdmin** o mediante **psql**, cree la base de datos `jmbeauty_db`:
   ```sql
   CREATE DATABASE jmbeauty_db WITH ENCODING = 'UTF8';
   ```
2. Ejecute el script de estructura [01_JMBeauty_PostgreSQL_Estructura.sql](file:///d:/jmbeuty/scripts/01_JMBeauty_PostgreSQL_Estructura.sql).
3. Ejecute el script de datos iniciales [02_JMBeauty_PostgreSQL_DatosIniciales.sql](file:///d:/jmbeuty/scripts/02_JMBeauty_PostgreSQL_DatosIniciales.sql).

### Paso 3: Compilar y Ejecutar la Solución
```powershell
dotnet build JMBeauty.sln
dotnet run --project JMBeauty.Presentacion
```

