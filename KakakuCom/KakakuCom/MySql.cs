using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Db
{
	class MySql
	{
		static MySqlConnection m_dbConn;
		static string m_strTableName = "";

		public MySql()
		{
		}
		// MySql DB接続
		public static int Connect(
			string i_strDb						// データベース名
			)
		{
			string txtDatabaseText = i_strDb;
			string txtServerIPText;
			if (Kc.Form1.m_iradioBtnServer == 1)
			{
				txtServerIPText = "127.0.0.1";
			}
			else
			{
				if (Kc.Form1.m_iradioBtnIP == 1)
				{
					txtServerIPText = "192.168.1.201";
				}
				else
				{
					txtServerIPText = "192.168.1.202";
				}
			}
			string txtUserNameText = "matsu";
			string txtPasswordText = "kenji0126";
			string strConnect = "server=" + txtServerIPText + ";uid=" + txtUserNameText + ";pwd=" + txtPasswordText + ";database=" + txtDatabaseText + ";";

			try
			{
				m_dbConn = new MySqlConnection(strConnect);
				m_dbConn.Open();
				if (m_dbConn.State.ToString() != "Open")
				{
					Dbg.Utl.MessageBoxShow("オープンできません", "DB connect エラー");
					return -1;
				}
			}
			catch (Exception ex)  // catch on general exceptions, not specific
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB connect エラー");
				return -1;
			}
			return 0;
		}

		public static int ChangeDb(
			string i_strDb						// データベース名
			)
		{
			try
			{
				m_dbConn.ChangeDatabase(i_strDb);
			}
			catch (Exception ex)  // catch on general exceptions, not specific
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB connect エラー");
				return -1;
			}
			return 0;
		}

		// MySql DB解放
		public static int DisConnect()
		{
			try
			{
				m_dbConn.Close();
				m_dbConn = null;
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB Disconnect エラー");
				return -1;
			}
			return 0;
		}

		// Close
		public static void Close()
		{
			if (m_dbConn != null)
			{
				m_dbConn.Close();
			}
		}

		// タイムアウト時間(8時間)設定
		public static int setTimeoutTime(
			)
		{
			string strSql;
			int nRet;
			try
			{
				strSql = "set global wait_timeout = 28800;";
				MySqlCommand command = new MySqlCommand(strSql, m_dbConn);
				nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{


				Dbg.Utl.MessageBoxShow(ex.Message, "DB タイムアウト設定 エラー");
				return -1;
			}
			return 0;
		}

		// DB作成
		public static void CreateDB(
			string i_strDBName					// DB名
			)
		{
			string strSql;
			MySqlCommand command;
			int nRet;
			// DB削除
			strSql = "DROP DATABASE IF EXISTS " + i_strDBName;
			command = new MySqlCommand(strSql, m_dbConn);
			nRet = command.ExecuteNonQuery();
			
			// DB作成
			strSql = "CREATE DATABASE " + i_strDBName + " CHARACTER SET utf8";
			command = new MySqlCommand(strSql, m_dbConn);
			nRet = command.ExecuteNonQuery();
		}

		// テーブル作成
		public static int CreateTable(
			string i_strTableName,				// テーブル名
			string[] i_strHdr,					// 項目名
			string[] i_strType,					// 項目型
			string i_strKey,					// キー,インデックス　または　NULL
			int i_nHdr							// 項目数
			)
		{
			string strSql;
			int nRet;
			MySqlCommand command;

			m_strTableName = i_strTableName;
			// テーブル削除
			try
			{
				strSql = "DROP TABLE IF EXISTS " + m_strTableName;
				command = new MySqlCommand(strSql, m_dbConn);
				nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB テーブル削除 エラー");
				return -1;
			}

			// テーブル作成
			try
			{
				//			strSql = "CREATE TABLE " + m_strTableName + " (IntData int, StringData varchar(32))";
				strSql = "CREATE TABLE " + m_strTableName + " (" + i_strHdr[0] + " " + i_strType[0];
				for (int j = 1; j < i_nHdr; j++)
				{
					strSql += ("," + i_strHdr[j] + " " + i_strType[j]);
				}
				if (i_strKey != "")
				{
					strSql += ("," + i_strKey);
				}
				strSql += ") CHARACTER SET utf8";
				command = new MySqlCommand(strSql, m_dbConn);
				nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB テーブル作成 エラー");
				return -1;
			}
			return 0;
		}

		// レコード追加
		public static int insRecord(
			string i_strTableName,				// テーブル名
			string[] i_strItem,					// 項目
			int i_nHdr							// 項目数
			)
		{
			string strSql;
			int nRet;
			try
			{
				strSql = "INSERT INTO " + i_strTableName + " VALUES(\"" + i_strItem[0];
				for (int j = 1; j < i_nHdr; j++)
				{
					strSql += ("\",\"" + i_strItem[j]);
				}
				strSql += "\")";
				MySqlCommand command = new MySqlCommand(strSql, m_dbConn);
				nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB レコード追加 エラー");
				return -1;
			}
			return 0;
		}

		// 複数レコード追加
		public static int insMultiRecord(
			string i_strTableName,              // テーブル名
			string[,] i_strItem,                // 項目
			int i_nr,                           // レコード数
			int i_nHdr                          // 項目数
			)
		{
			string strSql;
			int nRet;
			StringBuilder sb = new StringBuilder();
			for (int ir = 0; ir < i_nr; ir++)
			{
				sb.Append("(");
				for (int ik = 0; ik < i_nHdr; ik++)
				{
					sb.Append("\"" + i_strItem[ir, ik] + "\",");
				}
				sb.Remove(sb.Length - 1, 1);
				sb.Append("),");
			}
			sb.Remove(sb.Length - 1, 1);
			try
			{
				strSql = "INSERT INTO " + i_strTableName + " VALUES " + sb.ToString();
				MySqlCommand command = new MySqlCommand(strSql, m_dbConn);
				nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB レコード追加 エラー");
				return -1;
			}
			return 0;
		}

		//// レコード追加修正
		//public static int updtRecord(
		//	string i_strTableName,				// テーブル名
		//	string[] i_strItem,					// 項目
		//	int i_nHdr							// 項目数
		//	)
		//{
		//	string strSql;
		//	int nRet;
		//	try
		//	{
		//		strSql = "UPDATE " + i_strTableName + " VALUES(\"" + i_strItem[0];
		//		for (int j = 1; j < i_nHdr; j++)
		//		{
		//			strSql += ("\",\"" + i_strItem[j]);
		//		}
		//		strSql += "\")";
		//		MySqlCommand command = new MySqlCommand(strSql, m_dbConn);
		//		nRet = command.ExecuteNonQuery();
		//	}
		//	catch (Exception ex)
		//	{
		//		Dbg.Utl.MessageBoxShow(ex.Message, "DB レコード追加 エラー");
		//		return -1;
		//	}
		//	return 0;
		//}

		// レコード追加修正
		public static int repRecord(
			string i_strTableName,				// テーブル名
			string[] i_strItem,					// 項目
			int i_nHdr							// 項目数
			)
		{
			string strSql;
			int nRet;
			try
			{
				strSql = "REPLACE INTO " + i_strTableName + " VALUES(\"" + i_strItem[0];
				for (int j = 1; j < i_nHdr; j++)
				{
					strSql += ("\",\"" + i_strItem[j]);
				}
				strSql += "\")";
				MySqlCommand command = new MySqlCommand(strSql, m_dbConn);
				nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{


				Dbg.Utl.MessageBoxShow(ex.Message, "DB レコード追加 エラー");
				return -1;
			}
			return 0;
		}

		// レコード削除
		public static int delRecord(
			string i_strTableName,              // テーブル名
			string i_strIdName,                 // 属性名
			string i_strId                      // 属性ID
			)
		{
			try
			{
				string strSql = "DELETE FROM " + i_strTableName + " WHERE " +
								i_strIdName + "=\"" + i_strId + "\"";
				MySqlCommand command = new MySqlCommand(strSql, m_dbConn);
				int nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB レコード削除 エラー");
				return -1;
			}
			return 0;
		}

		// レコード削除(where 指定)
		public static int delRecordW(
			string i_strTableName,              // テーブル名
			string i_strCondW					// 条件(WHWRE)
			)
		{
			try
			{
				string strSql = "DELETE FROM " + i_strTableName + " WHERE " + i_strCondW;
				MySqlCommand command = new MySqlCommand(strSql, m_dbConn);
				int nRet = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB レコード削除 エラー");
				return -1;
			}
			return 0;
		}

		// 項目名取得
		public static int getHeader(
			string i_strTable,					// テーブル名
			string[] o_strHdrName,				// 項目名
			out int o_nHdr
			)
		{
			MySqlCommand cmd;
			MySqlDataReader rdr;
			o_nHdr = 0;
			try
			{
				cmd = new MySqlCommand("SELECT * FROM " + i_strTable, m_dbConn);
				rdr = cmd.ExecuteReader();

				//カラム名出力
				string[] names = new string[rdr.FieldCount];
				for (int i = 0; i < rdr.FieldCount; i++)
				{
					o_strHdrName[i] = rdr.GetName(i);
				}
				o_nHdr = rdr.FieldCount;
				rdr.Close();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB ヘッダー取得 エラー");
				return -1;
			}
			return 0;
		}

		// テーブルの条件が合致した複数行の指定項目データを取得
		// 項目データバッファの配列サイズが読み込みの最大行数
		// 条件==""の場合は全行読み込み
		// ソート条件に従った順に並べて出力
		public static int getTableItems(
			string i_strTbl,					// テーブル名
			string[] i_strHdr,					// 出力項目名
			int i_nHdr,							// 出力項目数
			string i_strCondw,					// 条件(WHERE)		例 "ID=XX" 
			string i_strCondo,					// ソート条件(ORDER BY)  例 "ID"			IDでソート
												//							"ID1, ID2 des"	ID1でソートを行い同一値ならID2で逆順にソート
			string[,] o_strItems,				// 項目データバッファ
			out int o_nRow						// 読み込み行数
			)
		{
			string sql;
			MySqlCommand cmd;
			MySqlDataReader rdr;
			o_nRow = 0;
			int nI = o_strItems.GetLength(0);					// 最大行数
			int nJ = o_strItems.GetLength(1);					// 最大項目数

			if (nJ < i_nHdr)
			{
				Dbg.Utl.MessageBoxShow("getTableItems:Db項目数オーバーフロー", "i_strTbl");
			}
			// sql = "SELECT hdr1,hdr2,hdr3,･･･ FROM table { WHERE 条件}{ ORDER BY ソート条件} LIMIT 項目データバッファの行数";
			sql = "SELECT " + i_strHdr[0];
			for (int j = 1; j < i_nHdr; j++)
			{
				sql += ("," + i_strHdr[j]);
			}
			sql += (" FROM " + i_strTbl);
			if (i_strCondw != "")
			{
				sql += (" WHERE " + i_strCondw);
			}
			if (i_strCondo != "")
			{
				sql += (" ORDER BY " + i_strCondo);
			}
			sql += (" LIMIT " + nI.ToString());
			try
			{
				cmd = new MySqlCommand(sql, m_dbConn);
				rdr = cmd.ExecuteReader();
				int iR;
				for (iR = 0; iR < nI && rdr.Read(); iR++)
				{
					for (int j = 0; j < i_nHdr; j++)
					{
						o_strItems[iR, j] = rdr.GetString(j);
					}
				}
				o_nRow = iR;
				rdr.Close();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB データ取得 エラー");
				return -1;
			}
			return 0;
		}

		// ログテーブルの条件が合致した複数行の指定項目データを取得
		// 条件==""の場合は全行の指定項目
		// ソート条件に従った順に並べて出力
		public static int getLogTableItems(
			string i_strTbl,					// テーブル名
			string[] i_strHdr,					// 出力項目名
			int i_nHdr,							// 出力項目数
			string i_strCondw,					// 条件(where)		例 "ID=XX" 
			string i_strCondo,					// 条件(order by)	例 "ID"  | "ID1, ID2 des" (ID1でソートを行い同一値ならID2で逆順にソート)
			string[,] o_strItems,				// 項目データ
			out int o_nRow						// 行数
			)
		{
			string sql;
			MySqlCommand cmd;
			MySqlDataReader rdr;
			o_nRow = 0;
			int nI = o_strItems.GetLength(0);					// 最大行数
			int nJ = o_strItems.GetLength(1);					// 最大項目数

			if (nJ < i_nHdr)
			{
				Dbg.Utl.MessageBoxShow("getTableItems:ログDb項目数オーバーフロー", "i_strTbl");
			}
			// sql = "SELECT hdr1,hdr2,hdr3,･･･ from table";
			sql = "SELECT pageview." + i_strHdr[0];
			for (int j = 1; j < i_nHdr; j++)
			{
				if (i_strHdr[j] == "useragent")
				{
					sql += (",text");
				}
/*
				else if (i_strHdr[j] == "customer")
				{
					// http://shobon.hatenablog.com/entry/2013/01/28/190900
					',customer,' || '.' ||
					lpad(split_part(customer, '.', 1), 3, '0') || '.' ||
					lpad(split_part(customer, '.', 2), 3, '0') || '.' ||
					lpad(split_part(customer, '.', 3), 3, '0') || '.' ||
					lpad(split_part(customer, '.', 4), 3, '0') ASCIIEncoding ip
				}
 */ 
				else
				{
					sql += ("," + i_strHdr[j]);
				}
			}
			sql += (" FROM " + i_strTbl + ",useragent");
			if (i_strCondw != "")
			{
				sql += (" WHERE " + i_strCondw);
			}
			sql += " AND pageview.useragent=useragent.id";
			if (i_strCondo != "")
			{
				sql += (" ORDER BY " + i_strCondo);
			}
			sql += (" LIMIT " + nI.ToString());
			try
			{
				cmd = new MySqlCommand(sql, m_dbConn);
				rdr = cmd.ExecuteReader();
				int iR;
				for (iR = 0; iR < nI && rdr.Read(); iR++)
				{
					for (int j = 0; j < i_nHdr; j++)
					{
						o_strItems[iR, j] = rdr.GetString(j);
					}
				}
				o_nRow = iR;
				rdr.Close();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB データ取得 エラー");
				return -1;
			}
			return 0;
		}

		// テーブルの条件が合致した行の指定項目データを取得 
		public static bool getRecordItems(		// ステイタス	(true:データ有り, false:データなし)
			string i_strTbl,					// テーブル名
			string[] i_strHdr,					// 出力項目名
			int i_nHdr,							// 出力項目数
			string i_strCond,					// 検索条件(where)	例 "ID=XX" 
			string[] o_strItems					// 項目データ
			)
		{
			bool bSt;
			string sql;
			MySqlCommand cmd;
			MySqlDataReader rdr;
			int nJ = o_strItems.GetLength(0);

			// sql = "SELECT hdr1,hdr2,hdr3,･･･ from table";
			sql = "SELECT " + i_strHdr[0];
			for (int j = 1; j < i_nHdr; j++)
			{
				if (j == nJ)
				{
					Dbg.Utl.MessageBoxShow("getRecordItems:Dbオーバーフロー", "i_strTbl");
				}
				sql += ("," + i_strHdr[j]);
			}
			sql += (" FROM " + i_strTbl + " WHERE " + i_strCond);
			try
			{
				cmd = new MySqlCommand(sql, m_dbConn);
				rdr = cmd.ExecuteReader();
				bSt = rdr.Read();
				if (bSt)
				{
					for (int j = 0; j < i_nHdr; j++)
					{
						if (j == nJ)
						{
							Dbg.Utl.MessageBoxShow("getRecordItems:Db項目数オーバーフロー", "i_strTbl");
						}
						o_strItems[j] = rdr.GetString(j);
					}
				}
				rdr.Close();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB データ取得 エラー");
				bSt=false;
			}
			return bSt;
		}

		// テーブルの最大2つの条件が合致した複数行の指定項目データを変更
		public static int modifyMultiCondTableItems2D(
			string i_strTbl,					// テーブル名
			string[] i_strHdr,					// 項目名
			int i_nHdr,							// 項目数
			// 条件 = 条件1 + 条件2
			string i_strCond11,					// 条件1(where)	固定部		(例 "ID=")
			string[] i_strCond12s,				// 条件2(where)	行選択部	(例 "XX1" ～ "XXn") または (例 "ID=XX1" ～ "ID=XXn")
			string i_strCond21,					// 条件1(where)	固定部		(例 "ID=")			または (例 "": 条件が一つの場合)
			string[] i_strCond22s,				// 条件2(where)	行選択部	(例 "XX1" ～ "XXn") または (例 "ID=XX1" ～ "ID=XXn")
			string[,] i_strItems,				// 項目データ
			int i_nRow							// 行数
			)
		{
			string sql;
			// sql = "UPDATE table hdr1=data1,hdr2=data2,hdr3=data3,･･･ where key1 = "a001";
			for (int i = 0; i < i_nRow; i++)
			{
				sql = "UPDATE " + i_strTbl + " SET " + i_strHdr[0] + "=\"" + i_strItems[i, 0] + "\"";
				for (int j = 1; j < i_nHdr; j++)
				{
					sql += ("," + i_strHdr[j] + "=\"" + i_strItems[i, j]) + "\"";
				}
				sql += (" WHERE " + i_strCond11 + i_strCond12s[i]);
				if (i_strCond21 != "")
				{
					sql += (" AND " + i_strCond21 + i_strCond22s[i]);
				}
				try
				{
					MySqlCommand cmd1 = new MySqlCommand(sql, m_dbConn);
					cmd1.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					Dbg.Utl.MessageBoxShow(ex.Message, "DB データ修正 エラー");
					return -1;
				}
			}
			return 0;
		}

		// テーブルの最大2つの条件が合致した行の指定項目データを変更
		// 2次元配列の項目データの指定行の値に変更
		public static int modifyMultiCondRecordItems2D(
			string i_strTbl,					// テーブル名
			string[] i_strHdr,					// 項目名
			int i_nHdr,							// 項目数
			// 条件 = 条件1 + 条件2
			string i_strCond11,					// 条件1(where)	固定部		(例 "ID=")
			string i_strCond12s,				// 条件2(where)	行選択部	(例 "XX1" ～ "XXn") または (例 "ID=XX1" ～ "ID=XXn")
			string i_strCond21,					// 条件1(where)	固定部		(例 "ID=")			または (例 "": 条件が一つの場合)
			string i_strCond22s,				// 条件2(where)	行選択部	(例 "XX1" ～ "XXn") または (例 "ID=XX1" ～ "ID=XXn")
			string[,] i_strItems,				// 複数行の項目データ
			int i_iItemn						// 修正する項目データの行番号
			)
		{
			string sql;
			// sql = "UPDATE table hdr1=data1,hdr2=data2,hdr3=data3,･･･ where key1 = "a001";
			sql = "UPDATE " + i_strTbl + " SET " + i_strHdr[0] + "=\"" + i_strItems[i_iItemn,0] + "\"";
			for (int j = 1; j < i_nHdr; j++)
			{
				sql += ("," + i_strHdr[j] + "=\"" + i_strItems[i_iItemn,j]) + "\"";
			}
			sql += (" WHERE " + i_strCond11 + i_strCond12s);
			if (i_strCond21 != "")
			{
				sql += (" AND " + i_strCond21 + i_strCond22s);
			}
			try
			{
				MySqlCommand cmd1 = new MySqlCommand(sql, m_dbConn);
				cmd1.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB データ修正 エラー");
				return -1;
			}
			return 0;
		}

		// テーブルの条件が合致した行の指定項目データを
		// 2次元配列の項目データの指定行の値に変更
		public static int modifySingleCondRecordItems2D(
			string i_strTbl,					// テーブル名
			string[] i_strHdr,					// 項目名
			int i_nHdr,							// 項目数
			string i_strConds,					// 条件(where)	例 "ID=XX" 
			string[,] i_strItems,				// 複数行の項目データ(2次元配列)
			int i_iItemn						// 修正する項目データの行番号
			)
		{
			string sql;
			// sql = "UPDATE table hdr1=data1,hdr2=data2,hdr3=data3,･･･ where key1 = "a001";
			sql = "UPDATE " + i_strTbl + " SET " + i_strHdr[0] + "=\"" + i_strItems[i_iItemn, 0] + "\"";
			for (int j = 1; j < i_nHdr; j++)
			{
				sql += ("," + i_strHdr[j] + "=\"" + i_strItems[i_iItemn, j]) + "\"";
			}
			sql += (" WHERE " + i_strConds);
			try
			{
				MySqlCommand cmd1 = new MySqlCommand(sql, m_dbConn);
				cmd1.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB データ修正 エラー");
				return -1;
			}
			return 0;
		}

		// テーブルの条件が合致した行の指定項目データを変更
		public static int modifySingleCondRecordItems1D(
			string i_strTbl,					// テーブル名
			string[] i_strHdr,					// 項目名
			int i_nHdr,							// 項目数
			string i_strConds,					// 条件(where)	例 "ID=XX" 
			string[] i_strItems					// 項目データ(1次元配列)
			)
		{
			string sql;
			// sql = "UPDATE table hdr1=data1,hdr2=data2,hdr3=data3,･･･ where key1 = "a001";
			sql = "UPDATE " + i_strTbl + " SET " + i_strHdr[0] + "=\"" + i_strItems[0] + "\"";
			for (int j = 1; j < i_nHdr; j++)
			{
				sql += ("," + i_strHdr[j] + "=\"" + i_strItems[j]) + "\"";
			}
			sql += (" WHERE " + i_strConds);
			try
			{
				MySqlCommand cmd1 = new MySqlCommand(sql, m_dbConn);
				cmd1.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB データ修正 エラー");
				return -1;
			}
			return 0;
		}

		public static void changeURL()
		{
			string[] strHdr = { "商品ID", "属性ID", "図形CD"};
			string[,] strItems = new string[1000000, 3];
			int nZukei;													// 小分類数
			Db.MySql.getTableItems(Kc.Const.z_strAttributeURL2Table, strHdr, 3, "", "", strItems, out nZukei);

			for (int ic = 0; ic < nZukei; ic++)
			{
				if (String.IsNullOrEmpty(strItems[ic,2]) == false)
				{
					if (strItems[ic, 2][0] == 'z')
					{
						continue;
					}
					// sql = "UPDATE table hdr1=data1,hdr2=data2,hdr3=data3,･･･ where key1 = "a001";
					string strValue = "Z0" + strItems[ic,2];
					string sql = "UPDATE " + Kc.Const.z_strAttributeURL2Table + " SET 図形CD=\"" + strValue +"\" WHERE 商品ID=\"" + 
								 strItems[ic,0] + "\" AND 属性ID=\"" + strItems[ic,1] + "\"";
					try
					{
						MySqlCommand cmd1 = new MySqlCommand(sql, m_dbConn);
						cmd1.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						Dbg.Utl.MessageBoxShow(ex.Message, "DB データ修正 エラー");
					}
				}
			}
		}

		// Temp 2
		public static void checkURL()
		{
			string[] strHdr = { "商品ID", "属性ID", "図形CD" };
			string[,] strItems = new string[1000000, 3];
			int nZukei;											// 小分類数
			Db.MySql.getTableItems(Kc.Const.z_strAttributeURL2Table, strHdr, 3, "", "", strItems, out nZukei);

			for (int ic = 0; ic < nZukei; ic++)
			{
				if (String.IsNullOrEmpty(strItems[ic, 2]))
				{
					continue;
				}
				string strZukeiCD = strItems[ic, 2];
				int nClm = strItems[ic, 2].Length;
				if (nClm != 15)
				{
//					int iStop = 1;								// デバッグ用 //F2
				}
/*
				if (strItems[ic, 2][0] == 'z')
				{
					continue;
				}
				// sql = "UPDATE table hdr1=data1,hdr2=data2,hdr3=data3,･･･ where key1 = "a001";
				string strValue = "Z0" + strItems[ic, 2];
				string sql = "UPDATE " + Kc.Const.z_strAttributeURLTable + " SET 図形CD=\"" + strValue + "\" WHERE 商品ID=\"" +
								strItems[ic, 0] + "\" AND 属性ID=\"" + strItems[ic, 1] + "\"";
				try
				{
					MySqlCommand cmd1 = new MySqlCommand(sql, m_dbConn);
					cmd1.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					Dbg.Utl.MessageBoxShow(ex.Message, "DB データ修正 エラー");
				}
 */ 
			}
		}

		// DB項目の最大値を取得 
		// 最大値の取得が出来ず中断
		public static bool getMaxValue(			// ステイタス	(true:データ有り, false:データなし)
			string i_strTbl,					// テーブル名
			string i_strHdr,					// 項目名
			string i_strCond,					// 条件(where)	例 "ID=XX" 
			out string o_strItems				// 最大値
			)
		{
			bool bSt;
			string sql;
			MySqlCommand cmd;
			MySqlDataReader rdr;
			o_strItems = "0";

			// sql = "SELECT hdr1,hdr2,hdr3,･･･ from table";
			sql = "SELECT 商品ID,MAX('" + i_strHdr + "') as MaxID";
			sql += (" FROM " + i_strTbl + " WHERE " + i_strCond);
			try
			{
				cmd = new MySqlCommand(sql, m_dbConn);
				rdr = cmd.ExecuteReader();

				//カラム名出力
				string[] names = new string[rdr.FieldCount];
				for (int i = 0; i < rdr.FieldCount; i++)
					names[i] = rdr.GetName(i);
				Console.WriteLine(string.Join("\t", names));

				//テーブル出力
				while (rdr.Read())
				{
					string[] row = new string[rdr.FieldCount];
					for (int i = 0; i < rdr.FieldCount; i++)
						row[i] = rdr.GetString(i);
					string si1 = rdr["MaxID"].ToString().Trim();
					Console.WriteLine(string.Join("\t", row));
				}

				bSt = rdr.Read();
				if (bSt)
				{
					o_strItems = rdr.GetString(0);
					string s1 = rdr[0].ToString();
				}
				bSt = rdr.Read();
				if (bSt)
				{
					o_strItems = rdr.GetString(0);
					string s1 = rdr[0].ToString();
				}
				rdr.Close();
			}
			catch (Exception ex)
			{
				Dbg.Utl.MessageBoxShow(ex.Message, "DB データ取得 エラー");
				bSt = false;
			}
			return bSt;
		}
	}
}
