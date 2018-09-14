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
namespace pspsharp.util
{

	public class ThreadLocalCookieManager : CookieManager
	{
		private static readonly ThreadLocal<CookieManager> @delegate = new ThreadLocal<CookieManager>();

		public override CookiePolicy CookiePolicy
		{
			set
			{
				CookieManager cookieManager = @delegate.get();
				if (cookieManager != null)
				{
					cookieManager.CookiePolicy = value;
				}
				else
				{
					base.CookiePolicy = value;
				}
			}
		}

		public override CookieStore CookieStore
		{
			get
			{
				CookieManager cookieManager = @delegate.get();
				if (cookieManager != null)
				{
					return cookieManager.CookieStore;
				}
				return base.CookieStore;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.util.Map<String, java.util.List<String>> get(java.net.URI uri, java.util.Map<String, java.util.List<String>> requestHeaders) throws java.io.IOException
		public override IDictionary<string, IList<string>> get(URI uri, IDictionary<string, IList<string>> requestHeaders)
		{
			CookieManager cookieManager = @delegate.get();
			if (cookieManager != null)
			{
				return cookieManager.get(uri, requestHeaders);
			}
			return base.get(uri, requestHeaders);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void put(java.net.URI uri, java.util.Map<String, java.util.List<String>> responseHeaders) throws java.io.IOException
		public override void put(URI uri, IDictionary<string, IList<string>> responseHeaders)
		{
			CookieManager cookieManager = @delegate.get();
			if (cookieManager != null)
			{
				cookieManager.put(uri, responseHeaders);
			}
			else
			{
				base.put(uri, responseHeaders);
			}
		}

		public static CookieManager CookieManager
		{
			set
			{
				@delegate.set(value);
			}
		}
	}

}