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
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;

	using Logger = org.apache.log4j.Logger;

	public class sceSsl : HLEModule
	{
		public static Logger log = Modules.getLogger("sceSsl");

		private bool isSslInit;
		private int maxMemSize;
		private int currentMemSize;
		private SysMemInfo cryptoMalloc;

		[HLELogging(level:"info"), HLEFunction(nid : 0x957ECBE2, version : 150)]
		public virtual int sceSslInit(int heapSize)
		{
			if (isSslInit)
			{
				return SceKernelErrors.ERROR_SSL_ALREADY_INIT;
			}
			if (heapSize <= 0)
			{
				return SceKernelErrors.ERROR_SSL_INVALID_PARAMETER;
			}

			maxMemSize = heapSize;
			currentMemSize = heapSize / 2; // Dummy value.
			isSslInit = true;

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x191CDEFF, version : 150)]
		public virtual int sceSslEnd()
		{
			if (!isSslInit)
			{
				return SceKernelErrors.ERROR_SSL_NOT_INIT;
			}

			isSslInit = false;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5BFB6B61, version = 150) public int sceSslGetNotAfter(@CanBeNull pspsharp.HLE.TPointer sslCertAddr, @CanBeNull pspsharp.HLE.TPointer endTimeAddr)
		[HLEFunction(nid : 0x5BFB6B61, version : 150)]
		public virtual int sceSslGetNotAfter(TPointer sslCertAddr, TPointer endTimeAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x17A10DCC, version = 150) public int sceSslGetNotBefore(@CanBeNull pspsharp.HLE.TPointer sslCertAddr, @CanBeNull pspsharp.HLE.TPointer startTimeAddr)
		[HLEFunction(nid : 0x17A10DCC, version : 150)]
		public virtual int sceSslGetNotBefore(TPointer sslCertAddr, TPointer startTimeAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3DD5E023, version = 150) public int sceSslGetSubjectName()
		[HLEFunction(nid : 0x3DD5E023, version : 150)]
		public virtual int sceSslGetSubjectName()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1B7C8191, version = 150) public int sceSslGetIssuerName()
		[HLEFunction(nid : 0x1B7C8191, version : 150)]
		public virtual int sceSslGetIssuerName()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCC0919B0, version = 150) public int sceSslGetSerialNumber()
		[HLEFunction(nid : 0xCC0919B0, version : 150)]
		public virtual int sceSslGetSerialNumber()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x058D21C0, version = 150) public int sceSslGetNameEntryCount()
		[HLEFunction(nid : 0x058D21C0, version : 150)]
		public virtual int sceSslGetNameEntryCount()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD6D097B4, version = 150) public int sceSslGetNameEntryInfo()
		[HLEFunction(nid : 0xD6D097B4, version : 150)]
		public virtual int sceSslGetNameEntryInfo()
		{
			return 0;
		}

		[HLEFunction(nid : 0xB99EDE6A, version : 150)]
		public virtual int sceSslGetUsedMemoryMax(TPointer32 maxMemAddr)
		{
			if (!isSslInit)
			{
				return SceKernelErrors.ERROR_SSL_NOT_INIT;
			}

			maxMemAddr.setValue(maxMemSize);

			return 0;
		}

		[HLEFunction(nid : 0x0EB43B06, version : 150)]
		public virtual int sceSslGetUsedMemoryCurrent(TPointer32 currentMemAddr)
		{
			if (!isSslInit)
			{
				return SceKernelErrors.ERROR_SSL_NOT_INIT;
			}

			currentMemAddr.setValue(currentMemSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF57765D3, version = 150) public int sceSslGetKeyUsage()
		[HLEFunction(nid : 0xF57765D3, version : 150)]
		public virtual int sceSslGetKeyUsage()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9266C0D5, version = 150) public int sceSsl_9266C0D5()
		[HLEFunction(nid : 0x9266C0D5, version : 150)]
		public virtual int sceSsl_9266C0D5()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB40D11EA, version = 150) public int SSLv3_client_method()
		[HLEFunction(nid : 0xB40D11EA, version : 150)]
		public virtual int SSLv3_client_method()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFB8273FE, version = 150) public int SSL_CTX_new(int method)
		[HLEFunction(nid : 0xFB8273FE, version : 150)]
		public virtual int SSL_CTX_new(int method)
		{
			return 0x12345678;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x588F2FE8, version = 150) public int SSL_CTX_free(int ctx)
		[HLEFunction(nid : 0x588F2FE8, version : 150)]
		public virtual int SSL_CTX_free(int ctx)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB4D78E98, version = 150) public int SSL_CTX_ctrl(int ctx, int cmd, int larg, int parg)
		[HLEFunction(nid : 0xB4D78E98, version : 150)]
		public virtual int SSL_CTX_ctrl(int ctx, int cmd, int larg, int parg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAEBF278B, version = 150) public int SSL_CTX_set_verify(int ctx, int mode, @CanBeNull pspsharp.HLE.TPointer verify_callback)
		[HLEFunction(nid : 0xAEBF278B, version : 150)]
		public virtual int SSL_CTX_set_verify(int ctx, int mode, TPointer verify_callback)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x529A9477, version = 150) public int sceSsl_lib_529A9477(int ctx, pspsharp.HLE.TPointer unknown1, int unknown2)
		[HLEFunction(nid : 0x529A9477, version : 150)]
		public virtual int sceSsl_lib_529A9477(int ctx, TPointer unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBF55C31C, version = 150) public int SSL_CTX_set_client_cert_cb(int ctx, @CanBeNull pspsharp.HLE.TPointer client_cert_cb)
		[HLEFunction(nid : 0xBF55C31C, version : 150)]
		public virtual int SSL_CTX_set_client_cert_cb(int ctx, TPointer client_cert_cb)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0861D934, version = 150) public int CRYPTO_malloc(int size)
		[HLEFunction(nid : 0x0861D934, version : 150)]
		public virtual int CRYPTO_malloc(int size)
		{
			cryptoMalloc = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, "CRYPTO_malloc", SysMemUserForUser.PSP_SMEM_Low, size, 0);
			if (cryptoMalloc == null)
			{
				return 0;
			}
			return cryptoMalloc.addr;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5E5C873A, version = 150) public int CRYPTO_free(int allocatedAddress)
		[HLEFunction(nid : 0x5E5C873A, version : 150)]
		public virtual int CRYPTO_free(int allocatedAddress)
		{
			if (cryptoMalloc != null && cryptoMalloc.addr == allocatedAddress)
			{
				Modules.SysMemUserForUserModule.free(cryptoMalloc);
				cryptoMalloc = null;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEBFB0E3C, version = 150) public int SSL_new(int ctx)
		[HLEFunction(nid : 0xEBFB0E3C, version : 150)]
		public virtual int SSL_new(int ctx)
		{
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x84833472, version = 150) public int SSL_free(int ssl)
		[HLEFunction(nid : 0x84833472, version : 150)]
		public virtual int SSL_free(int ssl)
		{
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x28B4DE33, version = 150) public int BIO_new_socket(int socket, int closeFlag)
		[HLEFunction(nid : 0x28B4DE33, version : 150)]
		public virtual int BIO_new_socket(int socket, int closeFlag)
		{
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB9C8CCE6, version = 150) public void SSL_set_bio(int ssl, int rbio, int wbio)
		[HLEFunction(nid : 0xB9C8CCE6, version : 150)]
		public virtual void SSL_set_bio(int ssl, int rbio, int wbio)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xECE07B61, version = 150) public int sceSsl_lib_ECE07B61(int bio, int unknown)
		[HLEFunction(nid : 0xECE07B61, version : 150)]
		public virtual int sceSsl_lib_ECE07B61(int bio, int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x80608663, version = 150) public void SSL_set_connect_state(int ssl)
		[HLEFunction(nid : 0x80608663, version : 150)]
		public virtual void SSL_set_connect_state(int ssl)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB7CA8717, version = 150) public int SSL_write(int ssl, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buffer, int length)
		[HLEFunction(nid : 0xB7CA8717, version : 150)]
		public virtual int SSL_write(int ssl, TPointer buffer, int length)
		{
			return length;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB3B04C58, version = 150) public int SSL_get_error(int ssl, int returnValue)
		[HLEFunction(nid : 0xB3B04C58, version : 150)]
		public virtual int SSL_get_error(int ssl, int returnValue)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE7C29542, version = 150) public int SSL_read(int ssl, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.returnValue, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer, int size)
		[HLEFunction(nid : 0xE7C29542, version : 150)]
		public virtual int SSL_read(int ssl, TPointer buffer, int size)
		{
			return 0;
		}
	}
}