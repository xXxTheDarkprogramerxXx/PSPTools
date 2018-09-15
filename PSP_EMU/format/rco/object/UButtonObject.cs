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
namespace pspsharp.format.rco.@object
{

	using EventType = pspsharp.format.rco.type.EventType;
	using ImageType = pspsharp.format.rco.type.ImageType;
	using IntType = pspsharp.format.rco.type.IntType;
	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;

	public class UButtonObject : BasePositionObject
	{
		[ObjectField(order : 201)]
		public ImageType image;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[ObjectField(order : 202)]
		public EventType onPush_Renamed;
		[ObjectField(order : 203)]
		public EventType onFocusIn;
		[ObjectField(order : 204)]
		public EventType onFocusOut;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[ObjectField(order : 205)]
		public EventType onLeft_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[ObjectField(order : 206)]
		public EventType onRight_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[ObjectField(order : 207)]
		public EventType onUp_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[ObjectField(order : 208)]
		public EventType onDown_Renamed;
		[ObjectField(order : 209)]
		public IntType unknownInt32;
		// Will be set by setTexture()
		private ImageObject texture;

		public override BufferedImage Image
		{
			get
			{
				if (texture != null)
				{
					return texture.Image;
				}
				return image.Image;
			}
		}

		public override void onUp()
		{
			trigger(onUp_Renamed);
		}

		public override void onDown()
		{
			trigger(onDown_Renamed);
		}

		public override void onLeft()
		{
			trigger(onLeft_Renamed);
		}

		public override void onRight()
		{
			trigger(onRight_Renamed);
		}

		public override void onPush()
		{
			trigger(onPush_Renamed);
		}

		public override void setFocus()
		{
			trigger(onFocusIn);

			base.setFocus();
		}

		public override void focusOut()
		{
			trigger(onFocusOut);

			base.focusOut();
		}

		public virtual BaseNativeObject Texture
		{
			set
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("UButtonObject.setTexture {0}", value));
				}
				if (value is ImageObject)
				{
					this.texture = (ImageObject) value;
				}
    
				onDisplayUpdated();
			}
		}

		protected internal override void ToString(StringBuilder s)
		{
			if (texture != null)
			{
				s.Append(string.Format(", texture={0}", texture));
			}
			base.ToString(s);
		}
	}

}