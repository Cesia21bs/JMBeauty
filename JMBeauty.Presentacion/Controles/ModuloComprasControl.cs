using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloComprasControl : UserControl
{
    private readonly string _clave;

    private List<Proveedor> _proveedores = [];
    private List<Empleado> _empleados = [];
    private List<Producto> _productos = [];

    private ComboBox _cboProveedor = null!;
    private ComboBox _cboEmpleado = null!;
    private TextBox _txtFactura = null!;
    private TextBox _txtNotas = null!;

    private ComboBox _cboProductoDetalle = null!;
    private NumericUpDown _numCantidad = null!;
    private NumericUpDown _numCosto = null!;
    private Button _btnAgregar = null!;

    private DataGridView _gridDetalles = null!;
    private readonly List<DetalleCompraFila> _detalles = [];

    private Label _lblTotal = null!;
    private Button _btnGuardarCompra = null!;

    public ModuloComprasControl(string clave)
    {
        _clave = clave;
        InicializarComponentes();
        _ = CargarCatalogosAsync();
    }

    private void InicializarComponentes()
    {
        Dock = DockStyle.Fill;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        // Cabecera de Compra
        var pnlCabecera = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.White,
            Padding = new Padding(15, 12, 15, 10)
        };

        var lblTit = new Label { Text = "📦 REGISTRO DE COMPRA / REABASTECIMIENTO DE INSUMOS", Font = TemaJMBeauty.FuenteSubtitulo, ForeColor = TemaJMBeauty.TextoOscuro, Dock = DockStyle.Top, Height = 25 };

        var lblProv = new Label { Text = "Proveedor:", Location = new Point(15, 38), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboProveedor = new ComboBox { Location = new Point(90, 35), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblFac = new Label { Text = "# Factura Prov:", Location = new Point(385, 38), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtFactura = new TextBox { Location = new Point(495, 35), Width = 160 };

        var lblEmp = new Label { Text = "Recibido por:", Location = new Point(670, 38), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboEmpleado = new ComboBox { Location = new Point(765, 35), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblNot = new Label { Text = "Notas:", Location = new Point(15, 75), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtNotas = new TextBox { Location = new Point(90, 72), Width = 885 };

        pnlCabecera.Controls.AddRange([lblTit, lblProv, _cboProveedor, lblFac, _txtFactura, lblEmp, _cboEmpleado, lblNot, _txtNotas]);

        // Panel para agregar detalle
        var pnlAgregar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = TemaJMBeauty.RosaClaro,
            Padding = new Padding(15, 12, 15, 12)
        };

        var lblPrd = new Label { Text = "Producto a abastecer:", Location = new Point(15, 20), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboProductoDetalle = new ComboBox { Location = new Point(175, 17), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblCant = new Label { Text = "Cantidad:", Location = new Point(510, 20), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numCantidad = new NumericUpDown { Location = new Point(580, 17), Width = 80, Minimum = 1, Maximum = 10000, Value = 1 };

        var lblCosto = new Label { Text = "Costo Unit (Lps.):", Location = new Point(675, 20), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numCosto = new NumericUpDown { Location = new Point(785, 17), Width = 95, DecimalPlaces = 2, Maximum = 100000m, Value = 10m };

        _btnAgregar = TemaJMBeauty.CrearBotonAccion("➕ Agregar Ítem", TemaJMBeauty.Dorado, Color.White, 125);
        _btnAgregar.Location = new Point(885, 14);
        _btnAgregar.Click += (s, e) => AgregarDetalle();

        pnlAgregar.Controls.AddRange([lblPrd, _cboProductoDetalle, lblCant, _numCantidad, lblCosto, _numCosto, _btnAgregar]);

        // Pie de Página con Total y Guardar
        var pnlPie = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = Color.White,
            Padding = new Padding(20, 12, 20, 12)
        };

        _lblTotal = new Label
        {
            Text = "TOTAL COMPRA: Lps. 0.00",
            Dock = DockStyle.Left,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.DarkGoldenrod,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _btnGuardarCompra = TemaJMBeauty.CrearBotonAccion("💾 Guardar Compra e Ingresar al Stock", TemaJMBeauty.Dorado, Color.White, 280, 40);
        _btnGuardarCompra.Dock = DockStyle.Right;
        _btnGuardarCompra.Click += async (s, e) => await GuardarCompraAsync();

        pnlPie.Controls.Add(_lblTotal);
        pnlPie.Controls.Add(_btnGuardarCompra);

        // Tabla de Detalles
        _gridDetalles = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_gridDetalles);

        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CÓDIGO", Width = 130 });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PRODUCTO / INSUMO", Width = 350 });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CANTIDAD", Width = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "COSTO UNITARIO (Lps.)", Width = 175, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SUBTOTAL (Lps.)", Width = 160, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });

        var btnQuitar = new DataGridViewButtonColumn { HeaderText = "QUITAR", Text = "❌", UseColumnTextForButtonValue = true, Width = 75 };
        _gridDetalles.Columns.Add(btnQuitar);

        _gridDetalles.CellContentClick += (s, e) =>
        {
            if (e.ColumnIndex == 5 && e.RowIndex >= 0 && e.RowIndex < _detalles.Count)
            {
                _detalles.RemoveAt(e.RowIndex);
                RefrescarDetalles();
            }
        };

        var pnlContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        pnlContenedor.Controls.Add(_gridDetalles);

        Controls.Add(pnlContenedor);
        Controls.Add(pnlPie);
        Controls.Add(pnlAgregar);
        Controls.Add(pnlCabecera);
    }

    private async Task CargarCatalogosAsync()
    {
        try
        {
            _proveedores = await CompraNegocio.ListarProveedoresAsync(_clave);
            _empleados = await EmpleadoNegocio.ListarEmpleadosAsync(_clave, soloActivos: true);
            _productos = await ProductoNegocio.ListarAsync(_clave);

            _cboProveedor.DataSource = _proveedores;
            _cboProveedor.DisplayMember = "RazonSocial";
            _cboProveedor.ValueMember = "ProveedorId";

            _cboEmpleado.DataSource = _empleados;
            _cboEmpleado.DisplayMember = "NombreCompleto";
            _cboEmpleado.ValueMember = "EmpleadoId";

            _cboProductoDetalle.DataSource = _productos;
            _cboProductoDetalle.DisplayMember = "DescripcionCombo";
            _cboProductoDetalle.ValueMember = "ProductoId";

            _cboProductoDetalle.SelectedIndexChanged += (s, e) =>
            {
                if (_cboProductoDetalle.SelectedItem is Producto p)
                {
                    _numCosto.Value = p.PrecioCosto > 0 ? p.PrecioCosto : 10m;
                }
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar catálogos de compras:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AgregarDetalle()
    {
        if (_cboProductoDetalle.SelectedItem is not Producto prd)
        {
            MessageBox.Show("Seleccione un producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cant = _numCantidad.Value;
        var costo = _numCosto.Value;

        _detalles.Add(new DetalleCompraFila
        {
            ProductoId = prd.ProductoId,
            Codigo = prd.Codigo,
            Nombre = prd.Nombre,
            Cantidad = cant,
            CostoUnitario = costo,
            Subtotal = cant * costo
        });

        RefrescarDetalles();
    }

    private void RefrescarDetalles()
    {
        _gridDetalles.Rows.Clear();
        foreach (var d in _detalles)
        {
            _gridDetalles.Rows.Add(d.Codigo, d.Nombre, d.Cantidad, d.CostoUnitario, d.Subtotal);
        }
        var total = _detalles.Sum(d => d.Subtotal);
        _lblTotal.Text = $"TOTAL COMPRA: Lps. {total:N2}";
    }

    private async Task GuardarCompraAsync()
    {
        if (_detalles.Count == 0)
        {
            MessageBox.Show("Debe agregar al menos un producto a la compra.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtFactura.Text))
        {
            MessageBox.Show("Ingrese el número de factura del proveedor.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cboProveedor.SelectedValue is not int provId || _cboEmpleado.SelectedValue is not int empId)
        {
            MessageBox.Show("Seleccione proveedor y empleado.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var compra = new Compra
        {
            ProveedorId = provId,
            EmpleadoId = empId,
            NumeroFacturaProveedor = _txtFactura.Text.Trim(),
            Notas = string.IsNullOrWhiteSpace(_txtNotas.Text) ? null : _txtNotas.Text.Trim(),
            Detalles = _detalles.Select(d => new DetalleCompra
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                CostoUnitario = d.CostoUnitario
            }).ToList()
        };

        _btnGuardarCompra.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var id = await CompraNegocio.RegistrarCompraAsync(_clave, compra);
            MessageBox.Show(
                $"¡Compra #{id} registrada con éxito!\n\nSe ha actualizado el stock de {_detalles.Count} producto(s) en PostgreSQL.",
                "Compra Registrada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            _detalles.Clear();
            _txtFactura.Clear();
            _txtNotas.Clear();
            RefrescarDetalles();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar compra:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnGuardarCompra.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private sealed class DetalleCompraFila
    {
        public int ProductoId { get; init; }
        public string Codigo { get; init; } = "";
        public string Nombre { get; init; } = "";
        public decimal Cantidad { get; init; }
        public decimal CostoUnitario { get; init; }
        public decimal Subtotal { get; init; }
    }
}

