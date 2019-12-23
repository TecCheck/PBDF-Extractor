using System;
using System.Threading;
using System.IO;

namespace PBDF_Extractor
{
    class Program
    {
        public static readonly string[] filetypes = { "Bv4", "Img" };

        public static bool log = false;

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                StartWithoutParams();
                return;
            }

            // WIP
        }

        static void StartWithoutParams()
        {
            PrintSplash();
            int filetype = ReadFileType();
            if (filetype >= filetypes.Length)
            {
                Console.WriteLine("Filetype not found");
                return;
            }

            bool folder = ReadFolder();

            string input = ReadInput(folder);
            if (folder)
                if (!Directory.Exists(input))
                {
                    Console.WriteLine("Input folder does not exist");
                    return;
                }
                else
                if (!File.Exists(input))
                {
                    Console.WriteLine("Input file does not exist");
                    return;
                }

            string output = ReadOutput();
            if (!Directory.Exists(output))
            {
                Console.WriteLine("Output folder does not exist");
                return;
            }

            log = ReadLog();

            if (folder)
            {
                if (filetype == 0)
                {
                    ConvertBv4Folder(input, output);
                }
                else if (filetype == 1)
                {
                    ConvertImgFolder(input, output);
                }
            }
            else
            {
                if (filetype == 0)
                {
                    ConvertBv4File(input, output);
                }
                else if (filetype == 1)
                {
                    ConvertImgFile(input, output);
                }
            }

            Console.WriteLine("\nPress any button to exit");
            Console.ReadLine();
        }

        static void PrintSplash()
        {
            Console.WriteLine(Resources.Splash);
            Console.WriteLine();
            Console.WriteLine(Resources.SplashUnderline);
            Console.WriteLine();
            Console.WriteLine(Resources.Website);
            Console.WriteLine();
            Console.WriteLine(Resources.HelpHint);
            Console.WriteLine();
        }

        static int ReadFileType()
        {
            Console.WriteLine(Resources.FiletypesHeadline);

            for (int i = 0; i < filetypes.Length; i++)
            {
                Console.WriteLine((i + 1) + ". " + filetypes[i]);
            }

            return Convert.ToInt32(Console.ReadLine()) - 1;
        }

        static bool ReadFolder()
        {
            Console.WriteLine(Resources.Folder);
            string s = Console.ReadLine();
            if (s.Equals("y", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        static string ReadInput(bool folder)
        {
            Console.WriteLine(Resources.Input + (folder ? " folder: " : " file: "));
            string s = Console.ReadLine();

            if (folder)
                if (!s.EndsWith("\\"))
                    s += "\\";

            return s;
        }

        static string ReadOutput()
        {
            Console.WriteLine(Resources.Output + " folder: ");
            string s = Console.ReadLine();

            if (!s.EndsWith("\\"))
                s += "\\";

            return s;
        }

        static bool ReadLog()
        {
            Console.WriteLine(Resources.Log);
            string s = Console.ReadLine();
            if (s.Equals("y", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        static void ConvertBv4Folder(string input, string output)
        {
            foreach (string file in Directory.EnumerateFiles(input))
            {
                string filename = file.Substring(file.LastIndexOf("\\") + 1);
                Console.WriteLine("Converting " + filename);

                Bv4 bv4 = new Bv4(file);
                string s = bv4.name.ToLower();
                s = s.Substring(0, 1).ToString().ToUpper() + s.Substring(1);

                bv4.SaveAllData(output + s);

                Console.WriteLine("done\r\n");
            }
        }

        static void ConvertBv4File(string input, string output)
        {
            string filename = input.Substring(input.LastIndexOf("\\") + 1);
            Console.WriteLine("Converting " + filename);

            Bv4 bv4 = new Bv4(input);
            string s = bv4.name.ToLower();
            s = s.Substring(0, 1).ToString().ToUpper() + s.Substring(1);

            bv4.SaveAllData(output + s);

            Console.WriteLine("done\r\n");
        }

        static void ConvertImgFolder(string input, string output)
        {
            foreach (string file in Directory.EnumerateFiles(input))
            {
                string filename = file.Substring(file.LastIndexOf("\\") + 1);
                Console.WriteLine("Converting " + filename);

                Img img = new Img(file);
                img.SaveFiles(output + filename);

                Console.WriteLine("done\r\n");
            }
        }

        static void ConvertImgFile(string input, string output)
        {
            string filename = input.Substring(input.LastIndexOf("\\") + 1);
            Console.WriteLine("Converting " + filename);

            Img img = new Img(input);
            img.SaveFiles(output + filename);

            Console.WriteLine("done\r\n");
        }
    }
}
