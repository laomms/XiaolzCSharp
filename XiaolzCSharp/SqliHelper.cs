using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XiaolzCSharp
{
    class SqliHelper
    {
		public static string DataPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\Test.db"; // Environment.CurrentDirectory & "\Test.db" ' System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) & "Test.db"
		[DllImport("kernel32")]
		private extern static IntPtr HeapAlloc(IntPtr heap, UInt32 flags, UInt32 bytes);

		[DllImport("kernel32")]
		private extern static IntPtr GetProcessHeap();

		[DllImport("kernel32")]
		private extern static int lstrlen(IntPtr str);
		public static IntPtr StringToPointer(string str)
		{
			if (str == null)
			{
				return IntPtr.Zero;
			}
			else
			{
				Encoding encoding = System.Text.Encoding.UTF8;
				byte[] bytes = encoding.GetBytes(str);
				int length = bytes.Length + 1;
				IntPtr pointer = HeapAlloc(GetProcessHeap(), 0, (uint)length);
				Marshal.Copy(bytes, 0, pointer, bytes.Length);
				Marshal.WriteByte(pointer, bytes.Length, 0);
				return pointer;
			}
		}
		public static string PointerToString(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
			{
				return null;
			}

			Encoding encoding = System.Text.Encoding.UTF8;

			int length = GetPointerLenght(ptr);
			byte[] bytes = new byte[length];
			Marshal.Copy(ptr, bytes, 0, length);
			return encoding.GetString(bytes, 0, length);
		}
		public static int GetPointerLenght(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
			{
				return 0;
			}
			return lstrlen(ptr);
		}
		public static string ConvertString2UTF8(string strSrc)
		{
			string strTemp = strSrc;
			byte[] utf8bytes = System.Text.Encoding.Default.GetBytes(strTemp);
			byte[] utf8bytes2 = System.Text.Encoding.Convert(System.Text.Encoding.Default, System.Text.Encoding.UTF8, utf8bytes);
			strTemp = System.Text.Encoding.Default.GetString(utf8bytes2);
			return strTemp;
		}
		public static bool CreateTable(string[] KeyNames, List<string[]> Values)
		{
			IntPtr hSqlite = new IntPtr();
			IntPtr transient = new IntPtr();
			sqlite3_open(ConvertString2UTF8(DataPath), ref hSqlite);
			for (int i = 0; i < KeyNames.Count(); i++)
			{
				string ss = string.Join(",", Values[i]);
				string sql = "CREATE TABLE IF NOT EXISTS `" + KeyNames[i] + "` (ID INTEGER PRIMARY KEY AUTOINCREMENT, " + string.Join(",", Values[i]) + ")";
				if (sqlite3_exec(hSqlite, StringToPointer(sql), null, IntPtr.Zero, ref transient) == SQLITE_OK)
				{
					Console.WriteLine("Create Successfully");
				}
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static List<string> ReadAllData(string tableName, string columnName, string columnValue, string columnSearch)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select " + columnSearch + " from " + tableName + " where " + columnName + " like '%" + columnValue + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						ItemList.Add(PointerToString(sqlite3_column_text(stmt, 0)).ToString().Replace("\r", ""));
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}

	   //CheckDataExsit("KeyStore", "Keys", RetKey) = True
		public static void CheckImporlistview(ListView ListView1, string tableName, string TiaoJina8)
		{
			ListView1.Items.Clear();
			ListViewItem ITM = null;
			var sql = "Select * from " + tableName + TiaoJina8;
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					var n = 0;
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						n = n + 1;
						int columnCount = sqlite3_column_count(stmt);
						ITM = ListView1.Items.Add((n + 1).ToString());
						for (var i = 1; i < columnCount; i++)
						{
							Console.WriteLine(PointerToString(sqlite3_column_text(stmt, i)));
							if (i == 2)
							{
								if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
								{
									ITM.SubItems.Add(Convert.ToDateTime(PointerToString(sqlite3_column_text(stmt, i))).ToString("yyyy-MM-dd hh:mm:ss"));
								}
							}
							else
							{
								if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
								{
									ITM.SubItems.Add(PointerToString(sqlite3_column_text(stmt, i)));
								}
							}
						}
					}
				}

				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
		}
		//InsertData("Activation", New String() {"QQGroup", "QQNumber", "MsgType"}, New String() {szGruopId, szQQId, "GroupMessage"})
		public static bool InsertData(string tableName, string[] columnName, string[] columnValue)
		{
			List<string> strList = new List<string>();
			for (int i = 0; i < columnValue.Count(); i++)
			{
				strList.Add("?");
			}
			//sql = "Insert Or Ignore Into " + tableName + "(" + String.Join(",", columnName) + ") VALUES('" + String.Join("','", columnValue) + "')"
			var sql = "Insert Into " + tableName + "(" + string.Join(",", columnName) + ") VALUES(" + string.Join(",", strList) + ")";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			IntPtr stmt = new IntPtr();
			IntPtr transient = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					for (var i = 0; i < columnName.Count(); i++)
					{
						sqlite3_bind_text(stmt, i + 1, StringToPointer(columnValue[i]), -1, transient);
					}
					int setp = sqlite3_step(stmt);
					if (setp == SQLITE_DONE)
					{
						sqlite3_reset(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static bool InsertDataEx(string szDataPath, string tableName, string[] columnName, string[] columnValue)
		{
			List<string> strList = new List<string>();
			for (int i = 0; i < columnValue.Count(); i++)
			{
				strList.Add("?");
			}
			//sql = "Insert Or Ignore Into " + tableName + "(" + String.Join(",", columnName) + ") VALUES('" + String.Join("','", columnValue) + "')"
			var sql = "Insert Into " + tableName + "(" + string.Join(",", columnName) + ") VALUES(" + string.Join(",", strList) + ")";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			IntPtr stmt = new IntPtr();
			IntPtr transient = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(szDataPath), ref hSqlite) == SQLITE_OK)
			{
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					for (var i = 0; i < columnName.Count(); i++)
					{
						sqlite3_bind_text(stmt, i + 1, StringToPointer(columnValue[i]), -1, transient);
					}
					int setp = sqlite3_step(stmt);
					if (setp == SQLITE_DONE)
					{
						sqlite3_reset(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		//InsertSingleData("AuthorizeAll", "QQ", m.Value)
		public static bool InsertSingleData(string tableName, string columnName, string columnValue)
		{
			var sql = "Insert Or Ignore Into " + tableName + "(" + columnName + ") VALUES(?)";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = System.IntPtr.Zero;
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					sqlite3_bind_text(stmt, 1, StringToPointer(columnValue), -1, transient);
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_DONE)
					{
						sqlite3_reset(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		//UpdateSingleData("Activation", "UserID", matchUserID.Value, Keys, KeysValue)
		public static bool UpdateSingleData(string tableName, string itemName, string itemValue, string columnName, string columnValue)
		{
			var sql = "UPDATE " + tableName + " Set " + columnName + "='" + columnValue + "' WHERE " + itemName + " Like '%" + itemValue + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_DONE)
					{
						sqlite3_reset(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		//UpdateData("Activation", "UserID", matchUserID.Value, "QQGroup='" + szGruopId + "'", "QQNumber='" + szQQId + "'", "Location='" + szQQId + "'")
		public static string MatchToString(Match m)
		{
			return m.Value;
		}
		public static bool UpdateData(string tableName, string itemName, string itemValue, params string[] columnAgr)
		{
			var setvalue = Regex.Replace(string.Join(",", columnAgr), "'.*?'", "?");
			MatchCollection matchList = Regex.Matches(string.Join(",", columnAgr), "(?<==').*?(?=')");
			Match[] matchArray = new Match[matchList.Count];
			matchList.CopyTo(matchArray, 0);
			string[] matches = Array.ConvertAll(matchArray, new Converter<Match, string>(MatchToString));
			//Dim sql = "UPDATE " + tableName + " SET " + String.Join(",", columnAgr) + " WHERE " + itemName + " like '%" + itemValue + "%'"
			var sql = "UPDATE " + tableName + " SET " + string.Join(",", setvalue) + " WHERE " + itemName + " Like '%" + itemValue + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					//sqlite3_exec(hSqlite, StringToPointer(sql), IntPtr.Zero, IntPtr.Zero, transient)
					for (var i = 0; i < columnAgr.Length; i++)
					{
						sqlite3_bind_text(stmt, i + 1, StringToPointer(matches[i]), -1, transient);
					}
					if (sqlite3_step(stmt) == SQLITE_DONE)
					{
						sqlite3_reset(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static bool UpdateData2(string tableName, string itemName, string itemValue, params string[] columnAgr)
		{
			var sql = "UPDATE " + tableName + " Set " + itemName + "='" + itemValue + "' WHERE " + string.Join(" AND ", columnAgr);
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_DONE)
					{
						sqlite3_reset(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		//Dim ActivationList As List(Of String) = ReadMultiData("Activation", New String() {"UserID", "Keys", "KeyType", "ActType"}, "QQGroup like '" & QQgroup.ToString & "'", "QQNumber like '" & QQnumber.ToString & "'")

		public static List<string> ReadSingleData(string tableName, string columnName, string columnValue, string columnSearch)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select " + columnSearch + " from " + tableName + " where " + columnName + " like '%" + columnValue + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						string szText = "";
						for (var i = 0; i < columnCount; i++)
						{
							if (columnSearch == PointerToString(sqlite3_column_name(stmt, i)))
							{
								if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
								{
									szText = PointerToString(sqlite3_column_text(stmt, i)).Replace(":", "-").Replace("\r", "");
									ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", ""));
								}
							}
						}
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;

		}
		public static List<string> ReadSingleMultiData(string tableName, string[] columnSearch, params string[] columnAgr)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select * from " + tableName + " where " + string.Join(" AND ", columnAgr) + " ORDER BY RANDOM() LIMIT 1 OFFSET 0";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						for (var i = 1; i < columnCount; i++)
						{
							for (var n = 1; n < columnSearch.Length; n++)
							{
								if (columnSearch[n].ToString() == PointerToString(sqlite3_column_name(stmt, i)))
								{
									if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
									{
										ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", ""));
									}
								}
							}
						}
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}
		public static List<string> ReadMultiData(string tableName, string[] columnSearch, params string[] columnAgr)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select * from " + tableName + " where " + string.Join(" AND ", columnAgr);
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						for (var n = 0; n < columnSearch.Length; n++)
						{
							for (var i = 1; i < columnCount; i++)
							{
								if (columnSearch[n].ToString() == PointerToString(sqlite3_column_name(stmt, i)))
								{
									if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
									{
										//szText = szText & ":" & PointerToString(sqlite3_column_text(stmt, i)).Replace(":", "-").Replace(vbCr, "")
										ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).Replace(":", "-").Replace("\r", ""));
									}
								}
							}
						}
						//ItemList.Add(szText.TrimStart(":"))
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}
		//Dim KeysList As List(Of String) = ReadMultiData2("KeyStore", "Keys", "Description like '%" & KeySign1 & "%'", "Description like '%" & KeySign2 & "%'", "Description like '%" & KeySign3 & "%'", "Description like '%" & KeySign4 & "%'")
		public static List<string> ReadMultiData2(string tableName, string columnSearch, params string[] columnAgr)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select * from " + tableName + " where " + string.Join(" AND ", columnAgr) + " ORDER BY RANDOM() LIMIT 1 OFFSET 0";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						for (var i = 1; i < columnCount; i++)
						{
							if (PointerToString(sqlite3_column_name(stmt, i)) == columnSearch)
							{
								if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
								{
									ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", ""));

								}
							}
						}
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}

		//Dim VersionList As List(Of String) = ReadCustomColumn("VersionList", "VersionID", "VersionName")
		public static List<string> ReadCustomColumn(string tableName, params string[] columnAgr)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select * from " + tableName;
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						for (var i = 1; i < columnCount; i++)
						{
							for (var n = 0; n < columnAgr.Count(); n++)
							{
								if (PointerToString(sqlite3_column_name(stmt, i)) == columnAgr[n])
								{
									if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
									{
										ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", ""));
									}
								}
							}
						}
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);

			return ItemList;
		}
		// Dim itemlist = ReadCustomColumn2("ErrorCodeList", "LOWER(Errorcode)", UCase(mcErrorCode.Value), "HRESULT", "Reson", "ResonCN")
		public static List<string> ReadCustomColumn2(string tableName, string itemName, string itemValue, params string[] columnAgr)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select * from " + tableName + " WHERE " + itemName + " like '" + itemValue + "'";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						for (var i = 1; i < columnCount; i++)
						{
							foreach (string columns in columnAgr)
							{
								if (PointerToString(sqlite3_column_name(stmt, i)) == columns)
								{
									if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
									{
										ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", ""));
									}
								}
							}
						}
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}
		//Dim itemlist = ReadData("MSDNDownload", "VerNameCN", VerName, "DownUrl")
		public static string ReadData(string tableName, string columnName, string columnValue, string columnSearch)
		{
			//Dim ItemList As New List(Of String)
			var RetString = "";
			var sql = "Select " + columnSearch + " from " + tableName + " where " + columnName + " like '%" + columnValue + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						if (!(PointerToString(sqlite3_column_text(stmt, 0)) == null))
						{
							RetString = PointerToString(sqlite3_column_text(stmt, 0)).ToString().Replace("\r", "");
						}
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return RetString;
		}
		public static List<string> ReadAllColumn(string tableName, string columnName, string columnValue)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select * from " + tableName + " where " + columnName + " like '%" + columnValue + "%' ORDER BY RANDOM() LIMIT 1 OFFSET 0";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						for (var i = 1; i < columnCount; i++)
						{
							if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
							{
								ItemList.Add((PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", "")));
							}
						}
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}
		public static List<string> ReadAllColumnEx(string szDataPath, string tableName, string columnName, string columnValue)
		{
			List<string> ItemList = new List<string>();
			var sql = "Select * from " + tableName + " where " + columnName + " like '%" + columnValue + "%' ORDER BY RANDOM() LIMIT 1 OFFSET 0"; //ORDER BY ID DESC LIMIT 1 OFFSET 0"
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(szDataPath), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					while (sqlite3_step(stmt) == SQLITE_ROW)
					{
						int columnCount = sqlite3_column_count(stmt);
						for (var i = 1; i < columnCount; i++)
						{
							try
							{
								if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
								{
									ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", ""));

								}
							}
							catch 
							{
								ItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).ToString().Replace("\r", ""));
							}
						}
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}

		//DeleteMultiData("Activation", "QQGroup Like '" & RetQQgroup & "'", "QQGroup like '" & RetQQnumber & "'")
		public static bool DeleteMultiData(string tableName, params string[] columnAgr)
		{
			var sql = "DELETE FROM " + tableName + " WHERE " + string.Join(" AND ", columnAgr);
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_DONE)
					{
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static bool DeleteData(string tableName, string CounName, string TEXTBB)
		{
			if (string.IsNullOrEmpty(TEXTBB))
			{
				return false;
			}
			var sql = "DELETE FROM " + tableName + " WHERE " + CounName + "  Like '%" + TEXTBB + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_DONE)
					{
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static bool DeleteDataEx(string szDataPath, string tableName, string CounName, string TEXTBB)
		{
			if (string.IsNullOrEmpty(TEXTBB))
			{
				return false;
			}
			var sql = "DELETE FROM " + tableName + " WHERE " + CounName + "  Like '%" + TEXTBB + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(szDataPath), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_DONE)
					{
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static bool CheckDataExsit(string tableName, string CounName, string TEXTBB)
		{
			var sql = "Select * from " + tableName + " where " + CounName + " like '%" + TEXTBB + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_ROW)
					{
						sqlite3_finalize(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static bool CheckDataExsitEx(string szDataPath, string tableName, string CounName, string TEXTBB)
		{
			var sql = "Select * from " + tableName + " where " + CounName + " like '%" + TEXTBB + "%' ";
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(szDataPath), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					int sql_step = sqlite3_step(stmt);
					if (sql_step == SQLITE_ROW)
					{
						sqlite3_finalize(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				else
				{
					Console.WriteLine(sqlite3_errcode(hSqlite));
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public static bool CheckDataExsit2(string tableName, params string[] columnAgr)
		{
			var sql = "Select * from " + tableName + " WHERE " + string.Join(" AND ", columnAgr);
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					if (sqlite3_step(stmt) == SQLITE_ROW)
					{
						sqlite3_finalize(stmt);
						sqlite3_close(hSqlite);
						return true;
					}
					else
					{
						Console.WriteLine(sqlite3_errcode(hSqlite));
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return false;
		}
		public const int SQLITE_OK = 0; // Successful result
		public const int SQLITE_ERROR = 1; // SQL error or missing database
		public const int SQLITE_INTERNAL = 2; // Internal logic error in SQLite
		public const int SQLITE_PERM = 3; // Access permission denied
		public const int SQLITE_ABORT = 4; // Callback routine requested an abort
		public const int SQLITE_BUSY = 5; // The database file is locked
		public const int SQLITE_LOCKED = 6; // A table in the database is locked
		public const int SQLITE_NOMEM = 7; // A malloc() failed
		public const int SQLITE_READONLY = 8; // Attempt to write a readonly database
		public const int SQLITE_INTERRUPT = 9; // Operation terminated by sqlite3_interrupt()
		public const int SQLITE_IOERR = 10; // Some kind of disk I/O error occurred
		public const int SQLITE_CORRUPT = 11; // The database disk image is malformed
		public const int SQLITE_NOTFOUND = 12; // Unknown opcode in sqlite3_file_control()
		public const int SQLITE_FULL = 13; // Insertion failed because database is full
		public const int SQLITE_CANTOPEN = 14; // Unable to open the database file
		public const int SQLITE_PROTOCOL = 15; // Database lock protocol error
		public const int SQLITE_EMPTY = 16; // Database is empty
		public const int SQLITE_SCHEMA = 17; // The database schema changed
		public const int SQLITE_TOOBIG = 18; // String or BLOB exceeds size limit
		public const int SQLITE_CONSTRAINT = 19; // Abort due to constraint violation
		public const int SQLITE_MISMATCH = 20; // Data type mismatch
		public const int SQLITE_MISUSE = 21; // Library used incorrectly
		public const int SQLITE_NOLFS = 22; // Uses OS features not supported on host
		public const int SQLITE_AUTH = 23; // Authorization denied
		public const int SQLITE_FORMAT = 24; // Auxiliary database format error
		public const int SQLITE_RANGE = 25; // 2nd parameter to sqlite3_bind out of range
		public const int SQLITE_NOTADB = 26; // File opened that is not a database file
		public const int SQLITE_ROW = 100; // sqlite3_step() has another row ready
		public const int SQLITE_DONE = 101; // sqlite3_step() has finished executing

		public enum SqliteErrorCode
		{
			SQLITE_ERROR,
			SQLITE_INTERNAL,
			SQLITE_PERM,
			SQLITE_ABORT,
			SQLITE_BUSY,
			SQLITE_LOCKED,
			SQLITE_NOMEM,
			SQLITE_READONLY,
			SQLITE_INTERRUPT,
			SQLITE_IOERR,
			SQLITE_CORRUPT,
			SQLITE_NOTFOUND,
			SQLITE_FULL,
			SQLITE_CANTOPEN,
			SQLITE_PROTOCOL,
			SQLITE_EMPTY,
			SQLITE_SCHEMA,
			SQLITE_TOOBIG,
			SQLITE_CONSTRAINT,
			SQLITE_MISMATCH,
			SQLITE_MISUSE,
			SQLITE_NOLFS,
			SQLITE_AUTH,
			SQLITE_FORMAT,
			SQLITE_RANGE,
			SQLITE_NOTADB
		}

		public enum SQLiteDataTypes
		{
			INT = 1,
			FLOAT,
			TEXT,
			BLOB,
			NULL
		}
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open")]
		public extern static int sqlite3_open(string filename, ref IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_close")]
		public extern static int sqlite3_close(IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_prepare_v2")]
		public extern static int sqlite3_prepare_v2(IntPtr db, string zSql, int nByte, ref IntPtr ppStmpt, ref IntPtr transient);
		[DllImport("sqlite3", EntryPoint = "sqlite3_prepare16_v2", CallingConvention = CallingConvention.Cdecl)]
		public extern static int sqlite3_prepare16_v2(IntPtr db, [MarshalAs(UnmanagedType.LPWStr)] string sql, int numBytes, out IntPtr stmt, IntPtr pzTail);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_text")]
		public extern static int sqlite3_bind_text(IntPtr stmHandle, int iParam, IntPtr value, int length, IntPtr destructor);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_text16")]
		public extern static int sqlite3_bind_text16(IntPtr stmHandle, int iParam, IntPtr value, int length, IntPtr destructor);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_int")]
		public extern static int sqlite3_bind_int(IntPtr stmHandle, int iParam, int value);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_double")]
		public extern static int sqlite3_bind_double(IntPtr stmHandle, int iParam, double value);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_blob")]
		public extern static int sqlite3_bind_blob(IntPtr stmHandle, int iParam, IntPtr value, int length, IntPtr destructor);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_value")]
		public extern static int sqlite3_bind_value(IntPtr stmHandle, int iParam, int value);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_null")]
		public extern static int sqlite3_bind_null(IntPtr stmHandle, int value);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_zeroblob")]
		public extern static int sqlite3_bind_zeroblob(IntPtr stmHandle, int iParam, int value);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_step")]
		public extern static int sqlite3_step(IntPtr stmHandle);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_count")]
		public extern static int sqlite3_column_count(IntPtr stmHandle);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_name")]
		public extern static IntPtr sqlite3_column_name(IntPtr stmHandle, int iCol);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text")]
		public extern static IntPtr sqlite3_column_text(IntPtr stmHandle, int iCol);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_reset")]
		public extern static int sqlite3_reset(IntPtr stmHandle);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_finalize")]
		public extern static int sqlite3_finalize(IntPtr stmHandle);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_type")]
		public extern static int sqlite3_column_type(IntPtr stmHandle, int iCol);
		[DllImport("sqlite3.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sqlite3_errcode(IntPtr sqlite3);
		[DllImport("sqlite3.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sqlite3_extended_errcode(IntPtr sqlite3);
		[DllImport("sqlite3.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static string sqlite3_errmsg(IntPtr sqlite3);
		[DllImport("sqlite3.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static string sqlite3_errstr(SqliHelper.SqliteErrorCode ErrCode);
		[DllImport("sqlite3.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sqlite3_extended_result_codes(IntPtr sqlite3, int OnOrOff);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int sqlite3_callback(IntPtr param, int size, string[] rec, string[] colName);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public extern static int sqlite3_exec(IntPtr db, IntPtr sql, sqlite3_callback cb, IntPtr callBackParam, ref IntPtr errMsg);
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetDllDirectory(string lpPathName);

	}
}
