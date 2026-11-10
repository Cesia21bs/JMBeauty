using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Datos;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class FormProductoEdicion : Form
{
    private readonly Producto? _original;
    private readonly List<CategoriaProducto> _categorias;
    private readonly string _clave;

    private TextBox _txtCodigo = null!;
    private TextBox _txtNombre = null!;
    private ComboBox _cboCategoria = null!;
    private NumericUpDown _numCosto = null!;
    private NumericUpDown _numVenta = null!;
    private NumericUpDown _numMinimo = null!;
    private CheckBox _chkInsumo = null!;
    private CheckBox _chkActivo = null!;
    private Button _btnGuardar = null!;
    private Button _btnCancelar = null!;

    public Producto? ProductoGuardado { get; private set; }

    public FormProductoEdicion(string clave, List<CategoriaProducto> categorias, Producto? original = null)
    {
        _clave = clave;
        _categorias = categorias ?? [];
        _original = original;

        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = _original is null ? "Nuevo Producto — JM-BEAUTY CLAROS" : $"Editar Producto [{_original.Codigo}]";
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
            Text = _original is null ? "✨ Registro de Nuevo Producto" : "✏️ Modificación de Producto",
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
        var lblCat = new Label { Text = "Categoría:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _cboCategoria = new ComboBox
        {
            Location = new Point(25, y + 20),
            Width = 470,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = TemaJMBeauty.FuenteCuerpo
        };
        _cboCategoria.DataSource = _categorias;
        _cboCategoria.DisplayMember = "Nombre";
        _cboCategoria.ValueMember = "CategoriaProductoId";

        y += 60;
        // Código
        var lblCod = new Label { Text = "Código de Producto / Barras:", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtCodigo = new TextBox { Location = new Point(25, y + 20), Width = 220, Font = TemaJMBeauty.FuenteCuerpo };

        // Nombre
        var lblNom = new Label { Text = "Nombre del Cosmético / Producto:", Location = new Point(260, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtNombre = new TextBox { Location = new Point(260, y + 20), Width = 235, Font = TemaJMBeauty.FuenteCuerpo };

        y += 60;
        // Costo
        var lblCosto = new Label { Text = "Precio Costo (Lps.):", Location = new Point(25, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numCosto = new NumericUpDown
        {
            Location = new Point(25, y + 20),
            Width = 140,
            DecimalPlaces = 2,
            Maximum = 100000m,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        // Precio Venta
        var lblVenta = new Label { Text = "Precio Venta (Lps.):", Location = new Point(190, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numVenta = new NumericUpDown
        {
            Location = new Point(190, y + 20),
            Width = 140,
            DecimalPlaces = 2,
            Maximum = 100000m,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        // Existencia Mínima
        var lblMin = new Label { Text = "Stock Mínimo:", Location = new Point(355, y), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _numMinimo = new NumericUpDown
        {
            Location = new Point(355, y + 20),
            Width = 140,
            DecimalPlaces = 0,
            Maximum = 10000m,
            Value = 5,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        y += 65;
        // Checks
        _chkInsumo = new CheckBox
        {
            Text = "Es insumo interno del salón (Uso técnico, no venta)",
            Location = new Point(25, y),
            AutoSize = true,
            Font = TemaJMBeauty.FuenteCuerpo
        };

        _chkActivo = new CheckBox
        {
            Text = "Producto Activo en Catálogo",
            Location = new Point(25, y + 30),
            AutoSize = true,
            Checked = true,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlForm.Controls.AddRange([
            lblCat, _cboCategoria,
            lblCod, _txtCodigo,
            lblNom, _txtNombre,
            lblCosto, _numCosto,
            lblVenta, _numVenta,
            lblMin, _numMinimo,
            _chkInsumo, _chkActivo
        ]);

        // Panel Botones Inferior
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

        _txtCodigo.Text = _original.Codigo;
        _txtNombre.Text = _original.Nombre;
        _cboCategoria.SelectedValue = _original.CategoriaProductoId;
        _numCosto.Value = _original.PrecioCosto;
        _numVenta.Value = _original.PrecioVenta;
        _numMinimo.Value = _original.ExistenciaMinima;
        _chkInsumo.Checked = _original.EsInsumoInterno;
        _chkActivo.Checked = _original.Activo;
    }

    private async Task GuardarAsync()
    {
        if (_cboCategoria.SelectedValue is not int catId || catId <= 0)
        {
            MessageBox.Show("Por favor seleccione una categoría válida.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var producto = new Producto
        {
            ProductoId = _original?.ProductoId ?? 0,
            CategoriaProductoId = catId,
            Codigo = _txtCodigo.Text.Trim(),
            Nombre = _txtNombre.Text.Trim(),
            PrecioCosto = _numCosto.Value,
            PrecioVenta = _numVenta.Value,
            ExistenciaMinima = _numMinimo.Value,
            EsInsumoInterno = _chkInsumo.Checked,
            Activo = _chkActivo.Checked
        };

        _btnGuardar.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var id = await ProductoNegocio.GuardarAsync(_clave, producto, _original);
            producto.ProductoId = id;
            ProductoGuardado = producto;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error al guardar producto", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}

