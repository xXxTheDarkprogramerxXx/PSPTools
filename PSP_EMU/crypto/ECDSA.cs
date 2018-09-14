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
	using ECCurve = org.bouncycastle.math.ec.ECCurve;
	using BouncyCastleProvider = org.bouncycastle.jce.provider.BouncyCastleProvider;
	using JCEECPrivateKey = org.bouncycastle.jce.provider.JCEECPrivateKey;
	using ECParameterSpec = org.bouncycastle.jce.spec.ECParameterSpec;
	using ECPublicKeySpec = org.bouncycastle.jce.spec.ECPublicKeySpec;
	using ECPrivateKeySpec = org.bouncycastle.jce.spec.ECPrivateKeySpec;

	public class ECDSA
	{
		private static Logger log = Emulator.log;
		private KeyPair keyPair;
		private ECCurve curve;
		private ECParameterSpec spec;
		private KeyPairGenerator g;
		private KeyFactory f;

		static ECDSA()
		{
			Security.addProvider(new BouncyCastleProvider());
		}

		public ECDSA()
		{
		}

		public virtual void setCurve()
		{
			try
			{
				curve = new ECCurve.Fp(System.Numerics.BigInteger.Parse("FFFFFFFFFFFFFFFF00000001FFFFFFFFFFFFFFFF", System.Globalization.NumberStyles.HexNumber), System.Numerics.BigInteger.Parse("FFFFFFFFFFFFFFFF00000001FFFFFFFFFFFFFFFC", System.Globalization.NumberStyles.HexNumber), System.Numerics.BigInteger.Parse("A68BEDC33418029C1D3CE33B9A321FCCBB9E0F0B", System.Globalization.NumberStyles.HexNumber)); // b

				spec = new ECParameterSpec(curve, curve.createPoint(System.Numerics.BigInteger.Parse("0128EC4256487FD8FDF64E2437BC0A1F6D5AFDE2C", System.Globalization.NumberStyles.HexNumber), System.Numerics.BigInteger.Parse("05958557EB1DB001260425524DBC379D5AC5F4ADF", System.Globalization.NumberStyles.HexNumber), false), System.Numerics.BigInteger.Parse("00FFFFFFFFFFFFFFFEFFFFB5AE3C523E63944F2127", System.Globalization.NumberStyles.HexNumber)); // n

				g = KeyPairGenerator.getInstance("ECDSA", "BC");
				f = KeyFactory.getInstance("ECDSA", "BC");
				g.initialize(spec, new SecureRandom());

				keyPair = g.generateKeyPair();
			}
			catch (Exception e)
			{
				log.error("setCurve", e);
			}
		}

		public virtual void sign(sbyte[] hash, sbyte[] priv, sbyte[] R, sbyte[] S)
		{
			// TODO
		}

		public virtual void verify(sbyte[] hash, sbyte[] pub, sbyte[] R, sbyte[] S)
		{
			// TODO
		}

		public virtual sbyte[] PrivateKey
		{
			get
			{
				return ((JCEECPrivateKey) keyPair.Private).D.toByteArray();
			}
		}

		public virtual sbyte[] PublicKey
		{
			get
			{
				return keyPair.Public.Encoded;
			}
		}

		public virtual sbyte[] multiplyPublicKey(sbyte[] pub, sbyte[] priv)
		{
			PublicKey multPubKey = null;
			ECPrivateKeySpec privateKeySpec = new ECPrivateKeySpec(new BigInteger(priv), spec);
			ECPublicKeySpec publicKeySpec = new ECPublicKeySpec(curve.decodePoint(pub), spec);
			ECPublicKeySpec newPublicKeySpec = new ECPublicKeySpec(publicKeySpec.Q.multiply(privateKeySpec.D), spec);
			try
			{
				multPubKey = f.generatePublic(newPublicKeySpec);
			}
			catch (Exception)
			{
			}
			return multPubKey.Encoded;
		}
	}
}