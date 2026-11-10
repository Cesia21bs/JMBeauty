using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloInventarioControl : UserControl
{
    private readonly string _clave;
    private List<InventarioItem> _todoElInventario = [];

    private DataGridView _grid = null!;
    private TextBox _txtBuscar = null!;
    private CheckBox _chkSoloBajoMinimo = null!;
    private Button _btnActualizar = null!;
    private Label _lblConteo = null!;
    private Label _lblAlerta = null!;

    public ModuloInventarioControl(string clave)
    {
        _clave = clave;
        InicializarComponentes();
        _ = CargarDatosAsync();
    }

    private void InicializarComponentes()
    {
        Dock = DockStyle.Fill;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        var pnlBarra = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.White,
            Padding = new Padding(15, 12, 15, 12)
        };

        var lblBuscar = new Label { Text = "🔍 Buscar:", Location = new Point(15, 20), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtBuscar = new TextBox { Location = new Point(85, 17), Width = 260, Font = TemaJMBeauty.FuenteCuerpo };
        _txtBuscar.TextChanged += (s, e) => Filtrar();

        _chkSoloBajoMinimo = new CheckBox
        {
            Text = "⚠️ Ver solo productos con stock bajo el mínimo",
            Location = new Point(360, 18),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteCuerpoBold,
            ForeColor = Color.DarkRed
        };
        _chkSoloBajoMinimo.CheckedChanged += (s, e) => Filtrar();

        _btnActualizar = TemaJMBeauty.CrearBotonAccion("🔄 Recalcular Stock", TemaJMBeauty.Dorado, Color.White, 160);
        _btnActualizar.Location = new Point(690, 14);
        _btnActualizar.Click += async (s, e) => await CargarDatosAsync();

        _lblConteo = new Label
        {
            Text = "Calculando existencias...",
            AutoSize = true,
            Location = new Point(865, 21),
            ForeColor = Color.Gray,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlBarra.Controls.AddRange([lblBuscar, _txtBuscar, _chkSoloBajoMinimo, _btnActualizar, _lblConteo]);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "CÓDIGO", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Producto", HeaderText = "PRODUCTO / COSMÉTICO", Width = 260 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Categoria", HeaderText = "CATEGORÍA", Width = 170 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "PrecioCosto",
            HeaderText = "COSTO (Lps.)",
            Width = 110,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "PrecioVenta",
            HeaderText = "VENTA (Lps.)",
            Width = 110,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ExistenciaMinima",
            HeaderText = "STOCK MÍNIMO",
            Width = 120,
            DefaultCellStyle = { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ExistenciaActual",
            HeaderText = "STOCK ACTUAL",
            Width = 130,
            DefaultCellStyle = { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleCenter, Font = TemaJMBeauty.FuenteCuerpoBold }
        });

        var colAlerta = new DataGridViewTextBoxColumn { HeaderText = "ESTADO STOCK", Width = 150 };
        _grid.Columns.Add(colAlerta);

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is InventarioItem item)
            {
                if (item.AlertaBajoMinimo)
                {
                    _grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 240);
                    _grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }

                if (e.ColumnIndex == 7) // Estado Stock
                {
                    e.Value = item.AlertaBajoMinimo ? "⚠️ REABASTECER" : "✅ Óptimo";
                    e.FormattingApplied = true;
                }
            }
        };

        var pnlContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        pnlContenedor.Controls.Add(_grid);

        Controls.Add(pnlContenedor);
        Controls.Add(pnlBarra);
    }

    private async Task CargarDatosAsync()
    {
        Cursor = Cursors.WaitCursor;
        _lblConteo.Text = "Calculando existencias netas en PostgreSQL...";

        try
        {
            _todoElInventario = await ProductoNegocio.ListarInventarioActualAsync(_clave);
            Filtrar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar inventario:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void Filtrar()
    {
        var filtro = _txtBuscar.Text.Trim().ToLowerInvariant();
        var lista = _todoElInventario.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            lista = lista.Where(p =>
                p.Codigo.ToLowerInvariant().Contains(filtro) ||
                p.Producto.ToLowerInvariant().Contains(filtro) ||
                p.Categoria.ToLowerInvariant().Contains(filtro));
        }

        if (_chkSoloBajoMinimo.Checked)
        {
            lista = lista.Where(p => p.AlertaBajoMinimo);
        }

        var resultado = lista.ToList();
        _grid.DataSource = null;
        _grid.DataSource = resultado;
        var bajoMinimoCount = _todoElInventario.Count(i => i.AlertaBajoMinimo);
        _lblConteo.Text = $"Total: {resultado.Count} ítem(s)  |  ⚠️ Bajo mínimo: {bajoMinimoCount}";
    }
}

