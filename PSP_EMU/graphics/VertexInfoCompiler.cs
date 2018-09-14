using System;
using System.Collections.Generic;

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
namespace pspsharp.graphics
{

	using ClassSpecializer = pspsharp.util.ClassSpecializer;

	using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class VertexInfoCompiler
	{
		private static Logger log = VideoEngine.log_Renamed;
		private static VertexInfoCompiler instance;
		private Dictionary<int, VertexInfoReaderTemplate> compiledVertexInfoReaders = new Dictionary<int, VertexInfoReaderTemplate>();
		private VertexInfo vinfo = new VertexInfo();

		public static VertexInfoCompiler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new VertexInfoCompiler();
				}
    
				return instance;
			}
		}

		private VertexInfoCompiler()
		{
		}

		public virtual VertexInfoReaderTemplate getCompiledVertexInfoReader(int vtype, bool readTexture)
		{
			int key = vtype;
			if (readTexture)
			{
				key |= 0x01000000;
			}
			if (log.TraceEnabled)
			{
				key |= 0x02000000;
			}
			VertexInfoReaderTemplate compiledVertexInfoReader = compiledVertexInfoReaders[key];
			if (compiledVertexInfoReader == null)
			{
				// Synchronize this block as it can be called by different threads in parallel
				// (GUI and AsyncVertexCache threads)
				lock (compiledVertexInfoReaders)
				{
					compiledVertexInfoReader = compiledVertexInfoReaders[key];
					if (compiledVertexInfoReader == null)
					{
						compiledVertexInfoReader = compileVertexInfoReader(key, vtype, readTexture);
						if (compiledVertexInfoReader != null)
						{
							compiledVertexInfoReaders[key] = compiledVertexInfoReader;
						}
					}
				}
			}

			return compiledVertexInfoReader;
		}

		private VertexInfoReaderTemplate compileVertexInfoReader(int key, int vtype, bool readTexture)
		{
			VertexInfo.processType(vinfo, vtype);

			if (log.InfoEnabled)
			{
				log.info(string.Format("Compiling VertexInfoReader for {0}", vinfo));
			}

			Dictionary<string, object> variables = new Dictionary<string, object>();
			// All these variables have to be defined as static members in the class VertexInfoReaderTemplate.
			variables["isLogTraceEnabled"] = Convert.ToBoolean(log.TraceEnabled);
			variables["transform2D"] = Convert.ToBoolean(vinfo.transform2D);
			variables["skinningWeightCount"] = Convert.ToInt32(vinfo.skinningWeightCount);
			variables["morphingVertexCount"] = Convert.ToInt32(vinfo.morphingVertexCount);
			variables["texture"] = Convert.ToInt32(vinfo.texture);
			variables["color"] = Convert.ToInt32(vinfo.color);
			variables["normal"] = Convert.ToInt32(vinfo.normal);
			variables["position"] = Convert.ToInt32(vinfo.position);
			variables["weight"] = Convert.ToInt32(vinfo.weight);
			variables["index"] = Convert.ToInt32(vinfo.index);
			variables["vtype"] = Convert.ToInt32(vinfo.vtype);
			variables["readTexture"] = Convert.ToBoolean(readTexture);
			variables["vertexSize"] = Convert.ToInt32(vinfo.vertexSize);
			variables["oneVertexSize"] = Convert.ToInt32(vinfo.oneVertexSize);
			variables["textureOffset"] = Convert.ToInt32(vinfo.textureOffset);
			variables["colorOffset"] = Convert.ToInt32(vinfo.colorOffset);
			variables["normalOffset"] = Convert.ToInt32(vinfo.normalOffset);
			variables["positionOffset"] = Convert.ToInt32(vinfo.positionOffset);
			variables["alignmentSize"] = Convert.ToInt32(vinfo.alignmentSize);

			string specializedClassName = string.Format("VertexInfoReader{0:X7}", key);
			ClassSpecializer cs = new ClassSpecializer();
			Type specializedClass = cs.specialize(specializedClassName, typeof(VertexInfoReaderTemplate), variables);
			VertexInfoReaderTemplate compiledVertexInfoReader = null;
			if (specializedClass != null)
			{
				try
				{
					compiledVertexInfoReader = (VertexInfoReaderTemplate) System.Activator.CreateInstance(specializedClass);
				}
				catch (InstantiationException e)
				{
					log.error("Error while instanciating compiled vertexInfoReader", e);
				}
				catch (IllegalAccessException e)
				{
					log.error("Error while instanciating compiled vertexInfoReader", e);
				}
			}

			return compiledVertexInfoReader;
		}
	}

}