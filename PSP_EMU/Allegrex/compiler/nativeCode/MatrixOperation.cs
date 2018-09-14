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
namespace pspsharp.Allegrex.compiler.nativeCode
{
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MatrixOperation : AbstractNativeCodeSequence
	{
		public static void multMat4x4ByVec4(int matReg, int matOffset, int vecReg, int vecOffset, int resultReg, int resultOffset)
		{
			int mat = getRegisterValue(matReg) + matOffset;
			int vec = getRegisterValue(vecReg) + vecOffset;
			int result = getRegisterValue(resultReg) + resultOffset;

			IMemoryReader matReader = MemoryReader.getMemoryReader(mat, 64, 4);
			IMemoryReader vecReader = MemoryReader.getMemoryReader(vec, 16, 4);
			IMemoryWriter resultWriter = MemoryWriter.getMemoryWriter(result, 16, 4);

			float vec0 = Float.intBitsToFloat(vecReader.readNext());
			float vec1 = Float.intBitsToFloat(vecReader.readNext());
			float vec2 = Float.intBitsToFloat(vecReader.readNext());
			float vec3 = Float.intBitsToFloat(vecReader.readNext());

			for (int i = 0; i < 4; i++)
			{
				float mat0 = Float.intBitsToFloat(matReader.readNext());
				float mat1 = Float.intBitsToFloat(matReader.readNext());
				float mat2 = Float.intBitsToFloat(matReader.readNext());
				float mat3 = Float.intBitsToFloat(matReader.readNext());

				float res = vec0 * mat0 + vec1 * mat1 + vec2 * mat2 + vec3 * mat3;
				resultWriter.writeNext(Float.floatToRawIntBits(res));
			}
			resultWriter.flush();
		}
	}

}