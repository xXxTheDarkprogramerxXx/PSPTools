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
namespace pspsharp.state
{

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using HLEModuleManager = pspsharp.HLE.HLEModuleManager;

	public class State : IState
	{
		//public static Logger log = Logger.getLogger("state");
		private const int STATE_VERSION = 0;

		public State()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(String fileName) throws java.io.IOException
		public virtual void read(string fileName)
		{
			System.IO.FileStream fileInputStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			GZIPInputStream gzipInputStream = new GZIPInputStream(fileInputStream);
			BufferedInputStream bufferedInputStream = new BufferedInputStream(gzipInputStream);
			StateInputStream stream = new StateInputStream(bufferedInputStream);

			if (log.InfoEnabled)
			{
				log.info(string.Format("Reading state from file '{0}'", fileName));
			}

			try
			{
				read(stream);
				if (stream.read() >= 0)
				{
					Console.WriteLine(string.Format("State file '{0}' containing too much data", fileName));
				}
			}
			finally
			{
				stream.close();
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Done reading state from file '{0}'", fileName));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(String fileName) throws java.io.IOException
		public virtual void write(string fileName)
		{
			System.IO.FileStream fileOutputStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			GZIPOutputStream gzipOutputStream = new GZIPOutputStream(fileOutputStream);
			BufferedOutputStream bufferedOutputStream = new BufferedOutputStream(gzipOutputStream);
			StateOutputStream stream = new StateOutputStream(bufferedOutputStream);

			if (log.InfoEnabled)
			{
				log.info(string.Format("Writing state to file '{0}'", fileName));
			}

			try
			{
				write(stream);
			}
			finally
			{
				stream.close();
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Done writing state to file '{0}'", fileName));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			Emulator.Clock.read(stream);
			Emulator.Processor.read(stream);
			Emulator.Memory.read(stream);
			HLEModuleManager.Instance.read(stream);
			if (RuntimeContextLLE.LLEActive)
			{
				RuntimeContextLLE.read(stream);
				RuntimeContextLLE.createMMIO();
				RuntimeContextLLE.MMIO.read(stream);
				RuntimeContextLLE.MediaEngineProcessor.read(stream);
				RuntimeContextLLE.MediaEngineProcessor.MEMemory.read(stream);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			Emulator.Clock.write(stream);
			Emulator.Processor.write(stream);
			Emulator.Memory.write(stream);
			HLEModuleManager.Instance.write(stream);
			if (RuntimeContextLLE.LLEActive)
			{
				RuntimeContextLLE.write(stream);
				RuntimeContextLLE.createMMIO();
				RuntimeContextLLE.MMIO.write(stream);
				RuntimeContextLLE.MediaEngineProcessor.write(stream);
				RuntimeContextLLE.MediaEngineProcessor.MEMemory.write(stream);
			}
		}
	}

}