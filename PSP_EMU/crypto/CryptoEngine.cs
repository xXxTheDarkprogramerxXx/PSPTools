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
namespace pspsharp.crypto
{
	using Logger = org.apache.log4j.Logger;

	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;

	public class CryptoEngine
	{
		public static Logger log = Logger.getLogger("crypto");
		private const string name = "CryptEngine";
		private static bool isCryptoEngineInit;
		private static bool cryptoSavedata;
		private static bool extractEboot;
		private static bool extractSavedataKey;
		private static KIRK kirk;
		private static PRX prx;
		private static SAVEDATA sd;
		private static AMCTRL amctrl;
		private static PGD pgd;
		private static DRM drm;
		private static CryptSavedataSettingsListerner cryptSavedataSettingsListerner;
		private static ExtractEbootSettingsListerner extractEbootSettingsListerner;
		private static ExtractSavedataKeySettingsListerner extractSavedataKeySettingsListerner;

		private class CryptSavedataSettingsListerner : AbstractBoolSettingsListener
		{

			protected internal override void settingsValueChanged(bool value)
			{
				SavedataCryptoStatus = !value;
			}
		}

		private class ExtractEbootSettingsListerner : AbstractBoolSettingsListener
		{

			protected internal override void settingsValueChanged(bool value)
			{
				ExtractEbootStatus = value;
			}
		}

		private class ExtractSavedataKeySettingsListerner : AbstractBoolSettingsListener
		{

			protected internal override void settingsValueChanged(bool value)
			{
				ExtractSavedataKeyStatus = value;
			}
		}

		public CryptoEngine()
		{
			installSettingsListeners();
			CryptoEngineStatus = true;
			kirk = new KIRK();
			prx = new PRX();
			sd = new SAVEDATA();
			amctrl = new AMCTRL();
			pgd = new PGD();
			drm = new DRM();
		}

		public virtual KIRK KIRKEngine
		{
			get
			{
				return kirk;
			}
		}

		public virtual PRX PRXEngine
		{
			get
			{
				return prx;
			}
		}

		public virtual SAVEDATA SAVEDATAEngine
		{
			get
			{
				return sd;
			}
		}

		public virtual AMCTRL AMCTRLEngine
		{
			get
			{
				return amctrl;
			}
		}

		public virtual PGD PGDEngine
		{
			get
			{
				return pgd;
			}
		}

		public virtual DRM DRMEngine
		{
			get
			{
				return drm;
			}
		}

		private static void installSettingsListeners()
		{
			if (cryptSavedataSettingsListerner == null)
			{
				cryptSavedataSettingsListerner = new CryptSavedataSettingsListerner();
				Settings.Instance.registerSettingsListener(name, "emu.cryptoSavedata", cryptSavedataSettingsListerner);
			}
			if (extractEbootSettingsListerner == null)
			{
				extractEbootSettingsListerner = new ExtractEbootSettingsListerner();
				Settings.Instance.registerSettingsListener(name, "emu.extractEboot", extractEbootSettingsListerner);
			}
			if (extractSavedataKeySettingsListerner == null)
			{
				extractSavedataKeySettingsListerner = new ExtractSavedataKeySettingsListerner();
				Settings.Instance.registerSettingsListener(name, "emu.extractSavedataKey", extractSavedataKeySettingsListerner);
			}
		}

		/*
		 * Helper functions: used for status checking and parameter sorting.
		 */
		public static bool CryptoEngineStatus
		{
			get
			{
				return isCryptoEngineInit;
			}
			set
			{
				isCryptoEngineInit = value;
			}
		}


		public static bool ExtractEbootStatus
		{
			get
			{
				installSettingsListeners();
				return extractEboot;
			}
			set
			{
				extractEboot = value;
			}
		}


		public static bool ExtractSavedataKeyStatus
		{
			get
			{
				installSettingsListeners();
				return extractSavedataKey;
			}
			set
			{
				extractSavedataKey = value;
			}
		}


		public static bool SavedataCryptoStatus
		{
			get
			{
				installSettingsListeners();
				return cryptoSavedata;
			}
			set
			{
				cryptoSavedata = value;
			}
		}

	}
}