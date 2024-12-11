using System;
using System.Runtime.Serialization;

namespace BRANDForUnity
{
	[Serializable]
	public sealed class BRANDException : Exception
	{
		public BRANDException(string message) : base(message) { }

		public BRANDException(string message, Exception innerException) : base(message, innerException) { }

        private BRANDException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    }
}

