using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static PBDF_Extractor.Utils;

namespace PBDF_Extractor
{
	public class Img : PBDF
	{
		public static uint blockSize = 0x00004000;
		public static uint coderKey = 0x00000F2E;
		public static CoderType coderType = CoderType.Xor;

		public uint imageCount;
		public uint pixelBufferSize;
		public uint paletteCount;
		Image[] images;
		ushort[,] paletteData;
		byte[] pixelBuffer;

		int currentIndex = 0;

		public Img(string fileName) : base(fileName)
		{
            //LoadData(fileName, coderType, coderKey, blockSize);
            decodedData = File.ReadAllBytes(fileName);

            ParseData();
		}

        public void SaveFiles(string path)
        {
            foreach(Image image in images)
            {
                image.bitmap.Save(path + image.imageID + ".png", ImageFormat.Png);
            }
        }

		void ParseData()
		{
            imageCount = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			pixelBufferSize = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			paletteCount = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;

            images = new Image[imageCount];

            for(int i = 0; i < imageCount; i++)
            {
                Image image = new Image();
                image.widht = BitConverter.ToUInt16(decodedData, currentIndex);
                currentIndex += 2;
                image.height = BitConverter.ToUInt16(decodedData, currentIndex);
                currentIndex += 2;

                image.imageID = BitConverter.ToUInt32(decodedData, currentIndex);
                currentIndex += 4;

                image.pixelOffset = BitConverter.ToUInt32(decodedData, currentIndex);
                currentIndex += 4;

                image.palleteIndex = BitConverter.ToInt16(decodedData, currentIndex);
                currentIndex += 2;

                image.reserved = BitConverter.ToUInt16(decodedData, currentIndex);
                currentIndex += 2;

                images[i] = image;
            }

            paletteData = new ushort[paletteCount, 265];

            for (int i = 0; i < paletteCount; i++)
            {
                for(int i2 = 0; i2 < 256; i2++)
                {
                    paletteData[i, i2] = BitConverter.ToUInt16(decodedData, currentIndex);
                    // Console.WriteLine("Color: #" + Convert.ToString(GetColor555(paletteData[i, i2]).ToArgb(), 16));
                    currentIndex += 2;
                }
            }

            pixelBuffer = new byte[pixelBufferSize];

            for(int i = 0; i < pixelBufferSize; i++)
            {
                pixelBuffer[i] = decodedData[currentIndex];
                // Console.WriteLine("Pixel: " + pixelBuffer[i]);
                currentIndex++;
            }

            foreach(Image image in images)
            {
                image.bitmap = new Bitmap(Convert.ToInt32(image.widht), Convert.ToInt32(image.height), PixelFormat.Format16bppRgb555);

                // direct color info
                if (image.palleteIndex == -1)
                {
                    int i = 0;
                    for (int y = 0; y < image.bitmap.Height; y++)
                    {
                        for (int x = 0; x < image.bitmap.Width; x++)
                        {
                            int offset = Convert.ToInt32(image.pixelOffset + i);
                            image.bitmap.SetPixel(x, y, GetColor555(BitConverter.ToUInt16(pixelBuffer, offset)));
                            i += 2;
                        }
                    }
                }
                // color info from palette
                else
                {
                    int i = 0;
                    for (int y = 0; y < image.bitmap.Height; y++)
                    {
                        for (int x = 0; x < image.bitmap.Width; x++)
                        {
                            image.bitmap.SetPixel(x, y, GetColor555(paletteData[image.palleteIndex, pixelBuffer[image.pixelOffset + i]]));
                            i++;
                        }
                    }
                }
            }
		}
	}

	public class Image
	{
		public uint widht;
        public uint height;
        public uint imageID;
        public uint pixelOffset;
        public short palleteIndex;
        public ushort reserved;
        public Bitmap bitmap;
	}
}
