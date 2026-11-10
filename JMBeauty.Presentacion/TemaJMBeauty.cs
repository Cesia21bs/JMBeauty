using System.Drawing;
using System.Windows.Forms;

namespace JMBeauty.Presentacion;

public static class TemaJMBeauty
{
    // Símbolo Oficial de Moneda
    public const string Moneda = "Lps.";

    // Paleta Oficial Solicitada
    public static readonly Color CremaClaro = ColorTranslator.FromHtml("#FFF8E1");
    public static readonly Color Dorado = ColorTranslator.FromHtml("#D4AF37");
    public static readonly Color RosaNude = ColorTranslator.FromHtml("#F4C2C2");
    public static readonly Color RosaPastel = ColorTranslator.FromHtml("#D98CA6");
    public static readonly Color RosaClaro = ColorTranslator.FromHtml("#FDE7F0");

    // Colores de soporte y contraste
    public static readonly Color TextoOscuro = ColorTranslator.FromHtml("#2A2124");
    public static readonly Color FondoEncabezado = ColorTranslator.FromHtml("#1C1719");
    public static readonly Color DoradoOscuro = ColorTranslator.FromHtml("#AA820A");
    public static readonly Color BordeControl = ColorTranslator.FromHtml("#E0D3C1");

    public static readonly Font FuenteTitulo = new("Segoe UI", 15F, FontStyle.Bold);
    public static readonly Font FuenteSubtitulo = new("Segoe UI", 11F, FontStyle.Bold);
    public static readonly Font FuenteCuerpo = new("Segoe UI", 9.5F, FontStyle.Regular);
    public static readonly Font FuenteCuerpoBold = new("Segoe UI", 9.5F, FontStyle.Bold);
    public static readonly Font FuenteBotones = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font FuenteCabeceraGrid = new("Segoe UI", 10F, FontStyle.Bold);

    public static Button CrearBotonMenu(string texto)
    {
        var btn = new Button
        {
            Text = texto,
            Height = 44,
            Dock = DockStyle.Top,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = TextoOscuro,
            Font = FuenteBotones,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = RosaClaro;
        btn.FlatAppearance.MouseDownBackColor = RosaNude;
        return btn;
    }

    public static Button CrearBotonAccion(string texto, Color fondo, Color textoColor, int ancho = 120, int alto = 34)
    {
        var btn = new Button
        {
            Text = texto,
            Width = ancho,
            Height = alto,
            FlatStyle = FlatStyle.Flat,
            BackColor = fondo,
            ForeColor = textoColor,
            Font = FuenteBotones,
            Cursor = Cursors.Hand,
            Margin = new Padding(4, 0, 4, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    public static void AplicarEstiloGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = RosaClaro;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.AutoGenerateColumns = false;
        grid.RowTemplate.Height = 36;
        grid.Font = FuenteCuerpo;

        // Cabeceras
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 38;
        grid.ColumnHeadersDefaultCellStyle.BackColor = RosaPastel;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = FuenteCabeceraGrid;
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

        // Filas alternadas y selección
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = TextoOscuro;
        grid.DefaultCellStyle.SelectionBackColor = RosaNude;
        grid.DefaultCellStyle.SelectionForeColor = TextoOscuro;
        grid.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(254, 250, 245);
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = RosaNude;
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextoOscuro;
    }

    /// <summary>
    /// Restringe el campo para que solo permita escribir letras, espacios y caracteres de control (no números ni símbolos).
    /// </summary>
    public static void ConfigurarSoloLetras(TextBox txt)
    {
        txt.KeyPress += (s, e) =>
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        };
    }

    /// <summary>
    /// Restringe el campo para que solo permita escribir números y teclas de control (no letras).
    /// </summary>
    public static void ConfigurarSoloNumeros(TextBox txt, bool permitirGuion = true)
    {
        txt.KeyPress += (s, e) =>
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && (!permitirGuion || (e.KeyChar != '-' && e.KeyChar != '+')))
            {
                e.Handled = true;
            }
        };
    }
}
