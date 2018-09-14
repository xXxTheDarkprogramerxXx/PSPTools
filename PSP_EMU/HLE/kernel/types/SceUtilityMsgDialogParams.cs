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
namespace pspsharp.HLE.kernel.types
{

	public class SceUtilityMsgDialogParams : pspUtilityBaseDialog
	{
		public int result;
		public int mode;
			public const int PSP_UTILITY_MSGDIALOG_MODE_ERROR = 0;
			public const int PSP_UTILITY_MSGDIALOG_MODE_TEXT = 1;
		public int errorValue;
		public string message; // 512 bytes
		public int options;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_ERROR = 0x00000000;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_NORMAL = 0x00000001;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_ALLOW_SOUND = 0x00000000;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_MUTE_SOUND = 0x00000002;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_NONE = 0x00000000;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_YESNO = 0x00000010;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_OK = 0x00000020;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_MASK = 0x00000030;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_ENABLE_CANCEL = 0x00000000;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_DISABLE_CANCEL = 0x00000080;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_MASK = 0x00000100;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_NONE = 0x00000000;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_YES = 0x00000000;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_NO = 0x00000100;
			public const int PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_OK = 0x00000000;
		public int buttonPressed;
			public const int PSP_UTILITY_BUTTON_PRESSED_INVALID = 0;
			public const int PSP_UTILITY_BUTTON_PRESSED_YES = 1;
			public const int PSP_UTILITY_BUTTON_PRESSED_OK = 1;
			public const int PSP_UTILITY_BUTTON_PRESSED_NO = 2;
			public const int PSP_UTILITY_BUTTON_PRESSED_ESC = 3;
		public string enterButtonString; // 64 bytes
		public string backButtonString; // 64 bytes

		public SceUtilityMsgDialogParams()
		{
			@base = new pspUtilityDialogCommon();
		}

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			result = read32();
			mode = read32();
			errorValue = read32();
			message = readStringNZ(512);
			options = read32();
			buttonPressed = read32();
			enterButtonString = readStringNZ(64);
			backButtonString = readStringNZ(64);
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			write32(result);
			write32(mode);
			write32(errorValue);
			writeStringNZ(512, message);
			write32(options);
			write32(buttonPressed);
			writeStringNZ(64, enterButtonString);
			writeStringNZ(64, backButtonString);
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public virtual bool OptionYesNoDefaultYes
		{
			get
			{
				if ((options & PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_YESNO) == PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_YESNO)
				{
					return (options & PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_MASK) == PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_YES;
				}
				return false;
			}
		}

		public virtual bool OptionYesNoDefaultNo
		{
			get
			{
				if ((options & PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_YESNO) == PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_YESNO)
				{
					return (options & PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_MASK) == PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_NO;
				}
				return false;
			}
		}

		public virtual bool OptionYesNo
		{
			get
			{
				return OptionYesNoDefaultYes || OptionYesNoDefaultNo;
			}
		}

		public virtual bool OptionOk
		{
			get
			{
				return ((options & PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_OK) == PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_OK);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("result " + string.Format("0x{0:X8}", result) + "\n");
			sb.Append("mode " + ((mode == PSP_UTILITY_MSGDIALOG_MODE_ERROR) ? "PSP_UTILITY_MSGDIALOG_MODE_ERROR" : (mode == PSP_UTILITY_MSGDIALOG_MODE_TEXT) ? "PSP_UTILITY_MSGDIALOG_MODE_TEXT" : string.Format("0x{0:X8}", mode)) + "\n");
			sb.Append("errorValue " + string.Format("0x{0:X8}", errorValue) + "\n");
			sb.Append("message '" + message + "'\n");
			sb.Append("options " + string.Format("0x{0:X8}", options) + "\n");
			if (OptionYesNoDefaultYes)
			{
				sb.Append("options PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_YES\n");
			}
			if (OptionYesNoDefaultNo)
			{
				sb.Append("options PSP_UTILITY_MSGDIALOG_OPTION_YESNO_DEFAULT_NO\n");
			}
			sb.Append("buttonPressed " + string.Format("0x{0:X8}'\n", buttonPressed));
			sb.Append("enterButtonString '" + enterButtonString + "'\n");
			sb.Append("backButtonString '" + backButtonString + "'");

			return sb.ToString();
		}
	}
}