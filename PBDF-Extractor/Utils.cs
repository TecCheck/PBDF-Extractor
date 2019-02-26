using System;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace PBDF_Extractor
{
	public class Utils
	{
		public enum CoderType
		{
			Xor,
			Spceial
		};

		public static byte[] GetDecodedData(byte[] encodedData, uint coderKey, CoderType coderType)
		{
			if (coderType == CoderType.Xor)
			{
				return DecodeXor(encodedData, coderKey);
			}
			else if (coderType == CoderType.Spceial)
			{
				return DecodeSpecial(encodedData, coderKey);
			}

			return null;
		}

		public static byte[] DecodeXor(byte[] encodedData, uint coderKey)
		{
			byte[] decoded = new byte[encodedData.Length];
			for (int i = 0; i < encodedData.Length; i += 4)
			{
				uint enc = BitConverter.ToUInt32(encodedData, i);
				enc ^= coderKey;
				byte[] dec = BitConverter.GetBytes(enc);
				decoded[i] = dec[0];
				decoded[i + 1] = dec[1];
				decoded[i + 2] = dec[2];
				decoded[i + 3] = dec[3];
			}
			return decoded;
		}

		public static byte[] DecodeSpecial(byte[] encodedData, uint coderKey)
		{
			return null;
		}

		public static string DecodeString(byte[] data, int startIndex)
		{
			byte lenght = data[startIndex];
			byte[] chars = new byte[lenght];
			for (byte index = 0; index < chars.Length; index++)
			{
				int dIndex = startIndex + 1 + index;
				//PrintHex(data[dIndex]);
				int key = ~index;
				int dataB = data[dIndex] ^ key;
				chars[index] = (byte)dataB;
			}
			string output = Encoding.UTF8.GetString(chars);
			return output;
		}


		public static uint[] GetArray(byte[] data, int startIndex)
		{
			uint lenght = BitConverter.ToUInt32(data, startIndex);
			uint[] ints = new uint[lenght];

			for (int index = 0; index < ints.Length; index++)
			{
				int dIndex = startIndex + 4 + index * 4;
				ints[index] = BitConverter.ToUInt32(data, dIndex);
			}
			return ints;
		}

		public static byte[] GetArraySnippet(byte[] input, int startIndex, int endIndex)
		{
			//Console.WriteLine("startInex: " + startIndex);
			int size = endIndex - startIndex;
			if (size > 0)
			{
				byte[] b = new byte[size];
				int index = 0;
				while (startIndex < endIndex)
				{
					b[index] = input[startIndex];
					index++;
					startIndex++;
				}
				return b;
			}
			return null;
		}

		public static void PrintHex(byte b)
		{
			Console.WriteLine(Convert.ToString(b, 16).PadLeft(2, '0'));
		}

		public static void PrintHex(uint i)
		{
			Console.WriteLine(Convert.ToString(i, 16).PadLeft(8, '0'));
		}

		public static void PrintHex(byte[] b)
		{
			string s = "Count: " + b.Length + " {";
			foreach (byte by in b)
			{
				s += Convert.ToString(by, 16).PadLeft(2, '0') + ",";
			}

			s += "}";
			s.Replace(",}", "}");
			Console.WriteLine(s);
		}

		public static int GetByteLenght(uint[] i)
		{
			return (i.Length + 1) * 4;
		}

		public static int GetByteLenght(byte[] b)
		{
			return b.Length + 1;
		}

		public static int GetByteLenght(string s)
		{
			return s.Length + 1;
		}

		public static Color GetColor(ushort rgb)
		{
			string s = Convert.ToString(rgb, 2).PadLeft(16, '0');
			//Console.WriteLine(s);
			string r = s.Substring(0, 5).PadLeft(8, '0');
			string g = s.Substring(5, 6).PadLeft(8, '0');
			string b = s.Substring(11, 5).PadLeft(8, '0');

			string rgbS = r + g + b;
			//Console.WriteLine(s);
			//Console.WriteLine(r + "," + g + "," + b);

			byte rb = Convert.ToByte(r, 2);
			rb *= 8;
			byte gb = Convert.ToByte(g, 2);
			gb *= 4;
			byte bb = Convert.ToByte(b, 2);
			bb *= 8;

			Color c = Color.FromArgb(rb, gb, bb);
			return c;
		}

		public static double GetDouble(byte[] data, int startIndex)
		{
			int fractionalBits = 16;
			double dQNumber;
			bool isNegative = false;
			int iCodeWord = (int)new BigInteger(GetArraySnippet(data, startIndex, startIndex + 4));
			if (iCodeWord < 0)
			{
				isNegative = true;
				iCodeWord = ~iCodeWord + 1;
			}
			dQNumber = iCodeWord * Math.Pow(2, -fractionalBits);
			/* If negative bit is on, flip answer to negative */
			if (isNegative)
				dQNumber *= -1;
			return dQNumber;
		}
	}
}
