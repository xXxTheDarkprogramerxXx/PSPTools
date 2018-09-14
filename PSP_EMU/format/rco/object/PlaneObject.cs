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

	using ImageType = pspsharp.format.rco.type.ImageType;
	using IntType = pspsharp.format.rco.type.IntType;
	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;

	public class PlaneObject : BasePositionObject
	{
		[ObjectField(order : 201)]
		public ImageType image;
		[ObjectField(order : 202)]
		public IntType unknownInt18;
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

		public virtual BaseNativeObject Texture
		{
			set
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("PlaneObject.setTexture {0}", value));
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