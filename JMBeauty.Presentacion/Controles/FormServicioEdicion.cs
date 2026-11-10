using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class FormServicioEdicion : Form
{
    private readonly Servicio? _original;
    private readonly List<CategoriaServicio> _categorias;
    private readonly string _clave;

    private ComboBox _cboCategoria = null!;
    private TextBox _txtCodigo = null!;
    private TextBox _txtNombre = null!;
    private TextBox _txtDescripcion = null!;
    private NumericUpDown _numDuracion = null!;
    private NumericUpDown _numPrecio = null!;
    private CheckBox _chkActivo = null!;
    private Button _btnGuardar = null!;
    private Button _btnCancelar = null!;

    public Servicio? ServicioGuardado { get; private set; }

    public FormServicioEdicion(string clave, List<CategoriaServicio> categorias, Servicio? original = null)
    {
        _clave = clave;
        _categorias = categorias ?? [];
        _original = original;

        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = _original is null ? "Nuevo Servicio — JM-BEAUTY CLAROS" : $"Editar Servicio [{_original.Codigo}]";
        Size = new Size(540, 520);
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
            Text = _original is null ? "💇‍♀️ Registro de Nuevo Servicio del Salón" : "✏️ Modificación de Servicio",
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
        // Categoría
        var lblCat = new Label { Text = "Categoría de Servicio:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboCategoria = new ComboBox
        {
            Location = new Point(25, y + 20),
            Width = 470,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _cboCategoria.DataSource = _categorias;
        _cboCategoria.DisplayMember = "Nombre";
        _cboCategoria.ValueMember = "CategoriaServicioId";

        y += 60;
        // Código
        var lblCod = new Label { Text = "Código de Servicio:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtCodigo = new TextBox { Location = new Point(25, y + 20), Width = 160, Font = TemaJMBeauty.FuenteCuerpo };

        // Nombre
        var lblNom = new Label { Text = "Nombre del Servicio:", Location = new Point(200, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtNombre = new TextBox { Location = new Point(200, y + 20), Width = 295, Font = TemaJMBeauty.FuenteCuerpo };

        y += 60;
        // Duración
        var lblDur = new Label { Text = "Duración Estimada (Minutos):", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numDuracion = new NumericUpDown
        {
            Location = new Point(25, y + 20),
            Width = 220,
            Minimum = 5,
            Maximum = 1440,
            Value = 45,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        // Precio Base
        var lblPre = new Label { Text = "Precio Base (Lps.):", Location = new Point(265, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numPrecio = new NumericUpDown
        {
            Location = new Point(265, y + 20),
            Width = 230,
            DecimalPlaces = 2,
            Maximum = 100000m,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        y += 60;
        // Descripción
        var lblDes = new Label { Text = "Detalles / Descripción del Tratamiento:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtDescripcion = new TextBox { Location = new Point(25, y + 20), Width = 470, Height = 55, Multiline = true, Font = TemaJMBeauty.FuenteCuerpo };

        y += 85;
        _chkActivo = new CheckBox
        {
            Text = "Servicio Activo en Menú del Salón",
            Location = new Point(25, y),
            AutoSize = true,
            Checked = true,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlForm.Controls.AddRange([
            lblCat, _cboCategoria,
            lblCod, _txtCodigo,
            lblNom, _txtNombre,
            lblDur, _numDuracion,
            lblPre, _numPrecio,
            lblDes, _txtDescripcion,
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
        _btnGuardar.Location = new Point(245, 12);
        _btnGuardar.Click += async (s, e) => await GuardarAsync();

        _btnCancelar = TemaJMBeauty.CrearBotonAccion("✖ Cancelar", Color.Gray, Color.White, 120, 36);
        _btnCancelar.Location = new Point(385, 12);
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
        _cboCategoria.SelectedValue = _original.CategoriaServicioId;
        _txtCodigo.Text = _original.Codigo;
        _txtNombre.Text = _original.Nombre;
        _numDuracion.Value = _original.DuracionMinutos;
        _numPrecio.Value = _original.PrecioBase;
        _txtDescripcion.Text = _original.Descripcion ?? "";
        _chkActivo.Checked = _original.Activo;
    }

    private async Task GuardarAsync()
    {
        if (_cboCategoria.SelectedValue is not int catId || catId <= 0)
        {
            MessageBox.Show("Por favor seleccione una categoría de servicio válida.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var servicio = new Servicio
        {
            ServicioId = _original?.ServicioId ?? 0,
            CategoriaServicioId = catId,
            Codigo = _txtCodigo.Text.Trim(),
            Nombre = _txtNombre.Text.Trim(),
            DuracionMinutos = (int)_numDuracion.Value,
            PrecioBase = _numPrecio.Value,
            Descripcion = string.IsNullOrWhiteSpace(_txtDescripcion.Text) ? null : _txtDescripcion.Text.Trim(),
            Activo = _chkActivo.Checked
        };

        _btnGuardar.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var id = await ServicioNegocio.GuardarAsync(_clave, servicio, _original);
            servicio.ServicioId = id;
            ServicioGuardado = servicio;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error al guardar servicio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}

