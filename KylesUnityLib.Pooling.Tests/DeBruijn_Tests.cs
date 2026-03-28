
namespace KylesUnityLib.Pooling.Tests
{
    public class DeBruijn_Tests
    {
        [Fact]
        public void TrailingZeroCountCorrectlyReturns()
        {
            ulong test = 1 << 3;
            int result = DeBruijn.TrailingZeroCount(test);
            Assert.Equal(3, result);
        }
        [Fact]
        public void TrailingZeroCountCorrectlyReturnsAcrossAllBits()
        {
            int bitShiftCount = 0;
            for (int i = 0; i < 65; i++)
            {
                ulong test = (ulong)(1 << bitShiftCount);
                int result = DeBruijn.TrailingZeroCount(test);
                Assert.Equal(bitShiftCount, result);
            }
        }
        [Fact]
        public void TrailingZeroCountReturns_64_WithAParameterOf_0()
        {
            int result = DeBruijn.TrailingZeroCount(0);
            Assert.Equal(64, result);
        }
    }
}
