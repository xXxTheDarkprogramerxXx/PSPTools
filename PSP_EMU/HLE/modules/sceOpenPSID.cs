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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	//using Logger = org.apache.log4j.Logger;

	public class sceOpenPSID : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceOpenPSID");

		protected internal int[] dummyOpenPSID = new int[] {0x10, 0x02, 0xA3, 0x44, 0x13, 0xF5, 0x93, 0xB0, 0xCC, 0x6E, 0xD1, 0x32, 0x27, 0x85, 0x0F, 0x9D};
		protected internal int[] dummyPSID = new int[] {0x10, 0x02, 0xA3, 0x44, 0x13, 0xF5, 0x93, 0xB0, 0xCC, 0x6E, 0xD1, 0x32, 0x27, 0x85, 0x0F, 0x9D};

		[HLEFunction(nid : 0xC69BEBCE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceOpenPSIDGetOpenPSID(TPointer openPSIDAddr)
		{
			for (int i = 0; i < dummyOpenPSID.Length; i++)
			{
				openPSIDAddr.setValue8(i, (sbyte) dummyOpenPSID[i]);
			}

			return 0;
		}

		[HLEFunction(nid : 0x19D579F0, version : 150)]
		public virtual int sceOpenPSIDGetPSID(TPointer PSIDAddr, int unknown)
		{
			for (int i = 0; i < dummyPSID.Length; i++)
			{
				PSIDAddr.setValue8(i, (sbyte) dummyPSID[i]);
			}

			return 0;
		}

		/// <summary>
		/// Encrypt the provided data. It will be encrypted using AES.
		/// 
		/// @note The used key is provided by the PSP.
		/// </summary>
		/// <param name="pSrcData"> Pointer to data to encrypt. The encrypted data will be written 
		///                 back into this buffer. </param>
		/// <param name="size"> The size of the data to encrypt. The size needs to be a multiple of ::KIRK_AES_BLOCK_LEN. \n
		///               Max size: ::SCE_DNAS_USER_DATA_MAX_LEN.
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x05D50F41, version = 150) public int sceDdrdbEncrypt(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer srcData, int size)
		[HLEFunction(nid : 0x05D50F41, version : 150)]
		public virtual int sceDdrdbEncrypt(TPointer srcData, int size)
		{
			return 0;
		}

		/// <summary>
		/// Verify a certificate.
		/// </summary>
		/// <param name="pCert"> Pointer to the certificate to verify. Certificate Length: ::KIRK_CERT_LEN.
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x370F456A, version = 150) public int sceDdrdbCertvry(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=184, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer cert)
		[HLEFunction(nid : 0x370F456A, version : 150)]
		public virtual int sceDdrdbCertvry(TPointer cert)
		{
			return 0;
		}

		/// <summary>
		/// Generate a SHA-1 hash value of the provided data.
		/// </summary>
		/// <param name="pSrcData"> Pointer to data to generate the hash for. </param>
		/// <param name="size"> The size of the source data. Max size: ::SCE_DNAS_USER_DATA_MAX_LEN. </param>
		/// <param name="pDigest"> Pointer to buffer receiving the hash. Size: ::KIRK_SHA1_DIGEST_LEN.
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x40CB752A, version = 150) public int sceDdrdbHash(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer srcData, int size, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer digest)
		[HLEFunction(nid : 0x40CB752A, version : 150)]
		public virtual int sceDdrdbHash(TPointer srcData, int size, TPointer digest)
		{
			return 0;
		}

		/// <summary>
		/// Generate a valid signature for the specified data using the specified private key.
		/// 
		/// @note The ECDSA algorithm is used to generate a signature.
		/// </summary>
		/// <param name="pPrivKey"> Pointer to the private key used to generate the signature. \n
		///                 CONFIRM: The key has to be AES encrypted before. </param>
		/// <param name="pData"> Pointer to data a signature has to be computed for. Data Length: ::KIRK_ECDSA_SRC_DATA_LEN </param>
		/// <param name="pSig"> Pointer to a buffer receiving the signature. Signature Length: ::KIRK_ECDSA_SIG_LEN
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB24E1391, version = 150) public int sceDdrdbSiggen(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer privKey, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer srcData, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=40, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer sig)
		[HLEFunction(nid : 0xB24E1391, version : 150)]
		public virtual int sceDdrdbSiggen(TPointer privKey, TPointer srcData, TPointer sig)
		{
			return 0;
		}

		/// <summary>
		/// Decrypt the provided data. The data has to be AES encrypted. 
		/// 
		/// @note The used key is provided by the PSP.
		/// </summary>
		/// <param name="pSrcData"> Pointer to data to decrypt. The decrypted data will be written \n
		///                 back into this buffer. </param>
		/// <param name="size"> The size of the data to decrypt. The size needs to be a multiple of ::KIRK_AES_BLOCK_LEN. \n
		///               Max size: ::SCE_DNAS_USER_DATA_MAX_LEN.
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB33ACB44, version = 150) public int sceDdrdbDecrypt(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer srcData, int size)
		[HLEFunction(nid : 0xB33ACB44, version : 150)]
		public virtual int sceDdrdbDecrypt(TPointer srcData, int size)
		{
			return 0;
		}

		/// <summary>
		/// Generate a ::KIRK_PRN_LEN large pseudorandom number (PRN). 
		/// 
		/// @note The seed is automatically set by the system software.
		/// </summary>
		/// <param name="pDstData"> Pointer to buffer receiving the PRN. Size has to be ::KIRK_PRN_LEN.
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB8218473, version = 150) public int sceDdrdbPrngen(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dstData)
		[HLEFunction(nid : 0xB8218473, version : 150)]
		public virtual int sceDdrdbPrngen(TPointer dstData)
		{
			return 0;
		}

		/// <summary>
		/// Verify if the provided signature is valid for the specified data given the public key. 
		/// 
		/// @note The ECDSA algorithm is used to verify a signature.
		/// </summary>
		/// <param name="pPubKey"> The public key used for validating the (data,signature) pair. \n
		///                Size has to be ::KIRK_ECDSA_PUBLIC_KEY_LEN. </param>
		/// <param name="pData"> Pointer to data the signature has to be verified for. \n
		///                Data Length: ::KIRK_ECDSA_SRC_DATA_LEN \n </param>
		/// <param name="pSig"> Pointer to the signature to verify. Signature Length: ::KIRK_ECDSA_SIG_LEN
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE27CE4CB, version = 150) public int sceDdrdbSigvry(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=40, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer pubKey, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer data, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=40, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer sig)
		[HLEFunction(nid : 0xE27CE4CB, version : 150)]
		public virtual int sceDdrdbSigvry(TPointer pubKey, TPointer data, TPointer sig)
		{
			return 0;
		}

		/// 
		/// <summary>
		/// Compute a new elliptic curve point by multiplying the provided private key with the \n
		/// provided base point of the elliptic curve.
		/// </summary>
		/// <param name="pPrivKey"> Pointer to the private key of a (public,private) key pair usable for ECDSA.
		/// </param>
		/// <param name="pBasePoint"> Pointer to a base point of the elliptic curve. Point size: ::KIRK_ECDSA_POINT_LEN </param>
		/// <param name="pNewPoint"> Pointer to a buffer receiving the new curve point. Buffer size: ::KIRK_ECDSA_POINT_LEN
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEC05300A, version = 150) public int sceDdrdbMul2(pspsharp.HLE.TPointer privKey, pspsharp.HLE.TPointer basePoint, pspsharp.HLE.TPointer newPoint)
		[HLEFunction(nid : 0xEC05300A, version : 150)]
		public virtual int sceDdrdbMul2(TPointer privKey, TPointer basePoint, TPointer newPoint)
		{
			return 0;
		}

		/// <summary>
		/// Generate a new (public,private) key pair to use with ECDSA.
		/// </summary>
		/// <param name="pKeyData"> Pointer to buffer receiving the computed key pair. \n
		///                 The first ::KIRK_ECDSA_PRIVATE_KEY_LEN byte will contain the private key. \n
		///                 The rest of the bytes will contain the public key (elliptic curve) point p = (x,y), \n
		///                 with the x-value being first. Both coordinates have size ::KIRK_ECDSA_POINT_LEN / 2.
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF970D54E, version = 150) public int sceDdrdbMul1(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=60, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer keyData)
		[HLEFunction(nid : 0xF970D54E, version : 150)]
		public virtual int sceDdrdbMul1(TPointer keyData)
		{
			return 0;
		}

		/// <summary>
		/// Verify if the provided signature is valid for the specified data. The public key\n
		/// is provided by the system software.
		/// 
		/// @note The ECDSA algorithm is used to verify a signature.
		/// </summary>
		/// <param name="pData"> Pointer to data the signature has to be verified for. \n
		///              Data Length: ::KIRK_ECDSA_SRC_DATA_LEN. </param>
		/// <param name="pSig"> Pointer to the signature to verify. Signature Length: ::KIRK_ECDSA_SIG_LEN.
		/// </param>
		/// <returns> 0 on success, otherwise < 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF013F8BF, version = 150) public int sceDdrdb_F013F8BF(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer data, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=40, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer sig)
		[HLEFunction(nid : 0xF013F8BF, version : 150)]
		public virtual int sceDdrdb_F013F8BF(TPointer data, TPointer sig)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8523E178, version = 150) public int sceMlnpsnlAuth1BB(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=8, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown2, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=128, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown3, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=64, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown4)
		[HLEFunction(nid : 0x8523E178, version : 150)]
		public virtual int sceMlnpsnlAuth1BB(TPointer unknown1, TPointer unknown2, TPointer unknown3, TPointer unknown4)
		{
			unknown3.clear(128);
			unknown4.clear(64);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6885F392, version = 150) public int sceMlnpsnlAuth2BB()
		[HLEFunction(nid : 0x6885F392, version : 150)]
		public virtual int sceMlnpsnlAuth2BB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF9ECFDDD, version = 150) public int scePcactAuth1BB()
		[HLEFunction(nid : 0xF9ECFDDD, version : 150)]
		public virtual int scePcactAuth1BB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x08BB9677, version = 150) public int scePcactAuth2BB()
		[HLEFunction(nid : 0x08BB9677, version : 150)]
		public virtual int scePcactAuth2BB()
		{
			return 0;
		}
	}
}