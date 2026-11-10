-- =============================================================================
-- PROYECTO: JM-BEAUTY CLAROS (Academia - Salón)
-- ARCHIVO: 02_JMBeauty_PostgreSQL_DatosIniciales.sql
-- DESCRIPCIÓN: Inserción de Catálogos Semilla y Datos de Prueba
-- =============================================================================

-- 1. ROLES
INSERT INTO public.rol (nombre, descripcion, activo) VALUES
('Administrador', 'Control total administrativo y financiero', TRUE),
('Recepcionista', 'Gestión de citas, clientes y cobros en caja', TRUE),
('Estilista Senior', 'Especialista en colorimetría, corte y peinado', TRUE),
('Cosmiatra / Esteticista', 'Tratamientos faciales, pestañas y cejas', TRUE),
('Manicurista / Pedicurista', 'Cuidado de uñas, gel y acrílico', TRUE)
ON CONFLICT (nombre) DO NOTHING;

-- 2. CATEGORÍAS DE SERVICIOS
INSERT INTO public.categoria_servicio (nombre, activo) VALUES
('Colorimetría y Tintes', TRUE),
('Corte y Peinado', TRUE),
('Manicura y Pedicura', TRUE),
('Tratamientos Capilares', TRUE),
('Maquillaje y Pestañas', TRUE)
ON CONFLICT (nombre) DO NOTHING;

-- 3. SERVICIOS DEL SALÓN
INSERT INTO public.servicio (categoria_servicio_id, codigo, nombre, descripcion, duracion_minutos, precio_base, activo) VALUES
(1, 'SRV-COL-001', 'Balayage Premium + Matiz', 'Técnica de aclaración con matizado y protector plex', 180, 85.00, TRUE),
(1, 'SRV-COL-002', 'Retoque de Raíz', 'Aplicación de tinte en raíz hasta 2 cm', 90, 35.00, TRUE),
(2, 'SRV-COR-001', 'Corte Dama + Cepillado', 'Corte estilizado con lavado y secado express', 45, 20.00, TRUE),
(2, 'SRV-COR-002', 'Ondas Glamour / Peinado', 'Peinado social para eventos con fijador térmico', 60, 30.00, TRUE),
(3, 'SRV-UNA-001', 'Uñas Acrílicas Esculpidas', 'Set completo de acrílico con esmaltado semipermanente', 90, 40.00, TRUE),
(3, 'SRV-UNA-002', 'Spa Pedicure Relax', 'Exfoliación, retiro de cutícula y masaje relajante', 50, 25.00, TRUE),
(4, 'SRV-CAP-001', 'Keratina Brasileña Antifrizz', 'Alisado termoactivo progresivo hidratante', 150, 95.00, TRUE),
(5, 'SRV-PES-001', 'Lifting de Pestañas + Tinte', 'Realce natural de curvatura con nutrición de keratina', 60, 28.00, TRUE)
ON CONFLICT (codigo) DO NOTHING;

-- 4. CATEGORÍAS DE PRODUCTOS
INSERT INTO public.categoria_producto (nombre, activo) VALUES
('Cuidado Capilar (Venta)', TRUE),
('Uñas y Esmaltes (Venta)', TRUE),
('Insumos Técnicos / Tintes', TRUE),
('Herramientas de Belleza', TRUE)
ON CONFLICT (nombre) DO NOTHING;

-- 5. PRODUCTOS DE INVENTARIO
INSERT INTO public.producto (categoria_producto_id, codigo, nombre, precio_costo, precio_venta, existencia_minima, es_insumo_interno, activo) VALUES
(1, 'PRD-CAP-001', 'Shampoo Matizador Violeta 300ml', 12.00, 22.00, 5.00, FALSE, TRUE),
(1, 'PRD-CAP-002', 'Mascarilla Reparación Profunda Olaplex N3', 18.50, 34.00, 4.00, FALSE, TRUE),
(1, 'PRD-CAP-003', 'Aceite de Argán Sérum 100ml', 8.00, 16.00, 6.00, FALSE, TRUE),
(2, 'PRD-UNA-001', 'Aceite para Cutículas Rosa Nude', 3.50, 7.50, 10.00, FALSE, TRUE),
(3, 'INS-TIN-001', 'Tinte Profesional Tubo 60ml Rubio Dorado 8.3', 4.20, 0.00, 15.00, TRUE, TRUE),
(3, 'INS-OXD-001', 'Oxigenta 20 Volúmenes 1000ml', 5.00, 0.00, 8.00, TRUE, TRUE)
ON CONFLICT (codigo) DO NOTHING;

