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
namespace pspsharp.graphics.RE
{

	/// <summary>
	/// @author Aredo, gid15
	/// 
	/// Implements a texture anisotropic filter.
	/// </summary>
	public class AnisotropicFilter : BaseRenderingEngineProxy
	{
		// When the anisotropic filter is active, map the magnification filter
		// to TFLT_LINEAR/TFLT_LINEAR_MIPMAP_LINEAR
		protected internal static readonly int[] anisotropicMipmapMagFilter = new int[] {GeCommands.TFLT_LINEAR, GeCommands.TFLT_LINEAR, GeCommands.TFLT_UNKNOW1, GeCommands.TFLT_UNKNOW2, GeCommands.TFLT_LINEAR_MIPMAP_LINEAR, GeCommands.TFLT_LINEAR_MIPMAP_LINEAR, GeCommands.TFLT_LINEAR_MIPMAP_LINEAR, GeCommands.TFLT_LINEAR_MIPMAP_LINEAR};
		private float maxTextureAnisotropy;
		private float textureAnisotropy;
		private bool useTextureAnisotropicFilter;

		public AnisotropicFilter(IRenderingEngine proxy) : base(proxy)
		{
		}

		public virtual float DefaultTextureAnisotropy
		{
			set
			{
				textureAnisotropy = value;
			}
		}

		public override void startDisplay()
		{
			useTextureAnisotropicFilter = VideoEngine.Instance.UseTextureAnisotropicFilter;
			base.startDisplay();
		}

		public override IRenderingEngine RenderingEngine
		{
			set
			{
				maxTextureAnisotropy = value.MaxTextureAnisotropy;
				textureAnisotropy = maxTextureAnisotropy;
				base.RenderingEngine = value;
			}
		}

		public override int TextureMipmapMagFilter
		{
			set
			{
				if (useTextureAnisotropicFilter)
				{
					re.TextureAnisotropy = textureAnisotropy;
					base.TextureMipmapMagFilter = anisotropicMipmapMagFilter[value];
				}
				else
				{
					base.TextureMipmapMagFilter = value;
				}
			}
		}
	}

}