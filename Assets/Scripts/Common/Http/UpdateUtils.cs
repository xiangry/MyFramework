using System.Security.Cryptography;
using System;
using System.Text;


public class UpdateUtils
{
	public UpdateUtils ()
	{

	}

    public static string MakeMD5String(byte[] md5)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < md5.Length; ++i)
        {
            sb.Append(md5[i].ToString("x2"));
        }
        return sb.ToString();
    }

	public static string Md5(string source)
	{
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(source));
        return MakeMD5String(result);
    }

    public static string Md5(byte[] source, int offset, int count)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(source, offset, count);
        return MakeMD5String(result); 
    } 

    public static string Md5(System.IO.Stream sm)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(sm);
        return MakeMD5String(result);
    }
}

