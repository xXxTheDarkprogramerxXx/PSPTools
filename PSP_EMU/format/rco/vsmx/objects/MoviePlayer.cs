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
namespace pspsharp.format.rco.vsmx.objects
{
	using Logger = org.apache.log4j.Logger;

	using UmdVideoPlayer = pspsharp.GUI.UmdVideoPlayer;
	using VSMXArray = pspsharp.format.rco.vsmx.interpreter.VSMXArray;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXFunction = pspsharp.format.rco.vsmx.interpreter.VSMXFunction;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXNumber = pspsharp.format.rco.vsmx.interpreter.VSMXNumber;
	using VSMXObject = pspsharp.format.rco.vsmx.interpreter.VSMXObject;
	using VSMXString = pspsharp.format.rco.vsmx.interpreter.VSMXString;
	using VSMXUndefined = pspsharp.format.rco.vsmx.interpreter.VSMXUndefined;

	public class MoviePlayer : BaseNativeObject
	{
		// FWVGA display resolution as default (854x480)
		public const int DEFAULT_WIDTH = 854;
		public const int DEFAULT_HEIGHT = 480;
		private new static readonly Logger log = VSMX.log;
		public const string objectName = "movieplayer";
		private VSMXInterpreter interpreter;
		private UmdVideoPlayer umdVideoPlayer;
		private VSMXNativeObject controller;
		private bool playing = false;
		private bool menuMode;
		private int playListNumber;
		private int chapterNumber;
		private int videoNumber;
		private int audioNumber;
		private int audioFlag;
		private int subtitleNumber;
		private int subtitleFlag;
		private int width = DEFAULT_WIDTH;
		private int height = DEFAULT_HEIGHT;
		private int x;
		private int y;

		public static VSMXNativeObject create(VSMXInterpreter interpreter, UmdVideoPlayer umdVideoPlayer, VSMXNativeObject controller)
		{
			MoviePlayer moviePlayer = new MoviePlayer(interpreter, umdVideoPlayer, controller);
			VSMXNativeObject @object = new VSMXNativeObject(interpreter, moviePlayer);
			moviePlayer.Object = @object;

			@object.setPropertyValue("audioLanguageCode", new VSMXString(interpreter, "en"));
			@object.setPropertyValue("subtitleLanguageCode", new VSMXString(interpreter, "en"));

			return @object;
		}

		private MoviePlayer(VSMXInterpreter interpreter, UmdVideoPlayer umdVideoPlayer, VSMXNativeObject controller)
		{
			this.interpreter = interpreter;
			this.umdVideoPlayer = umdVideoPlayer;
			this.controller = controller;

			if (umdVideoPlayer != null)
			{
				umdVideoPlayer.MoviePlayer = this;
			}
		}

