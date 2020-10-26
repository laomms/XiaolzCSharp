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


		#region 收到私聊消息
		public static Delegate funRecvicePrivateMsg = new RecvicePrivateMsg(RecvicetPrivateMessage);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int RecvicePrivateMsg(ref PrivateMessageEvent sMsg);
		public static int RecvicetPrivateMessage(ref PrivateMessageEvent sMsg)
		{
			API.MyQQ = sMsg.ThisQQ;
			long MessageRandom = 0;
			uint MessageReq = 0;
			if (SqliHelper.CheckDataExsit("中级权限", "QQID", sMsg.SenderQQ.ToString()) == false)//如果不在中级权限里不反馈
			{
				if (sMsg.SenderQQ != sMsg.ThisQQ)
				{

				}

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
					//复读机
					//API.SendPrivateMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString() + "发送了这样的消息:" + sMsg.MessageContent, ref MessageRandom, ref MessageReq);
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
			API.MyQQ = sMsg.ThisQQ;
			if (API.MsgRecod == true)
				SqliHelper.InsertData("消息记录", new string[] { "GroupID", "QQID", "MessageReq", "MessageRandom", "TimeStamp", "Msg" }, new string[] { sMsg.MessageGroupQQ.ToString(), sMsg.SenderQQ.ToString(), sMsg.MessageReq.ToString(), sMsg.MessageRandom.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture), sMsg.MessageContent }); ;
			//SqliHelper.InsertData("消息记录", new string[] { "GroupID", "QQID", "MessageReq", "MessageRandom", "TimeStamp", "Msg" }, new string[] { sMsg.MessageGroupQQ.ToString(), sMsg.SenderQQ.ToString(), sMsg.MessageReq.ToString(), sMsg.MessageRandom.ToString(), ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString(), sMsg.MessageContent }); ;
			if (SqliHelper.CheckDataExsit("授权群号", "GroupID", sMsg.MessageGroupQQ.ToString()) == false)//如果不在高级权限里不反馈
			{
				return 0;
			}

			if (SqliHelper.CheckDataExsit("高级权限", "QQID", sMsg.SenderQQ.ToString()) == false && API.GetAdministratorLists(sMsg.ThisQQ, sMsg.MessageGroupQQ).Contains(sMsg.SenderQQ.ToString()) == false)//如果不在高级权限里不反馈
			{
				if (sMsg.SenderQQ != sMsg.ThisQQ)
					//API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "抱歉!你的QQ号不在高级授权名单.", false);
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
				else if (sMsg.MessageContent == "开启消息记录")
				{
					API.MsgRecod = true;
					API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已开始消息记录.", false);
				}
				else if (sMsg.MessageContent.Contains("添加黑名单"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^添加黑名单]")).IsMatch(output.Replace(" ", "")) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						if (API.DeleteGroupMember(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, long.Parse(m.Value), true))
						{
							if (SqliHelper.CheckDataExsit("黑名单", "QQID", m.Value) == false)
							{
								SqliHelper.InsertData("黑名单", new string[] { "QQID", "time" }, new string[] { m.Value, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
								API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已将" + m.Value + "移除群并添加到黑名单!", false);
							}
						}
					}
				}
				else if (sMsg.MessageContent.Contains("添加全局黑名单"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^添加全局黑名单]")).IsMatch(output.Replace(" ", "")) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						if (API.DeleteGroupMember(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, long.Parse(m.Value), false))
						{
							if (SqliHelper.CheckDataExsit("黑名单", "QQID", m.Value) == false)
							{
								SqliHelper.InsertData("黑名单", new string[] { "QQID", "time" }, new string[] { m.Value, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.InvariantCulture) });
								API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已将" + m.Value + "移除群并添加到黑名单!", false);
							}
						}
					}
				}
				else if (sMsg.MessageContent.Contains("解除黑名单"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^解除黑名单]")).IsMatch(output.Replace(" ", "")) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						if (API.DeleteGroupMember(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, long.Parse(m.Value), false))
						{
							if (SqliHelper.CheckDataExsit("黑名单", "QQID", m.Value) == true)
							{
								SqliHelper.DeleteData("黑名单", "QQID", "QQID", "QQID like'" + m.Value + "'");
								API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已解除黑名单:" + m.Value, false);
							}
						}
					}
				}
				else if (sMsg.MessageContent.ToUpper() == "查询CPU占用")
				{
					string SendQQ = sMsg.SenderQQ.ToString();
					long GroupQQ = sMsg.MessageGroupQQ;
					long thisqq = sMsg.ThisQQ;
					new Thread(() =>
					{
						string text = string.Join(Environment.NewLine, CpuMemoryCapacity.GetCpuUsage());
						API.SendGroupMsg(PInvoke.plugin_key, thisqq, GroupQQ, "[@" + SendQQ + "]" + Environment.NewLine + text, false);
					}).Start();
				}
				else if (sMsg.MessageContent == "查询内存占用")
				{
					string SendQQ = sMsg.SenderQQ.ToString();
					long GroupQQ = sMsg.MessageGroupQQ;
					long thisqq = sMsg.ThisQQ;
					new Thread(() =>
					{
						string[] strArray = CpuMemoryCapacity.GetMemoryUsage().ToArray(); ;
						strArray = strArray.Select(s => s.TrimStart('0')).ToArray();
						API.SendGroupMsg(PInvoke.plugin_key, thisqq, GroupQQ, "[@" + SendQQ + "]" + Environment.NewLine + string.Join(Environment.NewLine, strArray), false);
					}).Start();
				}
				else if (sMsg.MessageContent == "查询资源占用")
				{
					string SendQQ = sMsg.SenderQQ.ToString();
					long GroupQQ = sMsg.MessageGroupQQ;
					long thisqq = sMsg.ThisQQ;
					new Thread(() =>
					{
						string text = string.Join(Environment.NewLine, CpuMemoryCapacity.HardwareInfo());
						text = text + Environment.NewLine + string.Join(Environment.NewLine, CpuMemoryCapacity.MemoryAvailable());
						text = text + Environment.NewLine + string.Join(Environment.NewLine, CpuMemoryCapacity.GetUsage());
						API.SendGroupMsg(PInvoke.plugin_key, thisqq, GroupQQ, "[@" + SendQQ + "]" + Environment.NewLine + text, false);
					}).Start();
				}
				else if (sMsg.MessageContent == "机器人菜单")
				{
					API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + Environment.NewLine + PInvoke.RobotMenu, false);
				}
				else if (sMsg.MessageContent == "全员禁言")
				{
					if (API.MuteGroupAll(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, true))
						API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "已开启全员禁言!", false);
				}
				else if (sMsg.MessageContent == "解除全员禁言")
				{
					if (API.MuteGroupAll(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, true))
						API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "已解除全员禁言!", false);
				}
				else if (sMsg.MessageContent.Contains("禁言") && sMsg.MessageContent.Contains("时间") && sMsg.MessageContent.Contains("分钟"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if (new Regex("(?i)[^禁言时间分钟]").IsMatch(output.Replace(" ", "")) == true)
						return 0;
					string szQQID = "123";
					uint minute = 0;
					MatchCollection matches = new Regex("\\d+").Matches(sMsg.MessageContent);
					if (matches.Count > 2) return 0;
					foreach (Match match in matches)
					{
						if (match.Value.ToString().Length >= 6)
						{
							szQQID = match.Value;
						}
						else if (match.Value.ToString().Length < 3)
						{
							minute = uint.Parse(match.Value);
						}
					}
					if (API.MuteGroupMember(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, long.Parse(szQQID), minute * 60))
						API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, szQQID + "已被禁言" + minute.ToString() + "分钟!", false);
				}
				else if (sMsg.MessageContent.Contains("解除禁言"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^解除禁言]")).IsMatch(output.Replace(" ", "")) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 7)
					{
						return 0;
					}
					if (m.Success)
					{
						if (API.MuteGroupMember(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, long.Parse(m.Value), 0))
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "已解除" + m.Value + "的禁言", false);
					}
				}
				else if (sMsg.MessageContent == "取群列表")
				{

					API.GetGroupLists(sMsg.ThisQQ, sMsg.MessageGroupQQ);

				}
				else if (sMsg.MessageContent == "取群成员列表")
				{

					API.GetGroupMemberlists(sMsg.ThisQQ, sMsg.MessageGroupQQ);

				}
				else if (sMsg.MessageContent.Contains("同意") && sMsg.MessageContent.Contains("入群"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if (new Regex("(?i)[^同意入群]").IsMatch(output.Replace(" ", "")) == true)
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
							API.GroupVerificationEvent(PInvoke.plugin_key, sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, GroupVerificationOperateEnum.Agree, PInvoke.EventTypeEnum.Friend_FriendRequest, "同意入群");
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.", false);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message.ToString());
						}
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
							API.GroupVerificationEvent(PInvoke.plugin_key, sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, GroupVerificationOperateEnum.Deny, PInvoke.EventTypeEnum.Friend_FriendRequest, "拒绝入群");
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.", false);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message.ToString());
						}
					}

				}
				else if (sMsg.MessageContent.Contains("同意进群"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^同意进群]")).IsMatch(output.Replace(" ", "")) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 6)
					{
						return 0;
					}
					if (m.Success)
					{
						try
						{
							var ret = API.GroupVerificationEvent(PInvoke.plugin_key, sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, GroupVerificationOperateEnum.Agree, PInvoke.EventTypeEnum.Group_Invited, "同意入群");
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已加入群:" + m.Value, false);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message.ToString());
						}
					}
				}
				else if (sMsg.MessageContent.Contains("拒绝进群"))
				{
					string output = Regex.Replace(sMsg.MessageContent, @"[\d]", string.Empty);
					if ((new Regex("(?i)[^拒绝进群]")).IsMatch(output.Replace(" ", "")) == true)
						return 0;
					Match m = new Regex("\\d+").Match(sMsg.MessageContent);
					if (m.Value.Length < 6)
					{
						return 0;
					}
					if (m.Success)
					{
						try
						{
							API.GroupVerificationEvent(PInvoke.plugin_key, sMsg.ThisQQ, API.EventDics[long.Parse(m.Value)].Item1, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, GroupVerificationOperateEnum.Agree, PInvoke.EventTypeEnum.Group_MemberInvited, "同意入群");
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已拒绝邀请.", false);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message.ToString());
						}
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
							API.FriendverificationEvent(PInvoke.plugin_key, sMsg.ThisQQ, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, FriendVerificationOperateEnum.Agree);
							API.EventDics.Remove(long.Parse(m.Value));
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.", false);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message.ToString());
						}

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
							API.FriendverificationEvent(PInvoke.plugin_key, sMsg.ThisQQ, long.Parse(m.Value), API.EventDics[long.Parse(m.Value)].Item3, FriendVerificationOperateEnum.Deny);
							API.EventDics.Remove(long.Parse(m.Value.ToString()));
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已处理完毕.", false);
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
					string szQQID = "123";
					int Number = 0;
					MatchCollection matches = new Regex("\\d+").Matches(sMsg.MessageContent);
					if (matches.Count > 2) return 0;
					foreach (Match match in matches)
					{
						if (match.Value.ToString().Length >= 6)
						{
							szQQID = match.Value;
						}
						else if (match.Value.ToString().Length < 3)
						{
							Number = int.Parse(match.Value);
						}
					}
					List<List<string>> MsgList = SqliHelper.ReadData("消息记录", new string[] { "GroupID", "QQID", "MessageReq", "MessageRandom", "TimeStamp" }, "ORDER BY ID DESC LIMIT " + Number, "QQID like '" + szQQID + "'");
					int n = 0;
					foreach (List<string> list in MsgList)
					{
						n = n + 1;
						bool sucess = API.Undo_GroupEvent(PInvoke.plugin_key, sMsg.ThisQQ, long.Parse(list[0]), long.Parse(list[3]), int.Parse(list[2]));
						if (sucess)
							API.SendGroupMsg(PInvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "已撤回" + szQQID + "最近消息" + n.ToString() + "条", false);
					}


				}
				else if (sMsg.MessageContent == "压力测试")
				{
					token = cts.Token;
					long thisqq = sMsg.ThisQQ;
					Task.Factory.StartNew(() =>
					{
						int i = 0;
						while (!token.IsCancellationRequested)
						{
							i = i + 1;
							API.SendGroupMsg(plugin_key, thisqq, 66847886, "小栗子机器人插件\r\n发送群消息压力测试\r\n测试~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\r\n" + DateTime.Now.ToString(), false);
							//API.SendGroupMsg(API.MyQQ, 66847886, "小栗子机器人插件\r\n发送群消息压力测试\r\n测试~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\r\n" +DateTime.Now.ToString());
							API.SendGroupMsg(plugin_key, thisqq, 66847886, i.ToString(), false);
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
					//复读机
					//API.SendGroupMsg(PInvoke.plugin_key,sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString() + "]" + "你发送了这样的消息:" + sMsg.MessageContent,false);
				}
			}
			return 0;
		}
		#endregion
	}

}
