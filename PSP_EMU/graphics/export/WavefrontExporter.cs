/*
This file is part of pspsharp.

pspsharp is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

pspsharp is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with pspsharp.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace pspsharp.graphics.export
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.PRIM_TRIANGLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.PRIM_TRIANGLE_STRIPS;


	using Logger = org.apache.log4j.Logger;


	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class WavefrontExporter : IGraphicsExporter
	{
		private static Logger log = VideoEngine.log_Renamed;
		// Use "." as decimal separator for floating point values
		private static Locale l = Locale.ENGLISH;
		private GeContext context;
		private System.IO.StreamWriter exportObj;
		private System.IO.StreamWriter exportMtl;
		private int exportVertexCount;
		private int exportTextureCount;
		private int exportNormalCount;
		private int exportModelCount;
		private int exportMaterialCount;
		// Blender does not import normals (as of blender 2.63)
		private const bool exportNormal = false;

		protected internal virtual void exportObjLine(string line)
		{
			if (exportObj != null)
			{
				try
				{
					exportObj.Write(line);
					exportObj.newLine();
				}
				catch (IOException e)
				{
					log.error("Error writing export.obj file", e);
				}
			}
		}

		protected internal virtual void exportMtlLine(string line)
		{
			if (exportMtl != null)
			{
				try
				{
					exportMtl.Write(line);
					exportMtl.newLine();
				}
				catch (IOException e)
				{
					log.error("Error writing export.mtl file", e);
				}
			}
		}

		public static string ExportDirectory
		{
			get
			{
				for (int i = 1; true; i++)
				{
					string directory = string.Format("{0}Export-{1:D}{2}", IGraphicsExporter_Fields.exportDirectory, i, System.IO.Path.DirectorySeparatorChar);
					if (!System.IO.Directory.Exists(directory) || System.IO.File.Exists(directory))
					{
						return directory;
					}
				}
			}
		}

		public virtual void startExport(GeContext context, string directory)
		{
			this.context = context;

			try
			{
				// Prepare the export writers
				exportObj = new System.IO.StreamWriter(string.Format("{0}export.obj", directory));
				exportMtl = new System.IO.StreamWriter(string.Format("{0}export.mtl", directory));
			}
			catch (IOException e)
			{
				log.error("Error creating the export files", e);
			}
			exportVertexCount = 1;
			exportModelCount = 1;
			exportTextureCount = 1;
			exportMaterialCount = 1;

			exportObjLine(string.Format("mtllib export.mtl"));
		}

		public virtual void endExport()
		{
			if (exportObj != null)
			{
				try
				{
					exportObj.Close();
				}
				catch (IOException)
				{
					// Ignore error
				}
				exportObj = null;
			}

			if (exportMtl != null)
			{
				try
				{
					exportMtl.Close();
				}
				catch (IOException)
				{
					// Ignore error
				}
				exportMtl = null;
			}
		}

		public virtual void startPrimitive(int numberOfVertex, int primitiveType)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Exporting Object model{0:D}", exportModelCount));
			}

			exportObjLine(string.Format("# modelCount={0:D}, vertexCount={1:D}, textureCount={2:D}, normalCount={3:D}", exportModelCount, exportVertexCount, exportTextureCount, exportNormalCount));
		}

		public virtual void exportVertex(VertexState originalV, VertexState transformedV)
		{
			exportObjLine(string.format(l, "v %f %f %f", transformedV.p[0], transformedV.p[1], transformedV.p[2]));
			if (context.vinfo.texture != 0)
			{
				exportObjLine(string.format(l, "vt %f %f", transformedV.t[0], transformedV.t[1]));
			}
			if (exportNormal && context.vinfo.normal != 0)
			{
				exportObjLine(string.format(l, "vn %f %f %f", transformedV.n[0], transformedV.n[1], transformedV.n[2]));
			}
		}

		public virtual void endVertex(int numberOfVertex, int primitiveType)
		{
			// Export object material
			exportObjLine(string.Format("g model{0:D}", exportModelCount));
			exportObjLine(string.Format("usemtl material{0:D}", exportMaterialCount));

			// Export faces
			exportObjLine("");
			switch (primitiveType)
			{
				case PRIM_TRIANGLE:
				{
					bool clockwise = context.frontFaceCw;

					for (int i = 0; i < numberOfVertex; i += 3)
					{
						if (clockwise)
						{
							exportFace(i + 1, i, i + 2);
						}
						else
						{
							exportFace(i, i + 1, i + 2);
						}
					}
					break;
				}
				case PRIM_TRIANGLE_STRIPS:
				{
					for (int i = 0; i < numberOfVertex - 2; i++)
					{
						// Front face is alternating every 2 triangle strips
						bool clockwise = (i % 2) == 0;

						if (!context.frontFaceCw)
						{
							clockwise = !clockwise;
						}

						if (clockwise)
						{
							exportFace(i + 1, i, i + 2);
						}
						else
						{
							exportFace(i, i + 1, i + 2);
						}
					}
					break;
				}
			}
		}

		public virtual void endPrimitive(int numberOfVertex, int primitiveType)
		{
			exportVertexCount += numberOfVertex;
			if (context.vinfo.texture != 0)
			{
				exportTextureCount += numberOfVertex;
			}
			if (exportNormal && context.vinfo.normal != 0)
			{
				exportNormalCount += numberOfVertex;
			}
			exportModelCount++;
			exportMaterialCount++;
		}

		public virtual void exportTexture(string fileName)
		{
			// Export material definition
			int illum = 1;
			exportMtlLine(string.Format("newmtl material{0:D}", exportMaterialCount));
			exportMtlLine(string.Format("illum {0:D}", illum));

			exportColor(l, "Ka", context.mat_ambient);
			exportColor(l, "Kd", context.mat_diffuse);
			exportColor(l, "Ks", context.mat_specular);

			if (!string.ReferenceEquals(fileName, null))
			{
				exportMtlLine(string.Format("map_Kd {0}", fileName));
			}
		}

		private void exportFace(int i1, int i2, int i3)
		{
			int p1 = i1 + exportVertexCount;
			int p2 = i2 + exportVertexCount;
			int p3 = i3 + exportVertexCount;
			if (exportNormal && context.vinfo.normal != 0)
			{
				int n1 = i1 + exportNormalCount;
				int n2 = i2 + exportNormalCount;
				int n3 = i3 + exportNormalCount;
				if (context.vinfo.texture != 0)
				{
					int t1 = i1 + exportTextureCount;
					int t2 = i2 + exportTextureCount;
					int t3 = i3 + exportTextureCount;
					exportObjLine(string.Format("f {0:D}/{1:D}/{2:D} {3:D}/{4:D}/{5:D} {6:D}/{7:D}/{8:D}", p1, t1, n1, p2, t2, n2, p3, t3, n3));
				}
				else
				{
					exportObjLine(string.Format("f {0:D}//{1:D} {2:D}//{3:D} {4:D}//{5:D}", p1, n1, p2, n2, p3, n3));
				}
			}
			else
			{
				if (context.vinfo.texture != 0)
				{
					int t1 = i1 + exportTextureCount;
					int t2 = i2 + exportTextureCount;
					int t3 = i3 + exportTextureCount;
					exportObjLine(string.Format("f {0:D}/{1:D} {2:D}/{3:D} {4:D}/{5:D}", p1, t1, p2, t2, p3, t3));
				}
				else
				{
					exportObjLine(string.Format("f {0:D} {1:D} {2:D}", p1, p2, p3));
				}
			}
		}

		private void exportColor(Locale l, string name, float[] color)
		{
			exportMtlLine(string.format(l, "%s %f %f %f", name, color[0], color[1], color[2]));
		}
	}

}