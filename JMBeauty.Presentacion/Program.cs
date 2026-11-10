using System.Windows.Forms;
using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Presentacion;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new JMBeautyAppContext());
    }    
}

internal sealed class JMBeautyAppContext : ApplicationContext
{
    private string _claveBd;

    public JMBeautyAppContext()
    {
        _claveBd = Conexion.ObtenerPasswordDeEntorno();
        IniciarLogin();
    }

    private void IniciarLogin()
    {
        var login = new FormLogin();
        login.FormClosed += (s, e) =>
        {
            if (login.DialogResult == DialogResult.OK && login.EmpleadoAutenticado is not null)
            {
                _claveBd = login.ClaveBdActual;
                IniciarPrincipal(login.EmpleadoAutenticado);
            }
            else
            {
                ExitThread();
            }
        };
        login.Show();
    }

    private void IniciarPrincipal(Empleado emp)
    {
        var principal = new FormPrincipal(emp, _claveBd);
        principal.FormClosed += (s, e) =>
        {
            if (principal.DialogResult == DialogResult.Retry)
            {
                IniciarLogin(); // Regresa al login para que pueda ingresar otro usuario
            }
            else
            {
                ExitThread();
            }
        };
        principal.Show();
    }
}
