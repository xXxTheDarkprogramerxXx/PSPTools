using System;
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
namespace pspsharp.HLE.modules
{

	using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using FontRegistryEntry = pspsharp.HLE.modules.sceFont.FontRegistryEntry;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	public class sceReg : HLEModule
	{
		public static Logger log = Modules.getLogger("sceReg");
		protected internal const int REG_TYPE_DIR = 1;
		protected internal const int REG_TYPE_INT = 2;
		protected internal const int REG_TYPE_STR = 3;
		protected internal const int REG_TYPE_BIN = 4;
		protected internal const int REG_MODE_READ_WRITE = 1;
		protected internal const int REG_MODE_READ_ONLY = 2;
		private IDictionary<int, RegistryHandle> registryHandles;
		private IDictionary<int, CategoryHandle> categoryHandles;
		private IDictionary<int, KeyHandle> keyHandles;
		private string authName;
		private string authKey;
		private int networkLatestId;
		private int wifiConnectCount;
		private int usbConnectCount;
		private int psnAccountCount;
		private int slideCount;
		private int bootCount;
		private int gameExecCount;
		private int oskVersionId;
		private int oskDispLocale;
		private int oskWritingLocale;
		private int oskInputCharMask;
		private int oskKeytopIndex;
		private string npEnv;
		private string adhocSsidPrefix;
		private int musicVisualizerMode;
		private int musicTrackInfoMode;
		private string lockPassword;
		private string browserHomeUri;
		private string npAccountId;
		private string npLoginId;
		private string npPassword;
		private int npAutoSignInEnable;
		private string ownerName;

		protected internal class RegistryHandle
		{
			internal const string registryHandlePurpose = "sceReg.RegistryHandle";
			public int uid;
			public int type;
			public string name;
			public int unknown1;
			public int unknown2;

			public RegistryHandle(int type, string name, int unknown1, int unknown2)
			{
				this.type = type;
				this.name = name;
				this.unknown1 = unknown1;
				this.unknown2 = unknown2;
				uid = SceUidManager.getNewUid(registryHandlePurpose);
			}

			public virtual void release()
			{
				SceUidManager.releaseUid(uid, registryHandlePurpose);
				uid = -1;
			}
		}

		protected internal class CategoryHandle
		{
			internal const string categoryHandlePurpose = "sceReg.CategoryHandle";
			public int uid;
			public RegistryHandle registryHandle;
			public string name;
			public int mode;

			public CategoryHandle(RegistryHandle registryHandle, string name, int mode)
			{
				this.registryHandle = registryHandle;
				this.name = name;
				this.mode = mode;
				uid = SceUidManager.getNewUid(categoryHandlePurpose);
			}

			public virtual string FullName
			{
				get
				{
					return registryHandle.name + name;
				}
			}

			public virtual void release()
			{
				SceUidManager.releaseUid(uid, categoryHandlePurpose);
				uid = -1;
			}
		}

		protected internal class KeyHandle
		{
			internal static int index = 0;
			public int uid;
			public string name;

			public KeyHandle(string name)
			{
				this.name = name;
				uid = index++;
			}
		}

		public virtual string AuthName
		{
			get
			{
				return authName;
			}
			set
			{
				this.authName = value;
			}
		}


		public virtual string AuthKey
		{
			get
			{
				return authKey;
			}
			set
			{
				this.authKey = value;
			}
		}


		public virtual int NetworkLatestId
		{
			get
			{
				return networkLatestId;
			}
			set
			{
				this.networkLatestId = value;
			}
		}


		public virtual string NpLoginId
		{
			get
			{
				return npLoginId;
			}
		}

		public virtual string NpPassword
		{
			get
			{
				return npPassword;
			}
		}

		private int getKey(CategoryHandle categoryHandle, string name, TPointer32 ptype, TPointer32 psize, TPointer buf, int size)
		{
			string fullName = categoryHandle.FullName;
			fullName = fullName.Replace("flash1:/registry/system", "");
			fullName = fullName.Replace("flash1/registry/system", "");
			fullName = fullName.Replace("flash2/registry/system", "");

			Settings settings = Settings.Instance;
			if ("/system/DATA/FONT".Equals(fullName) || "/DATA/FONT".Equals(fullName))
			{
				IList<sceFont.FontRegistryEntry> fontRegistry = Modules.sceFontModule.FontRegistry;
				if ("path_name".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(Modules.sceFontModule.FontDirPath.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, Modules.sceFontModule.FontDirPath);
					}
				}
				else if ("num_fonts".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(fontRegistry.Count);
					}
				}
				else
				{
					log.warn(string.Format("Unknown font registry entry '{0}'", name));
				}
			}
			else if (fullName.StartsWith("/system/DATA/FONT/PROPERTY/INFO", StringComparison.Ordinal) || fullName.StartsWith("/DATA/FONT/PROPERTY/INFO", StringComparison.Ordinal))
			{
				IList<sceFont.FontRegistryEntry> fontRegistry = Modules.sceFontModule.FontRegistry;
				int index = int.Parse(fullName.Substring(fullName.IndexOf("INFO", StringComparison.Ordinal) + 4));
				if (index < 0 || index >= fontRegistry.Count)
				{
					return -1;
				}
				FontRegistryEntry entry = fontRegistry[index];
				if ("h_size".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.h_size);
					}
				}
				else if ("v_size".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.v_size);
					}
				}
				else if ("h_resolution".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.h_resolution);
					}
				}
				else if ("v_resolution".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.v_resolution);
					}
				}
				else if ("extra_attributes".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.extra_attributes);
					}
				}
				else if ("weight".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.weight);
					}
				}
				else if ("family_code".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.family_code);
					}
				}
				else if ("style".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.style);
					}
				}
				else if ("sub_style".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.sub_style);
					}
				}
				else if ("language_code".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.language_code);
					}
				}
				else if ("region_code".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.region_code);
					}
				}
				else if ("country_code".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.country_code);
					}
				}
				else if ("file_name".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(string.ReferenceEquals(entry.file_name, null) ? 0 : entry.file_name.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, entry.file_name);
					}
				}
				else if ("font_name".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(string.ReferenceEquals(entry.font_name, null) ? 0 : entry.font_name.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, entry.font_name);
					}
				}
				else if ("expire_date".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.expire_date);
					}
				}
				else if ("shadow_option".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(entry.shadow_option);
					}
				}
				else
				{
					log.warn(string.Format("Unknown font registry entry '{0}'", name));
				}
			}
			else if ("/CONFIG/DATE".Equals(fullName))
			{
				if ("date_format".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.date_format", 2));
					}
				}
				else if ("time_format".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.time_format", 0));
					}
				}
				else if ("time_zone_offset".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.time_zone_offset", 0));
					}
				}
				else if ("time_zone_area".Equals(name))
				{
					string timeZoneArea = settings.readString("registry.time_zone_area", "united_kingdom");
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(timeZoneArea.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, timeZoneArea);
					}
				}
				else if ("summer_time".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.summer_time", 0));
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/BROWSER".Equals(fullName))
			{
				if ("flash_activated".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("home_uri".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(0x200);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, browserHomeUri);
					}
				}
				else if ("cookie_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("proxy_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(2);
					}
				}
				else if ("proxy_address".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(0x80);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, "");
					}
				}
				else if ("proxy_port".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("picture".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("animation".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("javascript".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("cache_size".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0x200); // Cache Size in KB
					}
				}
				else if ("char_size".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("disp_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("flash_play".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("connect_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("proxy_protect".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("proxy_autoauth".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("proxy_user".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(0x80);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, "");
					}
				}
				else if ("proxy_password".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(0x80);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, "");
					}
				}
				else if ("webpage_quality".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/BROWSER2".Equals(fullName))
			{
				if ("tm_service".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("tm_ec_ttl".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("tm_ec_ttl_update_time".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/NP".Equals(fullName))
			{
				if ("account_id".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(16);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, npAccountId);
					}
				}
				else if ("login_id".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(npLoginId.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, npLoginId);
					}
				}
				else if ("password".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(npPassword.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, npPassword);
					}
				}
				else if ("auto_sign_in_enable".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(npAutoSignInEnable);
					}
				}
				else if ("nav_only".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("env".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(npEnv.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, npEnv);
					}
				}
				else if ("guest_country".Equals(name))
				{
					string guestCount = "";
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(guestCount.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, guestCount);
					}
				}
				else if ("view_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("check_drm".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/PREMO".Equals(fullName))
			{
				if ("ps3_mac".Equals(name))
				{
					sbyte[] ps3Mac = new sbyte[6];
					ps3Mac[0] = 0x11;
					ps3Mac[1] = 0x22;
					ps3Mac[2] = 0x33;
					ps3Mac[3] = 0x44;
					ps3Mac[4] = 0x55;
					ps3Mac[5] = 0x66;
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(ps3Mac.Length);
					if (size > 0)
					{
						buf.setArray(ps3Mac, ps3Mac.Length);
					}
				}
				else if ("ps3_name".Equals(name))
				{
					string ps3Name = "My PS3";
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(ps3Name.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, ps3Name);
					}
				}
				else if ("guide_page".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("response".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("custom_video_buffer1".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("custom_video_bitrate1".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("setting_internet".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("custom_video_buffer2".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("custom_video_bitrate2".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("button_assign".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("ps3_keytype".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("ps3_key".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(16);
					if (size >= 16)
					{
						buf.clear(16);
					}
				}
				else if ("flags".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("account_id".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(16);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, npAccountId);
					}
				}
				else if ("login_id".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(npLoginId.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, npLoginId);
					}
				}
				else if ("password".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(npPassword.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, npPassword);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM".Equals(fullName))
			{
				if ("exh_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("umd_autoboot".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("usb_auto_connect".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("owner_mob".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("owner_dob".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("umd_cache".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("owner_name".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(ownerName.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, ownerName);
					}
				}
				else if ("slide_welcome".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("first_boot_tick".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					string firstBootTick = "";
					psize.setValue(firstBootTick.Length);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, firstBootTick);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM/SOUND".Equals(fullName))
			{
				if ("dynamic_normalizer".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("operation_sound_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("avls".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM/CHARACTER_SET".Equals(fullName))
			{
				if ("oem".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(5);
					}
				}
				else if ("ansi".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0x13);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM/XMB".Equals(fullName))
			{
				if ("language".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(sceUtility.SystemParamLanguage);
					}
				}
				else if ("button_assign".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(sceUtility.SystemParamButtonPreference);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM/XMB/THEME".Equals(fullName))
			{
				if ("wallpaper_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.theme.wallpaper_mode", 0));
					}
				}
				else if ("custom_theme_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.theme.custom_theme_mode", 0));
					}
				}
				else if ("color_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.theme.color_mode", 0));
					}
				}
				else if ("system_color".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(settings.readInt("registry.theme.system_color", 0));
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/SYSPROFILE/RESOLUTION".Equals(fullName))
			{
				if ("horizontal".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(8210);
					}
				}
				else if ("vertical".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(8210);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/ALARM".Equals(fullName))
			{
				if (name.matches("alarm_\\d+_time"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(-1);
					}
				}
				else if (name.matches("alarm_\\d+_property"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/NETWORK/GO_MESSENGER".Equals(fullName))
			{
				if (name.Equals("auth_name"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authName.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authName);
					}
				}
				else if (name.Equals("auth_key"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authKey.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authKey);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/NETWORK/ADHOC".Equals(fullName))
			{
				if (name.Equals("ssid_prefix"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(adhocSsidPrefix.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, adhocSsidPrefix);
					}
				}
				else if (name.Equals("channel"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(sceUtility.SystemParamAdhocChannel);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/NETWORK/INFRASTRUCTURE".Equals(fullName))
			{
				if ("latest_id".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(networkLatestId);
					}
				}
				else if (name.Equals("eap_md5"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if (name.Equals("auto_setting"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if (name.Equals("wifisvc_setting"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if (fullName.matches("/CONFIG/NETWORK/INFRASTRUCTURE/\\d+"))
			{
				string indexName = fullName.Replace("/CONFIG/NETWORK/INFRASTRUCTURE/", "");
				int index = int.Parse(indexName);
				if ("cnf_name".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string cnfName = sceUtility.getNetParamName(index);
					psize.setValue(cnfName.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, cnfName);
					}
				}
				else if ("ssid".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string ssid = sceNetApctl.SSID;
					psize.setValue(ssid.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, ssid);
					}
				}
				else if ("auth_proto".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						// 0 is no security.
						// 1 is WEP (64bit).
						// 2 is WEP (128bit).
						// 3 is WPA.
						buf.setValue32(1);
					}
				}
				else if ("wep_key".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					string wepKey = "XXXXXXXXXXXXX"; // Max length is 13
					psize.setValue(wepKey.Length);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, wepKey);
					}
				}
				else if ("how_to_set_ip".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						// 0 is DHCP.
						// 1 is static.
						// 2 is PPPOE.
						buf.setValue32(0);
					}
				}
				else if ("dns_flag".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						// 0 is auto.
						// 1 is manual.
						buf.setValue32(0);
					}
				}
				else if ("primary_dns".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string dns = sceNetApctl.PrimaryDNS;
					psize.setValue(dns.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, dns);
					}
				}
				else if ("secondary_dns".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string dns = sceNetApctl.SecondaryDNS;
					psize.setValue(dns.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, dns);
					}
				}
				else if ("http_proxy_flag".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						// 0 is to not use proxy.
						// 1 is to use proxy.
						buf.setValue32(0);
					}
				}
				else if ("http_proxy_server".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string httpProxyServer = "";
					psize.setValue(httpProxyServer.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, httpProxyServer);
					}
				}
				else if ("http_proxy_port".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(80);
					}
				}
				else if ("version".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						// 0 is not used.
						// 1 is old version.
						// 2 is new version.
						buf.setValue32(2);
					}
				}
				else if ("auth_8021x_type".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						// 0 is none.
						// 1 is EAP (MD5).
						buf.setValue32(0);
					}
				}
				else if ("browser_flag".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						// 0 is to not start the native browser.
						// 1 is to start the native browser.
						buf.setValue32(0);
					}
				}
				else if ("ip_address".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string ip = sceNetApctl.LocalHostIP;
					psize.setValue(ip.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, ip);
					}
				}
				else if ("netmask".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string netmask = sceNetApctl.SubnetMask;
					psize.setValue(netmask.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, netmask);
					}
				}
				else if ("default_route".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string gateway = sceNetApctl.Gateway;
					psize.setValue(gateway.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, gateway);
					}
				}
				else if (name.Equals("device"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if (name.Equals("auth_name"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authName.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authName);
					}
				}
				else if (name.Equals("auth_key"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authKey.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authKey);
					}
				}
				else if (name.Equals("auth_8021x_auth_name"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authName.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authName);
					}
				}
				else if (name.Equals("auth_8021x_auth_key"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authKey.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authKey);
					}
				}
				else if ("wpa_key_type".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if (name.Equals("wpa_key"))
				{
					ptype.setValue(REG_TYPE_BIN);
					string wpaKey = "";
					psize.setValue(wpaKey.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, wpaKey);
					}
				}
				else if ("wifisvc_config".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if (fullName.matches("/CONFIG/NETWORK/INFRASTRUCTURE/\\d+/SUB1"))
			{
				string indexName = fullName.Replace("/CONFIG/NETWORK/INFRASTRUCTURE/", "");
				int index = int.Parse(indexName.Substring(0, indexName.IndexOf("/", StringComparison.Ordinal)));
				if (log.DebugEnabled)
				{
					log.debug(string.Format("/CONFIG/NETWORK/INFRASTRUCTURE, index={0:D}, SUB1", index));
				}
				if ("last_leased_dhcp_addr".Equals(name))
				{
					ptype.setValue(REG_TYPE_STR);
					string lastLeasedDhcpAddr = "";
					psize.setValue(lastLeasedDhcpAddr.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, lastLeasedDhcpAddr);
					}
				}
				else if (name.Equals("wifisvc_auth_name"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authName.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authName);
					}
				}
				else if (name.Equals("wifisvc_auth_key"))
				{
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(authKey.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, authKey);
					}
				}
				else if (name.Equals("wifisvc_option"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if (name.Equals("bt_id"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if (name.Equals("at_command"))
				{
					ptype.setValue(REG_TYPE_STR);
					string atCommand = "";
					psize.setValue(atCommand.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, atCommand);
					}
				}
				else if (name.Equals("phone_number"))
				{
					ptype.setValue(REG_TYPE_STR);
					string phoneNumber = "";
					psize.setValue(phoneNumber.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, phoneNumber);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/DATA/COUNT".Equals(fullName))
			{
				if ("wifi_connect_count".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(wifiConnectCount);
					}
				}
				else if (name.Equals("usb_connect_count"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(usbConnectCount);
					}
				}
				else if (name.Equals("psn_access_count"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(psnAccountCount);
					}
				}
				else if (name.Equals("slide_count"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(slideCount);
					}
				}
				else if (name.Equals("boot_count"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(bootCount);
					}
				}
				else if (name.Equals("game_exec_count"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(gameExecCount);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM/LOCK".Equals(fullName))
			{
				if ("parental_level".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("browser_start".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("password".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(lockPassword.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, lockPassword);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM/POWER_SAVING".Equals(fullName))
			{
				if ("backlight_off_interval".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("suspend_interval".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("wlan_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("active_backlight_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/TOOL/CONFIG".Equals(fullName))
			{
				if ("np_debug".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/REGISTRY".Equals(fullName))
			{
				if ("category_version".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0x66);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/OSK".Equals(fullName))
			{
				if ("version_id".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(oskVersionId);
					}
				}
				else if (name.Equals("disp_locale"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(oskDispLocale);
					}
				}
				else if (name.Equals("writing_locale"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(oskWritingLocale);
					}
				}
				else if (name.Equals("input_char_mask"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(oskInputCharMask);
					}
				}
				else if (name.Equals("keytop_index"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(oskKeytopIndex);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/MUSIC".Equals(fullName))
			{
				if ("visualizer_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(musicVisualizerMode);
					}
				}
				else if (name.Equals("track_info_mode"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(musicTrackInfoMode);
					}
				}
				else if (name.Equals("wma_play"))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/PHOTO".Equals(fullName))
			{
				if ("slideshow_speed".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/VIDEO".Equals(fullName))
			{
				if ("lr_button_enable".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("list_play_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("title_display_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("sound_language".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(2);
					if (size >= 2)
					{
						buf.setValue8(0, (sbyte) '0');
						buf.setValue8(1, (sbyte) '0');
					}
				}
				else if ("subtitle_language".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(2);
					if (size >= 2)
					{
						buf.setValue8(0, (sbyte) 'e');
						buf.setValue8(1, (sbyte) 'n');
					}
				}
				else if ("menu_language".Equals(name))
				{
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(2);
					if (size >= 2)
					{
						buf.setValue8(0, (sbyte) 'e');
						buf.setValue8(1, (sbyte) 'n');
					}
				}
				else if ("appended_volume".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/INFOBOARD".Equals(fullName))
			{
				if ("locale_lang".Equals(name))
				{
					string localeLang = "en/en/rss.xml";
					ptype.setValue(REG_TYPE_STR);
					psize.setValue(localeLang.Length + 1);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, localeLang);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/CAMERA".Equals(fullName))
			{
				if ("still_size".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("still_quality".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("movie_size".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("movie_quality".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("white_balance".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("exposure_bias".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("still_effect".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("file_folder".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0x65);
					}
				}
				else if ("file_number".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("movie_fps".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("shutter_sound_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("file_number_eflash".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(1);
					}
				}
				else if ("folder_number_eflash".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0x65);
					}
				}
				else if ("msid".Equals(name))
				{
					string msid = "";
					ptype.setValue(REG_TYPE_BIN);
					psize.setValue(16);
					if (size > 0)
					{
						Utilities.writeStringNZ(buf.Memory, buf.Address, size, msid);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/RSS".Equals(fullName))
			{
				if ("download_items".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(5);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/DISPLAY".Equals(fullName))
			{
				if ("color_space_mode".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else if ("screensaver_start_time".Equals(name))
				{
					ptype.setValue(REG_TYPE_INT);
					psize.setValue(4);
					if (size >= 4)
					{
						buf.setValue32(0);
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else
			{
				log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
			}

			return 0;
		}

		public override void start()
		{
			registryHandles = new Dictionary<int, sceReg.RegistryHandle>();
			categoryHandles = new Dictionary<int, sceReg.CategoryHandle>();
			keyHandles = new Dictionary<int, sceReg.KeyHandle>();

			// TODO Read these values from the configuration file
			Settings settings = Settings.Instance;
			authName = "";
			authKey = "";
			networkLatestId = 0;
			wifiConnectCount = 0;
			usbConnectCount = 0;
			gameExecCount = 0;
			oskVersionId = 0x226;
			oskDispLocale = 0x1;
			oskWritingLocale = 0x1;
			oskInputCharMask = 0xF;
			oskKeytopIndex = 0x5;
			npEnv = "np"; // Max length 8
			adhocSsidPrefix = "PSP"; // Must be of length 3
			musicVisualizerMode = 0;
			musicTrackInfoMode = 1;
			lockPassword = "0000"; // 4-digit password
			browserHomeUri = "";
			npAccountId = settings.readString("registry.npAccountId");
			npLoginId = settings.readString("registry.npLoginId");
			npPassword = settings.readString("registry.npPassword");
			npAutoSignInEnable = settings.readInt("registry.npAutoSignInEnable");
			ownerName = sceUtility.SystemParamNickname;

			base.start();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x92E41280, version = 150) public int sceRegOpenRegistry(pspsharp.HLE.TPointer reg, int mode, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 h)
		[HLEFunction(nid : 0x92E41280, version : 150)]
		public virtual int sceRegOpenRegistry(TPointer reg, int mode, TPointer32 h)
		{
			int regType = reg.getValue32(0);
			int nameLen = reg.getValue32(260);
			int unknown1 = reg.getValue32(264);
			int unknown2 = reg.getValue32(268);
			string name = Utilities.readStringNZ(reg.Address + 4, nameLen);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("RegParam: regType={0:D}, name='{1}'(len={2:D}), unknown1={3:D}, unknown2={4:D}", regType, name, nameLen, unknown1, unknown2));
			}

			RegistryHandle registryHandle = new RegistryHandle(regType, name, unknown1, unknown2);
			registryHandles[registryHandle.uid] = registryHandle;

			h.setValue(registryHandle.uid);

			return 0;
		}

		[HLEFunction(nid : 0xFA8A5739, version : 150)]
		public virtual int sceRegCloseRegistry(int h)
		{
			RegistryHandle registryHandle = registryHandles[h];
			if (registryHandle == null)
			{
				return -1;
			}

			registryHandle.release();
			registryHandles.Remove(h);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDEDA92BF, version = 150) public int sceRegRemoveRegistry(pspsharp.HLE.TPointer reg)
		[HLEFunction(nid : 0xDEDA92BF, version : 150)]
		public virtual int sceRegRemoveRegistry(TPointer reg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1D8A762E, version = 150) public int sceRegOpenCategory(int h, String name, int mode, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 hd)
		[HLEFunction(nid : 0x1D8A762E, version : 150)]
		public virtual int sceRegOpenCategory(int h, string name, int mode, TPointer32 hd)
		{
			RegistryHandle registryHandle = registryHandles[h];
			if (registryHandle == null)
			{
				return -1;
			}
			CategoryHandle categoryHandle = new CategoryHandle(registryHandle, name, mode);
			categoryHandles[categoryHandle.uid] = categoryHandle;
			hd.setValue(categoryHandle.uid);

			if (categoryHandle.FullName.StartsWith("/system/DATA/FONT/PROPERTY/INFO", StringComparison.Ordinal))
			{
				IList<sceFont.FontRegistryEntry> fontRegistry = Modules.sceFontModule.FontRegistry;
				int index = int.Parse(categoryHandle.FullName.Substring(31));
				if (index < 0 || index >= fontRegistry.Count)
				{
					if (mode != REG_MODE_READ_WRITE)
					{
						return SceKernelErrors.ERROR_REGISTRY_NOT_FOUND;
					}
				}
			}
			else if (categoryHandle.FullName.StartsWith("flash2/registry/system/CONFIG/NETWORK/INFRASTRUCTURE/", StringComparison.Ordinal))
			{
				string indexString = categoryHandle.FullName.Substring(53);
				int sep = indexString.IndexOf('/');
				if (sep >= 0)
				{
					indexString = indexString.Substring(0, sep);
				}
				int index = int.Parse(indexString);
				// We do not return too many entries as some homebrew only support a limited number of entries.
				if (index > sceUtility.PSP_NETPARAM_MAX_NUMBER_DUMMY_ENTRIES)
				{
					return SceKernelErrors.ERROR_REGISTRY_NOT_FOUND;
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0x0CAE832B, version : 150)]
		public virtual int sceRegCloseCategory(int hd)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}

			categoryHandle.release();
			categoryHandles.Remove(hd);

			return 0;
		}

		[HLEFunction(nid : 0x39461B4D, version : 150)]
		public virtual int sceRegFlushRegistry(int h)
		{
			RegistryHandle registryHandle = registryHandles[h];
			if (registryHandle == null)
			{
				return -1;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0D69BF40, version = 150) public int sceRegFlushCategory(int hd)
		[HLEFunction(nid : 0x0D69BF40, version : 150)]
		public virtual int sceRegFlushCategory(int hd)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}
			return 0;
		}

		[HLEFunction(nid : 0x57641A81, version : 150)]
		public virtual int sceRegCreateKey(int hd, string name, int type, int size)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}

			if (categoryHandle.FullName.StartsWith("/system/DATA/FONT/PROPERTY/INFO", StringComparison.Ordinal))
			{
				IList<sceFont.FontRegistryEntry> fontRegistry = Modules.sceFontModule.FontRegistry;
				int index = int.Parse(categoryHandle.FullName.Substring(31));
				if (index < 0 || index > fontRegistry.Count)
				{
					return -1;
				}
				else if (index == fontRegistry.Count)
				{
					log.info(string.Format("sceRegCreateKey creating a new font entry '{0}'", categoryHandle.FullName));
					FontRegistryEntry entry = new FontRegistryEntry();
					fontRegistry.Add(entry);
					if ("h_size".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("v_size".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("h_resolution".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("v_resolution".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("extra_attributes".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("weight".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("family_code".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("style".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("sub_style".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("language_code".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("region_code".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("country_code".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("file_name".Equals(name) && size >= 0 && type == REG_TYPE_STR)
					{
						// OK
					}
					else if ("font_name".Equals(name) && size >= 0 && type == REG_TYPE_STR)
					{
						// OK
					}
					else if ("expire_date".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else if ("shadow_option".Equals(name) && size >= 4 && type == REG_TYPE_INT)
					{
						// OK
					}
					else
					{
						log.warn(string.Format("Unknown font registry entry '{0}' size=0x{1:X}, type={2:D}", name, size, type));
					}
				}
			}
			else
			{
				log.warn(string.Format("Unknown registry entry '{0}/{1}'", categoryHandle.FullName, name));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x17768E14, version = 150) public int sceRegSetKeyValue(int hd, String name, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buf, int size)
		[HLEFunction(nid : 0x17768E14, version : 150)]
		public virtual int sceRegSetKeyValue(int hd, string name, TPointer buf, int size)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}
			if (log.DebugEnabled)
			{
				log.debug(string.Format("buf: {0}", Utilities.getMemoryDump(buf.Address, size)));
			}

			string fullName = categoryHandle.FullName;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceRegSetKeyValue fullName='{0}/{1}'", fullName, name));
			}
			fullName = fullName.Replace("flash1:/registry/system", "");
			fullName = fullName.Replace("flash1/registry/system", "");
			fullName = fullName.Replace("flash2/registry/system", "");

			Settings settings = Settings.Instance;
			if ("/system/DATA/FONT".Equals(fullName))
			{
				if ("path_name".Equals(name))
				{
					string fontDirPath = buf.getStringNZ(size);
					if (log.InfoEnabled)
					{
						log.info(string.Format("Setting font dir path to '{0}'", fontDirPath));
					}
					Modules.sceFontModule.FontDirPath = fontDirPath;
				}
				else if ("num_fonts".Equals(name) && size >= 4)
				{
					IList<sceFont.FontRegistryEntry> fontRegistry = Modules.sceFontModule.FontRegistry;
					int numFonts = buf.getValue32();
					if (numFonts != fontRegistry.Count)
					{
						if (log.InfoEnabled)
						{
							log.info(string.Format("Changing the number of fonts from {0:D} to {1:D}", fontRegistry.Count, numFonts));
						}
					}
				}
				else
				{
					log.warn(string.Format("Unknown font registry entry '{0}'", name));
				}
			}
			else if (fullName.StartsWith("/system/DATA/FONT/PROPERTY/INFO", StringComparison.Ordinal))
			{
				IList<sceFont.FontRegistryEntry> fontRegistry = Modules.sceFontModule.FontRegistry;
				int index = int.Parse(fullName.Substring(31));
				if (index < 0 || index >= fontRegistry.Count)
				{
					return -1;
				}
				FontRegistryEntry entry = fontRegistry[index];
				if ("h_size".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("v_size".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("h_resolution".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("v_resolution".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("extra_attributes".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("weight".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("family_code".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("style".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("sub_style".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("language_code".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("region_code".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("country_code".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("file_name".Equals(name))
				{
					entry.file_name = buf.getStringNZ(size);
				}
				else if ("font_name".Equals(name))
				{
					entry.font_name = buf.getStringNZ(size);
				}
				else if ("expire_date".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else if ("shadow_option".Equals(name) && size >= 4)
				{
					entry.h_size = buf.getValue32();
				}
				else
				{
					log.warn(string.Format("Unknown font registry entry '{0}'", name));
				}
			}
			else if ("/DATA/COUNT".Equals(fullName))
			{
				if ("wifi_connect_count".Equals(name) && size >= 4)
				{
					wifiConnectCount = buf.getValue32();
				}
				else if ("usb_connect_count".Equals(name) && size >= 4)
				{
					usbConnectCount = buf.getValue32();
				}
				else if ("psn_access_count".Equals(name) && size >= 4)
				{
					psnAccountCount = buf.getValue32();
				}
				else if ("slide_count".Equals(name) && size >= 4)
				{
					slideCount = buf.getValue32();
				}
				else if ("boot_count".Equals(name) && size >= 4)
				{
					bootCount = buf.getValue32();
				}
				else if ("game_exec_count".Equals(name) && size >= 4)
				{
					gameExecCount = buf.getValue32();
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}'", name));
				}
			}
			else if ("/CONFIG/OSK".Equals(fullName))
			{
				if ("version_id".Equals(name) && size >= 4)
				{
					oskVersionId = buf.getValue32();
				}
				else if (name.Equals("disp_locale") && size >= 4)
				{
					oskDispLocale = buf.getValue32();
				}
				else if (name.Equals("writing_locale") && size >= 4)
				{
					oskWritingLocale = buf.getValue32();
				}
				else if (name.Equals("input_char_mask") && size >= 4)
				{
					oskInputCharMask = buf.getValue32();
				}
				else if (name.Equals("keytop_index") && size >= 4)
				{
					oskKeytopIndex = buf.getValue32();
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}'", name));
				}
			}
			else if ("/CONFIG/NP".Equals(fullName))
			{
				if ("env".Equals(name))
				{
					npEnv = buf.getStringNZ(size);
				}
				else if ("account_id".Equals(name))
				{
					npAccountId = buf.getStringNZ(size);
					settings.writeString("registry.npAccountId", npAccountId);
				}
				else if ("login_id".Equals(name))
				{
					npLoginId = buf.getStringNZ(size);
					settings.writeString("registry.npLoginId", npLoginId);
				}
				else if ("password".Equals(name))
				{
					npPassword = buf.getStringNZ(size);
					settings.writeString("registry.npPassword", npPassword);
				}
				else if ("auto_sign_in_enable".Equals(name) && size >= 4)
				{
					npAutoSignInEnable = buf.getValue32();
					settings.writeInt("registry.npAutoSignInEnable", npAutoSignInEnable);
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}'", name));
				}
			}
			else if ("/CONFIG/NETWORK/INFRASTRUCTURE".Equals(fullName))
			{
				if ("latest_id".Equals(name) && size >= 4)
				{
					networkLatestId = buf.getValue32();
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if (fullName.matches("/CONFIG/NETWORK/INFRASTRUCTURE/\\d+"))
			{
				string indexName = fullName.Replace("/CONFIG/NETWORK/INFRASTRUCTURE/", "");
				int index = int.Parse(indexName);
				if ("cnf_name".Equals(name))
				{
					string cnfName = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set cnf_name#{0:D}='{1}'", index, cnfName));
					}
				}
				else if ("ssid".Equals(name))
				{
					string ssid = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set ssid#{0:D}='{1}'", index, ssid));
					}
				}
				else if ("auth_proto".Equals(name) && size >= 4)
				{
					int authProto = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set auth_proto#{0:D}='{1}'", index, authProto));
					}
				}
				else if ("wep_key".Equals(name))
				{
					string wepKey = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set wep_key#{0:D}='{1}'", index, wepKey));
					}
				}
				else if ("how_to_set_ip".Equals(name) && size >= 4)
				{
					int howToSetIp = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set how_to_set_ip#{0:D}={1:D}", index, howToSetIp));
					}
				}
				else if ("ip_address".Equals(name))
				{
					string ipAddress = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set ip_address#{0:D}='{1}'", index, ipAddress));
					}
				}
				else if ("netmask".Equals(name))
				{
					string netmask = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set netmask#{0:D}='{1}'", index, netmask));
					}
				}
				else if ("default_route".Equals(name))
				{
					string defaultRoute = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set default_route#{0:D}='{1}'", index, defaultRoute));
					}
				}
				else if ("dns_flag".Equals(name) && size >= 4)
				{
					int dnsFlag = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set dns_flag#{0:D}={1:D}", index, dnsFlag));
					}
				}
				else if ("primary_dns".Equals(name))
				{
					string primaryDns = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set primary_dns#{0:D}='{1}'", index, primaryDns));
					}
				}
				else if ("secondary_dns".Equals(name))
				{
					string secondaryDns = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set secondary_dns#{0:D}='{1}'", index, secondaryDns));
					}
				}
				else if ("auth_name".Equals(name))
				{
					string authName = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set auth_name#{0:D}='{1}'", index, authName));
					}
				}
				else if ("auth_key".Equals(name))
				{
					string authKey = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set auth_key#{0:D}='{1}'", index, authKey));
					}
				}
				else if ("http_proxy_flag".Equals(name) && size >= 4)
				{
					int httpProxyFlag = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set http_proxy_flag#{0:D}={1:D}", index, httpProxyFlag));
					}
				}
				else if ("http_proxy_server".Equals(name))
				{
					string httpProxyServer = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set http_proxy_server#{0:D}='{1}'", index, httpProxyServer));
					}
				}
				else if ("http_proxy_port".Equals(name) && size >= 4)
				{
					int httpProxyPort = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set http_proxy_port#{0:D}={1:D}", index, httpProxyPort));
					}
				}
				else if ("version".Equals(name) && size >= 4)
				{
					int version = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set version#{0:D}={1:D}", index, version));
					}
				}
				else if ("device".Equals(name) && size >= 4)
				{
					int device = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set device#{0:D}={1:D}", index, device));
					}
				}
				else if ("auth_8021x_type".Equals(name) && size >= 4)
				{
					int authType = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set auth_8021x_type#{0:D}={1:D}", index, authType));
					}
				}
				else if ("auth_8021x_auth_name".Equals(name))
				{
					string authName = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set auth_8021x_auth_name#{0:D}='{1}'", index, authName));
					}
				}
				else if ("auth_8021x_auth_key".Equals(name))
				{
					string authKey = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set auth_8021x_auth_key#{0:D}='{1}'", index, authKey));
					}
				}
				else if ("wpa_key_type".Equals(name) && size >= 4)
				{
					int wpaKeyType = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set wpa_key_type#{0:D}={1:D}", index, wpaKeyType));
					}
				}
				else if ("wpa_key".Equals(name))
				{
					string wpaKey = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set wpa_key#{0:D}='{1}'", index, wpaKey));
					}
				}
				else if ("browser_flag".Equals(name) && size >= 4)
				{
					int browserFlag = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set browser_flag#{0:D}={1:D}", index, browserFlag));
					}
				}
				else if ("wifisvc_config".Equals(name) && size >= 4)
				{
					int wifisvcConfig = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set wifisvc_config#{0:D}={1:D}", index, wifisvcConfig));
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if (fullName.matches("/CONFIG/NETWORK/INFRASTRUCTURE/\\d+/SUB1"))
			{
				string indexName = fullName.Replace("/CONFIG/NETWORK/INFRASTRUCTURE/", "");
				int index = int.Parse(indexName.Substring(0, indexName.IndexOf("/", StringComparison.Ordinal)));
				if (log.DebugEnabled)
				{
					log.debug(string.Format("/CONFIG/NETWORK/INFRASTRUCTURE, index={0:D}, SUB1", index));
				}
				if ("wifisvc_auth_name".Equals(name))
				{
					string authName = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set wifisvc_auth_name#{0:D}='{1}'", index, authName));
					}
				}
				else if ("wifisvc_auth_key".Equals(name))
				{
					string authKey = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set wifisvc_auth_key#{0:D}='{1}'", index, authKey));
					}
				}
				else if ("wifisvc_option".Equals(name))
				{
					int wifisvcOption = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set wifisvc_option#{0:D}={1:D}", index, wifisvcOption));
					}
				}
				else if ("last_leased_dhcp_addr".Equals(name))
				{
					string lastLeasedDhcpAddr = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set last_leased_dhcp_addr#{0:D}='{1}'", index, lastLeasedDhcpAddr));
					}
				}
				else if ("bt_id".Equals(name))
				{
					int btId = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set bt_id#{0:D}={1:D}", index, btId));
					}
				}
				else if ("at_command".Equals(name))
				{
					string atCommand = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set at_command#{0:D}='{1}'", index, atCommand));
					}
				}
				else if ("phone_number".Equals(name))
				{
					string phoneNumber = buf.getStringNZ(size);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("set phone_number#{0:D}='{1}'", index, phoneNumber));
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/NETWORK/ADHOC".Equals(fullName))
			{
				if ("ssid_prefix".Equals(name))
				{
					adhocSsidPrefix = buf.getStringNZ(size);
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}'", name));
				}
			}
			else if ("/CONFIG/SYSTEM".Equals(fullName))
			{
				if ("owner_name".Equals(name))
				{
					ownerName = buf.getStringNZ(size);
				}
			}
			else if ("/CONFIG/SYSTEM/XMB/THEME".Equals(fullName))
			{
				if ("custom_theme_mode".Equals(name) && size >= 4)
				{
					settings.writeInt("registry.theme.custom_theme_mode", buf.getValue32());
				}
				else if ("color_mode".Equals(name) && size >= 4)
				{
					settings.writeInt("registry.theme.color_mode", buf.getValue32());
				}
				else if ("wallpaper_mode".Equals(name) && size >= 4)
				{
					settings.writeInt("registry.theme.wallpaper_mode", buf.getValue32());
				}
				else if ("system_color".Equals(name) && size >= 4)
				{
					settings.writeInt("registry.theme.system_color", buf.getValue32());
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}'", name));
				}
			}
			else if ("/CONFIG/MUSIC".Equals(fullName))
			{
				if ("visualizer_mode".Equals(name) && size >= 4)
				{
					musicVisualizerMode = buf.getValue32();
				}
				else if (name.Equals("track_info_mode") && size >= 4)
				{
					musicTrackInfoMode = buf.getValue32();
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/CAMERA".Equals(fullName))
			{
				if ("msid".Equals(name) && size >= 0)
				{
					string msid = buf.getStringNZ(16);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceRegSetKeyValue msid='{0}'", msid));
					}
				}
				else if (name.Equals("file_folder") && size >= 4)
				{
					int fileFolder = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceRegSetKeyValue fileFolder=0x{0:X}", fileFolder));
					}
				}
				else if (name.Equals("file_number") && size >= 4)
				{
					int fileNumber = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceRegSetKeyValue fileNumber=0x{0:X}", fileNumber));
					}
				}
				else if (name.Equals("movie_quality") && size >= 4)
				{
					int movieQuality = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceRegSetKeyValue movieQuality=0x{0:X}", movieQuality));
					}
				}
				else if (name.Equals("movie_size") && size >= 4)
				{
					int movieSize = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceRegSetKeyValue movieSize=0x{0:X}", movieSize));
					}
				}
				else if (name.Equals("movie_fps") && size >= 4)
				{
					int movieFps = buf.getValue32();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceRegSetKeyValue movieFps=0x{0:X}", movieFps));
					}
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/DATE".Equals(fullName))
			{
				if ("date_format".Equals(name))
				{
					settings.writeInt("registry.date_format", buf.getValue32());
				}
				else if ("time_format".Equals(name))
				{
					settings.writeInt("registry.time_format", buf.getValue32());
				}
				else if ("time_zone_offset".Equals(name))
				{
					settings.writeInt("registry.time_zone_offset", buf.getValue32());
				}
				else if ("time_zone_area".Equals(name))
				{
					settings.writeString("registry.time_zone_area", buf.StringZ);
				}
				else if ("summer_time".Equals(name))
				{
					settings.writeInt("registry.summer_time", buf.getValue32());
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else if ("/CONFIG/SYSTEM/XMB".Equals(fullName))
			{
				if ("language".Equals(name))
				{
					settings.writeInt(sceUtility.SYSTEMPARAM_SETTINGS_OPTION_LANGUAGE, buf.getValue32());
				}
				else if ("button_assign".Equals(name))
				{
					settings.writeInt(sceUtility.SYSTEMPARAM_SETTINGS_OPTION_BUTTON_PREFERENCE, buf.getValue32());
				}
				else
				{
					log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
				}
			}
			else
			{
				log.warn(string.Format("Unknown registry entry '{0}/{1}'", fullName, name));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD4475AA8, version = 150) public int sceRegGetKeyInfo(int hd, String name, pspsharp.HLE.TPointer32 hk, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 ptype, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 psize)
		[HLEFunction(nid : 0xD4475AA8, version : 150)]
		public virtual int sceRegGetKeyInfo(int hd, string name, TPointer32 hk, TPointer32 ptype, TPointer32 psize)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}

			KeyHandle keyHandle = new KeyHandle(name);
			keyHandles[keyHandle.uid] = keyHandle;

			hk.setValue(keyHandle.uid);

			return getKey(categoryHandle, name, ptype, psize, TPointer.NULL, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x28A8E98A, version = 150) public int sceRegGetKeyValue(int hd, int hk, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf, int size)
		[HLEFunction(nid : 0x28A8E98A, version : 150)]
		public virtual int sceRegGetKeyValue(int hd, int hk, TPointer buf, int size)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}

			KeyHandle keyHandle = keyHandles[hk];
			if (keyHandle == null)
			{
				return -1;
			}

			return getKey(categoryHandle, keyHandle.name, TPointer32.NULL, TPointer32.NULL, buf, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2C0DB9DD, version = 150) public int sceRegGetKeysNum(int hd, int num)
		[HLEFunction(nid : 0x2C0DB9DD, version : 150)]
		public virtual int sceRegGetKeysNum(int hd, int num)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D211135, version = 150) public int sceRegGetKeys(int hd, pspsharp.HLE.TPointer buf, int num)
		[HLEFunction(nid : 0x2D211135, version : 150)]
		public virtual int sceRegGetKeys(int hd, TPointer buf, int num)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4CA16893, version = 150) public int sceRegRemoveCategory(int h, String name)
		[HLEFunction(nid : 0x4CA16893, version : 150)]
		public virtual int sceRegRemoveCategory(int h, string name)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC5768D02, version = 150) public int sceRegGetKeyInfoByName(int hd, String name, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 ptype, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 psize)
		[HLEFunction(nid : 0xC5768D02, version : 150)]
		public virtual int sceRegGetKeyInfoByName(int hd, string name, TPointer32 ptype, TPointer32 psize)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceRegGetKeyInfoByName fullName='{0}/{1}'", categoryHandle.FullName, name));
			}

			return getKey(categoryHandle, name, ptype, psize, TPointer.NULL, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x30BE0259, version = 150) public int sceRegGetKeyValueByName(int hd, String name, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf, int size)
		[HLEFunction(nid : 0x30BE0259, version : 150)]
		public virtual int sceRegGetKeyValueByName(int hd, string name, TPointer buf, int size)
		{
			CategoryHandle categoryHandle = categoryHandles[hd];
			if (categoryHandle == null)
			{
				return -1;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceRegGetKeyValueByName fullName='{0}/{1}'", categoryHandle.FullName, name));
			}

			return getKey(categoryHandle, name, TPointer32.NULL, TPointer32.NULL, buf, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xDBA46704, version = 150) public int sceRegOpenRegistry_660(pspsharp.HLE.TPointer reg, int mode, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 h)
		[HLEFunction(nid : 0xDBA46704, version : 150)]
		public virtual int sceRegOpenRegistry_660(TPointer reg, int mode, TPointer32 h)
		{
			return sceRegOpenRegistry(reg, mode, h);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4F471457, version = 150) public int sceRegOpenCategory_660(int h, String name, int mode, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 hd)
		[HLEFunction(nid : 0x4F471457, version : 150)]
		public virtual int sceRegOpenCategory_660(int h, string name, int mode, TPointer32 hd)
		{
			return sceRegOpenCategory(h, name, mode, hd);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9980519F, version = 150) public int sceRegGetKeyInfo_660(int hd, String name, pspsharp.HLE.TPointer32 hk, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 ptype, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 psize)
		[HLEFunction(nid : 0x9980519F, version : 150)]
		public virtual int sceRegGetKeyInfo_660(int hd, string name, TPointer32 hk, TPointer32 ptype, TPointer32 psize)
		{
			return sceRegGetKeyInfo(hd, name, hk, ptype, psize);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF2619407, version = 150) public int sceRegGetKeyInfoByName_660(int hd, String name, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 ptype, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 psize)
		[HLEFunction(nid : 0xF2619407, version : 150)]
		public virtual int sceRegGetKeyInfoByName_660(int hd, string name, TPointer32 ptype, TPointer32 psize)
		{
			return sceRegGetKeyInfoByName(hd, name, ptype, psize);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF4A3E396, version = 150) public int sceRegGetKeyValue_660(int hd, int hk, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf, int size)
		[HLEFunction(nid : 0xF4A3E396, version : 150)]
		public virtual int sceRegGetKeyValue_660(int hd, int hk, TPointer buf, int size)
		{
			return sceRegGetKeyValue(hd, hk, buf, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x38415B9F, version = 150) public int sceRegGetKeyValueByName_660(int hd, String name, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf, int size)
		[HLEFunction(nid : 0x38415B9F, version : 150)]
		public virtual int sceRegGetKeyValueByName_660(int hd, string name, TPointer buf, int size)
		{
			return sceRegGetKeyValueByName(hd, name, buf, size);
		}

		[HLEFunction(nid : 0x3B6CA1E6, version : 150)]
		public virtual int sceRegCreateKey_660(int hd, string name, int type, int size)
		{
			return sceRegCreateKey(hd, name, type, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x49C70163, version = 150) public int sceRegSetKeyValue_660(int hd, String name, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buf, int size)
		[HLEFunction(nid : 0x49C70163, version : 150)]
		public virtual int sceRegSetKeyValue_660(int hd, string name, TPointer buf, int size)
		{
			return sceRegSetKeyValue(hd, name, buf, size);
		}

		[HLEFunction(nid : 0x5FD4764A, version : 150)]
		public virtual int sceRegFlushRegistry_660(int h)
		{
			return sceRegFlushRegistry(h);
		}

		[HLEFunction(nid : 0xFC742751, version : 150)]
		public virtual int sceRegCloseCategory_660(int hd)
		{
			return sceRegCloseCategory(hd);
		}

		[HLEFunction(nid : 0x49D77D65, version : 150)]
		public virtual int sceRegCloseRegistry_660(int h)
		{
			return sceRegCloseRegistry(h);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x61DB9D06, version = 150) public int sceRegRemoveCategory_660(int h, String name)
		[HLEFunction(nid : 0x61DB9D06, version : 150)]
		public virtual int sceRegRemoveCategory_660(int h, string name)
		{
			return sceRegRemoveCategory(h, name);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD743A608, version = 150) public int sceRegFlushCategory_660(int hd)
		[HLEFunction(nid : 0xD743A608, version : 150)]
		public virtual int sceRegFlushCategory_660(int hd)
		{
			return sceRegFlushCategory(hd);
		}
	}

}