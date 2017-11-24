
using System.IO;
namespace Drone.API.Dig.Common.Extensions
{
	public static class StringExtensions
	{
		public static int ConvertStringToInt(this string value, int defaultValue)
		{
			int outValue;
			if (int.TryParse(value, out outValue))
			{
				defaultValue = outValue;
			}

			return defaultValue;
		}

		public static byte[] ToByteArray(this Stream stream)
		{
			var syncStream = Stream.Synchronized(stream);
			using (MemoryStream memStream = new MemoryStream())
			{
				syncStream.Position = 0;
				syncStream.CopyTo(memStream);
				return memStream.ToArray();
			}
		}
	}
}
