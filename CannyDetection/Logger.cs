using System;
using System.IO;
using System.Reflection;

namespace CannyDetection
{
    public class Logger
    {
        private string path = string.Empty;

        public Logger(string message)
        {
            log(message);
        }

        public void log(string m)
        {
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter w = File.AppendText(path + "\\" + "log.txt"))
                {
                    Log(m, w);
                }
            }
            catch (Exception e)
            {
                
            }
        }

        public void Log(string m, TextWriter w)
        {
            try
            {
                w.Write("\r\nLog Entry: ");
               // w.WriteLine("{0}  {1}", DateTime.Now.ToLongTimeString(),
                 //                       DateTime.Now.ToLongDateString());
                w.WriteLine(" :{0}", m);
            }
            catch (Exception e)
            {

            }
        }
    }
}
