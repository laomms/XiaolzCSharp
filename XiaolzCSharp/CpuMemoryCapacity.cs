using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XiaolzCSharp
{
    public class CpuMemoryCapacity
    {
        public static List<string> MemoryAvailable()
        {
            List<string> status=new List<string>();
            //获取总物理内存大小
            ManagementClass cimobject1 = new ManagementClass("Win32_PhysicalMemory");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            double available = 0, capacity = 0;
            foreach (ManagementObject mo1 in moc1)
            {
                capacity += ((Math.Round(Int64.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024 / 1024 / 1024.0, 1)));
            }
            moc1.Dispose();
            cimobject1.Dispose();
            //获取内存可用大小
            ManagementClass cimobject2 = new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory");
            ManagementObjectCollection moc2 = cimobject2.GetInstances();
            foreach (ManagementObject mo2 in moc2)
            {
                available += ((Math.Round(Int64.Parse(mo2.Properties["AvailableMBytes"].Value.ToString()) / 1024.0, 1)));
            }
            moc2.Dispose();
            cimobject2.Dispose();
            status.Add("总内存=" + capacity.ToString() + "G");
            status.Add("可使用=" + available.ToString() + "G");
            status.Add("已使用=" + ((capacity - available)).ToString() + "G," + (Math.Round((capacity - available) / capacity * 100, 0)).ToString() + "%");
            return status;
        }
        public static List<string> HardwareInfo()
        {
            List<string> status = new List<string>();

            string CPUName = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_Processor");//Win32_Processor  CPU处理器
            foreach (ManagementObject mo in mos.Get())
            {
                CPUName = mo["Name"].ToString();
            }
            mos.Dispose();
            string PhysicalMemory = "";
            ManagementClass m = new ManagementClass("Win32_PhysicalMemory");//内存条
            ManagementObjectCollection mn = m.GetInstances();
            PhysicalMemory = "物理内存条数量：" + mn.Count.ToString() + "  ";
            double capacity = 0.0;
            int count = 0;
            foreach (ManagementObject mo1 in mn)
            {
                count++;
                capacity = ((Math.Round(Int64.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024 / 1024 / 1024.0, 1)));
                PhysicalMemory += "第" + count.ToString() + "张内存条大小：" + capacity.ToString() + "G   ";
            }
            mn.Dispose();
            m.Dispose();
            ManagementClass h = new ManagementClass("win32_DiskDrive");//硬盘
            ManagementObjectCollection hn = h.GetInstances();
            foreach (ManagementObject mo1 in hn)
            {
                capacity += Int64.Parse(mo1.Properties["Size"].Value.ToString()) / 1024 / 1024 / 1024;
            }
            mn.Dispose();
            m.Dispose();
            status.Add("CPU型号：" + CPUName);
            status.Add("内存状况：" + PhysicalMemory);
            status.Add("硬盘状况：" + "硬盘为：" + capacity.ToString() +"G");
            return status;
        }
        public static List<string> GetUsage()
        {
            List<string> status = new List<string>();
            var process = Process.GetCurrentProcess();
            var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total", Environment.MachineName);
            var ram = new PerformanceCounter("Process", "Private Bytes", process.ProcessName, true);
            cpu.NextValue();
            ram.NextValue();
            System.Threading.Thread.Sleep(500);
            status.Add("机器人CPU使用率: " + Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2).ToString() +"%");
            status.Add("机器人使用内存:" + Math.Round(ram.NextValue() / 1024 / 1024, 2).ToString() + "M");
            return status;
        }
     
        public static List<string> GetMemoryUsage()
        {
            List<string> status = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PerfFormattedData_PerfProc_Process"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    PerformanceCounter memCounter = new PerformanceCounter("Memory", "Available MBytes");
                    string mem = memCounter.NextValue().ToString();
                    status.Add(string.Format("{0:0000.00}", Math.Round(double.Parse(obj["WorkingSetPrivate"].ToString()) / 1024 / 1024, 2)) + "MB (使用内存)  " + Math.Round(double.Parse(obj["PercentProcessorTime"].ToString()) / Environment.ProcessorCount, 2).ToString() + "% (CPU占用)  " + "进程名称: " + obj["Name"].ToString());
                }
            }
            status.Sort();
            status.Reverse();
            return status.Take(10).ToList();
        }
        public static List<string> GetCpuUsage()
        {
            List<string> status = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM Win32_PerfFormattedData_PerfProc_Process"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                    PerformanceCounter memCounter = new PerformanceCounter("Memory", "Available MBytes");
                    string mem = memCounter.NextValue().ToString();
                    status.Add(string.Format("{0:00.00}", Math.Round(double.Parse(obj["PercentProcessorTime"].ToString())) / Environment.ProcessorCount, 2).ToString() + "% (CPU占用)  " + Math.Round(double.Parse(obj["WorkingSetPrivate"].ToString()) / 1024 / 1024, 2).ToString() + "MB (使用内存)  " + "进程名称: " +obj["Name"].ToString());
                    }
                }                
            status.Sort();
            status.Reverse();
            return status.Take(10).ToList();
        }
    }
}
