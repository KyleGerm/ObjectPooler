using System;
using System.Collections.Generic;
using System.Text;

namespace KylesUnityLib.Factory
{
    public class NullObjectConstructionException : Exception
    {
        public NullObjectConstructionException(string message) : base(message) { }
    }
}
