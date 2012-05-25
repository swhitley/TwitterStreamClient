using System;
using System.Security.Cryptography;
using System.IO;
using System.Web.UI;
using System.Web.Security;
using System.Web.Mail;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.Xml;

using System.Resources;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Web.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


namespace TwitterStreamClient
{
    public static class Common
    {
        private static Byte[] KEY_64 = System.Text.ASCIIEncoding.ASCII.GetBytes("39393939");
        private static Byte[] IV_64 = System.Text.ASCIIEncoding.ASCII.GetBytes("38383838");

        // returns DES encrypted string
        public static string Encrypt(string value)
        {
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cs);


            sw.Write(value);
            sw.Flush();
            cs.FlushFinalBlock();
            ms.Flush();

            // convert back to a string
            return Convert.ToBase64String(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
        }

        // returns DES decrypted string
        public static string Decrypt(string value)
        {
            value = value.Replace(" ", "+");

            while (value.Length % 4 != 0)
            {
                value += "=";
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            Byte[] buffer = Convert.FromBase64String(value);
            MemoryStream ms = new MemoryStream(buffer);
            CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}

