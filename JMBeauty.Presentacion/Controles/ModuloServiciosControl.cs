using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloServiciosControl : UserControl
{
    private readonly string _clave;
    private List<Servicio> _todosLosServicios = [];
    private List<CategoriaServicio> _categorias = [];

    private DataGridView _grid = null!;
    private TextBox _txtBuscar = null!;
    private Button _btnNuevo = null!;
    private Button _btnEditar = null!;
    private Button _btnActualizar = null!;
    private Label _lblConteo = null!;

    public ModuloServiciosControl(string clave)
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
        _txtBuscar = new TextBox { Location = new Point(85, 17), Width = 280, Font = TemaJMBeauty.FuenteCuerpo };
        _txtBuscar.TextChanged += (s, e) => FiltrarServicios();

        _btnNuevo = TemaJMBeauty.CrearBotonAccion("➕ Nuevo Servicio", TemaJMBeauty.Dorado, Color.White, 140);
        _btnNuevo.Location = new Point(380, 14);
        _btnNuevo.Click += (s, e) => AbrirEdicion(null);

        _btnEditar = TemaJMBeauty.CrearBotonAccion("✏️ Editar", TemaJMBeauty.RosaPastel, Color.White, 100);
        _btnEditar.Location = new Point(530, 14);
        _btnEditar.Click += (s, e) => EditarSeleccionado();

        _btnActualizar = TemaJMBeauty.CrearBotonAccion("🔄 Actualizar", Color.DimGray, Color.White, 110);
        _btnActualizar.Location = new Point(640, 14);
        _btnActualizar.Click += async (s, e) => await CargarDatosAsync();

        _lblConteo = new Label
        {
            Text = "Cargando servicios...",
            AutoSize = true,
            Location = new Point(770, 21),
            ForeColor = Color.Gray,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlBarra.Controls.AddRange([lblBuscar, _txtBuscar, _btnNuevo, _btnEditar, _btnActualizar, _lblConteo]);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "CÓDIGO", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "SERVICIO", Width = 280 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoriaNombre", HeaderText = "CATEGORÍA", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "DuracionMinutos",
            HeaderText = "DURACIÓN (MIN)",
            Width = 140,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "PrecioBase",
            HeaderText = "TARIFA BASE (Lps.)",
            Width = 130,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Descripcion", HeaderText = "DESCRIPCIÓN / TRATAMIENTO", Width = 280 });

        var colEstado = new DataGridViewTextBoxColumn { HeaderText = "ESTADO", Width = 100 };
        _grid.Columns.Add(colEstado);

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is Servicio srv && e.ColumnIndex == 6)
            {
                e.Value = srv.Activo ? "Activo" : "Inactivo";
                e.FormattingApplied = true;
            }
        };

        _grid.DoubleClick += (s, e) => EditarSeleccionado();

        var pnlContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        pnlContenedor.Controls.Add(_grid);

        Controls.Add(pnlContenedor);
        Controls.Add(pnlBarra);
    }

    private async Task CargarDatosAsync()
    {
        Cursor = Cursors.WaitCursor;
        _lblConteo.Text = "Cargando...";

        try
        {
            _categorias = await ServicioNegocio.ListarCategoriasAsync(_clave);
            _todosLosServicios = await ServicioNegocio.ListarAsync(_clave);
            FiltrarServicios();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar servicios:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void FiltrarServicios()
    {
        var filtro = _txtBuscar.Text.Trim().ToLowerInvariant();
        var lista = string.IsNullOrWhiteSpace(filtro)
            ? _todosLosServicios
            : _todosLosServicios.Where(s =>
                s.Codigo.ToLowerInvariant().Contains(filtro) ||
                s.Nombre.ToLowerInvariant().Contains(filtro) ||
                s.CategoriaNombre.ToLowerInvariant().Contains(filtro)).ToList();

        _grid.DataSource = null;
        _grid.DataSource = lista;
        _lblConteo.Text = $"Total: {lista.Count} servicio(s)";
    }

    private void EditarSeleccionado()
    {
        if (_grid.CurrentRow?.DataBoundItem is Servicio seleccionado)
        {
            AbrirEdicion(seleccionado);
        }
    }

    private void AbrirEdicion(Servicio? servicio)
    {
        using var frm = new FormServicioEdicion(_clave, _categorias, servicio);
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            _ = CargarDatosAsync();
        }
    }
}

