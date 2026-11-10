using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JMBeauty.Datos;
using JMBeauty.Entidades;
using JMBeauty.Negocio;
using JMBeauty.Presentacion.Controles;

namespace JMBeauty.Presentacion;

public partial class FormPrincipal : Form
{
    private Panel _pnlEncabezado = null!;
    private Panel _pnlMenuLateral = null!;
    private Panel _pnlContenido = null!;
    private Panel _pnlEstado = null!;
    private Label _lblTitulo = null!;
    private Label _lblSubtitulo = null!;
    private PictureBox _picLogo = null!;
    private Label _lblUsuarioSesion = null!;
    private Button _btnCerrarSesion = null!;
    private Label _lblEstadoConexion = null!;

    private readonly Empleado? _usuarioActual;
    private string _claveActual;
    private Button? _botonMenuActivo;
    private Action? _accionModuloActivo;

    public FormPrincipal() : this(null, null)
    {
    }

    public FormPrincipal(Empleado? usuarioActual, string? claveBd = null)
    {
        _usuarioActual = usuarioActual;
        _claveActual = claveBd ?? Conexion.ObtenerPasswordDeEntorno();
        InicializarComponentesPersonalizados();
    }

    private void InicializarComponentesPersonalizados()
    {
        SuspendLayout();

        var nombreRol = _usuarioActual is not null ? $"[{_usuarioActual.RolNombre}]" : "[Admin]";
        Text = $"JM-BEAUTY CLAROS — {(_usuarioActual?.NombreCompleto ?? "Administrador")} {nombreRol}";
        Size = new Size(1366, 820);
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        // 1. Panel de Encabezado Superior (Estilo de Lujo Dorado/Negro del Logo)
        _pnlEncabezado = new Panel
        {
            Dock = DockStyle.Top,
            Height = 85,
            BackColor = TemaJMBeauty.FondoEncabezado
        };

        _picLogo = new PictureBox
        {
            Size = new Size(75, 75),
            Location = new Point(15, 5),
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

        _lblTitulo = new Label
        {
            Text = "JM-BEAUTY CLAROS",
            Location = new Point(100, 14),
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = TemaJMBeauty.Dorado,
            BackColor = Color.Transparent
        };

        _lblSubtitulo = new Label
        {
            Text = "Academia – Salón de Belleza  |  Sistema Administrativo y Operativo",
            Location = new Point(102, 48),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Italic),
            ForeColor = TemaJMBeauty.RosaNude,
            BackColor = Color.Transparent
        };

        // Identificación del usuario logueado en la esquina derecha
        var nombreUsuario = _usuarioActual?.NombreCompleto ?? "Administrador General";
        var rolTexto = _usuarioActual?.RolNombre ?? "Acceso Total";
        var iconoUsuario = _usuarioActual?.TipoAcceso switch
        {
            "ADMINISTRADOR" => "👑",
            "RECEPCIONISTA" => "📋",
            _ => "💅"
        };

        _lblUsuarioSesion = new Label
        {
            Text = $"{iconoUsuario} {nombreUsuario}\nRol: {rolTexto}",
            Location = new Point(Width - 460, 20),
            Size = new Size(260, 44),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent
        };

        // Botón en la esquina superior derecha para salir y regresar al login
        _btnCerrarSesion = TemaJMBeauty.CrearBotonAccion("🚪 Salir al Login", Color.FromArgb(170, 40, 55), Color.White, 160, 40);
        _btnCerrarSesion.Location = new Point(Width - 190, 22);
        _btnCerrarSesion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnCerrarSesion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _btnCerrarSesion.Cursor = Cursors.Hand;
        _btnCerrarSesion.Click += (s, e) => CerrarSesionYRegresarAlLogin();

        _pnlEncabezado.Controls.Add(_picLogo);
        _pnlEncabezado.Controls.Add(_lblTitulo);
        _pnlEncabezado.Controls.Add(_lblSubtitulo);
        _pnlEncabezado.Controls.Add(_lblUsuarioSesion);
        _pnlEncabezado.Controls.Add(_btnCerrarSesion);

        // 2. Barra de Estado Inferior
        _pnlEstado = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 32,
            BackColor = TemaJMBeauty.RosaClaro,
            Padding = new Padding(12, 6, 12, 6)
        };

