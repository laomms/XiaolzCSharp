

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
		public static long MyQQ = 0;

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
		#region 函数委托指针
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr OutputLog(string pkey, [MarshalAs(UnmanagedType.LPStr)] string message, int text_color, int background_color);

		public static SendPrivateMsgDelegate SendPrivateMsg = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SendPrivateMsgDelegate(string pkey, long ThisQQ, long SenderQQ, [MarshalAs(UnmanagedType.LPStr)] string MessageContent, ref long MessageRandom, ref uint MessageReq);

		public static SendGroupMsgDelegate SendGroupMsg = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SendGroupMsgDelegate(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string msgcontent, bool anonymous);

		public static PrivateUndoDelegate Undo_PrivateEvent = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool PrivateUndoDelegate(string pkey, long thisQQ, long otherQQ, long message_random, int message_req, int time);

		public static UndoGroupDelegate Undo_GroupEvent = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool UndoGroupDelegate(string pkey, long thisQQ, long groupQQ, long message_random, int message_req);

		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr RecviceimageDelegate(string pkey, string guid, long thisQQ, long groupQQ);

		public static GetFriendListDelegate GetFriendList = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate int GetFriendListDelegate(string pkey, long thisQQ, ref DataArray[] DataInfo);

		public static GetGroupListDelegate GetGroupList = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate int GetGroupListDelegate(string pkey, long thisQQ, ref DataArray[] DataInfo);

		public static GetGroupMemberlistDelegate GetGroupMemberlist = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate int GetGroupMemberlistDelegate(string pkey, long thisQQ, long groupQQ, ref DataArray[] DataInfo);

		public static GetAdministratorListDelegate GetAdministratorList = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetAdministratorListDelegate(string pkey, long thisQQ, long gruopQQ);

		public static RestartDelegate restart = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate void RestartDelegate(string pkey);

		public static GetLoginQQDelegate GetLoginQQ = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetLoginQQDelegate(string pkey);

		public static FriendverificationEventDelegate FriendverificationEvent = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate void FriendverificationEventDelegate(string pkey, long thisQQ, long triggerQQ, long message_seq, FriendVerificationOperateEnum operate_type);

		public static GroupVerificationEventDelegate GroupVerificationEvent = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate void GroupVerificationEventDelegate(string pkey, long thisQQ, long source_groupQQ, long triggerQQ, long message_seq, GroupVerificationOperateEnum operate_type, EventTypeEnum event_type, [MarshalAs(UnmanagedType.LPStr)] string refuse_reason);

		public static GetImageDownloadLinkDelegate GetImageDownloadLink = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetImageDownloadLinkDelegate(string pkey, string guid, long thisQQ, long groupQQ);

		public static GetFriendInfoDelegate GetFriendInfo = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool GetFriendInfoDelegate(string pkey, long thisQQ, long otherQQ, ref GetFriendDataInfo[] friendInfos);

		public static GroupVerificationDelegate GroupVerification = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate void GroupVerificationDelegate(string pkey, long thisQQ, long source_groupQQ, long triggerQQ, long message_seq, GroupVerificationOperateEnum operate_type, EventTypeEnum event_type, [MarshalAs(UnmanagedType.LPStr)] string refuse_reason);

		public static GetGroupInfoDelegate GetGroupInfo = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool GetGroupInfoDelegate(string pkey, long thisQQ, long otherGroupQQ, ref GetGroupDataInfo[] GroupInfos);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool GetGroupCardInfoDelegate(string pkey, long thisQQ, long otherGroupQQ, ref GroupCardInfoDatList[] groupCardInfo);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool Undo_GroupDelegate(string pkey, long thisQQ, long groupQQ, long message_random, int message_req);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool Undo_PrivateDelegate(string pkey, long thisQQ, long otherQQ, long message_random, int message_req, int time);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr CreateGroupFolderDelegate(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string folder);

		public static SendFriendJSONMessageDelegate SendFriendJSONMessage = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SendFriendJSONMessageDelegate(string pkey, long thisQQ, long friendQQ, [MarshalAs(UnmanagedType.LPStr)] string json_content);

		public static SendGroupJSONMessageDelegate SendGroupJSONMessage = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SendGroupJSONMessageDelegate(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string json_content, bool anonymous);


		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SendFreeGiftDelegate(string pkey, long thisQQ, long groupQQ, long otherQQ, int gift);


		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SendGroupTemporaryMessage(string pkey, long thisQQ, long groupQQ, long otherQQ, [MarshalAs(UnmanagedType.LPStr)] string content, ref long random, ref int req);

		public static ReadForwardedChatHistoryDelegate ReadForwardedChatHistory = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate void ReadForwardedChatHistoryDelegate(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string resID, [MarshalAs(UnmanagedType.LPStr)] ref string retPtr);

		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ShareMusic(string pkey, long thisQQ, long otherQQ, [MarshalAs(UnmanagedType.LPStr)] string music_name, [MarshalAs(UnmanagedType.LPStr)] string artist_name, [MarshalAs(UnmanagedType.LPStr)] string redirect_link, [MarshalAs(UnmanagedType.LPStr)] string cover_link, [MarshalAs(UnmanagedType.LPStr)] string file_path, int app_type, int share_type);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ModifyGroupMessageContent(string pkey, [MarshalAs(UnmanagedType.SysInt)] int data_pointer, [MarshalAs(UnmanagedType.LPStr)] string new_message_content);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ModifyPrivateMessageContent(string pkey, [MarshalAs(UnmanagedType.SysInt)] int data_pointer, [MarshalAs(UnmanagedType.LPStr)] string new_message_content);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GroupDrawRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string question, [MarshalAs(UnmanagedType.LPStr)] string payment_password, int card_serial, ref GetCaptchaInfoDataList[] captchaInfo);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr FriendNormalRedEnvelope(string pkey, long thisQQ, int total_number, int total_amount, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string question, int skinID, [MarshalAs(UnmanagedType.LPStr)] string payment_password, int card_serial, ref GetCaptchaInfoDataList[] ciDataLists);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool FriendFileToFriend(string pkey, long thisQQ, long sourceQQ, long targetQQ, [MarshalAs(UnmanagedType.LPStr)] string fileID, [MarshalAs(UnmanagedType.LPStr)] string file_name, long file_size, ref int msgReq, ref long Random, ref int time);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetPluginDataDirectory(string pkey);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetClientKey(string pkey, long thisQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetPSKey(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string domain);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetOrderDetail(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string orderID, ref OrderDetaildDataList[] data);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool DissolveGroup(string pkey, long thisQQ, long gruopNumber);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ShutUpGroupMember(string pkey, long thisQQ, long groupQQ, long otherQQ, int time);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetNameForce(string pkey, long thisQQ, long otherQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetQQWalletPersonalInformation(string pkey, long thisQQ, ref QQWalletInfoDataList[] qQWalletInfoDataLists);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetNameFromCache(string pkey, long otherQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetGroupNickname(string pkey, long thisQQ, long groupQQ, long otherQQ);

		public static GetGroupFileListDelegate GetGroupFileList = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		public delegate string GetGroupFileListDelegate(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string folder, ref GroupFileInfoDataList[] groupFileInfoDataLists);


		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ShutUpAll(string pkey, long thisQQ, long groupQQ, bool is_shut_up_all);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool GroupPermission_SetInviteMethod(string pkey, long thisQQ, long groupQQ, int method);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ForwardGroupFileToFriend(string pkey, long thisQQ, long source_groupQQ, long target_groupQQ, [MarshalAs(UnmanagedType.LPStr)] string fileID, [MarshalAs(UnmanagedType.LPStr)] string filename, long filesize, ref int msgReq, ref long Random, ref int time);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ForwardGroupFileToGroup(string pkey, long thisQQ, long source_groupQQ, long target_groupQQ, [MarshalAs(UnmanagedType.LPStr)] string fileID);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool RemoveGroupMember(string pkey, long thisQQ, long groupQQ, long otherQQ, bool is_verification_refused);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr DeleteGroupFile(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string file_id, [MarshalAs(UnmanagedType.LPStr)] string folder);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr DeleteGroupFolder(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string folder);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr RenameGroupFolder(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string old_folder, [MarshalAs(UnmanagedType.LPStr)] string new_folder);

		public static UploadGroupFileDelegate UploadGroupFile = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr UploadGroupFileDelegate(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string path, [MarshalAs(UnmanagedType.LPStr)] string folder);


		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr MoveGroupFile(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string file_id, [MarshalAs(UnmanagedType.LPStr)] string old_folder, [MarshalAs(UnmanagedType.LPStr)] string new_folder);

		public static UploadFriendImageDelegate UploadFriendImage = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr UploadFriendImageDelegate(string pkey, long thisQQ, long friendQQ, bool is_flash, [MarshalAs(UnmanagedType.LPArray)] byte[] pic, int picsize);

		public static UploadGroupImageDelegate UploadGroupImage = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr UploadGroupImageDelegate(string pkey, long thisQQ, long friendQQ, bool is_flash, [MarshalAs(UnmanagedType.LPArray)] byte[] pic, int picsize);

		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool UploadGroupAvatar(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPArray)] byte[] pic, int picsize);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr UploadAvatar(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPArray)] byte[] pic, int picsize);

		public static UploadFriendAudioDelegate UploadFriendAudio = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr UploadFriendAudioDelegate(string pkey, long thisQQ, long friendQQ, int audio_type, [MarshalAs(UnmanagedType.LPStr)] string audio_text, [MarshalAs(UnmanagedType.LPArray)] byte[] audio, int audiosize);

		public static UploadGroupAudioDelegate UploadGroupAudio = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr UploadGroupAudioDelegate(string pkey, long thisQQ, long friendQQ, int audio_type, [MarshalAs(UnmanagedType.LPStr)] string audio_text, [MarshalAs(UnmanagedType.LPArray)] byte[] audio, int audiosize);

		public static SaveFileToWeiYunDelegate SaveFileToWeiYun = null;
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SaveFileToWeiYunDelegate(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string file_id);

		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool ReportCurrent(string pkey, long thisQQ, long groupQQ, double Longitude, double Latitude);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SetGroupNickname(string pkey, long thisQQ, long groupQQ, long otherQQ, [MarshalAs(UnmanagedType.LPStr)] string nickname);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetLocationShare(string pkey, long thisQQ, long groupQQ, double Longitude, double Latitude, bool is_enabled);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetStatus(string pkey, long thisQQ, int main, int sun, int battery);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool Setexclusivetitle(string pkey, long thisQQ, long groupQQ, long otherQQ, [MarshalAs(UnmanagedType.LPStr)] string name);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate long IsShuttedUp(string pkey, long thisQQ, long groupQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr AddFriend(string pkey, long thisQQ, long otherQQ, [MarshalAs(UnmanagedType.LPStr)] string verification, [MarshalAs(UnmanagedType.LPStr)] string comment);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr AddGroup(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string verification);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool QuitGroup(string pkey, long thisQQ, long groupQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetSignature(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string signature, [MarshalAs(UnmanagedType.LPStr)] string location);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetName(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string name);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SetBlockFriend(string pkey, long thisQQ, long otherQQ, bool is_blocked);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetGroupMessageReceive(string pkey, long thisQQ, long groupQQ, int set_type);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SetSpecialFriend(string pkey, long thisQQ, long otherQQ, bool is_special);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr SubmitPaymentCaptcha(string pkey, long thisQQ, IntPtr captcha_information, [MarshalAs(UnmanagedType.LPStr)] string captcha, [MarshalAs(UnmanagedType.LPStr)] string payment_password);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool LoginSpecifyQQ(string pkey, long otherQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SendIMEStatus(string pkey, long thisQQ, long ohterQQ, int iMEStatus);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool CheckPermission(string pkey, int permission);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr QQLike(string pkey, long thisQQ, long otherQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool Modifyinformation(string pkey, long thisQQ, [MarshalAs(UnmanagedType.LPStr)] string json);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GetRedEnvelope(string pkey, long thisQQ, long GroupQQ, ref RedEnvelopesDataList[] reDataList);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate void CallPhone(string pkey, long thisQQ, long otherQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate IntPtr GroupFileDownloadLink(string pkey, long thisQQ, long GroupQQ, [MarshalAs(UnmanagedType.LPTStr)] string FileID, [MarshalAs(UnmanagedType.LPTStr)] string FileName);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool DoubleclickGroupFace(string pkey, long thisQQ, long otherQQ, long groupQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool GroupTop(string pkey, long thisQQ, long GroupQQ, bool istop);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetEssence(string pkey, long thisQQ, long groupQQ, int message_req, long message_random);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetGroupNickRules(string pkey, long thisQQ, long GroupQQ, [MarshalAs(UnmanagedType.LPWStr)] string rules);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool SetGroupLimitNumber(string pkey, long thisQQ, long GroupQQ, int LimitNumber);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool FriendjoinGroup(string pkey, long thisQQ, long GroupQQ, long otherQQ, long otherGroupQQ);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool GroupNoticeMethod(string pkey, long thisQQ, long GroupQQ, long otherQQ, int metohd);
		public delegate IntPtr GetGroupMemberBriefInfo(string pkey, long thisQQ, long GroupQQ, ref GMBriefDataList[] gMBriefDataLists);
		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public delegate bool UpdataGroupName(string pkey, long thisQQ, long GroupQQ, [MarshalAs(UnmanagedType.LPStr)] string NewGroupName);
		#endregion
		#region 初始化传入的函数指针
		public static void InitFunction()
		{
			dynamic json = new JavaScriptSerializer().DeserializeObject(jsonstr);
			RestartDelegate ReStartAPI = (RestartDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["框架重启"]), typeof(RestartDelegate));
			restart = ReStartAPI;
			GC.KeepAlive(restart);
			GetLoginQQDelegate GetLoginQQAPI = (GetLoginQQDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["取框架QQ"]), typeof(GetLoginQQDelegate));
			GetLoginQQ = GetLoginQQAPI;
			GC.KeepAlive(GetLoginQQ);
			SendPrivateMsgDelegate SendPrivateMsgAPI = (SendPrivateMsgDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["发送好友消息"]), typeof(SendPrivateMsgDelegate));
			SendPrivateMsg = SendPrivateMsgAPI;
			GC.KeepAlive(SendPrivateMsg);
			SendGroupMsgDelegate SendGroupMsgAPI = (SendGroupMsgDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["发送群消息"]), typeof(SendGroupMsgDelegate));
			SendGroupMsg = SendGroupMsgAPI;
			GC.KeepAlive(SendGroupMsg);
			FriendverificationEventDelegate FriendverificationEventAPI = (FriendverificationEventDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["处理好友验证事件"]), typeof(FriendverificationEventDelegate));
			FriendverificationEvent = FriendverificationEventAPI;
			GC.KeepAlive(FriendverificationEvent);
			GroupVerificationEventDelegate GroupVerificationEventAPI = (GroupVerificationEventDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["处理群验证事件"]), typeof(GroupVerificationEventDelegate));
			GroupVerificationEvent = GroupVerificationEventAPI;
			GC.KeepAlive(GroupVerificationEvent);
			UploadFriendImageDelegate UploadFriendImageAPI = (UploadFriendImageDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["上传好友图片"]), typeof(UploadFriendImageDelegate));
			UploadFriendImage = UploadFriendImageAPI;
			GC.KeepAlive(UploadFriendImage);
			UploadGroupImageDelegate UploadGroupImageAPI = (UploadGroupImageDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["上传群图片"]), typeof(UploadGroupImageDelegate));
			UploadGroupImage = UploadGroupImageAPI;
			GC.KeepAlive(UploadGroupImage);
			UploadFriendAudioDelegate UploadFriendAudioAPI = (UploadFriendAudioDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["上传好友语音"]), typeof(UploadFriendAudioDelegate));
			UploadFriendAudio = UploadFriendAudioAPI;
			GC.KeepAlive(UploadFriendAudio);
			UploadGroupAudioDelegate UploadGroupAudioAPI = (UploadGroupAudioDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["上传群语音"]), typeof(UploadGroupAudioDelegate));
			UploadGroupAudio = UploadGroupAudioAPI;
			GC.KeepAlive(UploadGroupAudio);
			GetImageDownloadLinkDelegate GetImageDownloadLinkAPI = (GetImageDownloadLinkDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["取图片下载地址"]), typeof(GetImageDownloadLinkDelegate));
			GetImageDownloadLink = GetImageDownloadLinkAPI;
			GC.KeepAlive(GetImageDownloadLink);
			GetFriendListDelegate GetFriendListAPI = (GetFriendListDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["取好友列表"]), typeof(GetFriendListDelegate));
			GetFriendList = GetFriendListAPI;
			GC.KeepAlive(GetFriendList);
			GetFriendInfoDelegate GetFriendInfoAPI = (GetFriendInfoDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["查询好友信息"]), typeof(GetFriendInfoDelegate));
			GetFriendInfo = GetFriendInfoAPI;
			GC.KeepAlive(GetFriendInfo);
			GetGroupMemberlistDelegate GetGroupMemberlistAPI = (GetGroupMemberlistDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["取群成员列表"]), typeof(GetGroupMemberlistDelegate));
			GetGroupMemberlist = GetGroupMemberlistAPI;
			GC.KeepAlive(GetGroupMemberlist);
			GetGroupListDelegate GetGroupListAPI = (GetGroupListDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["取群列表"]), typeof(GetGroupListDelegate));
			GetGroupList = GetGroupListAPI;
			GC.KeepAlive(GetGroupList);
			GetGroupInfoDelegate GetGroupInfoAPI = (GetGroupInfoDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["查询群信息"]), typeof(GetGroupInfoDelegate));
			GetGroupInfo = GetGroupInfoAPI;
			GC.KeepAlive(GetGroupInfo);
			PrivateUndoDelegate UndoPrivateAPI = (PrivateUndoDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["撤回消息_私聊本身"]), typeof(PrivateUndoDelegate));
			Undo_PrivateEvent = UndoPrivateAPI;
			GC.KeepAlive(Undo_PrivateEvent);
			UndoGroupDelegate UndoGroupApi = (UndoGroupDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["撤回消息_群聊"]), typeof(UndoGroupDelegate));
			Undo_GroupEvent = UndoGroupApi;
			GC.KeepAlive(Undo_GroupEvent);
			GetAdministratorListDelegate GetAdministratorListAPI = (GetAdministratorListDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["取管理层列表"]), typeof(GetAdministratorListDelegate));
			GetAdministratorList = GetAdministratorListAPI;
			GC.KeepAlive(GetAdministratorList);
			SendFriendJSONMessageDelegate SendFriendJSONMessageAPI = (SendFriendJSONMessageDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["发送好友json消息"]), typeof(SendFriendJSONMessageDelegate));
			SendFriendJSONMessage = SendFriendJSONMessageAPI;
			GC.KeepAlive(SendFriendJSONMessage);
			SendGroupJSONMessageDelegate SendGroupJSONMessageAPI = (SendGroupJSONMessageDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["发送群json消息"]), typeof(SendGroupJSONMessageDelegate));
			SendGroupJSONMessage = SendGroupJSONMessageAPI;
			GC.KeepAlive(SendGroupJSONMessage);
			SaveFileToWeiYunDelegate SaveFileToWeiYunAPI = (SaveFileToWeiYunDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["保存文件到微云"]), typeof(SaveFileToWeiYunDelegate));
			SaveFileToWeiYun = SaveFileToWeiYunAPI;
			GC.KeepAlive(SaveFileToWeiYun);
			ReadForwardedChatHistoryDelegate ReadForwardedChatHistoryAPI = (ReadForwardedChatHistoryDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["查看转发聊天记录内容"]), typeof(ReadForwardedChatHistoryDelegate));
			ReadForwardedChatHistory = ReadForwardedChatHistoryAPI;
			GC.KeepAlive(ReadForwardedChatHistory);
			UploadGroupFileDelegate UploadGroupFileAPI = (UploadGroupFileDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["上传群文件"]), typeof(UploadGroupFileDelegate));
			UploadGroupFile = UploadGroupFileAPI;
			GC.KeepAlive(UploadGroupFile);
			GetGroupFileListDelegate GetGroupFileListAPI = (GetGroupFileListDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(json["取群文件列表"]), typeof(GetGroupFileListDelegate));
			GetGroupFileList = GetGroupFileListAPI;
			GC.KeepAlive(GetGroupFileList);
		}
		#endregion
		#region 插件启动	
		public static DelegateAppEnable appEnableFunc = new DelegateAppEnable(appEnable);
		public delegate int DelegateAppEnable();
		public static int appEnable()		
		{
			InitFunction();
			string res = CallGetLoginQQ();
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
		#region 插件卸载		
		public static DelegateAppUnInstall AppUninstallEvent = new DelegateAppUnInstall(AppUnInstall);
		public delegate int DelegateAppUnInstall();
		public static int AppUnInstall()
		{
			//托管程序集插件不支持FreeLibrary的方式卸载插件,只支持AppDomain的方式卸载,所以要删除插件,必须先关掉框架,手动删除.
			return 0;
		}

		#endregion
		#region 插件禁用
		public static DelegateAppDisabled AppDisabledEvent = new DelegateAppDisabled(appDisable);
		public delegate int DelegateAppDisabled();
		public static int appDisable()
		{
			return 0;
		}
		#endregion
		#region 取框架QQ
		public static string CallGetLoginQQ()
		{
			string RetJson =Marshal.PtrToStringAnsi( GetLoginQQ(plugin_key));
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
			return "";
		}
		#endregion
		#region 插件设置
		public static DelegateAppSetting AppSettingEvent = new DelegateAppSetting(AppSetting);
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
						MyQQ = EvenType.ThisQQ;						
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
						//API.SendPrivateMsg(EvenType.ThisQQ, "12345678", EvenType.TriggerQQName+ "(" + EvenType.TriggerQQ.ToString() +" ) 将机器人加入黑名单");
						API.SendGroupMsg(plugin_key, EvenType.ThisQQ, 64596829, "[@37476230]" + EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + " ) 将机器人加入黑名单",false);
						break;
					case EventTypeEnum.Group_MemberVerifying:
						API.SendGroupMsg(plugin_key, EvenType.ThisQQ, 64596829, "[@37476230]" + EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + " ) 想加入群: "+ EvenType.SourceGroupName + "(" + EvenType.SourceGroupQQ.ToString() + " )",false);
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
						API.SendGroupMsg(plugin_key, EvenType.ThisQQ, EvenType.SourceGroupQQ, "[@" + EvenType.TriggerQQ.ToString() + "]" +  EvenType.TriggerQQName + ",欢迎你加入本群!",false);
						break;
					case EventTypeEnum.Group_MemberVerifying:
						Console.WriteLine("某人申请加群");
						break;
					case EventTypeEnum.Group_MemberQuit:
						Console.WriteLine("某人退出了群");
						API.SendGroupMsg(plugin_key, EvenType.ThisQQ, EvenType.SourceGroupQQ, EvenType.TriggerQQName + "已退出本群!",false);
						break;
					case EventTypeEnum.Group_MemberUndid:
						API.SendGroupMsg(plugin_key, EvenType.ThisQQ, EvenType.SourceGroupQQ, EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + " ) 撤回了一条消息,内容如下:" + EvenType.MessageContent,false);
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
						//API.SendPrivateMsg(EvenType.ThisQQ, "12345678",  EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + ")对方加机器人为好友,发送了这样的消息:" + EvenType.MessageContent);
						API.SendGroupMsg(plugin_key, EvenType.ThisQQ, 64596829, "[@37476230]" + EvenType.TriggerQQName + "(" + EvenType.TriggerQQ.ToString() + ")欲加机器人为好友,发送了这样的消息:" + EvenType.MessageContent +",是否同意?",false);
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
		#region 发送好友图片
		public string SendFriendImage(long thisQQ, long friendQQ, string picpath, bool is_flash)
		{
			Bitmap bitmap = new Bitmap(picpath);
			byte[] picture = GetByteArrayByImage(bitmap);
			IntPtr piccode = UploadFriendImage(plugin_key, thisQQ, friendQQ, is_flash, picture, picture.Length);
			long MessageRandom = 0;
			uint MessageReq = 0;
			IntPtr res= SendPrivateMsg(plugin_key, thisQQ, friendQQ, Marshal.PtrToStringAnsi(piccode), ref MessageRandom, ref MessageReq);
			return Marshal.PtrToStringAnsi(res);
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
			Bitmap bitmap = new Bitmap(picpath);
			byte[] picture = GetByteArrayByImage(bitmap);
			IntPtr piccode = UploadGroupImage(plugin_key, thisQQ, groupQQ, is_flash, picture, picture.Length);
			IntPtr res=SendGroupMsg(plugin_key, thisQQ, groupQQ, Marshal.PtrToStringAnsi(piccode), false);
			return Marshal.PtrToStringAnsi(res);
		}
		#endregion
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
		#region 获取图片地址		
		public static string GetImageLink(long thisQQ, long sendQQ, long groupQQ, string ImgGuid)
		{
			var ImgUrl = GetImageDownloadLink(plugin_key, ImgGuid, thisQQ, groupQQ);
			if (groupQQ == 0)
			{
				long MessageRandom = 0;
				uint MessageReq = 0;
				SendPrivateMsg(plugin_key, thisQQ, sendQQ, "图片地址为:" + ImgUrl + "\r\n",ref MessageRandom,ref MessageReq);
			}
			else
			{
				SendGroupMsg(plugin_key, thisQQ, groupQQ, "图片地址为:" + ImgUrl + "\r\n",false);
			}
			return "";
		}
		#endregion
		#region 取好友列表		
		public static int GetFriendLists(long thisQQ, long sendQQ)
		{		
			DataArray[] ptrArray = new DataArray[2];
			int count = GetFriendList(plugin_key, thisQQ, ref ptrArray);
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
				long MessageRandom = 0;
				uint MessageReq = 0;
				SendPrivateMsg(plugin_key, thisQQ, sendQQ, "好友列表:" + "\r\n" + string.Join("\r\n", list), ref MessageRandom, ref MessageReq);
			}
			return count;
		}
		#endregion
		#region 查询好友信息
		public static void GetFriendData(long thisQQ, long otherQQ)
		{
			long MessageRandom = 0;
			uint MessageReq = 0;
			GetFriendDataInfo[] pFriendInfo = new GetFriendDataInfo[2];
			bool res = GetFriendInfo(plugin_key, thisQQ, otherQQ, ref pFriendInfo);
			if (res == true)
			{
				var result = (new JavaScriptSerializer()).Serialize(pFriendInfo[0].friendInfo);
				SendPrivateMsg(plugin_key, thisQQ, otherQQ, result, ref MessageRandom, ref MessageReq);
			}
			else
			{
				SendPrivateMsg(plugin_key, thisQQ, otherQQ, "查询好友信息失败", ref MessageRandom, ref MessageReq);
			}
		}
		#endregion
		#region 取群成员列表
		public static int GetGroupMemberlists(long thisQQ, long groupQQ)
		{			
			DataArray[] ptrArray = new DataArray[2];			
			int count = GetGroupMemberlist(plugin_key, thisQQ, groupQQ, ref ptrArray);
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
				SendGroupMsg(plugin_key, thisQQ, groupQQ, "群列表:" + "\r\n" + string.Join("\r\n", list),false);
			}
			return count;
		}
		#endregion
		#region 取群列表
		public static int GetGroupLists(long thisQQ, long groupQQ)
		{		
			DataArray[] ptrArray = new DataArray[2];
			int count = GetGroupList(plugin_key, thisQQ, ref ptrArray);
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
				SendGroupMsg(plugin_key, thisQQ, groupQQ, "群列表:" + "\r\n" + string.Join("\r\n", list),false);
			}
			return count;
		}
		#endregion
		#region 取管理列表
		public string[] GetAdministratorLists(long thisQQ, long gruopNumber)
		{		
			string ret =Marshal.PtrToStringAnsi(GetAdministratorList(plugin_key, thisQQ, gruopNumber));
			string[] adminlist = ret.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			return adminlist;
		}
		#endregion
		#region 查询群信息
		public static void GetGroupData(long thisQQ, long otherGroupQQ)
		{
			long MessageRandom = 0;
			uint MessageReq = 0;
			GetGroupDataInfo[] pGroupInfo = new GetGroupDataInfo[2];
			bool res = GetGroupInfo(plugin_key, thisQQ, otherGroupQQ, ref pGroupInfo);
			if (res == true)
			{
				var result = (new JavaScriptSerializer()).Serialize(pGroupInfo[0].GroupInfo);
				SendPrivateMsg(plugin_key, thisQQ, otherGroupQQ, result, ref MessageRandom, ref MessageReq);
			}
			else
			{
				SendPrivateMsg(plugin_key, thisQQ, otherGroupQQ, "查询好友信息失败", ref MessageRandom, ref MessageReq);
			}
		}
		#endregion			
		#region 取群文件列表	
		public delegate string GetGroupFileLists(string pkey, long thisQQ, long groupQQ, [MarshalAs(UnmanagedType.LPStr)] string folder, ref GroupFileInfoDataList[] groupFileInfoDataLists);
		public List<GroupFileInformation> GetGroupFileListEvent(long thisQQ, long groupQQ, string folder)
		{
		
			GroupFileInfoDataList[] pdatalist = new GroupFileInfoDataList[2];
			string ret = GetGroupFileList(plugin_key, thisQQ, groupQQ, folder, ref pdatalist);
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
				return list;
			}
			return null;
		}
		#endregion

	}
}