		public virtual void play(VSMXBaseObject @object, VSMXBaseObject pauseMode, VSMXBaseObject menuMode, VSMXBaseObject playListNumber, VSMXBaseObject chapterNumber, VSMXBaseObject videoNumber, VSMXBaseObject audioNumber, VSMXBaseObject audioFlag, VSMXBaseObject subtitleNumber, VSMXBaseObject subtitleFlag, VSMXBaseObject unknownBool)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("MoviePlayer.play pauseMode=%d, menuMode=%d, playListNumber=%d, chapterNumber=%d, videoNumber=0x%X, audioNumber=0x%X, audioFlag=0x%X, subtitleNumber=%d, subtitleFlag=0x%X, unknownBool=%b", pauseMode.getIntValue(), menuMode.getIntValue(), playListNumber.getIntValue(), chapterNumber.getIntValue(), videoNumber.getIntValue(), audioNumber.getIntValue(), audioFlag.getIntValue(), subtitleNumber.getIntValue(), subtitleFlag.getIntValue(), unknownBool.getBooleanValue()));
				log.debug(string.Format("MoviePlayer.play pauseMode=%d, menuMode=%d, playListNumber=%d, chapterNumber=%d, videoNumber=0x%X, audioNumber=0x%X, audioFlag=0x%X, subtitleNumber=%d, subtitleFlag=0x%X, unknownBool=%b", pauseMode.IntValue, menuMode.IntValue, playListNumber.IntValue, chapterNumber.IntValue, videoNumber.IntValue, audioNumber.IntValue, audioFlag.IntValue, subtitleNumber.IntValue, subtitleFlag.IntValue, unknownBool.BooleanValue));
			}
			playing = true;
			bool previousMenuMode = this.menuMode;
			this.menuMode = menuMode.BooleanValue;
			this.playListNumber = playListNumber.IntValue;
			this.chapterNumber = chapterNumber.IntValue;
			this.videoNumber = videoNumber.IntValue;
			this.audioNumber = audioNumber.IntValue;
			this.audioFlag = audioFlag.IntValue;
			this.subtitleNumber = subtitleNumber.IntValue;
			this.subtitleFlag = subtitleFlag.IntValue;

			if (umdVideoPlayer != null)
			{
				umdVideoPlayer.play(this.playListNumber, this.chapterNumber, this.videoNumber, this.audioNumber, this.audioFlag, this.subtitleNumber, this.subtitleFlag);
			}

			// Going to menu mode?
			if (!previousMenuMode && this.menuMode)
			{
				// Call the "controller.onMenu" callback
				VSMXBaseObject callback = controller.getPropertyValue("onMenu");
				if (callback is VSMXFunction)
				{
					VSMXBaseObject[] arguments = new VSMXBaseObject[0];
					interpreter.interpretFunction((VSMXFunction) callback, null, arguments);
				}
			}
		}

		public virtual void stop(VSMXBaseObject @object, VSMXBaseObject unknownInt, VSMXBaseObject unknownBool)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("MoviePlayer.stop unknownInt=%d, unknownBool=%b", unknownInt.getIntValue(), unknownBool.getBooleanValue()));
				log.debug(string.Format("MoviePlayer.stop unknownInt=%d, unknownBool=%b", unknownInt.IntValue, unknownBool.BooleanValue));
			}
			playing = false;
		}

		public virtual void resume(VSMXBaseObject @object)
		{
			playing = true;
		}

		public virtual VSMXBaseObject getResumeInfo(VSMXBaseObject @object)
		{
			VSMXBaseObject resumeInfo;
			if (playing)
			{
				resumeInfo = new VSMXObject(interpreter, "ResumeInfo");
				resumeInfo.setPropertyValue("playListNumber", new VSMXNumber(interpreter, playListNumber));
			}
			else
			{
				resumeInfo = VSMXUndefined.singleton;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("MoviePlayer.getResumeInfo() returning {0}", resumeInfo));
			}

			return resumeInfo;
		}

		public virtual void changeResumeInfo(VSMXBaseObject @object, VSMXBaseObject videoNumber, VSMXBaseObject audioNumber, VSMXBaseObject audioFlag, VSMXBaseObject subtitleNumber, VSMXBaseObject subtitleFlag)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("MoviePlayer.changeResumeInfo videoNumber=0x{0:X}, audioNumber=0x{1:X}, audioFlag=0x{2:X}, subtitleNumber={3:D}, subtitleFlag=0x{4:X}", videoNumber.IntValue, audioNumber.IntValue, audioFlag.IntValue, subtitleNumber.IntValue, subtitleFlag.IntValue));
			}
			this.videoNumber = videoNumber.IntValue;
			this.audioNumber = audioNumber.IntValue;
			this.audioFlag = audioFlag.IntValue;
			this.subtitleNumber = subtitleNumber.IntValue;
			this.subtitleFlag = subtitleFlag.IntValue;
		}

		public virtual VSMXBaseObject getPlayerStatus(VSMXBaseObject @object)
		{
			VSMXBaseObject playerStatus;
			if (playing)
			{
				playerStatus = new VSMXObject(interpreter, "PlayerStatus");
				playerStatus.setPropertyValue("playListNumber", new VSMXNumber(interpreter, playListNumber));
				playerStatus.setPropertyValue("chapterNumber", new VSMXNumber(interpreter, chapterNumber));
				playerStatus.setPropertyValue("videoNumber", new VSMXNumber(interpreter, videoNumber));
				playerStatus.setPropertyValue("audioNumber", new VSMXNumber(interpreter, audioNumber));
				playerStatus.setPropertyValue("audioFlag", new VSMXNumber(interpreter, audioFlag));
				playerStatus.setPropertyValue("subtitleNumber", new VSMXNumber(interpreter, subtitleNumber));
				playerStatus.setPropertyValue("subtitleFlag", new VSMXNumber(interpreter, subtitleFlag));
			}
			else
			{
				playerStatus = VSMXUndefined.singleton;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("MoviePlayer.getPlayerStatus() returning {0}", playerStatus));
			}

			return playerStatus;
		}

		public virtual void onPlayListEnd(int playListNumber)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("MoviePlayer.onPlayListEnd playListNumber={0:D}", playListNumber));
			}

			VSMXBaseObject callback = object.getPropertyValue("onPlayListEnd");
			if (callback is VSMXFunction)
			{
				VSMXBaseObject argument = new VSMXObject(interpreter, null);
				argument.setPropertyValue("playListNumber", new VSMXNumber(interpreter, playListNumber));

				VSMXBaseObject[] arguments = new VSMXBaseObject[1];
				arguments[0] = argument;
				interpreter.interpretFunction((VSMXFunction) callback, null, arguments);
			}
		}

		public virtual void onChapter(int chapterNumber)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("MoviePlayer.onChapter chapterNumber={0:D}", chapterNumber));
			}

			VSMXBaseObject callback = object.getPropertyValue("onChapter");
			if (callback is VSMXFunction)
			{
				VSMXBaseObject argument = new VSMXObject(interpreter, null);
				argument.setPropertyValue("chapterNumber", new VSMXNumber(interpreter, chapterNumber));

				VSMXBaseObject[] arguments = new VSMXBaseObject[1];
				arguments[0] = argument;
				interpreter.interpretFunction((VSMXFunction) callback, null, arguments);
			}
		}

		public virtual VSMXBaseObject getSize(VSMXBaseObject @object)
		{
			VSMXInterpreter interpreter = @object.Interpreter;
			VSMXArray size = new VSMXArray(interpreter, 2);
			size.setPropertyValue(0, new VSMXNumber(interpreter, width));
			size.setPropertyValue(1, new VSMXNumber(interpreter, height));

			return size;
		}

		public virtual void setSize(VSMXBaseObject @object, VSMXBaseObject width, VSMXBaseObject height)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("MoviePlayer.setSize({0}, {1})", width, height));
			}

			this.width = width.IntValue;
			this.height = height.IntValue;
		}

		public virtual VSMXBaseObject getPos(VSMXBaseObject @object)
		{
			VSMXInterpreter interpreter = @object.Interpreter;
			VSMXArray pos = new VSMXArray(interpreter, 2);
			pos.setPropertyValue(0, new VSMXNumber(interpreter, x));
			pos.setPropertyValue(1, new VSMXNumber(interpreter, y));

			return pos;
		}

		public virtual void setPos(VSMXBaseObject @object, VSMXBaseObject x, VSMXBaseObject y)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("MoviePlayer.setPos({0}, {1})", x, y));
			}

			this.x = x.IntValue;
			this.y = y.IntValue;
		}

		public virtual void onUp()
		{
			((Controller) controller.Object).onUp();
		}

		public virtual void onDown()
		{
			((Controller) controller.Object).onDown();
		}

		public virtual void onLeft()
		{
			((Controller) controller.Object).onLeft();
		}

		public virtual void onRight()
		{
			((Controller) controller.Object).onRight();
		}

		public virtual void onPush()
		{
			((Controller) controller.Object).onPush();
		}
	}

}