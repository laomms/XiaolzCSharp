using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static XiaolzCSharp.PInvoke;

namespace XiaolzCSharp
{
	public class Main
	{
		public string SendMessageCallBack(string szGroupID, string szQQID, string szContent) //dll回调函数
		{
			if (szGroupID == szQQID && !string.IsNullOrEmpty(szQQID) && !string.IsNullOrEmpty(szContent))
			{
				API.SendPrivateMessage(long.Parse(PInvoke.RobotQQ), long.Parse(szQQID), szContent);
			}
			else if (string.IsNullOrEmpty(szGroupID) && string.IsNullOrEmpty(szQQID))
			{
				return "";
			}
			else
			{
				if (!string.IsNullOrEmpty(szGroupID) && szGroupID != szQQID && !string.IsNullOrEmpty(szContent))
				{
					if (!string.IsNullOrEmpty(szQQID))
					{
						API.SendGroupMessage(long.Parse(PInvoke.RobotQQ), long.Parse(szGroupID), "[@" + szQQID + "]" + szContent);
					}
					else
					{
						API.SendGroupMessage(long.Parse(PInvoke.RobotQQ), long.Parse(szGroupID), szContent);
					}
				}
			}
			return "";
		}
		public string GetImageCallBack(string szGroupID, string szQQID, string szContent)
		{
			if (szContent.Contains("[pic,hash="))
			{
				dynamic jsonkey = new JavaScriptSerializer().DeserializeObject(PInvoke.jsonstr);
				int ptr = jsonkey("取图片下载地址");
				API.DelegateGetImageDownloadLink GetImageLink = (API.DelegateGetImageDownloadLink)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(API.DelegateGetImageDownloadLink));
				MatchCollection matches = Regex.Matches(szContent, "\\[pic,hash.*?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				foreach (Match match in matches)
				{
					if (szGroupID == szQQID)
					{
						var ImgUrl = GetImageLink(PInvoke.plugin_key, match.Value, long.Parse(PInvoke.RobotQQ), 0);
						return ImgUrl;
					}
					else
					{
						var ImgUrl = GetImageLink(PInvoke.plugin_key, match.Value, long.Parse(PInvoke.RobotQQ), long.Parse(szGroupID));
						return ImgUrl;
					}
				}
			}
			return "";
		}

		#region 收到私聊消息
		public static Delegate funRecvicePrivateMsg = new RecvicePrivateMsg(RecvicetPrivateMessage);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int RecvicePrivateMsg(ref PrivateMessageEvent sMsg);
		public static int RecvicetPrivateMessage(ref PrivateMessageEvent sMsg)
		{
			if (sMsg.SenderQQ != sMsg.ThisQQ)
			{

				if (sMsg.MessageContent.Contains("[pic,hash="))
				{
					MatchCollection matches = Regex.Matches(sMsg.MessageContent, "\\[pic,hash.*?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);

					foreach (Match match in matches)
					{

						API.GetImageDownloadLink(sMsg.ThisQQ, sMsg.SenderQQ, 0, match.Value);

					}

				}
				else if (sMsg.MessageContent.Contains("取好友列表"))
				{

					API.GetFriendList(sMsg.ThisQQ, sMsg.SenderQQ);

				}
				else if (sMsg.MessageContent.Contains("查询好友信息"))
				{

					API.GetFriendData(sMsg.ThisQQ, sMsg.SenderQQ);

				}
				else
				{

					//for (int i = 0; i < 200; i++)
					//{
					//	API.SendPrivateMessage(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString() + "发送了这样的消息:" + "第1枚密钥:类别: Win 10 RTM ProfessionalEducation OEM: DM 密钥: VR72F - 6NJ39 - WJD82 - GKTBG - HFT4M 代码: 0x8007007A 时间: 2020 - 09 - 05 14:14:30 PM");
					//	API.SendPrivateMessage(sMsg.ThisQQ, sMsg.SenderQQ, i.ToString());
					//	Thread.Sleep(200);
					//}
					API.SendPrivateMessage(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString() + "发送了这样的消息:" + sMsg.MessageContent);

				}

			}
			return 0;
		}
		#endregion
		#region 收到群聊消息
		public static RecviceGroupMsg funRecviceGroupMsg = new RecviceGroupMsg(RecvicetGroupMessage);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int RecviceGroupMsg(ref GroupMessageEvent sMsg);
		public static int RecvicetGroupMessage(ref GroupMessageEvent sMsg)
		{
			if (sMsg.SenderQQ != sMsg.ThisQQ)
			{

				if (sMsg.MessageContent.Contains("[pic,hash="))
				{
					MatchCollection matches = Regex.Matches(sMsg.MessageContent, "\\[pic,hash.*?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);

					foreach (Match match in matches)
					{

						API.GetImageDownloadLink(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.MessageGroupQQ, match.Value);

					}

				}
				else if (sMsg.MessageContent.Contains("[file,fileId=")) //发送文件
				{


				}
				else if (sMsg.MessageContent.Contains("[Audio,hash=")) //发送语音
				{


				}
				else if (sMsg.MessageContent.Contains("取群列表"))
				{

					API.GetGroupList(sMsg.ThisQQ, sMsg.MessageGroupQQ);

				}
				else if (sMsg.MessageContent.Contains("取群成员列表"))
				{

					API.GetgroupMemberlist(sMsg.ThisQQ, sMsg.MessageGroupQQ);

				}
				else
				{
					if (sMsg.ThisQQ != sMsg.SenderQQ)
					{

						string res = API.SendGroupMessage(sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "你发送了这样的消息:" + sMsg.MessageContent);

					}

				}

			}


			return 0;
		}
		#endregion
	}

}
