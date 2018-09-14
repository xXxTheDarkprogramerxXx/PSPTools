using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

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


	using Settings = pspsharp.settings.Settings;

	using AppenderSkeleton = org.apache.log4j.AppenderSkeleton;
	using Layout = org.apache.log4j.Layout;
	using Level = org.apache.log4j.Level;
	using LoggingEvent = org.apache.log4j.spi.LoggingEvent;

	/// <summary>
	/// <b>Experimental</b> TextPaneAppender. <br>
	/// 
	/// 
	/// Created: Sat Feb 26 18:50:27 2000 <br>
	/// 
	/// @author Sven Reimers
	/// </summary>
	public class TextPaneAppender : AppenderSkeleton
	{

		internal JTextPane textpane;
		internal StyledDocument doc;
		internal StringWriter sw;
		internal ConcurrentDictionary<Level, MutableAttributeSet> attributes;
		internal ConcurrentDictionary<Level, ImageIcon> icons;
		private string label;
		private bool fancy;
		internal readonly string LABEL_OPTION = "Label";
		internal readonly string COLOR_OPTION_FATAL = "Color.Emerg";
		internal readonly string COLOR_OPTION_ERROR = "Color.Error";
		internal readonly string COLOR_OPTION_WARN = "Color.Warn";
		internal readonly string COLOR_OPTION_INFO = "Color.Info";
		internal readonly string COLOR_OPTION_DEBUG = "Color.Debug";
		internal readonly string COLOR_OPTION_BACKGROUND = "Color.Background";
		internal readonly string FANCY_OPTION = "Fancy";
		internal readonly string FONT_NAME_OPTION = "Font.Name";
		internal readonly string FONT_SIZE_OPTION = "Font.Size";
		internal static readonly Level[] levels = new Level[] {Level.FATAL, Level.ERROR, Level.WARN, Level.INFO, Level.DEBUG, Level.TRACE};

		public static Image loadIcon(string path)
		{
			Image img = null;
			try
			{
				URL url = ClassLoader.getSystemResource(path);
				img = (Toolkit.DefaultToolkit).getImage(url);
			}
			catch (Exception e)
			{
				Emulator.log.error("Exception occured: " + e.Message, e);
			}
			return (img);
		}

		public TextPaneAppender(Layout layout, string name) : this()
		{
			this.layout = layout;
			this.name = name;
			TextPane = new JTextPane();
			createAttributes();
			createIcons();
		}

		public TextPaneAppender() : base()
		{
			TextPane = new JTextPane();
			createAttributes();
			createIcons();
			label = "";
			sw = new StringWriter();
			fancy = false;
		}

		public override void close()
		{
		}

		private void createAttributes()
		{
			attributes = new ConcurrentDictionary<Level, MutableAttributeSet>();
			for (int i = 0; i < levels.Length; i++)
			{
				MutableAttributeSet att = new SimpleAttributeSet();
				attributes[levels[i]] = att;
				StyleConstants.setFontSize(att, Settings.Instance.Font.Size);
				StyleConstants.setFontFamily(att, Settings.Instance.Font.Family);
			}
			StyleConstants.setForeground(attributes[Level.FATAL], Color.red);
			StyleConstants.setForeground(attributes[Level.ERROR], Color.red);
			StyleConstants.setForeground(attributes[Level.WARN], Color.orange);
			StyleConstants.setForeground(attributes[Level.INFO], Color.black);
			StyleConstants.setForeground(attributes[Level.DEBUG], Color.gray);
			StyleConstants.setForeground(attributes[Level.TRACE], Color.gray);
		}

		private void createIcons()
		{
			icons = new ConcurrentDictionary<Level, ImageIcon>();
		}

		public override void append(LoggingEvent @event)
		{
			string text = layout.format(@event);
			string trace = "";
			string keyword = Settings.Instance.readString("log.keyword");
			if (@event.ThrowableInformation != null)
			{
				string[] ts = @event.ThrowableStrRep;
				foreach (string s in ts)
				{
					sw.write(s);
				}
				for (int i = 0; i < sw.Buffer.length(); i++)
				{
					if (sw.Buffer.charAt(i) == '\t')
					{
						sw.Buffer.replace(i, i + 1, "        ");
					}
				}
				trace = sw.ToString();
				sw.Buffer.delete(0, sw.Buffer.length());
			}
			try
			{
				lock (textpane)
				{
					if (fancy)
					{
						textpane.Editable = true;
						textpane.insertIcon(icons[@event.Level]);
						textpane.Editable = false;
					}

					// Log everything if there's no keyword, or just log messages with the
					// specified keyword when it exists.
					if (keyword.Equals("LOG_ALL") || (!keyword.Equals("LOG_ALL") && text.Contains(keyword)))
					{
						doc.insertString(doc.Length, text + trace, attributes[@event.Level]);
					}

					int l = doc.Length;
					if (l > 30000)
					{
						doc.remove(0, l - 30000);
					}
				}
			}
			catch (BadLocationException badex)
			{
				Console.Error.WriteLine(badex);
			}
			textpane.CaretPosition = doc.Length;
		}

		public virtual JTextPane TextPane
		{
			get
			{
				return textpane;
			}
			set
			{
				this.textpane = value;
				value.Editable = false;
				doc = value.StyledDocument;
			}
		}

		private static Color parseColor(string v)
		{
			StringTokenizer st = new StringTokenizer(v, ",");
			int[] val = new int[] {255, 255, 255, 255};
			int i = 0;
			while (st.hasMoreTokens())
			{
				val[i] = int.Parse(st.nextToken());
				i++;
			}
			return new Color(val[0], val[1], val[2], val[3]);
		}

		private static string colorToString(Color c)
		{
			// alpha component emitted only if not default (255)
			string res = "" + c.Red + "," + c.Green + "," + c.Blue;
			return c.Alpha >= 255 ? res : res + "," + c.Alpha;
		}

		public override Layout Layout
		{
			set
			{
				this.layout = value;
			}
		}

		public override string Name
		{
			set
			{
				this.name = value;
			}
		}


		private void setColor(Level p, string v)
		{
			StyleConstants.setForeground(attributes[p], parseColor(v));
		}

		private string getColor(Level p)
		{
			Color c = StyleConstants.getForeground(attributes[p]);
			return c == null ? null : colorToString(c);
		}

		public virtual string Label
		{
			set
			{
				this.label = value;
			}
			get
			{
				return label;
			}
		}


		public virtual string ColorEmerg
		{
			set
			{
				setColor(Level.FATAL, value);
			}
			get
			{
				return getColor(Level.FATAL);
			}
		}


		public virtual string ColorError
		{
			set
			{
				setColor(Level.ERROR, value);
			}
			get
			{
				return getColor(Level.ERROR);
			}
		}


		public virtual string ColorWarn
		{
			set
			{
				setColor(Level.WARN, value);
			}
			get
			{
				return getColor(Level.WARN);
			}
		}


		public virtual string ColorInfo
		{
			set
			{
				setColor(Level.INFO, value);
			}
			get
			{
				return getColor(Level.INFO);
			}
		}


		public virtual string ColorDebug
		{
			set
			{
				setColor(Level.DEBUG, value);
			}
			get
			{
				return getColor(Level.DEBUG);
			}
		}


		public virtual string ColorBackground
		{
			set
			{
				textpane.Background = parseColor(value);
			}
			get
			{
				return colorToString(textpane.Background);
			}
		}


		public virtual bool Fancy
		{
			set
			{
				this.fancy = value;
			}
			get
			{
				return fancy;
			}
		}


		public virtual int FontSize
		{
			set
			{
				IEnumerator<MutableAttributeSet> e = attributes.Values.GetEnumerator();
				while (e.MoveNext())
				{
					StyleConstants.setFontSize(e.Current, value);
				}
			}
			get
			{
				AttributeSet attrSet = attributes[Level.INFO];
				return StyleConstants.getFontSize(attrSet);
			}
		}


		public virtual string FontName
		{
			set
			{
				IEnumerator<MutableAttributeSet> e = attributes.Values.GetEnumerator();
				while (e.MoveNext())
				{
					StyleConstants.setFontFamily(e.Current, value);
				}
			}
			get
			{
				AttributeSet attrSet = attributes[Level.INFO];
				return StyleConstants.getFontFamily(attrSet);
			}
		}


		public override bool requiresLayout()
		{
			return true;
		}
	}

}