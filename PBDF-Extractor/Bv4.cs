/* Thanks to Nico Bendlins for his documentations.
 * http://www.bendlins.de/nico/pod/
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using static PBDF_Extractor.Utils;
using System.Drawing.Imaging;
using System.Threading;
using static PBDF_Extractor.Bv4;

namespace PBDF_Extractor
{
	public class Bv4 : PBDF
	{
		public enum ObjectType
		{
			chassis,
			wheel,
			shadow,
			colission
		};

		public static string[] objNames = {"Chassis[good,rear,right]","Chassis[good,rear,left]","Chassis[good,side,right]","Chassis[good,side,left]","Chassis[good,front,right]","Chassis[good,front,left]",
			"Chassis[damaged,rear,right]","Chassis[damaged,rear,left]","Chassis[damaged,side,right]","Chassis[damaged,side,left]","Chassis[damaged,front,right]","Chassis[damaged,front,left]",
			"Chassis[ruined,rear,right]","Chassis[ruined,rear,left]","Chassis[ruined,side,right]","Chassis[ruined,side,left]","Chassis[ruined,front,right]","Chassis[ruined,front,left]",
			"Wheel[front,right]", "Wheel[rear,right]", "Wheel[front,left]", "Wheel[rear,left]",
			"Shadow[good,front]", "Shadow[good,rear]", "Shadow[ruined,front]", "Shadow[ruined,rear]", "Colission1", "Colission2"};

		public static uint blockSize = 0x00004000;
		public static uint coderKey = 0x00000F2E;
		public static CoderType coderType = CoderType.Xor;

		public List<PBDFBlock> blocks = new List<PBDFBlock>();

		public int currentIndex = 0;

		public string name = "";
		public Material material = new Material();
		public Objects objects = new Objects();
		public Noise noise = new Noise();
		public Characteristics characteristics = new Characteristics();

		public double[] positions = new double[15];

		public Bv4(string fileName) : base(fileName)
		{
			byte[] fileData = File.ReadAllBytes(fileName);
			LoadData(fileData);
			Console.WriteLine("fileSize: " + fileData.Length);
			Console.WriteLine("decodedData: " + decodedData.Length);
			AnalyzeData();
		}

		public void SaveAllData(string path)
		{
			Console.WriteLine("save " + name + " to " + path);
			SaveImages(path, ImageFormat.Png);
			SaveCharacteristics(path);
			Save3DModels(path);
			//Thread.Sleep(2000);
		}

		public void SaveImages(string path, ImageFormat format)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			material.textures[0].texture.Save(path + "\\" + name + "_0.png", format);
			material.textures[1].texture.Save(path + "\\" + name + "_1.png", format);
		}

		public void SaveCharacteristics(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			string s = "Acceleration: " + characteristics.acceleration + ";\n"
				+ "Brakes: " + characteristics.brakes + ";\n"
				+ "Grip: " + characteristics.grip + ";\n"
				+ "Handling: " + characteristics.handling + ";\n"
				+ "Speed: " + characteristics.speed + ";";

			File.WriteAllText(path + "\\" + name + "_char.txt", s);
		}

		public void Save3DModels(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			List<string> file = new List<string>();
			file.Add("# This is a Pod Gold car model");
			file.Add("");
			file.Add("mtllib " + name + ".mtl");

			int objIndex = 0;

			int lastVertexCount = 0;
			int lastUvCount = 0;
			int wheelIndex = 0;

			foreach (Object obj in objects.objects)
			{
				if (obj.objectType == ObjectType.chassis || obj.objectType == ObjectType.wheel)
				{
					List<string> vertexes = new List<string>();
					List<string> uvs = new List<string>();
					List<string> faces = new List<string>();

					for (int i = 0; i < obj.vertexArray.Length / 3; i++)
					{
						if (obj.objectType == ObjectType.wheel)
						{
							double p0 = obj.vertexArray[i, 0] + positions[wheelIndex];
							double p1 = obj.vertexArray[i, 1] + positions[wheelIndex + 1];
							double p2 = obj.vertexArray[i, 2] + positions[wheelIndex + 2];

							vertexes.Add("v " + p0 + " " + p1 + " " + p2);
						}
						else
						{
							vertexes.Add("v " + obj.vertexArray[i, 0] + " " + obj.vertexArray[i, 1] + " " + obj.vertexArray[i, 2]);
						}
					}

					foreach (OFace face in obj.faces)
					{
						int n0 = uvs.Count + 1 + lastUvCount;
						int n1 = uvs.Count + 2 + lastUvCount;
						int n2 = uvs.Count + 3 + lastUvCount;
						int n3 = uvs.Count + 4 + lastUvCount;

						int i0 = (int)face.indices[0] + 1 + lastVertexCount;
						int i1 = (int)face.indices[1] + 1 + lastVertexCount;
						int i2 = (int)face.indices[2] + 1 + lastVertexCount;
						int i3 = (int)face.indices[3] + 1 + lastVertexCount;

						string s = "";
						s = "f " + i0 + "/" + n0 + " " + i1 + "/" + n1 + " " + i2 + "/" + n2 + " ";

						if (face.vertices == 4)
							s += i3 + "/" + n3;

						faces.Add("usemtl " + name + face.textureIndex);
						faces.Add(s);

						for (int i = 0; i < face.textureUV.Length / 2; i++)
						{
							double uv0 = face.textureUV[i, 0] * 0.003921;
							//uv0 = Math.Round(uv0, 2);
							double uv1 = face.textureUV[i, 1] * 0.003921;
							//uv1 = Math.Round(uv1, 2);
							uvs.Add("vt " + uv0 + " " + uv1);
						}

					}

					file.Add("");
					file.Add("o group" + Convert.ToString(objIndex).PadLeft(2, '0'));
					file.Add("g 	" + objNames[objIndex].ToLower());


					foreach (string s in vertexes)
					{
						file.Add(s);
					}

					foreach (string s in uvs)
					{
						file.Add(s);
					}

					file.Add("s off");

					foreach (string s in faces)
					{
						file.Add(s);
					}


					lastUvCount += uvs.Count;
					lastVertexCount += vertexes.Count;
				}
				objIndex++;
				if (obj.objectType == ObjectType.wheel)
					wheelIndex += 3;
			}

			string[] fileA = file.ToArray();

			File.WriteAllLines(path + "\\" + name + ".obj", fileA);

			//mtl
			string[] mtl = { "# This is a Pod Gold car material",
			"",
			"newmtl " + name + "0",
			"Ka 1.000000 1.000000 1.000000",
			"Kd 0.640000 0.640000 0.640000",
			"Ks 0.500000 0.500000 0.500000",
			"Ke 0.000000 0.000000 0.000000",
			"Ni 1.000000",
			"d 1.00000",
			"illum 2",
			"map_Kd " + path + "\\" + name + "_0.png",
			"",
			"newmtl " + name + "0",
			"Ka 1.000000 1.000000 1.000000",
			"Kd 0.640000 0.640000 0.640000",
			"Ks 0.500000 0.500000 0.500000",
			"Ke 0.000000 0.000000 0.000000",
			"Ni 1.000000",
			"d 1.00000",
			"illum 2",
			"map_Kd " + path + "\\" + name + "_1.png"};

			File.WriteAllLines(path + "\\" + name + ".mtl", mtl);
		}

		void AnalyzeData()
		{

			//Header
			fileSize = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			Console.WriteLine("fileSize: " + fileSize);

			offsetTable = GetArray(decodedData, currentIndex);
			currentIndex += 4 + 4 * offsetTable.Length;

			//Name
			name = DecodeString(decodedData, currentIndex);
			currentIndex += GetByteLenght(name);
			Console.WriteLine("name: " + name);

			GetPositions();

			//Skip section
			//currentIndex += 1368; // 1080 + 288
			currentIndex += 288;
			currentIndex += (1 + GetArray(decodedData, currentIndex).Length) * 4;

			GetMaterials();
			Get3DModels();
			GetMisc();
		}

		void GetPositions()
		{
			for (int i = 0; i < 5; i++)
			{
				currentIndex += 72;

				positions[i * 3] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				positions[i * 3 + 1] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				positions[i * 3 + 2] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;

				currentIndex += 132;
			}
		}

		void GetMaterials()
		{
			//Materials and Textures
			material.name = DecodeString(decodedData, currentIndex);
			//Console.WriteLine("Mname: " + material.name);
			currentIndex += GetByteLenght(material.name);

			if (!material.name.Equals("GOURAUD"))
			{
				//textureCount
				material.textureCount = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;
				//textureFlags
				material.textureFlags = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;
				//textures
				material.textures = new MTexture[material.textureCount];
				for (int i = 0; i < material.textureCount; i++)
				{
					MTexture tex = new MTexture();
					//images
					tex.images = new MImage[(int)BitConverter.ToUInt32(decodedData, currentIndex)];
					currentIndex += 4;
					for (int i2 = 0; i2 < tex.images.Length; i2++)
					{
						MImage image = new MImage();
						image.name = Encoding.UTF8.GetString(GetArraySnippet(decodedData, currentIndex, currentIndex + 32));
						image.coordinates = new uint[4];
						currentIndex += 32;
						image.coordinates[0] = BitConverter.ToUInt32(decodedData, currentIndex);
						currentIndex += 4;
						image.coordinates[1] = BitConverter.ToUInt32(decodedData, currentIndex);
						currentIndex += 4;
						image.coordinates[2] = BitConverter.ToUInt32(decodedData, currentIndex);
						currentIndex += 4;
						image.coordinates[3] = BitConverter.ToUInt32(decodedData, currentIndex);
						currentIndex += 4;
						image.imageFlags = BitConverter.ToUInt32(decodedData, currentIndex);
						currentIndex += 4;
						tex.images[i] = image;
					}
					material.textures[i] = tex;
				}

				for (int i = 0; i < material.textureCount; i++)
				{
					Bitmap bitmap = new Bitmap(128, 128);
					for (int i2 = 0; i2 < 128; i2++)
					{
						for (int i3 = 0; i3 < 128; i3++)
						{
							ushort s = BitConverter.ToUInt16(decodedData, currentIndex);
							//Console.WriteLine("x: " + i2 + ", y: " + i3);
							currentIndex += 2;
							//Console.WriteLine("currentIndex: " + currentIndex);
							int x = i3;
							int y = (i2 - 127) * -1;
							bitmap.SetPixel(x, y, GetColor(s));
						}
					}
					material.textures[i].texture = bitmap;
					//bitmap.Save(@"C:\Users\Christoph\Projekte\podFiles\test" + i + ".png", ImageFormat.Png);
					//material.textures[i] = tex;
				}
			}
		}

		void Get3DModels()
		{
			//3d Models

			objects.namedFaces = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			int objCount = 22;
			int shadowCount = 4;
			int collissionMeshCount = 2;
			int objIndex = 0;
			objects.objects = new Object[28];
			//Console.WriteLine("currentIndex: " + currentIndex);

			//Objects
			for (int i = 0; i < objCount; i++)
			{
				//Console.WriteLine();
				//Console.WriteLine("Object " + i);
				//Console.WriteLine("Object Index: " + objIndex);
				//Vertextes
				GetVertices(objIndex);
				//Faces
				GetFaces(objIndex);
				//Normals
				GetNormals(objIndex);

				GetPrism(objIndex);
				if (objIndex < 18)
				{
					objects.objects[objIndex].objectType = ObjectType.chassis;
				}
				else
				{
					objects.objects[objIndex].objectType = ObjectType.wheel;
				}
				objIndex++;
			}

			//Shadows
			for (int i = 0; i < shadowCount; i++)
			{
				//Console.WriteLine();
				//Console.WriteLine("Shadow " + i);
				//Console.WriteLine("Object Index: " + objIndex);
				//Vertextes
				GetVertices(objIndex);
				//Faces
				GetFaces(objIndex);
				//Normals
				GetNormals(objIndex);
				objects.objects[objIndex].objectType = ObjectType.shadow;
				objIndex++;
			}

			//Colission Meshes
			for (int i = 0; i < collissionMeshCount; i++)
			{
				//Console.WriteLine();
				//Console.WriteLine("Colission " + i);
				//Console.WriteLine("Object Index: " + objIndex);

				objects.objects[objIndex] = new Object();
				objects.objects[objIndex].materialName = DecodeString(decodedData, currentIndex);
				currentIndex += GetByteLenght(objects.objects[objIndex].materialName);
				//Console.WriteLine("name: " + objects.objects[objIndex].materialName);

				objects.objects[objIndex].namedFaces = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;
				//Vertextes
				GetVertices(objIndex);
				//Faces
				GetFaces(objIndex);
				//Normals
				GetNormals(objIndex);

				GetPrism(objIndex);
				//currentIndex += 28;
				currentIndex += 24;
				uint ui1 = BitConverter.ToUInt32(decodedData, currentIndex);
				//Console.WriteLine("uint32_t Unknown_0030 = " + ui1);
				currentIndex += (64 * (int)ui1) + 4;
				objects.objects[objIndex].objectType = ObjectType.colission;
				objIndex++;
			}
		}

		void GetPrism(int i)
		{
			//prism
			for (int i2 = 0; i2 < 28; i2++)
			{
				objects.objects[i].prism[i2] = decodedData[i2];
				currentIndex++;
			}
		}

		void GetNormals(int i)
		{
			//Normals
			objects.objects[i].normals = new double[objects.objects[i].vertexCount, 3];
			for (int i2 = 0; i2 < objects.objects[i].vertexCount; i2++)
			{
				objects.objects[i].normals[i2, 0] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].normals[i2, 1] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].normals[i2, 2] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
			}
			//unknown
			currentIndex += 4;
		}

		void GetVertices(int i)
		{
			//VertexCount
			objects.objects[i] = new Object();
			objects.objects[i].vertexCount = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			Console.WriteLine("vertexCount: " + objects.objects[i].vertexCount);
			objects.objects[i].vertexArray = new double[objects.objects[i].vertexCount, 3];

			//Vertexes
			for (int i2 = 0; i2 < objects.objects[i].vertexCount; i2++)
			{
				objects.objects[i].vertexArray[i2, 0] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].vertexArray[i2, 1] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].vertexArray[i2, 2] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				//Console.WriteLine("test");
			}
			objects.objects[i].faceCount = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			objects.objects[i].triangleCount = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			objects.objects[i].quadrangleCount = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
		}

		void GetFaces(int i)
		{
			//Faces
			objects.objects[i].faces = new OFace[objects.objects[i].faceCount];
			//Console.WriteLine("FaceCount: " + objects.objects[i].faceCount);
			for (int i2 = 0; i2 < objects.objects[i].faceCount; i2++)
			{
				objects.objects[i].faces[i2] = new OFace();
				if (objects.namedFaces == 1)
				{
					objects.objects[i].faces[i2].name = DecodeString(decodedData, currentIndex);
					currentIndex += GetByteLenght(objects.objects[i].faces[i2].name);
					//Console.WriteLine("MName: " + objects.objects[i].faces[i2].name);
				}
				objects.objects[i].faces[i2].vertices = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;

				//indicies
				objects.objects[i].faces[i2].indices[0] = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].faces[i2].indices[1] = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].faces[i2].indices[2] = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].faces[i2].indices[3] = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;

				//normals
				objects.objects[i].faces[i2].normal[0] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].faces[i2].normal[1] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;
				objects.objects[i].faces[i2].normal[2] = GetDouble(decodedData, currentIndex);
				currentIndex += 4;

				//material type
				objects.objects[i].faces[i2].materialType = DecodeString(decodedData, currentIndex);
				currentIndex += GetByteLenght(objects.objects[i].faces[i2].materialType);
				//Console.WriteLine("MType: " + objects.objects[i].faces[i2].materialType + "\n");

				//shadign color + textureIndex
				//objects.objects[i].faces[i2].shadingColor = BitConverter.ToUInt32(decodedData, currentIndex);
				//currentIndex += 4;
				objects.objects[i].faces[i2].textureIndex = BitConverter.ToUInt32(decodedData, currentIndex);
				currentIndex += 4;

				//UVs
				for (int i3 = 0; i3 < 4; i3++)
				{
					objects.objects[i].faces[i2].textureUV[i3, 0] = BitConverter.ToUInt32(decodedData, currentIndex);
					currentIndex += 4;
					objects.objects[i].faces[i2].textureUV[i3, 1] = BitConverter.ToUInt32(decodedData, currentIndex);
					currentIndex += 4;
				}
				//unknown
				currentIndex += 4;
				//quadReserved
				if (objects.objects[i].faces[i2].vertices == 4)
				{
					objects.objects[i].faces[i2].quadReserved[0] = GetDouble(decodedData, currentIndex);
					currentIndex += 4;
					objects.objects[i].faces[i2].quadReserved[1] = GetDouble(decodedData, currentIndex);
					currentIndex += 4;
					objects.objects[i].faces[i2].quadReserved[2] = GetDouble(decodedData, currentIndex);
					currentIndex += 4;
				}
				//flags
				objects.objects[i].faces[i2].flags1 = decodedData[currentIndex];
				currentIndex++;
				objects.objects[i].faces[i2].flags2 = decodedData[currentIndex];
				currentIndex++;
				//unknown
				currentIndex++;
				currentIndex++;
			}
		}

		void GetMisc()
		{
			//Noise
			noise.runtime = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			currentIndex += 30;
			noise.reserved = BitConverter.ToUInt16(decodedData, currentIndex);
			currentIndex += 2;

			//Characteristics
			characteristics.acceleration = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			Console.WriteLine("Acceleration: " + characteristics.acceleration);
			characteristics.brakes = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			Console.WriteLine("Brakes: " + characteristics.brakes);
			characteristics.grip = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			Console.WriteLine("Grip: " + characteristics.grip);
			characteristics.handling = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			Console.WriteLine("Handling: " + characteristics.handling);
			characteristics.speed = BitConverter.ToUInt32(decodedData, currentIndex);
			currentIndex += 4;
			Console.WriteLine("Speed: " + characteristics.speed);
		}

		void LoadData(byte[] fileData)
		{
			//writing encoded Data into blocks
			byte[] block = new byte[blockSize];
			uint blockByteIndex = 0;
			uint index = 0;
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

	public class Noise
	{
		public uint runtime;
		public ushort reserved;
	}

	public class Characteristics
	{
		public uint acceleration;
		public uint brakes;
		public uint grip;
		public uint handling;
		public uint speed;
	}

	public class Material
	{
		public string name;
		public uint textureCount;
		public uint textureFlags;
		public MTexture[] textures;
	}

	public class MTexture
	{
		public MImage[] images;
		public Bitmap texture;
	}

	public class MImage
	{
		public string name;
		public uint[] coordinates;
		public uint imageFlags;
	}

	public class Objects
	{
		public uint namedFaces;
		public Object[] objects;
		//for Colission Meshes
	}

	public class Object
	{
		public ObjectType objectType;
		public uint vertexCount;
		public double[,] vertexArray;
		public uint faceCount;
		public uint triangleCount;
		public uint quadrangleCount;
		public OFace[] faces;
		public double[,] normals;
		public uint unknown;
		public byte[] prism = new byte[28];

		//ColissionMeshes
		public string materialName;
		public uint namedFaces;
	}

	public class OFace
	{
		public string name;
		public uint vertices;
		public uint[] indices = new uint[4];
		public double[] normal = new double[3];
		public string materialType;
		//MaterialData
		public uint shadingColor;
		public uint textureIndex;
		public uint[,] textureUV = new uint[4, 2];
		public uint unknown;
		public double[] quadReserved = new double[3];
		//Face Properies
		public byte flags1;
		public byte flags2;
		public byte[] unknown2 = new byte[2];
	}
}