        _lblEstadoConexion = new Label
        {
            Text = "Conectado a PostgreSQL (jmbeauty_db con usuario 'postgres').",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = TemaJMBeauty.TextoOscuro,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
        };
        _pnlEstado.Controls.Add(_lblEstadoConexion);

        // 3. Menú Lateral de Navegación con Control de Acceso por Rol
        _pnlMenuLateral = new Panel
        {
            Dock = DockStyle.Left,
            Width = 260,
            BackColor = Color.White,
            Padding = new Padding(0, 10, 0, 10)
        };

        var tipo = _usuarioActual?.TipoAcceso ?? "ADMINISTRADOR";
        var esAdmin = tipo == "ADMINISTRADOR";
        var esRecep = tipo == "RECEPCIONISTA";
        var esEspecialista = tipo == "ESPECIALISTA";

        var btnCitas = TemaJMBeauty.CrearBotonMenu("📅  Agendar Citas");
        var btnVentas = TemaJMBeauty.CrearBotonMenu("💳  Facturación / Ventas");
        var btnInventario = TemaJMBeauty.CrearBotonMenu("📦  Inventario de Productos");
        var btnServicios = TemaJMBeauty.CrearBotonMenu("💇‍♀️  Servicios del Salón");
        var btnClientes = TemaJMBeauty.CrearBotonMenu("💆‍♀️  Clientes y Ficha Técnica");
        var btnEmpleados = TemaJMBeauty.CrearBotonMenu("👑  Personal y Roles");
        var btnCompras = TemaJMBeauty.CrearBotonMenu("🛒  Compras / Insumos");
        var btnComisiones = TemaJMBeauty.CrearBotonMenu(esEspecialista ? "📊  Mis Comisiones" : "📊  Reporte de Comisiones");

        btnCitas.Click += (s, e) => SeleccionarBotonMenu(btnCitas, () => CargarModuloCitas());
        btnVentas.Click += (s, e) => SeleccionarBotonMenu(btnVentas, () => CargarModuloVentas(null));
        btnInventario.Click += (s, e) => SeleccionarBotonMenu(btnInventario, () => CargarModuloInventario());
        btnServicios.Click += (s, e) => SeleccionarBotonMenu(btnServicios, () => CargarModuloServicios());
        btnClientes.Click += (s, e) => SeleccionarBotonMenu(btnClientes, () => CargarModuloClientes());
        btnEmpleados.Click += (s, e) => SeleccionarBotonMenu(btnEmpleados, () => CargarModuloEmpleados());
        btnCompras.Click += (s, e) => SeleccionarBotonMenu(btnCompras, () => CargarModuloCompras());
        btnComisiones.Click += (s, e) => SeleccionarBotonMenu(btnComisiones, () => CargarModuloComisiones());

        // Control de Visibilidad según especificaciones del usuario:
        // Administrador: Acceso total al sistema
        // Recepcionista: Control de agenda, facturación/ventas y fichas técnicas de clientes (+ catálogo servicios/inventario)
        // Maquilladora / Estilista / Manicurista / Cosmiatra: Agendamiento de citas, ficha técnica de clientes y consulta de sus comisiones
        btnVentas.Visible = esAdmin || esRecep;
        btnInventario.Visible = esAdmin || esRecep;
        btnServicios.Visible = esAdmin || esRecep;
        btnEmpleados.Visible = esAdmin;
        btnCompras.Visible = esAdmin;
        btnComisiones.Visible = esAdmin || esEspecialista; // El especialista ve sus comisiones, el admin todas
        btnClientes.Visible = true; // Todos acceden a clientes y sus fichas técnicas
        btnCitas.Visible = true; // Todos agendan citas

