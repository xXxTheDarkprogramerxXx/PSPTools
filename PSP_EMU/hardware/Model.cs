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
namespace pspsharp.hardware
{
	public class Model
	{
		public const int MODEL_PSP_FAT = 0;
		public const int MODEL_PSP_SLIM = 1;
		public const int MODEL_PSP_BRITE = 2;
		public const int MODEL_PSP_BRITE2 = 3;
		public const int MODEL_PSP_GO = 4;
		private static int model = MODEL_PSP_FAT;
		private static readonly string[] modelNames = new string[] {"MODEL_PSP_FAT", "MODEL_PSP_SLIM", "MODEL_PSP_BRITE", "MODEL_PSP_BRITE2", "MODEL_PSP_GO"};

		public static int getModel()
		{
			return model;
		}

		public static void setModel(int model)
		{
			Model.model = model;
		}

		public static string getModelName(int model)
		{
			if (model >= 0 && model < modelNames.Length)
			{
				return modelNames[model];
			}

			return string.Format("Unknown Model {0:D}", model);
		}
	}

}