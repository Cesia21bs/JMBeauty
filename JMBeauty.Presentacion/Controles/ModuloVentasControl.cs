using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloVentasControl : UserControl
{
    private readonly string _clave;
    private Cita? _citaVinculada;

    private List<Cliente> _clientes = [];
    private List<Empleado> _empleados = [];
    private List<Servicio> _servicios = [];
    private List<Producto> _productos = [];

    // Controles de Cabecera
    private ComboBox _cboCliente = null!;
    private ComboBox _cboCajero = null!;
    private ComboBox _cboTipoPago = null!;
    private Label _lblComprobante = null!;
    private Label _lblCitaInfo = null!;

    // Controles para Agregar Ítems
    private ComboBox _cboServicios = null!;
    private ComboBox _cboProfesionalSrv = null!;
    private Button _btnAgregarServicio = null!;

    private ComboBox _cboProductos = null!;
    private ComboBox _cboVendedorPrd = null!;
    private NumericUpDown _numCantidadPrd = null!;
    private Button _btnAgregarProducto = null!;

    // Tabla de Ítems en Venta
    private DataGridView _gridItems = null!;
    private readonly List<ItemVentaFila> _itemsVenta = [];

    // Totales
    private Label _lblSubtotal = null!;
    private NumericUpDown _numDescuentoPct = null!;
    private Label _lblMontoDescuento = null!;
    private Label _lblTotalPagar = null!;
    private Label _lblTotalComisiones = null!;
    private Button _btnEmitirFactura = null!;

    public ModuloVentasControl(string clave, Cita? citaParaCobrar = null)
    {
        _clave = clave;
        _citaVinculada = citaParaCobrar;

        InicializarComponentes();
        _ = CargarDatosAsync();
    }

    private void InicializarComponentes()
    {
        Dock = DockStyle.Fill;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        // 1. Cabecera POS
        var pnlCabecera = new Panel
        {
            Dock = DockStyle.Top,
            Height = 85,
            BackColor = Color.White,
            Padding = new Padding(15, 10, 15, 10)
        };

        _lblComprobante = new Label
        {
            Text = "Ticket: Generando...",
            Location = new Point(15, 12),
            Font = TemaJMBeauty.FuenteSubtitulo,
            ForeColor = TemaJMBeauty.DoradoOscuro,
            AutoSize = true
        };

        _lblCitaInfo = new Label
        {
            Text = _citaVinculada is null ? "" : $"📌 Cobro vinculado a Cita #{_citaVinculada.CitaId}",
            Location = new Point(220, 14),
            Font = TemaJMBeauty.FuenteCuerpoBold,
            ForeColor = Color.Purple,
            AutoSize = true
        };

        var lblCli = new Label { Text = "Cliente:", Location = new Point(15, 42), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboCliente = new ComboBox { Location = new Point(75, 39), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblCaj = new Label { Text = "Cajero(a):", Location = new Point(370, 42), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboCajero = new ComboBox { Location = new Point(440, 39), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblPago = new Label { Text = "Forma de Pago:", Location = new Point(680, 42), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboTipoPago = new ComboBox { Location = new Point(790, 39), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboTipoPago.DataSource = Enum.GetValues<TipoPago>();

        pnlCabecera.Controls.AddRange([
            _lblComprobante, _lblCitaInfo,
            lblCli, _cboCliente,
            lblCaj, _cboCajero,
            lblPago, _cboTipoPago
        ]);

        // 2. Panel Izquierdo: Agregar Ítems (Servicios y Cosméticos)
        var pnlAgregar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 380,
            BackColor = Color.White,
            Padding = new Padding(15)
        };

        // Sección Servicios
        var pnlSecSrv = new Panel { Dock = DockStyle.Top, Height = 190, BackColor = TemaJMBeauty.RosaClaro, Padding = new Padding(12) };
        var lblTitSrv = new Label { Text = "💇‍♀️ AGREGAR SERVICIO AL TICKET", Font = TemaJMBeauty.FuenteCuerpoBold, ForeColor = TemaJMBeauty.TextoOscuro, Dock = DockStyle.Top, Height = 22 };

        var lblSrvNom = new Label { Text = "Servicio realizado:", Location = new Point(12, 32), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpo };
        _cboServicios = new ComboBox { Location = new Point(12, 50), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblProfSrv = new Label { Text = "Estilista que lo realizó (Comisión):", Location = new Point(12, 80), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpo };
        _cboProfesionalSrv = new ComboBox { Location = new Point(12, 100), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };

        _btnAgregarServicio = TemaJMBeauty.CrearBotonAccion("➕ Agregar Servicio", TemaJMBeauty.Dorado, Color.White, 330, 34);
        _btnAgregarServicio.Location = new Point(12, 138);
        _btnAgregarServicio.Click += (s, e) => AgregarServicioATicket();

        pnlSecSrv.Controls.AddRange([lblTitSrv, lblSrvNom, _cboServicios, lblProfSrv, _cboProfesionalSrv, _btnAgregarServicio]);

        // Separador
        var pnlSep = new Panel { Dock = DockStyle.Top, Height = 15 };

        // Sección Productos
        var pnlSecPrd = new Panel { Dock = DockStyle.Top, Height = 220, BackColor = Color.FromArgb(255, 252, 245), Padding = new Padding(12) };
        var lblTitPrd = new Label { Text = "🧴 VENTA DE PRODUCTO / COSMÉTICO", Font = TemaJMBeauty.FuenteCuerpoBold, ForeColor = TemaJMBeauty.TextoOscuro, Dock = DockStyle.Top, Height = 22 };

        var lblPrdNom = new Label { Text = "Producto:", Location = new Point(12, 32), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpo };
        _cboProductos = new ComboBox { Location = new Point(12, 50), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblVendPrd = new Label { Text = "Vendedor / Recomendado por:", Location = new Point(12, 80), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpo };
        _cboVendedorPrd = new ComboBox { Location = new Point(12, 100), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblCant = new Label { Text = "Cant:", Location = new Point(245, 80), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpo };
        _numCantidadPrd = new NumericUpDown { Location = new Point(245, 100), Width = 95, Minimum = 1, Maximum = 100, Value = 1, Font = TemaJMBeauty.FuenteCuerpo };

        _btnAgregarProducto = TemaJMBeauty.CrearBotonAccion("➕ Agregar Cosmético", TemaJMBeauty.RosaPastel, Color.White, 330, 34);
        _btnAgregarProducto.Location = new Point(12, 145);
        _btnAgregarProducto.Click += (s, e) => AgregarProductoATicket();

        pnlSecPrd.Controls.AddRange([lblTitPrd, lblPrdNom, _cboProductos, lblVendPrd, _cboVendedorPrd, lblCant, _numCantidadPrd, _btnAgregarProducto]);

        pnlAgregar.Controls.Add(pnlSecPrd);
        pnlAgregar.Controls.Add(pnlSep);
        pnlAgregar.Controls.Add(pnlSecSrv);

        // 3. Panel Derecho: Totales y Cobro
        var pnlTotales = new Panel
        {
            Dock = DockStyle.Right,
            Width = 320,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        var lblTitTot = new Label { Text = "RESUMEN DE CUENTA", Font = TemaJMBeauty.FuenteSubtitulo, ForeColor = TemaJMBeauty.TextoOscuro, Dock = DockStyle.Top, Height = 28 };

        _lblSubtotal = new Label { Text = "Subtotal: Lps. 0.00", Font = TemaJMBeauty.FuenteCuerpoBold, ForeColor = Color.Gray, Location = new Point(20, 50), AutoSize = true };

        var lblDesc = new Label { Text = "Descuento (%):", Location = new Point(20, 85), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numDescuentoPct = new NumericUpDown { Location = new Point(140, 82), Width = 100, DecimalPlaces = 0, Maximum = 100, Value = 0 };
        _numDescuentoPct.ValueChanged += (s, e) => RecalcularTotales();

        _lblMontoDescuento = new Label { Text = "Ahorro: Lps. 0.00", Location = new Point(20, 115), AutoSize = true, ForeColor = Color.DarkRed, Font = TemaJMBeauty.FuenteCuerpo };

        _lblTotalComisiones = new Label { Text = "Comisiones personal: Lps. 0.00", Location = new Point(20, 150), AutoSize = true, ForeColor = Color.DimGray, Font = TemaJMBeauty.FuenteCuerpo };

        var pnlCajaTotal = new Panel
        {
            Location = new Point(15, 195),
            Width = 285,
            Height = 90,
            BackColor = TemaJMBeauty.CremaClaro,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };
        var lblTotalLabel = new Label { Text = "TOTAL A COBRAR:", Dock = DockStyle.Top, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = TemaJMBeauty.TextoOscuro };
        _lblTotalPagar = new Label
        {
            Text = "Lps. 0.00",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.DarkGoldenrod
        };
        pnlCajaTotal.Controls.Add(_lblTotalPagar);
        pnlCajaTotal.Controls.Add(lblTotalLabel);

        _btnEmitirFactura = TemaJMBeauty.CrearBotonAccion("💰 REGISTRAR COBRO", TemaJMBeauty.Dorado, Color.White, 285, 48);
        _btnEmitirFactura.Location = new Point(15, 305);
        _btnEmitirFactura.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _btnEmitirFactura.Click += async (s, e) => await RegistrarCobroAsync();

        pnlTotales.Controls.AddRange([
            lblTitTot, _lblSubtotal,
            lblDesc, _numDescuentoPct,
            _lblMontoDescuento, _lblTotalComisiones,
            pnlCajaTotal, _btnEmitirFactura
        ]);

        // 4. Panel Central: Grid de Ítems en Venta
        var pnlCentro = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            BackColor = TemaJMBeauty.CremaClaro
        };

        _gridItems = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_gridItems);

        _gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TIPO", Width = 90 });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DESCRIPCIÓN DEL ÍTEM", Width = 230 });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PROFESIONAL / COMISIONA", Width = 180 });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CANT", Width = 65, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PRECIO (Lps.)", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "COMISIÓN (Lps.)", Width = 120, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SUBTOTAL (Lps.)", Width = 120, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });

        // Botón Eliminar fila
        var btnColEliminar = new DataGridViewButtonColumn
        {
            HeaderText = "QUITAR",
            Text = "❌",
            UseColumnTextForButtonValue = true,
            Width = 70
        };
        _gridItems.Columns.Add(btnColEliminar);

        _gridItems.CellContentClick += (s, e) =>
        {
            if (e.ColumnIndex == 7 && e.RowIndex >= 0 && e.RowIndex < _itemsVenta.Count)
            {
                _itemsVenta.RemoveAt(e.RowIndex);
                RefrescarGridItems();
            }
        };

        pnlCentro.Controls.Add(_gridItems);

        Controls.Add(pnlCentro);
        Controls.Add(pnlTotales);
        Controls.Add(pnlAgregar);
        Controls.Add(pnlCabecera);
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            _clientes = await ClienteNegocio.ListarAsync(_clave, soloActivos: true);
            _empleados = await EmpleadoNegocio.ListarEmpleadosAsync(_clave, soloActivos: true);
            _servicios = await ServicioNegocio.ListarAsync(_clave, soloActivos: true);
            _productos = await ProductoNegocio.ListarAsync(_clave, soloActivos: true);

            _cboCliente.DataSource = _clientes;
            _cboCliente.DisplayMember = "NombreCompleto";
            _cboCliente.ValueMember = "ClienteId";

            _cboCajero.DataSource = _empleados;
            _cboCajero.DisplayMember = "NombreCompleto";
            _cboCajero.ValueMember = "EmpleadoId";

            _cboServicios.DataSource = _servicios;
            _cboServicios.DisplayMember = "DescripcionCombo";
            _cboServicios.ValueMember = "ServicioId";

            var estilistas = _empleados.Where(e => e.PorcentajeComisionServicio > 0 || e.RolNombre.Contains("Estilista") || e.RolNombre.Contains("Senior")).ToList();
            if (estilistas.Count == 0) estilistas = _empleados;
            _cboProfesionalSrv.DataSource = estilistas;
            _cboProfesionalSrv.DisplayMember = "NombreCompleto";
            _cboProfesionalSrv.ValueMember = "EmpleadoId";

            var prodsVenta = _productos.Where(p => !p.EsInsumoInterno).ToList();
            _cboProductos.DataSource = prodsVenta.Count > 0 ? prodsVenta : _productos;
            _cboProductos.DisplayMember = "DescripcionCombo";
            _cboProductos.ValueMember = "ProductoId";

            _cboVendedorPrd.DataSource = new List<Empleado>(_empleados);
            _cboVendedorPrd.DisplayMember = "NombreCompleto";
            _cboVendedorPrd.ValueMember = "EmpleadoId";

            var numComprobante = await VentaNegocio.GenerarNumeroComprobanteAsync(_clave);
            _lblComprobante.Text = $"Ticket: {numComprobante}";

            // Si venimos de una cita, precargar cliente y servicios
            if (_citaVinculada is not null)
            {
                _cboCliente.SelectedValue = _citaVinculada.ClienteId;
                _cboProfesionalSrv.SelectedValue = _citaVinculada.EmpleadoId;

                // Si la cita tenía servicios asignados
                if (_citaVinculada.Servicios.Count > 0)
                {
                    foreach (var srv in _citaVinculada.Servicios)
                    {
                        var srvReal = _servicios.FirstOrDefault(s => s.ServicioId == srv.ServicioId);
                        var empReal = _empleados.FirstOrDefault(e => e.EmpleadoId == _citaVinculada.EmpleadoId);
                        if (srvReal != null && empReal != null)
                        {
                            var comision = Math.Round(srv.PrecioEstimado * (empReal.PorcentajeComisionServicio / 100m), 2);
                            _itemsVenta.Add(new ItemVentaFila
                            {
                                EsServicio = true,
                                Id = srvReal.ServicioId,
                                Descripcion = srvReal.Nombre,
                                EmpleadoId = empReal.EmpleadoId,
                                EmpleadoNombre = empReal.NombreCompleto,
                                Cantidad = 1,
                                PrecioUnitario = srv.PrecioEstimado,
                                PorcentajeComision = empReal.PorcentajeComisionServicio,
                                MontoComision = comision,
                                Subtotal = srv.PrecioEstimado
                            });
                        }
                    }
                }
                else
                {
                    // Fallback a servicios texto
                    var srvDefault = _servicios.FirstOrDefault();
                    var empReal = _empleados.FirstOrDefault(e => e.EmpleadoId == _citaVinculada.EmpleadoId);
                    if (srvDefault != null && empReal != null)
                    {
                        var comision = Math.Round(_citaVinculada.TotalEstimado * (empReal.PorcentajeComisionServicio / 100m), 2);
                        _itemsVenta.Add(new ItemVentaFila
                        {
                            EsServicio = true,
                            Id = srvDefault.ServicioId,
                            Descripcion = _citaVinculada.ServiciosResumen,
                            EmpleadoId = empReal.EmpleadoId,
                            EmpleadoNombre = empReal.NombreCompleto,
                            Cantidad = 1,
                            PrecioUnitario = _citaVinculada.TotalEstimado > 0 ? _citaVinculada.TotalEstimado : srvDefault.PrecioBase,
                            PorcentajeComision = empReal.PorcentajeComisionServicio,
                            MontoComision = comision,
                            Subtotal = _citaVinculada.TotalEstimado > 0 ? _citaVinculada.TotalEstimado : srvDefault.PrecioBase
                        });
                    }
                }

                RefrescarGridItems();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar catálogos de venta:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AgregarServicioATicket()
    {
        if (_cboServicios.SelectedItem is not Servicio srv || _cboProfesionalSrv.SelectedItem is not Empleado emp)
        {
            MessageBox.Show("Seleccione un servicio y un estilista.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var comision = Math.Round(srv.PrecioBase * (emp.PorcentajeComisionServicio / 100m), 2);

        _itemsVenta.Add(new ItemVentaFila
        {
            EsServicio = true,
            Id = srv.ServicioId,
            Descripcion = srv.Nombre,
            EmpleadoId = emp.EmpleadoId,
            EmpleadoNombre = emp.NombreCompleto,
            Cantidad = 1,
            PrecioUnitario = srv.PrecioBase,
            PorcentajeComision = emp.PorcentajeComisionServicio,
            MontoComision = comision,
            Subtotal = srv.PrecioBase
        });

        RefrescarGridItems();
    }

    private void AgregarProductoATicket()
    {
        if (_cboProductos.SelectedItem is not Producto prd || _cboVendedorPrd.SelectedItem is not Empleado emp)
        {
            MessageBox.Show("Seleccione un producto y un vendedor.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cant = (int)_numCantidadPrd.Value;
        var subtotal = prd.PrecioVenta * cant;
        var comision = Math.Round(subtotal * (emp.PorcentajeComisionProducto / 100m), 2);

        _itemsVenta.Add(new ItemVentaFila
        {
            EsServicio = false,
            Id = prd.ProductoId,
            Descripcion = prd.Nombre,
            EmpleadoId = emp.EmpleadoId,
            EmpleadoNombre = emp.NombreCompleto,
            Cantidad = cant,
            PrecioUnitario = prd.PrecioVenta,
            PorcentajeComision = emp.PorcentajeComisionProducto,
            MontoComision = comision,
            Subtotal = subtotal
        });

        RefrescarGridItems();
    }

    private void RefrescarGridItems()
    {
        _gridItems.Rows.Clear();
        foreach (var it in _itemsVenta)
        {
            _gridItems.Rows.Add(
                it.EsServicio ? "Servicio" : "Producto",
                it.Descripcion,
                it.EmpleadoNombre,
                it.Cantidad,
                it.PrecioUnitario,
                it.MontoComision,
                it.Subtotal
            );
        }
        RecalcularTotales();
    }

    private void RecalcularTotales()
    {
        decimal subtotal = _itemsVenta.Sum(i => i.Subtotal);
        decimal comisiones = _itemsVenta.Sum(i => i.MontoComision);
        decimal pctDesc = _numDescuentoPct.Value;
        decimal montoDesc = Math.Round(subtotal * (pctDesc / 100m), 2);
        decimal total = Math.Max(0, subtotal - montoDesc);

        _lblSubtotal.Text = $"Subtotal: Lps. {subtotal:N2}";
        _lblMontoDescuento.Text = $"Ahorro aplicado: -Lps. {montoDesc:N2}";
        _lblTotalComisiones.Text = $"Comisiones personal: Lps. {comisiones:N2}";
        _lblTotalPagar.Text = $"Lps. {total:N2}";
    }

    private async Task RegistrarCobroAsync()
    {
        if (_itemsVenta.Count == 0)
        {
            MessageBox.Show("No hay servicios ni productos en el ticket para cobrar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cboCliente.SelectedValue is not int cliId || _cboCajero.SelectedValue is not int cajId)
        {
            MessageBox.Show("Seleccione un cliente y cajero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal subtotal = _itemsVenta.Sum(i => i.Subtotal);
        decimal pctDesc = _numDescuentoPct.Value;
        decimal montoDesc = Math.Round(subtotal * (pctDesc / 100m), 2);
        decimal total = Math.Max(0, subtotal - montoDesc);

        var venta = new Venta
        {
            ClienteId = cliId,
            EmpleadoCajeroId = cajId,
            CitaId = _citaVinculada?.CitaId,
            TipoPago = (TipoPago)_cboTipoPago.SelectedItem!,
            Estado = EstadoDocumento.EMITIDA,
            PorcentajeDescuento = pctDesc,
            MontoDescuento = montoDesc,
            MontoTotal = total,
            Servicios = _itemsVenta.Where(i => i.EsServicio).Select(i => new DetalleVentaServicio
            {
                ServicioId = i.Id,
                EmpleadoProfesionalId = i.EmpleadoId,
                Cantidad = (int)i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                PorcentajeComision = i.PorcentajeComision
            }).ToList(),
            Productos = _itemsVenta.Where(i => !i.EsServicio).Select(i => new DetalleVentaProducto
            {
                ProductoId = i.Id,
                EmpleadoVendedorId = i.EmpleadoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                PorcentajeComision = i.PorcentajeComision
            }).ToList()
        };

        _btnEmitirFactura.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var ventaId = await VentaNegocio.RegistrarVentaAsync(_clave, venta);
            MessageBox.Show(
                $"¡Venta #{ventaId} registrada con éxito!\n\nComprobante: {venta.NumeroComprobante}\nTotal cobrado: Lps. {total:N2}\n\nLas existencias de productos y comisiones han sido actualizadas.",
                "Cobro Exitoso — JM-BEAUTY CLAROS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Limpiar para nueva venta
            _itemsVenta.Clear();
            _citaVinculada = null;
            _lblCitaInfo.Text = "";
            RefrescarGridItems();
            _numDescuentoPct.Value = 0;
            var nuevoComprobante = await VentaNegocio.GenerarNumeroComprobanteAsync(_clave);
            _lblComprobante.Text = $"Ticket: {nuevoComprobante}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al registrar cobro:\n\n{ex.Message}", "Error en Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnEmitirFactura.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private sealed class ItemVentaFila
    {
        public bool EsServicio { get; init; }
        public int Id { get; init; }
        public string Descripcion { get; init; } = "";
        public int EmpleadoId { get; init; }
        public string EmpleadoNombre { get; init; } = "";
        public decimal Cantidad { get; init; }
        public decimal PrecioUnitario { get; init; }
        public decimal PorcentajeComision { get; init; }
        public decimal MontoComision { get; init; }
        public decimal Subtotal { get; init; }
    }
}

