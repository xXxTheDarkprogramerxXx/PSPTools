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
	public class SceKernelErrors
	{
		/*
		 * PSP Errors:
		 * Represented by a 32-bit value with the following scheme:
		 *
		 *  31  30  29  28  27        16  15        0
		 * | 1 | 0 | 0 | 0 | X | ... | X | E |... | E |
		 *
		 * Bits 31 and 30: Can only be 1 or 0.
		 *      -> If both are 0, there's no error (0x0==SUCCESS).
		 *      -> If 31 is 1 but 30 is 0, there's an error (0x80000000).
		 *      -> If both bits are 1, a critical error stops the PSP (0xC0000000).
		 *
		 * Bits 29 and 28: Unknown. Never change.
		 *
		 * Bits 27 to 16 (X): Represent the system area associated with the error.
		 *      -> 0x000 - Null (can be used anywhere).
		 *      -> 0x001 - Errno (PSP's implementation of errno.h).
		 *      -> 0x002 - Kernel.
		 *      -> 0x011 - Utility.
		 *      -> 0x021 - UMD.
		 *      -> 0x022 - MemStick.
		 *      -> 0x026 - Audio.
		 *      -> 0x02b - Power.
		 *      -> 0x041 - Wlan.
		 *      -> 0x042 - SAS.
		 *      -> 0x043 - HTTP(0x0431)/HTTPS/SSL(0x0435).
		 *      -> 0x044 - WAVE.
		 *      -> 0x046 - Font.
		 *      -> 0x051 - PGD.
		 *      -> 0x055 - NP(0x05503)/NPDRM(0x05509).
		 *      -> 0x061 - MPEG(0x0618)/PSMF(0x0615)/PSMF Player(0x0616).
		 *      -> 0x062 - AVC.
		 *      -> 0x063 - ATRAC.
		 *      -> 0x069 - AAC.
		 *      -> 0x07f - Codec.
		 *
		 * Bits 15 to 0 (E): Represent the error code itself (different for each area).
		 *      -> E.g.: 0x80110001 - Error -> Utility -> Some unknown error.
		 */

		public const int ERROR_NOT_INITIALIZED = unchecked((int)0x80000001);
		public const int ERROR_UNMATCHED_VERSION = unchecked((int)0x80000002);
		public const int ERROR_NOT_IMPLEMENTED = unchecked((int)0x80000003);
		public const int ERROR_NOT_SUPPORTED = unchecked((int)0x80000004);
		public const int ERROR_ALREADY = unchecked((int)0x80000020);
		public const int ERROR_BUSY = unchecked((int)0x80000021);
		public const int ERROR_OUT_OF_MEMORY = unchecked((int)0x80000022);
		public const int ERROR_PRIV_REQUIRED = unchecked((int)0x80000023);
		public const int ERROR_TIMEOUT = unchecked((int)0x80000024);
		public const int ERROR_NOT_FOUND = unchecked((int)0x80000025);
		public const int ERROR_ILLEGAL_CONTEXT = unchecked((int)0x80000030);
		public const int ERROR_CPUDI = unchecked((int)0x80000031);
		public const int ERROR_THREAD = unchecked((int)0x80000040);
		public const int ERROR_SEMAPHORE = unchecked((int)0x80000041);
		public const int ERROR_EVENTFLAG = unchecked((int)0x80000042);
		public const int ERROR_TIMER = unchecked((int)0x80000043);
		public const int ERROR_ALARM = unchecked((int)0x80000044);

		public const int ERROR_INVALID_ID = unchecked((int)0x80000100);
		public const int ERROR_INVALID_NAME = unchecked((int)0x80000101);
		public const int ERROR_INVALID_INDEX = unchecked((int)0x80000102);
		public const int ERROR_INVALID_POINTER = unchecked((int)0x80000103);
		public const int ERROR_INVALID_SIZE = unchecked((int)0x80000104);
		public const int ERROR_INVALID_FLAG = unchecked((int)0x80000105);
		public const int ERROR_INVALID_COMMAND = unchecked((int)0x80000106);
		public const int ERROR_INVALID_MODE = unchecked((int)0x80000107);
		public const int ERROR_INVALID_FORMAT = unchecked((int)0x80000108);
		public const int ERROR_INVALID_VALUE = unchecked((int)0x800001FE);
		public const int ERROR_INVALID_ARGUMENT = unchecked((int)0x800001FF);

		public const int ERROR_NOENT = unchecked((int)0x80000202);
		public const int ERROR_BAD_FILE = unchecked((int)0x80000209);
		public const int ERROR_ACCESS_ERROR = unchecked((int)0x8000020D);
		public const int ERROR_EXIST = unchecked((int)0x80000211);
		public const int ERROR_INVAL = unchecked((int)0x80000216);
		public const int ERROR_MFILE = unchecked((int)0x80000218);
		public const int ERROR_NOSPC = unchecked((int)0x8000021C);
		public const int ERROR_DFUNC = unchecked((int)0x800002FF);

		public const int ERROR_ERRNO_BASE = unchecked((int)0x80010000);
		public const int ERROR_ERRNO_OPERATION_NOT_PERMITTED = unchecked((int)0x80010001);
		public const int ERROR_ERRNO_FILE_NOT_FOUND = unchecked((int)0x80010002);
		public const int ERROR_ERRNO_FILE_OPEN_ERROR = unchecked((int)0x80010003);
		public const int ERROR_ERRNO_IO_ERROR = unchecked((int)0x80010005);
		public const int ERROR_ERRNO_ARG_LIST_TOO_LONG = unchecked((int)0x80010007);
		public const int ERROR_ERRNO_INVALID_FILE_DESCRIPTOR = unchecked((int)0x80010009);
		public const int ERROR_ERRNO_RESOURCE_UNAVAILABLE = unchecked((int)0x8001000B);
		public const int ERROR_ERRNO_NO_MEMORY = unchecked((int)0x8001000C);
		public const int ERROR_ERRNO_NO_PERM = unchecked((int)0x8001000D);
		public const int ERROR_ERRNO_FILE_INVALID_ADDR = unchecked((int)0x8001000E);
		public const int ERROR_ERRNO_DEVICE_BUSY = unchecked((int)0x80010010);
		public const int ERROR_ERRNO_FILE_ALREADY_EXISTS = unchecked((int)0x80010011);
		public const int ERROR_ERRNO_CROSS_DEV_LINK = unchecked((int)0x80010012);
		public const int ERROR_ERRNO_DEVICE_NOT_FOUND = unchecked((int)0x80010013);
		public const int ERROR_ERRNO_NOT_A_DIRECTORY = unchecked((int)0x80010014);
		public const int ERROR_ERRNO_IS_DIRECTORY = unchecked((int)0x80010015);
		public const int ERROR_ERRNO_INVALID_ARGUMENT = unchecked((int)0x80010016);
		public const int ERROR_ERRNO_TOO_MANY_OPEN_SYSTEM_FILES = unchecked((int)0x80010018);
		public const int ERROR_ERRNO_FILE_IS_TOO_BIG = unchecked((int)0x8001001B);
		public const int ERROR_ERRNO_DEVICE_NO_FREE_SPACE = unchecked((int)0x8001001C);
		public const int ERROR_ERRNO_READ_ONLY = unchecked((int)0x8001001E);
		public const int ERROR_ERRNO_CLOSED = unchecked((int)0x80010020);
		public const int ERROR_ERRNO_FILE_PATH_TOO_LONG = unchecked((int)0x80010024);
		public const int ERROR_ERRNO_FILE_PROTOCOL = unchecked((int)0x80010047);
		public const int ERROR_ERRNO_DIRECTORY_IS_NOT_EMPTY = unchecked((int)0x8001005A);
		public const int ERROR_ERRNO_TOO_MANY_SYMBOLIC_LINKS = unchecked((int)0x8001005C);
		public const int ERROR_ERRNO_FILE_ADDR_IN_USE = unchecked((int)0x80010062);
		public const int ERROR_ERRNO_CONNECTION_ABORTED = unchecked((int)0x80010067);
		public const int ERROR_ERRNO_CONNECTION_RESET = unchecked((int)0x80010068);
		public const int ERROR_ERRNO_NO_FREE_BUF_SPACE = unchecked((int)0x80010069);
		public const int ERROR_ERRNO_FILE_TIMEOUT = unchecked((int)0x8001006E);
		public const int ERROR_ERRNO_IN_PROGRESS = unchecked((int)0x80010077);
		public const int ERROR_ERRNO_ALREADY = unchecked((int)0x80010078);
		public const int ERROR_ERRNO_NO_MEDIA = unchecked((int)0x8001007B);
		public const int ERROR_ERRNO_INVALID_MEDIUM = unchecked((int)0x8001007C);
		public const int ERROR_ERRNO_ADDRESS_NOT_AVAILABLE = unchecked((int)0x8001007D);
		public const int ERROR_ERRNO_IS_ALREADY_CONNECTED = unchecked((int)0x8001007F);
		public const int ERROR_ERRNO_NOT_CONNECTED = unchecked((int)0x80010080);
		public const int ERROR_ERRNO_FILE_QUOTA_EXCEEDED = unchecked((int)0x80010084);
		public const int ERROR_ERRNO_INVALID_IODEVCTL_CMD = unchecked((int)0x80010086);
		public const int ERROR_ERRNO_FUNCTION_NOT_SUPPORTED = unchecked((int)0x8001B000);
		public const int ERROR_ERRNO_ADDR_OUT_OF_MAIN_MEM = unchecked((int)0x8001B001);
		public const int ERROR_ERRNO_INVALID_UNIT_NUM = unchecked((int)0x8001B002);
		public const int ERROR_ERRNO_INVALID_FILE_SIZE = unchecked((int)0x8001B003);
		public const int ERROR_ERRNO_INVALID_FLAG = unchecked((int)0x8001B004);

		public const int ERROR_KERNEL_ERROR = unchecked((int)0x80020001);
		public const int ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT = unchecked((int)0x80020064);
		public const int ERROR_KERNEL_INVALID_INTR_NUMBER = unchecked((int)0x80020065);
		public const int ERROR_KERNEL_INTERRUPTS_ALREADY_DISABLED = unchecked((int)0x80020066);
		public const int ERROR_KERNEL_SUBINTR_ALREADY_REGISTERED = unchecked((int)0x80020067);
		public const int ERROR_KERNEL_SUBINTR_NOT_REGISTERED = unchecked((int)0x80020068);
		public const int ERROR_KERNEL_UNKNOWN_UID_TYPE = unchecked((int)0x800200c9);
		public const int ERROR_KERNEL_UNKNOWN_UID = unchecked((int)0x800200cb);
		public const int ERROR_KERNEL_UNMATCH_TYPE_UID = unchecked((int)0x800200cc);
		public const int ERROR_KERNEL_NOT_EXIST_ID = unchecked((int)0x800200cd);
		public const int ERROR_KERNEL_NOT_FOUND_FUNCTION_UID = unchecked((int)0x800200ce);
		public const int ERROR_KERNEL_ALREADY_HOLDER_UID = unchecked((int)0x800200cf);
		public const int ERROR_KERNEL_NOT_HOLDER_UID = unchecked((int)0x800200d0);
		public const int ERROR_KERNEL_ILLEGAL_PERMISSION = unchecked((int)0x800200d1);
		public const int ERROR_KERNEL_ILLEGAL_ARGUMENT = unchecked((int)0x800200d2);
		public const int ERROR_KERNEL_ILLEGAL_ADDR = unchecked((int)0x800200d3);
		public const int ERROR_KERNEL_OUT_OF_RANGE = unchecked((int)0x800200d4);
		public const int ERROR_KERNEL_MEMORY_AREA_IS_OVERLAP = unchecked((int)0x800200d5);
		public const int ERROR_KERNEL_ILLEGAL_PARTITION_ID = unchecked((int)0x800200d6);
		public const int ERROR_KERNEL_PARTITION_IN_USE = unchecked((int)0x800200d7);
		public const int ERROR_KERNEL_ILLEGAL_MEMBLOCK_ALLOC_TYPE = unchecked((int)0x800200d8);
		public const int ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK = unchecked((int)0x800200d9);
		public const int ERROR_KERNEL_INHIBITED_RESIZE_MEMBLOCK = unchecked((int)0x800200da);
		public const int ERROR_KERNEL_FAILED_RESIZE_MEMBLOCK = unchecked((int)0x800200db);
		public const int ERROR_KERNEL_FAILED_ALLOC_HEAPBLOCK = unchecked((int)0x800200dc);
		public const int ERROR_KERNEL_FAILED_ALLOC_HEAP = unchecked((int)0x800200dd);
		public const int ERROR_KERNEL_ILLEGAL_CHUNK_ID = unchecked((int)0x800200de);
		public const int ERROR_KERNEL_CANNOT_FIND_CHUNK_NAME = unchecked((int)0x800200df);
		public const int ERROR_KERNEL_NO_FREE_CHUNK = unchecked((int)0x800200e0);
		public const int ERROR_KERNEL_MEMBLOCK_FRAGMENTED = unchecked((int)0x800200e1);
		public const int ERROR_KERNEL_MEMBLOCK_CANNOT_JOINT = unchecked((int)0x800200e2);
		public const int ERROR_KERNEL_MEMBLOCK_CANNOT_SEPARATE = unchecked((int)0x800200e3);
		public const int ERROR_KERNEL_ILLEGAL_ALIGNMENT_SIZE = unchecked((int)0x800200e4);
		public const int ERROR_KERNEL_ILLEGAL_DEVKIT_VER = unchecked((int)0x800200e5);

		public const int ERROR_KERNEL_MODULE_LINK_ERROR = unchecked((int)0x8002012c);
		public const int ERROR_KERNEL_ILLEGAL_OBJECT_FORMAT = unchecked((int)0x8002012d);
		public const int ERROR_KERNEL_UNKNOWN_MODULE = unchecked((int)0x8002012e);
		public const int ERROR_KERNEL_UNKNOWN_MODULE_FILE = unchecked((int)0x8002012f);
		public const int ERROR_KERNEL_FILE_READ_ERROR = unchecked((int)0x80020130);
		public const int ERROR_KERNEL_MEMORY_IN_USE = unchecked((int)0x80020131);
		public const int ERROR_KERNEL_PARTITION_MISMATCH = unchecked((int)0x80020132);
		public const int ERROR_KERNEL_MODULE_ALREADY_STARTED = unchecked((int)0x80020133);
		public const int ERROR_KERNEL_MODULE_NOT_STARTED = unchecked((int)0x80020134);
		public const int ERROR_KERNEL_MODULE_ALREADY_STOPPED = unchecked((int)0x80020135);
		public const int ERROR_KERNEL_MODULE_CANNOT_STOP = unchecked((int)0x80020136);
		public const int ERROR_KERNEL_MODULE_NOT_STOPPED = unchecked((int)0x80020137);
		public const int ERROR_KERNEL_MODULE_CANNOT_REMOVE = unchecked((int)0x80020138);
		public const int ERROR_KERNEL_EXCLUSIVE_LOAD = unchecked((int)0x80020139);
		public const int ERROR_KERNEL_LIBRARY_IS_NOT_LINKED = unchecked((int)0x8002013a);
		public const int ERROR_KERNEL_LIBRARY_ALREADY_EXISTS = unchecked((int)0x8002013b);
		public const int ERROR_KERNEL_LIBRARY_NOT_FOUND = unchecked((int)0x8002013c);
		public const int ERROR_KERNEL_ILLEGAL_LIBRARY_HEADER = unchecked((int)0x8002013d);
		public const int ERROR_KERNEL_LIBRARY_IN_USE = unchecked((int)0x8002013e);
		public const int ERROR_KERNEL_MODULE_ALREADY_STOPPING = unchecked((int)0x8002013f);
		public const int ERROR_KERNEL_ILLEGAL_OFFSET_VALUE = unchecked((int)0x80020140);
		public const int ERROR_KERNEL_ILLEGAL_POSITION_CODE = unchecked((int)0x80020141);
		public const int ERROR_KERNEL_ILLEGAL_ACCESS_CODE = unchecked((int)0x80020142);
		public const int ERROR_KERNEL_MODULE_MANAGER_BUSY = unchecked((int)0x80020143);
		public const int ERROR_KERNEL_ILLEGAL_FLAG = unchecked((int)0x80020144);
		public const int ERROR_KERNEL_CANNOT_GET_MODULE_LIST = unchecked((int)0x80020145);
		public const int ERROR_KERNEL_PROHIBIT_LOADMODULE_DEVICE = unchecked((int)0x80020146);
		public const int ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE = unchecked((int)0x80020147);
		public const int ERROR_KERNEL_UNSUPPORTED_PRX_TYPE = unchecked((int)0x80020148);
		public const int ERROR_KERNEL_ILLEGAL_PERMISSION_CALL = unchecked((int)0x80020149);
		public const int ERROR_KERNEL_CANNOT_GET_MODULE_INFO = unchecked((int)0x8002014a);
		public const int ERROR_KERNEL_ILLEGAL_LOADEXEC_BUFFER = unchecked((int)0x8002014b);
		public const int ERROR_KERNEL_ILLEGAL_LOADEXEC_FILENAME = unchecked((int)0x8002014c);
		public const int ERROR_KERNEL_NO_EXIT_CALLBACK = unchecked((int)0x8002014d);
		public const int ERROR_KERNEL_MEDIA_CHANGED = unchecked((int)0x8002014e);
		public const int ERROR_KERNEL_CANNOT_USE_BETA_VER_MODULE = unchecked((int)0x8002014f);

		public const int ERROR_KERNEL_NO_MEMORY = unchecked((int)0x80020190);
		public const int ERROR_KERNEL_ILLEGAL_ATTR = unchecked((int)0x80020191);
		public const int ERROR_KERNEL_ILLEGAL_THREAD_ENTRY_ADDR = unchecked((int)0x80020192);
		public const int ERROR_KERNEL_ILLEGAL_PRIORITY = unchecked((int)0x80020193);
		public const int ERROR_KERNEL_ILLEGAL_STACK_SIZE = unchecked((int)0x80020194);
		public const int ERROR_KERNEL_ILLEGAL_MODE = unchecked((int)0x80020195);
		public const int ERROR_KERNEL_ILLEGAL_MASK = unchecked((int)0x80020196);
		public const int ERROR_KERNEL_ILLEGAL_THREAD = unchecked((int)0x80020197);
		public const int ERROR_KERNEL_NOT_FOUND_THREAD = unchecked((int)0x80020198);
		public const int ERROR_KERNEL_NOT_FOUND_SEMAPHORE = unchecked((int)0x80020199);
		public const int ERROR_KERNEL_NOT_FOUND_EVENT_FLAG = unchecked((int)0x8002019a);
		public const int ERROR_KERNEL_NOT_FOUND_MESSAGE_BOX = unchecked((int)0x8002019b);
		public const int ERROR_KERNEL_NOT_FOUND_VPOOL = unchecked((int)0x8002019c);
		public const int ERROR_KERNEL_NOT_FOUND_FPOOL = unchecked((int)0x8002019d);
		public const int ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE = unchecked((int)0x8002019e);
		public const int ERROR_KERNEL_NOT_FOUND_ALARM = unchecked((int)0x8002019f);
		public const int ERROR_KERNEL_NOT_FOUND_THREAD_EVENT_HANDLER = unchecked((int)0x800201a0);
		public const int ERROR_KERNEL_NOT_FOUND_CALLBACK = unchecked((int)0x800201a1);
		public const int ERROR_KERNEL_THREAD_ALREADY_DORMANT = unchecked((int)0x800201a2);
		public const int ERROR_KERNEL_THREAD_ALREADY_SUSPEND = unchecked((int)0x800201a3);
		public const int ERROR_KERNEL_THREAD_IS_NOT_DORMANT = unchecked((int)0x800201a4);
		public const int ERROR_KERNEL_THREAD_IS_NOT_SUSPEND = unchecked((int)0x800201a5);
		public const int ERROR_KERNEL_THREAD_IS_NOT_WAIT = unchecked((int)0x800201a6);
		public const int ERROR_KERNEL_WAIT_CAN_NOT_WAIT = unchecked((int)0x800201a7);
		public const int ERROR_KERNEL_WAIT_TIMEOUT = unchecked((int)0x800201a8);
		public const int ERROR_KERNEL_WAIT_CANCELLED = unchecked((int)0x800201a9);
		public const int ERROR_KERNEL_WAIT_STATUS_RELEASED = unchecked((int)0x800201aa);
		public const int ERROR_KERNEL_WAIT_STATUS_RELEASED_CALLBACK = unchecked((int)0x800201ab);
		public const int ERROR_KERNEL_THREAD_IS_TERMINATED = unchecked((int)0x800201ac);
		public const int ERROR_KERNEL_SEMA_ZERO = unchecked((int)0x800201ad);
		public const int ERROR_KERNEL_SEMA_OVERFLOW = unchecked((int)0x800201ae);
		public const int ERROR_KERNEL_EVENT_FLAG_POLL_FAILED = unchecked((int)0x800201af);
		public const int ERROR_KERNEL_EVENT_FLAG_NO_MULTI_PERM = unchecked((int)0x800201b0);
		public const int ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN = unchecked((int)0x800201b1);
		public const int ERROR_KERNEL_MESSAGEBOX_NO_MESSAGE = unchecked((int)0x800201b2);
		public const int ERROR_KERNEL_MESSAGE_PIPE_FULL = unchecked((int)0x800201b3);
		public const int ERROR_KERNEL_MESSAGE_PIPE_EMPTY = unchecked((int)0x800201b4);
		public const int ERROR_KERNEL_WAIT_DELETE = unchecked((int)0x800201b5);
		public const int ERROR_KERNEL_ILLEGAL_MEMBLOCK = unchecked((int)0x800201b6);
		public const int ERROR_KERNEL_ILLEGAL_MEMSIZE = unchecked((int)0x800201b7);
		public const int ERROR_KERNEL_ILLEGAL_SCRATCHPAD_ADDR = unchecked((int)0x800201b8);
		public const int ERROR_KERNEL_SCRATCHPAD_IN_USE = unchecked((int)0x800201b9);
		public const int ERROR_KERNEL_SCRATCHPAD_NOT_IN_USE = unchecked((int)0x800201ba);
		public const int ERROR_KERNEL_ILLEGAL_TYPE = unchecked((int)0x800201bb);
		public const int ERROR_KERNEL_ILLEGAL_SIZE = unchecked((int)0x800201bc);
		public const int ERROR_KERNEL_ILLEGAL_COUNT = unchecked((int)0x800201bd);
		public const int ERROR_KERNEL_NOT_FOUND_VTIMER = unchecked((int)0x800201be);
		public const int ERROR_KERNEL_ILLEGAL_VTIMER = unchecked((int)0x800201bf);
		public const int ERROR_KERNEL_ILLEGAL_KTLS = unchecked((int)0x800201c0);
		public const int ERROR_KERNEL_KTLS_IS_FULL = unchecked((int)0x800201c1);
		public const int ERROR_KERNEL_KTLS_IS_BUSY = unchecked((int)0x800201c2);
		public const int ERROR_KERNEL_MUTEX_NOT_FOUND = unchecked((int)0x800201c3);
		public const int ERROR_KERNEL_MUTEX_LOCKED = unchecked((int)0x800201c4);
		public const int ERROR_KERNEL_MUTEX_UNLOCKED = unchecked((int)0x800201c5);
		public const int ERROR_KERNEL_MUTEX_LOCK_OVERFLOW = unchecked((int)0x800201c6);
		public const int ERROR_KERNEL_MUTEX_UNLOCK_UNDERFLOW = unchecked((int)0x800201c7);
		public const int ERROR_KERNEL_MUTEX_RECURSIVE_NOT_ALLOWED = unchecked((int)0x800201c8);
		public const int ERROR_KERNEL_MESSAGEBOX_DUPLICATE_MESSAGE = unchecked((int)0x800201c9);
		public const int ERROR_KERNEL_LWMUTEX_NOT_FOUND = unchecked((int)0x800201ca);
		public const int ERROR_KERNEL_LWMUTEX_LOCKED = unchecked((int)0x800201cb);
		public const int ERROR_KERNEL_LWMUTEX_UNLOCKED = unchecked((int)0x800201cc);
		public const int ERROR_KERNEL_LWMUTEX_LOCK_OVERFLOW = unchecked((int)0x800201cd);
		public const int ERROR_KERNEL_LWMUTEX_UNLOCK_UNDERFLOW = unchecked((int)0x800201ce);
		public const int ERROR_KERNEL_LWMUTEX_RECURSIVE_NOT_ALLOWED = unchecked((int)0x800201cf);

		public const int ERROR_KERNEL_POWER_CANNOT_CANCEL = unchecked((int)0x80020261);

		public const int ERROR_KERNEL_TOO_MANY_OPEN_FILES = unchecked((int)0x80020320);
		public const int ERROR_KERNEL_NO_SUCH_DEVICE = unchecked((int)0x80020321);
		public const int ERROR_KERNEL_BAD_FILE_DESCRIPTOR = unchecked((int)0x80020323);
		public const int ERROR_KERNEL_UNSUPPORTED_OPERATION = unchecked((int)0x80020325);
		public const int ERROR_KERNEL_NOCWD = unchecked((int)0x8002032c);
		public const int ERROR_KERNEL_FILENAME_TOO_LONG = unchecked((int)0x8002032d);
		public const int ERROR_KERNEL_ASYNC_BUSY = unchecked((int)0x80020329);
		public const int ERROR_KERNEL_NO_ASYNC_OP = unchecked((int)0x8002032a);

		public const int ERROR_KERNEL_NOT_CACHE_ALIGNED = unchecked((int)0x8002044c);
		public const int ERROR_KERNEL_MAX_ERROR = unchecked((int)0x8002044d);

		public const int ERROR_REGISTRY_NOT_FOUND = unchecked((int)0x80082718);

		public const int ERROR_UTILITY_INVALID_STATUS = unchecked((int)0x80110001);
		public const int ERROR_UTILITY_INVALID_PARAM_ADDR = unchecked((int)0x80110002);
		public const int ERROR_UTILITY_IS_UNKNOWN = unchecked((int)0x80110003);
		public const int ERROR_UTILITY_INVALID_PARAM_SIZE = unchecked((int)0x80110004);
		public const int ERROR_UTILITY_WRONG_TYPE = unchecked((int)0x80110005);
		public const int ERROR_UTILITY_MODULE_NOT_FOUND = unchecked((int)0x80110006);

		public const int ERROR_UTILITY_INVALID_SYSTEM_PARAM_ID = unchecked((int)0x80110103);
		public const int ERROR_UTILITY_INVALID_ADHOC_CHANNEL = unchecked((int)0x80110104);

		public const int ERROR_SAVEDATA_LOAD_NO_MEMSTICK = unchecked((int)0x80110301);
		public const int ERROR_SAVEDATA_LOAD_MEMSTICK_REMOVED = unchecked((int)0x80110302);
		public const int ERROR_SAVEDATA_LOAD_ACCESS_ERROR = unchecked((int)0x80110305);
		public const int ERROR_SAVEDATA_LOAD_DATA_BROKEN = unchecked((int)0x80110306);
		public const int ERROR_SAVEDATA_LOAD_NO_DATA = unchecked((int)0x80110307);
		public const int ERROR_SAVEDATA_LOAD_BAD_PARAMS = unchecked((int)0x80110308);
		public const int ERROR_SAVEDATA_LOAD_NO_UMD = unchecked((int)0x80110309);
		public const int ERROR_SAVEDATA_LOAD_INTERNAL_ERROR = unchecked((int)0x80110309);

		public const int ERROR_SAVEDATA_RW_NO_MEMSTICK = unchecked((int)0x80110321);
		public const int ERROR_SAVEDATA_RW_MEMSTICK_REMOVED = unchecked((int)0x80110322);
		public const int ERROR_SAVEDATA_RW_MEMSTICK_FULL = unchecked((int)0x80110323);
		public const int ERROR_SAVEDATA_RW_MEMSTICK_PROTECTED = unchecked((int)0x80110324);
		public const int ERROR_SAVEDATA_RW_ACCESS_ERROR = unchecked((int)0x80110325);
		public const int ERROR_SAVEDATA_RW_DATA_BROKEN = unchecked((int)0x80110326);
		public const int ERROR_SAVEDATA_RW_NO_DATA = unchecked((int)0x80110327);
		public const int ERROR_SAVEDATA_RW_BAD_PARAMS = unchecked((int)0x80110328);
		public const int ERROR_SAVEDATA_RW_FILE_NOT_FOUND = unchecked((int)0x80110329);
		public const int ERROR_SAVEDATA_RW_CAN_NOT_SUSPEND = unchecked((int)0x8011032a);
		public const int ERROR_SAVEDATA_RW_INTERNAL_ERROR = unchecked((int)0x8011032b);
		public const int ERROR_SAVEDATA_RW_BAD_STATUS = unchecked((int)0x8011032c);
		public const int ERROR_SAVEDATA_RW_SECURE_FILE_FULL = unchecked((int)0x8011032d);

		public const int ERROR_SAVEDATA_DELETE_NO_MEMSTICK = unchecked((int)0x80110341);
		public const int ERROR_SAVEDATA_DELETE_MEMSTICK_REMOVED = unchecked((int)0x80110342);
		public const int ERROR_SAVEDATA_DELETE_MEMSTICK_PROTECTED = unchecked((int)0x80110344);
		public const int ERROR_SAVEDATA_DELETE_ACCESS_ERROR = unchecked((int)0x80110345);
		public const int ERROR_SAVEDATA_DELETE_DATA_BROKEN = unchecked((int)0x80110346);
		public const int ERROR_SAVEDATA_DELETE_NO_DATA = unchecked((int)0x80110347);
		public const int ERROR_SAVEDATA_DELETE_BAD_PARAMS = unchecked((int)0x80110348);
		public const int ERROR_SAVEDATA_DELETE_INTERNAL_ERROR = unchecked((int)0x8011034b);

		public const int ERROR_SAVEDATA_SAVE_NO_MEMSTICK = unchecked((int)0x80110381);
		public const int ERROR_SAVEDATA_SAVE_MEMSTICK_REMOVED = unchecked((int)0x80110382);
		public const int ERROR_SAVEDATA_SAVE_NO_SPACE = unchecked((int)0x80110383);
		public const int ERROR_SAVEDATA_SAVE_MEMSTICK_PROTECTED = unchecked((int)0x80110384);
		public const int ERROR_SAVEDATA_SAVE_ACCESS_ERROR = unchecked((int)0x80110385);
		public const int ERROR_SAVEDATA_SAVE_DATA_BROKEN = unchecked((int)0x80110386);
		public const int ERROR_SAVEDATA_SAVE_BAD_PARAMS = unchecked((int)0x80110388);
		public const int ERROR_SAVEDATA_SAVE_NO_UMD = unchecked((int)0x80110389);
		public const int ERROR_SAVEDATA_SAVE_WRONG_UMD = unchecked((int)0x8011038a);
		public const int ERROR_SAVEDATA_SAVE_INTERNAL_ERROR = unchecked((int)0x8011038b);

		public const int ERROR_SAVEDATA_SIZES_NO_MEMSTICK = unchecked((int)0x801103c1);
		public const int ERROR_SAVEDATA_SIZES_MEMSTICK_REMOVED = unchecked((int)0x801103c2);
		public const int ERROR_SAVEDATA_SIZES_ACCESS_ERROR = unchecked((int)0x801103c5);
		public const int ERROR_SAVEDATA_SIZES_DATA_BROKEN = unchecked((int)0x801103c6);
		public const int ERROR_SAVEDATA_SIZES_NO_DATA = unchecked((int)0x801103c7);
		public const int ERROR_SAVEDATA_SIZES_BAD_PARAMS = unchecked((int)0x801103c8);
		public const int ERROR_SAVEDATA_SIZES_INTERNAL_ERROR = unchecked((int)0x801103cb);

		public const int ERROR_NETPARAM_BAD_NETCONF = unchecked((int)0x80110601);
		public const int ERROR_NETPARAM_BAD_PARAM = unchecked((int)0x80110604);

		public const int ERROR_NET_MODULE_BAD_ID = unchecked((int)0x80110801);
		public const int ERROR_NET_MODULE_ALREADY_LOADED = unchecked((int)0x80110802);
		public const int ERROR_NET_MODULE_NOT_LOADED = unchecked((int)0x80110803);

		public const int ERROR_AV_MODULE_BAD_ID = unchecked((int)0x80110F01);
		public const int ERROR_AV_MODULE_ALREADY_LOADED = unchecked((int)0x80110F02);
		public const int ERROR_AV_MODULE_NOT_LOADED = unchecked((int)0x80110F03);

		public const int ERROR_MODULE_BAD_ID = unchecked((int)0x80111101);
		public const int ERROR_MODULE_ALREADY_LOADED = unchecked((int)0x80111102);
		public const int ERROR_MODULE_NOT_LOADED = unchecked((int)0x80111103);

		public const int ERROR_SCREENSHOT_CONT_MODE_NOT_INIT = unchecked((int)0x80111229);

		public const int ERROR_UMD_NOT_READY = unchecked((int)0x80210001);
		public const int ERROR_UMD_LBA_OUT_OF_BOUNDS = unchecked((int)0x80210002);
		public const int ERROR_UMD_NO_DISC = unchecked((int)0x80210003);

		public const int ERROR_MEMSTICK_DEVCTL_BAD_PARAMS = unchecked((int)0x80220081);
		public const int ERROR_MEMSTICK_DEVCTL_TOO_MANY_CALLBACKS = unchecked((int)0x80220082);

		public const int ERROR_USBMIC_INVALID_MAX_SAMPLES = unchecked((int)0x80243806);
		public const int ERROR_USBMIC_INVALID_FREQUENCY = unchecked((int)0x8024380A);

		public const int ERROR_USBCAM_NOT_READY = unchecked((int)0x80243902);
		public const int ERROR_USBCAM_NO_READ_ON_VIDEO_FRAME = unchecked((int)0x8024390C);
		public const int ERROR_USBCAM_NO_VIDEO_FRAME_AVAILABLE = unchecked((int)0x8024390E);

		public const int ERROR_AUDIO_CHANNEL_NOT_INIT = unchecked((int)0x80260001);
		public const int ERROR_AUDIO_CHANNEL_BUSY = unchecked((int)0x80260002);
		public const int ERROR_AUDIO_INVALID_CHANNEL = unchecked((int)0x80260003);
		public const int ERROR_AUDIO_PRIV_REQUIRED = unchecked((int)0x80260004);
		public const int ERROR_AUDIO_NO_CHANNELS_AVAILABLE = unchecked((int)0x80260005);
		public const int ERROR_AUDIO_OUTPUT_SAMPLE_DATA_SIZE_NOT_ALIGNED = unchecked((int)0x80260006);
		public const int ERROR_AUDIO_INVALID_FORMAT = unchecked((int)0x80260007);
		public const int ERROR_AUDIO_CHANNEL_NOT_RESERVED = unchecked((int)0x80260008);
		public const int ERROR_AUDIO_NOT_OUTPUT = unchecked((int)0x80260009);
		public const int ERROR_AUDIO_INVALID_FREQUENCY = unchecked((int)0x8026000A);
		public const int ERROR_AUDIO_INVALID_VOLUME = unchecked((int)0x8026000B);
		public const int ERROR_AUDIO_CHANNEL_ALREADY_RESERVED = unchecked((int)0x80268002);

		public const int ERROR_POWER_VMEM_IN_USE = unchecked((int)0x802b0200);

		public const int ERROR_NET_BUFFER_TOO_SMALL = unchecked((int)0x80400706);

		public const int ERROR_NET_NO_SPACE = unchecked((int)0x80410001);

		public const int ERROR_NET_NO_EVENT = unchecked((int)0x80410184);

		public const int ERROR_NET_RESOLVER_BAD_ID = unchecked((int)0x80410408);
		public const int ERROR_NET_RESOLVER_ALREADY_STOPPED = unchecked((int)0x8041040a);
		public const int ERROR_NET_RESOLVER_INVALID_HOST = unchecked((int)0x80410414);

		public const int ERROR_NET_ADHOC_INVALID_SOCKET_ID = unchecked((int)0x80410701);
		public const int ERROR_NET_ADHOC_INVALID_ADDR = unchecked((int)0x80410702);
		public const int ERROR_NET_ADHOC_NO_DATA_AVAILABLE = unchecked((int)0x80410709);
		public const int ERROR_NET_ADHOC_PORT_IN_USE = unchecked((int)0x8041070a);
		public const int ERROR_NET_ADHOC_INVALID_ARG = unchecked((int)0x80410711);
		public const int ERROR_NET_ADHOC_NOT_INITIALIZED = unchecked((int)0x80410712);
		public const int ERROR_NET_ADHOC_ALREADY_INITIALIZED = unchecked((int)0x80410713);
		public const int ERROR_NET_ADHOC_DISCONNECTED = unchecked((int)0x8041070c);
		public const int ERROR_NET_ADHOC_TIMEOUT = unchecked((int)0x80410715);
		public const int ERROR_NET_ADHOC_NO_ENTRY = unchecked((int)0x80410716);
		public const int ERROR_NET_ADHOC_CONNECTION_REFUSED = unchecked((int)0x80410718);

		public const int ERROR_NET_ADHOC_INVALID_MATCHING_ID = unchecked((int)0x80410807);
		public const int ERROR_NET_ADHOC_MATCHING_ALREADY_INITIALIZED = unchecked((int)0x80410812);
		public const int ERROR_NET_ADHOC_MATCHING_NOT_INITIALIZED = unchecked((int)0x80410813);

		public const int ERROR_NET_ADHOCCTL_INVALID_PARAMETER = unchecked((int)0x80410b04);
		public const int ERROR_NET_ADHOCCTL_ALREADY_INITIALIZED = unchecked((int)0x80410b07);
		public const int ERROR_NET_ADHOCCTL_NOT_INITIALIZED = unchecked((int)0x80410b08);
		public const int ERROR_NET_ADHOCCTL_TOO_MANY_HANDLERS = unchecked((int)0x80410b12);

		public const int ERROR_WLAN_BAD_PARAMS = unchecked((int)0x80410d13);
		public const int ERROR_WLAN_NOT_IN_GAMEMODE = unchecked((int)0x80410d14);

		public const int ERROR_SAS_INVALID_GRAIN = unchecked((int)0x80420001);
		public const int ERROR_SAS_INVALID_MAX_VOICES = unchecked((int)0x80420002);
		public const int ERROR_SAS_INVALID_OUTPUT_MODE = unchecked((int)0x80420003);
		public const int ERROR_SAS_INVALID_SAMPLE_RATE = unchecked((int)0x80420004);
		public const int ERROR_SAS_INVALID_ADDRESS = unchecked((int)0x80420005);
		public const int ERROR_SAS_INVALID_VOICE_INDEX = unchecked((int)0x80420010);
		public const int ERROR_SAS_INVALID_NOISE_CLOCK = unchecked((int)0x80420011);
		public const int ERROR_SAS_INVALID_PITCH_VAL = unchecked((int)0x80420012);
		public const int ERROR_SAS_INVALID_ADSR_CURVE_MODE = unchecked((int)0x80420013);
		public const int ERROR_SAS_INVALID_ADPCM_SIZE = unchecked((int)0x80420014);
		public const int ERROR_SAS_INVALID_LOOP_MODE = unchecked((int)0x80420015);
		public const int ERROR_SAS_VOICE_PAUSED = unchecked((int)0x80420016);
		public const int ERROR_SAS_INVALID_VOLUME_VAL = unchecked((int)0x80420018);
		public const int ERROR_SAS_INVALID_ADSR_VAL = unchecked((int)0x80420019);
		public const int ERROR_SAS_INVALID_SIZE = unchecked((int)0x8042001A);
		public const int ERROR_SAS_INVALID_FX_TYPE = unchecked((int)0x80420020);
		public const int ERROR_SAS_INVALID_FX_FEEDBACK = unchecked((int)0x80420021);
		public const int ERROR_SAS_INVALID_FX_DELAY = unchecked((int)0x80420022);
		public const int ERROR_SAS_INVALID_FX_VOLUME_VAL = unchecked((int)0x80420023);
		public const int ERROR_SAS_BUSY = unchecked((int)0x80420030);
		public const int ERROR_SAS_CANNOT_CONCATENATE_ATRA3 = unchecked((int)0x80420042);
		public const int ERROR_SAS_NOT_INIT = unchecked((int)0x80420100);
		public const int ERROR_SAS_ALREADY_INIT = unchecked((int)0x80420101);

		public const int ERROR_HTTP_NOT_INIT = unchecked((int)0x80431001);
		public const int ERROR_HTTP_ALREADY_INIT = unchecked((int)0x80431020);
		public const int ERROR_HTTP_NOT_FOUND = unchecked((int)0x80431025);
		public const int ERROR_HTTP_NO_CONTENT_LENGTH = unchecked((int)0x80431071);
		public const int ERROR_HTTP_NO_MEMORY = unchecked((int)0x80431077);
		public const int ERROR_HTTP_SYSTEM_COOKIE_NOT_LOADED = unchecked((int)0x80431078);
		public const int ERROR_HTTP_INVALID_PARAMETER = unchecked((int)0x804311FE);

		public const int ERROR_PARSE_HTTP_NOT_FOUND = unchecked((int)0x80432025);

		public const int ERROR_SSL_NOT_INIT = unchecked((int)0x80435001);
		public const int ERROR_SSL_ALREADY_INIT = unchecked((int)0x80435020);
		public const int ERROR_SSL_OUT_OF_MEMORY = unchecked((int)0x80435022);
		public const int ERROR_HTTPS_CERT_ERROR = unchecked((int)0x80435060);
		public const int ERROR_HTTPS_HANDSHAKE_ERROR = unchecked((int)0x80435061);
		public const int ERROR_HTTPS_IO_ERROR = unchecked((int)0x80435062);
		public const int ERROR_HTTPS_INTERNAL_ERROR = unchecked((int)0x80435063);
		public const int ERROR_HTTPS_PROXY_ERROR = unchecked((int)0x80435064);
		public const int ERROR_SSL_INVALID_PARAMETER = unchecked((int)0x804351FE);

		public const int ERROR_WAVE_NOT_INIT = unchecked((int)0x80440001);
		public const int ERROR_WAVE_FAILED_EXIT = unchecked((int)0x80440002);
		public const int ERROR_WAVE_BAD_VOL = unchecked((int)0x8044000a);
		public const int ERROR_WAVE_INVALID_CHANNEL = unchecked((int)0x80440010);
		public const int ERROR_WAVE_INVALID_SAMPLE_COUNT = unchecked((int)0x80440011);

		public const int ERROR_FONT_OUT_OF_MEMORY = unchecked((int)0x80460001);
		public const int ERROR_FONT_INVALID_LIBID = unchecked((int)0x80460002);
		public const int ERROR_FONT_INVALID_PARAMETER = unchecked((int)0x80460003);
		public const int ERROR_FONT_FILE_NOT_FOUND = unchecked((int)0x80460005);
		public const int ERROR_FONT_TOO_MANY_OPEN_FONTS = unchecked((int)0x80460009);

		public const int ERROR_PGD_INVALID_HEADER = unchecked((int)0x80510204);
		public const int ERROR_PGD_INVALID_DATA = unchecked((int)0x80510207);

		public const int ERROR_NPAUTH_NOT_INIT = unchecked((int)0x80550302);
		public const int ERROR_NPSERVICE_NOT_INIT = unchecked((int)0x80550502);
		public const int ERROR_NP_MANAGER_INVALID_ARGUMENT = unchecked((int)0x80550503);

		public const int ERROR_NPDRM_INVALID_FILE = unchecked((int)0x80550901);
		public const int ERROR_NPDRM_INTERNAL_ERROR = unchecked((int)0x80550902);
		public const int ERROR_NPDRM_INVALID_ACT_SIGN = unchecked((int)0x80550903);
		public const int ERROR_NPDRM_INVALID_RIF_SIGN = unchecked((int)0x80550904);
		public const int ERROR_NPDRM_DIFF_ACC_ID = unchecked((int)0x80550905);
		public const int ERROR_NPDRM_WRONG_VERSION = unchecked((int)0x80550906);
		public const int ERROR_NPDRM_FILE_ERROR = unchecked((int)0x80550907);
		public const int ERROR_NPDRM_BAD_MEDIA_ID = unchecked((int)0x80550908);
		public const int ERROR_NPDRM_BAD_PRODUCT_ID = unchecked((int)0x80550909);
		public const int ERROR_NPDRM_NO_RIF = unchecked((int)0x80550910);
		public const int ERROR_NPDRM_NO_ACT = unchecked((int)0x80550911);
		public const int ERROR_NPDRM_INVALID_PERM = unchecked((int)0x80550912);
		public const int ERROR_NPDRM_INVALID_FILE_FORMAT = unchecked((int)0x80550913);
		public const int ERROR_NPDRM_TIME_SERVICE_ENDED = unchecked((int)0x80550914);
		public const int ERROR_NPDRM_TIME_SERVICE_NOT_STARTED = unchecked((int)0x80550915);
		public const int ERROR_NPDRM_NO_K_LICENSEE_SET = unchecked((int)0x80550916);
		public const int ERROR_NPDRM_NO_FILENAME_MATCH = unchecked((int)0x80550917);

		public const int ERROR_LIB_UPDATE_LATEST_VERSION_INSTALLED = unchecked((int)0x805F0004);

		public const int ERROR_MPEG_BAD_VERSION = unchecked((int)0x80610002);
		public const int ERROR_MPEG_NO_MEMORY = unchecked((int)0x80610022);
		public const int ERROR_MPEG_INVALID_ADDR = unchecked((int)0x80610103);
		public const int ERROR_MPEG_INVALID_VALUE = unchecked((int)0x806101fe);

		public const int ERROR_PSMF_NOT_INITIALIZED = unchecked((int)0x80615001);
		public const int ERROR_PSMF_BAD_VERSION = unchecked((int)0x80615002);
		public const int ERROR_PSMF_NOT_FOUND = unchecked((int)0x80615025);
		public const int ERROR_PSMF_INVALID_ID = unchecked((int)0x80615100);
		public const int ERROR_PSMF_INVALID_VALUE = unchecked((int)0x806151fe);
		public const int ERROR_PSMF_INVALID_TIMESTAMP = unchecked((int)0x80615500);
		public const int ERROR_PSMF_INVALID_PSMF = unchecked((int)0x80615501);

		public const int ERROR_PSMFPLAYER_NOT_INITIALIZED = unchecked((int)0x80616001);
		public const int ERROR_PSMFPLAYER_INVALID_CONFIG_MODE = unchecked((int)0x80616006);
		public const int ERROR_PSMFPLAYER_INVALID_CONFIG_VALUE = unchecked((int)0x80616008);
		public const int ERROR_PSMFPLAYER_AUDIO_VIDEO_OUT_OF_SYNC = unchecked((int)0x8061600c);

		public const int ERROR_MP4_INVALID_VALUE = unchecked((int)0x80617003);
		public const int ERROR_MP4_INVALID_SAMPLE_NUMBER = unchecked((int)0x80617006);
		public const int ERROR_MP4_NO_AVAILABLE_SIZE = unchecked((int)0x80617009);
		public const int ERROR_MP4_NO_MORE_DATA = unchecked((int)0x8061700a);
		public const int ERROR_MP4_AAC_DECODE_ERROR = unchecked((int)0x80617141);

		public const int ERROR_MPEG_NO_DATA = unchecked((int)0x80618001);
		public const int ERROR_MPEG_UNKNOWN_STREAM_ID = unchecked((int)0x80618009);

		public const int ERROR_AVC_NO_IMAGE_AVAILABLE = unchecked((int)0x806201FE);
		public const int ERROR_AVC_VIDEO_FATAL = unchecked((int)0x80628002);

		public const int ERROR_ATRAC_PARAM_FAIL = unchecked((int)0x80630001);
		public const int ERROR_ATRAC_API_FAIL = unchecked((int)0x80630002);
		public const int ERROR_ATRAC_NO_ID = unchecked((int)0x80630003);
		public const int ERROR_ATRAC_INVALID_CODEC = unchecked((int)0x80630004);
		public const int ERROR_ATRAC_BAD_ID = unchecked((int)0x80630005);
		public const int ERROR_ATRAC_UNKNOWN_FORMAT = unchecked((int)0x80630006);
		public const int ERROR_ATRAC_WRONG_CODEC = unchecked((int)0x80630007);
		public const int ERROR_ATRAC_BAD_DATA = unchecked((int)0x80630008);
		public const int ERROR_ATRAC_ALL_DATA_LOADED = unchecked((int)0x80630009);
		public const int ERROR_ATRAC_NO_DATA = unchecked((int)0x80630010);
		public const int ERROR_ATRAC_INVALID_SIZE = unchecked((int)0x80630011);
		public const int ERROR_ATRAC_SECOND_BUFFER_NEEDED = unchecked((int)0x80630012);
		public const int ERROR_ATRAC_INCORRECT_READ_SIZE = unchecked((int)0x80630013);
		public const int ERROR_ATRAC_NOT_4BYTE_ALIGNMENT = unchecked((int)0x80630014);
		public const int ERROR_ATRAC_BAD_SAMPLE = unchecked((int)0x80630015);
		public const int ERROR_ATRAC_WRITEBYTE_FIRST_BUFFER = unchecked((int)0x80630016);
		public const int ERROR_ATRAC_WRITEBYTE_SECOND_BUFFER = unchecked((int)0x80630017);
		public const int ERROR_ATRAC_ADD_DATA_IS_TOO_BIG = unchecked((int)0x80630018);
		public const int ERROR_ATRAC_NO_LOOP_INFORMATION = unchecked((int)0x80630021);
		public const int ERROR_ATRAC_SECOND_BUFFER_NOT_NEEDED = unchecked((int)0x80630022);
		public const int ERROR_ATRAC_BUFFER_IS_EMPTY = unchecked((int)0x80630023);
		public const int ERROR_ATRAC_ALL_DATA_DECODED = unchecked((int)0x80630024);

		public const int ERROR_AA3_INVALID_HEADER_VERSION = unchecked((int)0x80631002);
		public const int ERROR_AA3_INVALID_HEADER = unchecked((int)0x80631003);
		public const int ERROR_AA3_INVALID_CODEC = unchecked((int)0x80631004);
		public const int ERROR_AA3_INVALID_HEADER_FLAGS = unchecked((int)0x80631005);

		public const int ERROR_MP3_INVALID_ID = unchecked((int)0x80671001);
		public const int ERROR_MP3_INVALID_ADDRESS = unchecked((int)0x80671002);
		public const int ERROR_MP3_INVALID_PARAMETER = unchecked((int)0x80671003);
		public const int ERROR_MP3_ID_NOT_RESERVED = unchecked((int)0x80671103);
		public const int ERROR_MP3_DECODING_ERROR = unchecked((int)0x80671402);
		public const int ERROR_MP3_LOW_LEVEL_DECODING_ERROR = unchecked((int)0x80672001);

		public const int ERROR_AAC_INVALID_ID = unchecked((int)0x80691001);
		public const int ERROR_AAC_INVALID_ADDRESS = unchecked((int)0x80691002);
		public const int ERROR_AAC_INVALID_PARAMETER = unchecked((int)0x80691003);
		public const int ERROR_AAC_ID_NOT_INITIALIZED = unchecked((int)0x80691103);
		public const int ERROR_AAC_NO_MORE_FREE_ID = unchecked((int)0x80691201);
		public const int ERROR_AAC_DECODING_ERROR = unchecked((int)0x80691401);
		public const int ERROR_AAC_NOT_ENOUGH_MEMORY = unchecked((int)0x80691501);
		public const int ERROR_AAC_RESOURCE_NOT_INITIALIZED = unchecked((int)0x80691503);

		public const int ERROR_CODEC_AUDIO_EDRAM_NOT_ALLOCATED = unchecked((int)0x807f0004);
		public const int ERROR_CODEC_AUDIO_FATAL = unchecked((int)0x807f00fc);

		public const int FATAL_UMD_UNKNOWN_MEDIUM = unchecked((int)0xC0210004);
		public const int FATAL_UMD_HARDWARE_FAILURE = unchecked((int)0xC0210005);
	}
}