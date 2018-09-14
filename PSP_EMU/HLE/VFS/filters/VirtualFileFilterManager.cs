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
namespace pspsharp.HLE.VFS.filters
{

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class VirtualFileFilterManager
	{
		private static VirtualFileFilterManager instance;

		public static VirtualFileFilterManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new VirtualFileFilterManager();
				}
    
				return instance;
			}
		}

		private VirtualFileFilterManager()
		{
		}

		public virtual IVirtualFileFilter Filter
		{
			get
			{
				// This should be configurable through the game specific patch files.
				if ("NPJH50676".Equals(State.discId))
				{
					return new XorVirtualFileFilter((sbyte) 0x7B);
				}
    
				return null;
			}
		}

		public virtual IVirtualFile getFilteredVirtualFile(IVirtualFile vFile)
		{
			IVirtualFileFilter filter = Filter;
			if (filter == null)
			{
				return vFile;
			}

			filter.VirtualFile = vFile;
			return filter;
		}

		public virtual void filter(sbyte[] data, int offset, int length)
		{
			IVirtualFileFilter filter = Filter;
			if (filter != null)
			{
				filter.filter(data, offset, length);
			}
		}
	}

}