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
namespace pspsharp.network.upnp
{


	using Logger = org.apache.log4j.Logger;
	using Document = org.w3c.dom.Document;
	using Element = org.w3c.dom.Element;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;
	using SAXException = org.xml.sax.SAXException;

	public class IGD
	{
		protected internal static Logger log = UPnP.log;
		internal string descriptionUrl;
		internal string baseUrl;
		internal string presentationUrl;
		internal IGDdataService cif;
		internal IGDdataService first;
		internal IGDdataService second;
		internal IGDdataService ipV6FC;

		protected internal class IGDdataService
		{
			internal string serviceType;
			internal string controlUrl;
			internal string eventSubUrl;
			internal string scpdUrl;

			public override string ToString()
			{
				return string.Format("serviceType={0}[controlUrl={1}, eventSubUrl={2}, scpdUrl={3}]", serviceType, controlUrl, eventSubUrl, scpdUrl);
			}
		}

		public IGD()
		{
		}

		public virtual void discover(string descriptionUrl)
		{
			DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
			documentBuilderFactory.IgnoringElementContentWhitespace = true;
			documentBuilderFactory.IgnoringComments = true;
			documentBuilderFactory.Coalescing = true;

			try
			{
				URL url = new URL(descriptionUrl);
				DocumentBuilder documentBuilder = documentBuilderFactory.newDocumentBuilder();
				Document description = documentBuilder.parse(url.openStream());
				parseIGDdata(description);
				this.descriptionUrl = descriptionUrl;
			}
			catch (ParserConfigurationException e)
			{
				log.error("Discovery", e);
			}
			catch (SAXException e)
			{
				log.error("Discovery", e);
			}
			catch (MalformedURLException e)
			{
				log.error("Discovery", e);
			}
			catch (IOException e)
			{
				log.error("Discovery", e);
			}
		}

		public virtual bool Valid
		{
			get
			{
				return first != null && !string.ReferenceEquals(first.serviceType, null);
			}
		}

		public virtual bool isConnected(UPnP upnp)
		{
			return "Connected".Equals(upnp.getStatusInfo(buildUrl(first.controlUrl), first.serviceType));
		}

		public virtual string getExternalIPAddress(UPnP upnp)
		{
			return upnp.getExternalIPAddress(buildUrl(first.controlUrl), first.serviceType);
		}

		public virtual void addPortMapping(UPnP upnp, string remoteHost, int externalPort, string protocol, int internalPort, string internalClient, string description, int leaseDuration)
		{
			if (first != null)
			{
				upnp.addPortMapping(buildUrl(first.controlUrl), first.serviceType, remoteHost, externalPort, protocol, internalPort, internalClient, description, leaseDuration);
			}
		}

		public virtual void deletePortMapping(UPnP upnp, string remoteHost, int externalPort, string protocol)
		{
			if (first != null)
			{
				upnp.deletePortMapping(buildUrl(first.controlUrl), first.serviceType, remoteHost, externalPort, protocol);
			}
		}

		private void parseIGDdata(Document description)
		{
			baseUrl = null;
			presentationUrl = null;
			cif = null;
			first = null;
			second = null;
			ipV6FC = null;
			parseElement(description.DocumentElement);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("IGD data: {0}", ToString()));
			}
		}

		private void parseElement(Element element)
		{
			if ("service".Equals(element.NodeName))
			{
				parseService(element);
			}
			else if ("URLBase".Equals(element.NodeName))
			{
				baseUrl = getContent(element);
			}
			else if ("presentationURL".Equals(element.NodeName))
			{
				presentationUrl = getContent(element);
			}
			else
			{
				NodeList children = element.ChildNodes;
				for (int i = 0; i < children.Length; i++)
				{
					Node node = children.item(i);
					if (node is Element)
					{
						parseElement((Element) node);
					}
				}
			}
		}

		private string getContent(Node node)
		{
			if (node.hasChildNodes())
			{
				return getContent(node.ChildNodes);
			}

			return node.NodeValue;
		}

		private string getContent(NodeList nodeList)
		{
			if (nodeList == null || nodeList.Length <= 0)
			{
				return null;
			}

			StringBuilder content = new StringBuilder();
			int n = nodeList.Length;
			for (int i = 0; i < n; i++)
			{
				Node node = nodeList.item(i);
				content.Append(getContent(node));
			}

			return content.ToString();
		}

		private string getNodeValue(Element element, string nodeName)
		{
			return getContent(element.getElementsByTagName(nodeName));
		}

		private void parseService(Element element)
		{
			string serviceType = getNodeValue(element, "serviceType");
			IGDdataService dataService = null;
			if ("urn:schemas-upnp-org:service:WANCommonInterfaceConfig:1".Equals(serviceType))
			{
				cif = new IGDdataService();
				dataService = cif;
			}
			else if ("urn:schemas-upnp-org:service:WANIPv6FirewallControl:1".Equals(serviceType))
			{
				ipV6FC = new IGDdataService();
				dataService = ipV6FC;
			}
			else if ("urn:schemas-upnp-org:service:WANIPConnection:1".Equals(serviceType) || "urn:schemas-upnp-org:service:WANPPPConnection:1".Equals(serviceType))
			{
				if (first == null)
				{
					first = new IGDdataService();
					dataService = first;
				}
				else if (second == null)
				{
					second = new IGDdataService();
					dataService = second;
				}
			}

			if (dataService != null)
			{
				dataService.serviceType = serviceType;
				dataService.controlUrl = getNodeValue(element, "controlURL");
				dataService.eventSubUrl = getNodeValue(element, "eventSubURL");
				dataService.scpdUrl = getNodeValue(element, "SCPDURL");
			}
		}

		protected internal virtual string buildUrl(string url)
		{
			if (url.matches("^https?://.*"))
			{
				return url;
			}

			StringBuilder completeUrl = new StringBuilder();
			if (!string.ReferenceEquals(baseUrl, null) && baseUrl.Length > 0)
			{
				completeUrl.Append(baseUrl);
			}
			else
			{
				completeUrl.Append(descriptionUrl);
			}

			int firstColon = completeUrl.ToString().IndexOf(":");
			if (firstColon >= 0)
			{
				int firstSep = completeUrl.ToString().IndexOf("/", firstColon + 3);
				if (firstSep >= 0)
				{
					completeUrl.Length = firstSep;
				}
			}

			if (!url.StartsWith("/", StringComparison.Ordinal))
			{
				completeUrl.Append("/");
			}
			completeUrl.Append(url);

			return completeUrl.ToString();
		}

		public override string ToString()
		{
			return string.Format("urlBase={0}, presentationUrl={1}, CIF: {2}, first: {3}, second: {4}, IPv6FC: {5}", baseUrl, presentationUrl, cif, first, second, ipV6FC);
		}
	}

}