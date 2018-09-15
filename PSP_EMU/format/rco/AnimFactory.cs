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
	using Anim = pspsharp.format.rco.anim.Anim;
	using BaseAnim = pspsharp.format.rco.anim.BaseAnim;
	using DelayAnim = pspsharp.format.rco.anim.DelayAnim;
	using FadeAnim = pspsharp.format.rco.anim.FadeAnim;
	using FireEventAnim = pspsharp.format.rco.anim.FireEventAnim;
	using LockAnim = pspsharp.format.rco.anim.LockAnim;
	using MoveToAnim = pspsharp.format.rco.anim.MoveToAnim;
	using RecolourAnim = pspsharp.format.rco.anim.RecolourAnim;
	using ResizeAnim = pspsharp.format.rco.anim.ResizeAnim;
	using RotateAnim = pspsharp.format.rco.anim.RotateAnim;
	using SlideOutAnim = pspsharp.format.rco.anim.SlideOutAnim;
	using UnlockAnim = pspsharp.format.rco.anim.UnlockAnim;

	public class AnimFactory
	{
		public static BaseAnim newAnim(int type)
		{
			switch (type)
			{
				case 1:
					return new Anim();
				case 2:
					return new MoveToAnim();
				case 3:
					return new RecolourAnim();
				case 4:
					return new RotateAnim();
				case 5:
					return new ResizeAnim();
				case 6:
					return new FadeAnim();
				case 7:
					return new DelayAnim();
				case 8:
					return new FireEventAnim();
				case 9:
					return new LockAnim();
				case 10:
					return new UnlockAnim();
				case 11:
					return new SlideOutAnim();
			}

			RCO.Console.WriteLine(string.Format("AnimFactory.newAnim unknown type 0x{0:X}", type));

			return null;
		}
	}

}