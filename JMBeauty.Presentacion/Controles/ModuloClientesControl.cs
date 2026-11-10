using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloClientesControl : UserControl
{
    private readonly string _clave;
    private List<Cliente> _todosLosClientes = [];

    private DataGridView _grid = null!;
    private TextBox _txtBuscar = null!;
    private Button _btnNuevo = null!;
    private Button _btnEditar = null!;
    private Button _btnActualizar = null!;
    private Label _lblConteo = null!;

    public ModuloClientesControl(string clave)
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
        _txtBuscar.TextChanged += (s, e) => FiltrarClientes();

        _btnNuevo = TemaJMBeauty.CrearBotonAccion("➕ Nuevo Cliente", TemaJMBeauty.Dorado, Color.White, 140);
        _btnNuevo.Location = new Point(380, 14);
        _btnNuevo.Click += (s, e) => AbrirEdicion(null);

        _btnEditar = TemaJMBeauty.CrearBotonAccion("✏️ Editar Ficha", TemaJMBeauty.RosaPastel, Color.White, 120);
        _btnEditar.Location = new Point(530, 14);
        _btnEditar.Click += (s, e) => EditarSeleccionado();

        _btnActualizar = TemaJMBeauty.CrearBotonAccion("🔄 Actualizar", Color.DimGray, Color.White, 110);
        _btnActualizar.Location = new Point(660, 14);
        _btnActualizar.Click += async (s, e) => await CargarDatosAsync();

        _lblConteo = new Label
        {
            Text = "Cargando clientes...",
            AutoSize = true,
            Location = new Point(785, 21),
            ForeColor = Color.Gray,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlBarra.Controls.AddRange([lblBuscar, _txtBuscar, _btnNuevo, _btnEditar, _btnActualizar, _lblConteo]);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NombreCompleto", HeaderText = "CLIENTE", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Telefono", HeaderText = "TELÉFONO / WHATSAPP", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "EMAIL", Width = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Direccion", HeaderText = "DIRECCIÓN", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HistorialAlergiasNotas", HeaderText = "FICHA TÉCNICA (TINTES / ALERGIAS)", Width = 320 });

        var colEstado = new DataGridViewTextBoxColumn { HeaderText = "ESTADO", Width = 90 };
        _grid.Columns.Add(colEstado);

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is Cliente cli && e.ColumnIndex == 5)
            {
                e.Value = cli.Activo ? "Activo" : "Inactivo";
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
            _todosLosClientes = await ClienteNegocio.ListarAsync(_clave);
            FiltrarClientes();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar clientes:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void FiltrarClientes()
    {
        var filtro = _txtBuscar.Text.Trim().ToLowerInvariant();
        var lista = string.IsNullOrWhiteSpace(filtro)
            ? _todosLosClientes
            : _todosLosClientes.Where(c =>
                c.NombreCompleto.ToLowerInvariant().Contains(filtro) ||
                c.Telefono.Contains(filtro) ||
                (c.HistorialAlergiasNotas != null && c.HistorialAlergiasNotas.ToLowerInvariant().Contains(filtro))).ToList();

        _grid.DataSource = null;
        _grid.DataSource = lista;
        _lblConteo.Text = $"Total: {lista.Count} cliente(s)";
    }

    private void EditarSeleccionado()
    {
        if (_grid.CurrentRow?.DataBoundItem is Cliente seleccionado)
        {
            AbrirEdicion(seleccionado);
        }
    }

    private void AbrirEdicion(Cliente? cliente)
    {
        using var frm = new FormClienteEdicion(_clave, cliente);
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            _ = CargarDatosAsync();
        }
    }
}

