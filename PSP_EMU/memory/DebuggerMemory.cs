using System;
using System.Collections.Generic;
using System.Text;

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
namespace pspsharp.memory
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContextLLE.getProcessor;


	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using MemoryBreakpoint = pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint;
	using Utilities = pspsharp.util.Utilities;

	public class DebuggerMemory : Memory
	{

		public bool traceMemoryRead = false;
		public bool traceMemoryWrite = false;
		public bool traceMemoryRead8 = false;
		public bool traceMemoryWrite8 = false;
		public bool traceMemoryRead16 = false;
		public bool traceMemoryWrite16 = false;
		public bool traceMemoryRead32 = false;
		public bool traceMemoryWrite32 = false;
		public bool pauseEmulatorOnMemoryBreakpoint = false;
		private HashSet<int> memoryReadBreakpoint;
		private HashSet<int> memoryWriteBreakpoint;
		private IList<MemoryBreakpoint> memoryBreakpoints;
		private Memory mem;
		// external breakpoint list
		public static string mBrkFilePath = "Memory.mbrk";

		public DebuggerMemory(Memory mem)
		{
			this.mem = mem;

			memoryReadBreakpoint = new HashSet<int>();
			memoryWriteBreakpoint = new HashSet<int>();
			memoryBreakpoints = new LinkedList<MemoryBreakpoint>();

			// backwards compatibility
			if (System.IO.Directory.Exists(mBrkFilePath) || System.IO.File.Exists(mBrkFilePath))
			{
				importBreakpoints(mBrkFilePath);
			}
		}

		public virtual IList<MemoryBreakpoint> MemoryBreakpoints
		{
			get
			{
				return memoryBreakpoints;
			}
		}

		private string BreakpointToken
		{
			set
			{
				if (value.Length == 0)
				{
					return;
				}
				if (value.Equals("read"))
				{
					traceMemoryRead = true;
				}
				else if (value.Equals("read8"))
				{
					traceMemoryRead8 = true;
				}
				else if (value.Equals("read16"))
				{
					traceMemoryRead16 = true;
				}
				else if (value.Equals("read32"))
				{
					traceMemoryRead32 = true;
				}
				else if (value.Equals("write"))
				{
					traceMemoryWrite = true;
				}
				else if (value.Equals("write8"))
				{
					traceMemoryWrite8 = true;
				}
				else if (value.Equals("write16"))
				{
					traceMemoryWrite16 = true;
				}
				else if (value.Equals("write32"))
				{
					traceMemoryWrite32 = true;
				}
				else if (value.Equals("pause"))
				{
					pauseEmulatorOnMemoryBreakpoint = true;
				}
				else
				{
					Console.WriteLine(string.Format("Unknown token '{0}'", value));
				}
			}
		}

		public void exportBreakpoints(string filename)
		{
			exportBreakpoints(new File(filename));
		}

		public void exportBreakpoints(File f)
		{
			System.IO.StreamWriter @out = null;
			try
			{
				@out = new System.IO.StreamWriter(f);

				IEnumerator<MemoryBreakpoint> it = memoryBreakpoints.GetEnumerator();
				while (it.MoveNext())
				{
					MemoryBreakpoint mbp = it.Current;

					string line = "";
					switch (mbp.Access)
					{
						case READ:
							line += "R ";
							break;
						case WRITE:
							line += "W ";
							break;
						case READWRITE:
							line += "RW ";
							break;
						default:
							// ignore - but should never happen
							continue;
					}

					line += string.Format("0x{0:X8}", mbp.StartAddress);
					if (mbp.StartAddress != mbp.EndAddress)
					{
						line += string.Format(" - 0x{0:X8}", mbp.EndAddress);
					}
					line += System.getProperty("line.separator");

					@out.Write(line);
				}

				if (traceMemoryRead)
				{
					@out.Write("read|");
				}
				if (traceMemoryWrite)
				{
					@out.Write("write|");
				}
				if (traceMemoryRead8)
				{
					@out.Write("read8|");
				}
				if (traceMemoryWrite8)
				{
					@out.Write("write8|");
				}
				if (traceMemoryRead16)
				{
					@out.Write("read16|");
				}
				if (traceMemoryWrite16)
				{
					@out.Write("write16|");
				}
				if (traceMemoryRead32)
				{
					@out.Write("read32|");
				}
				if (traceMemoryWrite32)
				{
					@out.Write("write32|");
				}
				if (pauseEmulatorOnMemoryBreakpoint)
				{
					@out.Write("pause");
				}

				@out.BaseStream.WriteByte(System.getProperty("line.separator"));

			}
			catch (IOException)
			{
				// ignore
			}
			finally
			{
				Utilities.close(@out);
			}
		}

		public void importBreakpoints(string filename)
		{
			importBreakpoints(new File(filename));
		}

		public void importBreakpoints(File f)
		{
			System.IO.StreamReader @in = null;
			try
			{
				@in = new System.IO.StreamReader(f);

				// reset current configuration
				traceMemoryRead = false;
				traceMemoryWrite = false;
				traceMemoryRead8 = false;
				traceMemoryWrite8 = false;
				traceMemoryRead16 = false;
				traceMemoryWrite16 = false;
				traceMemoryRead32 = false;
				traceMemoryWrite32 = false;
				pauseEmulatorOnMemoryBreakpoint = false;

				while (true)
				{
					string line = @in.ReadLine();
					if (string.ReferenceEquals(line, null))
					{
						break;
					}
					line = line.Trim();
					int rangeIndex = line.IndexOf("-", StringComparison.Ordinal);
					if (rangeIndex >= 0)
					{
						// Range parsing
						if (line.StartsWith("RW ", StringComparison.Ordinal))
						{
							int start = Utilities.parseAddress(line.Substring(2, rangeIndex - 2));
							int end = Utilities.parseAddress(line.Substring(rangeIndex + 1));
							memoryBreakpoints.Add(new MemoryBreakpoint(this, start, end, MemoryBreakpoint.AccessType.READWRITE));
						}
						else if (line.StartsWith("R ", StringComparison.Ordinal))
						{
							int start = Utilities.parseAddress(line.Substring(1, rangeIndex - 1));
							int end = Utilities.parseAddress(line.Substring(rangeIndex + 1));
							memoryBreakpoints.Add(new MemoryBreakpoint(this, start, end, MemoryBreakpoint.AccessType.READ));
						}
						else if (line.StartsWith("W ", StringComparison.Ordinal))
						{
							int start = Utilities.parseAddress(line.Substring(1, rangeIndex - 1));
							int end = Utilities.parseAddress(line.Substring(rangeIndex + 1));
							memoryBreakpoints.Add(new MemoryBreakpoint(this, start, end, MemoryBreakpoint.AccessType.WRITE));
						}
					}
					else if (line.StartsWith("RW ", StringComparison.Ordinal))
					{
						int address = Utilities.parseAddress(line.Substring(2));
						memoryBreakpoints.Add(new MemoryBreakpoint(this, address, MemoryBreakpoint.AccessType.READWRITE));
					}
					else if (line.StartsWith("R ", StringComparison.Ordinal))
					{
						int address = Utilities.parseAddress(line.Substring(1));
						memoryBreakpoints.Add(new MemoryBreakpoint(this, address, MemoryBreakpoint.AccessType.READ));
					}
					else if (line.StartsWith("W ", StringComparison.Ordinal))
					{
						int address = Utilities.parseAddress(line.Substring(1));
						memoryBreakpoints.Add(new MemoryBreakpoint(this, address, MemoryBreakpoint.AccessType.WRITE));
					}
					else if (!line.StartsWith("#", StringComparison.Ordinal))
					{
						string[] tokens = line.Split("\\|", true);
						for (int i = 0; tokens != null && i < tokens.Length; i++)
						{
							string token = tokens[i].Trim().ToLower();
							BreakpointToken = token;
						}
					}
				}
			}
			catch (IOException)
			{
				// ignore
			}
			finally
			{
				Utilities.close(@in);
			}

			log.info(string.Format("{0:D} memory breakpoint(s) imported", memoryBreakpoints.Count));
		}

		public static bool Installed
		{
			get
			{
				return Memory.Instance is DebuggerMemory;
			}
		}

		public static void install()
		{
			if (!Installed)
			{
				log.info("Using DebuggerMemory");
				DebuggerMemory debuggerMemory = new DebuggerMemory(Memory.Instance);
				Memory.Instance = debuggerMemory;
				RuntimeContext.updateMemory();
			}
		}

		public static void deinstall()
		{
			if (Installed)
			{
				DebuggerMemory debuggerMemory = (DebuggerMemory) Memory.Instance;
				Memory.Instance = debuggerMemory.mem;
			}
		}

		public virtual void addReadBreakpoint(int address)
		{
			address &= Memory.addressMask;
			memoryReadBreakpoint.Add(address);
		}

		public virtual void removeReadBreakpoint(int address)
		{
			address &= Memory.addressMask;
			memoryReadBreakpoint.remove(address);
		}

		public virtual void addRangeReadBreakpoint(int start, int end)
		{
			for (int address = start; address <= end; address++)
			{
				addReadBreakpoint(address);
			}
		}

		public virtual void removeRangeReadBreakpoint(int start, int end)
		{
			for (int address = start; address <= end; address++)
			{
				removeReadBreakpoint(address);
			}
		}

		public virtual void addWriteBreakpoint(int address)
		{
			address &= Memory.addressMask;
			memoryWriteBreakpoint.Add(address);
		}

		public virtual void removeWriteBreakpoint(int address)
		{
			address &= Memory.addressMask;
			memoryWriteBreakpoint.remove(address);
		}

		public virtual void addRangeWriteBreakpoint(int start, int end)
		{
			for (int address = start; address <= end; address++)
			{
				addWriteBreakpoint(address);
			}
		}

		public virtual void removeRangeWriteBreakpoint(int start, int end)
		{
			for (int address = start; address <= end; address++)
			{
				removeWriteBreakpoint(address);
			}
		}

		public virtual void addReadWriteBreakpoint(int address)
		{
			address &= Memory.addressMask;
			memoryReadBreakpoint.Add(address);
			memoryWriteBreakpoint.Add(address);
		}

		public virtual void removeReadWriteBreakpoint(int address)
		{
			address &= Memory.addressMask;
			memoryReadBreakpoint.remove(address);
			memoryWriteBreakpoint.remove(address);
		}

		public virtual void addRangeReadWriteBreakpoint(int start, int end)
		{
			for (int address = start; address <= end; address++)
			{
				addReadWriteBreakpoint(address);
			}
		}

		public virtual void removeRangeReadWriteBreakpoint(int start, int end)
		{
			for (int address = start; address <= end; address++)
			{
				removeReadWriteBreakpoint(address);
			}
		}

		protected internal virtual string getMemoryReadMessage(int address, int width)
		{
			StringBuilder message = new StringBuilder();

			Processor processor = Processor;
			if (processor != null)
			{
				message.Append(string.Format("0x{0:X8} - ", processor.cpu.pc));
			}

			if (width == 8 || width == 16 || width == 32)
			{
				message.Append(string.Format("read{0:D}(0x{1:X8})=0x", width, address));
				if (width == 8)
				{
					message.Append(string.Format("{0:X2}", mem.read8(address)));
				}
				else if (width == 16)
				{
					message.Append(string.Format("{0:X4}", mem.read16(address)));
				}
				else if (width == 32)
				{
					int value = mem.read32(address);
					//message.append(String.format("%08X (%f)", value, Float.intBitsToFloat(value)));
					message.Append(string.Format("{0:X8}", value));
				}
			}
			else
			{
				int Length = width / 8;
				message.Append(string.Format("read 0x{0:X8}-0x{1:X8} (Length={2:D})", address, address + Length, Length));
			}

			return message.ToString();
		}

		protected internal virtual void memoryRead(int address, int width, bool trace)
		{
			address &= Memory.addressMask;
			if ((traceMemoryRead || trace) && log.TraceEnabled)
			{
				log.trace(getMemoryReadMessage(address, width));
			}

			if ((pauseEmulatorOnMemoryBreakpoint || log.InfoEnabled) && memoryReadBreakpoint.Contains(address))
			{
				log.info(getMemoryReadMessage(address, width));
				if (pauseEmulatorOnMemoryBreakpoint)
				{
					Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_BREAKPOINT);
				}
			}
		}

		protected internal virtual string getMemoryWriteMessage(int address, int value, int width)
		{
			StringBuilder message = new StringBuilder();

			Processor processor = Processor;
			if (processor != null)
			{
				message.Append(string.Format("0x{0:X8} - ", processor.cpu.pc));
			}

			message.Append(string.Format("write{0:D}(0x{1:X8}, 0x", width, address));
			if (width == 8)
			{
				message.Append(string.Format("{0:X2}", value & 0xFF));
			}
			else if (width == 16)
			{
				message.Append(string.Format("{0:X4}", value & 0xFFFF));
			}
			else if (width == 32)
			{
				//message.append(String.format("%08X (%f)", value, Float.intBitsToFloat(value)));
				message.Append(string.Format("{0:X8}", value));
			}
			message.Append(")");

			return message.ToString();
		}

		protected internal virtual void memoryWrite(int address, int value, int width, bool trace)
		{
			address &= Memory.addressMask;
			if ((traceMemoryWrite || trace) && log.TraceEnabled)
			{
				log.trace(getMemoryWriteMessage(address, value, width));
			}

			if ((pauseEmulatorOnMemoryBreakpoint || log.InfoEnabled) && memoryWriteBreakpoint.Contains(address))
			{
				log.info(getMemoryWriteMessage(address, value, width));
				if (pauseEmulatorOnMemoryBreakpoint)
				{
					Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_BREAKPOINT);
				}
			}
		}

		public override void Initialise()
		{
			mem.Initialise();
		}

		public override bool allocate()
		{
			return mem.allocate();
		}

		public override void copyToMemory(int address, ByteBuffer source, int Length)
		{
			// Perform copyToMemory using write8 to check memory access
			for (int i = 0; i < Length && source.hasRemaining(); i++)
			{
				sbyte value = source.get();
				write8(address + i, value);
			}
		}

		public override Buffer getBuffer(int address, int Length)
		{
			memoryRead(address, Length * 8, false);
			return mem.getBuffer(address, Length);
		}

		public override Buffer MainMemoryByteBuffer
		{
			get
			{
				return mem.MainMemoryByteBuffer;
			}
		}

		protected internal override void memcpy(int destination, int source, int Length, bool checkOverlap)
		{
			destination = normalizeAddress(destination);
			source = normalizeAddress(source);

			// Overlapping address ranges must be correctly handled:
			//   If source >= destination:
			//                 [---source---]
			//       [---destination---]
			//      => Copy from the head
			//   If source < destination:
			//       [---source---]
			//                 [---destination---]
			//      => Copy from the tail
			//
			if (!checkOverlap || source >= destination || !areOverlapping(destination, source, Length))
			{
				// Perform memcpy using read8/write8 to check memory access
				for (int i = 0; i < Length; i++)
				{
					write8(destination + i, (sbyte) read8(source + i));
				}
			}
			else
			{
				// Perform memcpy using read8/write8 to check memory access
				for (int i = Length - 1; i >= 0; i--)
				{
					write8(destination + i, (sbyte) read8(source + i));
				}
			}
		}

		public override void memset(int address, sbyte data, int Length)
		{
			// Perform memset using write8 to check memory access
			for (int i = 0; i < Length; i++)
			{
				write8(address + i, data);
			}
		}

		public override int read8(int address)
		{
			memoryRead(address, 8, traceMemoryRead8);
			return mem.read8(address);
		}

		public override int read16(int address)
		{
			memoryRead(address, 16, traceMemoryRead16);
			return mem.read16(address);
		}

		public override int read32(int address)
		{
			memoryRead(address, 32, traceMemoryRead32);
			return mem.read32(address);
		}

		public override void write8(int address, sbyte data)
		{
			memoryWrite(address, data, 8, traceMemoryWrite8);
			mem.write8(address, data);
		}

		public override void write16(int address, short data)
		{
			memoryWrite(address, data, 16, traceMemoryWrite16);
			mem.write16(address, data);
		}

		public override void write32(int address, int data)
		{
			memoryWrite(address, data, 32, traceMemoryWrite32);
			mem.write32(address, data);
		}

		public override bool IgnoreInvalidMemoryAccess
		{
			set
			{
				base.IgnoreInvalidMemoryAccess = value;
				if (mem != null)
				{
					mem.IgnoreInvalidMemoryAccess = value;
				}
			}
		}
	}

}