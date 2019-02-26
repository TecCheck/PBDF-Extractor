using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBDF_Extractor
{
	class Bl4 : PBDF
	{
		public Bl4(string fileName) : base(fileName)
		{
			byte[] fileData = File.ReadAllBytes(fileName);
			LoadData(fileData);
			Console.WriteLine("fileSize: " + fileData.Length);
			Console.WriteLine("decodedData: " + decodedData.Length);
			AnalyzeData();
		}

		void AnalyzeData()
		{

		}
	}
}
