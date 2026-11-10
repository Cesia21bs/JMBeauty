-- =============================================================================
-- PROYECTO: JM-BEAUTY CLAROS (Academia - Salón)
-- ARCHIVO: 01_JMBeauty_PostgreSQL_Estructura.sql
-- DESCRIPCIÓN: Definición completa de Tablas, Restricciones, Índices y Vistas
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS public;

-- -----------------------------------------------------------------------------
-- 0. TIPOS ENUM PERSONALIZADOS
-- -----------------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'estado_cita') THEN
        CREATE TYPE estado_cita AS ENUM ('PROGRAMADA', 'CONFIRMADA', 'EN_ATENCION', 'FINALIZADA', 'CANCELADA', 'NO_ASISTIO');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'estado_documento') THEN
        CREATE TYPE estado_documento AS ENUM ('REGISTRADA', 'EMITIDA', 'ANULADA');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'tipo_pago') THEN
        CREATE TYPE tipo_pago AS ENUM ('EFECTIVO', 'TARJETA_CREDITO', 'TARJETA_DEBITO', 'TRANSFERENCIA', 'MIXTO');
    END IF;
END $$;

-- -----------------------------------------------------------------------------
-- 1. TABLA: ROL DE PERSONAL
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.rol (
    rol_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    descripcion VARCHAR(150),
    tipo_acceso VARCHAR(30) NOT NULL DEFAULT 'ESPECIALISTA', -- ADMINISTRADOR, RECEPCIONISTA, ESPECIALISTA
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_rol_nombre UNIQUE (nombre)
);

-- -----------------------------------------------------------------------------
-- 2. TABLA: EMPLEADOS / PROFESIONALES DEL SALÓN
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.empleado (
    empleado_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    rol_id INT NOT NULL REFERENCES public.rol(rol_id),
    nombres VARCHAR(80) NOT NULL,
    apellidos VARCHAR(80) NOT NULL,
    documento_identidad VARCHAR(30) NOT NULL,
    telefono VARCHAR(25) NOT NULL,
    email VARCHAR(100),
    clave_acceso VARCHAR(50) NOT NULL DEFAULT '1234', -- Clave/PIN de acceso al sistema
    porcentaje_comision_servicio NUMERIC(5,2) NOT NULL DEFAULT 0.00 CHECK (porcentaje_comision_servicio BETWEEN 0 AND 100),
    porcentaje_comision_producto NUMERIC(5,2) NOT NULL DEFAULT 0.00 CHECK (porcentaje_comision_producto BETWEEN 0 AND 100),
    color_agenda VARCHAR(10) NOT NULL DEFAULT '#D4AF37', -- Color Hex para identificación en calendario
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_empleado_documento UNIQUE (documento_identidad)
);
CREATE INDEX IF NOT EXISTS idx_empleado_activo ON public.empleado(activo);