        // Botón en la parte inferior del Menú Lateral para Cerrar Sesión y regresar al Login
        var btnSalirMenu = new Button
        {
            Text = "🚪  Cerrar Sesión / Salir",
            Height = 44,
            Dock = DockStyle.Bottom,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(254, 240, 242),
            ForeColor = Color.FromArgb(170, 40, 55),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        btnSalirMenu.FlatAppearance.BorderColor = TemaJMBeauty.RosaPastel;
        btnSalirMenu.Click += (s, e) => CerrarSesionYRegresarAlLogin();

        _pnlMenuLateral.Controls.Add(btnSalirMenu);
        _pnlMenuLateral.Controls.Add(btnComisiones);
        _pnlMenuLateral.Controls.Add(btnCompras);
        _pnlMenuLateral.Controls.Add(btnEmpleados);
        _pnlMenuLateral.Controls.Add(btnClientes);
        _pnlMenuLateral.Controls.Add(btnServicios);
        _pnlMenuLateral.Controls.Add(btnInventario);
        _pnlMenuLateral.Controls.Add(btnVentas);
        _pnlMenuLateral.Controls.Add(btnCitas);

        // 4. Panel Central de Trabajo
        _pnlContenido = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = TemaJMBeauty.CremaClaro
        };

        Controls.Add(_pnlContenido);
        Controls.Add(_pnlMenuLateral);
        Controls.Add(_pnlEstado);
        Controls.Add(_pnlEncabezado);

        ResumeLayout(false);

        // Cargar por defecto el primer módulo disponible según rol
        SeleccionarBotonMenu(btnCitas, () => CargarModuloCitas());
    }

    private void SeleccionarBotonMenu(Button boton, Action accionCarga)
    {
        if (_botonMenuActivo != null)
        {
            _botonMenuActivo.BackColor = Color.White;
            _botonMenuActivo.ForeColor = TemaJMBeauty.TextoOscuro;
        }

        _botonMenuActivo = boton;
        _botonMenuActivo.BackColor = TemaJMBeauty.RosaClaro;
        _botonMenuActivo.ForeColor = Color.DarkGoldenrod;

        _accionModuloActivo = accionCarga;
        accionCarga();
    }

    private void MostrarControlEnContenido(Control control)
    {
        _pnlContenido.SuspendLayout();
        _pnlContenido.Controls.Clear();
        control.Dock = DockStyle.Fill;
        _pnlContenido.Controls.Add(control);
        _pnlContenido.ResumeLayout(true);
    }

    private void CargarModuloCitas()
    {
        var ctrl = new ModuloCitasControl(_claveActual, onCobrarCita: cita =>
        {
            CargarModuloVentas(cita);
        });
        MostrarControlEnContenido(ctrl);
    }

    private void CargarModuloVentas(Cita? cita)
    {
        var ctrl = new ModuloVentasControl(_claveActual, cita);
        MostrarControlEnContenido(ctrl);
    }

    private void CargarModuloInventario()
    {
        var ctrl = new ModuloInventarioControl(_claveActual);
        MostrarControlEnContenido(ctrl);
    }

    private void CargarModuloServicios()
    {
        var ctrl = new ModuloServiciosControl(_claveActual);
        MostrarControlEnContenido(ctrl);
    }

    private void CargarModuloClientes()
    {
        var ctrl = new ModuloClientesControl(_claveActual);
        MostrarControlEnContenido(ctrl);
    }

    private void CargarModuloEmpleados()
    {
        var ctrl = new ModuloEmpleadosControl(_claveActual);
        MostrarControlEnContenido(ctrl);
    }

    private void CargarModuloCompras()
    {
        var ctrl = new ModuloComprasControl(_claveActual);
        MostrarControlEnContenido(ctrl);
    }

    private void CargarModuloComisiones()
    {
        var soloId = _usuarioActual?.TipoAcceso == "ESPECIALISTA" ? _usuarioActual.EmpleadoId : (int?)null;
        var ctrl = new ModuloComisionesControl(_claveActual, soloId);
        MostrarControlEnContenido(ctrl);
    }

    private void CerrarSesionYRegresarAlLogin()
    {
        DialogResult = DialogResult.Retry;
        Close();
    }
}
