﻿using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf.Crypto
{
    public class Scrypt
    {
        //https://github.com/viniciuschiele/Scrypt/blob/master/src/Scrypt/ScryptEncoder.cs
        /* Copyright 2014 Vinicius Chiele
        *
        * Licensed under the Apache License, Version 2.0 (the "License");
        * you may not use this file except in compliance with the License.
        * You may obtain a copy of the License at
        *
        * http://www.apache.org/licenses/LICENSE-2.0
        *
        * Unless required by applicable law or agreed to in writing, software
        * distributed under the License is distributed on an "AS IS" BASIS,
        * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
        * See the License for the specific language governing permissions and
        * limitations under the License.
        */

        //This is a modified version of Scrypt.Net by Vinicius Chiele. 
        //The original version has a dependency on System.Security.Cryptography which is not in Netstandard1.1
        //This is replaced with Bouncy Castle Pbkdf2
        //Also there is not entry point provided by ScryptNet on dependency.

        private static byte[] SingleIterationPBKDF2(byte[] P, byte[] S, int dkLen)
        {
            PbeParametersGenerator pGen = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            pGen.Init(P, S, 1);
            KeyParameter key = (KeyParameter)pGen.GenerateDerivedMacParameters(dkLen * 8);
            return key.GetKey();
        }

        public unsafe static byte[] CryptoScrypt(byte[] password, byte[] salt, int N, int r, int p, int dkLen)
        {
            var Ba = new byte[128 * r * p + 63];
            var XYa = new byte[256 * r + 63];
            var Va = new byte[128 * r * N + 63];
            var buf = new byte[32];

            Ba = SingleIterationPBKDF2(password, salt, p * 128 * r);

            fixed (byte* B = Ba)
            fixed (void* V = Va)
            fixed (void* XY = XYa)
            {
                /* 2: for i = 0 to p - 1 do */
                for (var i = 0; i < p; i++)
                {
                    /* 3: B_i <-- MF(B_i, N) */
                    SMix(&B[i * 128 * r], r, N, (uint*)V, (uint*)XY);
                }
            }

            /* 5: DK <-- PBKDF2(P, B, 1, dkLen) */
            return SingleIterationPBKDF2(password, Ba, dkLen);
        }

        /// <summary>
        /// Copies a specified number of bytes from a source pointer to a destination pointer.
        /// </summary>
        private unsafe static void BulkCopy(void* dst, void* src, int len)
        {
            var d = (byte*)dst;
            var s = (byte*)src;

            while (len >= 8)
            {
                *(ulong*)d = *(ulong*)s;
                d += 8;
                s += 8;
                len -= 8;
            }
            if (len >= 4)
            {
                *(uint*)d = *(uint*)s;
                d += 4;

                s += 4;
                len -= 4;
            }
            if (len >= 2)
            {
                *(ushort*)d = *(ushort*)s;
                d += 2;
                s += 2;
                len -= 2;
            }
            if (len >= 1)
            {
                *d = *s;
            }
        }

        /// <summary>
        /// Copies a specified number of bytes from a source pointer to a destination pointer.
        /// </summary>
        private unsafe static void BulkXor(void* dst, void* src, int len)
        {
            var d = (byte*)dst;
            var s = (byte*)src;

            while (len >= 8)
            {
                *(ulong*)d ^= *(ulong*)s;
                d += 8;
                s += 8;
                len -= 8;
            }
            if (len >= 4)
            {
                *(uint*)d ^= *(uint*)s;
                d += 4;
                s += 4;
                len -= 4;
            }
            if (len >= 2)
            {
                *(ushort*)d ^= *(ushort*)s;
                d += 2;
                s += 2;
                len -= 2;
            }
            if (len >= 1)
            {
                *d ^= *s;
            }
        }

        /// <summary>
        /// Encode an integer to byte array on any alignment in little endian format.
        /// </summary>
        private unsafe static void Encode32(byte* p, uint x)
        {
            p[0] = (byte)(x & 0xff);
            p[1] = (byte)((x >> 8) & 0xff);
            p[2] = (byte)((x >> 16) & 0xff);
            p[3] = (byte)((x >> 24) & 0xff);
        }

        /// <summary>
        /// Decode an integer from byte array on any alignment in little endian format.
        /// </summary>
        private unsafe static uint Decode32(byte* p)
        {
            return
                ((uint)(p[0]) +
                ((uint)(p[1]) << 8) +
                ((uint)(p[2]) << 16) +
                ((uint)(p[3]) << 24));
        }

        /// <summary>
        /// Apply the salsa20/8 core to the provided block.
        /// </summary>
        private unsafe static void Salsa208(uint* B)
        {
            uint x0 = B[0];
            uint x1 = B[1];
            uint x2 = B[2];
            uint x3 = B[3];
            uint x4 = B[4];
            uint x5 = B[5];
            uint x6 = B[6];
            uint x7 = B[7];
            uint x8 = B[8];
            uint x9 = B[9];
            uint x10 = B[10];
            uint x11 = B[11];
            uint x12 = B[12];
            uint x13 = B[13];
            uint x14 = B[14];
            uint x15 = B[15];

            for (var i = 0; i < 8; i += 2)
            {
                //((x0 + x12) << 7) | ((x0 + x12) >> (32 - 7));
                /* Operate on columns. */
                x4 ^= R(x0 + x12, 7); x8 ^= R(x4 + x0, 9);
                x12 ^= R(x8 + x4, 13); x0 ^= R(x12 + x8, 18);

                x9 ^= R(x5 + x1, 7); x13 ^= R(x9 + x5, 9);
                x1 ^= R(x13 + x9, 13); x5 ^= R(x1 + x13, 18);

                x14 ^= R(x10 + x6, 7); x2 ^= R(x14 + x10, 9);
                x6 ^= R(x2 + x14, 13); x10 ^= R(x6 + x2, 18);

                x3 ^= R(x15 + x11, 7); x7 ^= R(x3 + x15, 9);
                x11 ^= R(x7 + x3, 13); x15 ^= R(x11 + x7, 18);

                /* Operate on rows. */
                x1 ^= R(x0 + x3, 7); x2 ^= R(x1 + x0, 9);
                x3 ^= R(x2 + x1, 13); x0 ^= R(x3 + x2, 18);

                x6 ^= R(x5 + x4, 7); x7 ^= R(x6 + x5, 9);
                x4 ^= R(x7 + x6, 13); x5 ^= R(x4 + x7, 18);

                x11 ^= R(x10 + x9, 7); x8 ^= R(x11 + x10, 9);
                x9 ^= R(x8 + x11, 13); x10 ^= R(x9 + x8, 18);

                x12 ^= R(x15 + x14, 7); x13 ^= R(x12 + x15, 9);
                x14 ^= R(x13 + x12, 13); x15 ^= R(x14 + x13, 18);
            }

            B[0] += x0;
            B[1] += x1;
            B[2] += x2;
            B[3] += x3;
            B[4] += x4;
            B[5] += x5;
            B[6] += x6;
            B[7] += x7;
            B[8] += x8;
            B[9] += x9;
            B[10] += x10;
            B[11] += x11;
            B[12] += x12;
            B[13] += x13;
            B[14] += x14;
            B[15] += x15;
        }

        /// <summary>
        /// Utility method for Salsa208.
        /// </summary>
        private unsafe static uint R(uint a, int b)
        {
            return (a << b) | (a >> (32 - b));
        }

        /// <summary>
        /// Compute Bout = BlockMix_{salsa20/8, r}(Bin).  The input Bin must be 128r
        /// bytes in length; the output Bout must also be the same size.
        /// The temporary space X must be 64 bytes.
        /// </summary>
        private unsafe static void BlockMix(uint* Bin, uint* Bout, uint* X, int r)
        {
            /* 1: X <-- B_{2r - 1} */
            BulkCopy(X, &Bin[(2 * r - 1) * 16], 64);

            /* 2: for i = 0 to 2r - 1 do */
            for (var i = 0; i < 2 * r; i += 2)
            {
                /* 3: X <-- H(X \xor B_i) */
                BulkXor(X, &Bin[i * 16], 64);
                Salsa208(X);

                /* 4: Y_i <-- X */
                /* 6: B' <-- (Y_0, Y_2 ... Y_{2r-2}, Y_1, Y_3 ... Y_{2r-1}) */
                BulkCopy(&Bout[i * 8], X, 64);

                /* 3: X <-- H(X \xor B_i) */
                BulkXor(X, &Bin[i * 16 + 16], 64);
                Salsa208(X);

                /* 4: Y_i <-- X */
                /* 6: B' <-- (Y_0, Y_2 ... Y_{2r-2}, Y_1, Y_3 ... Y_{2r-1}) */
                BulkCopy(&Bout[i * 8 + r * 16], X, 64);
            }
        }

        /// <summary>
        /// Return the result of parsing B_{2r-1} as a little-endian integer.
        /// </summary>
        private unsafe static long Integerify(uint* B, int r)
        {
            var X = (uint*)(((byte*)B) + (2 * r - 1) * 64);

            return (((long)(X[1]) << 32) + X[0]);
        }

        /// <summary>
        /// Compute B = SMix_r(B, N).  The input B must be 128r bytes in length;
        /// the temporary storage V must be 128rN bytes in length; the temporary
        /// storage XY must be 256r + 64 bytes in length.  The value N must be a
        /// power of 2 greater than 1.  The arrays B, V, and XY must be aligned to a
        /// multiple of 64 bytes.
        /// </summary>
        private unsafe static void SMix(byte* B, int r, int N, uint* V, uint* XY)
        {
            var X = XY;
            var Y = &XY[32 * r];
            var Z = &XY[64 * r];

            /* 1: X <-- B */
            for (var k = 0; k < 32 * r; k++)
            {
                X[k] = Decode32(&B[4 * k]);
            }

            /* 2: for i = 0 to N - 1 do */
            for (var i = 0L; i < N; i += 2)
            {
                /* 3: V_i <-- X */
                BulkCopy(&V[i * (32 * r)], X, 128 * r);

                /* 4: X <-- H(X) */
                BlockMix(X, Y, Z, r);

                /* 3: V_i <-- X */
                BulkCopy(&V[(i + 1) * (32 * r)], Y, 128 * r);

                /* 4: X <-- H(X) */
                BlockMix(Y, X, Z, r);
            }

            /* 6: for i = 0 to N - 1 do */
            for (var i = 0; i < N; i += 2)
            {
                /* 7: j <-- Integerify(X) mod N */
                var j = Integerify(X, r) & (N - 1);

                /* 8: X <-- H(X \xor V_j) */
                BulkXor(X, &V[j * (32 * r)], 128 * r);
                BlockMix(X, Y, Z, r);

                /* 7: j <-- Integerify(X) mod N */
                j = Integerify(Y, r) & (N - 1);

                /* 8: X <-- H(X \xor V_j) */
                BulkXor(Y, &V[j * (32 * r)], 128 * r);
                BlockMix(Y, X, Z, r);
            }

            /* 10: B' <-- X */
            for (var k = 0; k < 32 * r; k++)
            {
                Encode32(&B[4 * k], X[k]);
            }
        }

    }
}
