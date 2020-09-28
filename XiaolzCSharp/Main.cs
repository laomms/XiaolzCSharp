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


		//public string SendMessageCallBack(string szGroupID, string szQQID, string szContent) //dll回调函数
		//{
		//	if (szGroupID == szQQID && !string.IsNullOrEmpty(szQQID) && !string.IsNullOrEmpty(szContent))
		//	{
		//		API.SendPrivateMessage(long.Parse(PInvoke.RobotQQ), long.Parse(szQQID), szContent);
		//	}
		//	else if (string.IsNullOrEmpty(szGroupID) && string.IsNullOrEmpty(szQQID))
		//	{
		//		return "";
		//	}
		//	else
		//	{
		//		if (!string.IsNullOrEmpty(szGroupID) && szGroupID != szQQID && !string.IsNullOrEmpty(szContent))
		//		{
		//			if (!string.IsNullOrEmpty(szQQID))
		//			{
		//				API.SendGroupMessage(long.Parse(PInvoke.RobotQQ), long.Parse(szGroupID), "[@" + szQQID + "]" + szContent);
		//			}
		//			else
		//			{
		//				API.SendGroupMessage(long.Parse(PInvoke.RobotQQ), long.Parse(szGroupID), szContent);
		//			}
		//		}
		//	}
		//	return "";
		//}
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
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int RecvicePrivateMsg(ref PrivateMessageEvent sMsg);
		public static int RecvicetPrivateMessage(ref PrivateMessageEvent sMsg)
		{
			Console.WriteLine(sMsg.MessageType.ToString());
			Console.WriteLine(sMsg.MessageSubType.ToString());
			if (SqliHelper.CheckDataExsit("中级权限", "QQID", sMsg.SenderQQ.ToString()) == false)//如果不在中级权限里不反馈
			{
				if (sMsg.SenderQQ != sMsg.ThisQQ)
					API.SendPrivateMessage(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString() + "抱歉!你的QQ号不在高级授权名单.");
				return 0;
			}
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
					API.SendPrivateMessage(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString() + "发送了这样的消息:" + sMsg.MessageContent);
				}

			}
			return 0;
		}
		#endregion
		#region 收到群聊消息
		public static RecviceGroupMsg funRecviceGroupMsg = new RecviceGroupMsg(RecvicetGroupMessage);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int RecviceGroupMsg(ref GroupMessageEvent sMsg);
		public static int RecvicetGroupMessage(ref GroupMessageEvent sMsg)
		{
			Console.WriteLine(sMsg.MessageType.ToString());
			if (SqliHelper.CheckDataExsit("授权群号", "GroupID", sMsg.MessageGroupQQ.ToString()) == false)//如果不在高级权限里不反馈
				return 0;
			if (SqliHelper.CheckDataExsit("高级权限", "QQID", sMsg.SenderQQ.ToString()) == false)//如果不在高级权限里不反馈
			{
				if (sMsg.SenderQQ != sMsg.ThisQQ)
					API.SendGroupMessage(sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "抱歉!你的QQ号不在高级授权名单.");
				return 0;
			}

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
				else if (sMsg.MessageContent.Contains("同意") && sMsg.MessageContent.Contains("入群"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^同意入群]")).IsMatch(output) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						try
						{
							API.DealGroupEvent(sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, 11, API.EventDics[long.Parse(m.Value)].Item4, "同意入群");
							API.EventDics.Remove(long.Parse(m.Value));
						}
						catch { }
					}

				}
				else if (sMsg.MessageContent.Contains("拒绝") && sMsg.MessageContent.Contains("入群"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^拒绝入群]")).IsMatch(output) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						try
						{
							API.DealGroupEvent(sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, 12, API.EventDics[long.Parse(m.Value)].Item4, "拒绝入群");
							API.EventDics.Remove(long.Parse(m.Value));
						}
						catch { }
					}

				}
				else if (sMsg.MessageContent.Contains("同意加") && sMsg.MessageContent.Contains("为好友"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^同意加为好友]")).IsMatch(output) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						try
						{
							API.DealFriendEvent(sMsg.ThisQQ, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, 1);
							API.EventDics.Remove(long.Parse(m.Value));
						}
						catch { }

					}
				}
				else if (sMsg.MessageContent.Contains("拒绝加") && sMsg.MessageContent.Contains("为好友"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d-]", string.Empty);
					if (new Regex("(?i)[^拒绝加为好友]").IsMatch(output) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.ToString().Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						try
						{
							API.DealFriendEvent(sMsg.ThisQQ, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, 2);
							API.EventDics.Remove(long.Parse(m.Value.ToString()));
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}

					}
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