-- 6. EMPLEADOS / PROFESIONALES
INSERT INTO public.empleado (rol_id, nombres, apellidos, documento_identidad, telefono, email, porcentaje_comision_servicio, porcentaje_comision_producto, color_agenda, activo) VALUES
(1, 'Jazmín', 'Claros', '04589211-3', '+503 7890-1122', 'administracion@jmbeauty.com', 50.00, 15.00, '#D4AF37', TRUE),
(3, 'Valeria', 'Montes', '05123984-7', '+503 7122-3344', 'valeria.estilista@jmbeauty.com', 40.00, 10.00, '#D98CA6', TRUE),
(5, 'Karla', 'Gutiérrez', '06234112-9', '+503 7654-8899', 'karla.unas@jmbeauty.com', 45.00, 10.00, '#F4C2C2', TRUE),
(2, 'Sofía', 'Ramírez', '03891244-1', '+503 7234-5678', 'sofia.recepcion@jmbeauty.com', 0.00, 5.00, '#FFF8E1', TRUE)
ON CONFLICT (documento_identidad) DO NOTHING;

-- 7. CLIENTES CON FICHA TÉCNICA
INSERT INTO public.cliente (nombres, apellidos, telefono, email, direccion, historial_alergias_notas, fecha_nacimiento, activo) VALUES
('Carolina', 'Mendoza', '+503 7988-1100', 'caro.mendoza@email.com', 'Col. Escalón, Pasaje Las Rosas #4', 'Cabello poroso, decoloración previa a 9 tonos. Tono preferido: Rubio Miel.', '1994-06-15', TRUE),
('Gabriela', 'Pineda', '+503 7011-2244', 'gaby.pineda@email.com', 'Residencial Santa Teresa #12', 'Alergia leve a esmaltes con formaldehído. Prefiere tonos nudes y rosados.', '1998-11-20', TRUE),
('Beatriz', 'Hernández', '+503 7744-8833', 'beatriz.h@email.com', 'Santa Tecla, Av. San Martín #8', 'Tratamiento de queratina realizado cada 5 meses.', '1987-03-08', TRUE);

-- 8. PROVEEDORES
INSERT INTO public.proveedor (razon_social, contacto, telefono, direccion, activo) VALUES
('Distribuidora Cosmética L''Oréal & Wella', 'Marcos Estrada', '+503 2288-4455', 'Zona Industrial Merliot #22', TRUE),
('Beauty Supply Professional S.A.', 'Elena Zelaya', '+503 2277-1199', 'Boulevard de Los Héroes #450', TRUE);

-- 9. COMPRA INICIAL DE REABASTECIMIENTO (Para generar inventario disponible)
DO $$
DECLARE
    v_compra_id INT;
    v_emp_id INT;
    v_prov_id INT;
    v_prod_shampoo INT;
    v_prod_olaplex INT;
    v_prod_argan INT;
    v_prod_aceite INT;
BEGIN
    SELECT empleado_id INTO v_emp_id FROM public.empleado LIMIT 1;
    SELECT proveedor_id INTO v_prov_id FROM public.proveedor LIMIT 1;
    SELECT producto_id INTO v_prod_shampoo FROM public.producto WHERE codigo = 'PRD-CAP-001';
    SELECT producto_id INTO v_prod_olaplex FROM public.producto WHERE codigo = 'PRD-CAP-002';
    SELECT producto_id INTO v_prod_argan FROM public.producto WHERE codigo = 'PRD-CAP-003';
    SELECT producto_id INTO v_prod_aceite FROM public.producto WHERE codigo = 'PRD-UNA-001';

    IF NOT EXISTS (SELECT 1 FROM public.compra WHERE numero_factura_proveedor = 'FAC-PROV-INIT-001') THEN
        INSERT INTO public.compra (proveedor_id, empleado_id, numero_factura_proveedor, estado, notas)
        VALUES (v_prov_id, v_emp_id, 'FAC-PROV-INIT-001', 'EMITIDA', 'Stock inicial de apertura')
        RETURNING compra_id INTO v_compra_id;

        INSERT INTO public.detalle_compra (compra_id, producto_id, cantidad, costo_unitario) VALUES
        (v_compra_id, v_prod_shampoo, 20.00, 12.00),
        (v_compra_id, v_prod_olaplex, 15.00, 18.50),
        (v_compra_id, v_prod_argan, 25.00, 8.00),
        (v_compra_id, v_prod_aceite, 30.00, 3.50);
    END IF;
END $$;
