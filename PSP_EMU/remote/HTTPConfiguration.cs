using System;
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
namespace pspsharp.remote
{
	public class HTTPConfiguration
	{
		public class HttpServerConfiguration
		{
			public string serverName;
			public int serverPort;
			public bool doKeepAlive;
			public string[] fakedPaths;

			public HttpServerConfiguration(string serverName)
			{
				this.serverName = serverName;
				serverPort = 80;
				doKeepAlive = true;
			}

			public HttpServerConfiguration(string serverName, int serverPort)
			{
				this.serverName = serverName;
				this.serverPort = serverPort;
				doKeepAlive = true;
			}

			public HttpServerConfiguration(string serverName, int serverPort, bool doKeepAlive)
			{
				this.serverName = serverName;
				this.serverPort = serverPort;
				this.doKeepAlive = doKeepAlive;
			}

			public HttpServerConfiguration(string serverName, int serverPort, bool doKeepAlive, string[] fakedPaths)
			{
				this.serverName = serverName;
				this.serverPort = serverPort;
				this.doKeepAlive = doKeepAlive;
				this.fakedPaths = fakedPaths;
			}

			public virtual string BaseUrl
			{
				get
				{
					return getBaseUrl(Https);
				}
			}

			internal virtual string getBaseUrl(bool isHttps)
			{
				StringBuilder baseUrl = new StringBuilder();

				if (isHttps)
				{
					baseUrl.Append("https");
				}
				else
				{
					baseUrl.Append("http");
				}
				baseUrl.Append("://");
				baseUrl.Append(serverName);
				if (serverPort != 80 && serverPort != 443)
				{
					baseUrl.Append(":");
					baseUrl.Append(serverPort);
				}
				baseUrl.Append("/");

				return baseUrl.ToString();
			}

			public virtual bool isMatchingUrl(string url)
			{
				if (string.ReferenceEquals(url, null))
				{
					return false;
				}

				if (url.StartsWith(BaseUrl, StringComparison.Ordinal))
				{
					return true;
				}

				// The URL could have already been patched and "https" replaced with "http"
				if (Https && url.StartsWith(getBaseUrl(false), StringComparison.Ordinal))
				{
					return true;
				}

				return false;
			}

			public virtual bool Https
			{
				get
				{
					return serverPort == 443;
				}
			}
		}

		public static HttpServerConfiguration[] doProxyServers = new HttpServerConfiguration[]
		{
			new HttpServerConfiguration("fe01.psp.update.playstation.org", 80, true, new string[] {"/update/eu/psp-updatelist.txt"}),
			new HttpServerConfiguration("native.np.ac.playstation.net", 443, false),
			new HttpServerConfiguration("legaldoc.dl.playstation.net"),
			new HttpServerConfiguration("auth.np.ac.playstation.net", 443),
			new HttpServerConfiguration("getprof.gb.np.community.playstation.net", 443, true, new string[] {"/basic_view/func/get_avatar_category", "/basic_view/func/get_avatar_list"}),
			new HttpServerConfiguration("getprof.us.np.community.playstation.net", 443, true, new string[] {"/basic_view/func/get_avatar_category", "/basic_view/func/get_avatar_list"}),
			new HttpServerConfiguration("getprof.de.np.community.playstation.net", 443, true, new string[] {"/basic_view/func/get_avatar_category", "/basic_view/func/get_avatar_list"}),
			new HttpServerConfiguration("getprof.jp.np.community.playstation.net", 443, true, new string[] {"/basic_view/func/get_avatar_category", "/basic_view/func/get_avatar_list"}),
			new HttpServerConfiguration("getprof.hk.np.community.playstation.net", 443, true, new string[] {"/basic_view/func/get_avatar_category", "/basic_view/func/get_avatar_list"}),
			new HttpServerConfiguration("getprof.cn.np.community.playstation.net", 443, true, new string[] {"/basic_view/func/get_avatar_category", "/basic_view/func/get_avatar_list"}),
			new HttpServerConfiguration("getprof.gb.np.community.playstation.net"),
			new HttpServerConfiguration("getprof.us.np.community.playstation.net"),
			new HttpServerConfiguration("getprof.de.np.community.playstation.net"),
			new HttpServerConfiguration("getprof.jp.np.community.playstation.net"),
			new HttpServerConfiguration("getprof.hk.np.community.playstation.net"),
			new HttpServerConfiguration("getprof.cn.np.community.playstation.net"),
			new HttpServerConfiguration("profile.gb.np.community.playstation.net", 443),
			new HttpServerConfiguration("profile.us.np.community.playstation.net", 443),
			new HttpServerConfiguration("profile.de.np.community.playstation.net", 443),
			new HttpServerConfiguration("profile.jp.np.community.playstation.net", 443),
			new HttpServerConfiguration("profile.hk.np.community.playstation.net", 443),
			new HttpServerConfiguration("profile.cn.np.community.playstation.net", 443),
			new HttpServerConfiguration("static-resource.np.community.playstation.net"),
			new HttpServerConfiguration("commerce.np.ac.playstation.net", 443, true, new string[] {"/cap.m", "/kdp.m"}),
			new HttpServerConfiguration("account.np.ac.playstation.net", 443),
			new HttpServerConfiguration("mds.np.ac.playstation.net", 443),
			new HttpServerConfiguration("nsx.sec.np.dl.playstation.net", 443, true, new string[] {"/"}),
			new HttpServerConfiguration("nsx-e.sec.np.dl.playstation.net", 443),
			new HttpServerConfiguration("nsx-e.np.dl.playstation.net"),
			new HttpServerConfiguration("video.dl.playstation.net", 80, true, new string[] {"/cdn/video/US/g"}),
			new HttpServerConfiguration("apollo.dl.playstation.net"),
			new HttpServerConfiguration("poseidon.dl.playstation.net"),
			new HttpServerConfiguration("zeus.dl.playstation.net"),
			new HttpServerConfiguration("comic.dl.playstation.net"),
			new HttpServerConfiguration("infoboard.ww.dl.playstation.net"),
			new HttpServerConfiguration("a0.ww.np.dl.playstation.net", 443),
			new HttpServerConfiguration("www.playstation.com", 443),
			new HttpServerConfiguration("radio.psp.dl.playstation.net"),
			new HttpServerConfiguration("api.shoutcast.com"),
			new HttpServerConfiguration("yp.shoutcast.com"),
			new HttpServerConfiguration("www.shoutcast.com"),
			new HttpServerConfiguration("cdn-a.sonyentertainmentnetwork.com", 443),
			new HttpServerConfiguration("boxreg.trendmicro.com", 443),
			new HttpServerConfiguration("eu.playstation.com", 443, false),
			new HttpServerConfiguration("fj00.psp.update.playstation.org"),
			new HttpServerConfiguration("sensme.dl.playstation.net"),
			new HttpServerConfiguration("ps-devices.qriocity.com"),
			new HttpServerConfiguration("www.playstationmusic.com", 443),
			new HttpServerConfiguration("uploader.us.np.community.playstation.net", 443)
		};
	}

}