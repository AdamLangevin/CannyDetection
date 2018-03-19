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
        private static Boolean runFull = false;      //used to start either in debug mode or run in automatic mode.

        class MyContext : ApplicationContext
        {
            private Form1 form1;

            private FileStream fileStream;

            public MyContext(FileStream file)
            {
                form1 = new Form1();
                fileStream = file;
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
        static void Main(string[] args)
        {
            String file = @"C:\Users\py120\Desktop\Dev\robotServer\public\uploads\Test.jpg";
            FileInfo fileinfo = new FileInfo(file);
            FileStream fileStream;
            fileStream  = File.Open(file, FileMode.Open, FileAccess.ReadWrite);

            MyContext context = new MyContext(fileStream);
            Application.Run(context);

            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(0);
            }
            
            
        }
    }
}
