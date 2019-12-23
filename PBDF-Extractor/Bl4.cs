using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PBDF_Extractor.Utils;

namespace PBDF_Extractor
{
	class Bl4 : PBDF
	{
		public static uint blockSize = 0x00001000;
		public static uint coderKey = 0x00000F7E;
		public static CoderType coderType = CoderType.Xor;

		public Bl4(string fileName) : base(fileName)
		{
			byte[] fileData = File.ReadAllBytes(fileName);
			LoadData(fileData, coderType, coderKey, blockSize);
			Console.WriteLine("fileSize: " + fileData.Length);
			Console.WriteLine("decodedData: " + decodedData.Length);
			AnalyzeData();
		}

		void AnalyzeData()
		{

		}
	}


}
