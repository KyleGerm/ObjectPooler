
using Microsoft.Win32.SafeHandles;

namespace KylesUnityLib
{
    internal static class DeBruijn
    {
        private const ulong _DeBruijn64 = 0x022FDD63CC95386DUL;

        private static readonly int[] DeBruijnIndex64 = 
        {
            0, 1, 2, 53, 3, 7, 54, 27,
            4, 38, 41, 8, 34, 55, 48, 28,
            62, 5, 39, 46, 44, 42, 22, 9,
            24, 35, 59, 56, 49, 18, 29, 11,
            63, 52, 6, 26, 37, 40, 33, 47,
            61, 45, 43, 21, 23, 58, 17, 10,
            51, 25, 36, 32, 60, 20, 57, 16,
            50, 31, 19, 15, 30, 14, 13, 12
        };

        private static int _64 = 64;
        public static  int TrailingZeroCount(ulong value)
        {
            if (value == 0)
                return  _64;
            
            ulong isolated = value & (~value + 1);
            ulong product = isolated * _DeBruijn64;
            return  DeBruijnIndex64[product >> 58];

        }
    }
}
