

using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static XiaolzCSharp.PInvoke;

namespace XiaolzCSharp
{
	public class API
	{
		public static Dictionary<long, Tuple<long, string, long, uint>> EventDics = new Dictionary<long, Tuple<long, string, long, uint>>();
		public static bool MsgRecod=false;
		public static string MyQQ = "";

		#region 导出函数给框架并取到两个参数值
		[DllExport(CallingConvention = CallingConvention.StdCall)]
		//[return: MarshalAs(UnmanagedType.LPStr)]
		public static string apprun([MarshalAs(UnmanagedType.LPStr)] string apidata, [MarshalAs(UnmanagedType.LPStr)] string pluginkey)
		{
			jsonstr = apidata;
			plugin_key = pluginkey;

			var json = "";
			json = AddPermission("输出日志", json);
			json = AddPermission("发送好友消息", json);
			json = AddPermission("查询好友信息", json);
			json = AddPermission("发送群消息", json);
			json = AddPermission("取图片下载地址", json);
			json = AddPermission("取好友列表", json);
			json = AddPermission("取群成员列表", json);
			json = AddPermission("取群列表", json);
			json = AddPermission("取框架QQ", json);
			json = AddPermission("处理好友验证事件", json);
			json = AddPermission("处理群验证事件", json);
			json = AddPermission("撤回消息_群聊", json);
			json = AddPermission("撤回消息_私聊本身", json);

			object jsonkey = new JavaScriptSerializer().DeserializeObject(json);
			var resultJson = new JavaScriptSerializer().Serialize(new { needapilist = jsonkey });
			var App_Info = new AppInfo();

			App_Info.sdkv = "2.7.5";
			App_Info.appname = "测试插件";
			App_Info.author = "插件作者";
			App_Info.describe = "这是一个测试插件" + "\r\n" + "可以用此空壳来开发插件" + "\r\n" + "官网地址：http://www.xiaolz.cn/";
			App_Info.appv = "1.0.0";
			GC.KeepAlive(appEnableFunc);
			App_Info.useproaddres = Marshal.GetFunctionPointerForDelegate(appEnableFunc).ToInt64();
			GC.KeepAlive(AppDisabledEvent);
			App_Info.banproaddres = Marshal.GetFunctionPointerForDelegate(AppDisabledEvent).ToInt64();
			GC.KeepAlive(AppSettingEvent);
			App_Info.setproaddres = Marshal.GetFunctionPointerForDelegate(AppSettingEvent).ToInt64();
			GC.KeepAlive(AppUninstallEvent);
			App_Info.unitproaddres = Marshal.GetFunctionPointerForDelegate(AppUninstallEvent).ToInt64();
			GC.KeepAlive(Main.funRecvicePrivateMsg);
			App_Info.friendmsaddres = Marshal.GetFunctionPointerForDelegate(Main.funRecvicePrivateMsg).ToInt64();
			GC.KeepAlive(Main.funRecviceGroupMsg);
			App_Info.groupmsaddres = Marshal.GetFunctionPointerForDelegate(Main.funRecviceGroupMsg).ToInt64();
			GC.KeepAlive(funEvent);
			App_Info.eventmsaddres = Marshal.GetFunctionPointerForDelegate(funEvent).ToInt64();

			App_Info.data = "\\\\" + resultJson + "\\\\";
			string jsonstring = (new JavaScriptSerializer()).Serialize(App_Info);
			jsonstring = jsonstring.Replace("\"\\\\", "").Replace("\\\\\"", "").Replace("\\", "");

			return jsonstring;
		}
		public static string AddPermission(string desc, string json)
		{
			var Permission = new MyData
			{
				PermissionList = new Needapilist
				{
					state = "1",
					safe = "1",
					desc = desc
				}
			};
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			var jsonstring = serializer.Serialize(Permission).Replace("PermissionList", desc);
			if (string.IsNullOrEmpty(json))
			{
				return jsonstring;
			}
			else
			{
				return (json + jsonstring).Replace("}{", ",");
			}
		}
		#endregion

