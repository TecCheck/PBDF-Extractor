using System;
using System.Threading;
using System.IO;

namespace PBDF_Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            //LoadBl4();
            LoadImg1();

            Console.WriteLine("\n-----End-----");
            while (true)
            {
                Thread.Sleep(20000);
            }
        }

        static void LoadBv4()
        {
            foreach (string file in Directory.EnumerateFiles(@"C:\Users\Christoph\Projekte\podFiles\POD GOLD\DATA\BINARY\VOITURES\"))
            {
                Bv4 bv4 = new Bv4(file);
                string s = bv4.name;
                s = s.ToLower();
                s = s.Substring(0, 1).ToString().ToUpper() + s.Substring(1);

                Console.WriteLine("fileNameToSave: " + @"C:\Users\Christoph\Projekte\podFiles\Cars\" + s);
                bv4.SaveAllData(@"C:\Users\Christoph\Projekte\podFiles\Cars\" + s);
            }
        }

        static void LoadBl4()
        {
            Bl4 bl4 = new Bl4(@"C:\Users\Christoph\Projekte\podFiles\POD GOLD\DATA\BINARY\CIRCUITS\CITY.BL4");
        }

        static void LoadImg()
        {
            Img img = new Img(@"C:\Users\Christoph\Projekte\podFiles\POD GOLD\DATA\BINARY\MIMG\ALDERON.IMG");
            img.SaveFiles(@"C:\Users\Christoph\Projekte\podFiles\Export\UI\ALDERON.IMG");
        }

        static void LoadImg1()
        {
            foreach (string file in Directory.EnumerateFiles(@"C:\Users\Christoph\Projekte\podFiles\POD GOLD\DATA\BINARY\MIMG\"))
            {

                Console.WriteLine(file);
                Img img = new Img(file);
                string s = file.Substring(file.LastIndexOf("\\") + 1);
                //img.SaveFiles(@"C:\Users\Christoph\Projekte\podFiles\Export\UI\" + s);
                Console.WriteLine("\tdone");
            }
        }
    }
}
