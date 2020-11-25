/*
 * Sobol sequence direction vectors.
 *
 * This file contains code to create direction vectors for generating sobol
 * sequences in high dimensions. It is adapted from code on this webpage:
 *
 * http://web.maths.unsw.edu.au/~fkuo/sobol/
 *
 * From these papers:
 *
 * S. Joe and F. Y. Kuo, Remark on Algorithm 659: Implementing Sobol's quasirandom
 * sequence generator, ACM Trans. Math. Softw. 29, 49-57 (2003)
 *
 * S. Joe and F. Y. Kuo, Constructing Sobol sequences with better two-dimensional
 * projections, SIAM J. Sci. Comput. 30, 2635-2654 (2008)
 */

/* Copyright (c) 2008, Frances Y. Kuo and Stephen Joe
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *
 *     * Neither the names of the copyright holders nor the names of the
 *       University of New South Wales and the University of Waikato
 *       and its contributors may be used to endorse or promote products derived
 *       from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

 /* I adapted the following from Blender's Cycles renderer */

using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BrassRay.RayTracer
{
    public static class Sobol
    {
        public const int SobolBits = 32;

        public static MemoryHandle GetSobolDirectionVectors(int dimensions)
        {
            using var rawStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("BrassRay.RayTracer.sobol.32le.br");
            using var stream = new System.IO.Compression.BrotliStream(
                rawStream ?? throw new InvalidOperationException(), System.IO.Compression.CompressionMode.Decompress);

            var vectors = new uint[dimensions * SobolBits];

            for (var i = 0; i < SobolBits; i++)
                vectors[i] = 1u << (31 - i);  // all m's = 1

            Span<byte> sBuffer = stackalloc byte[16];
            Span<byte> mBuffer = stackalloc byte[72];

            for (var dim = 1; dim < dimensions; dim++)
            {
                stream.Read(sBuffer);
                var header = MemoryMarshal.Read<SobolHeader>(sBuffer);
                var mBytes = mBuffer.Slice(0, header.MLength * 4);
                stream.Read(mBytes);
                var m = MemoryMarshal.Cast<byte, uint>(mBytes);

                var s = (int)header.S;
                var a = header.A;

                if (SobolBits <= s)
                {
                    for (var i = 0; i < SobolBits; i++)
                        vectors[dim * SobolBits + i] = m[i] << (31 - i);
                }
                else
                {
                    for (var i = 0; i < s; i++)
                        vectors[dim * SobolBits + i] = m[i] << (31 - i);

                    for (var i = s; i < SobolBits; i++)
                    {
                        vectors[dim * SobolBits + i] =
                            vectors[dim * SobolBits + i - s] ^ (vectors[dim * SobolBits + i - s] >> s);

                        for (var k = 1; k < s; k++)
                            vectors[dim * SobolBits + i] ^= ((a >> (s - 1 - k)) & 1) * vectors[dim * SobolBits + i - k];
                    }
                }
            }

            return new Memory<uint>(vectors).Pin();
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct SobolHeader
        {
            public uint D { get; }
            public uint S { get; }
            public uint A { get; }
            public int MLength { get; }
        }

        public readonly unsafe ref struct Sequence
        {
            public Sequence(MemoryHandle vectorsHandle, int startDimension, Span<uint> indices)
            {
                _indices = indices;
                _p = (uint*)vectorsHandle.Pointer + startDimension * SobolBits;
            }

            private const int Offset = 64;
            private readonly uint* _p;
            private readonly Span<uint> _indices;

            public float Get(int dimension)
            {
                uint result = 0;
                var i = _indices[dimension] + Offset;
                _indices[dimension]++;
                for (var j = 0; i > 0; i >>= 1, j++)
                    if ((i & 1) > 0)
                        result ^= _p[dimension * SobolBits + j];

                return result * (1.0f / 0xFFFFFFFF);
            }
        }
    }
}
