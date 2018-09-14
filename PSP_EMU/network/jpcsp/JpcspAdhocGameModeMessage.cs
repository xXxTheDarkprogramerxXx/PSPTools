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
namespace pspsharp.network.pspsharp
{
	using GameModeArea = pspsharp.HLE.modules.sceNetAdhoc.GameModeArea;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class JpcspAdhocGameModeMessage : JpcspAdhocPdpMessage
	{
		public JpcspAdhocGameModeMessage(GameModeArea gameModeArea) : base()
		{
			Data = gameModeArea.NewData;
			gameModeArea.resetNewData();
			if (gameModeArea.macAddress != null)
			{
				ToMacAddress = gameModeArea.macAddress.macAddress;
			}
		}

		public JpcspAdhocGameModeMessage(sbyte[] message, int length) : base(message, length)
		{
		}
	}

}