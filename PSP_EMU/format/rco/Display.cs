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
namespace pspsharp.format.rco
{

	//using Logger = org.apache.log4j.Logger;

	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;
	using MoviePlayer = pspsharp.format.rco.vsmx.objects.MoviePlayer;

	public class Display : JComponent
	{
		private const long serialVersionUID = 5488196052725313236L;
		private static readonly Logger log = RCO.log;
		private IList<IDisplay> objects;
		private IDisplay focus;
		private float moviePlayerWidth = (float) MoviePlayer.DEFAULT_WIDTH;
		private float moviePlayerHeight = (float) MoviePlayer.DEFAULT_HEIGHT;

		public Display()
		{
			objects = new LinkedList<IDisplay>();
		}

		protected internal override void paintComponent(Graphics g)
		{
			base.paintComponent(g);

			if (objects.Count > 0 && log.TraceEnabled)
			{
				log.trace(string.Format("Starting to paint Display with {0:D} objects, focus={1}", objects.Count, focus));
				log.trace(string.Format("display {0:D}x{1:D}", Width, Height));
			}

			lock (objects)
			{
				foreach (IDisplay @object in objects)
				{
					paint(g, @object);
				}
			}
		}

		private void paint(Graphics g, IDisplay @object)
		{
			int cx = @object.X;
			int cy = @object.Y;
			int width = @object.Width;
			int height = @object.Height;
			int x = cx - width / 2;
			int y = cy + height / 2;

			BufferedImage image = @object.AnimImage;

			float displayWidth = Width;
			float displayHeight = Height;
			x = System.Math.Round(x / (moviePlayerWidth / 2f) * (displayWidth / 2f) + (displayWidth / 2f));
			y = System.Math.Round(-y / (moviePlayerHeight / 2f) * (displayHeight / 2f) + (displayHeight / 2f));
			width = System.Math.Round(width / (moviePlayerWidth / displayWidth));
			height = System.Math.Round(height / (moviePlayerWidth / displayWidth));
			float alpha = @object.Alpha;

			if (log.TraceEnabled)
			{
				log.trace(string.Format("paint at ({0:D},{1:D}) {2:D}x{3:D} alpha={4:F} - image={5:D}x{6:D}, object={7}", x, y, width, height, alpha, image == null ? 0 : image.Width, image == null ? 0 : image.Height, @object));
			}

			if (image != null)
			{
				if (g is Graphics2D)
				{
					AlphaComposite ac = AlphaComposite.getInstance(AlphaComposite.SRC_ATOP, alpha);
					((Graphics2D) g).Composite = ac;
				}
				g.drawImage(image, x, y, x + width - 1, y + height - 1, 0, 0, image.Width - 1, image.Height - 1, null);
			}
			else
			{
				g.Color = Color.BLACK;
				g.drawRect(x, y, width, height);
			}

			if (focus == @object)
			{
				g.Color = Color.RED;
				g.drawRect(x, y, width, height);
			}
		}

		public virtual void add(IDisplay @object)
		{
			lock (objects)
			{
				objects.Add(@object);
			}
		}

		public virtual void add(BaseNativeObject @object)
		{
			if (@object is IDisplay)
			{
				add((IDisplay) @object);
			}
		}

		public virtual void add(VSMXBaseObject @object)
		{
			if (@object is VSMXNativeObject)
			{
				add(((VSMXNativeObject) @object).Object);
			}
		}

		public virtual void remove(IDisplay @object)
		{
			lock (objects)
			{
				objects.Remove(@object);
			}
		}

		public virtual void remove(BaseNativeObject @object)
		{
			if (@object is IDisplay)
			{
				remove((IDisplay) @object);
			}
		}

		public virtual void remove(VSMXBaseObject @object)
		{
			if (@object is VSMXNativeObject)
			{
				remove(((VSMXNativeObject) @object).Object);
			}
		}

		public virtual void changeResource()
		{
			lock (objects)
			{
				objects.Clear();
			}
			focus = null;
		}

		public virtual IDisplay Focus
		{
			set
			{
				if (this.focus != value)
				{
					this.focus = value;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Display.setFocus {0}", value));
					}
					repaint();
				}
			}
		}

		public virtual BaseNativeObject Focus
		{
			set
			{
				if (value is IDisplay)
				{
					Focus = (IDisplay) value;
				}
			}
		}

		public virtual VSMXBaseObject Focus
		{
			set
			{
				if (value is VSMXNativeObject)
				{
					Focus = ((VSMXNativeObject) value).Object;
				}
			}
		}
	}

}