		#region 插件启动	
		public static DelegateAppEnable appEnableFunc = new DelegateAppEnable(appEnable);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int DelegateAppEnable();
		public static int appEnable()		{

			string res = GetLoginQQ();
			string sqlite3path = System.Environment.CurrentDirectory + "\\bin\\sqlite3.dll"; 
			SqliHelper.SetDllDirectory(sqlite3path);
			var tablevalue = new List<string[]>() {
				new string[]{ "`GroupID` TEXT", "`time` TEXT" },
				new string[]{ "`QQID` TEXT", "`time` TEXT" },
				new string[]{ "`QQID` TEXT", "`time` TEXT" },
				new string[]{ "`GroupID` TEXT", "`QQID` TEXT", "`MessageReq` NUMERIC", "`MessageRandom` NUMERIC", "`TimeStamp` NUMERIC" , "`Msg` TEXT" }
			};
			SqliHelper.CreateTable(new string[] { "授权群号", "高级权限", "中级权限","消息记录" }, tablevalue);
			return 0;
		}
		#endregion
		#region 框架重启		
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate void DelegateRestart(string pkey);
		public int ReStart()
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["框架重启"];
			DelegateRestart ReStartAPI = (DelegateRestart)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateRestart));			
			ReStartAPI(plugin_key);
			return 0;
		}
		#endregion
		#region 插件卸载		
		public static DelegateAppUnInstall AppUninstallEvent = new DelegateAppUnInstall(AppUnInstall);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int DelegateAppUnInstall();
		public static int AppUnInstall()
		{
			//托管程序集插件不支持FreeLibrary的方式卸载插件,只支持AppDomain的方式卸载,所以要删除插件,必须先关掉框架,手动删除.
			return 0;
		}

		#endregion
		#region 插件禁用
		public static DelegateAppDisabled AppDisabledEvent = new DelegateAppDisabled(appDisable);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int DelegateAppDisabled();
		public static int appDisable()
		{
			return 0;
		}
		#endregion
		#region 取框架QQ
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public delegate string DelegateGetLoginQQ(string pkey);
		public static string GetLoginQQ()
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["取框架QQ"];
			DelegateGetLoginQQ GetLoginQQAPI = (DelegateGetLoginQQ)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateGetLoginQQ));		
			var RetJson = GetLoginQQAPI(plugin_key);
			try
			{
				dynamic root = new JavaScriptSerializer().Deserialize<Dictionary<string, Dictionary<string, object>>>(RetJson);
				var QQlist = root[root.Keys[0]];
				for (var i = 0; i <= root.Count; i++)
				{
					if (QQlist.Keys[i] == "12345") //控制插件被滥用,如果不是该QQ号码登录就禁用发送信息功能
					{
						RobotQQ = QQlist.Keys[i];
						PluginStatus = true;
						return RetJson;
					}
					else if (QQlist.Keys[i] != "2222222")
					{
						RobotQQ = QQlist.Keys[i];
						PluginStatus = true;
						return RetJson;
					}
					else if (QQlist.Keys[i] != "33333")
					{
						RobotQQ = QQlist.Keys[i];
						PluginStatus = true;
						return RetJson;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message.ToString());
			}
			//PluginStatus = false;
			PluginStatus = true;//自己改下
			GetLoginQQAPI = null;
			return "";
		}
		#endregion
		#region 获取clientkey
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate string GetClientKey(string pkey, long thisQQ);
		public string GetClientKeyEvent(long thisQQ)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["获取clientkey"];
			GetClientKey GetClientKeyAPI = (GetClientKey)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GetClientKey));
			var ret = GetClientKeyAPI(plugin_key, thisQQ);
			GetClientKeyAPI = null;
			return ret;
		}
		#endregion
		#region 获取pskey
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string GetPSKey(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string domain);
		public string GetPSKeyEvent(long thisQQ, string domain)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["获取clientkey"];
			GetPSKey GetPSKeyAPI = (GetPSKey)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GetPSKey));		
			var ret = GetPSKeyAPI(plugin_key, thisQQ, domain);
			GetPSKeyAPI = null;
			return ret;
		}

		#endregion
		#region 获取skey
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string GetSKey(string pkey, long thisQQ);
		public string GetSKeyEvent(long thisQQ, string domain)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["获取clientkey"];
			GetSKey GetSKeyAPI = (GetSKey)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GetSKey));		
			var ret = GetSKeyAPI(plugin_key, thisQQ);
			GetSKeyAPI = null;	
			return ret;
		}

		#endregion
		#region 输出日志
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string OutputLog(string pkey, [MarshalAs(UnmanagedType.LPStr)] string message, int text_color, int background_color);
		public static string OutLog(string message, int text_color = 16711680, int background_color = 16777215)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["输出日志"];
			OutputLog outputLogAPI = (OutputLog)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(OutputLog));		
			var ret = outputLogAPI(plugin_key, message, text_color, background_color);
			outputLogAPI = null;
			return ret;
		}

		#endregion
		#region 插件设置
		public static DelegateAppSetting AppSettingEvent = new DelegateAppSetting(AppSetting);
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate void DelegateAppSetting();
		public static void AppSetting()
		{
			Form1 frm = new Form1();
			frm.Show();
		}
		#endregion
		#region 插件事件
		public static DelegatefunEvent funEvent = new DelegatefunEvent(OnEvent);
		public delegate void DelegatefunEvent(ref EventTypeBase EvenType);
		public static void OnEvent(ref EventTypeBase EvenType)
		{
			if (EvenType.EventSubType == 0)
			{
				switch (EvenType.EventType)
				{
					case EventTypeEnum.This_SignInSuccess:
						Console.WriteLine("登录成功");
						MyQQ = EvenType.ThisQQ.ToString();
						break;
					case EventTypeEnum.Friend_NewFriend:
						Console.WriteLine("有新好友");
						break;
					case EventTypeEnum.Friend_FriendRequest:
						Console.WriteLine("好友请求");
						break;
					case EventTypeEnum.Friend_FriendRequestAccepted:
						Console.WriteLine("对方同意了您的好友请求");
						break;
					case EventTypeEnum.Friend_FriendRequestRefused:
						Console.WriteLine("对方拒绝了您的好友请求");
						break;
					case EventTypeEnum.Friend_Removed:
						Console.WriteLine("被好友删除");
						break;
					case EventTypeEnum.Friend_Blacklist:
						//API.SendPrivateMessage(EvenType.ThisQQ, "12345678", EvenType.TriggerQQName+ "(" + EvenType.TriggerQQ.ToString() +" ) 将机器人加入黑名单");
						API.SendGroupMessage(EvenType.ThisQQ, 64596829, "[@37476230]" + EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + " ) 将机器人加入黑名单");
						break;
					case EventTypeEnum.Group_MemberVerifying:
						API.SendGroupMessage(EvenType.ThisQQ, 64596829, "[@37476230]" + EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + " ) 想加入群: "+ EvenType.SourceGroupName + "(" + EvenType.SourceGroupQQ.ToString() + " )");
						if (EventDics.ContainsKey(EvenType.TriggerQQ) == false)
							EventDics.Add(EvenType.TriggerQQ, new Tuple<long, string, long, uint>(EvenType.SourceGroupQQ, EvenType.TriggerQQName, EvenType.MessageSeq, (uint)EvenType.EventType));
						break;	
					default:
						Console.WriteLine(EvenType.EventType.ToString());
						break;
				}
			}
			else if (EvenType.EventSubType == 1)
			{
				switch (EvenType.EventType)
				{
					case EventTypeEnum.This_SignInSuccess:
						Console.WriteLine("登录成功");
						break;
					case EventTypeEnum.Group_Invited:
						Console.WriteLine("我被邀请加入群");
						break;
					case EventTypeEnum.Group_MemberJoined:
						Console.WriteLine("某人加入了群");
						API.SendGroupMessage(EvenType.ThisQQ, EvenType.SourceGroupQQ, "[@" + EvenType.TriggerQQ.ToString() + "]" +  EvenType.TriggerQQName + ",欢迎你加入本群!");
						break;
					case EventTypeEnum.Group_MemberVerifying:
						Console.WriteLine("某人申请加群");
						break;
					case EventTypeEnum.Group_MemberQuit:
						Console.WriteLine("某人退出了群");
						API.SendGroupMessage(EvenType.ThisQQ, EvenType.SourceGroupQQ, EvenType.TriggerQQName + "已退出本群!");
						break;
					case EventTypeEnum.Group_MemberUndid:
						API.SendGroupMessage(EvenType.ThisQQ, EvenType.SourceGroupQQ, EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + " ) 撤回了一条消息,内容如下:" + EvenType.MessageContent);
						break;
					case EventTypeEnum.Group_MemberInvited:
						Console.WriteLine("某人被邀请入群");
						break;
					case EventTypeEnum.Group_AllowUploadFile:
						Console.WriteLine("群事件_允许上传群文件");
						break;
					case EventTypeEnum.Group_ForbidUploadFile:
						Console.WriteLine("群事件_禁止上传群文件");
						break;
					case EventTypeEnum.Group_AllowUploadPicture:
						Console.WriteLine("群事件_允许上传相册");
						break;
					case EventTypeEnum.Group_ForbidUploadPicture:
						Console.WriteLine("群事件_禁止上传相册");
						break;
					case EventTypeEnum.Friend_NewFriend:
						Console.WriteLine("通过好友的请求");
						break;
					case EventTypeEnum.Friend_FriendRequest:
						Console.WriteLine("对方加你为好友");
						break;
					default:
						Console.WriteLine(EvenType.EventType.ToString());
						break;
				}
			}
			else if (EvenType.EventSubType == 2)
            {
				switch (EvenType.EventType)
				{
					case EventTypeEnum.This_SignInSuccess:
						Console.WriteLine("登录成功");
						break;
					case EventTypeEnum.Group_Invited:
						Console.WriteLine("我被邀请加入群");
						break;
					case EventTypeEnum.Group_MemberJoined:
						Console.WriteLine("某人加入了群");
						break;
					case EventTypeEnum.Group_MemberVerifying:
						Console.WriteLine("某人申请加群");
						break;
					case EventTypeEnum.Group_MemberQuit:
						Console.WriteLine("某人退出了群");
						break;
					case EventTypeEnum.Group_MemberUndid:
						Console.WriteLine(EvenType.OperateQQName + "(" + EvenType.OperateQQ.ToString() + ")" + "删除了文件");
						break;
					case EventTypeEnum.Group_MemberInvited:
						Console.WriteLine("某人被邀请入群");
						break;
					case EventTypeEnum.Group_AllowUploadFile:
						Console.WriteLine("群事件_允许上传群文件");
						break;
					case EventTypeEnum.Group_ForbidUploadFile:
						Console.WriteLine("群事件_禁止上传群文件");
						break;
					case EventTypeEnum.Group_AllowUploadPicture:
						Console.WriteLine("群事件_允许上传相册");
						break;
					case EventTypeEnum.Group_ForbidUploadPicture:
						Console.WriteLine("群事件_禁止上传相册");
						break;
					case EventTypeEnum.Friend_NewFriend:
						Console.WriteLine("对方通过了你的好友的请求");
						break;
					case EventTypeEnum.Friend_FriendRequest:
						//API.SendPrivateMessage(EvenType.ThisQQ, "12345678",  EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + ")对方加机器人为好友,发送了这样的消息:" + EvenType.MessageContent);
						API.SendGroupMessage(EvenType.ThisQQ, 64596829, "[@37476230]" + EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + ")欲加机器人为好友,发送了这样的消息:" + EvenType.MessageContent +",是否同意?");
					    if (EventDics.ContainsKey(EvenType.TriggerQQ) == false)
							EventDics.Add(EvenType.TriggerQQ, new Tuple<long, string, long, uint>(EvenType.SourceGroupQQ, EvenType.TriggerQQName, EvenType.MessageSeq, EvenType.EventSubType));
						break;
					default:
						Console.WriteLine(EvenType.EventType.ToString());
						break;
				}
			}

		}
		#endregion
		#region 处理好友验证事件

		public delegate void DelegateDealFriendEvent(string pkey, long ThisQQ, long TriggerQQ, long MessageSeq, int dealtype);
		public static void DealFriendEvent( long ThisQQ, long TriggerQQ, long MessageSeq,int dealtype)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["处理好友验证事件"];
			DelegateDealFriendEvent DealFriendEventAPI = (DelegateDealFriendEvent)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateDealFriendEvent));
			DealFriendEventAPI(plugin_key, ThisQQ,  TriggerQQ,  MessageSeq,  dealtype);
			DealFriendEventAPI = null;
			return;
		}
		#endregion
		#region 处理群验证事件
	
		public delegate void DelegateDealGroupEvent(string pkey, long thisQQ, long senderQQ, long TriggerQQ, long MessageSeq, int dealtype, uint eventtype, string reason);
		public static void DealGroupEvent( long thisQQ, long sourceGroup, long TriggerQQ,long MessageSeq, int dealtype, uint eventtype,string reason)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["处理群验证事件"];
			DelegateDealGroupEvent DealGroupEventAPI = (DelegateDealGroupEvent)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateDealGroupEvent));
			DealGroupEventAPI(plugin_key, thisQQ, sourceGroup, TriggerQQ, MessageSeq, dealtype, eventtype, reason);
			DealGroupEventAPI = null;
			return;
		}
		#endregion
		#region 发送私聊消息
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public delegate string SendPivateMsg(string pkey, long ThisQQ, long SenderQQ, IntPtr MessageContent, ref long MessageRandom, ref int MessageReq);
		public static string SendPrivateMessage(long ThisQQ, long SenderQQ, string MessageContent)
		{
			if (PluginStatus == false)
			{
				return "";
			}
			var res = "";
			try
			{
				dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
				int ptr = json["发送好友消息"];
				SendPivateMsg SendPrivateMsgAPI = (SendPivateMsg)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(SendPivateMsg));			
				long MessageRandom = 0;
				int MessageReq = 0;
				GC.KeepAlive(SendPrivateMsgAPI);
				//StringBuilder sb = new StringBuilder(MessageContent);
				res = SendPrivateMsgAPI(plugin_key, ThisQQ, SenderQQ, Marshal.StringToHGlobalAnsi(MessageContent), ref MessageRandom, ref MessageReq);
				SendPrivateMsgAPI = null;
			}
			catch (Exception ex)
			{
				res = ex.ToString();
			}

			return res;
		}
		#endregion
		#region 发送群聊消息
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public delegate string SendGroupMsg(string pkey, long ThisQQ, long GroupQQ, IntPtr MessageContent, bool Anonymous);
		public static string SendGroupMessage(long ThisQQ, long GroupQQ, string MessageContent)
		{
			if (PluginStatus == false)
			{
				return "";
			}
			var res = "";
			try
			{
				dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
				int ptr = json["发送群消息"];
				SendGroupMsg SendGroupMsgAPI = (SendGroupMsg)Marshal.GetDelegateForFunctionPointer((System.IntPtr)ptr, typeof(SendGroupMsg));
				//StringBuilder sb = new StringBuilder(MessageContent);
				res = SendGroupMsgAPI(plugin_key, ThisQQ, GroupQQ, Marshal.StringToHGlobalAnsi(MessageContent), false);				
				SendGroupMsgAPI = null;
			}
			catch (Exception ex)
			{
				res = ex.ToString();
			}
			return res;
		}
		#endregion
	
		#region 发送好友图片
		public string SendFriendImage(long thisQQ, long friendQQ, string picpath, bool is_flash)
		{
			string piccode = UploadFriendImageEvent(thisQQ, friendQQ, picpath, is_flash);
			return SendPrivateMessage(thisQQ, friendQQ, piccode);
		}
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string UploadFriendImage(string pkey, long thisQQ, long friendQQ, bool is_flash, [MarshalAs(UnmanagedType.LPArray)] byte[] pic, int picsize);
		public string UploadFriendImageEvent(long thisQQ, long friendQQ, string picpath, bool is_flash)
		{
			Bitmap bitmap = new Bitmap(picpath);
			byte[] picture = GetByteArrayByImage(bitmap);
			int picsize = picture.Length;
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["上传好友图片"];
			UploadFriendImage UploadFriendImageAPI = (UploadFriendImage)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(UploadFriendImage));		
			string ret = UploadFriendImageAPI(plugin_key, thisQQ, friendQQ, is_flash, picture, picsize);
			UploadFriendImageAPI = null;
			return ret;
		}
		private byte[] GetByteArrayByImage(Bitmap bitmap)
		{
			byte[] result = null;
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				bitmap.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				result = array;
			}
			catch
			{
				result = null;
			}
			return result;
		}

		#endregion
		#region 发送群图片
		public string SendGroupImage(long thisQQ, long groupQQ, string picpath, bool is_flash)
		{
			string piccode = UploadGroupImage(thisQQ, groupQQ, picpath, is_flash);
			return SendGroupMessage(thisQQ, groupQQ, piccode);
		}
		#endregion
		#region 上传群图片
		public string UploadGroupImage(long thisQQ, long groupQQ, string picpath, bool is_flash)
		{
			Bitmap bitmap = new Bitmap(picpath);
			byte[] picture = GetByteArrayByImage(bitmap);
			int picsize = picture.Length;
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["上传群图片"];
			UploadFriendImage UploadFriendImageAPI = (UploadFriendImage)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(UploadFriendImage));
			GC.Collect();
			string ret = UploadFriendImageAPI(plugin_key, thisQQ, groupQQ, is_flash, picture, picsize);
			UploadFriendImageAPI = null;
			return ret;
		}
		#endregion
		#region 上传头像
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string UploadAvatar(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPArray)] byte[] pic, int picsize);
		public string UploadAvatarEvent(long thisQQ, string picpath)
		{
			Bitmap bitmap = new Bitmap(picpath);
			byte[] picture = GetByteArrayByImage(bitmap);
			int picsize = picture.Length;
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["上传头像"];
			UploadAvatar UploadAvatarAPI = (UploadAvatar)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(UploadAvatar));
			var ret = UploadAvatarAPI(plugin_key, thisQQ, picture, picsize);
			UploadAvatarAPI = null;
			return ret;
		}
		#endregion
		#region 上传群头像
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool UploadGroupAvatar(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPArray)] byte[] pic, int picsize);
		public bool UploadGroupAvatarEvent(long thisQQ, long groupQQ, string picpath)
		{
			Bitmap bitmap = new Bitmap(picpath);
			byte[] picture = GetByteArrayByImage(bitmap);
			int picsize = picture.Length;
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["上传群头像"];
			UploadGroupAvatar UploadGroupAvatarAPI = (UploadGroupAvatar)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(UploadGroupAvatar));
			var ret = UploadGroupAvatarAPI(plugin_key, thisQQ, groupQQ, picture, picsize);
			UploadGroupAvatarAPI = null;
			return ret;
		}

		#endregion
		#region 上传好友语音
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string UploadFriendAudio(string pkey, long thisQQ, long friendQQ, int audio_type, [MarshalAs(UnmanagedType.LPStr)] string audio_text, [MarshalAs(UnmanagedType.LPArray)] byte[] audio, int audiosize);
		public string UploadFriendAudioEvent(long thisQQ, long friendQQ, AudioTypeEnum audio_type, string audio_text, byte[] audio)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["上传好友语音"];
			UploadFriendAudio UploadFriendAudioAPI = (UploadFriendAudio)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(UploadFriendAudio));	
			var ret = UploadFriendAudioAPI(plugin_key, thisQQ, friendQQ, Convert.ToInt32(audio_type), audio_text, audio, audio.Length);
			UploadFriendAudioAPI = null;
			return ret;
		}
		public byte[] SilkDecoding(string audio_path)
		{
			SilkHelp silkHelp = new SilkHelp();
			return silkHelp.SilkDecoding(audio_path);
		}
		public byte[] SilkEncoding(string audio_path)
		{
			SilkHelp silkHelp = new SilkHelp();
			return silkHelp.SilkEncoding(audio_path);
		}

		#endregion
		#region 上传群语音
		[return: MarshalAs(UnmanagedType.LPStr)]
		public string UploadGroupAudio(long thisQQ, long groupQQ, AudioTypeEnum audio_type, [MarshalAs(UnmanagedType.LPStr)] string audio_text, [MarshalAs(UnmanagedType.LPArray)] byte[] audio)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["上传群语音"];
			UploadFriendAudio UploadFriendAudioAPI = (UploadFriendAudio)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(UploadFriendAudio));		
			var ret = UploadFriendAudioAPI(plugin_key, thisQQ, groupQQ, Convert.ToInt32(audio_type), audio_text, audio, audio.Length);
			UploadFriendAudioAPI = null;
			return ret;
		}
		#endregion
		#region 获取图片地址
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public delegate string DelegateGetImageDownloadLink(string pkey, string guid, long thisQQ, long groupQQ);
		public static string GetImageDownloadLink(long thisQQ, long sendQQ, long groupQQ, string ImgGuid)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["取图片下载地址"];
			DelegateGetImageDownloadLink GetImageLink = (DelegateGetImageDownloadLink)Marshal.GetDelegateForFunctionPointer((System.IntPtr)ptr, typeof(DelegateGetImageDownloadLink));		
			var ImgUrl = GetImageLink(plugin_key, ImgGuid, thisQQ, groupQQ);
			if (groupQQ == 0)
			{
				SendPrivateMessage(thisQQ, sendQQ, "图片地址为:" + ImgUrl + "\r\n");
			}
			else
			{
				SendGroupMessage(thisQQ, groupQQ, "图片地址为:" + ImgUrl + "\r\n");
			}
			GetImageLink = null;
			return "1";
		}
		#endregion
		#region 取好友列表
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int DelegateGetFriendList(string pkey, long thisQQ, ref DataArray[] DataInfo);
		public static int GetFriendList(long thisQQ, long sendQQ)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["取好友列表"];
			DelegateGetFriendList GetFriendListAPI = (DelegateGetFriendList)Marshal.GetDelegateForFunctionPointer((System.IntPtr)ptr, typeof(DelegateGetFriendList));		
			DataArray[] ptrArray = new DataArray[2];
			int count = GetFriendListAPI(plugin_key, thisQQ, ref ptrArray);
			if (count > 0)
			{
				List<string> list = new List<string>();
				byte[] pAddrBytes = ptrArray[0].pAddrList;
				for (int i = 0; i < count; i++)
				{
					byte[] readByte = new byte[4];
					Array.Copy(pAddrBytes, i * 4, readByte, 0, readByte.Length);
					IntPtr StuctPtr = new IntPtr(BitConverter.ToInt32(readByte, 0));
					FriendInfo info = (FriendInfo)Marshal.PtrToStructure(StuctPtr, typeof(FriendInfo));
					list.Add(info.QQNumber.ToString() + "-" + info.Name);
				}
				SendPrivateMessage(thisQQ, sendQQ, "好友列表:" + "\r\n" + string.Join("\r\n", list));
			}
			GetFriendListAPI = null;
			return count;
		}
		#endregion
		#region 查询好友信息
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate bool DelegateGetFriendInfo(string pkey, long thisQQ, long otherQQ, ref GetFriendDataInfo[] friendInfos);
		public static void GetFriendData(long thisQQ, long otherQQ)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["查询好友信息"];
			DelegateGetFriendInfo GetFriendInfoAPI = (DelegateGetFriendInfo)Marshal.GetDelegateForFunctionPointer((System.IntPtr)ptr, typeof(DelegateGetFriendInfo));		
			GetFriendDataInfo[] pFriendInfo = new GetFriendDataInfo[2];
			bool res = GetFriendInfoAPI(plugin_key, thisQQ, otherQQ, ref pFriendInfo);
			if (res == true)
			{
				var result = (new JavaScriptSerializer()).Serialize(pFriendInfo[0].friendInfo);
				SendPrivateMessage(thisQQ, otherQQ, result);
			}
			else
			{
				SendPrivateMessage(thisQQ, otherQQ, "查询好友信息失败");
			}
			GetFriendInfoAPI = null;
		}
		#endregion
		#region 取群成员列表
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int DelegateGetgroupMemberlist(string pkey, long thisQQ, long groupQQ, ref DataArray[] DataInfo);
		public static int GetgroupMemberlist(long thisQQ, long groupQQ)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["取群成员列表"];
			DataArray[] ptrArray = new DataArray[2];
			DelegateGetgroupMemberlist GetgroupMemberlistAPI = (DelegateGetgroupMemberlist)Marshal.GetDelegateForFunctionPointer((System.IntPtr)ptr, typeof(DelegateGetgroupMemberlist));		
			int count = GetgroupMemberlistAPI(plugin_key, thisQQ, groupQQ, ref ptrArray);
			if (count > 0)
			{
				List<string> list = new List<string>();
				byte[] pAddrBytes = ptrArray[0].pAddrList;
				for (int i = 0; i < count; i++)
				{
					byte[] readByte = new byte[4];
					Array.Copy(pAddrBytes, i * 4, readByte, 0, readByte.Length);
					IntPtr StuctPtr = new IntPtr(BitConverter.ToInt32(readByte, 0));
					GroupMemberInfo info = (GroupMemberInfo)Marshal.PtrToStructure(StuctPtr, typeof(GroupMemberInfo));
					list.Add(info.QQNumber + "-" + info.Name);
				}
				SendGroupMessage(thisQQ, groupQQ, "群列表:" + "\r\n" + string.Join("\r\n", list));
			}
			GetgroupMemberlistAPI = null;
			return count;
		}
		#endregion
		#region 取群列表
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate int DelegateGetGroupList(string pkey, long thisQQ, ref DataArray[] DataInfo);
		public static int GetGroupList(long thisQQ, long groupQQ)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["取群列表"];
			DelegateGetGroupList GetGroupListAPI = (DelegateGetGroupList)Marshal.GetDelegateForFunctionPointer((System.IntPtr)ptr, typeof(DelegateGetGroupList));		
			DataArray[] ptrArray = new DataArray[2];
			int count = GetGroupListAPI(plugin_key, thisQQ, ref ptrArray);
			if (count > 0)
			{
				List<string> list = new List<string>();
				byte[] pAddrBytes = ptrArray[0].pAddrList;
				for (int i = 0; i < count; i++)
				{
					byte[] readByte = new byte[4];
					Array.Copy(pAddrBytes, i * 4, readByte, 0, readByte.Length);
					IntPtr StuctPtr = new IntPtr(BitConverter.ToInt32(readByte, 0));
					GroupInfo info = (GroupInfo)Marshal.PtrToStructure(StuctPtr, typeof(GroupInfo));
					list.Add(info.GroupID.ToString() + "-" + info.GroupName);
				}
				SendGroupMessage(thisQQ, groupQQ, "群列表:" + "\r\n" + string.Join("\r\n", list));
			}
			GetGroupListAPI = null;
			return count;
		}
		#endregion
		#region 取管理列表
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string DelegateGetadministratorList(string pkey, long thisQQ, long gruopNumber);
		public string[] GetAdministratorList(long thisQQ, long gruopNumber)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["取管理层列表"];
			DelegateGetadministratorList GetAdministratorListAPI = (DelegateGetadministratorList)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateGetadministratorList));
			var ret = GetAdministratorListAPI(plugin_key, thisQQ, gruopNumber);
			string[] adminlist = ret.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			GetAdministratorListAPI = null;
			return adminlist;
		}
		#endregion
		#region 查询群信息
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate bool DelegateGetGroupInfo(string pkey, long thisQQ, long otherGroupQQ, ref GetGroupDataInfo[] GroupInfos);
		public static void GetGroupData(long thisQQ, long otherGroupQQ)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["查询好友信息"];
			DelegateGetGroupInfo GetGroupInfoAPI = (DelegateGetGroupInfo)Marshal.GetDelegateForFunctionPointer((System.IntPtr)ptr, typeof(DelegateGetGroupInfo));
			GetGroupDataInfo[] pGroupInfo = new GetGroupDataInfo[2];
			bool res = GetGroupInfoAPI(plugin_key, thisQQ, otherGroupQQ, ref pGroupInfo);
			if (res == true)
			{
				var result = (new JavaScriptSerializer()).Serialize(pGroupInfo[0].GroupInfo);
				SendPrivateMessage(thisQQ, otherGroupQQ, result);
			}
			else
			{
				SendPrivateMessage(thisQQ, otherGroupQQ, "查询好友信息失败");
			}
			GetGroupInfoAPI = null;
		}
		#endregion
		#region 解散群
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool DissolveGroup(string pkey, long thisQQ, long gruopNumber);
		public bool DissolveGroupEvent(long thisQQ, long groupQQ)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["解散群"];
			DissolveGroup DissolveGroupAPI = (DissolveGroup)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DissolveGroup));
			var res = DissolveGroupAPI(plugin_key, thisQQ, groupQQ);
			DissolveGroupAPI = null;
			return res;
		}

		#endregion
		#region 撤回消息_私聊本身
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool DelegateUndoPrivate(string pkey, long thisQQ, long otherQQ, long message_random, int message_req, int time);
		public bool Undo_PrivateEvent(long thisQQ, long otherQQ, long message_random, int message_req, int time)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["撤回消息_私聊本身"];
			DelegateUndoPrivate UndoPrivateAPI = (DelegateUndoPrivate)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateUndoPrivate));
			var res = UndoPrivateAPI(plugin_key, thisQQ, otherQQ, message_random, message_req, time);
			UndoPrivateAPI = null;
			return res;
		}
		#endregion
		#region 撤回消息_群聊
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool DelegateUndoGroup(string pkey, long thisQQ, long groupQQ, long message_random, int message_req);
		public static bool Undo_GroupEvent(long thisQQ, long groupQQ, long message_random, int message_req)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["撤回消息_群聊"];
			DelegateUndoGroup UndoGroupApi = (DelegateUndoGroup)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateUndoGroup));
			var res = UndoGroupApi(plugin_key, thisQQ, groupQQ, message_random, message_req);
			UndoGroupApi = null;
			return res;
		}
		#endregion
		#region 发送群临时消息
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string SendGroupTemporaryMessage(string pkey, long thisQQ, long groupQQ, long otherQQ, [MarshalAs(UnmanagedType.LPStr)] string content, ref long random, ref int req);
		public string SendGroupTemporaryMessageEvent(long thisQQ, long groupQQ, long otherQQ, string content, long random = 0, int req = 0)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["发送群临时消息"];
			SendGroupTemporaryMessage SendGroupTemporaryMessageAPI = (SendGroupTemporaryMessage)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(SendGroupTemporaryMessage));
			var res = SendGroupTemporaryMessageAPI(plugin_key, thisQQ, groupQQ, otherQQ, content, ref random, ref req);
			SendGroupTemporaryMessageAPI = null;
			return res;
		}
		#endregion
		#region 发送好友json消息
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string SendFriendJSONMessage(string pkey, long thisQQ, long friendQQ, [MarshalAs(UnmanagedType.LPStr)] string json_content);
		public string SendFriendJSONMessageEvent(long thisQQ, long friendQQ, string json_content)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["发送好友json消息"];
			SendFriendJSONMessage SendFriendJSONMessageAPI = (SendFriendJSONMessage)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(SendFriendJSONMessage));
			var res = SendFriendJSONMessageAPI(plugin_key, thisQQ, friendQQ, json_content);
			SendFriendJSONMessageAPI = null;
			return res;
		}
		#endregion
		#region 更改私聊消息内容
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool ModifyPrivateMessageContent(string pkey, [MarshalAs(UnmanagedType.LPStr)] string data_pointer, [MarshalAs(UnmanagedType.LPStr)] string new_message_content);
		public bool ModifyPrivateMessageContentEvent(string data_pointer, string new_message_content)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["创建群文件夹"];
			ModifyPrivateMessageContent ModifyPrivateMessage = (ModifyPrivateMessageContent)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(ModifyPrivateMessageContent));
			var res = ModifyPrivateMessage(plugin_key, data_pointer, new_message_content);
			ModifyPrivateMessage = null;
			return res;
		}

		#endregion
		#region 更改群聊消息内容
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool ModifyGroupMessageContent(string pkey, [MarshalAs(UnmanagedType.LPStr)] string data_pointer, [MarshalAs(UnmanagedType.LPStr)] string new_message_content);
		public bool ModifyGroupMessageContentEvent(string data_pointer, string new_message_content)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["创建群文件夹"];
			ModifyGroupMessageContent ModifyGroupMessage = (ModifyGroupMessageContent)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(ModifyGroupMessageContent));
			var res = ModifyGroupMessage(plugin_key, data_pointer, new_message_content);
			ModifyGroupMessage = null;
			return res;
		}

		#endregion
		#region 处理好友验证事件
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate void DelegateFriendVerificationEvent(string pkey, long thisQQ, long triggerQQ, long message_seq, FriendVerificationOperateEnum operate_type);
		public void FriendVerificationEvent(long thisQQ, long triggerQQ, long message_seq, FriendVerificationOperateEnum operate_type)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["处理好友验证事件"];
			DelegateFriendVerificationEvent FriendVerificationEventAPI = (DelegateFriendVerificationEvent)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateFriendVerificationEvent));
			FriendVerificationEventAPI(plugin_key, thisQQ, triggerQQ, message_seq, operate_type);
			FriendVerificationEventAPI = null;
		}
		#endregion
		#region 处理好友验证事件
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate void DelegateGroupVerificationEvent(string pkey, long thisQQ, long source_groupQQ, long triggerQQ, long message_seq, GroupVerificationOperateEnum operate_type, EventTypeEnum event_type, [MarshalAs(UnmanagedType.LPStr)] string refuse_reason);
		public void GroupVerificationEvent(long thisQQ, long source_groupQQ, long triggerQQ, long message_seq, GroupVerificationOperateEnum operate_type, EventTypeEnum event_type, string refuse_reason = "")
		{
			if (event_type == EventTypeEnum.Group_MemberVerifying || event_type == EventTypeEnum.Group_Invited)
			{
				dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
				int ptr = json["处理好友验证事件"];
				DelegateGroupVerificationEvent GroupVerificationEventAPI = (DelegateGroupVerificationEvent)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateGroupVerificationEvent));
				GroupVerificationEventAPI(plugin_key, thisQQ, source_groupQQ, triggerQQ, message_seq, operate_type, event_type, refuse_reason);
				GroupVerificationEventAPI = null;
			}
		}
		#endregion

		#region 文件处理
		#region 好友文件转发至好友
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool FriendFileToFriend(string pkey, long thisQQ, long sourceQQ, long targetQQ, [MarshalAs(UnmanagedType.LPStr)] string fileID, [MarshalAs(UnmanagedType.LPStr)] string file_name, long file_size, ref int msgReq, ref long Random, ref int time);
		public bool FriendFileToFriendEvent(long thisQQ, long sourceQQ, long targetQQ, string fileID, string file_name, long file_size, int msgReq = 0, long Random = 0, int time = 0)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["好友文件转发至好友"];
			FriendFileToFriend FriendFileToFriendAPI = (FriendFileToFriend)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(FriendFileToFriend));
			var res = FriendFileToFriendAPI(plugin_key, thisQQ, sourceQQ, targetQQ, fileID, file_name, file_size, ref msgReq, ref Random, ref time);
			FriendFileToFriendAPI = null;
			return res;
		}
		#endregion
		#region 保存文件到微云
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string DelegateSaveFileToWeiYun(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string file_id);
		public string SaveFileToWeiYunEvent(long thisQQ, long groupQQ, string file_id)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["保存文件到微云"];
			DelegateSaveFileToWeiYun SaveFileToWeiYunAPI = (DelegateSaveFileToWeiYun)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateSaveFileToWeiYun));
			var res = SaveFileToWeiYunAPI(plugin_key, thisQQ, groupQQ, file_id);
			SaveFileToWeiYunAPI = null;
			return res;
		}
		#endregion
		#region 查看转发聊天记录内容
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate void DelegateReadForwardedChatHistory(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string resID, [MarshalAs(UnmanagedType.LPStr)] ref string retPtr);
		public void ReadForwardedChatHistoryEvent(long thisQQ, string resID)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["查看转发聊天记录内容"];
			DelegateReadForwardedChatHistory ReadForwardedChatHistoryAPI = (DelegateReadForwardedChatHistory)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(DelegateReadForwardedChatHistory));
			string RetValue = "";
			ReadForwardedChatHistoryAPI(plugin_key, thisQQ, resID, ref RetValue);
			ReadForwardedChatHistoryAPI = null;
		}
		#endregion
		#region 上传群文件
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string UploadGroupFile(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string path, [MarshalAs(UnmanagedType.LPStr)] string folder);
		public string UploadGroupFileEvent(long thisQQ, long groupQQ, string path, string folder)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["上传群文件"];
			UploadGroupFile UploadGroupFileAPI = (UploadGroupFile)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(UploadGroupFile));
			var res = UploadGroupFileAPI(plugin_key, thisQQ, groupQQ, path, folder);
			UploadGroupFileAPI = null;
			return res;
		}
		#endregion
		#region 群文件转发至群
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private delegate bool ForwardGroupFileToGroup(string pkey, long thisQQ, long source_groupQQ, long target_groupQQ, [MarshalAs(UnmanagedType.LPStr)] string fileID);
		public bool ForwardGroupFileToGroupEvent(long thisQQ, long source_groupQQ, long target_groupQQ, string fileID)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["群文件转发至群"];
			ForwardGroupFileToGroup ForwardGroupFileToGroupAPI = (ForwardGroupFileToGroup)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(ForwardGroupFileToGroup));
			var res = ForwardGroupFileToGroupAPI(plugin_key, thisQQ, source_groupQQ, target_groupQQ, fileID);
			ForwardGroupFileToGroupAPI = null;
			return res;
		}
		#endregion
		#region 取群文件列表
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public delegate string GetGroupFileList(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string folder, ref GroupFileInfoDataList[] groupFileInfoDataLists);
		public List<GroupFileInformation> GetGroupFileListEvent(long thisQQ, long groupQQ, string folder)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["取群文件列表"];
			GetGroupFileList GetGroupFileListAPI = (GetGroupFileList)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GetGroupFileList));
			GroupFileInfoDataList[] pdatalist = new GroupFileInfoDataList[2];
			string ret = GetGroupFileListAPI(plugin_key, thisQQ, groupQQ, folder, ref pdatalist);
			if (pdatalist[0].Amount > 0)
			{
				List<GroupFileInformation> list = new List<GroupFileInformation>();
				int i = 0;
				while (i < pdatalist[0].Amount)
				{
					byte[] recbyte = new byte[4];
					Array.Copy(pdatalist[0].pAddrList, i * 4, recbyte, 0, recbyte.Length);
					IntPtr pStruct = new IntPtr(BitConverter.ToInt32(recbyte, 0));
					GroupFileInformation gf = (GroupFileInformation)Marshal.PtrToStructure(pStruct, typeof(GroupFileInformation));
					list.Add(gf);
					i += 1;
				}
				GetGroupFileListAPI = null;
				return list;
			}
			GetGroupFileListAPI = null;
			return null;
		}
		#endregion
		#region 创建群文件夹
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private delegate string CreateGroupFolder(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string folder);
		public string CreateGroupFolderEvent(long thisQQ, long groupQQ, string folder)
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			int ptr = json["创建群文件夹"];
			CreateGroupFolder CreateGroupFolderAPI = (CreateGroupFolder)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(CreateGroupFolder));
			var res = CreateGroupFolderAPI(plugin_key, thisQQ, groupQQ, folder);
			CreateGroupFolderAPI = null;
			return res;
		}
		#endregion


		#endregion


		//#region 分享音乐
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate bool ShareMusic(string pkey, long thisQQ, long otherQQ, [MarshalAs(UnmanagedType.LPStr)] string music_name, [MarshalAs(UnmanagedType.LPStr)] string artist_name, [MarshalAs(UnmanagedType.LPStr)] string redirect_link, [MarshalAs(UnmanagedType.LPStr)] string cover_link, [MarshalAs(UnmanagedType.LPStr)] string file_path, MusicAppTypeEnum app_type, MusicShare_Type share_type);
		//public bool ShareMusicEvent(long thisQQ, long otherQQ, string music_name, string artist_name, string redirect_link, string cover_link, string file_path, MusicAppTypeEnum app_type = MusicAppTypeEnum.QQMusic, MusicShare_Type share_type = MusicShare_Type.GroupMsg)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["分享音乐"];
		//	ShareMusic ShareMusicAPI = (ShareMusic)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(ShareMusic));
		//	var res = ShareMusicAPI(plugin_key, thisQQ, otherQQ, music_name, artist_name, redirect_link, cover_link, file_path, app_type, share_type);
		//	ShareMusicAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 发送免费礼物
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string SendFreeGift(string pkey, long thisQQ, long groupQQ, long otherQQ, int gift);
		//public string SendFreeGiftEvent(long thisQQ, long groupQQ, long otherQQ, FreeGiftEnum gift)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["发送免费礼物"];
		//	SendFreeGift SendFreeGiftAPI = (SendFreeGift)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(SendFreeGift));
		//	var res = SendFreeGiftAPI(plugin_key, thisQQ, groupQQ, otherQQ, Convert.ToInt32(gift));
		//	SendFreeGiftAPI = null;
		//	return res;
		//}
		//#endregion



		//#region 以下为红包事件
		//#region 好友接龙红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string FriendFollowRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long otherQQ, string follow_content, string payment_password, int card_serial);
		//public string FriendFollowRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long otherQQ, string follow_content, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["好友接龙红包"];
		//	FriendFollowRedEnvelope FriendFollowRedEnvelopeAPI = (FriendFollowRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(FriendFollowRedEnvelope));
		//	var res = FriendFollowRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, otherQQ, follow_content, payment_password, card_serial);
		//	FriendFollowRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 好友画图红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string FriendDrawRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long otherQQ, string question, string payment_password, int card_serial);
		//public string FriendDrawRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long otherQQ, string question, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["好友画图红包"];
		//	FriendDrawRedEnvelope FriendDrawRedEnvelopeAPI = (FriendDrawRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(FriendDrawRedEnvelope));
		//	var res = FriendDrawRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, otherQQ, question, payment_password, card_serial);
		//	FriendDrawRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 好友口令红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string FriendPasswordRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long otherQQ, string kouling, string payment_password, int card_serial);
		//public string FriendPasswordRedEnvelopeeEvent(long thisQQ, int total_number, int total_amount, long otherQQ, string kouling, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["好友口令红包"];
		//	FriendPasswordRedEnvelope FriendPasswordRedEnvelopeAPI = (FriendPasswordRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(FriendPasswordRedEnvelope));
		//	var res = FriendPasswordRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, otherQQ, kouling, payment_password, card_serial);
		//	FriendPasswordRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 好友普通红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string FriendNormalRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string question, int skinID, [MarshalAs(UnmanagedType.LPStr)] string payment_password, int card_serial);
		//public string FriendNormalRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long otherQQ, string blessing, int skinID, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["好友普通红包"];
		//	FriendNormalRedEnvelope GroupNormalRedEnvelopeAPI = (FriendNormalRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(FriendNormalRedEnvelope));
		//	var res = GroupNormalRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, otherQQ, blessing, skinID, payment_password, card_serial);
		//	GroupNormalRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 好友语音红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string FriendAudioRedEnvelope(long thisQQ, int total_number, int total_amount, long otherQQ, string blessing, int skinID, string payment_password, int card_serial);
		//public string FriendAudioRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long otherQQ, string blessing, int skinID, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["好友语音红包"];
		//	FriendNormalRedEnvelope FriendAudioRedEnvelopeAPI = (FriendNormalRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(FriendNormalRedEnvelope));
		//	var res = FriendAudioRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, otherQQ, blessing, skinID, payment_password, card_serial);
		//	FriendAudioRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 群聊红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string GroupDrawRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, string question, string payment_password, int card_serial);
		//public string GroupDrawRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long groupQQ, string question, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["群聊红包"];
		//	GroupDrawRedEnvelope GroupDrawRedEnvelopeAPI = (GroupDrawRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GroupDrawRedEnvelope));
		//	var res = GroupDrawRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, groupQQ, question, payment_password, card_serial);
		//	GroupDrawRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 群聊口令红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string GroupPasswordRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, string password, string payment_password, int card_serial);
		//public string GroupPasswordRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long groupQQ, string password, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["群聊口令红包"];
		//	GroupPasswordRedEnvelope GroupPasswordRedEnvelopeAPI = (GroupPasswordRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GroupPasswordRedEnvelope));
		//	var res = GroupPasswordRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, groupQQ, password, payment_password, card_serial);
		//	GroupPasswordRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 群聊接龙红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string GroupFollowRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, string follow_content, string payment_password, int card_serial);
		//public string GroupFollowRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long groupQQ, string follow_content, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["群聊接龙红包"];
		//	GroupFollowRedEnvelope GroupFollowRedEnvelopeAPI = (GroupFollowRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GroupFollowRedEnvelope));
		//	var res = GroupFollowRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, groupQQ, follow_content, payment_password, card_serial);
		//	GroupFollowRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 群聊拼手气红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string GroupRandomRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string question, int skinID, [MarshalAs(UnmanagedType.LPStr)] string payment_password, int card_serial);
		//public string GroupRandomRedEnvelopeEvet(long thisQQ, int total_number, int total_amount, long groupQQ, string blessing, int skinID, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["群聊拼手气红包"];
		//	GroupRandomRedEnvelope GroupRandomRedEnvelopeAPI = (GroupRandomRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GroupRandomRedEnvelope));
		//	var res = GroupRandomRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, groupQQ, blessing, skinID, payment_password, card_serial);
		//	GroupRandomRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 群聊普通红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string GroupNormalRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string question, int skinID, [MarshalAs(UnmanagedType.LPStr)] string payment_password, int card_serial);
		//public string GroupNormalRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long groupQQ, string blessing, int skinID, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["群聊普通红包"];
		//	GroupNormalRedEnvelope GroupNormalRedEnvelopeAPI = (GroupNormalRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GroupNormalRedEnvelope));
		//	var res = GroupNormalRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, groupQQ, blessing, skinID, payment_password, card_serial);
		//	GroupNormalRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 群聊语音红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string GroupAudioRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, string audio_password, string payment_password, int card_serial);
		//public string GroupAudioRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long groupQQ, string audio_password, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["群聊语音红包"];
		//	GroupAudioRedEnvelope GroupAudioRedEnvelopeAPI = (GroupAudioRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GroupAudioRedEnvelope));
		//	var res = GroupAudioRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, groupQQ, audio_password, payment_password, card_serial);
		//	GroupAudioRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#region 群聊专属红包
		//[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		//private delegate string GroupExclusiveRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long otherQQ, string blessing, string payment_password, int card_serial);
		//public string GroupExclusiveRedEnvelopeEvent(long thisQQ, int total_number, int total_amount, long otherQQ, string blessing, string payment_password, int card_serial)
		//{
		//	dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
		//	int ptr = json["群聊专属红包"];
		//	GroupExclusiveRedEnvelope GroupExclusiveRedEnvelopeAPI = (GroupExclusiveRedEnvelope)Marshal.GetDelegateForFunctionPointer(new IntPtr(ptr), typeof(GroupExclusiveRedEnvelope));
		//	var res = GroupExclusiveRedEnvelopeAPI(plugin_key, thisQQ, total_number, total_amount, otherQQ, blessing, payment_password, card_serial);
		//	GroupExclusiveRedEnvelopeAPI = null;
		//	return res;
		//}
		//#endregion
		//#endregion

	}
}
