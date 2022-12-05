using System.IO;
using System.Security.Cryptography;

namespace ETModel
{
	public static class MD5Helper
	{
		public static string FileMD5(string filePath)
		{
			byte[] retVal;
            using (FileStream file = new FileStream(filePath, FileMode.Open))
			{
				MD5 md5 = new MD5CryptoServiceProvider();
				retVal = md5.ComputeHash(file);
			}
			return retVal.ToHex("x2");
		}

		public static string BytesMD5(byte[] data)
		{
			byte[] retVal;
			MD5 md5 = new MD5CryptoServiceProvider();
			retVal = md5.ComputeHash(data);
			return retVal.ToHex("x2");
		}

		public static string StringMD5(string data)
		{
			byte[] retVal;
			MD5 md5 = new MD5CryptoServiceProvider();
			retVal = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
			return retVal.ToHex("x2");
		}
	}
}
