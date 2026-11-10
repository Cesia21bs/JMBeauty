using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class FormClienteEdicion : Form
{
    private readonly Cliente? _original;
    private readonly string _clave;

    private TextBox _txtNombres = null!;
    private TextBox _txtApellidos = null!;
    private TextBox _txtTelefono = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtDireccion = null!;
    private DateTimePicker _dtpNacimiento = null!;
    private CheckBox _chkTieneNacimiento = null!;
    private TextBox _txtFichaTecnica = null!;
    private CheckBox _chkActivo = null!;
    private Button _btnGuardar = null!;
    private Button _btnCancelar = null!;

    public Cliente? ClienteGuardado { get; private set; }

    public FormClienteEdicion(string clave, Cliente? original = null)
    {
        _clave = clave;
        _original = original;

        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = _original is null ? "Nuevo Cliente — JM-BEAUTY CLAROS" : $"Editar Cliente [{_original.NombreCompleto}]";
        Size = new Size(620, 640);
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
            Text = _original is null ? "✨ Registro de Cliente y Ficha de Belleza" : "✏️ Modificación de Cliente y Ficha Técnica",
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
        // Nombres y Apellidos
        var lblNom = new Label { Text = "Nombres:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtNombres = new TextBox { Location = new Point(25, y + 20), Width = 260, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloLetras(_txtNombres);

        var lblApe = new Label { Text = "Apellidos:", Location = new Point(300, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtApellidos = new TextBox { Location = new Point(300, y + 20), Width = 265, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloLetras(_txtApellidos);

        y += 55;
        // Teléfono y Email
        var lblTel = new Label { Text = "Teléfono / WhatsApp:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtTelefono = new TextBox { Location = new Point(25, y + 20), Width = 260, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloNumeros(_txtTelefono, permitirGuion: true);

        var lblEmail = new Label { Text = "Correo Electrónico:", Location = new Point(300, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtEmail = new TextBox { Location = new Point(300, y + 20), Width = 265, Font = TemaJMBeauty.FuenteCuerpo };

        y += 55;
        // Dirección
        var lblDir = new Label { Text = "Dirección de Domicilio:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtDireccion = new TextBox { Location = new Point(25, y + 20), Width = 540, Font = TemaJMBeauty.FuenteCuerpo };

        y += 55;
        // Cumpleaños
        _chkTieneNacimiento = new CheckBox { Text = "Registrar Fecha de Nacimiento (Para promociones):", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _dtpNacimiento = new DateTimePicker
        {
            Location = new Point(25, y + 22),
            Width = 260,
            Format = DateTimePickerFormat.Short,
            Enabled = false,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _chkTieneNacimiento.CheckedChanged += (s, e) => _dtpNacimiento.Enabled = _chkTieneNacimiento.Checked;

        y += 60;
        // Ficha Técnica de Belleza (Alergias, Tintes, Fórmulas)
        var pnlFichaEnc = new Panel { Location = new Point(25, y), Width = 540, Height = 28, BackColor = TemaJMBeauty.RosaClaro, Padding = new Padding(10, 5, 0, 0) };
        var lblFicha = new Label
        {
            Text = "📋 FICHA TÉCNICA (Fórmulas de tinte, alergias, tipo de piel/cabello, historial)",
            Font = TemaJMBeauty.FuenteCuerpoBold,
            ForeColor = TemaJMBeauty.TextoOscuro,
            AutoSize = true
        };
        pnlFichaEnc.Controls.Add(lblFicha);

        _txtFichaTecnica = new TextBox
        {
            Location = new Point(25, y + 30),
            Width = 540,
            Height = 110,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            BackColor = Color.FromArgb(255, 253, 248)
        };

        y += 150;
        _chkActivo = new CheckBox
        {
            Text = "Cliente Activo",
            Location = new Point(25, y),
            AutoSize = true,
            Checked = true,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlForm.Controls.AddRange([
            lblNom, _txtNombres,
            lblApe, _txtApellidos,
            lblTel, _txtTelefono,
            lblEmail, _txtEmail,
            lblDir, _txtDireccion,
            _chkTieneNacimiento, _dtpNacimiento,
            pnlFichaEnc, _txtFichaTecnica,
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
        _btnGuardar.Location = new Point(320, 12);
        _btnGuardar.Click += async (s, e) => await GuardarAsync();

        _btnCancelar = TemaJMBeauty.CrearBotonAccion("✖ Cancelar", Color.Gray, Color.White, 120, 36);
        _btnCancelar.Location = new Point(460, 12);
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
        _txtNombres.Text = _original.Nombres;
        _txtApellidos.Text = _original.Apellidos;
        _txtTelefono.Text = _original.Telefono;
        _txtEmail.Text = _original.Email ?? "";
        _txtDireccion.Text = _original.Direccion ?? "";
        if (_original.FechaNacimiento.HasValue)
        {
            _chkTieneNacimiento.Checked = true;
            _dtpNacimiento.Value = _original.FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue);
        }
        _txtFichaTecnica.Text = _original.HistorialAlergiasNotas ?? "";
        _chkActivo.Checked = _original.Activo;
    }

    private async Task GuardarAsync()
    {
        var cliente = new Cliente
        {
            ClienteId = _original?.ClienteId ?? 0,
            Nombres = _txtNombres.Text.Trim(),
            Apellidos = _txtApellidos.Text.Trim(),
            Telefono = _txtTelefono.Text.Trim(),
            Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
            Direccion = string.IsNullOrWhiteSpace(_txtDireccion.Text) ? null : _txtDireccion.Text.Trim(),
            FechaNacimiento = _chkTieneNacimiento.Checked ? DateOnly.FromDateTime(_dtpNacimiento.Value) : null,
            HistorialAlergiasNotas = string.IsNullOrWhiteSpace(_txtFichaTecnica.Text) ? null : _txtFichaTecnica.Text.Trim(),
            Activo = _chkActivo.Checked
        };

        _btnGuardar.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var id = await ClienteNegocio.GuardarAsync(_clave, cliente, _original);
            cliente.ClienteId = id;
            ClienteGuardado = cliente;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error al guardar cliente", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}

