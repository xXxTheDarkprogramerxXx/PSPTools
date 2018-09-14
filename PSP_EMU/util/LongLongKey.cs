using System;

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
namespace pspsharp.util
{
	/// <summary>
	/// @author gid15
	/// 
	/// A 128-bit key value that can be used as a hash key.
	/// </summary>
	public class LongLongKey
	{
		private long key1;
		private long key2;
		private int shift;

		public LongLongKey()
		{
		}

		public LongLongKey(LongLongKey key)
		{
			key1 = key.key1;
			key2 = key.key2;
			shift = key.shift;
		}

		public virtual void reset()
		{
			key1 = 0;
			key2 = 0;
			shift = 0;
		}

		/// <summary>
		/// Add an integer value to the current key.
		/// </summary>
		/// <param name="value">  the integer value </param>
		/// <param name="bits">   the number of bits of the integer value to be considered. </param>
		public virtual void addKeyComponent(int value, int bits)
		{
			if (shift < (sizeof(long) * 8))
			{
				if (shift + bits > (sizeof(long) * 8))
				{
					shift = (sizeof(long) * 8);
					key2 = value;
				}
				else
				{
					key1 += ((long) value) << shift;
				}
			}
			else
			{
				key2 += ((long) value) << (shift - (sizeof(long) * 8));
			}
			shift += bits;
		}

		/// <summary>
		/// Add a boolean value to the current key.
		/// </summary>
		/// <param name="value">  the boolean value </param>
		public virtual void addKeyComponent(bool value)
		{
			if (shift < (sizeof(long) * 8))
			{
				key1 += (value ? 1L : 0L) << shift;
			}
			else
			{
				key2 += (value ? 1L : 0L) << (shift - (sizeof(long) * 8));
			}
			shift++;
		}

		/* (non-Javadoc)
		 * @see java.lang.Object#equals(java.lang.Object)
		 * 
		 * Required by the Hashtable implementation.
		 */
		public override bool Equals(object obj)
		{
			if (obj is LongLongKey)
			{
				LongLongKey longLongKey = (LongLongKey) obj;
				return key1 == longLongKey.key1 && key2 == longLongKey.key2;
			}
			return base.Equals(obj);
		}

		/* (non-Javadoc)
		 * @see java.lang.Object#hashCode()
		 *
		 * Required by the Hashtable implementation.
		 */
		public override int GetHashCode()
		{
			// Mix both key1 and key2
			return Convert.ToInt64(key1).GetHashCode() ^ Convert.ToInt64(key2).GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("LongLongKey(key1=0x{0:X}, key2=0x{1:X})", key1, key2);
		}
	}

}