using System.Threading;

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
	public class GPS
	{
		// Simulate position of New-York City
		private static float positionLatitude = 40.713387f;
		private static float positionLongitude = -74.005516f;
		private static float positionAltitude = 39f;

		public static float PositionLatitude
		{
			get
			{
				return positionLatitude;
			}
			set
			{
				GPS.positionLatitude = value;
			}
		}


		public static float PositionLongitude
		{
			get
			{
				return positionLongitude;
			}
			set
			{
				GPS.positionLongitude = value;
			}
		}


		public static float PositionAltitude
		{
			get
			{
				return positionAltitude;
			}
			set
			{
				GPS.positionAltitude = value;
			}
		}


		public static void initialize()
		{
			FakeGPSMove.initialize();
		}

		private class FakeGPSMove : Thread
		{
			internal static FakeGPSMove instance;
			internal long sleepMillis;
			internal float latitudeDelta;
			internal float longitudeDelta;
			internal float altitudeDelta;

			internal static void initialize()
			{
				if (instance == null)
				{
					// Fake a slight position move every 2 seconds
					instance = new FakeGPSMove(2000, 0.00001f, 0.00001f, 0f);
					instance.Daemon = true;
					instance.Name = "Fake GPS Move";
					instance.Start();
				}
			}

			public FakeGPSMove(long sleepMillis, float latitudeDelta, float longitudeDelta, float altitudeDelta)
			{
				this.sleepMillis = sleepMillis;
				this.latitudeDelta = latitudeDelta;
				this.longitudeDelta = longitudeDelta;
				this.altitudeDelta = altitudeDelta;
			}

			public override void run()
			{
				while (true)
				{
					try
					{
						sleep(sleepMillis);
					}
					catch (InterruptedException)
					{
						// Ignore exception
					}

					if (latitudeDelta != 0f)
					{
						GPS.PositionLatitude = GPS.PositionLatitude + latitudeDelta;
					}
					if (longitudeDelta != 0f)
					{
						GPS.PositionLongitude = GPS.PositionLongitude + longitudeDelta;
					}
					if (altitudeDelta != 0f)
					{
						GPS.PositionAltitude = GPS.PositionAltitude + altitudeDelta;
					}
				}
			}
		}
	}

}