using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class FormEmpleadoEdicion : Form
{
    private readonly Empleado? _original;
    private readonly List<Rol> _roles;
    private readonly string _clave;

    private ComboBox _cboRol = null!;
    private Button _btnNuevoRol = null!;
    private TextBox _txtNombres = null!;
    private TextBox _txtApellidos = null!;
    private TextBox _txtDocumento = null!;
    private TextBox _txtTelefono = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtClaveAcceso = null!;
    private NumericUpDown _numComisionServicios = null!;
    private NumericUpDown _numComisionProductos = null!;
    private Panel _pnlColorAgenda = null!;
    private CheckBox _chkActivo = null!;
    private Button _btnGuardar = null!;
    private Button _btnCancelar = null!;

    public Empleado? EmpleadoGuardado { get; private set; }

    public FormEmpleadoEdicion(string clave, List<Rol> roles, Empleado? original = null)
    {
        _clave = clave;
        _roles = roles ?? [];
        _original = original;

        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = _original is null ? "Nuevo Empleado / Profesional — JM-BEAUTY CLAROS" : $"Editar Empleado [{_original.NombreCompleto}]";
        Size = new Size(580, 620);
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
            Text = _original is null ? "👑 Registro de Empleado / Profesional" : "✏️ Modificación de Empleado y Comisiones",
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
        // Rol
        var lblRol = new Label { Text = "Rol / Cargo:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboRol = new ComboBox
        {
            Location = new Point(25, y + 20),
            Width = 370,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _cboRol.DataSource = _roles;
        _cboRol.DisplayMember = "Nombre";
        _cboRol.ValueMember = "RolId";

        _btnNuevoRol = TemaJMBeauty.CrearBotonAccion("➕ Nuevo Rol", TemaJMBeauty.Dorado, Color.White, 125, 28);
        _btnNuevoRol.Location = new Point(410, y + 19);
        _btnNuevoRol.Click += (s, e) =>
        {
            using var frm = new FormRolEdicion(_clave);
            if (frm.ShowDialog(this) == DialogResult.OK && frm.RolCreado is not null)
            {
                _roles.Add(frm.RolCreado);
                _cboRol.DataSource = null;
                _cboRol.DataSource = _roles;
                _cboRol.DisplayMember = "Nombre";
                _cboRol.ValueMember = "RolId";
                _cboRol.SelectedValue = frm.RolCreado.RolId;
            }
        };

        y += 55;
        // Nombres y Apellidos
        var lblNom = new Label { Text = "Nombres:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtNombres = new TextBox { Location = new Point(25, y + 20), Width = 245, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloLetras(_txtNombres);

        var lblApe = new Label { Text = "Apellidos:", Location = new Point(285, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtApellidos = new TextBox { Location = new Point(285, y + 20), Width = 250, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloLetras(_txtApellidos);

        y += 55;
        // Documento y Teléfono
        var lblDoc = new Label { Text = "Documento de Identidad (DUI / Cédula):", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtDocumento = new TextBox { Location = new Point(25, y + 20), Width = 245, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloNumeros(_txtDocumento, permitirGuion: true);

        var lblTel = new Label { Text = "Teléfono de Contacto:", Location = new Point(285, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtTelefono = new TextBox { Location = new Point(285, y + 20), Width = 250, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloNumeros(_txtTelefono, permitirGuion: true);

        y += 55;
        // Email y Clave/PIN de Acceso al Login
        var lblEmail = new Label { Text = "Correo Electrónico:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtEmail = new TextBox { Location = new Point(25, y + 20), Width = 285, Font = TemaJMBeauty.FuenteCuerpo };

        var lblClave = new Label { Text = "Contraseña / PIN de Login:", Location = new Point(325, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtClaveAcceso = new TextBox { Location = new Point(325, y + 20), Width = 210, Font = TemaJMBeauty.FuenteCuerpo, Text = "1234" };

        y += 55;
        // Comisiones %
        var lblComS = new Label { Text = "Comisión por Servicios (%):", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numComisionServicios = new NumericUpDown
        {
            Location = new Point(25, y + 20),
            Width = 160,
            DecimalPlaces = 2,
            Maximum = 100m,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        var lblComP = new Label { Text = "Comisión por Productos (%):", Location = new Point(200, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numComisionProductos = new NumericUpDown
        {
            Location = new Point(200, y + 20),
            Width = 160,
            DecimalPlaces = 2,
            Maximum = 100m,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        // Color Agenda
        var lblColor = new Label { Text = "Color en Agenda:", Location = new Point(375, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _pnlColorAgenda = new Panel
        {
            Location = new Point(375, y + 20),
            Width = 160,
            Height = 24,
            BackColor = ColorTranslator.FromHtml("#D4AF37"),
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        _pnlColorAgenda.Click += (s, e) =>
        {
            using var cd = new ColorDialog { Color = _pnlColorAgenda.BackColor };
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                _pnlColorAgenda.BackColor = cd.Color;
            }
        };

        y += 65;
        _chkActivo = new CheckBox
        {
            Text = "Empleado Activo en el Salón",
            Location = new Point(25, y),
            AutoSize = true,
            Checked = true,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlForm.Controls.AddRange([
            lblRol, _cboRol, _btnNuevoRol,
            lblNom, _txtNombres,
            lblApe, _txtApellidos,
            lblDoc, _txtDocumento,
            lblTel, _txtTelefono,
            lblEmail, _txtEmail,
            lblClave, _txtClaveAcceso,
            lblComS, _numComisionServicios,
            lblComP, _numComisionProductos,
            lblColor, _pnlColorAgenda,
            _chkActivo
        ]);

        var pnlBotones = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = TemaJMBeauty.RosaClaro,
            Padding = new Padding(0, 12, 25, 0)
        };

        _btnGuardar = TemaJMBeauty.CrearBotonAccion("💾 Guardar", TemaJMBeauty.Dorado, Color.White, 130, 36);
        _btnGuardar.Location = new Point(285, 12);
        _btnGuardar.Click += async (s, e) => await GuardarAsync();

        _btnCancelar = TemaJMBeauty.CrearBotonAccion("✖ Cancelar", Color.Gray, Color.White, 120, 36);
        _btnCancelar.Location = new Point(425, 12);
        _btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

        pnlBotones.Controls.Add(_btnGuardar);
        pnlBotones.Controls.Add(_btnCancelar);

        Controls.Add(pnlForm);
        Controls.Add(pnlBotones);
        Controls.Add(pnlTitulo);
    }

    private void CargarDatos()
    {
        if (_original is null) return;
        _cboRol.SelectedValue = _original.RolId;
        _txtNombres.Text = _original.Nombres;
        _txtApellidos.Text = _original.Apellidos;
        _txtDocumento.Text = _original.DocumentoIdentidad;
        _txtTelefono.Text = _original.Telefono;
        _txtEmail.Text = _original.Email ?? "";
        _txtClaveAcceso.Text = _original.ClaveAcceso;
        _numComisionServicios.Value = _original.PorcentajeComisionServicio;
        _numComisionProductos.Value = _original.PorcentajeComisionProducto;
        try { _pnlColorAgenda.BackColor = ColorTranslator.FromHtml(_original.ColorAgenda); } catch { }
        _chkActivo.Checked = _original.Activo;
    }

    private async Task GuardarAsync()
    {
        if (_cboRol.SelectedValue is not int rolId || rolId <= 0)
        {
            MessageBox.Show("Por favor seleccione un rol válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var empleado = new Empleado
        {
            EmpleadoId = _original?.EmpleadoId ?? 0,
            RolId = rolId,
            Nombres = _txtNombres.Text.Trim(),
            Apellidos = _txtApellidos.Text.Trim(),
            DocumentoIdentidad = _txtDocumento.Text.Trim(),
            Telefono = _txtTelefono.Text.Trim(),
            Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
            ClaveAcceso = string.IsNullOrWhiteSpace(_txtClaveAcceso.Text) ? "1234" : _txtClaveAcceso.Text.Trim(),
            PorcentajeComisionServicio = _numComisionServicios.Value,
            PorcentajeComisionProducto = _numComisionProductos.Value,
            ColorAgenda = $"#{_pnlColorAgenda.BackColor.R:X2}{_pnlColorAgenda.BackColor.G:X2}{_pnlColorAgenda.BackColor.B:X2}",
            Activo = _chkActivo.Checked
        };

        _btnGuardar.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var id = await EmpleadoNegocio.GuardarAsync(_clave, empleado, _original);
            empleado.EmpleadoId = id;
            EmpleadoGuardado = empleado;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error al guardar empleado", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}

