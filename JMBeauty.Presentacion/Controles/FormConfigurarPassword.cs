using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Datos;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class FormConfigurarPassword : Form
{
    private TextBox _txtClave = null!;
    private Button _btnProbarYGuardar = null!;
    private Button _btnCancelar = null!;
    private Label _lblEstado = null!;

    public string? ClaveConfirmada { get; private set; }

    public FormConfigurarPassword(string claveInicial)
    {
        InicializarComponentes(claveInicial);
    }

    private void InicializarComponentes(string claveInicial)
    {
        Text = "Configuración de Contraseña PostgreSQL — JM-BEAUTY CLAROS";
        Size = new Size(500, 290);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = TemaJMBeauty.FuenteCuerpo;

        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = TemaJMBeauty.FondoEncabezado,
            Padding = new Padding(20, 14, 0, 0)
        };
        var lblTitulo = new Label
        {
            Text = "🔑 Contraseña de Conexión a Base de Datos",
            Font = TemaJMBeauty.FuenteSubtitulo,
            ForeColor = TemaJMBeauty.Dorado,
            AutoSize = true
        };
        pnlHeader.Controls.Add(lblTitulo);

        var pnlCuerpo = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25, 20, 25, 10)
        };

        var lblInfo = new Label
        {
            Text = "Ingrese la contraseña de su usuario 'postgres' en PostgreSQL local:",
            Dock = DockStyle.Top,
            Height = 35,
            Font = TemaJMBeauty.FuenteCuerpoBold,
            ForeColor = TemaJMBeauty.TextoOscuro
        };

        _txtClave = new TextBox
        {
            Location = new Point(25, 65),
            Width = 430,
            Font = new Font("Segoe UI", 11F, FontStyle.Regular),
            PasswordChar = '•',
            Text = claveInicial
        };

        var chkMostrar = new CheckBox
        {
            Text = "Mostrar contraseña",
            Location = new Point(25, 102),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        chkMostrar.CheckedChanged += (s, e) => _txtClave.PasswordChar = chkMostrar.Checked ? '\0' : '•';

        _lblEstado = new Label
        {
            Text = "Al pulsar guardar, se probará la conexión con 'jmbeauty_db'.",
            Location = new Point(25, 130),
            Width = 430,
            Height = 25,
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic)
        };

        pnlCuerpo.Controls.AddRange([lblInfo, _txtClave, chkMostrar, _lblEstado]);

        var pnlBotones = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = TemaJMBeauty.RosaClaro,
            Padding = new Padding(0, 12, 25, 0)
        };

        _btnProbarYGuardar = TemaJMBeauty.CrearBotonAccion("✔ Probar y Guardar", TemaJMBeauty.Dorado, Color.White, 180, 36);
        _btnProbarYGuardar.Location = new Point(140, 12);
        _btnProbarYGuardar.Click += async (s, e) => await ProbarYGuardarAsync();

        _btnCancelar = TemaJMBeauty.CrearBotonAccion("✖ Cancelar", Color.Gray, Color.White, 120, 36);
        _btnCancelar.Location = new Point(335, 12);
        _btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

        pnlBotones.Controls.Add(_btnProbarYGuardar);
        pnlBotones.Controls.Add(_btnCancelar);

        Controls.Add(pnlCuerpo);
        Controls.Add(pnlBotones);
        Controls.Add(pnlHeader);
    }

    private async Task ProbarYGuardarAsync()
    {
        var clave = _txtClave.Text;
        if (string.IsNullOrWhiteSpace(clave))
        {
            MessageBox.Show("Por favor ingrese una contraseña.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnProbarYGuardar.Enabled = false;
        _lblEstado.Text = "Probando conexión con PostgreSQL...";
        _lblEstado.ForeColor = Color.DarkGoldenrod;
        Cursor = Cursors.WaitCursor;

        try
        {
            var ok = await ConexionNegocio.ProbarAsync(clave);
            if (ok)
            {
                Conexion.GuardarPassword(clave);
                ClaveConfirmada = clave;
                MessageBox.Show("¡Conexión exitosa a 'jmbeauty_db'! La contraseña ha sido guardada en su equipo.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            _lblEstado.Text = "Error: contraseña rechazada por PostgreSQL.";
            _lblEstado.ForeColor = Color.Red;
            MessageBox.Show($"La contraseña ingresada no pudo conectar a PostgreSQL:\n\n{ex.Message}", "Fallo de Autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnProbarYGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}

