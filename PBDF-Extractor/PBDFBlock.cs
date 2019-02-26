using System;
using static PBDF_Extractor.Utils;

namespace PBDF_Extractor
{
	public class PBDFBlock
	{
		uint blockSize;
		uint checksum;
		byte[] data;

		public PBDFBlock(uint blockSize, byte[] dataIn, uint coderKey, CoderType coderType)
		{
			this.blockSize = blockSize;
			data = dataIn;
			data = GetDecodedData(data, coderKey, coderType);
			byte[] realData = new byte[data.Length - 4];

			for(int i = 0; i < realData.Length; i++)
			{
				realData[i] = data[i];
			}
			checksum = BitConverter.ToUInt32(data, data.Length - 4);
			data = realData;
		}

		public void SetBlockSize(uint blockSize)
		{
			this.blockSize = blockSize;
		}

		public uint GetBlockSize()
		{
			return blockSize;
		}

		public void SetChecksum(uint checksum)
		{
			this.checksum = checksum;
		}

		public uint GetChecksum()
		{
			return checksum;
		}

		public uint CalculateChecksum()
		{
			return 0;
		}

		public void SetData(byte[] data)
		{
			this.data = data;
		}

		public byte[] GetData()
		{
			return data;
		}

	}
}
