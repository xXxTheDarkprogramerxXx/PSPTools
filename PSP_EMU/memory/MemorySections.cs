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
namespace pspsharp.memory
{

	public class MemorySections
	{
		private static MemorySections instance;
		private IList<MemorySection> allMemorySections;
		private IList<MemorySection> readMemorySections;
		private IList<MemorySection> writeMemorySections;
		private IList<MemorySection> executeMemorySections;

		public static MemorySections Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MemorySections();
				}
				return instance;
			}
		}

		private MemorySections()
		{
			allMemorySections = new LinkedList<MemorySection>();
			readMemorySections = new LinkedList<MemorySection>();
			writeMemorySections = new LinkedList<MemorySection>();
			executeMemorySections = new LinkedList<MemorySection>();
		}

		public virtual void reset()
		{
			allMemorySections.Clear();
			readMemorySections.Clear();
			writeMemorySections.Clear();
			executeMemorySections.Clear();
		}

		public virtual void addMemorySection(MemorySection memorySection)
		{
			allMemorySections.Add(memorySection);
			if (memorySection.canRead())
			{
				readMemorySections.Add(memorySection);
			}
			if (memorySection.canWrite())
			{
				writeMemorySections.Add(memorySection);
			}
			if (memorySection.canExecute())
			{
				executeMemorySections.Add(memorySection);
			}
		}

		public virtual MemorySection getMemorySection(int address)
		{
			foreach (MemorySection memorySection in allMemorySections)
			{
				if (memorySection.contains(address))
				{
					return memorySection;
				}
			}

			return null;
		}

		private bool contains(IList<MemorySection> memorySections, int address, bool defaultValue)
		{
			foreach (MemorySection memorySection in memorySections)
			{
				if (memorySection.contains(address))
				{
					return true;
				}
			}

			return defaultValue;
		}

		public virtual bool canRead(int address, bool defaultValue)
		{
			return contains(readMemorySections, address, defaultValue);
		}

		public virtual bool canWrite(int address, bool defaultValue)
		{
			return contains(writeMemorySections, address, defaultValue);
		}

		public virtual bool canExecute(int address, bool defaultValue)
		{
			return contains(executeMemorySections, address, defaultValue);
		}
	}

}