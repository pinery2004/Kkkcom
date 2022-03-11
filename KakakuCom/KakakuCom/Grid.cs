//#define SWITCH_SCLASS
//#define SWITCH_ATTRVALUE
//#define SWITCH_ATTRURL
#define SWITCH_SYOHIN

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Kc
{
	public partial class Form1 : Form
	{
		static string z_strGridTblName = "";                    // グリッドテーブルに表示中のテーブル名
		static int z_nmaxHdr = 100;                             // 最大項目数
		static int z_nmaxRow = 1000000;                         // 最大行数
		string z_strGTblName = "DB";                            // テーブルの左上の項目に表示

		static int m_nHdr, m_nRow;                              // グリッドテーブルの項目数と行数

		string[] m_strOrigKey1 = new string[z_nmaxRow];         // オリジナルキー
		string[] m_strOrigKey2 = new string[z_nmaxRow];         // オリジナルキー

		// グリッドテーブル初期化
		private void InitGridTbl()
		{
			z_strGridTblName = "";
			// 初期化
			dataGridView1.Rows.Clear();
			dataGridView1.Columns.Clear();
			// 行ヘッダーの幅が自動調整を規定値に戻す	
			dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
			// データセルの幅を自動調整しないよう戻す
			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
		}

		// グリッドテーブルにmySQL DBから取り込む
		private void DispGridTbl(
			string i_strTblName                 // PHP DB テーブル名称
			)
		{
			string[] strDB = { Const.z_strDb1, Const.z_strDb2, Const.z_strDbLog };

			// 初期化
			InitGridTbl();

			z_strGridTblName = i_strTblName;

			if (i_strTblName == Kc.Const.z_strLogTable || i_strTblName == Kc.Const.z_strUserAgentTable)
			{
				if (Db.MySql.ChangeDb(strDB[2]) < 0)
				{
					Dbg.Utl.MessageBoxShow(strDB[2] + "DBオープンエラー", "DBエラー");
				}
			}

			//// 初期化
			//dataGridView1.Rows.Clear();
			//dataGridView1.Columns.Clear();
			//// 行ヘッダーの幅が自動調整を規定値に戻す	
			//dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
			//// データセルの幅を自動調整しないよう戻す
			//dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

			// 検索条件
			string strCond = "";
			if (i_strTblName == Kc.Const.z_strAttributeNameTable || i_strTblName == Kc.Const.z_strMSCRelation ||
				i_strTblName == Kc.Const.z_strSyohin2Table)
			{
				if (String.IsNullOrEmpty(Form1.m_strTextSClassID))
				{
					strCond = "";
				}
				else
				{
					strCond = "小分類ID=" + Form1.m_strTextSClassID;
				}
			}
			else if (i_strTblName == Kc.Const.z_strAttributeValue2Table || i_strTblName == Kc.Const.z_strAttributeURL2Table)
			{
				if (String.IsNullOrEmpty(Form1.m_strTextSyohinID))
				{
					if (string.IsNullOrEmpty(Form1.m_strTextSClassID))
					{
						strCond = "";
					}
					else
					{
						string strMinSyohinID = Form1.m_strTextSClassID + "00000";
						string strMaxSyohinID = Form1.m_strTextSClassID + "99999";
						strCond = "商品ID>=" + strMinSyohinID + " AND 商品ID<=" + strMaxSyohinID;
					}
				}
				else
				{
					strCond = "商品ID=" + Form1.m_strTextSyohinID;
				}
			}
			string strCondIn = textBoxCtrlUrlFN.Text;
			if (strCondIn != "")
			{
				if (strCond != "")
				{
					strCond += " AND ";
				}
				strCond += "(" + strCondIn + ")";
			}

			// ユーザ操作による行追加を無効(禁止)
			dataGridView1.AllowUserToAddRows = false;

			// DataGridViewコントロールの設定
			// 項目名の取得
			string[] strHdr = new string[z_nmaxHdr];
			Db.MySql.getHeader(i_strTblName, strHdr, out m_nHdr);

			// 項目名(カラム)の設定
			for (int j = 0; j < m_nHdr; j++)
			{
				dataGridView1.Columns.Add("column" + j.ToString(), strHdr[j]);
			}
			// 左上隅のヘッダーセルに表示
			dataGridView1.TopLeftHeaderCell.Value = z_strGTblName;

			// mySQLから項目データの取得
			string[,] strData = new string[z_nmaxRow, m_nHdr];
			//			Db.MySql.getTableItems(i_strTblName, strHdr, m_nHdr, strCond, "", strData, out m_nRow);
			string strOrder;
			if (i_strTblName.Substring(0, 4) == "attr" || i_strTblName.Substring(0, 4) == "cont")
			{
				strOrder = strHdr[0] + "," + strHdr[1];
			}
			else
			{
				strOrder = strHdr[0];
			}
			Db.MySql.getTableItems(i_strTblName, strHdr, m_nHdr, strCond, strOrder, strData, out m_nRow);

			// 項目データの表示
			for (int i = 0; i < m_nRow; i++)
			{
				dataGridView1.Rows.Add();
				for (int j = 0; j < m_nHdr; j++)
				{
					dataGridView1.Rows[i].Cells[j].Value = strData[i, j];

				}
				//DataGridView1の行ヘッダーに行番号を表示する
				dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
				m_strOrigKey1[i] = strData[i, 0];
				if (z_strGridTblName.Substring(0, 4) == "attr")
				{
					m_strOrigKey2[i] = strData[i, 1];
				}
                //if (i > 11111) break;
			}
			// 行ヘッダーの幅が自動調整されるようにする	
			dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
			// ヘッダーとすべてのセルの内容に合わせて、列の幅を自動調整する
			//			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
			//自動的に並び替えられないようにする
			foreach (DataGridViewColumn c in dataGridView1.Columns)
			{
				c.SortMode = DataGridViewColumnSortMode.NotSortable;
			}
			// DBのOpenを元に戻す
			if (i_strTblName == Kc.Const.z_strLogTable || i_strTblName == Kc.Const.z_strUserAgentTable)
			{
				if (Db.MySql.ChangeDb(strDB[m_iradioBtnDB - 1]) < 0)
				{
					Dbg.Utl.MessageBoxShow(strDB[m_iradioBtnDB - 1] + "DBチェンジエラー", "DBエラー");
				}
			}
		}

		// グリッドテーブルの全行の項目[取込]を"0"クリア
		private void clrAllTorikomi()
		{
			if (z_strGridTblName == Kc.Const.z_strSClass2Table || z_strGridTblName == Kc.Const.z_strSyohin2Table)
			{
				for (int i = 0; i < m_nRow; i++)
				{
					dataGridView1.Rows[i].Cells[1].Value = "0";
				}
			}
		}

		// グリッドテーブルに小分類または商品が表示中なら["表示"]!="0"の行の[取込]を"0"クリア
		private void setTorikomi()
		{
			if (z_strGridTblName == Kc.Const.z_strSClass2Table || z_strGridTblName == Kc.Const.z_strSyohin2Table)
			{
				for (int i = 0; i < m_nRow; i++)
				{
					if ((string)dataGridView1.Rows[i].Cells[2].Value != "0")
					{
						dataGridView1.Rows[i].Cells[1].Value = "1";
					}
				}
			}
		}

		// グリッドテーブルをmySQL D/Bに書き込む
		private void WriteGridTbl()
		{
			string strTblName = z_strGridTblName;

			string[] strHdr = new string[m_nHdr];
			string[,] strData = new string[m_nRow, m_nHdr];

			// 項目名(カラム)の取得
			for (int j = 0; j < m_nHdr; j++)
			{
				strHdr[j] = (string)dataGridView1.Columns[j].HeaderCell.Value;
			}
			for (int i = 0; i < m_nRow; i++)
			{
				for (int j = 0; j < m_nHdr; j++)
				{
					strData[i, j] = (string)dataGridView1.Rows[i].Cells[j].Value;
				}
			}

			// mySQLのデータを修正
			string strCond1 = strHdr[0] + "=";
			string strCond2 = "";
			if (z_strGridTblName.Substring(0, 4) == "attr")
			{
				strCond2 = strHdr[1] + "=";
			}
			if (Db.MySql.modifyMultiCondTableItems2D(strTblName, strHdr, m_nHdr, strCond1, m_strOrigKey1,
													 strCond2, m_strOrigKey2, strData, m_nRow) != 0)
			{
				Dbg.Utl.MessageBoxShow("データ修正エラー", "DBエラー");
			}
		}
		// LOGを表示
		private int DispLog()               // 返値: 0:正常, !0:エラー
		{
			string[] strDB = { Const.z_strDb1, Const.z_strDb2, Const.z_strDbLog };
			//			string strDBLog = Const.z_strDbLog;

			// 初期化
			InitGridTbl();
			listBox1.Items.Clear();

			z_strGridTblName = strDB[2];

			if (Db.MySql.ChangeDb(strDB[2]) < 0)
			{
				Dbg.Utl.MessageBoxShow(strDB[2] + "DBオープンエラー", "DBエラー");
			}


			// 検索条件
			string strCond = "";
			if (m_bChkLogGoogle)
			{
				listBox1.Items.Add("Googleからのアクセスログを表示");
			}
			else
			{
				strCond = "(customer>='66.250.' OR customer<'66.249.')";                    // 66.249.		Google
			}

			if (m_bChkLogMy)
			{
				listBox1.Items.Add("Inlineとヤフー翻訳からのアクセスログを表示");
			}
			else
			{
				if (strCond != "")
				{
					strCond += " AND ";
				}
				strCond += "(" +
							"(customer>='192.169.' OR customer<'192.168.')" +               // 192.168.			個人
							" AND (customer>='127.1.' OR customer<'127.0.')" +              // 127.0.			個人
							" AND (customer>='124.83.191.' OR customer<'124.83.190.')" +    // 124.83.190		ヤフー翻訳
						   ")";
			}

			if (m_bChkLogOther)
			{
				listBox1.Items.Add("その他からのアクセスログを表示");
			}
			else
			{
				if (strCond != "")
				{
					strCond += " AND ";
				}
				strCond += "(" +
							"(customer>='133.237.9' OR customer<'133.237.8')" +             // 133.237.8.191	楽天
							" AND (customer>='202.72.64' OR customer<'202.72.48')" +        // 202.72.48～63	楽天
							" AND (customer>='65.55.60' OR customer<'65.55.20.')" +         // 65.55.20～59		Msnbot
							" AND (customer>='131.253.48' OR customer<'131.253.21.')" +     // 131.253.21～47	Msnbot
							" AND (customer>='157.61' OR customer<'157.54')" +              // 157.54～60		Msnbot
							" AND (customer>='199.30.32' OR customer<'199.30.16.')" +       // 199.30.16～31	Msnbot
							" AND (customer>='207.47' OR customer<'207.46')" +              // 207.46			Msnbot
							" AND (customer>='150.71' OR customer<'150.70')" +              // 150.70.17X		トレンドマイクロ
																							//							" AND (customer>='150.71' OR customer<'150.70.90')" +			// 150.70.9X		トレンドマイクロ
							" AND (customer>='39' OR customer<'38')" +                      // 38				US ???
						   ")";
			}

			if (m_bChkLogCustomer)
			{
				listBox1.Items.Add("顧客からのアクセスログを表示");
			}
			else
			{
				if (strCond != "")
				{
					strCond += " AND ";
				}
				strCond += "(" +
							"(customer<'66.250.' AND customer>='66.249.')" +                // 66.249.			Google
							" OR (customer<'192.169.' AND customer>='192.168.')" +          // 192.168.			個人
							" OR (customer<'127.1.' AND customer>='127.0.')" +              // 127.0.			個人
							" OR (customer<'124.83.191.' AND customer>='124.83.190.')" +    // 124.83.190		ヤフー翻訳
							" OR (customer<'133.237.9' AND customer>='133.237.8')" +        // 133.237.0～255 (133.237.8.191)	楽天
							" OR (customer<'65.55.60' AND customer>='65.55.20.')" +         // 65.55.20～59		Msnbot
							" OR (customer<'131.253.48' AND customer>='131.253.21.')" +     // 131.253.21～47	Msnbot
							" OR (customer<'157.61' AND customer>='157.54')" +              // 157.54～60		Msnbot
							" OR (customer<'199.30.32' AND customer>='199.30.16.')" +       // 199.30.16～31	Msnbot
							" OR (customer<'207.47' AND customer>='207.46')" +              // 207.46			Msnbot
							" OR (customer<'150.71' AND customer>='150.70')" +              // 150.70.170～179	トレンドマイクロ
																							//							" OR (customer<'150.71' AND customer>='150.70.90')" +			// 150.70.9X		トレンドマイクロ
							" OR (customer<'39' AND customer>='38')" +                      // 38				US ???
						   ")";
			}

			string strCondIn = textBoxCtrlUrlFN.Text;
			int iCn = strCondIn.IndexOf(",");
			if (strCondIn != "")
			{
				if (strCond != "")
				{
					strCond += " AND ";
				}
				if (0 <= strCondIn.IndexOf("=") || 0 <= strCondIn.IndexOf("<") || 0 <= strCondIn.IndexOf(">"))
				{
					if (strCondIn.IndexOf("'") == -1)
					{
						return -1;
					}
					strCond += "(" + strCondIn + ")";
				}
				else if (0 <= strCondIn.IndexOf("like"))
				{
					if (strCondIn.IndexOf("'") == -1)
					{
						return -1;
					}
					strCondIn = strCondIn.Replace("'", "%");
					strCondIn = strCondIn.Replace("like ", "like '");
					strCondIn = strCondIn + "'";
					strCond += "(" + strCondIn + ")";
				}
				else if (0 <= iCn)
				{
					string strCondIn1 = strCondIn.Substring(0, iCn);
					string strCondIn2 = strCondIn.Substring(iCn + 1);
					strCond += ("(customer<'" + strCondIn1 + "' AND customer>='" + strCondIn2 + "')");
				}
				else
				{
					strCond += ("(customer='" + strCondIn + "')");
				}
			}

			DateTime DateC = DateTime.Now;

			if (m_bChkLogAll)
			{
				//				listBox1.Items.Add("全日のアクセスログを表示");

				TimeSpan TimeSpan2 = new TimeSpan(100, 0, 0, 0);            // 100日
				DateTime DateB = DateC - TimeSpan2;
				string strDateB = DateB.ToString("yyyy-MM-dd hh:mm:ss");

				listBox1.Items.Add("100日前(" + strDateB + ")からのアクセスログを表示");

				if (strCond != "")
				{
					strCond += " AND ";
				}
				strCond += "(time>='" + strDateB + "')";
			}
			else
			{
				TimeSpan TimeSpan2 = new TimeSpan(2, 0, 0, 0);          // 2日
				DateTime DateB = DateC - TimeSpan2;
				string strDateB = DateB.ToString("yyyy-MM-dd hh:mm:ss");

				listBox1.Items.Add("2日前(" + strDateB + ")からのアクセスログを表示");

				if (strCond != "")
				{
					strCond += " AND ";
				}
				strCond += "(time>='" + strDateB + "')";
			}
			// ユーザ操作による行追加を無効(禁止)
			dataGridView1.AllowUserToAddRows = false;

			// DataGridViewコントロールの設定
			// 項目名の取得
			string[] strHdr = new string[z_nmaxHdr];
			string strTblName = Const.z_strLogTable;
			Db.MySql.getHeader(strTblName, strHdr, out m_nHdr);

			// 項目名(カラム)の設定
			for (int j = 0; j < m_nHdr; j++)
			{
				dataGridView1.Columns.Add("column" + j.ToString(), strHdr[j]);
			}
			// 左上隅のヘッダーセルに表示
			dataGridView1.TopLeftHeaderCell.Value = z_strGTblName;

			// mySQLから項目データの取得
			string[,] strData = new string[z_nmaxRow, m_nHdr];
			string strOrder = strHdr[0] + " DESC";
			Db.MySql.getLogTableItems(strTblName, strHdr, m_nHdr, strCond, strOrder, strData, out m_nRow);

			// 項目データの表示
			for (int i = 0; i < m_nRow; i++)
			{
				dataGridView1.Rows.Add();
				for (int j = 0; j < m_nHdr; j++)
				{
					dataGridView1.Rows[i].Cells[j].Value = strData[i, j];

				}
				//DataGridView1の行ヘッダーに行番号を表示する
				dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
				m_strOrigKey1[i] = strData[i, 0];
			}
			// 行ヘッダーの幅が自動調整されるようにする	
			dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
																					// ヘッダーとすべてのセルの内容に合わせて、列の幅を自動調整する
																					//			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
																					//			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
																					//			dataGridView1.AllowUserToResizeColumns = true;
																					//			int[] iWidth = { 30, 112, 80, 80, 130, 130, 50, 25, 300, 50, 50, 100, 100 };
			int[] iWidth = { 30, 112, 80, 80, 130, 100, 130, 25, 200, 50, 50, 115, 55, 1, 1, 1 };
			for (int iC = 0; iC < m_nHdr; iC++)
			{
				dataGridView1.Columns[iC].Width = iWidth[iC];
			}
			//自動的に並び替えられないようにする
			foreach (DataGridViewColumn c in dataGridView1.Columns)
			{
				//				c.SortMode = DataGridViewColumnSortMode.NotSortable;
			}
			// DBのOpenを元に戻す
			if (Db.MySql.ChangeDb(strDB[m_iradioBtnDB - 1]) < 0)
			{
				Dbg.Utl.MessageBoxShow(strDB[m_iradioBtnDB - 1] + "DBチェンジエラー", "DBエラー");
			}
			return 0;
		}

		// LOGレコードを削除
		private int DelLogRec()               // 返値: 0:正常, !0:エラー
		{
			string[] strDB = { Const.z_strDb1, Const.z_strDb2, Const.z_strDbLog };

			if (Db.MySql.ChangeDb(strDB[2]) < 0)
			{
				Dbg.Utl.MessageBoxShow(strDB[2] + "DBオープンエラー", "DBエラー");
			}
			string strCond = "";

			//	本日より100日前　以前のレコード
			DateTime DateC = DateTime.Now;
			TimeSpan TimeSpan2 = new TimeSpan(500, 0, 0, 0);            // 100日
			DateTime DateB = DateC - TimeSpan2;
			string strDateB = DateB.ToString("yyyy-MM-dd hh:mm:ss");

			listBox1.Items.Add("100日前(" + strDateB + ")以前のアクセスログを削除");

			strCond = "(time<'" + strDateB + "')";
			strCond = "(id<1000000)";

			Db.MySql.delRecordW(Const.z_strLogTable, strCond);

			// DBのOpenを元に戻す
			if (Db.MySql.ChangeDb(strDB[m_iradioBtnDB - 1]) < 0)
			{
				Dbg.Utl.MessageBoxShow(strDB[m_iradioBtnDB - 1] + "DBチェンジエラー", "DBエラー");
			}
			return 0;
		}
		// 選択小分類の属性名称、一般属性、URL属性を削除する
		private int DelAttr()               // 返値: 0:正常, !0:エラー
		{
			string strSClassID = Form1.m_strTextSClassID;
			if (string.IsNullOrEmpty(strSClassID))
			{
				Dbg.Utl.MessageBoxShow("小分類IDが未設定です", "btnDeleteAttr_Click");
			}
			else
			{
				//bool bRecoveryFlg = false;
				string strMsg = "小分類ID(" + strSClassID + ")の属性名称、一般属性、URL属性を削除します。\nよろしいですか？";
				DialogResult result = MessageBox.Show(strMsg,
					"質問",
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button2);
				if (result == DialogResult.OK)
				{
					// 小分類の属性名を削除
					string strCond1 = "小分類ID=" + strSClassID;
					Db.MySql.delRecordW(Const.z_strAttributeNameTable, strCond1);
					// 小分類の一般属性を削除
					string strMinSyohinID = strSClassID + "00000";
					string strMaxSyohinID = strSClassID + "99999";
					string strCond2 = "商品ID>=" + strMinSyohinID + " AND 商品ID<=" + strMaxSyohinID;
					Db.MySql.delRecordW(Const.z_strAttributeValue2Table, strCond2);
					// 小分類のURL属性を削除
					Db.MySql.delRecordW(Const.z_strAttributeURL2Table, strCond2);
				}
			}
			return 0;
		}
		// Temp 1
		//		static DateTime DateS = new DateTime();
		private void tempLearn()
		{
			/* 1
						DateTime DateC = DateTime.Now;

						string strDateC = DateC.ToString("o");					// string strDateC = DateC.ToString("yyyy-MM-dd hh:mm:ss");

						listBox1.Items.Add("現在 =" + strDateC);
						TimeSpan TimeSpan2 = new TimeSpan(2, 0, 0, 0);			// 2日
						DateTime DateB = DateC - TimeSpan2;

						string strDateB = DateB.ToString("s") + "+09:00";		// string strDateB = DateB.ToString("yyyy-MM-dd hh:mm:ss");

						listBox1.Items.Add("2日前=" + strDateB);

						// 時間差
						string strDateS = DateS.ToString("o");
						TimeSpan TimeSpan3 = DateTime.Now - DateS;
						DateS = DateTime.Now;
						string strTimeSpan3 = TimeSpan3.ToString();
						listBox1.Items.Add(strDateS + " " + strTimeSpan3);
						listBox1.Items.Add(strDateS + " " + strTimeSpan3);
			1 */
			String str1 = "abcdefgabcdefg";

			int ip1 = str1.IndexOf("ab");
			listBox1.Items.Add("ab pos1=" + ip1);

			int ip2 = str1.IndexOf("ab", ip1);
			listBox1.Items.Add("ab pos2=" + ip2);

			int ip3 = str1.IndexOf("ab", ip1 + 1);
			listBox1.Items.Add("ab pos3=" + ip3);
		}
//		// 旧テーブルの商品IDの追番と図形コードの商品ID部分の追番を1桁増やし新テーブルにコピーする
//		private void convertSyohinID()
//		{
//#if SWITCH_SCLASS
//			// 小分類マスタ
			
//			string[] strHdr = { "小分類ID", "取込", "表示", "カテゴリID", "大分類ID", "中分類ID",
//					                    "小分類名", "小分類URL", "表示小分類名", "表示小分類URL", "最大商品ID" };
//			// int nItem = 11;
         
//            int iIdn = 10;
//			int iZn = -1;
//			int nItemsMax = Kc.Const.z_nSClassMax;
//			string strTableNm1 = Kc.Const.z_strSClass1Table;
//			string strTableNm2 = Kc.Const.z_strSClass2Table;
//#endif
//#if SWITCH_ATTRVALUE
//			// 一般属性値マスタ

//			string[] strHdr = { "商品ID", "属性ID", "属性値", "表示属性値" };
//			// int nItem = 4;

//            int iIdn = 0;
//			int iZn = -1;
//            int nItemsMax = Kc.Const.z_nAttrValueMax;
//			string strTableNm1 = Kc.Const.z_strAttributeValue1Table;
//			string strTableNm2 = Kc.Const.z_strAttributeValue2Table;
//#endif
//#if SWITCH_ATTRURL
//			// URL型属性マスタ

//			string[] strHdr = { "商品ID", "属性ID", "URL", "図形CD" };
//			// int nItem = 4;

//			int iIdn = 0;
//			int iZn = 3;
//			int nItemsMax = Kc.Const.z_nAttrURLMax;
//			string strTableNm1 = Kc.Const.z_strAttributeURL1Table;
//			string strTableNm2 = Kc.Const.z_strAttributeURL2Table;
//#endif
//#if SWITCH_SYOHIN
//			// 商品マスタ
//			string[] strHdr = { "商品ID", "取込", "表示", "小分類ID", "商品名", "表示商品名", "安値", "評判", "評価", "発売時期" };
//			// int nItem = 10;

//            int iIdn = 0;
//			int iZn = -1;
//			int nItemsMax = Kc.Const.z_nSyohinMax;
//			string strTableNm1 = Kc.Const.z_strSyohin1Table;
//			string strTableNm2 = Kc.Const.z_strSyohin2Table;
//#endif

//			// テーブルの全レコードを取得する
//			int nItems = strHdr.Length;
//			string[,] strItemss = new string[nItemsMax, nItems]; // 全レコードの項目のリスト
//			int nRec;                                                           // レコード数
//			Db.MySql.getTableItems(strTableNm1, strHdr, nItems, "", "", strItemss, out nRec);

//			// 全レコードの商品IDと図形コードを修正して新しいテーブルに追加する
//			int nRecM = 100;                                                    // 追加レコード一まとめ数
//			string[,] strItems = new string[nRec, nItems];
//			for (int irs = 0; irs < nRec; irs += nRecM)
//			{
//				int irc = nRec - irs;
//				if (irc > nRecM) irc = nRecM;
//				int ics = 0;
//				for (int ic = 0; ic < irc; ic++)
//				{
//					int ir = irs + ic;

//					//if (strItemss[ir, 0] != "12010046")
//					//{
//					//	continue;
//					//}

//					for (int it = 0; it < nItems; it++)
//					{
//						strItems[ics, it] = strItemss[ir, it];
//					}
//					int len = strItems[ics, iIdn].Length;
//					strItems[ics, iIdn] = strItems[ics, iIdn].Insert(len - 4, "0");
//					if (iZn >= 0)
//					{
//						len = strItems[ics, iZn].Length;
//						if (len >= 15)
//							strItems[ics, iZn] = strItems[ics, iZn].Insert(len - 9, "0");
//					}
//					ics++;
//				}
//				if (ics > 0)
//				{
//					Db.MySql.insMultiRecord(strTableNm2, strItems, ics, nItems);
//				}
//			}
//		}

//		// 制御テーブルの図形コードの商品ID部分の追番を1桁増やす
//		private void convertZukeiCode()
//		{
//			// 制御マスター
//			string[] strHdrCtrl = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
//			string[,] strItemCtrls = new string[Kc.Const.z_nCtrlIDMax, strHdrCtrl.Length];  // 制御マスタの制御IDのリスト
//			int nItem1;
//			// 制御マスタのレコードを取得する
//			string strCond0 = "制御TP>=1 AND 制御TP<=3";
//			string strOrder = "制御TP,制御ID";
//			// コントロールテーブルの読み込み
//			Db.MySql.getTableItems(Kc.Const.z_strControlTable, strHdrCtrl, strHdrCtrl.Length,
//							   strCond0, strOrder, strItemCtrls, out nItem1);

//			// temporary coding
//			string strCond1 = "制御TP=";
//			string strCond2 = "制御ID=";
//			for (int iT = 0; iT < nItem1; iT++)
//			{
//				int len = strItemCtrls[iT, 3].Length;
//				if(len == 10)
//				{
//					strItemCtrls[iT, 3] = strItemCtrls[iT, 3].Insert(len - 4, "0");

//					// mySQLのデータを修正

//					Db.MySql.modifyMultiCondRecordItems2D(Kc.Const.z_strControlTable, strHdrCtrl, strHdrCtrl.Length,
//																strCond1, strItemCtrls[iT, 0],
//																strCond2, strItemCtrls[iT, 1], strItemCtrls, iT);
//				}
//			}
//			// temporary coding

//		}

//		// 旧テーブルの商品IDの追番と図形コードの商品ID部分の追番を1桁増やし新テーブルと比較する
//		private void diffSyohinID()
//		{
//#if SWITCH_SCLASS
//			// 小分類マスタ
//			string[] strHdr = { "小分類ID", "取込", "表示", "カテゴリID", "大分類ID", "中分類ID",
//					                    "小分類名", "小分類URL", "表示小分類名", "表示小分類URL", "最大商品ID" };
//			// int nItem = 11;
         
//			int iIdn = 10;
//			int iZn = -1;
//			int nItemsMax = Kc.Const.z_nSClassMax;
//			string strTableNm1 = Kc.Const.z_strSClass1Table;
//			string strTableNm2 = Kc.Const.z_strSClass2Table;
//#endif
//#if SWITCH_ATTRVALUE
//			// 一般属性値マスタ
//			string[] strHdr = { "商品ID", "属性ID", "属性値", "表示属性値" };
//			// int nItem = 4;

//			int iIdn = 0;
//			int iZn = -1;
//			int nItemsMax = Kc.Const.z_nAttrValueMax;
//			string strTableNm1 = Kc.Const.z_strAttributeValue1Table;
//			string strTableNm2 = Kc.Const.z_strAttributeValue2Table;
//#endif
//#if SWITCH_ATTRURL
//			// URL型属性マスタ
//			string[] strHdr = { "商品ID", "属性ID", "URL", "図形CD" };
//			// int nItem = 4;

//			int iIdn = 0;
//			int iZn = 3;
//			int nItemsMax = Kc.Const.z_nAttrURLMax;
//			string strTableNm1 = Kc.Const.z_strAttributeURL1Table;
//			string strTableNm2 = Kc.Const.z_strAttributeURL2Table;
//#endif
//#if SWITCH_SYOHIN
//			// 商品マスタ
//			string[] strHdr = { "商品ID", "取込", "表示", "小分類ID", "商品名", "表示商品名", "安値", "評判", "評価", "発売時期" };
//			// int nItem = 10;

//			int iIdn = 0;
//			int iZn = -1;
//			int nItemsMax = Kc.Const.z_nSyohinMax;
//			string strTableNm1 = Kc.Const.z_strSyohin1Table;
//			string strTableNm2 = Kc.Const.z_strSyohin2Table;
//#endif

//			int nItems = strHdr.Length;

//			// 旧テーブルの全レコードを取得する
//			string[,] strItemss1 = new string[nItemsMax, nItems]; // 全レコードの項目のリスト
//			int nRec1;                                                           // レコード数
//			Db.MySql.getTableItems(strTableNm1, strHdr, nItems, "", "", strItemss1, out nRec1);

//			// 新規テーブルの全レコードを取得する
//			string[,] strItemss2 = new string[nItemsMax, nItems]; // 全レコードの項目のリスト
//			int nRec2;                                                           // レコード数
//			Db.MySql.getTableItems(strTableNm2, strHdr, nItems, "", "", strItemss2, out nRec2);

//			// 旧テーブルの全レコードの商品IDを修正して新規テーブルsと比較する
//			string[] strItems = new string[nItems];
//			for (int ir = 0; ir < nRec1; ir++)
//			{
//				for (int it = 0; it < nItems; it++)
//				{
//					strItems[it] = strItemss1[ir, it];
//				}
//				int len = strItems[iIdn].Length;
//				strItems[iIdn] = strItems[iIdn].Insert(len - 4, "0");
//				if (iZn >= 0)
//				{
//					len = strItems[iZn].Length;
//					if (len >= 15)
//						strItems[iZn] = strItems[iZn].Insert(len - 9, "0");
//				}

//				for (int it = 0; it < nItems; it++)
//				{
//					if (strItems[it] != strItemss2[ir, it])
//						Dbg.Utl.MessageBoxShow(ir + " " + strItemss1[ir, 0] + " " + strItemss1[ir, 1] + " " + strItemss1[ir, 2] + " " + strItemss1[ir, 3] + " !=\r\n" +
//														  strItemss2[ir, 0] + " " + strItemss2[ir, 1] + " " + strItemss2[ir, 2] + " " + strItemss2[ir, 3], "DB比較エラー");
//					;
//				}
//			}
//		}

//		// 制御テーブルの図形コードの商品ID部分の追番を1桁増やす
//		private void convertZukeiFileName()
//		{
//			// フォルダのファイルをすべて取得する
//			//string[] files = System.IO.Directory.GetFiles(
//			//	@"N:\xampp\htdocs\mf2\z\kaden", "z*", System.IO.SearchOption.AllDirectories);
//			string[] files = System.IO.Directory.GetFiles(
//			//	@"N:\xampp\htdocs\mf2\z\pc", "Z*");
//			//	@"N:\xampp\htdocs\mf2\z\kaden", "Z*");
//			//	@"N:\xampp\htdocs\mf2\z\camera", "Z*");
//				@"N:\xampp\htdocs\mf2\z\sumafo", "Z*");

//			int nFiles = files.Length;
//			string[] filets = new string[nFiles];

//			string[] dispfiles = new string[15];
//			string[] dispfilesch = new string[15];
//			int icd = 0;
//			for (int ic=0; ic<nFiles; ic++)
//			{
//				int nch = files[icd].Length - 25;
//				dispfiles[icd] = (ic + 1) + " " + nch +" " + files[ic];
//				dispfilesch[icd] = "";
//				for (int jc=0; jc<25; jc++)
//				{
//					dispfilesch[icd] += (files[ic].Substring(nch + jc, 1) + " ");
//				}
//				if (nFiles >=10 && icd == 9)
//				{
//					ic++;
//					icd++;
//					dispfiles[icd] = "";
//					dispfilesch[icd] = "";
//					ic = nFiles - 5;
//				}
//				icd++;
//			}
//			//listBox1.Items.AddRange(files);
//			for (int ic = 0; ic<nFiles; ic++)
//			{
//				int nch = files[ic].Length;
//				string strChk = files[ic].Substring(nch - 20, 2);
//				if (strChk == "\\z" || strChk == "\\Z")
//				{
//					filets[ic] = files[ic].Insert(nch - 13, "0");
//					System.IO.File.Move(files[ic], filets[ic]);
//				}
//			}
//		}

		// 最大商品IDの誤りを修正する
		private void correctMaxSyohinID()
		{
			// 小分類マスタ

			string[] strHdr = { "小分類ID", "取込", "表示", "小分類名", "最大商品ID" };
			string[,] strItems = new string[Kc.Const.z_nSClassMax, 5];      // 全小分類の[小分類ID],[取込],[小分類名]のリスト
			int nSClass;
			Db.MySql.getTableItems(Kc.Const.z_strSClass2Table, strHdr, 5, "", "", strItems, out nSClass);

			for (int ic=0; ic<nSClass; ic++)
			{
				string strSClassID = strItems[ic, 0];
				string strSClassGetFlg = strItems[ic, 1];                   // [取込]
				if (strSClassGetFlg != "1") continue;                       // 取込対象外はスキップ
																			/*
																					public static string m_strCurSClassName = "";           // 小分類名
																					public static string m_strCurSClassID = "";             // 小分類ID
																					public static string m_strCurSyohinName = "";           // 商品名
																					public static string m_strCurSyohinID = "";             // 商品ID
																					public static int m_iCurPageN = 0;                      // 小分類ページ追番
																					public static int m_iCurSyohinN = 0;                    // 商品追番
																			*/
				Mkdb.m_strCurSClassID = strItems[ic, 0];                    // 処理経過表示用 小分類ID
				Mkdb.m_strCurSClassName = strItems[ic, 2];                  // 処理経過表示用 小分類名

				uint iMaxSyohinID;                        // 最大商品ID
				if (!UInt32.TryParse(strItems[ic, 4], out iMaxSyohinID))
					Dbg.Utl.MessageBoxShow(strItems[ic, 4], "最大商品ID UInt");

				// [取込]が1である小分類の最大商品IDを調べる
				// D/Bから小分類の商品IDを１件取得する
				string[] strSyohinIDs = new string[Kc.Const.z_nSyohinMax];
				int nSyohinID;
				Mkdb.getSyohinIDofSClass(strSClassID, strSyohinIDs, Kc.Const.z_nSyohinMax, out nSyohinID);

				uint iLastSyohinID;
				if (!UInt32.TryParse(strSyohinIDs[nSyohinID - 1], out iLastSyohinID))
					Dbg.Utl.MessageBoxShow(strSyohinIDs[nSyohinID - 1], "最終商品ID UInt");

				if (iLastSyohinID > iMaxSyohinID)
				{
					// 最大商品IDの修正
					strItems[ic, 4] = strSyohinIDs[nSyohinID - 1];
					Db.MySql.modifySingleCondRecordItems2D(Kc.Const.z_strSClass2Table, strHdr, 5,
														   "小分類ID=" + strSClassID, strItems, ic);
				}
			}
		}

        // 旧テーブルの商品IDの追番と図形コードの商品ID部分の追番を1桁増やし新テーブルにコピーする
        private void clearSyohinDispFlg1()
        {
			// 検索条件
			string strSClassID = "2325";
			string strDispOffSelYear = "2017";
			Syohin.clearSyohinDispFlg(strSClassID, strDispOffSelYear);
        }

        // 不使用関数
        private void NU_deleteAttrName()
		{
			// 全小分類の小分類IDと取込を取得する
			string[] strHdr = { "小分類ID", "取込", "小分類名" };
			string[,] strItems = new string[Kc.Const.z_nSClassMax, 3];      // 全小分類の[小分類ID],[取込],[小分類名]のリスト
			int nSClass;                                                    // 小分類数
			Db.MySql.getTableItems(Kc.Const.z_strSClass2Table, strHdr, 3, "", "", strItems, out nSClass);

			for (int ic = 0; ic < nSClass; ic++)
			{
				string strSClassID = strItems[ic, 0];
				string strSClassGetFlg = strItems[ic, 1];                   // [取込]
				if (strSClassGetFlg != "1") continue;                       // 取込対象外はスキップ

				Mkdb.m_strCurSClassID = strItems[ic, 0];                    // 処理経過表示用 小分類ID
				Mkdb.m_strCurSClassName = strItems[ic, 2];                  // 処理経過表示用 小分類名

				// [取込]が1である小分類の全商品の不必要な属性名の属性リストをD/Bから削除する
				// D/Bから小分類の商品を１件取得する
				string[] strSyohinIDs = new string[1];
				int nSyohinID;
				Mkdb.getSyohinIDofSClass(strSClassID, strSyohinIDs, 1, out nSyohinID);
				if (nSyohinID != 0)
				{
					// 小分類に商品あり
					if (Form1.m_iradioBtnHenkoMode == 2)   // 変更モード: 追加(2)
					{
						// 小分類の全商品の不要属性を削除
						Syohin.NU_deleteSClassNGAttr(strSClassID);
					}
				}
			}
		}
	}
}