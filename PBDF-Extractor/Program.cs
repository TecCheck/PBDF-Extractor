using System;
using System.Threading;
using System.IO;

namespace PBDF_Extractor
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach(string file in Directory.EnumerateFiles(@"C:\Users\Christoph\Projekte\podFiles\POD GOLD\DATA\BINARY\VOITURES\"))
			{
				Bv4 bv4_1 = new Bv4(file);
				string s = bv4_1.name;
				s = s.ToLower();
				s = s.Substring(0,1).ToString().ToUpper() + s.Substring(1);

				Console.WriteLine("fileNameToSave: " + @"C:\Users\Christoph\Projekte\podFiles\Cars\" + s);
				bv4_1.SaveAllData(@"C:\Users\Christoph\Projekte\podFiles\Cars\" + s);
			}
			/*
			Bv4 bv4 = new Bv4(@"C:\Users\Christoph\Projekte\podFiles\POD GOLD\DATA\BINARY\VOITURES\buffo.bv4");

			Console.WriteLine();
			bv4.SaveAllData(@"C:\Users\Christoph\Projekte\podFiles\Cars\Buffo");
			*/

			Console.WriteLine("\n-----End-----");
			while (true)
			{
				Thread.Sleep(20000);
			}
		}
	}
}
