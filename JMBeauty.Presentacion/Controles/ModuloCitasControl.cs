using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloCitasControl : UserControl
{
    private readonly string _clave;
    private readonly Action<Cita>? _onCobrarCita;

    private List<Cita> _todasLasCitas = [];
    private List<Cliente> _clientes = [];
    private List<Empleado> _empleados = [];
    private List<Servicio> _servicios = [];

    private DataGridView _grid = null!;
    private DateTimePicker _dtpDesde = null!;
    private DateTimePicker _dtpHasta = null!;
    private ComboBox _cboFiltroEmpleado = null!;
    private Button _btnFiltrar = null!;
    private Button _btnNuevaCita = null!;
    private Button _btnEditar = null!;
    private Button _btnCambiarEstado = null!;
    private Button _btnCobrar = null!;
    private Label _lblConteo = null!;

    public ModuloCitasControl(string clave, Action<Cita>? onCobrarCita = null)
    {
        _clave = clave;
        _onCobrarCita = onCobrarCita;

        InicializarComponentes();
        _ = CargarCatalogosYDatosAsync();
    }

    private void InicializarComponentes()
    {
        Dock = DockStyle.Fill;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        // Barra Superior
        var pnlBarra = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.White,
            Padding = new Padding(12, 14, 12, 10)
        };

        var lblDesde = new Label { Text = "Desde:", Location = new Point(12, 16), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _dtpDesde = new DateTimePicker { Location = new Point(65, 14), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

        var lblHasta = new Label { Text = "Hasta:", Location = new Point(185, 16), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _dtpHasta = new DateTimePicker { Location = new Point(235, 14), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(7) };

        var lblEmp = new Label { Text = "Estilista:", Location = new Point(355, 16), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboFiltroEmpleado = new ComboBox { Location = new Point(415, 14), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

        _btnFiltrar = TemaJMBeauty.CrearBotonAccion("🔍 Buscar", Color.DimGray, Color.White, 90);
        _btnFiltrar.Location = new Point(585, 12);
        _btnFiltrar.Click += async (s, e) => await ConsultarCitasAsync();

        _btnNuevaCita = TemaJMBeauty.CrearBotonAccion("➕ Agendar Cita", TemaJMBeauty.Dorado, Color.White, 130);
        _btnNuevaCita.Location = new Point(685, 12);
        _btnNuevaCita.Click += (s, e) => AbrirEdicion(null);

        _btnEditar = TemaJMBeauty.CrearBotonAccion("✏️ Editar", TemaJMBeauty.RosaPastel, Color.White, 90);
        _btnEditar.Location = new Point(825, 12);
        _btnEditar.Click += (s, e) => EditarSeleccionada();

        _btnCobrar = TemaJMBeauty.CrearBotonAccion("💳 Cobrar / Facturar", TemaJMBeauty.DoradoOscuro, Color.White, 150);
        _btnCobrar.Location = new Point(925, 12);
        _btnCobrar.Click += (s, e) => CobrarSeleccionada();

        pnlBarra.Controls.AddRange([
            lblDesde, _dtpDesde,
            lblHasta, _dtpHasta,
            lblEmp, _cboFiltroEmpleado,
            _btnFiltrar, _btnNuevaCita,
            _btnEditar, _btnCobrar
        ]);

        // Tabla Grid
        _grid = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CitaId", HeaderText = "# CITA", Width = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "FechaHoraInicio",
            HeaderText = "FECHA / HORA INICIO",
            Width = 160,
            DefaultCellStyle = { Format = "dd/MM/yyyy hh:mm tt" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "FechaHoraFin",
            HeaderText = "HORA FIN",
            Width = 110,
            DefaultCellStyle = { Format = "hh:mm tt" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClienteNombre", HeaderText = "CLIENTE", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ClienteTelefono", HeaderText = "TELÉFONO", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EstilistaNombre", HeaderText = "ESTILISTA", Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ServiciosResumen", HeaderText = "SERVICIOS AGENDADOS", Width = 280 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TotalEstimado",
            HeaderText = "TOTAL EST. (Lps.)",
            Width = 130,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });

        var colEstado = new DataGridViewTextBoxColumn { DataPropertyName = "Estado", HeaderText = "ESTADO", Width = 130 };
        _grid.Columns.Add(colEstado);

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is Cita cita && e.ColumnIndex == 8)
            {
                e.CellStyle.Font = TemaJMBeauty.FuenteCuerpoBold;
                switch (cita.Estado)
                {
                    case EstadoCita.PROGRAMADA:
                        e.CellStyle.ForeColor = Color.DarkGoldenrod;
                        break;
                    case EstadoCita.CONFIRMADA:
                        e.CellStyle.ForeColor = Color.DarkGreen;
                        break;
                    case EstadoCita.EN_ATENCION:
                        e.CellStyle.ForeColor = Color.Purple;
                        break;
                    case EstadoCita.FINALIZADA:
                        e.CellStyle.ForeColor = Color.Gray;
                        break;
                    case EstadoCita.CANCELADA:
                        e.CellStyle.ForeColor = Color.Red;
                        break;
                }
            }
        };

        _grid.DoubleClick += (s, e) => EditarSeleccionada();

        var pnlContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        pnlContenedor.Controls.Add(_grid);

        Controls.Add(pnlContenedor);
        Controls.Add(pnlBarra);
    }

    private async Task CargarCatalogosYDatosAsync()
    {
        try
        {
            _clientes = await ClienteNegocio.ListarAsync(_clave, soloActivos: true);
            _empleados = await EmpleadoNegocio.ListarEmpleadosAsync(_clave, soloActivos: true);
            _servicios = await ServicioNegocio.ListarAsync(_clave, soloActivos: true);

            var listaFiltro = new List<Empleado> { new() { EmpleadoId = 0, Nombres = "(Todos los estilistas)" } };
            listaFiltro.AddRange(_empleados);
            _cboFiltroEmpleado.DataSource = listaFiltro;
            _cboFiltroEmpleado.DisplayMember = "NombreCompleto";
            _cboFiltroEmpleado.ValueMember = "EmpleadoId";

            await ConsultarCitasAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar agenda:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ConsultarCitasAsync()
    {
        Cursor = Cursors.WaitCursor;
        var inicio = _dtpDesde.Value.Date;
        var fin = _dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);
        int? empId = _cboFiltroEmpleado.SelectedValue is int id && id > 0 ? id : null;

        try
        {
            _todasLasCitas = await CitaNegocio.ListarPorRangoAsync(_clave, inicio, fin, empId);
            _grid.DataSource = null;
            _grid.DataSource = _todasLasCitas;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al consultar citas:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void EditarSeleccionada()
    {
        if (_grid.CurrentRow?.DataBoundItem is Cita seleccionada)
        {
            AbrirEdicion(seleccionada);
        }
    }

    private void CobrarSeleccionada()
    {
        if (_grid.CurrentRow?.DataBoundItem is Cita seleccionada)
        {
            if (_onCobrarCita != null)
            {
                _onCobrarCita(seleccionada);
            }
            else
            {
                MessageBox.Show($"Abriendo Punto de Venta para cobrar la Cita #{seleccionada.CitaId} de {seleccionada.ClienteNombre}.", "Cobro", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void AbrirEdicion(Cita? cita)
    {
        using var frm = new FormCitaEdicion(_clave, _clientes, _empleados, _servicios, cita);
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            _ = ConsultarCitasAsync();
        }
    }
}

