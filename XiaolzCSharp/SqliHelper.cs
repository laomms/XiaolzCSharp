//https://github.com/laomms
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
		/// <summary>
		/// 创建表
		/// </summary>
		/// <param name="TableNames">表名数组,每个元素对应一个列名数组/例:new string[] { "Table1", ""Table2" }</param>
		/// <param name="KeyNames"><列名>+<数据类型>数组/例:new string[]{ "`KEY1` TEXT", "`KEY2` TEXT" }</param>
		/// <returns>返回true创建成功</returns>
		/// <sample>CreateTable(new string[] { "table1", "table2" },  new List<string[]>() {new string[]{ "`key1` TEXT", "`key2` TEXT" },new string[]{ "`key1` TEXT", "`key2` TEXT" }});</sample>
		public static bool CreateTable(string[] TableNames, List<string[]> KeyNames)
		{
			if (TableNames.Length != KeyNames.Count) return false;
			IntPtr hSqlite = new IntPtr();
			IntPtr transient = new IntPtr();
			sqlite3_open(ConvertString2UTF8(DataPath), ref hSqlite);
			for (int i = 0; i < TableNames.Count(); i++)
			{
				string ss = string.Join(",", KeyNames[i]);
				string sql = "CREATE TABLE IF NOT EXISTS `" + TableNames[i] + "` (ID INTEGER PRIMARY KEY AUTOINCREMENT, " + string.Join(",", KeyNames[i]) + ")";
				if (sqlite3_exec(hSqlite, StringToPointer(sql), null, IntPtr.Zero, ref transient) == SQLITE_OK)
				{
					Console.WriteLine("Create Successfully");
				}
			}
			sqlite3_close(hSqlite);
			return false;
		}
		/// <summary>
		/// 读取数据库某一栏
		/// </summary>
		/// <param name="tableNames">表名</param>
		/// <param name="columnName">键名</param>
		/// <param name="columnValue">键值</param>
		/// <param name="columnSearch">要搜索的键名</param>
		/// <returns>返回该列表所有字符串泛型集合</returns>
		/// <sample>ReadAllData("Table1", "Key1", Value1, "Key2")</sample>
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
		private string Condition(GroupBox GroupBox1)
		{
			string MyWhere = "";
			foreach (Control Ctl in GroupBox1.Controls)
			{
				if ((Ctl) is TextBox)
				{
					if (Ctl.Text.Length != 0)
					{
						MyWhere = MyWhere + Ctl.Name + " like '%" + Ctl.Text + "%' And ";
					}
				}
				else if ((Ctl) is ComboBox)
				{
					if (Ctl.Text.Length != 0)
					{
						MyWhere = MyWhere + Ctl.Name + " like '%" + Ctl.Text + "%' And ";
					}
				}
			}
			if (MyWhere.Length > 0)
			{
				MyWhere = " where " + MyWhere.Substring(0, MyWhere.Length - 4);
			}
			return MyWhere;
		}

		/// <summary>
		/// 导入某表到listview
		/// </summary>
		/// <param name="ListView1">ListView控件名</param>
		/// <param name="tableName">表名名</param>
		/// <param name="condition">要搜索的条件</param>
		/// <returns></returns>
		/// <sample>CheckImporlistview(this.listView1, "table1", "");</sample>
		public static void CheckImporlistview(ListView ListView1, string tableName, string condition)
		{
			ListView1.Items.Clear();
			ListViewItem ITM = null;
			var sql = "Select * from " + tableName + condition;
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
									try
                                    {
										ITM.SubItems.Add(Convert.ToDateTime(PointerToString(sqlite3_column_text(stmt, i))).ToString("yyyy-MM-dd hh:mm:ss"));
									}
                                    catch 
									{
										ITM.SubItems.Add(PointerToString(sqlite3_column_text(stmt, i)));
									}
									
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
		/// <summary>
		/// 插入数据
		/// </summary>
		/// <param name="tableNames">表名</param>
		/// <param name="columnName">键名数组</param>
		/// <param name="columnValue">键值数组</param>
		/// <returns>返回是否成功</returns>
		/// <sample>InsertData("table1", new string[] { "key1", "key2" }, new string[] {value1,value2 });</sample>
		public static bool InsertData(string tableName, string[] columnName, string[] columnValue)
		{
			if (columnName.Length != columnValue.Length) return false;
			List<string> strList = new List<string>();
			string sql = "";
			if (columnName.Length > 1)
            {				
				for (int i = 0; i < columnValue.Count(); i++)
				{
					strList.Add("?");
				}
				sql = "Insert Or Ignore Into " + tableName + "(" + string.Join(",", columnName) + ") VALUES(" + string.Join(",", strList) + ")";
			}
            else
            {
				sql = "Insert Or Ignore Into " + tableName[0] + "(" + columnName[0] + ") VALUES(?)";
			}			
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

		public static string MatchToString(Match m)
		{
			return m.Value;
		}
		/// <summary>
		/// 更新数据
		/// </summary>
		/// <param name="tableNames">表名</param>
		/// <param name="setAgr">要更新的键名组合</param>
		/// <param name="condition">搜索的条件数组</param>
		/// <returns>返回是否成功</returns>
		/// <sample>UpdateData("tabl1", new string[] { "key3 like'" + condition1 + "'", "key4 like'" + condition2 + "'"}, "key1='" + value1 + "'", "key2='value2'" )</sample>
		public static bool UpdateData(string tableName, string[] condition, params string[] setAgr)
		{
			if (condition.Length == 0 || setAgr.Length == 0) return false;
			string sql;
			string[] matches=null;
			var setvalue = Regex.Replace(string.Join(",", setAgr), "'.*?'", "?");
			MatchCollection matchList = Regex.Matches(string.Join(",", setAgr), "(?<==').*?(?=')");
			Match[] matchArray = new Match[matchList.Count];
			matchList.CopyTo(matchArray, 0);
			matches = Array.ConvertAll(matchArray, new Converter<Match, string>(MatchToString));
			if (setAgr.Length > 1 && condition.Length > 1 )
            {				
				sql = "UPDATE " + tableName + " SET " + string.Join(",", setvalue) + "' WHERE " + string.Join(" AND ", condition);
			}
			else if (setAgr.Length == 1 && condition.Length > 1)
            {
				sql = "UPDATE " + tableName + " SET " + setAgr[0] + "' WHERE " + string.Join(" AND ", condition);
			}
			else if (setAgr.Length > 1 && condition.Length == 1)
            {
				sql = "UPDATE " + tableName + " SET " + string.Join(",", setvalue) + " WHERE " + condition[0];
			}
			else
            {
				sql = "UPDATE " + tableName + " SET " + setAgr[0] + " WHERE " + condition[0];
			}
			
			int num = System.Text.Encoding.Unicode.GetByteCount(sql);
			IntPtr hSqlite = new IntPtr();
			if (sqlite3_open(ConvertString2UTF8(Convert.ToString(DataPath)), ref hSqlite) == SQLITE_OK)
			{
				IntPtr stmt = new IntPtr();
				IntPtr transient = new IntPtr();
				if (sqlite3_prepare16_v2(hSqlite, sql, num, out stmt, transient) == SQLITE_OK)
				{
					//sqlite3_exec(hSqlite, StringToPointer(sql), IntPtr.Zero, IntPtr.Zero, transient)
					for (var i = 0; i < setAgr.Length; i++)
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

		/// <summary>
		/// 读取数据
		/// </summary>
		/// <param name="tableNames">表名</param>
		/// <param name="condition">搜索的条件集合</param>
		/// <param name="columnSearch">要读取的数组</param>
		/// <returns>返回搜索结果集合</returns>
		/// <sample>ReadData("table1", new string[] {"key2", "key5"}, "key1 like '" + value1 + "'", "key2 like 'value2'")</sample>
		public static List<List<string>> ReadData(string tableName, string[] columnSearch, string SortOrder, params string[] condition )
		{
			
			List<List<string>> ItemList = new List<List<string>>();
			if (condition.Length == 0 || columnSearch.Length == 0) return ItemList;
			List<string> SubItemList = new List<string>();
			string sql = "";
			if (condition.Length > 1 && columnSearch.Length > 1 )
            {
				sql = "Select " + string.Join(",", columnSearch) + " from " + tableName + " where " + string.Join(" AND ", condition) + SortOrder;
			}
			else if (condition.Length > 1 && columnSearch.Length == 1)
            {
				sql = "Select " + columnSearch[0] + " from " + tableName + " where " + string.Join(" AND ", condition) + SortOrder;
			}
			else if (condition.Length == 1 && columnSearch.Length > 1)
			{
				sql = "Select " + string.Join(",", columnSearch)  + " from " + tableName + " where " + condition[0] + SortOrder;
			}
			else
            {
				sql = "Select " + columnSearch[0] + " from " + tableName + " where " + condition[0] + SortOrder; //+ " ORDER BY RANDOM() LIMIT 1 OFFSET 0";'
			}
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
						SubItemList=new List<string>();
						int columnCount = sqlite3_column_count(stmt);
						for (var i = 0; i < columnCount; i++)
						{
							if (!(PointerToString(sqlite3_column_text(stmt, i)) == null))
							{
								SubItemList.Add(PointerToString(sqlite3_column_text(stmt, i)).Replace(":", "-").Replace("\r", ""));
							}

						}
						ItemList.Add(SubItemList);
					}
				}
				sqlite3_finalize(stmt);
			}
			sqlite3_close(hSqlite);
			return ItemList;
		}

		/// <summary>
		/// 删除数据
		/// </summary>
		/// <param name="tableNames">表名</param>
		/// <param name="condition">搜索的条件集合</param>
		/// <param name="columnSearch">要读取的数组</param>
		/// <returns>返回搜索结果集合</returns>
		/// <sample>DeleteData("table1", "key1 Like '" + value1 + "'", "key2 like 'value2'")</sample>
		public static bool DeleteData(string tableName, params string[] condition)
		{
			if (condition.Length == 0) return false;
			string sql = "";
			if (condition.Length > 0)
            {
				sql = "DELETE FROM " + tableName + " WHERE " + string.Join(" AND ", condition);
			}
			else
            {
				sql = "DELETE FROM " + tableName + " WHERE " + condition[0];
			}
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

		/// <summary>
		/// 清空某表
		/// </summary>
		/// <param name="tableNames">表名</param>		
		/// <returns>返回是否成功</returns>
		/// <sample>ClearTable("table1")</sample>
		public static bool ClearTable(string tableName)
		{
			string sql = "DELETE FROM " + tableName ;
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
		/// <summary>
		/// 单条件判断是否存在某键值
		/// </summary>
		/// <param name="tableNames">表名</param>
		/// <param name="columnName">键名</param>
		/// <param name="columnValue">键值</param>
		/// <returns>有该键值返回true</returns>
		/// <sample>CheckDataExsit("Table1", "Key1", Value1)</sample>
		public static bool CheckDataExsit(string tableName, string columnName, string columnValue)
		{
			var sql = "Select * from " + tableName + " where " + columnName + " like '%" + columnValue + "%' ";
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
		/// <summary>
		/// 多条件判断是否存在某键值
		/// </summary>
		/// <param name="tableNames">表名</param>
		/// <param name="condition">键名+键值条件参数集合/例: "Key1 like '%" + value1 + "%'", "Key2 like 'value2'" </param>
		/// <returns>有该键值返回true</returns>
		/// <sample>CheckDataExsit2("Table1", "Key1 like '%" + value1 + "%'", "Key2 like 'value2'"</sample>
		public static bool CheckDataExsit2(string tableName, params string[] condition)
		{
			var sql = "Select * from " + tableName + " WHERE " + string.Join(" AND ", condition);
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
