using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class FormCitaEdicion : Form
{
    private readonly Cita? _original;
    private readonly List<Cliente> _clientes;
    private readonly List<Empleado> _empleados;
    private readonly List<Servicio> _serviciosDisponibles;
    private readonly string _clave;

    private ComboBox _cboCliente = null!;
    private ComboBox _cboEmpleado = null!;
    private DateTimePicker _dtpFecha = null!;
    private DateTimePicker _dtpHoraInicio = null!;
    private CheckedListBox _clbServicios = null!;
    private Label _lblDuracionTotal = null!;
    private Label _lblTotalEstimado = null!;
    private ComboBox _cboEstado = null!;
    private TextBox _txtObservaciones = null!;
    private Button _btnGuardar = null!;
    private Button _btnCancelar = null!;

    public Cita? CitaGuardada { get; private set; }

    public FormCitaEdicion(
        string clave,
        List<Cliente> clientes,
        List<Empleado> empleados,
        List<Servicio> servicios,
        Cita? original = null)
    {
        _clave = clave;
        _clientes = clientes ?? [];
        _empleados = empleados ?? [];
        _serviciosDisponibles = servicios ?? [];
        _original = original;

        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = _original is null ? "Agendar Nueva Cita — JM-BEAUTY CLAROS" : $"Editar Cita #{_original.CitaId}";
        Size = new Size(620, 680);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        var pnlTitulo = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = TemaJMBeauty.FondoEncabezado,
            Padding = new Padding(20, 14, 0, 0)
        };
        var lblTitulo = new Label
        {
            Text = _original is null ? "📅 Agendamiento de Cita en el Salón" : "✏️ Modificación de Cita",
            Font = TemaJMBeauty.FuenteSubtitulo,
            ForeColor = TemaJMBeauty.Dorado,
            AutoSize = true
        };
        pnlTitulo.Controls.Add(lblTitulo);

        var pnlForm = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25, 15, 25, 10),
            BackColor = Color.White
        };

        int y = 10;
        // Cliente
        var lblCli = new Label { Text = "Cliente:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboCliente = new ComboBox
        {
            Location = new Point(25, y + 20),
            Width = 540,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _cboCliente.DataSource = _clientes;
        _cboCliente.DisplayMember = "DescripcionCombo";
        _cboCliente.ValueMember = "ClienteId";

        y += 55;
        // Estilista
        var lblEmp = new Label { Text = "Estilista / Profesional Asignado:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboEmpleado = new ComboBox
        {
            Location = new Point(25, y + 20),
            Width = 350,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _cboEmpleado.DataSource = _empleados;
        _cboEmpleado.DisplayMember = "NombreCompleto";
        _cboEmpleado.ValueMember = "EmpleadoId";

        // Estado
        var lblEst = new Label { Text = "Estado:", Location = new Point(390, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboEstado = new ComboBox
        {
            Location = new Point(390, y + 20),
            Width = 175,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _cboEstado.DataSource = Enum.GetValues<EstadoCita>();

        y += 55;
        // Fecha y Hora
        var lblFec = new Label { Text = "Fecha de la Cita:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _dtpFecha = new DateTimePicker
        {
            Location = new Point(25, y + 20),
            Width = 260,
            Format = DateTimePickerFormat.Short,
            Font = TemaJMBeauty.FuenteCuerpo,
            MinDate = _original is null ? DateTime.Today : DateTimePicker.MinimumDateTime,
            Value = _original is null ? DateTime.Today : _original.FechaHoraInicio.Date
        };

        var lblHora = new Label { Text = "Hora de Inicio:", Location = new Point(300, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _dtpHoraInicio = new DateTimePicker
        {
            Location = new Point(300, y + 20),
            Width = 265,
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Font = TemaJMBeauty.FuenteCuerpo,
            Value = _original is null ? DateTime.Now.AddMinutes(15) : _original.FechaHoraInicio
        };

        y += 55;
        // Servicios a realizar
        var lblSrv = new Label { Text = "Seleccione los Servicios a Realizar:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _clbServicios = new CheckedListBox
        {
            Location = new Point(25, y + 20),
            Width = 540,
            Height = 135,
            CheckOnClick = true,
            Font = TemaJMBeauty.FuenteCuerpo,
            DisplayMember = "DescripcionCombo"
        };
        foreach (var srv in _serviciosDisponibles)
        {
            _clbServicios.Items.Add(srv);
        }
        _clbServicios.ItemCheck += (s, e) => BeginInvoke(ActualizarTotalesEstimados);

        y += 165;
        // Resumen calculado
        _lblDuracionTotal = new Label
        {
            Text = "⏱ Duración Total: 0 min",
            Location = new Point(25, y),
            AutoSize = true,
            ForeColor = TemaJMBeauty.TextoOscuro,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        _lblTotalEstimado = new Label
        {
            Text = "💵 Total Estimado: Lps. 0.00",
            Location = new Point(300, y),
            AutoSize = true,
            ForeColor = Color.DarkGoldenrod,
            Font = TemaJMBeauty.FuenteSubtitulo
        };

        y += 35;
        // Observaciones
        var lblObs = new Label { Text = "Observaciones / Peticiones Especiales:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtObservaciones = new TextBox { Location = new Point(25, y + 20), Width = 540, Height = 48, Multiline = true, Font = TemaJMBeauty.FuenteCuerpo };

        pnlForm.Controls.AddRange([
            lblCli, _cboCliente,
            lblEmp, _cboEmpleado,
            lblEst, _cboEstado,
            lblFec, _dtpFecha,
            lblHora, _dtpHoraInicio,
            lblSrv, _clbServicios,
            _lblDuracionTotal, _lblTotalEstimado,
            lblObs, _txtObservaciones
        ]);

        var pnlBotones = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = TemaJMBeauty.RosaClaro,
            Padding = new Padding(0, 12, 25, 0)
        };

        _btnGuardar = TemaJMBeauty.CrearBotonAccion("📅 Confirmar Cita", TemaJMBeauty.Dorado, Color.White, 160, 36);
        _btnGuardar.Location = new Point(260, 12);
        _btnGuardar.Click += async (s, e) => await GuardarAsync();

        _btnCancelar = TemaJMBeauty.CrearBotonAccion("✖ Cancelar", Color.Gray, Color.White, 120, 36);
        _btnCancelar.Location = new Point(440, 12);
        _btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

        pnlBotones.Controls.Add(_btnGuardar);
        pnlBotones.Controls.Add(_btnCancelar);

        Controls.Add(pnlForm);
        Controls.Add(pnlBotones);
        Controls.Add(pnlTitulo);
    }

    private void CargarDatos()
    {
        if (_original is null)
        {
            _cboEstado.SelectedItem = EstadoCita.PROGRAMADA;
            return;
        }

        _cboCliente.SelectedValue = _original.ClienteId;
        _cboEmpleado.SelectedValue = _original.EmpleadoId;
        _cboEstado.SelectedItem = _original.Estado;
        _dtpFecha.Value = _original.FechaHoraInicio.Date;
        _dtpHoraInicio.Value = _original.FechaHoraInicio;
        _txtObservaciones.Text = _original.Observaciones ?? "";

        // Marcar servicios
        for (int i = 0; i < _clbServicios.Items.Count; i++)
        {
            if (_clbServicios.Items[i] is Servicio s && _original.Servicios.Any(ds => ds.ServicioId == s.ServicioId))
            {
                _clbServicios.SetItemChecked(i, true);
            }
        }

        ActualizarTotalesEstimados();
    }

    private void ActualizarTotalesEstimados()
    {
        int duracionTotal = 0;
        decimal precioTotal = 0;

        foreach (var item in _clbServicios.CheckedItems)
        {
            if (item is Servicio s)
            {
                duracionTotal += s.DuracionMinutos;
                precioTotal += s.PrecioBase;
            }
        }

        _lblDuracionTotal.Text = $"⏱ Duración Total: {duracionTotal} min ({duracionTotal / 60}h {duracionTotal % 60}m)";
        _lblTotalEstimado.Text = $"💵 Total Estimado: Lps. {precioTotal:N2}";
    }

    private async Task GuardarAsync()
    {
        if (_cboCliente.SelectedValue is not int cliId || cliId <= 0)
        {
            MessageBox.Show("Debe seleccionar un cliente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cboEmpleado.SelectedValue is not int empId || empId <= 0)
        {
            MessageBox.Show("Debe seleccionar un estilista.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var serviciosSeleccionados = _clbServicios.CheckedItems.OfType<Servicio>().ToList();
        if (serviciosSeleccionados.Count == 0)
        {
            MessageBox.Show("Debe seleccionar al menos un servicio para la cita.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var duracionMinutos = serviciosSeleccionados.Sum(s => s.DuracionMinutos);
        var fecha = _dtpFecha.Value.Date;
        var hora = _dtpHoraInicio.Value.TimeOfDay;
        var inicio = fecha.Add(hora);
        var fin = inicio.AddMinutes(duracionMinutos);

        if (_original is null && inicio < DateTime.Now.AddMinutes(-2))
        {
            MessageBox.Show("No se pueden agendar citas en fechas u horas pasadas. Por favor seleccione una fecha y hora posterior a la actual.", "Fecha Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cita = new Cita
        {
            CitaId = _original?.CitaId ?? 0,
            ClienteId = cliId,
            EmpleadoId = empId,
            FechaHoraInicio = inicio,
            FechaHoraFin = fin,
            Estado = (EstadoCita)_cboEstado.SelectedItem!,
            Observaciones = string.IsNullOrWhiteSpace(_txtObservaciones.Text) ? null : _txtObservaciones.Text.Trim(),
            Servicios = serviciosSeleccionados.Select(s => new DetalleCitaServicio
            {
                ServicioId = s.ServicioId,
                DuracionMinutos = s.DuracionMinutos,
                PrecioEstimado = s.PrecioBase
            }).ToList()
        };

        _btnGuardar.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var id = await CitaNegocio.AgendarCitaAsync(_clave, cita);
            cita.CitaId = id;
            CitaGuardada = cita;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "No se pudo agendar la cita", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _btnGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}

