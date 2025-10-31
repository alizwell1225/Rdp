using System;
using System.Windows.Forms;

namespace GrpcServerApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new ServerForm());
        }
    }
}
