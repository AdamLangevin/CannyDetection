using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CannyDetection
{
    static class Program
    {
        private static Boolean runFull = false;

        class MyContext : ApplicationContext
        {
            private Form1 form1;

            private FileStream fileStream;

            public MyContext()
            {
                form1 = new Form1();
                try
                {
                    form1.startUp(fileStream, runFull);
                    
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine("File not found, Error on the opening function: {0}", ex.ToString());
                }
                form1.Show();
            }

        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MyContext context = new MyContext();
            Application.Run(context);
        }
    }
}
