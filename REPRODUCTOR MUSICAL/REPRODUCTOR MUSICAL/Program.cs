using System;
using System.Windows.Forms;
using REPRODUCTOR_MUSICAL.Infrastructure;

namespace REPRODUCTOR_MUSICAL
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicacion.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(ApplicationFactory.CreateHomeForm());
        }
    }
}
