using System.Drawing;
using System.Windows.Forms;
using JMBeauty.Entidades;
using JMBeauty.Negocio;

namespace JMBeauty.Presentacion.Controles;

public sealed class ModuloEmpleadosControl : UserControl
{
    private readonly string _clave;
    private List<Empleado> _todosLosEmpleados = [];
    private List<Rol> _roles = [];

    private DataGridView _grid = null!;
    private TextBox _txtBuscar = null!;
    private Button _btnNuevo = null!;
    private Button _btnEditar = null!;
    private Button _btnActualizar = null!;
    private Label _lblConteo = null!;

    public ModuloEmpleadosControl(string clave)
    {
        _clave = clave;
        InicializarComponentes();
        _ = CargarDatosAsync();
    }

    private void InicializarComponentes()
    {
        Dock = DockStyle.Fill;
        BackColor = TemaJMBeauty.CremaClaro;
        Font = TemaJMBeauty.FuenteCuerpo;

        var pnlBarra = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.White,
            Padding = new Padding(15, 12, 15, 12)
        };

        var lblBuscar = new Label { Text = "🔍 Buscar:", Location = new Point(15, 20), AutoSize = true, Font = TemaJMBeauty.FuenteCuerpoBold };
        _txtBuscar = new TextBox { Location = new Point(85, 17), Width = 280, Font = TemaJMBeauty.FuenteCuerpo };
        _txtBuscar.TextChanged += (s, e) => FiltrarEmpleados();

        _btnNuevo = TemaJMBeauty.CrearBotonAccion("➕ Nuevo Personal", TemaJMBeauty.Dorado, Color.White, 140);
        _btnNuevo.Location = new Point(380, 14);
        _btnNuevo.Click += (s, e) => AbrirEdicion(null);

        _btnEditar = TemaJMBeauty.CrearBotonAccion("✏️ Editar", TemaJMBeauty.RosaPastel, Color.White, 100);
        _btnEditar.Location = new Point(530, 14);
        _btnEditar.Click += (s, e) => EditarSeleccionado();

        _btnActualizar = TemaJMBeauty.CrearBotonAccion("🔄 Actualizar", Color.DimGray, Color.White, 110);
        _btnActualizar.Location = new Point(640, 14);
        _btnActualizar.Click += async (s, e) => await CargarDatosAsync();

        _lblConteo = new Label
        {
            Text = "Cargando personal...",
            AutoSize = true,
            Location = new Point(770, 21),
            ForeColor = Color.Gray,
            Font = TemaJMBeauty.FuenteCuerpoBold
        };

        pnlBarra.Controls.AddRange([lblBuscar, _txtBuscar, _btnNuevo, _btnEditar, _btnActualizar, _lblConteo]);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        TemaJMBeauty.AplicarEstiloGrid(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NombreCompleto", HeaderText = "PROFESIONAL / EMPLEADO", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RolNombre", HeaderText = "ROL / CARGO", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DocumentoIdentidad", HeaderText = "DOCUMENTO", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Telefono", HeaderText = "TELÉFONO", Width = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "PorcentajeComisionServicio",
            HeaderText = "COMISIÓN SRV (%)",
            Width = 145,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "PorcentajeComisionProducto",
            HeaderText = "COMISIÓN PRD (%)",
            Width = 145,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });

        var colColor = new DataGridViewTextBoxColumn { HeaderText = "COLOR AGENDA", Width = 120 };
        _grid.Columns.Add(colColor);

        var colEstado = new DataGridViewTextBoxColumn { HeaderText = "ESTADO", Width = 90 };
        _grid.Columns.Add(colEstado);

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is Empleado emp)
            {
                if (e.ColumnIndex == 6) // Color
                {
                    e.Value = emp.ColorAgenda;
                    try
                    {
                        e.CellStyle.BackColor = ColorTranslator.FromHtml(emp.ColorAgenda);
                        e.CellStyle.ForeColor = Color.Black;
                    }
                    catch { }
                    e.FormattingApplied = true;
                }
                else if (e.ColumnIndex == 7) // Estado
                {
                    e.Value = emp.Activo ? "Activo" : "Inactivo";
                    e.FormattingApplied = true;
                }
            }
        };

        _grid.DoubleClick += (s, e) => EditarSeleccionado();

        var pnlContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        pnlContenedor.Controls.Add(_grid);

        Controls.Add(pnlContenedor);
        Controls.Add(pnlBarra);
    }

    private async Task CargarDatosAsync()
    {
        Cursor = Cursors.WaitCursor;
        _lblConteo.Text = "Cargando...";

        try
        {
            _roles = await EmpleadoNegocio.ListarRolesAsync(_clave);
            _todosLosEmpleados = await EmpleadoNegocio.ListarEmpleadosAsync(_clave);
            FiltrarEmpleados();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar personal:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void FiltrarEmpleados()
    {
        var filtro = _txtBuscar.Text.Trim().ToLowerInvariant();
        var lista = string.IsNullOrWhiteSpace(filtro)
            ? _todosLosEmpleados
            : _todosLosEmpleados.Where(e =>
                e.NombreCompleto.ToLowerInvariant().Contains(filtro) ||
                e.RolNombre.ToLowerInvariant().Contains(filtro) ||
                e.Telefono.Contains(filtro) ||
                e.DocumentoIdentidad.Contains(filtro)).ToList();

        _grid.DataSource = null;
        _grid.DataSource = lista;
        _lblConteo.Text = $"Total: {lista.Count} empleado(s)";
    }

    private void EditarSeleccionado()
    {
        if (_grid.CurrentRow?.DataBoundItem is Empleado seleccionado)
        {
            AbrirEdicion(seleccionado);
        }
    }

    private void AbrirEdicion(Empleado? emp)
    {
        using var frm = new FormEmpleadoEdicion(_clave, _roles, emp);
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            _ = CargarDatosAsync();
        }
    }
}

