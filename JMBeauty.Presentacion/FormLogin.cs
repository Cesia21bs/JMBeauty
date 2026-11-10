using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JMBeauty.Datos;
using JMBeauty.Entidades;
using JMBeauty.Negocio;
using JMBeauty.Presentacion.Controles;

namespace JMBeauty.Presentacion;

public sealed class FormLogin : Form
{
    private string _claveBd;
    private List<Empleado> _empleados = [];

    private PictureBox _picLogo = null!;
    private ComboBox _cboEmpleado = null!;
    private TextBox _txtUsuarioManual = null!;
    private CheckBox _chkModoManual = null!;
    private TextBox _txtPassword = null!;
    private CheckBox _chkVerPassword = null!;
    private Button _btnIngresar = null!;
    private Button _btnSalir = null!;
    private Label _lblRolSeleccionado = null!;
    private Label _lblPermisosInfo = null!;
    private Label _lblMensajeError = null!;
    private Button _btnConfigurarDb = null!;

    public Empleado? EmpleadoAutenticado { get; private set; }
    public string ClaveBdActual => _claveBd;

    public FormLogin()
    {
        _claveBd = Conexion.ObtenerPasswordDeEntorno();
        InicializarComponentes();
        _ = CargarEmpleadosAsync();
    }

    private void InicializarComponentes()
    {
        Text = "Inicio de Sesión — JM-BEAUTY CLAROS (Academia - Salón)";
        Size = new Size(820, 680);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = TemaJMBeauty.FuenteCuerpo;

        // 1. Panel Superior con Logo y Banner Oscuro de Lujo
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = TemaJMBeauty.FondoEncabezado,
            Padding = new Padding(20, 10, 20, 10)
        };

        _picLogo = new PictureBox
        {
            Size = new Size(90, 90),
            Location = new Point(20, 10),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        var rutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "assesnt", "logo_sin_fondo.png");
        if (!File.Exists(rutaLogo))
            rutaLogo = Path.Combine(Directory.GetCurrentDirectory(), "assesnt", "logo_sin_fondo.png");

        if (File.Exists(rutaLogo))
        {
            try { _picLogo.Image = Image.FromFile(rutaLogo); } catch { }
        }

        var lblMarca = new Label
        {
            Text = "JM-BEAUTY CLAROS",
            Location = new Point(125, 18),
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = TemaJMBeauty.Dorado,
            BackColor = Color.Transparent
        };

