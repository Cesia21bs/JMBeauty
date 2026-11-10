
1. **Implementar módulo completo de Empleados**:
   - Entidad `Empleado.cs` con roles y comisión.
   - CRUD completo con validaciones (RNIS, rangos de edad).
   - Formulario de edición con `LookUpEdit` de sucursales y `SpinEdit` de comisión.

2. **Implementar módulo completo de Clientes**:
   - Entidad `Cliente.cs` y `TipoCliente.cs`.
   - CRUD con validaciones de NIT y rangos de edad.
   - Formulario de edición con `LookUpEdit`.

3. **Implementar módulo de Servicios**:
   - Entidad `Servicio.cs`.
   - CRUD con validación de precio y duración.
   - Formulario de edición con `LookUpEdit` de categorías.

4. **Implementar módulo de Sucursales**:
   - Entidad `Sucursal.cs`.
   - CRUD con validaciones de teléfono y horarios.
   - Formulario de edición con `LookUpEdit` y `CheckEdit`.

5. **Implementar módulo de Compras**:
   - Entidades de compra con detalle de productos y servicios.
   - Formulario de compras con detalle en `GridControl`.

6. **Implementar módulo de Venta Diaria (Caja)**:
   - Formulario de venta con selección de empleado, cliente, servicios y productos en tiempo real.
   - Cálculos de subtotal, descuento, IVA y total.
   - Historial de ventas diarias con indicadores visuales de ganancia vs. costo.