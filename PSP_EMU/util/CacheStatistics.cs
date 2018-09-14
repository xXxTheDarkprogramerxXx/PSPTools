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
namespace pspsharp.util
{
	public class CacheStatistics
	{
		private string name;
		private int cacheMaxSize;
		public long totalHits = 0; // Number of times a vertex was searched
		public long successfulHits = 0; // Number of times a vertex was successfully found
		public long notPresentHits = 0; // Number of times a vertex was not present
		public long changedHits = 0; // Number of times a vertex was present but had to be discarded because it was changed
		public long entriesRemoved = 0; // Number of times a vertex had to be removed from the cache due to the size limit
		public long maxSizeUsed = 0; // Maximum size of the cache

		public CacheStatistics(string name, int cacheMaxSize)
		{
			this.name = name;
			this.cacheMaxSize = cacheMaxSize;
		}

		private string percentage(long n, long max)
		{
			return string.Format("{0:F2}%", (n / (double) max) * 100);
		}

		private string percentage(long hits)
		{
			return percentage(hits, totalHits);
		}

		public virtual void reset()
		{
			totalHits = 0;
			successfulHits = 0;
			notPresentHits = 0;
			changedHits = 0;
			entriesRemoved = 0;
			maxSizeUsed = 0;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			if (!string.ReferenceEquals(name, null))
			{
				result.Append(name);
				result.Append(" ");
			}
			result.Append("Cache Statistics: ");
			if (totalHits == 0)
			{
				result.Append("Cache deactivated");
			}
			else
			{
				result.Append("TotalHits=" + totalHits + ", ");
				result.Append("SuccessfulHits=" + successfulHits + " (" + percentage(successfulHits) + "), ");
				result.Append("NotPresentHits=" + notPresentHits + " (" + percentage(notPresentHits) + "), ");
				result.Append("ChangedHits=" + changedHits + " (" + percentage(changedHits) + "), ");
				result.Append("EntriesRemoved=" + entriesRemoved + ", ");
				result.Append("MaxSizeUsed=" + maxSizeUsed + " (" + percentage(maxSizeUsed, cacheMaxSize) + ")");
			}
			return result.ToString();
		}

	}

}