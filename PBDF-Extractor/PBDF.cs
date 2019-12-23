/* Thanks to Nico Bendlins for his documentations.
 * http://www.bendlins.de/nico/pod/
 */

using System.Collections.Generic;
using System.IO;
using static PBDF_Extractor.Utils;

namespace PBDF_Extractor
{
	public abstract class PBDF
	{
		//General Filetype information
		public static uint blockSize;
		public static uint coderKey;
		public static CoderType coderType;

		//Header information
		public uint fileSize;
		public uint[] offsetTable; // count = offsetCount;

		public List<PBDFBlock> blocks;
		public byte[] decodedData;

		public PBDF(string fileName)
		{

		}

		public void LoadData(string fileName, CoderType coderType, uint coderKey, uint blockSize)
		{
			byte[] fileData = File.ReadAllBytes(fileName);
			LoadData(fileData, coderType, coderKey, blockSize);
		}

		public void LoadData(byte[] fileData, CoderType coderType, uint coderKey, uint blockSize)
		{
			//writing encoded Data into blocks
			byte[] block = new byte[blockSize];
			uint blockByteIndex = 0;
			uint index = 0;
			blocks = new List<PBDFBlock>();
			while (index < fileData.Length)
			{
				block[blockByteIndex] = fileData[index];
				index++;
				blockByteIndex++;
				if (blockByteIndex == blockSize)
				{
					blocks.Add(new PBDFBlock(blockSize, block, coderKey, coderType));
					blockByteIndex = 0;
					block = new byte[blockSize];
				}
			}

			//writing decoded data from blocks into an array (without checksums)
			long byteCount = blocks[0].GetData().Length * blocks.Count;
			decodedData = new byte[byteCount];
			int decodedDataIndex = 0;
			foreach (PBDFBlock pbdfBlock in blocks)
			{
				byte[] pbdfData = pbdfBlock.GetData();
				int dataLenght = pbdfData.Length;
				for (int i = 0; i < dataLenght; i++)
				{
					decodedData[decodedDataIndex] = pbdfData[i];
					decodedDataIndex++;
				}
			}
			string s = "";
			for (int i = 0; i < 16; i++)
			{
				s += decodedData[i] + ", ";
			}
		}
	}
}