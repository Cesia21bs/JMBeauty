using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class FormRolEdicion : Form
{
    private readonly string _clave;
    private TextBox _txtNombre = null!;
    private TextBox _txtDescripcion = null!;
    private ComboBox _cboTipoAcceso = null!;
    private Button _btnGuardar = null!;
    private Button _btnCancelar = null!;

    public Rol? RolCreado { get; private set; }

    public FormRolEdicion(string clave)
    {
        _clave = clave;
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        Text = "Nuevo Rol de Personal — JM-BEAUTY CLAROS";
        Size = new Size(520, 420);
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
            Text = "👑 Registro de Nuevo Rol / Cargo",
            Font = TemaJMBeauty.FuenteSubtitulo,
            ForeColor = TemaJMBeauty.Dorado,
            AutoSize = true
        };
        pnlTitulo.Controls.Add(lblTitulo);

        var pnlForm = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25, 20, 25, 10),
            BackColor = Color.White
        };

        int y = 15;
        // Nombre del Rol
        var lblNom = new Label { Text = "Nombre del Rol (Ej: Maquilladora, Cosmiatra, Barbero):", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtNombre = new TextBox { Location = new Point(25, y + 22), Width = 450, Font = TemaJMBeauty.FuenteCuerpo };
        TemaJMBeauty.ConfigurarSoloLetras(_txtNombre);

        y += 60;
        // Tipo de Acceso / Perfil
        var lblTipo = new Label { Text = "Nivel de Acceso y Funciones en el Sistema:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboTipoAcceso = new ComboBox
        {
            Location = new Point(25, y + 22),
            Width = 450,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        _cboTipoAcceso.Items.Add(new OpcionAcceso("ESPECIALISTA", "💅 Especialista / Profesional (Agenda, Clientes y sus propias Comisiones)"));
        _cboTipoAcceso.Items.Add(new OpcionAcceso("RECEPCIONISTA", "📋 Recepcionista (Agenda de Citas, Cobros/Ventas y Clientes)"));
        _cboTipoAcceso.Items.Add(new OpcionAcceso("ADMINISTRADOR", "👑 Administrador (Acceso Total: Personal, Finanzas, Catálogos)"));
        _cboTipoAcceso.DisplayMember = "Texto";
        _cboTipoAcceso.ValueMember = "Codigo";
        _cboTipoAcceso.SelectedIndex = 0; // Por defecto Especialista (como Maquilladora)

        y += 60;
        // Descripción
        var lblDesc = new Label { Text = "Descripción / Especialidad (Opcional):", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtDescripcion = new TextBox { Location = new Point(25, y + 22), Width = 450, Height = 50, Multiline = true, Font = TemaJMBeauty.FuenteCuerpo };

        pnlForm.Controls.AddRange([lblNom, _txtNombre, lblTipo, _cboTipoAcceso, lblDesc, _txtDescripcion]);

        var pnlBotones = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = TemaJMBeauty.RosaClaro,
            Padding = new Padding(0, 12, 25, 0)
        };

        _btnGuardar = TemaJMBeauty.CrearBotonAccion("💾 Guardar Rol", TemaJMBeauty.Dorado, Color.White, 150, 36);
        _btnGuardar.Location = new Point(220, 12);
        _btnGuardar.Click += async (s, e) => await GuardarAsync();

        _btnCancelar = TemaJMBeauty.CrearBotonAccion("✖ Cancelar", Color.Gray, Color.White, 120, 36);
        _btnCancelar.Location = new Point(380, 12);
        _btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

        pnlBotones.Controls.Add(_btnGuardar);
        pnlBotones.Controls.Add(_btnCancelar);

        Controls.Add(pnlForm);
        Controls.Add(pnlBotones);
        Controls.Add(pnlTitulo);
    }

    private async Task GuardarAsync()
    {
        var nombre = _txtNombre.Text.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
        {
            MessageBox.Show("Por favor escriba el nombre del nuevo rol (ej: Maquilladora).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNombre.Focus();
            return;
        }

        var tipo = ((OpcionAcceso)_cboTipoAcceso.SelectedItem!).Codigo;

        var rol = new Rol
        {
            Nombre = nombre,
            Descripcion = string.IsNullOrWhiteSpace(_txtDescripcion.Text) ? null : _txtDescripcion.Text.Trim(),
            TipoAcceso = tipo,
            Activo = true
        };

        _btnGuardar.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var id = await EmpleadoNegocio.GuardarRolAsync(_clave, rol);
            rol.RolId = id;
            RolCreado = rol;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error al crear rol", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private sealed record OpcionAcceso(string Codigo, string Texto);
}

