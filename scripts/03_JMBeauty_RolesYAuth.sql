-- Modificación de roles y empleados para autenticación
ALTER TABLE public.rol ADD COLUMN IF NOT EXISTS tipo_acceso VARCHAR(30) NOT NULL DEFAULT 'ESPECIALISTA';

UPDATE public.rol SET tipo_acceso = 'ADMINISTRADOR' WHERE lower(nombre) LIKE '%admin%';
UPDATE public.rol SET tipo_acceso = 'RECEPCIONISTA' WHERE lower(nombre) LIKE '%recep%' OR lower(nombre) LIKE '%caja%';
UPDATE public.rol SET tipo_acceso = 'ESPECIALISTA' WHERE tipo_acceso NOT IN ('ADMINISTRADOR', 'RECEPCIONISTA');

INSERT INTO public.rol (nombre, descripcion, tipo_acceso, activo)
VALUES ('Maquilladora / Maquillador', 'Especialista en maquillaje social, novias y artístico', 'ESPECIALISTA', TRUE)
ON CONFLICT (nombre) DO UPDATE SET tipo_acceso = 'ESPECIALISTA';

ALTER TABLE public.empleado ADD COLUMN IF NOT EXISTS clave_acceso VARCHAR(50) NOT NULL DEFAULT '1234';

UPDATE public.empleado SET clave_acceso = '1234' WHERE clave_acceso IS NULL OR clave_acceso = '';

