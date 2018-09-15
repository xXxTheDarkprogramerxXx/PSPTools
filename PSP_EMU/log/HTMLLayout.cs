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

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace pspsharp.log
{
	using Layout = org.apache.log4j.Layout;
	using Level = org.apache.log4j.Level;
	using Transform = org.apache.log4j.helpers.Transform;
	using LocationInfo = org.apache.log4j.spi.LocationInfo;
	using LoggingEvent = org.apache.log4j.spi.LoggingEvent;

	/// <summary>
	/// This layout outputs events in a HTML table.
	/// 
	/// Appenders using this layout should have their encoding set to UTF-8 or
	/// UTF-16, otherwise events containing non ASCII characters could result in
	/// corrupted log files.
	/// 
	/// @author Ceki G&uuml;lc&uuml;
	/// @modified Florent Castelli
	/// </summary>
	public class HTMLLayout : Layout
	{
		private bool InstanceFieldsInitialized = false;

		public HTMLLayout()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			sbuf = new StringBuilder(BUF_SIZE);
		}


		protected internal readonly int BUF_SIZE = 256;
		protected internal readonly int MAX_CAPACITY = 1024;

		internal static string TRACE_PREFIX = "<br>&nbsp;&nbsp;&nbsp;&nbsp;";

		// output buffer appended to when format() is invoked
		private StringBuilder sbuf;

		/// <summary>
		/// A string constant used in naming the option for setting the the location
		/// information flag. Current value of this string constant is
		/// <b>LocationInfo</b>.
		/// 
		/// <para>
		/// Note that all option keys are case sensitive.
		/// 
		/// </para>
		/// </summary>
		/// @deprecated Options are now handled using the JavaBeans paradigm. This
		///             constant is not longer needed and will be removed in the
		///             <em>near</em> term. 
		[Obsolete("Options are now handled using the JavaBeans paradigm. This")]
		public const string LOCATION_INFO_OPTION = "LocationInfo";

		/// <summary>
		/// A string constant used in naming the option for setting the the HTML
		/// document title. Current value of this string constant is <b>Title</b>.
		/// </summary>
		public const string TITLE_OPTION = "Title";

		// Print no location info by default
		internal bool locationInfo = false;

		internal string title = "Log4J Log Messages";

		/// <summary>
		/// The <b>LocationInfo</b> option takes a boolean value. By default, it is
		/// set to false which means there will be no location information output by
		/// this layout. If the the option is set to true, then the file name and
		/// line number of the statement at the origin of the log statement will be
		/// output.
		/// 
		/// <para>
		/// If you are embedding this layout within an
		/// <seealso cref="org.apache.log4j.net.SMTPAppender"/> then make sure to set the
		/// <b>LocationInfo</b> option of that appender as well.
		/// </para>
		/// </summary>
		public virtual bool LocationInfo
		{
			set
			{
				locationInfo = value;
			}
			get
			{
				return locationInfo;
			}
		}


		/// <summary>
		/// The <b>Title</b> option takes a String value. This option sets the
		/// document title of the generated HTML document.
		/// 
		/// <para>
		/// Defaults to 'Log4J Log Messages'.
		/// </para>
		/// </summary>
		public virtual string Title
		{
			set
			{
				this.title = value;
			}
			get
			{
				return title;
			}
		}


		/// <summary>
		/// Returns the content type output by this layout, i.e "text/html".
		/// </summary>
		public override string ContentType
		{
			get
			{
				return "text/html";
			}
		}

		/// <summary>
		/// No options to activate.
		/// </summary>
		public override void activateOptions()
		{
		}

		public override string format(LoggingEvent @event)
		{

			if (sbuf.Capacity > MAX_CAPACITY)
			{
				sbuf = new StringBuilder(BUF_SIZE);
			}
			else
			{
				sbuf.Length = 0;
			}

			sbuf.Append(Layout.LINE_SEP + "<tr>" + Layout.LINE_SEP);

			sbuf.Append("<td>");
			sbuf.Append(@event.timeStamp - LoggingEvent.StartTime);
			sbuf.Append("</td>" + Layout.LINE_SEP);

			string escapedThread = Transform.escapeTags(@event.ThreadName);
			sbuf.Append("<td title=\"" + escapedThread + " thread\">");
			sbuf.Append(escapedThread);
			sbuf.Append("</td>" + Layout.LINE_SEP);

			sbuf.Append("<td title=\"Level\" loglevel=\"");
			sbuf.Append(@event.Level.toInt());
			sbuf.Append("\">");
			if (@event.Level.Equals(Level.DEBUG))
			{
				sbuf.Append("<font color=\"#339933\">");
				sbuf.Append(Transform.escapeTags(@event.Level.ToString()));
				sbuf.Append("</font>");
			}
			else if (@event.Level.isGreaterOrEqual(Level.WARN))
			{
				sbuf.Append("<font color=\"#993300\"><strong>");
				sbuf.Append(Transform.escapeTags(@event.Level.ToString()));
				sbuf.Append("</strong></font>");
			}
			else
			{
				sbuf.Append(Transform.escapeTags(@event.Level.ToString()));
			}
			sbuf.Append("</td>" + Layout.LINE_SEP);

			string escapedLogger = Transform.escapeTags(@event.LoggerName);
			sbuf.Append("<td title=\"" + escapedLogger + " category\">");
			sbuf.Append(escapedLogger);
			sbuf.Append("</td>" + Layout.LINE_SEP);

			if (locationInfo)
			{
				LocationInfo locInfo = @event.LocationInformation;
				sbuf.Append("<td>");
				sbuf.Append(Transform.escapeTags(locInfo.FileName));
				sbuf.Append(':');
				sbuf.Append(locInfo.LineNumber);
				sbuf.Append("</td>" + Layout.LINE_SEP);
			}

			sbuf.Append("<td title=\"Message\" class=\"message\">");
			sbuf.Append(Transform.escapeTags(@event.RenderedMessage));
			sbuf.Append("</td>" + Layout.LINE_SEP);
			sbuf.Append("</tr>" + Layout.LINE_SEP);

			if (@event.NDC != null)
			{
				sbuf.Append("<tr><td bgcolor=\"#EEEEEE\" style=\"font-size : xx-small;\" colspan=\"6\" title=\"Nested Diagnostic Context\">");
				sbuf.Append("NDC: " + Transform.escapeTags(@event.NDC));
				sbuf.Append("</td></tr>" + Layout.LINE_SEP);
			}

			string[] s = @event.ThrowableStrRep;
			if (s != null)
			{
				sbuf.Append("<tr><td bgcolor=\"#993300\" style=\"color:White; font-size : xx-small;\" colspan=\"6\">");
				appendThrowableAsHTML(s, sbuf);
				sbuf.Append("</td></tr>" + Layout.LINE_SEP);
			}

			return sbuf.ToString();
		}

		internal virtual void appendThrowableAsHTML(string[] s, StringBuilder sbuf)
		{
			if (s != null)
			{
				int len = s.Length;
				if (len == 0)
				{
					return;
				}
				sbuf.Append(Transform.escapeTags(s[0]));
				sbuf.Append(Layout.LINE_SEP);
				for (int i = 1; i < len; i++)
				{
					sbuf.Append(TRACE_PREFIX);
					sbuf.Append(Transform.escapeTags(s[i]));
					sbuf.Append(Layout.LINE_SEP);
				}
			}
		}

		/// <summary>
		/// Returns appropriate HTML headers.
		/// </summary>
		public override string Header
		{
			get
			{
				StringBuilder sbuf = new StringBuilder();
				sbuf.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">" + Layout.LINE_SEP);
				sbuf.Append("<html>" + Layout.LINE_SEP);
				sbuf.Append("<head>" + Layout.LINE_SEP);
				sbuf.Append("<title>" + title + "</title>" + Layout.LINE_SEP);
				sbuf.Append("<style type=\"text/css\">" + Layout.LINE_SEP);
				sbuf.Append("<!--" + Layout.LINE_SEP);
				sbuf.Append("body, table {font-family: arial,sans-serif; font-size: x-small;}" + Layout.LINE_SEP);
				sbuf.Append("th {background: #336699; color: #FFFFFF; text-align: left;}" + Layout.LINE_SEP);
				sbuf.Append("td.message {white-space: pre; font-family: monospace;}" + Layout.LINE_SEP);
				sbuf.Append("-->" + Layout.LINE_SEP);
				sbuf.Append("</style>" + Layout.LINE_SEP);
				sbuf.Append("<script type=\"text/javascript\">" + Layout.LINE_SEP);
				sbuf.Append("var isIE = false;" + Layout.LINE_SEP);
				sbuf.Append("if(navigator.userAgent.indexOf('MSIE') >= 0){isIE = true;}" + Layout.LINE_SEP);
				sbuf.Append("function changeLogLevel(level) {" + Layout.LINE_SEP);
				sbuf.Append("  var allElements, e;" + Layout.LINE_SEP);
				sbuf.Append("    allElements = document.getElementsByTagName(\"td\");" + Layout.LINE_SEP);
				sbuf.Append("  for ( var i = 0; i < allElements.Length; i++) {" + Layout.LINE_SEP);
				sbuf.Append("    e = allElements[i];" + Layout.LINE_SEP);
				sbuf.Append("    if(e.getAttribute(\"logLevel\") != null)" + Layout.LINE_SEP);
				sbuf.Append("      if(e.getAttribute(\"logLevel\") < level)" + Layout.LINE_SEP);
				sbuf.Append("        e.parentNode.style.display = \"none\";" + Layout.LINE_SEP);
				sbuf.Append("      else" + Layout.LINE_SEP);
				sbuf.Append("        e.parentNode.style.display = isIE ? \"block\" : \"table-row\";" + Layout.LINE_SEP);
				sbuf.Append("  }" + Layout.LINE_SEP);
				sbuf.Append("}" + Layout.LINE_SEP);
				sbuf.Append("function findUnimplemented() {" + Layout.LINE_SEP);
				sbuf.Append("  var allElements, e, recorded;" + Layout.LINE_SEP);
				sbuf.Append("  recorded = new Array();" + Layout.LINE_SEP);
				sbuf.Append("  allElements = document.getElementsByTagName(\"td\");" + Layout.LINE_SEP);
				sbuf.Append("  for ( var i = 0; i < allElements.Length; i++) {" + Layout.LINE_SEP);
				sbuf.Append("    e = allElements[i];" + Layout.LINE_SEP);
				sbuf.Append("    if (e.getAttribute(\"title\") == \"Message\") {" + Layout.LINE_SEP);
				sbuf.Append("      var m = isIE ? e.innerHTML.toLowerCase() : e.textContent.toLowerCase();" + Layout.LINE_SEP);
    
				sbuf.Append("      if ((m.indexOf(\"unimplement\") == -1" + Layout.LINE_SEP);
				sbuf.Append("          && m.indexOf(\"unsupport\") == -1)" + Layout.LINE_SEP);
				sbuf.Append("          || recorded[m] != null)" + Layout.LINE_SEP);
				sbuf.Append("        e.parentNode.style.display = \"none\";" + Layout.LINE_SEP);
				sbuf.Append("      else {" + Layout.LINE_SEP);
				sbuf.Append("        if(m.indexOf(\"unsupported syscall\") != 1)" + Layout.LINE_SEP);
				sbuf.Append("          m = m.substr(0, m.Length - 27);" + Layout.LINE_SEP);
				sbuf.Append("        if(recorded[m] != null)" + Layout.LINE_SEP);
				sbuf.Append("          e.parentNode.style.display = \"none\";" + Layout.LINE_SEP);
				sbuf.Append("        else {   " + Layout.LINE_SEP);
				sbuf.Append("          recorded[m] = true;" + Layout.LINE_SEP);
				sbuf.Append("          e.parentNode.style.display = isIE ? \"block\" : \"table-row\";" + Layout.LINE_SEP);
				sbuf.Append("        }" + Layout.LINE_SEP);
				sbuf.Append("      }" + Layout.LINE_SEP);
				sbuf.Append("    }" + Layout.LINE_SEP);
				sbuf.Append("  }" + Layout.LINE_SEP);
				sbuf.Append("}" + Layout.LINE_SEP);
				sbuf.Append("</script>" + Layout.LINE_SEP);
				sbuf.Append("</head>" + Layout.LINE_SEP);
				sbuf.Append("<body bgcolor=\"#FFFFFF\" topmargin=\"6\" leftmargin=\"6\">" + Layout.LINE_SEP);
				foreach (Level l in new Level[] {Level.FATAL, Level.ERROR, Level.WARN, Level.INFO, Level.DEBUG, Level.TRACE})
				{
					sbuf.Append("<button onclick=\"javascript:changeLogLevel(");
					sbuf.Append(l.toInt());
					sbuf.Append(")\" type=\"button\">");
					sbuf.Append(l);
					sbuf.Append("</button>" + Layout.LINE_SEP);
				}
				sbuf.Append("<button onclick=\"javascript:findUnimplemented()\" type=\"button\">Find unimplemented</button>" + Layout.LINE_SEP);
				sbuf.Append("<hr size=\"1\" noshade>" + Layout.LINE_SEP);
				sbuf.Append("Log session start time " + DateTime.Now + "<br>" + Layout.LINE_SEP);
				sbuf.Append("<br>" + Layout.LINE_SEP);
				sbuf.Append("<table cellspacing=\"0\" cellpadding=\"4\" border=\"1\" bordercolor=\"#224466\" width=\"100%\">" + Layout.LINE_SEP);
				sbuf.Append("<tr>" + Layout.LINE_SEP);
				sbuf.Append("<th>Time</th>" + Layout.LINE_SEP);
				sbuf.Append("<th>Thread</th>" + Layout.LINE_SEP);
				sbuf.Append("<th>Level</th>" + Layout.LINE_SEP);
				sbuf.Append("<th>Category</th>" + Layout.LINE_SEP);
				if (locationInfo)
				{
					sbuf.Append("<th>File:Line</th>" + Layout.LINE_SEP);
				}
				sbuf.Append("<th>Message</th>" + Layout.LINE_SEP);
				sbuf.Append("</tr>" + Layout.LINE_SEP);
				return sbuf.ToString();
			}
		}

		/// <summary>
		/// Returns the appropriate HTML footers.
		/// </summary>
		public override string Footer
		{
			get
			{
				StringBuilder sbuf = new StringBuilder();
				sbuf.Append("</table>" + Layout.LINE_SEP);
				sbuf.Append("<br>" + Layout.LINE_SEP);
				sbuf.Append("</body></html>");
				return sbuf.ToString();
			}
		}

		/// <summary>
		/// The HTML layout handles the throwable contained in logging events. Hence,
		/// this method return <code>false</code>.
		/// </summary>
		public override bool ignoresThrowable()
		{
			return false;
		}
	}
}