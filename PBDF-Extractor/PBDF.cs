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
	}
}
