using System;
using System.Collections.Generic;
using System.Globalization;
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
		public static CancellationTokenSource cts = new CancellationTokenSource();
		public static CancellationToken token = CancellationToken.None;

		public string GetImageCallBack(string szGroupID, string szQQID, string szContent)
		{
			if (szContent.Contains("[pic,hash="))
			{				
				MatchCollection matches = Regex.Matches(szContent, "\\[pic,hash.*?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				foreach (Match match in matches)
				{
					if (szGroupID == szQQID)
					{
						IntPtr ImgUrl = API.GetImageDownloadLink(PInvoke.plugin_key, match.Value, long.Parse(PInvoke.RobotQQ), 0);
						return Marshal.PtrToStringAnsi( ImgUrl);
					}
					else
					{
						IntPtr ImgUrl = API.GetImageDownloadLink(PInvoke.plugin_key, match.Value, long.Parse(PInvoke.RobotQQ), long.Parse(szGroupID));
						return Marshal.PtrToStringAnsi(ImgUrl);
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
			long MessageRandom = 0;
			uint MessageReq = 0;
			if (SqliHelper.CheckDataExsit("中级权限", "QQID", sMsg.SenderQQ.ToString()) == false)//如果不在中级权限里不反馈
			{
				if (sMsg.SenderQQ != sMsg.ThisQQ)
					API.SendPrivateMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString() + "抱歉!你的QQ号不在高级授权名单.", ref MessageRandom, ref MessageReq);
				return 0;
			}
			if (sMsg.SenderQQ != sMsg.ThisQQ)
			{

				if (sMsg.MessageContent.Contains("[pic,hash="))
				{
					MatchCollection matches = Regex.Matches(sMsg.MessageContent, "\\[pic,hash.*?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);

					foreach (Match match in matches)
					{

						API.GetImageLink(sMsg.ThisQQ, sMsg.SenderQQ, 0, match.Value);
					}
				}
				else if (sMsg.MessageContent.Contains("取好友列表"))
				{

					API.GetFriendLists(sMsg.ThisQQ, sMsg.SenderQQ);

				}
				else if (sMsg.MessageContent.Contains("查询好友信息"))
				{
					API.GetFriendData(sMsg.ThisQQ, sMsg.SenderQQ);
				}
				else
				{
					API.SendPrivateMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString() + "发送了这样的消息:" + sMsg.MessageContent, ref MessageRandom, ref MessageReq);
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
		
			if (API.MsgRecod==true)
				SqliHelper.InsertData("消息记录", new string[] { "GroupID", "QQID", "MessageReq", "MessageRandom", "TimeStamp", "Msg" }, new string[] { sMsg.MessageGroupQQ.ToString(), sMsg.SenderQQ.ToString(), sMsg.MessageReq.ToString(), sMsg.MessageRandom.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture), sMsg.MessageContent }); ;
			//SqliHelper.InsertData("消息记录", new string[] { "GroupID", "QQID", "MessageReq", "MessageRandom", "TimeStamp", "Msg" }, new string[] { sMsg.MessageGroupQQ.ToString(), sMsg.SenderQQ.ToString(), sMsg.MessageReq.ToString(), sMsg.MessageRandom.ToString(), ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString(), sMsg.MessageContent }); ;
			if (SqliHelper.CheckDataExsit("授权群号", "GroupID", sMsg.MessageGroupQQ.ToString()) == false)//如果不在高级权限里不反馈
				return 0;
			if (SqliHelper.CheckDataExsit("高级权限", "QQID", sMsg.SenderQQ.ToString()) == false)//如果不在高级权限里不反馈
			{
				if (sMsg.SenderQQ != sMsg.ThisQQ)
					API.SendGroupMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "抱歉!你的QQ号不在高级授权名单.",false);
				return 0;
			}

			if (sMsg.SenderQQ != sMsg.ThisQQ)
			{
				if (sMsg.MessageContent.Contains("[pic,hash="))
				{
					MatchCollection matches = Regex.Matches(sMsg.MessageContent, "\\[pic,hash.*?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);

					foreach (Match match in matches)
					{
						API.GetImageLink(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.MessageGroupQQ, match.Value);

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

					API.GetGroupLists(sMsg.ThisQQ, sMsg.MessageGroupQQ);

				}
				else if (sMsg.MessageContent.Contains("取群成员列表"))
				{

					API.GetGroupMemberlists(sMsg.ThisQQ, sMsg.MessageGroupQQ);

				}
				else if (sMsg.MessageContent.Contains("同意") && sMsg.MessageContent.Contains("入群"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^同意入群]")).IsMatch(output.Replace(" ", "")) == true)
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
							API.GroupVerificationEvent(PInvoke.plugin_key,sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, GroupVerificationOperateEnum.Agree, PInvoke.EventTypeEnum.Friend_FriendRequest, "同意入群");
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.",false);
						}
						catch { }
					}

				}
				else if (sMsg.MessageContent.Contains("拒绝") && sMsg.MessageContent.Contains("入群"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^拒绝入群]")).IsMatch(output.Replace(" ", "")) == true)
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
							API.GroupVerificationEvent(PInvoke.plugin_key,sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, GroupVerificationOperateEnum.Deny, PInvoke.EventTypeEnum.Friend_FriendRequest, "拒绝入群");
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.",false);
						}
						catch { }
					}

				}
				else if (sMsg.MessageContent.Contains("同意加") && sMsg.MessageContent.Contains("为好友"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^同意加为好友]")).IsMatch(output.Replace(" ", "")) == true)
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
							API.FriendverificationEvent(PInvoke.plugin_key,sMsg.ThisQQ, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, FriendVerificationOperateEnum.Agree);
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.",false);
						}
						catch { }

					}
				}
				else if (sMsg.MessageContent.Contains("拒绝加") && sMsg.MessageContent.Contains("为好友"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d-]", string.Empty);
					if (new Regex("(?i)[^拒绝加为好友]").IsMatch(output.Replace(" ", "")) == true)
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
							API.FriendverificationEvent(PInvoke.plugin_key,sMsg.ThisQQ, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, FriendVerificationOperateEnum.Deny);
							API.EventDics.Remove(long.Parse(m.Value.ToString()));
							API.SendGroupMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.",false);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}

					}
				}
				else if (sMsg.MessageContent.Contains("撤回") && sMsg.MessageContent.Contains("最近消息") && sMsg.MessageContent.Contains("条"))
                {
					string output = Regex.Replace(sMsg.MessageContent, @"[\d-]", string.Empty);
					if (new Regex("(?i)[^撤回最近消息条]").IsMatch(output.Replace(" ", "")) == true)
						return 0;
					string szQQID="123";
					int Number=0;
					MatchCollection matches = new Regex("\\d+").Matches(sMsg.MessageContent);
					if (matches.Count > 2) return 0;
					foreach (Match match in matches)
					{
						if (match.Value.ToString().Length >=6 )
						{
							szQQID= match.Value;
						}
						else if(match.Value.ToString().Length < 3)
                        {
							Number= int.Parse(match.Value);
						}
					}
					List<List<string>> MsgList = SqliHelper.ReadData("消息记录", new string[] { "GroupID", "QQID", "MessageReq", "MessageRandom", "TimeStamp" }, "ORDER BY ID DESC LIMIT " + Number, "QQID like '" + szQQID + "'");
					int n = 0;
					foreach (List<string> list in MsgList)
					{
						n = n + 1;
						bool sucess = API.Undo_GroupEvent(PInvoke.plugin_key, API.MyQQ, long.Parse(list[0]), long.Parse(list[3]), int.Parse(list[2]));
						if (sucess)
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已撤回" + szQQID + "最近消息" + n.ToString() + "条", false);
					}
					

				}
				else if (sMsg.MessageContent=="压力测试")
                {
					token = cts.Token;
					Task.Factory.StartNew(() =>
					{
						int i = 0;
						while (!token.IsCancellationRequested)
						{
							i = i + 1;
							API.SendGroupMsg(plugin_key, API.MyQQ, 66847886, "小栗子机器人插件\r\n发送群消息压力测试\r\n测试~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\r\n" + DateTime.Now.ToString(), false);
							//API.SendGroupMsg(API.MyQQ, 66847886, "小栗子机器人插件\r\n发送群消息压力测试\r\n测试~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\r\n" +DateTime.Now.ToString());
							API.SendGroupMsg(plugin_key, API.MyQQ, 66847886, i.ToString(),false);
							Thread.Sleep(500);
						}
					}, token);

				}
				else if (sMsg.MessageContent == "停止压力测试")
				{
					if (cts != null)
					{
						cts.Cancel();
						cts.Dispose();
						cts = new CancellationTokenSource();
					}

				}
				else
				{
					if (sMsg.ThisQQ != sMsg.SenderQQ)
					{

						API.SendGroupMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "你发送了这样的消息:" + sMsg.MessageContent,false);

					}

				}				
			}
			return 0;
		}
		#endregion
	}

}