        var lblSub = new Label
        {
            Text = "Academia – Salón de Belleza  |  Control de Acceso y Roles",
            Location = new Point(128, 60),
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Italic),
            ForeColor = TemaJMBeauty.RosaNude,
            BackColor = Color.Transparent
        };

        _btnConfigurarDb = new Button
        {
            Text = "⚙ BD",
            Size = new Size(60, 28),
            Location = new Point(pnlHeader.Width - 90, 20),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = TemaJMBeauty.RosaPastel,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        _btnConfigurarDb.FlatAppearance.BorderColor = TemaJMBeauty.RosaPastel;
        _btnConfigurarDb.Click += (s, e) => AbrirDialogoConfiguracionClave();

        pnlHeader.Controls.AddRange([_picLogo, lblMarca, lblSub, _btnConfigurarDb]);

        // 2. Contenedor Principal (Dividido en 2 columnas: Izquierda Login, Derecha Roles y Permisos)
        var pnlPrincipal = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25, 20, 25, 20),
            BackColor = TemaJMBeauty.CremaClaro
        };

        // Columna Izquierda: Formulario de Login (Ancho 380)
        var pnlLogin = new Panel
        {
            Location = new Point(25, 20),
            Width = 370,
            Height = 490,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        var lblLoginTitulo = new Label
        {
            Text = "🔐 Identificación de Usuario",
            Location = new Point(18, 15),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteSubtitulo,
            ForeColor = TemaJMBeauty.TextoOscuro
        };

        var lblEmp = new Label
        {
            Text = "Seleccione su Nombre / Usuario:",
            Location = new Point(18, 55),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        _cboEmpleado = new ComboBox
        {
            Location = new Point(18, 80),
            Width = 330,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _cboEmpleado.SelectedIndexChanged += (s, e) => ActualizarInfoRolSeleccionado();

        _chkModoManual = new CheckBox
        {
            Text = "O escribir DUI / Correo manualmente",
            Location = new Point(18, 115),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F)
        };
        _chkModoManual.CheckedChanged += (s, e) =>
        {
            _cboEmpleado.Visible = !_chkModoManual.Checked;
            _txtUsuarioManual.Visible = _chkModoManual.Checked;
            if (_chkModoManual.Checked) _txtUsuarioManual.Focus();
        };

        _txtUsuarioManual = new TextBox
        {
            Location = new Point(18, 80),
            Width = 330,
            Font = TemaJMBeauty.FuenteCuerpo,
            Visible = false
        };

        var lblPass = new Label
        {
            Text = "Contraseña / PIN de Acceso:",
            Location = new Point(18, 145),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        _txtPassword = new TextBox
        {
            Location = new Point(18, 170),
            Width = 330,
            Font = new Font("Segoe UI", 11F),
            UseSystemPasswordChar = true,
            Text = "1234"
        };
        _txtPassword.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _btnIngresar.PerformClick();
            }
        };

        _chkVerPassword = new CheckBox
        {
            Text = "Mostrar contraseña",
            Location = new Point(18, 205),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F)
        };
        _chkVerPassword.CheckedChanged += (s, e) => _txtPassword.UseSystemPasswordChar = !_chkVerPassword.Checked;

        _lblRolSeleccionado = new Label
        {
            Location = new Point(18, 235),
            Width = 330,
            Height = 45,
            BackColor = TemaJMBeauty.RosaClaro,
            ForeColor = Color.DarkGoldenrod,
            Font = TemaJMBeauty.FuenteCuerpoBold,
            TextAlign = ContentAlignment.MiddleCenter,
            BorderStyle = BorderStyle.FixedSingle,
            Text = "Rol: Cargando personal..."
        };

        _lblMensajeError = new Label
        {
            Location = new Point(18, 285),
            Width = 330,
            Height = 45,
            ForeColor = Color.Crimson,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = ""
        };

        _btnIngresar = TemaJMBeauty.CrearBotonAccion("✨ INGRESAR AL SALÓN", TemaJMBeauty.Dorado, Color.White, 330, 42);
        _btnIngresar.Location = new Point(18, 335);
        _btnIngresar.Click += async (s, e) => await EjecutarLoginAsync();

        _btnSalir = TemaJMBeauty.CrearBotonAccion("✖ Salir del Sistema", Color.FromArgb(235, 235, 235), TemaJMBeauty.TextoOscuro, 330, 34);
        _btnSalir.Location = new Point(18, 385);
        _btnSalir.Click += (s, e) => Application.Exit();

        pnlLogin.Controls.AddRange([
            lblLoginTitulo,
            lblEmp, _cboEmpleado, _txtUsuarioManual, _chkModoManual,
            lblPass, _txtPassword, _chkVerPassword,
            _lblRolSeleccionado,
            _lblMensajeError,
            _btnIngresar,
            _btnSalir
        ]);

        // Columna Derecha: Tarjeta de Roles y Accesos (Ancho 360)
        var pnlRolesInfo = new Panel
        {
            Location = new Point(415, 20),
            Width = 360,
            Height = 490,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        var lblRolesHeader = new Label
        {
            Text = "📋 PERFILES Y ROLES DEL SISTEMA",
            Location = new Point(15, 15),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteSubtitulo,
            ForeColor = Color.DarkGoldenrod
        };

        _lblPermisosInfo = new Label
        {
            Location = new Point(15, 45),
            Width = 330,
            Height = 425,
            Font = new Font("Segoe UI", 8.8F, FontStyle.Regular),
            ForeColor = TemaJMBeauty.TextoOscuro,
            Text = """
                👑 Administrador
                • Acceso total al sistema.
                • Gestión de personal, finanzas, ventas, compras, comisiones y configuración general.

                ───────────────────────────────────

                📋 Recepcionista
                • Control de agenda y citas del salón.
                • Facturación / Ventas y cobros en caja.
                • Registro y consulta de fichas técnicas de clientes.

                ───────────────────────────────────

                💄 Maquilladora / Maquillador
                • Agendamiento de citas del salón.
                • Registro de notas en la ficha técnica del cliente (alergias, tipo de piel, fórmulas).
                • Consulta de sus propias comisiones por servicios y productos.

                ───────────────────────────────────

                ✂️ Estilista / Manicurista / Cosmiatra
                • Agendamiento de sus citas de belleza.
                • Atención, historial y notas de clientes.
                • Consulta de sus comisiones ganadas.
                """
        };

        pnlRolesInfo.Controls.Add(lblRolesHeader);
        pnlRolesInfo.Controls.Add(_lblPermisosInfo);

        pnlPrincipal.Controls.Add(pnlLogin);
        pnlPrincipal.Controls.Add(pnlRolesInfo);

        Controls.Add(pnlPrincipal);
        Controls.Add(pnlHeader);
    }

    private async Task CargarEmpleadosAsync()
    {
        try
        {
            _empleados = await EmpleadoNegocio.ListarEmpleadosAsync(_claveBd, soloActivos: true);
            _cboEmpleado.DataSource = null;
            _cboEmpleado.DataSource = _empleados;
            _cboEmpleado.DisplayMember = "DescripcionCombo";
            _cboEmpleado.ValueMember = "EmpleadoId";

            if (_empleados.Count > 0)
            {
                _cboEmpleado.SelectedIndex = 0;
                ActualizarInfoRolSeleccionado();
            }
            else
            {
                _lblRolSeleccionado.Text = "No hay empleados registrados.";
            }
        }
        catch (Exception ex)
        {
            _lblMensajeError.Text = $"Error al cargar personal: {ex.Message}";
        }
    }

    private void ActualizarInfoRolSeleccionado()
    {
        if (_cboEmpleado.SelectedItem is Empleado emp)
        {
            var icono = emp.TipoAcceso switch
            {
                "ADMINISTRADOR" => "👑",
                "RECEPCIONISTA" => "📋",
                _ => emp.RolNombre.Contains("Maquill", StringComparison.OrdinalIgnoreCase) ? "💄" : "✂️"
            };

            _lblRolSeleccionado.Text = $"{icono} {emp.RolNombre}\nAcceso: {FormatearTipoAcceso(emp.TipoAcceso)}";
        }
    }

    private static string FormatearTipoAcceso(string tipo) => tipo switch
    {
        "ADMINISTRADOR" => "Control Total del Sistema",
        "RECEPCIONISTA" => "Agenda, Ventas y Clientes",
        _ => "Especialista (Agenda, Fichas y Comisiones)"
    };

    private async Task EjecutarLoginAsync()
    {
        _lblMensajeError.Text = "";
        var password = _txtPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(password))
        {
            _lblMensajeError.Text = "Por favor ingrese su contraseña o PIN.";
            _txtPassword.Focus();
            return;
        }

        string usuarioParaValidar;
        Empleado? seleccionado = null;

        if (_chkModoManual.Checked)
        {
            usuarioParaValidar = _txtUsuarioManual.Text.Trim();
            if (string.IsNullOrWhiteSpace(usuarioParaValidar))
            {
                _lblMensajeError.Text = "Por favor ingrese su DUI o correo.";
                _txtUsuarioManual.Focus();
                return;
            }
        }
        else
        {
            if (_cboEmpleado.SelectedItem is not Empleado emp)
            {
                _lblMensajeError.Text = "Seleccione un empleado de la lista.";
                return;
            }
            seleccionado = emp;
            usuarioParaValidar = emp.DocumentoIdentidad;
        }

        _btnIngresar.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            // Validar credenciales
            var empAutenticado = await EmpleadoNegocio.AutenticarAsync(_claveBd, usuarioParaValidar, password);

            if (empAutenticado is null)
            {
                _lblMensajeError.Text = "Credenciales incorrectas. Verifique su PIN/Contraseña.";
                _txtPassword.SelectAll();
                _txtPassword.Focus();
                return;
            }

            EmpleadoAutenticado = empAutenticado;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblMensajeError.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _btnIngresar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private void AbrirDialogoConfiguracionClave()
    {
        using var frm = new FormConfigurarPassword(_claveBd);
        if (frm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(frm.ClaveConfirmada))
        {
            _claveBd = frm.ClaveConfirmada;
            _ = CargarEmpleadosAsync();
        }
    }
}

