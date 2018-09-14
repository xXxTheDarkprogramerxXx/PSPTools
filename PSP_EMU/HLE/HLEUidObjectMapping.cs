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
namespace pspsharp.HLE
{

	public class HLEUidObjectMapping
	{
		public class DoubleHash<TKey, TValue>
		{
			internal Dictionary<TKey, TValue> map;
			internal Dictionary<TValue, TKey> reverseMap;

			public DoubleHash()
			{
				map = new Dictionary<TKey, TValue>();
				reverseMap = new Dictionary<TValue, TKey>();
			}

			public virtual void put(TKey key, TValue value)
			{
				map[key] = value;
				reverseMap[value] = key;
			}

			public virtual TValue getValueByKey(TKey key)
			{
				return map[key];
			}

			public virtual TKey getKeyByValue(TValue value)
			{
				return reverseMap[value];
			}

			public virtual bool containsKey(TKey key)
			{
				return map.ContainsKey(key);
			}

			public virtual bool containsValue(TValue value)
			{
				return reverseMap.ContainsKey(value);
			}

			public virtual void removeKey(TKey key)
			{
				TValue value = map[key];
				map.Remove(key);
				reverseMap.Remove(value);
			}

			public virtual void removeValue(TValue value)
			{
				TKey key = reverseMap[value];
				map.Remove(key);
				reverseMap.Remove(value);
			}

			public virtual int size()
			{
				return map.Count;
			}
		}

		private static Dictionary<string, DoubleHash<int, object>> map = new Dictionary<string, DoubleHash<int, object>>();

		public static void reset()
		{
			map.Clear();
		}

		protected internal static DoubleHash<int, object> getMapForClass(string className)
		{
			if (!map.ContainsKey(className))
			{
				map[className] = new DoubleHash<int, object>();
			}
			return map[className];
		}

		public static int addObjectMap(string className, int uid, object @object)
		{
			getMapForClass(className).put(uid, @object);
			return uid;
		}

		public static int addObjectMap(int uid, object @object)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			return addObjectMap(@object.GetType().FullName, uid, @object);
		}

		public static int createUidForObject(string className, object @object)
		{
			return addObjectMap(className, getMapForClass(className).size(), @object);
		}

		public static int createUidForObject(object @object)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			return createUidForObject(@object.GetType().FullName, @object);
		}

		public static object getObject(string className, int uid)
		{
			return getMapForClass(className).getValueByKey(uid);
		}

		public static void removeObject(string className, object @object)
		{
			getMapForClass(className).removeValue(@object);
		}

		public static void removeObject(object @object)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			removeObject(@object.GetType().FullName, @object);
		}
	}

}