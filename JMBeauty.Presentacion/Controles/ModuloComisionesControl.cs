using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloComisionesControl : UserControl
{
    private readonly string _clave;
    private readonly int? _soloEmpleadoId;
    private List<ComisionResumen> _comisiones = [];

    private DataGridView _grid = null!;
    private DateTimePicker _dtpFecha = null!;
    private CheckBox _chkFiltrarFecha = null!;
    private Button _btnActualizar = null!;
    private Label _lblTotalGeneral = null!;

    public ModuloComisionesControl(string clave, int? soloEmpleadoId = null)
    {
        _clave = clave;
        _soloEmpleadoId = soloEmpleadoId;
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

        _chkFiltrarFecha = new CheckBox
        {
            Text = "Filtrar por fecha específica:",
            Location = new Point(15, 20),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };
        _dtpFecha = new DateTimePicker
        {
            Location = new Point(230, 17),
            Width = 140,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today,
            Enabled = false
        };
        _chkFiltrarFecha.CheckedChanged += (s, e) => _dtpFecha.Enabled = _chkFiltrarFecha.Checked;

        _btnActualizar = TemaJMBeauty.CrearBotonAccion("🔄 Consultar Comisiones", TemaJMBeauty.Dorado, Color.White, 190);
        _btnActualizar.Location = new Point(390, 14);
        _btnActualizar.Click += async (s, e) => await CargarDatosAsync();

        _lblTotalGeneral = new Label
        {
            Text = "Total Comisiones: Lps. 0.00",
            Location = new Point(620, 19),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteSubtitulo,
            ForeColor = Color.DarkGoldenrod
        };

        pnlBarra.Controls.AddRange([_chkFiltrarFecha, _dtpFecha, _btnActualizar, _lblTotalGeneral]);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Profesional", HeaderText = "PROFESIONAL / ESTILISTA", Width = 280 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Fecha", HeaderText = "FECHA DE LIQUIDACIÓN", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ComisionServicios",
            HeaderText = "COMISIÓN SERVICIOS (Lps.)",
            Width = 200,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ComisionProductos",
            HeaderText = "COMISIÓN PRODUCTOS (Lps.)",
            Width = 200,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TotalComision",
            HeaderText = "TOTAL GANADO (Lps.)",
            Width = 180,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = TemaJMBeauty.FuenteCuerpoBold }
        });

        var pnlContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        pnlContenedor.Controls.Add(_grid);

        Controls.Add(pnlContenedor);
        Controls.Add(pnlBarra);
    }

    private async Task CargarDatosAsync()
    {
        Cursor = Cursors.WaitCursor;
        DateOnly? fecha = _chkFiltrarFecha.Checked ? DateOnly.FromDateTime(_dtpFecha.Value) : null;

        try
        {
            var todas = await VentaNegocio.ListarComisionesAsync(_clave, fecha);
            _comisiones = _soloEmpleadoId.HasValue
                ? todas.Where(c => c.EmpleadoId == _soloEmpleadoId.Value).ToList()
                : todas;

            _grid.DataSource = null;
            _grid.DataSource = _comisiones;
            var total = _comisiones.Sum(c => c.TotalComision);
            _lblTotalGeneral.Text = _soloEmpleadoId.HasValue
                ? $"Mis Comisiones: Lps. {total:N2}"
                : $"Total Comisiones: Lps. {total:N2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al consultar comisiones:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }
}

