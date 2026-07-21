using System;
using System.Security.Cryptography;
using System.Text;

public class HashCompute
{
    public static string Compute256(string login)
    {
        SHA256 hash = SHA256.Create();
        byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(login));
        string ourt = bytes.Aggregate("", (output, byt) => output + byt.ToString("x2"));
        return ourt;
    }
}