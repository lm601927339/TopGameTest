using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
public class MD5Helper
{
   
    public static bool VerifyMd5Hash(string input, string hash)
    {
        MD5 md5Hash = MD5.Create();

        var hashOfInput = GetMd5Hash(input);
        var comparer = StringComparer.OrdinalIgnoreCase;
        return 0 == comparer.Compare(hashOfInput, hash);
    }

    // string to md5
   
    public static string GetMD5(string msg)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++)
        {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');
        return destString;
    }
   
    public static string GetMd5Hash(string input)
    {
        MD5 md5Hash = MD5.Create();

        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sBuilder = new StringBuilder();
        foreach (byte t in data)
        {
            sBuilder.Append(t.ToString("x2"));
        }
        return sBuilder.ToString();
    }
   
    public static string FormatMD5(Byte[] data)
    {
        return System.BitConverter.ToString(data).Replace("-", "").ToLower();
    }
   
    public static string BuildFileMd5(string filename)
    {
        string filemd5 = null;
        try
        {
            using (var fileStream = File.OpenRead(filename))
            {
                var md5 = MD5.Create();
                var fileMD5Bytes = md5.ComputeHash(fileStream);                                 
                filemd5 = FormatMD5(fileMD5Bytes);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("BuildFileMd5 " + ex);
        }
        return filemd5;
    }
}

