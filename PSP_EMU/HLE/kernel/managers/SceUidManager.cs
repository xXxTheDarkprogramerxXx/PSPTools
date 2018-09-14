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
namespace pspsharp.HLE.kernel.managers
{

	using SceUid = pspsharp.HLE.kernel.types.SceUid;

	/// 
	/// <summary>
	/// @author hli, gid15
	/// </summary>
	public class SceUidManager
	{
		// UID is a unique identifier across all purposes
		private static Dictionary<int, SceUid> uidMap = new Dictionary<int, SceUid>();
		private static int uidNext = 0x1; // LocoRoco expects UID to be 8bit
		public static readonly int INVALID_ID = int.MinValue;

		// ID is an identifier only unique for the same purpose.
		// Different purposes can share the save ID values.
		// An ID has always a range of valid values, e.g. [0..255]
		private static Dictionary<object, LinkedList<int>> freeIdsMap = new Dictionary<object, LinkedList<int>>();

		public static void reset()
		{
			uidMap.Clear();
			freeIdsMap.Clear();
			uidNext = 1;
		}

		/// <summary>
		/// classes should call getUid to get a new unique SceUID </summary>
		public static int getNewUid(object purpose)
		{
			SceUid uid = new SceUid(purpose, uidNext++);
			uidMap[uid.Uid] = uid;
			return uid.Uid;
		}

		/// <summary>
		/// classes should call checkUidPurpose before using a SceUID </summary>
		/// <returns> true is the uid is ok.  </returns>
		public static bool checkUidPurpose(int uid, object purpose, bool allowUnknown)
		{
			SceUid found = uidMap[uid];

			if (found == null)
			{
				if (!allowUnknown)
				{
					Emulator.log.warn("Attempt to use unknown SceUID (purpose='" + purpose.ToString() + "')");
					return false;
				}
			}
			else if (!purpose.Equals(found.Purpose))
			{
				Emulator.log.error("Attempt to use SceUID for different purpose (purpose='" + purpose.ToString() + "',original='" + found.Purpose.ToString() + "')");
				return false;
			}

			return true;
		}

		/// <summary>
		/// classes should call releaseUid when they are finished with a SceUID </summary>
		/// <returns> true on success.  </returns>
		public static bool releaseUid(int uid, object purpose)
		{
			SceUid found = uidMap[uid];

			if (found == null)
			{
				Emulator.log.warn("Attempt to release unknown SceUID (purpose='" + purpose.ToString() + "')");
				return false;
			}

			if (purpose.Equals(found.Purpose))
			{
				uidMap.Remove(uid);
			}
			else
			{
				Emulator.log.error("Attempt to release SceUID for different purpose (purpose='" + purpose.ToString() + "',original='" + found.Purpose.ToString() + "')");
				return false;
			}

			return true;
		}

		public static bool isValidUid(int uid)
		{
			return uidMap.ContainsKey(uid);
		}

		/// <summary>
		/// Return a new ID for the given purpose.
		/// The ID will be unique for the given purpose but will not be unique
		/// across different purposes.
		/// The ID will be higher of equal to minimumId, and lower or equal to
		/// maximumId, i.e. in the range [minimumId..maximumId].
		/// The ID will be lowest possible free ID.
		/// </summary>
		/// <param name="purpose">    The ID will be unique for this purpose </param>
		/// <param name="minimumId">  The lowest possible value for the ID </param>
		/// <param name="maximumId">  The highest possible value for the ID </param>
		/// <returns>           The lowest possible free ID for the given purpose </returns>
		public static int getNewId(object purpose, int minimumId, int maximumId)
		{
			LinkedList<int> freeIds = freeIdsMap[purpose];
			if (freeIds == null)
			{
				freeIds = new LinkedList<int>();
				for (int id = minimumId; id <= maximumId; id++)
				{
					freeIds.AddLast(id);
				}
				freeIdsMap[purpose] = freeIds;
			}

			// No more free IDs?
			if (freeIds.Count <= 0)
			{
				// Return an invalid ID
				return INVALID_ID;
			}

			// Return the lowest free ID
			return freeIds.RemoveFirst();
		}

		public static void resetIds(object purpose)
		{
			freeIdsMap.Remove(purpose);
		}

		/// <summary>
		/// Release an ID for a given purpose. The ID had to be created first
		/// by getNewId().
		/// After release, the ID is marked as being free and can be returned
		/// again by getNewId().
		/// </summary>
		/// <param name="id">       The ID to be released </param>
		/// <param name="purpose">  The ID will be releases for this purpose. </param>
		/// <returns>         true if the ID was successfully released
		///                 false if the ID could not be released
		///                       (because the purpose was not exiting or
		///                        the ID was already released) </returns>
		public static bool releaseId(int id, object purpose)
		{
			LinkedList<int> freeIds = freeIdsMap[purpose];

			if (freeIds == null)
			{
				Emulator.log.warn(string.Format("Attempt to release ID={0:D} with unknown purpose='{1}'", id, purpose));
				return false;
			}

			// Add the id back to the freeIds list,
			// and keep the id's ordered (lowest first).
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<int> lit = freeIds.GetEnumerator(); lit.MoveNext();)
			{
				int currentId = lit.Current;
				if (currentId == id)
				{
					Emulator.log.warn(string.Format("Attempt to release free ID={0:D} with purpose='{1}'", id, purpose));
					return false;
				}
				if (currentId > id)
				{
					// Insert the id before the currentId
					lit.set(id);
					lit.add(currentId);
					return true;
				}
			}

			freeIds.AddLast(id);

			return true;
		}
	}

}