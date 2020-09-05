﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XiaolzCSharp
{
    public class SilkHelp
    {
        /// <summary>
        /// Silk解码
        /// </summary>
        /// <param name="audio_path"></param>
        /// <returns></returns>
        public byte[] SilkDecoding(string audio_path)
        {
            if (!File.Exists(audio_path))
            {
                return null;
            }
            string rootdicpath = System.Environment.CurrentDirectory;
            string ffmpge = rootdicpath + "\\main\\corn\\ffmpeg.exe";
            string silkdecode = rootdicpath + "\\main\\corn\\silkdecode.exe";//解码
            string silkencode = rootdicpath + "\\main\\corn\\silkencode.exe";//编码
            if (File.Exists(ffmpge) && File.Exists(silkdecode) && File.Exists(silkencode))
            {
                string name = audio_path.Substring(audio_path.LastIndexOf("\\") + 1);
                string audioslik = rootdicpath + "\\main\\data\\voice";
                if (!Directory.Exists(audioslik))
                {
                    DirectoryInfo dic = Directory.CreateDirectory(audioslik);
                }
                string tempname = audioslik + "\\" + name.Substring(0, name.LastIndexOf("."));
                string arg = $" -i \"{name}\" \"{tempname}.mp3\"";
                //ffmpeg -i "name.silk" "name1.mp3"
                try
                {
                    Runcmd(ffmpge, arg);
                    return GetByte($"{tempname}.mp3");
                }
                catch (Exception)
                {
                    //silkdecode "name.silk" "name2.pcm"
                    arg = $"\"{audio_path}\" \"{tempname}.pcm\"";
                    Runcmd(silkdecode, arg);
                    //ffmpeg -f s16le -ar 24000 -ac 1 -i "name2.pcm" "name2.mp3"
                    arg = $" -f s16le -ar 24000 -ac 1 -i \"{tempname}.pcm\" \"{tempname}.mp3\"";
                    Runcmd(ffmpge, arg);
                    return GetByte($"{tempname}.mp3");
                }
            }
            return null;
        }
        /// <summary>
        /// Silk编码
        /// </summary>
        /// <param name="audio_path"></param>
        /// <returns></returns>
        public byte[] SilkEncoding(string audio_path)
        {
            if (!File.Exists(audio_path))
            {
                return null;
            }
            string rootdicpath = System.Environment.CurrentDirectory;
            string ffmpge = rootdicpath + "\\main\\corn\\ffmpeg.exe";
            string silkdecode = rootdicpath + "\\main\\corn\\silkdecode.exe";//解码
            string silkencode = rootdicpath + "\\main\\corn\\silkencode.exe";//编码
            if (File.Exists(ffmpge) && File.Exists(silkdecode) && File.Exists(silkencode))
            {
                string name = audio_path.Substring(audio_path.LastIndexOf("\\") + 1);
                string audioslik = rootdicpath + "\\main\\data\\voice";
                if (!Directory.Exists(audioslik))
                {
                    DirectoryInfo dic = Directory.CreateDirectory(audioslik);
                }
                string tempname = audioslik + "\\" + name.Substring(0, name.LastIndexOf("."));
                string arg = $"-y -i \"{name}\" -f s16le -ar 24000 -ac 1 \"{tempname}.pcm\"";
                //ffmpeg -y -i "1.mp3" -f s16le -ar 24000 -ac 1 "name.pcm"
                Runcmd(ffmpge, arg);
                //silkencode "name.pcm" "name.silk" -tencent
                arg = $"\"{tempname}.pcm\" \"{tempname}.silk\" -tencent";
                Runcmd(silkencode, arg);
                return GetByte($"{tempname}.silk");
            }
            return null;
        }
        /// <summary>
        /// 将文件转换为byte[]
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private static byte[] GetByte(string filepath)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                byte[] by = new byte[fs.Length];
                fs.Read(by, 0, (int)fs.Length);
                fs.Close();
                return by;
            }
        }
        /// <summary>
        /// 调用外部程序
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="arg"></param>
        private void Runcmd(string filename, string arg)
        {
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = filename;
            p.Arguments = arg;
            p.UseShellExecute = false;
            p.CreateNoWindow = true;
            Process.Start(p);
        }
    }
}