-- -----------------------------------------------------------------------------
-- 3. TABLA: CLIENTES Y FICHA TÉCNICA
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.cliente (
    cliente_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombres VARCHAR(80) NOT NULL,
    apellidos VARCHAR(80) NOT NULL,
    telefono VARCHAR(25) NOT NULL,
    email VARCHAR(100),
    direccion VARCHAR(200),
    historial_alergias_notas TEXT, -- Ficha técnica: alergias, fórmulas de tinte, preferencias capilares
    fecha_nacimiento DATE,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_cliente_telefono ON public.cliente(telefono);
CREATE INDEX IF NOT EXISTS idx_cliente_apellidos ON public.cliente(lower(apellidos));

-- -----------------------------------------------------------------------------
-- 4. TABLAS: CATEGORÍAS Y CATÁLOGO DE SERVICIOS
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.categoria_servicio (
    categoria_servicio_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre VARCHAR(80) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_cat_servicio_nombre UNIQUE (nombre)
);

CREATE TABLE IF NOT EXISTS public.servicio (
    servicio_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    categoria_servicio_id INT NOT NULL REFERENCES public.categoria_servicio(categoria_servicio_id),
    codigo VARCHAR(30) NOT NULL,
    nombre VARCHAR(120) NOT NULL,
    descripcion VARCHAR(255),
    duracion_minutos INT NOT NULL CHECK (duracion_minutos > 0),
    precio_base NUMERIC(12,2) NOT NULL CHECK (precio_base >= 0),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_servicio_codigo UNIQUE (codigo)
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_servicio_codigo_lower ON public.servicio(lower(codigo));

-- -----------------------------------------------------------------------------
-- 5. TABLAS: CATEGORÍAS Y PRODUCTOS (INVENTARIO Y KARDEX)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.categoria_producto (
    categoria_producto_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre VARCHAR(80) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_cat_producto_nombre UNIQUE (nombre)
);

CREATE TABLE IF NOT EXISTS public.producto (
    producto_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    categoria_producto_id INT NOT NULL REFERENCES public.categoria_producto(categoria_producto_id),
    codigo VARCHAR(30) NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    precio_costo NUMERIC(12,2) NOT NULL DEFAULT 0.00 CHECK (precio_costo >= 0),
    precio_venta NUMERIC(12,2) NOT NULL CHECK (precio_venta >= 0),
    existencia_minima NUMERIC(12,2) NOT NULL DEFAULT 5.00 CHECK (existencia_minima >= 0),
    es_insumo_interno BOOLEAN NOT NULL DEFAULT FALSE, -- Champús de uso técnico vs venta al público
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_producto_codigo UNIQUE (codigo)
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_producto_codigo_lower ON public.producto(lower(codigo));

-- -----------------------------------------------------------------------------
-- 6. TABLAS: PROVEEDORES Y COMPRAS
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.proveedor (
    proveedor_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    razon_social VARCHAR(150) NOT NULL,
    contacto VARCHAR(100),
    telefono VARCHAR(25),
    direccion VARCHAR(200),
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS public.compra (
    compra_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    proveedor_id INT NOT NULL REFERENCES public.proveedor(proveedor_id),
    empleado_id INT NOT NULL REFERENCES public.empleado(empleado_id),
    numero_factura_proveedor VARCHAR(50) NOT NULL,
    fecha_compra TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    estado estado_documento NOT NULL DEFAULT 'EMITIDA',
    notas VARCHAR(255)
);

CREATE TABLE IF NOT EXISTS public.detalle_compra (
    detalle_compra_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    compra_id INT NOT NULL REFERENCES public.compra(compra_id) ON DELETE RESTRICT,
    producto_id INT NOT NULL REFERENCES public.producto(producto_id) ON DELETE RESTRICT,
    cantidad NUMERIC(12,2) NOT NULL CHECK (cantidad > 0),
    costo_unitario NUMERIC(12,2) NOT NULL CHECK (costo_unitario >= 0),
    subtotal NUMERIC(12,2) GENERATED ALWAYS AS (ROUND(cantidad * costo_unitario, 2)) STORED
);
CREATE INDEX IF NOT EXISTS idx_detalle_compra_compra ON public.detalle_compra(compra_id);
CREATE INDEX IF NOT EXISTS idx_detalle_compra_prod ON public.detalle_compra(producto_id);

-- -----------------------------------------------------------------------------
-- 7. TABLAS: AGENDAMIENTO DE CITAS
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.cita (
    cita_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cliente_id INT NOT NULL REFERENCES public.cliente(cliente_id),
    empleado_id INT NOT NULL REFERENCES public.empleado(empleado_id),
    fecha_hora_inicio TIMESTAMPTZ NOT NULL,
    fecha_hora_fin TIMESTAMPTZ NOT NULL,
    estado estado_cita NOT NULL DEFAULT 'PROGRAMADA',
    observaciones VARCHAR(255),
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ck_rango_horario CHECK (fecha_hora_fin > fecha_hora_inicio)
);
CREATE INDEX IF NOT EXISTS idx_cita_horario ON public.cita(empleado_id, fecha_hora_inicio, fecha_hora_fin);

CREATE TABLE IF NOT EXISTS public.detalle_cita_servicio (
    detalle_cita_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cita_id INT NOT NULL REFERENCES public.cita(cita_id) ON DELETE CASCADE,
    servicio_id INT NOT NULL REFERENCES public.servicio(servicio_id),
    duracion_minutos INT NOT NULL,
    precio_estimado NUMERIC(12,2) NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_detalle_cita_cita ON public.detalle_cita_servicio(cita_id);

-- -----------------------------------------------------------------------------
-- 8. TABLAS: VENTAS / FACTURACIÓN Y DETALLES
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.venta (
    venta_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    numero_comprobante VARCHAR(30) NOT NULL,
    cliente_id INT NOT NULL REFERENCES public.cliente(cliente_id),
    empleado_cajero_id INT NOT NULL REFERENCES public.empleado(empleado_id),
    cita_id INT NULL REFERENCES public.cita(cita_id),
    fecha_venta TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tipo_pago tipo_pago NOT NULL DEFAULT 'EFECTIVO',
    estado estado_documento NOT NULL DEFAULT 'EMITIDA',
    porcentaje_descuento NUMERIC(5,2) NOT NULL DEFAULT 0.00 CHECK (porcentaje_descuento BETWEEN 0 AND 100),
    monto_descuento NUMERIC(12,2) NOT NULL DEFAULT 0.00 CHECK (monto_descuento >= 0),
    monto_impuesto NUMERIC(12,2) NOT NULL DEFAULT 0.00 CHECK (monto_impuesto >= 0),
    monto_total NUMERIC(12,2) NOT NULL DEFAULT 0.00 CHECK (monto_total >= 0),
    CONSTRAINT uq_venta_comprobante UNIQUE (numero_comprobante)
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_venta_comprobante_lower ON public.venta(lower(numero_comprobante));

CREATE TABLE IF NOT EXISTS public.detalle_venta_servicio (
    detalle_servicio_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    venta_id INT NOT NULL REFERENCES public.venta(venta_id) ON DELETE RESTRICT,
    servicio_id INT NOT NULL REFERENCES public.servicio(servicio_id),
    empleado_profesional_id INT NOT NULL REFERENCES public.empleado(empleado_id),
    cantidad INT NOT NULL DEFAULT 1 CHECK (cantidad > 0),
    precio_unitario NUMERIC(12,2) NOT NULL CHECK (precio_unitario >= 0),
    porcentaje_comision NUMERIC(5,2) NOT NULL DEFAULT 0.00,
    monto_comision NUMERIC(12,2) GENERATED ALWAYS AS (ROUND((precio_unitario * cantidad) * (porcentaje_comision / 100.0), 2)) STORED,
    subtotal NUMERIC(12,2) GENERATED ALWAYS AS (ROUND(cantidad * precio_unitario, 2)) STORED
);
CREATE INDEX IF NOT EXISTS idx_dvs_venta ON public.detalle_venta_servicio(venta_id);
CREATE INDEX IF NOT EXISTS idx_dvs_profesional ON public.detalle_venta_servicio(empleado_profesional_id);

CREATE TABLE IF NOT EXISTS public.detalle_venta_producto (
    detalle_producto_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    venta_id INT NOT NULL REFERENCES public.venta(venta_id) ON DELETE RESTRICT,
    producto_id INT NOT NULL REFERENCES public.producto(producto_id),
    empleado_vendedor_id INT NOT NULL REFERENCES public.empleado(empleado_id),
    cantidad NUMERIC(12,2) NOT NULL CHECK (cantidad > 0),
    precio_unitario NUMERIC(12,2) NOT NULL CHECK (precio_unitario >= 0),
    porcentaje_comision NUMERIC(5,2) NOT NULL DEFAULT 0.00,
    monto_comision NUMERIC(12,2) GENERATED ALWAYS AS (ROUND((precio_unitario * cantidad) * (porcentaje_comision / 100.0), 2)) STORED,
    subtotal NUMERIC(12,2) GENERATED ALWAYS AS (ROUND(cantidad * precio_unitario, 2)) STORED
);
CREATE INDEX IF NOT EXISTS idx_dvp_venta ON public.detalle_venta_producto(venta_id);
CREATE INDEX IF NOT EXISTS idx_dvp_producto ON public.detalle_venta_producto(producto_id);
CREATE INDEX IF NOT EXISTS idx_dvp_vendedor ON public.detalle_venta_producto(empleado_vendedor_id);

-- -----------------------------------------------------------------------------
-- 9. VISTAS ANALÍTICAS Y DE SALDOS
-- -----------------------------------------------------------------------------
CREATE OR REPLACE VIEW public.vw_inventario_actual AS
SELECT 
    p.producto_id,
    p.codigo,
    p.nombre AS producto,
    cp.nombre AS categoria,
    p.precio_costo,
    p.precio_venta,
    p.existencia_minima,
    p.es_insumo_interno,
    COALESCE(compras.total_comprado, 0) - COALESCE(ventas.total_vendido, 0) AS existencia_actual,
    CASE 
        WHEN (COALESCE(compras.total_comprado, 0) - COALESCE(ventas.total_vendido, 0)) <= p.existencia_minima THEN TRUE
        ELSE FALSE
    END AS alerta_bajo_minimo
FROM public.producto p
JOIN public.categoria_producto cp ON p.categoria_producto_id = cp.categoria_producto_id
LEFT JOIN (
    SELECT dc.producto_id, SUM(dc.cantidad) AS total_comprado
    FROM public.detalle_compra dc
    JOIN public.compra c ON dc.compra_id = c.compra_id
    WHERE c.estado = 'EMITIDA'
    GROUP BY dc.producto_id
) compras ON p.producto_id = compras.producto_id
LEFT JOIN (
    SELECT dvp.producto_id, SUM(dvp.cantidad) AS total_vendido
    FROM public.detalle_venta_producto dvp
    JOIN public.venta v ON dvp.venta_id = v.venta_id
    WHERE v.estado = 'EMITIDA'
    GROUP BY dvp.producto_id
) ventas ON p.producto_id = ventas.producto_id
WHERE p.activo = TRUE;

CREATE OR REPLACE VIEW public.vw_resumen_comisiones_diarias AS
WITH comisiones_unificadas AS (
    SELECT 
        dvs.empleado_profesional_id AS empleado_id,
        DATE(v.fecha_venta) AS fecha,
        dvs.monto_comision AS comision_servicios,
        0.00::numeric AS comision_productos
    FROM public.detalle_venta_servicio dvs
    JOIN public.venta v ON dvs.venta_id = v.venta_id
    WHERE v.estado = 'EMITIDA'
    
    UNION ALL
    
    SELECT 
        dvp.empleado_vendedor_id AS empleado_id,
        DATE(v.fecha_venta) AS fecha,
        0.00::numeric AS comision_servicios,
        dvp.monto_comision AS comision_productos
    FROM public.detalle_venta_producto dvp
    JOIN public.venta v ON dvp.venta_id = v.venta_id
    WHERE v.estado = 'EMITIDA'
)
SELECT 
    e.empleado_id,
    e.nombres || ' ' || e.apellidos AS profesional,
    c.fecha,
    ROUND(SUM(c.comision_servicios), 2) AS comision_servicios,
    ROUND(SUM(c.comision_productos), 2) AS comision_productos,
    ROUND(SUM(c.comision_servicios + c.comision_productos), 2) AS total_comision
FROM comisiones_unificadas c
JOIN public.empleado e ON c.empleado_id = e.empleado_id
GROUP BY e.empleado_id, e.nombres, e.apellidos, c.fecha;

CREATE OR REPLACE VIEW public.vw_agenda_citas_completa AS
SELECT 
    c.cita_id,
    c.fecha_hora_inicio,
    c.fecha_hora_fin,
    c.estado,
    c.observaciones,
    cl.cliente_id,
    cl.nombres || ' ' || cl.apellidos AS cliente_nombre,
    cl.telefono AS cliente_telefono,
    e.empleado_id,
    e.nombres || ' ' || e.apellidos AS estilista_nombre,
    e.color_agenda,
    COALESCE(STRING_AGG(s.nombre, ', '), 'Sin servicios asignados') AS servicios_resumen,
    COALESCE(SUM(dcs.precio_estimado), 0) AS total_estimado
FROM public.cita c
JOIN public.cliente cl ON c.cliente_id = cl.cliente_id
JOIN public.empleado e ON c.empleado_id = e.empleado_id
LEFT JOIN public.detalle_cita_servicio dcs ON c.cita_id = dcs.cita_id
LEFT JOIN public.servicio s ON dcs.servicio_id = s.servicio_id
GROUP BY c.cita_id, c.fecha_hora_inicio, c.fecha_hora_fin, c.estado, c.observaciones,
         cl.cliente_id, cl.nombres, cl.apellidos, cl.telefono,
         e.empleado_id, e.nombres, e.apellidos, e.color_agenda;

