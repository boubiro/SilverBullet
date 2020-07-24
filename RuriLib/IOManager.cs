using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Newtonsoft.Json;
using RuriLib.Models;

namespace RuriLib
{
    /// <summary>
    /// Static Class used to access serialization and deserialization of objects.
    /// </summary>
    public static class IOManager
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        /// <summary>
        /// Saves the RuriLib settings to a file.
        /// </summary>
        /// <param name="settingsFile">The file you want to save to</param>
        /// <param name="settings">The RuriLib settings object</param>
        public static void SaveSettings<T>(string settingsFile, T settings)
        {
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        /// <summary>
        /// Loads the RuriLib settings from a file.
        /// </summary>
        /// <param name="settingsFile">The file you want to load from</param>
        /// <returns>An instance of RLSettingsViewModel</returns>
        public static T LoadSettings<T>(string settingsFile)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(settingsFile));
        }

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Serializes a block to a JSON string.
        /// </summary>
        /// <param name="block">The block to serialize</param>
        /// <returns>The JSON-encoded BlockBase object with TypeNameHandling on</returns>
        public static string SerializeBlock(BlockBase block)
        {
            return JsonConvert.SerializeObject(block, Formatting.None, settings);
        }

        /// <summary>
        /// Deserializes a Block from a JSON string.
        /// </summary>
        /// <param name="block">The JSON-encoded string with TypeNameHandling on</param>
        /// <returns>An instance of BlockBase</returns>
        public static BlockBase DeserializeBlock(string block)
        {
            return JsonConvert.DeserializeObject<BlockBase>(block, settings);
        }

        /// <summary>
        /// Serializes a list of blocks to a JSON string.
        /// </summary>
        /// <param name="blocks">The list of blocks to serialize</param>
        /// <returns>The JSON-encoded List of BlockBase objects with TypeNameHandling on</returns>
        public static string SerializeBlocks(List<BlockBase> blocks)
        {
            return JsonConvert.SerializeObject(blocks, Formatting.None, settings);
        }

        /// <summary>
        /// Deserializes a list of blocks from a JSON string.
        /// </summary>
        /// <param name="blocks">The JSON-encoded string with TypeNameHandling on</param>
        /// <returns>A list of instances of BlockBase</returns>
        public static List<BlockBase> DeserializeBlocks(string blocks)
        {
            return JsonConvert.DeserializeObject<List<BlockBase>>(blocks, settings);
        }

        /// <summary>
        /// Serializes a Config object to the loli-formatted string.
        /// </summary>
        /// <param name="config">The Config to serialize</param>
        /// <returns>The loli-formatted string</returns>
        public static string SerializeConfig(Config config)
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("[SETTINGS]");
            writer.WriteLine(JsonConvert.SerializeObject(config.Settings, Formatting.Indented));
            writer.WriteLine("");
            writer.WriteLine("[SCRIPT]");
            writer.Write(config.Script);
            return writer.ToString();
        }

        /// <summary>
        /// Deserializes a Config object from a loli-formatted string.
        /// </summary>
        /// <param name="config">The loli-formatted string</param>
        /// <returns>An instance of the Config object</returns>
        public static Config DeserializeConfig(string config)
        {
            var split = config.Split(new string[] { "[SETTINGS]", "[SCRIPT]" }, StringSplitOptions.RemoveEmptyEntries);
            return new Config(JsonConvert.DeserializeObject<ConfigSettings>(split[0]), split[1].TrimStart('\r', '\n'));
        }

        /// <summary>
        /// Serializes a list of proxies to a JSON string.
        /// </summary>
        /// <param name="proxies">The list of proxies to serialize</param>
        /// <returns>The JSON-encoded list of CProxy objects with TypeNameHandling on</returns>
        public static string SerializeProxies(List<CProxy> proxies)
        {
            return JsonConvert.SerializeObject(proxies, Formatting.None);
        }

        /// <summary>
        /// Deserializes a list of proxies from a JSON string.
        /// </summary>
        /// <param name="proxies">The JSON-encoded list of proxies with TypeNameHandling on</param>
        /// <returns>A list of CProxy objects</returns>
        public static List<CProxy> DeserializeProxies(string proxies)
        {
            return JsonConvert.DeserializeObject<List<CProxy>>(proxies);
        }

        /// <summary>
        /// Loads a Config object from a .loli file.
        /// </summary>
        /// <param name="fileName">The config file</param>
        /// <returns>A Config object</returns>
        public static Config LoadConfig(string fileName, bool liloX = false)
        {
            if (liloX) { return DeserializeConfigX(File.ReadAllText(fileName)); }
            return DeserializeConfig(File.ReadAllText(fileName));
        }

        private static Config LoadConfigX(string fileName)
        {
            return DeserializeConfigX(File.ReadAllText(fileName));
        }

        private static Config DeserializeConfigX(string config)
        {
            if (!config.Contains("ID") && !config.Contains("Body"))
                try
                {
                    byte[] bytes = Convert.FromBase64String(config);
                    config = Encoding.UTF8.GetString(bytes);
                    config = DecryptX(Regex.Match(config, "0x;(.*?)x;0").Groups[1].Value, "THISISOBmodedByForlax");
                }
                catch
                {
                    config = DecryptX(Regex.Match(config, "0x;(.*?)x;0").Groups[1].Value, "0THISISOBmodedByForlaxNIGGAs");
                }
            else
            {
               var bytes = Convert.FromBase64String(BellaCiao(Regex.Match(config, "\"Body\": \"(.*?)\"").Groups[1].Value, 2));
                config = DecryptX(Regex.Match(Encoding.UTF8.GetString(bytes), "x0;(.*?)0;x").Groups[1].Value, "0THISISOBmodedByForlaxNIGGAs");
            }

            string[] array = config.Split(new string[]
                {
                "[SETTINGS]",
                "[SCRIPT]"
                }, StringSplitOptions.RemoveEmptyEntries);
            return new Config(JsonConvert.DeserializeObject<ConfigSettings>(array[0]), array[1].TrimStart(new char[]
            {
                '\r',
                '\n'
            }));
        }

        private static string DecryptX(string cipherText, string passPhrase)
        {
            byte[] array = Convert.FromBase64String(cipherText);
            byte[] salt = array.Take(32).ToArray<byte>();
            byte[] rgbIV = array.Skip(32).Take(32).ToArray<byte>();
            byte[] array2 = array.Skip(64).Take(array.Length - 64).ToArray<byte>();
            string script;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passPhrase, salt, 1000))
            {
                byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
                using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.BlockSize = 256;
                    rijndaelManaged.Mode = CipherMode.CBC;
                    rijndaelManaged.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(bytes, rgbIV))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(array2))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                            {
                                byte[] array3 = new byte[array2.Length];
                                int count = cryptoStream.Read(array3, 0, array3.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                script = Encoding.UTF8.GetString(array3, 0, count);
                            }
                        }
                    }
                }
            }
            return script;
        }

        public static string BellaCiao(string helpme, int op)
        {
            if (op != 1)
            {
                string str2 = "ay$a5%&jwrtmnh;lasjdf98787OMGFORLAX";
                string str3 = "abc@98797hjkas$&asd(*$%GJMANIGE";
                byte[] buffer = new byte[0];
                buffer = Encoding.UTF8.GetBytes(str3.Substring(0, 8));
                byte[] buffer2 = new byte[0];
                buffer2 = Encoding.UTF8.GetBytes(str2.Substring(0, 8));
                byte[] buffer3 = new byte[helpme.Length / 2];
                for (int i = 0; i < buffer3.Length; i++)
                {
                    buffer3[i] = Convert.ToByte(helpme.Substring(i * 2, 2), 0x10);
                }
                helpme = Encoding.UTF8.GetString(buffer3);
                byte[] buffer4 = new byte[helpme.Replace(" ", "+").Length];
                buffer4 = Convert.FromBase64String(helpme.Replace(" ", "+"));
                using (DESCryptoServiceProvider provider2 = new DESCryptoServiceProvider())
                {
                    MemoryStream stream = new MemoryStream();
                    CryptoStream stream3 = new CryptoStream(stream, provider2.CreateDecryptor(buffer2, buffer), CryptoStreamMode.Write);
                    stream3.Write(buffer4, 0, buffer4.Length);
                    stream3.FlushFinalBlock();
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            string s = "";
            string str5 = "ay$a5%&jwrtmnh;lasjdf98787OMGFORLAX";
            string str6 = "abc@98797hjkas$&asd(*$%GJMANIGE";
            byte[] rgbIV = new byte[0];
            rgbIV = Encoding.UTF8.GetBytes(str6.Substring(0, 8));
            byte[] rgbKey = new byte[0];
            rgbKey = Encoding.UTF8.GetBytes(str5.Substring(0, 8));
            byte[] bytes = Encoding.UTF8.GetBytes(helpme);
            using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
            {
                CryptoStream stream1 = new CryptoStream(new MemoryStream(), provider.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                stream1.Write(bytes, 0, bytes.Length);
                stream1.FlushFinalBlock();
                MemoryStream stream2 = new MemoryStream();
                s = Convert.ToBase64String(stream2.ToArray());
                StringBuilder builder = new StringBuilder();
                byte[] buffer8 = Encoding.UTF8.GetBytes(s);
                int index = 0;
                while (true)
                {
                    if (index >= buffer8.Length)
                    {
                        s = builder.ToString();
                        break;
                    }
                    builder.Append(buffer8[index].ToString("X2"));
                    index++;
                }
            }
            return s;
        }


        /// <summary>
        /// Saves a Config object to a .loli file.
        /// </summary>
        /// <param name="config">The viewmodel of the config to save</param>
        /// <param name="fileName">The path of the file where the Config will be saved</param>
        /// <returns>Whether the file has been saved successfully</returns>
        public static void SaveConfig(Config config, string fileName)
        {
            File.WriteAllText(fileName, SerializeConfig(config));
        }

        /// <summary>
        /// Clones a Config object by serializing and deserializing it.
        /// </summary>
        /// <param name="config">The object to clone</param>
        /// <returns>The cloned Config object</returns>
        public static Config CloneConfig(Config config)
        {
            return DeserializeConfig(SerializeConfig(config));
        }

        /// <summary>
        /// Clones a BlockBase object by serializing and deserializing it.
        /// </summary>
        /// <param name="block">The object to clone</param>
        /// <returns>The cloned BlockBase object</returns>
        public static BlockBase CloneBlock(BlockBase block)
        {
            return DeserializeBlock(SerializeBlock(block));
        }

        /// <summary>
        /// Clones a list of proxies by serializing and deserializing it.
        /// </summary>
        /// <param name="proxies">The list of proxies to clone</param>
        /// <returns>The cloned list of proxies</returns>
        public static List<CProxy> CloneProxies(List<CProxy> proxies)
        {
            return DeserializeProxies(SerializeProxies(proxies));
        }

        /// <summary>
        /// Parses the EnvironmentSettings from a file.
        /// </summary>
        /// <param name="envFile">The .ini file of the settings</param>
        /// <returns>The loaded EnvironmentSettings object</returns>
        public static EnvironmentSettings ParseEnvironmentSettings(string envFile)
        {
            var env = new EnvironmentSettings();
            var lines = File.ReadAllLines(envFile).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            for (int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];
                if (line.StartsWith("#")) continue;
                else if (line.StartsWith("["))
                {
                    Type type;
                    var header = line;
                    switch (line.Trim())
                    {
                        case "[WLTYPE]": type = typeof(WordlistType); break;
                        case "[CUSTOMKC]": type = typeof(CustomKeychain); break;
                        case "[EXPFORMAT]": type = typeof(ExportFormat); break;
                        default: throw new Exception("Unrecognized ini header");
                    }

                    var parameters = new List<string>();
                    int j = i + 1;
                    for (; j < lines.Count(); j++)
                    {
                        line = lines[j];
                        if (line.StartsWith("[")) break;
                        else if (line.Trim() == string.Empty || line.StartsWith("#")) continue;
                        else parameters.Add(line);
                    }

                    switch (header)
                    {
                        case "[WLTYPE]": env.WordlistTypes.Add((WordlistType)ParseObjectFromIni(type, parameters)); break;
                        case "[CUSTOMKC]": env.CustomKeychains.Add((CustomKeychain)ParseObjectFromIni(type, parameters)); break;
                        case "[EXPFORMAT]": env.ExportFormats.Add((ExportFormat)ParseObjectFromIni(type, parameters)); break;
                        default: break;
                    }

                    i = j - 1;
                }
            }

            return env;
        }

        private static object ParseObjectFromIni(Type type, List<string> parameters)
        {
            object obj = Activator.CreateInstance(type);
            foreach (var pair in parameters
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => p.Split(new char[] { '=' }, 2)))
            {
                var prop = type.GetProperty(pair[0]);
                var propObj = prop.GetValue(obj);
                dynamic value = null;

                switch (propObj)
                {
                    case string x:
                        value = pair[1];
                        break;

                    case int x:
                        value = int.Parse(pair[1]);
                        break;

                    case bool x:
                        value = bool.Parse(pair[1]);
                        break;

                    case List<string> x:
                        value = pair[1].Split(',').ToList();
                        break;

                    case Color x:
                        value = Color.FromRgb(
                            System.Drawing.Color.FromName(pair[1]).R,
                            System.Drawing.Color.FromName(pair[1]).G,
                            System.Drawing.Color.FromName(pair[1]).B);
                        break;
                }

                prop.SetValue(obj, value);
            }
            return obj;
        }
    }
}
