using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kc
{
	public partial class Syohin
	{
		// SQLDBオープン
		public static int OpenMySqlDB(			// 返値: 0:正常, !0:エラー
			int i_iDBn							// DB番号　(1:カテゴリ名リスト, 2:小分類リスト, 3:商品の属性リスト)
			)
		{
			int ist = 0;
			if (i_iDBn == 1)
			{
				// カテゴリマスタ
				string[] strHdr0 = { "カテゴリID", "表示", "名称", "カテゴリURL", "内容", "表示名称", "表示カテゴリURL", "表示内容",
									 "最大大分類ID", "最大中分類ID", "最大小分類ID" };
				string[] strType0 = { "int", "tinyint", "varchar(50)", "varchar(50)", "text", "varchar(50)", "varchar(50)", "text",
									  "int", "int", "int" };
				Db.MySql.CreateTable(Const.z_strCategoryTable, strHdr0, strType0, "PRIMARY KEY (カテゴリID)", 11);
			} else
			if (i_iDBn == 2)
			{
				// 大分類マスタ
				string[] strHdr1 = { "大分類ID", "表示", "カテゴリID", "大分類名", "表示大分類名", "内容" };
				string[] strType1 = { "int", "tinyint", "int", "varchar(50)", "varchar(50)", "text" };
				Db.MySql.CreateTable(Const.z_strLClassTable, strHdr1, strType1, "PRIMARY KEY (大分類ID)", 6);
				// 中分類マスタ
				string[] strHdr2 = { "中分類ID", "表示", "カテゴリID", "大分類ID", "中分類名", "表示中分類名" };
				string[] strType2 = { "int", "tinyint", "int", "int", "varchar(50)", "varchar(50)" };
				Db.MySql.CreateTable(Const.z_strMClassTable, strHdr2, strType2, "PRIMARY KEY (中分類ID)", 6);
				// 小分類マスタ
				string[] strHdr3 = { "小分類ID", "取込", "表示", "カテゴリID", "大分類ID", "中分類ID",
                                     "小分類名", "小分類URL", "表示小分類名", "表示小分類URL", "最大商品ID" };
                string[] strType3 = { "int", "tinyint", "int", "int", "int",
									  "varchar(50)", "varchar(50)", "varchar(50)", "varchar(50)", "int unsigned" };
				Db.MySql.CreateTable(Const.z_strSClass2Table, strHdr3, strType3, "PRIMARY KEY (小分類ID), UNIQUE (小分類名)", 11);
				// 中小分類関係マスタ
				string[] strHdrA = { "中分類ID", "小分類ID" };
				string[] strTypeA = { "int", "int" };
				Db.MySql.CreateTable(Const.z_strMSCRelation, strHdrA, strTypeA, "INDEX (中分類ID), INDEX (小分類ID)", 2);
			} else
			if (i_iDBn == 3)
			{
				// 属性名マスタ
				string[] strHdr4 = { "小分類ID", "属性ID", "表示", "属性名", "表示属性名", "表示巾" };
				string[] strType4 = { "int", "int", "tinyint", "varchar(127)", "varchar(127)", "int" };
				Db.MySql.CreateTable(Const.z_strAttributeNameTable, strHdr4, strType4, "UNIQUE mk (小分類ID,属性ID)", 6);
				// 一般属性値マスタ
				string[] strHdr5 = { "商品ID", "属性ID", "属性値", "表示属性値" };
				string[] strType5 = { "int unsigned", "int", "varchar(255)", "varchar(255)" };
				Db.MySql.CreateTable(Const.z_strAttributeValue2Table, strHdr5, strType5, "UNIQUE mk (商品ID,属性ID)", 4);
				// URL型属性マスタ
				string[] strHdr6 = { "商品ID", "属性ID", "URL", "図形CD" };
				string[] strType6 = { "int unsigned", "int", "text", "varchar(20)" };
				Db.MySql.CreateTable(Const.z_strAttributeURL2Table, strHdr6, strType6, "UNIQUE mk (商品ID,属性ID)", 4);
				// 商品マスタ
				string[] strHdr7 = { "商品ID", "取込", "表示", "小分類ID", "商品名", "表示商品名" };
				string[] strType7 = { "int unsigned", "tinyint", "tinyint", "int", "varchar(255)", "varchar(255)" };
				Db.MySql.CreateTable(Const.z_strSyohin2Table, strHdr7, strType7, "PRIMARY KEY (商品ID),UNIQUE (商品名)", 6);
				// 制御マスタ
				string[] strHdr8 = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
				string[] strType8 = { "int", "tinyint", "text", "text", "text", "text" };
				Db.MySql.CreateTable(Const.z_strControlTable, strHdr8, strType8, "UNIQUE mk (制御TP,制御ID)", 6);
			}
			return ist;
		}

		// LOGオープン
		public static int OpenLogFile()				// 返値: 0:正常, !0:エラー
		{
			int ist = 0;
			DateTime dt = DateTime.Now;
			string strDt = dt.ToString("yyyyMMdd");
			Mkdb.OpenLogFile(Const.z_pathMatukenDB + Const.z_pathLogFile + strDt + ".txt", true);
			Mkdb.WriteLogFile("Start Log " + DateTime.Now);
			return ist;
		}

		// LOGクローズ
		public static void CloseLogFile()
		{
			Mkdb.WriteLogFile("Close Log " + DateTime.Now);
			Mkdb.CloseLogFile();
		}

		// Dump File オープン
		public static int OpenDumpFile(			// 返値: 0:正常, !0:エラー
			string i_strFilePath				// Dumpファイルパス
			)
		{
			int ist = 0;
			Mkdb.OpenDumpFile(i_strFilePath, false);
			return ist;
		}

		// Dump File クローズ
		public static void CloseDumpFile()
		{
			Mkdb.CloseDumpFile();
		}

		// 相対URLを絶対URLに変換する
		public static string absURL(string i_strURL)
		{
			string strAbsURL = i_strURL;
			if (i_strURL[0] == '/')
			{
				strAbsURL = Const.z_urlKakakuHome + i_strURL;
			}
			return strAbsURL;
		}

		// 絶対URLを相対URLに変換する
		public static string relURL(string i_strURL)
		{
			string strRelURL = i_strURL;
			if (strRelURL.StartsWith(Const.z_urlKakakuHome))
			{
				strRelURL = i_strURL.Substring(Const.z_nKakakuHomeUrl);
			}
			return strRelURL;
		}

		// カテゴリリストをD/Bに書き込む
		public static void importCategoryList()
		{
			const int nmaxCATEGORY = 100;
			string[] strCategoryNameBf = new string[nmaxCATEGORY];				// カテゴリ名リスト
			string[] strCategoryPageURLBf = new string[nmaxCATEGORY];			// カテゴリURLリスト
			string[] strCategoryDescriptBf = new string[nmaxCATEGORY];			// カテゴリ説明
			int nCategory;														// カテゴリ数

			Mkdb.DispLine2("============== カテゴリマスタ 1 ============== ");

			// URLのホームページをテーブルに読み込み
			string strHomePageURL = Const.z_urlKakakuDataHomePage;			// ホームページのURL
			HTML.Table.loadTable(absURL(strHomePageURL));
			// カテゴリの名称とカテゴリ表示URLを取得
			HTML.Table.getCategoryPage(strCategoryNameBf, strCategoryPageURLBf,
									   strCategoryDescriptBf, out nCategory);

			// カテゴリの名称とカテゴリ表示URLのリストをMySqlマスタに書き込む
			Mkdb.createCategoryListMst(strCategoryNameBf, strCategoryPageURLBf,
									strCategoryDescriptBf, nCategory);
		}
		
		// 全カテゴリの小分類リストをD/Bに書き込む
        public static void importAllSClassList()
        {
			Mkdb.DispLine2("============== 全カテゴリの小分類リスト 1 ============== ");

            // 全カテゴリの小分類リストをD/Bに書き込む
			const int nmaxSCLASS = 1000;
			string[] strLClassNameBf = new string[nmaxSCLASS];					// 大分類リスト
			string[] strMClassNameBf = new string[nmaxSCLASS];					// 中分類リスト
			string[] strSClassNameBf = new string[nmaxSCLASS];					// 小分類リスト
			string[] strSClassURLBf = new string[nmaxSCLASS];					// 小分類URLリスト
			int nSClass;														// 小分類数

			// カテゴリ名とカテゴリページのURLを取得
			string[] strHdr = { "名称", "カテゴリURL" };
			string[,] strItems = new string[50,2];								// 名称と小分類URLのリスト
			int nCategory;														// カテゴリ数
			Db.MySql.getTableItems(Const.z_strCategoryTable, strHdr, 2, "", "", strItems, out nCategory);

            for (int ic = 0; ic < nCategory; ic++)
            {
				string strCategoryName = strItems[ic,0];
				string strCategoryPageURL = absURL(strItems[ic,1]);

				int idCategory = ic + 1;

				// URLのホームページをテーブルに読み込み
				HTML.Table.loadTable(strCategoryPageURL);
				// 大中小の分類名リストと小分類表示URLリストを取得
				HTML.Table.getSClassListOfCategory(strLClassNameBf, strMClassNameBf,
										strSClassNameBf, strSClassURLBf, out nSClass);
				// 大中小分類リストをMySqlマスタに書き込む (カテゴリ名,大中小分類名のリスト)
				Mkdb.CreateSClassListMst(idCategory, strCategoryName, strLClassNameBf, strMClassNameBf,
										strSClassNameBf, strSClassURLBf, nSClass);
			}
        }

		// 指定カテゴリに含まれる小分類リストをD/Bに書き込む
        public static void importSClassListOfCategory(
			int i_idCategory					// カテゴリID
            )
        {
			const int nmaxSCLASS = 1000;
			string[] strLClassNameBf = new string[nmaxSCLASS];					// 大分類名リスト
			string[] strMClassNameBf = new string[nmaxSCLASS];					// 中分類名リスト
			string[] strSClassNameBf = new string[nmaxSCLASS];					// 小分類名リスト
			string[] strSClassURLBf = new string[nmaxSCLASS];					// 小分類URLリスト
			int nSClass;														// 小分類数

			// 指定カテゴリのカテゴリ名とカテゴリページのURLを取得
			string[] strHdr = { "名称", "小分類URL" };
			string[] strItems = new string[2];
			string strCond = "ID=" + i_idCategory.ToString();
			Db.MySql.getRecordItems(Const.z_strCategoryTable, strHdr, 2, strCond, strItems);
			string strCategoryName = strItems[0];
			string strCategoryPageURL = absURL(strItems[1]);

			// URLのカテゴリページをテーブルに読み込み
            HTML.Table.loadTable(absURL(strCategoryPageURL));

            // 大中小の分類名リストと小分類表示URLリストを取得
            HTML.Table.getSClassListOfCategory(strLClassNameBf, strMClassNameBf,
									strSClassNameBf, strSClassURLBf, out nSClass);
        }

		// 小分類の全商品の属性リストを取得しD/Bに書き込む
		public static int importSClassProducts(
			string i_strSClassID				// 小分類ID
			)
		{
			int irt = 0;
			int ist = 0;
			const int nmaxPAGE = 1000;
			// URLのホームページをテーブルに読み込み
			int iszAttrValueBf = Const.z_nSClassAttrNameMax *
								 Const.z_nPageSyohinMax;						// 小分類の属性値バッファサイズ
			string[] strAttrNameBf = new string[iszAttrValueBf];				// 小分類ページ商品の属性名
			string[] strAttrValueBf = new string[iszAttrValueBf];				// 小分類ページ商品の属性値
			int[] iflgAttrValueBf = new int[iszAttrValueBf]; 					// 小分類ページ商品の属性先頭フラグ	1:商品属性の先頭 2:補助属性の先頭 3:補助属性サマリ(debug用)
			int nAttrBf;														// 小分類ページ商品の属性数
			int iszAttrValues = Const.z_nSyohinAttrNameMax;					// 商品の属性値リストサイズ
			string[] strAttrNames = new string[iszAttrValues];					// 商品の属性名リスト
			string[] strAttrValues = new string[iszAttrValues];					// 商品の属性値リスト
			int nAttrS;															// 商品の属性数
			string[] strPhotoURLBf = new string[iszAttrValues];					// 一覧表示用商品写真URL
			string[] strSyohinURLBf = new string[iszAttrValues];				// 商品説明URL
			int nSyohin;														// 商品数
			string strNextPageURL;												// 次ページURL

			string strMakerTopPageURL;											// メーカーTopPageURL
			string strSeihinInfoURL;											// メーカー製品情報URL
			string strSeihinSiyoURL;											// メーカー仕様表URL
			string strPreReleaseURL;											// プレリリースURL

			int iszProductPicts = Const.z_nSyohinZukeiMax;					// 商品の図形URL最大数
			string[] strProductPictURLs = new string[iszProductPicts];			// 製品写真URL(主図形,サブ1図形,サブ2図形,･･･)
			string[] strLSizeProductPictHPURLs = new string[iszProductPicts];	// フルスケール製品写真掲載ホームページのURL
			int nProductPictURL;												// 製品写真数

			// 属性名検索用の初期設定
			// 小分類内の属性IDと属性名を検索用テーブルに設定する
			DbAttr.setAttrNameSearchTable(i_strSClassID);

			// 小分類IDから小分類URLを取得
			string[] strHdr = { "小分類URL" };
			string[] strItems = new string[1];
			string strCond = "小分類ID=" + i_strSClassID;
			if (!Db.MySql.getRecordItems(Const.z_strSClass2Table, strHdr, 1, strCond, strItems))
			{
				Dbg.Utl.MessageBoxShow("小分類URL取得エラー", "importSClassProducts");
				return -1;
			}
			string strSClassPageURL = absURL(strItems[0]) + Const.z_urlKakakuAllSyohin;

			int nCs = nmaxPAGE;
			for (int ic = 1; ic <= System.Math.Min(40,nCs); ic++, strSClassPageURL = strNextPageURL)
			{
				Mkdb.m_iCurPageN = ic;											// 処理経過表示用 小分類ページ追番
				Mkdb.DispLine2("============== " + ic.ToString() + " ============== ");
				// 待ち(100ms)
				System.Threading.Thread.Sleep(100);
				// URLの小分類ページをテーブルに読み込み
				HTML.Table.loadTable(strSClassPageURL);

				// 小分類ページデータのテーブルから各商品の属性を商品の属性BFに取り込む
				ist = HTML.Table.getSyohinAttrOfSClassPage(strAttrNameBf, strAttrValueBf, iflgAttrValueBf, out nAttrBf,
													 strPhotoURLBf, strSyohinURLBf, out nSyohin,
													 out strNextPageURL);
				if ( ist < 0)
				{
					string strErr = "	小分類ID=" + i_strSClassID + ", URL=" + strSClassPageURL + ", No." + ic;
					Mkdb.WriteLogFile(strErr);
					irt = ist;
					break;
				}

				int nCz = nSyohin;
				for (int iSyohinN = 1; iSyohinN <= nCz; iSyohinN++)
				{
					Mkdb.m_iCurSyohinN = iSyohinN;                              // 処理経過表示用 商品追番
					string strIcs0 = ic.ToString() + "," + iSyohinN.ToString();

					// 小分類の商品の属性BFから商品追番目の商品の属性を取得
					getSyohinAttr(iSyohinN, strAttrNameBf, strAttrValueBf, iflgAttrValueBf, nAttrBf,
								  strAttrNames, strAttrValues, out nAttrS);
					string strSyohinName = strAttrValues[1];
					if (Dbg.Utl.CheckError(strAttrNames[1] != "商品名" || String.IsNullOrEmpty(strSyohinName),
										   "商品名なし", "importSClassProducts"))
						continue;

					// 商品名が対象外の商品は無視する
					if (!chkDispOffSyohin(strAttrNames, strAttrValues, nAttrS))
					{
						continue;
					}

					// 属性値に不要な文字列を含んでいるか調べログ出力する
					logNgWordInAttr(strAttrValues, nAttrS);

					Mkdb.m_strCurSyohinName = strSyohinName;                    // 処理経過表示用 商品名
					Mkdb.DispLine2("============== " + strIcs0 + " : " + strAttrValues[1] + " ============== ");

					// 商品名から商品IDを取得
					string strSyohinID;
					int iTourokusumi;                                   // 商品有無				(1:登録済商品)
					int iTorikomi;                                      // 商品の取込みフラグ	(1:取込み)
					int iSyoriMode = Form1.m_iradioBtnHenkoMode;        // 処理モード			(1:スキップ, 2:新規追加, 3:削除追加)
					bool bAddSyohin = false;                            // 商品追加フラグ
					bool bModifyValiableAttr = false;                   // 可変項目変更フラグ

					iTourokusumi = Mkdb.getSyohinIDfromSyohinName(strAttrValues[1], i_strSClassID, out strSyohinID, out iTorikomi);
					Mkdb.m_strCurSyohinID = strSyohinID;						// 処理経過表示用 商品ID

					// 小分類単位での取込み
					if (iTourokusumi == 1)
					{
						// 既に商品あり
						if (iTorikomi == 0)
						{
							// 商品の[取込] = 0
							// 変化している一般属性と一覧表示のURL属性を取込む、子HPのURL属性は取り込まない
							bModifyValiableAttr = true;
						}
						else
						{
							// 商品の[取込]=1の場合は修正(削除後追加)する
							Mkdb.deleteSyohinAttr(strSyohinID);	// 商品の属性を削除
						}
					}
					else
					{
						// 商品追加
						bAddSyohin = true;
					}

					if (bModifyValiableAttr)
					{
						Mkdb.ReplaceSyohinAttrListMst(i_strSClassID, strSyohinID,
													 strAttrNames, strAttrValues,
													 nAttrS, strPhotoURLBf[iSyohinN - 1]);
						continue;
					}

					if (bAddSyohin)
					{
						// 未登録商品(新規追加商品)

						// {商品}を商品マスタに書き込む
						//string[] strHdr7 = { "商品ID", "取込", "表示", "小分類ID", "商品名", "表示商品名", "安値", "評判", "評価", "発売時期" };
						//string[] strType7 = { "int", "tinyint", "tinyint", "int", "varchar(255)", "varchar(255)", "int", "int", "float", "varchar(8)" };
						string[] strSnItems = { strSyohinID, "0", "1", i_strSClassID, strSyohinName, strSyohinName, "0", "0", "0", "00/00/00" };
						Db.MySql.insRecord(Const.z_strSyohin2Table, strSnItems, strSnItems.Length);
					}
					// 商品属性リストをMySqlマスタに書き込む
					Mkdb.CreateSyohinAttrListMst(strSyohinID, strAttrNames, strAttrValues, nAttrS,
										         strPhotoURLBf[iSyohinN - 1]);

					if (Form1.m_bChkURL)						// URL
					{
						// URLの商品ページをテーブルに読み込み
						HTML.Table.loadTable(absURL(strSyohinURLBf[iSyohinN - 1]));
						// 商品のページから製品の仕様URLを取得
						HTML.Table.getSiyoURL(out strMakerTopPageURL, out strSeihinInfoURL,
											  out strSeihinSiyoURL, out strPreReleaseURL,
											  strProductPictURLs, strLSizeProductPictHPURLs,
											  out nProductPictURL);
						// 仕様URL、図形URLリストをMySqlマスタに書き込む
						Mkdb.CreateSyohinURLListMst(strSyohinID, strMakerTopPageURL,
													strSeihinInfoURL, strSeihinSiyoURL, strPreReleaseURL,
													strProductPictURLs, strLSizeProductPictHPURLs, nProductPictURL);
					}
				}
				if (String.IsNullOrEmpty(strNextPageURL))
					break;
			}
			return irt;
		}

		// 小分類の全商品の不要な属性名の属性リストを削除する
		public static int NU_deleteSClassNGAttr(
			string i_strSClassID                // 小分類ID
			)
		{
			int irt = 0;
			int ist = 0;

			// 属性名検索用の初期設定
			// 小分類内の属性IDと属性名を検索用テーブルに設定する
			DbAttr.setAttrNameSearchTable(i_strSClassID);
			//先頭の不要な属性名のIDを得る
			int id;
			int ic1, ic2;
			string strAttrID = "";
			for (id=0; id< DbAttr.m_nAttrID; id++)
			{
				ic1 = DbAttr.m_strAttrNames[id].IndexOf("\r\n");
				ic2 = DbAttr.m_strAttrNames[id].IndexOf(" ");
				if (ic1 >= 0)
				{
					strAttrID = DbAttr.m_idAttrNames[id].ToString();
					break;
				}
			}
			if (id < DbAttr.m_nAttrID)
			{
				// 小分類の商品IDを取得 
				int iszSyohinID = Const.z_nSClassSyohinMax;
				string[] strSyohinIDs = new string[iszSyohinID];
				int nSyohinID;
				Mkdb.getSyohinIDofSClass(i_strSClassID, strSyohinIDs, iszSyohinID, out nSyohinID);
				string strCondW;
				for (int isy=0; isy<nSyohinID; isy++)
				{
					strCondW = "商品ID=" + strSyohinIDs[isy] + " AND 属性ID>=" + strAttrID;
					ist = Db.MySql.delRecordW(Const.z_strAttributeValue2Table, strCondW);
				}

				// classの不要属性名を削除
				strCondW = "小分類ID=" + i_strSClassID + " AND 属性ID>=" + strAttrID;
				ist = Db.MySql.delRecordW(Const.z_strAttributeNameTable, strCondW);
			}
			irt = ist;
			return irt;
		}

		// 小分類ページの商品の属性BFから商品の属性を取得
		public static void  getSyohinAttr(
			int i_iSyohinN,						// 小分類ページ内の商品連番(1～n)
			string[] i_strAttrNameBf,			// 属性名バッファ
			string[] i_strAttrValueBf,			// 属性値バッファ
			int[] i_iflgAttrValueBf,			// 属性タイプフラグ (1:商品の先頭属性)
			int i_nAttr,						// 属性数
			string[] o_strAttrNames,			// 属性名リスト
			string[] o_strAttrValues,			// 属性値リスト
			out int o_nAttr						// 属性数
			)
		{
			Array.Clear(o_strAttrNames, 0, o_strAttrNames.Length);
			Array.Clear(o_strAttrValues, 0, o_strAttrValues.Length);
			o_nAttr = 0;
			int isn = 0;
			for (int iz = 0; iz < i_nAttr; iz++)
			{
				if (i_iflgAttrValueBf[iz] == 1)
				{
					isn++;
				}
				if (isn != i_iSyohinN)
					continue;

				// 小分類内の(i_iSyohinN)番目の商品
				if (i_iflgAttrValueBf[iz] == 3)
					break;						// 属性終了
				if (i_strAttrNameBf[iz] == null)
					break;                      // 属性終了

				o_strAttrNames[o_nAttr] = i_strAttrNameBf[iz];
				o_strAttrValues[o_nAttr] = i_strAttrValueBf[iz];

				// 属性値に不要な文字列を含んでいるか調査しログ出力する
				if (i_strAttrValueBf[iz].IndexOf("価格") >= 0)
				{
					string strErr = "属性値に価格を含んでいます: ";
					if (o_nAttr >= 1)
					{
						strErr += ("メーカー: " + o_strAttrValues[0] + " 商品名: " + o_strAttrValues[1]);
					}
					if (o_nAttr != 1)
					{
						strErr += ("属性値:" + o_strAttrValues[o_nAttr]);
					}
					Mkdb.WriteLogFile(strErr);
				}
				o_nAttr++;
			}
			// 同一属性名が複数ある場合は2番目の属性名には"_1"を付加し、以降"_2"･･･"_n"の追番を付加する
			for (int i=0; i<o_nAttr; i++)
			{
				int iC = 1;
				for(int j=i+1; j<o_nAttr; j++) {
					if (o_strAttrNames[j] == o_strAttrNames[i])
					{
						o_strAttrNames[j] += "_" + iC.ToString();
						iC++;
					}
				}
			}
		}

		// 全図形データを取り込む
		public static int importAllZukeiData()
		{
			int ist = 0;                        // ステイタス(<0:エラー)

			// 図形取得用フォルダを作成
			SafeCreateNewDirectory();

			// 図形取得
			int nZukeiURLMax = Const.z_nSyohinZukeiMax;
			int[] idAttrs = new int[nZukeiURLMax];
			string[] strZukeiURLs = new string[nZukeiURLMax];
			string[] strZukeiCDs = new string[nZukeiURLMax];
			int nZukeiURL;

			string[] strHdr1 = { "小分類ID", "取込" , "カテゴリID", "小分類名" };
			string[,] strItem1s = new string[Const.z_nSClassMax, 4];			// 全小分類の[小分類ID],[取込]のリスト
			int nSClass;														// 小分類数
			string[] strHdr2 = { "商品ID", "取込" };
			string[,] strItem2s = new string[Const.z_nSClassSyohinMax, 2];	// 商品ID,取込のリスト
			int nSyohin;														// 商品数
			bool bDeleteSClassZukei = false;								// 小分類の全属性データを削除しない
			bool bFirst = true;

			// 小分類IDと取込を取得
			Db.MySql.getTableItems(Const.z_strSClass2Table, strHdr1, 4, "", "", strItem1s, out nSClass);
			for (int ic1 = 0; ic1 < nSClass; ic1++)
			{
				string strCategoryURL = Mkdb.getCategoryURL(strItem1s[ic1, 2]);
			
				// 小分類単位の図形削除
				if (strItem1s[ic1, 1] == "0")
				{
					// 小分類の[取込]フラグが立っていないならスキップ
					continue;
				}
				if (Form1.m_iradioBtnHenkoMode == 1)
				{
					if (isSClassZukei(strCategoryURL, strItem1s[ic1, 0]) > 0)
					{
						// 変更モードがスキップモードで小分類の図形が存在する場合はスキップ
						continue;
					}
				}
				else if (Form1.m_iradioBtnHenkoMode == 3)
				{
					if (bFirst)
					{
						// 変更モードが削除後追加モードなら、小分類の全図形ファイルを削除する
						DialogResult result = MessageBox.Show("小分類(" + strItem1s[ic1, 3] + "の全図形ファイルを削除します。よろしいですか？",
							"質問",
							MessageBoxButtons.OKCancel,
							MessageBoxIcon.Exclamation,
							MessageBoxDefaultButton.Button2);
						if (result == DialogResult.OK)
						{
							bDeleteSClassZukei = true;
						}
						bFirst = false;
					}
					if (bDeleteSClassZukei)
					{
						deleteSClassZukei(strCategoryURL, strItem1s[ic1, 0]);
					}
				}

				// 商品IDと取込を取得
				string strCond = "小分類ID=" + strItem1s[ic1, 0];
				Db.MySql.getTableItems(Const.z_strSyohin2Table, strHdr2, 2, strCond, "", strItem2s, out nSyohin);
				for (int ic2 = 0; ic2 < nSyohin; ic2++)
				{
					// 商品の図形URLを取得する
					getZukeiURLs(strItem2s[ic2, 0], idAttrs, strZukeiURLs, strZukeiCDs, out nZukeiURL);

					// 未取込の図形(図形コードのファイルがない図形)を取り込む
					for (int iz = 0; iz < nZukeiURL; iz++)
					{
						//////////////////////////////////////////////////////////////////////////////////////////////
						// 実行時間を早めるため取込み図形の間引き
						// if (strZukeiCDs[iz].Substring(1, 8) == "00000000")
						if (strZukeiCDs[iz].Substring(1, 9) == "000000000")
						{
							continue;							// 取込可能図形のみ取り込む
						}
						// if (strZukeiCDs[iz].Substring(11, 4) != "S000")
						if (strZukeiCDs[iz].Substring(12, 4) != "S000")
						{
							continue;							// 一覧表の図形のみ取り込む
						}
						//////////////////////////////////////////////////////////////////////////////////////////////
						Mkdb.DispLine2(strZukeiCDs[iz]);
						if (Dbg.Utl.CheckError(String.IsNullOrEmpty(strZukeiCDs[iz]), "図形コードなし", "impAllZukeiData"))
						{
							ist = -1;
							break;
						}
						// 図形ファイルが既にありならスキップ
						if (isZukeiDataFile(strCategoryURL, strZukeiCDs[iz]) == 1)
						{
							continue;
						}
						// 図形取込み
						importZukeiData(strZukeiURLs[iz], strCategoryURL, strZukeiCDs[iz]);
					}
				}
			}
			return ist;
		}

		// New図形フォルダの有無を調べ無ければ作成する
		public static DirectoryInfo SafeCreateNewDirectory()
		{
			string strDirectoryName;
			strDirectoryName = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + "/New";
			if (System.IO.Directory.Exists(strDirectoryName))
			{
				return null;
			}
			return Directory.CreateDirectory(strDirectoryName);
		}

		// 図形データファイルの有無を調べる
		private static int isZukeiDataFile(
			string i_strCategoryURL,
			string i_strZukeiCD
			)
		{
			int ist = 0;						// ファイルの有無　(0: 無し, 1: 有り)
			string strFileName;
			strFileName = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + i_strCategoryURL + i_strZukeiCD + ".jpg";
			if (System.IO.File.Exists(strFileName))
			{
				ist = 1;
			}
			return ist;
		}

		// 小分類の図形データの存在を調べる
		private static int isSClassZukei(							// ファイル数
			string i_strCategoryURL,
			string i_strSClassID)
		{
			string strZukeiDir;
			strZukeiDir = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + i_strCategoryURL;		// 図形データフォルダ
			string strZSClassID = "00000" + i_strSClassID;
			int nSClassID5 = strZSClassID.Length;
			strZSClassID = "z" + strZSClassID.Substring(nSClassID5 - 5, 5) + "*";

			// 小分類の全図形ファイルパスを取得する
			string[] files = System.IO.Directory.GetFiles(strZukeiDir, strZSClassID, System.IO.SearchOption.TopDirectoryOnly);

			return files.Length;
		}

		// 小分類の図形データを全て削除する
		private static int deleteSClassZukei(
			string i_strCategoryURL,
			string i_strSClassID)
		{
			int ist = 0;
			string strZukeiDir;
			strZukeiDir = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + i_strCategoryURL;		// 図形データフォルダ

			string strZSClassID = "00000" + i_strSClassID;
			int nSClassID5 = strZSClassID.Length;
			strZSClassID = "z" + strZSClassID.Substring(nSClassID5 - 5, 5);
			string strSearchFile = strZSClassID + "*.jpg";
			
			// 小分類の全図形ファイルパスを取得する
			string[] files1 = System.IO.Directory.GetFiles(strZukeiDir, strSearchFile, System.IO.SearchOption.TopDirectoryOnly);

			// 図形ファイルを削除する
			for (int iZ=0; iZ<files1.Length; iZ++)
			{
				if (files1[iZ].IndexOf(strZSClassID) > 0)
				{
					Mkdb.DeleteFile(files1[iZ]);
				}
				else
				{
					// 操作が煩雑でありエラーメッセージは出さないように変更
//					Dbg.Utl.MessageBoxShow("対象外の削除図形ファイルエラー\nSystem.IO.Directory.GetFilesの不具合で\n最後の1ファイルが対象外である可能性あり", "deleteSClassZukei");
				}
			}
			return ist;
		}

		// 図形データを取り込む
		private static int importZukeiData(		// 戻り値	(0:正常, 1:エラー)
			string i_strZukeiURL,				// 取込み図形データURL
			string i_strCategoryURL,
			string i_strZukeiCD					// 図形コード
			)
		{
			int ist = 0;
			string strFileName;
			string strFileIns;
			try
			{
				// 図形ファイルパス
				strFileName = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + i_strCategoryURL + i_strZukeiCD + ".jpg";

				// 新規追加図形保存ファイルパス
				strFileIns = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + Const.z_pathZukeiIns + i_strZukeiCD + ".jpg";

				// 図形データURLの図形取込み
				WebClient cl = new WebClient();
				byte[] pic = cl.DownloadData(i_strZukeiURL);
				MemoryStream st = new MemoryStream(pic);

				// 図形ファイル作成
				using (Bitmap oImage = new Bitmap(st))
				{
					oImage.Save(strFileName);
					oImage.Save(strFileIns);
				}
				st.Close();
			}
			catch (Exception ex)
			{
				// エラーログ
				string strErrZukei = "図形コード:" + i_strZukeiCD + " 図形URL:" + i_strZukeiURL;
				string strErrLog = "図形取込みエラー: " + strErrZukei + "\n" + ex.ToString();
				Mkdb.WriteLogFile(strErrLog);

				if (!Form1.m_bChkErrorSkip)						//エラースキップ
				{
					string strInquiry = strErrZukei + "\n" + ex.ToString() + "\n 次の図形取込みエラーはスキップしますか？";
					DialogResult result = MessageBox.Show(strInquiry,
						"質問",
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Exclamation,
						MessageBoxDefaultButton.Button2);
					if (result == DialogResult.OK)
					{
						Form1.m_bChkErrorSkip = true;			// 図形取込みエラースキップ
					}
				}

				ist = 1;
			}
			return ist;
		}

		// 図形URLを取得する
		public static int getZukeiURLs(
			string i_strSyohinID,				// 商品ID または "":全図形URL
			int[] o_idAttrs,					// 属性IDリスト
			string[] o_strZukeiURLs,			// 図形URLリスト
			string[] o_strZukeiCDs,				// 図形コードリスト
			out int o_nZukeiURL					// 図形URL数
			)
		{
			int ist = 0;
			int nZukeiURL = o_strZukeiURLs.Length;
			o_nZukeiURL = 0;
			// URL型属性値マスタからURLと図形コードを取得
			string[] strHdr = { "属性ID", "URL", "図形CD" };
			string[,] strItems = new string[Const.z_nAttrURLMax, 3];				// 全URL型属性値の[属性ID],[URL],[図形CD]のリスト
			string strCond = "";
			if (i_strSyohinID != "")
			{
				strCond = "商品ID=" + i_strSyohinID + " AND 属性ID >= 2000 AND 属性ID < 3000";
			}
			ist = Db.MySql.getTableItems(Const.z_strAttributeURL2Table, strHdr, 3, strCond, "", strItems, out nZukeiURL);

			for (int ic = 0; ist == 0 && ic < nZukeiURL; ic++)
			{
				if(	Dbg.Utl.CheckError(String.IsNullOrEmpty(strItems[ic, 2]),"図形コードなし", "getZukeiURLs") ||
					Dbg.Utl.CheckOverflow(o_nZukeiURL >= nZukeiURL, "図形URL", "getZukeiURLs"))
				{
					ist = -1;
					break;
				}
				o_idAttrs[o_nZukeiURL] = int.Parse(strItems[ic,0]);
				o_strZukeiURLs[o_nZukeiURL] = strItems[ic, 1];
				o_strZukeiCDs[o_nZukeiURL] = strItems[ic, 2];
				o_nZukeiURL++;
			}
			return ist;
		}

		// メーカー製品情報URLを取得する
		public static int getProductInfoURLs(
			string i_strSyohinID,				// 商品ID または "":全URL
			int[] o_idAttrs,					// 属性IDリスト
			string[] o_strProductURLs,			// メーカー製品情報URLリスト
			out int o_nZukeiURL					// メーカー製品情報URL数
			)
		{
			int ist = 0;
			int nZukeiURL = o_strProductURLs.Length;
			o_nZukeiURL = 0;
			// URL型属性値マスタからURLと図形コードを取得
			string[] strHdr = { "属性ID", "URL"};
			string[,] strItems = new string[Const.z_nAttrURLMax, 2];			// 全URL型属性値の[属性ID],[URL],[図形CD]のリスト
			string strCond = "";
			if (i_strSyohinID != "")
			{
				strCond = "商品ID=" + i_strSyohinID + " AND 属性ID >= 1001 AND 属性ID <= 1004";
			}
			ist = Db.MySql.getTableItems(Const.z_strAttributeURL2Table, strHdr, 2,
										 strCond, "", strItems, out nZukeiURL);

			for (int ic = 0; ist == 0 && ic < nZukeiURL; ic++)
			{
				o_idAttrs[o_nZukeiURL] = int.Parse(strItems[ic, 0]);
				o_strProductURLs[o_nZukeiURL] = strItems[ic, 1];
				o_nZukeiURL++;
			}
			return ist;
		}

		// 商品属性データの削除
		public static void deleteSyohinAttr(
			string i_strSyohinID				// 商品ID
			)
		{
			Mkdb.deleteSyohinAttr(i_strSyohinID);
		}

		// 商品データの削除
		public static void deleteSyohin(
			string i_strSyohinID				// 商品ID
			)
		{
			Mkdb.deleteSyohin(i_strSyohinID);
		}

		// 小分類の全データの削除
		public static void deleteSClass(
			string i_strSClassID				// 小分類ID
			)
		{
			Mkdb.deleteSClass(i_strSClassID);
		}

		// DBの内容をファイルにDump出力する
		public static void Dump(
			string i_strFilePath				// Dumpファイルパス
			)
		{
			string[] strDB = { Const.z_strDb1, Const.z_strDb2 };

			// ダンプファイルオープン
			OpenDumpFile(i_strFilePath);

			// MySql DB オープン
			if (Db.MySql.Connect(strDB[Form1.m_iradioBtnDB - 1]) == 0)
			{
				// マスタのダンプ出力
				// カテゴリマスタ
				Mkdb.DispLine2("カテゴリマスタ");
				Mkdb.WriteDumpFile("◎カテゴリマスタ");
				string[] strHdr1 = { "カテゴリID", "表示", "名称", "カテゴリURL", "内容", "表示名称", "表示カテゴリURL", "表示内容",
									 "最大大分類ID", "最大中分類ID", "最大小分類ID" };
				Mkdb.DumpMst("CTG", Const.z_strCategoryTable, strHdr1, strHdr1[0], Const.z_nCategoryMax, 11);

				// 大分類マスタ
				Mkdb.DispLine2("大分類マスタ");
				Mkdb.WriteDumpFile("◎大分類マスタ");
				string[] strHdr2 = { "大分類ID", "表示", "カテゴリID", "大分類名", "表示大分類名", "内容" };
				Mkdb.DumpMst("LCL", Const.z_strLClassTable, strHdr2, strHdr2[0], Const.z_nLClassMax, 6);

				// 中分類マスタ
				Mkdb.DispLine2("中分類マスタ");
				Mkdb.WriteDumpFile("◎中分類マスタ");
				string[] strHdr3 = { "中分類ID", "表示", "カテゴリID", "大分類ID", "中分類名", "表示中分類名" };
				Mkdb.DumpMst("MCL", Const.z_strMClassTable, strHdr3, strHdr3[0], Const.z_nMClassMax, 6);

				// 小分類マスタ
				Mkdb.DispLine2("小分類マスタ");
				Mkdb.WriteDumpFile("◎小分類マスタ");
				string[] strHdr4 = { "小分類ID", "取込", "表示", "カテゴリID", "大分類ID", "中分類ID",
										 "小分類名", "小分類URL", "表示小分類名", "表示小分類URL", "最大商品ID" };
				Mkdb.DumpMst("SCL", Const.z_strSClass2Table, strHdr4, strHdr4[0], Const.z_nSClassMax, 11);

				// 中小分類関係マスタ
				Mkdb.DispLine2("中小分類関係マスタ");
				Mkdb.WriteDumpFile("◎中小分類関係マスタ");
				string[] strHdr5 = { "中分類ID", "小分類ID" };
				Mkdb.DumpMst("MSR", Const.z_strMSCRelation, strHdr5, strHdr5[0], Const.z_nMSCRelationMax, 2);

				// 属性名マスタ
				Mkdb.DispLine2("属性名マスタ");
				Mkdb.WriteDumpFile("◎属性名マスタ");
				string[] strHdr6 = { "小分類ID", "属性ID", "表示", "属性名", "表示属性名", "表示巾" };
				string strOdr6 = strHdr6[0] + "," + strHdr6[1];
				Mkdb.DumpMst("ATN", Const.z_strAttributeNameTable, strHdr6, strOdr6, Const.z_nSyohinMax, 6);

				// 一般属性値マスタ
				Mkdb.DispLine2("一般属性値マスタ");
				Mkdb.WriteDumpFile("◎一般属性値マスタ");
				string[] strHdr7 = { "商品ID", "属性ID", "属性値", "表示属性値" };
				string strOdr7 = strHdr7[0] + "," + strHdr7[1];
				Mkdb.DumpMst("ATV", Const.z_strAttributeValue2Table, strHdr7, strOdr7, Const.z_nAttrValueMax, 4);

				// URL型属性値マスタ
				Mkdb.DispLine2("URL型属性値マスタ");
				Mkdb.WriteDumpFile("◎URL型属性値マスタ");
				string[] strHdr8 = { "商品ID", "属性ID", "URL", "図形CD" };
				string strOdr8 = strHdr8[0] + "," + strHdr8[1];
				Mkdb.DumpMst("ATU", Const.z_strAttributeURL2Table, strHdr8, strOdr8, Const.z_nAttrURLMax, 4);

				// 商品マスタ
				Mkdb.DispLine2("商品マスタ");
				Mkdb.WriteDumpFile("◎商品マスタ");
				string[] strHdr9 = { "商品ID", "取込", "表示", "小分類ID", "商品名", "表示商品名", "安値", "評判", "評価", "発売時期" };
				Mkdb.DumpMst("SYO", Const.z_strSyohin2Table, strHdr9, strHdr9[0], Const.z_nSyohinMax, 6);

				// 制御マスタ
				Mkdb.DispLine2("制御マスタ");
				Mkdb.WriteDumpFile("◎制御マスタ");
				string[] strHdr10 = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
				string strOdr10 = strHdr10[0] + "," + strHdr10[1];
				Mkdb.DumpMst("CTL", Const.z_strControlTable, strHdr10, strOdr10, Const.z_nSyohinMax, 6);

				// MySql DB クローズ
				Db.MySql.DisConnect();
			}
			// ダンプファイルクローズ
			CloseDumpFile();
		}

		// 商品HP作成
		public static int createSyohinHP()
		{
			int ist = 0;
			string[] strHdr1 = { "小分類ID", "取込", "カテゴリID", "小分類名" };
			string[,] strItem1s = new string[Const.z_nSClassMax, 4];			// 全小分類の[小分類ID],[取込]のリスト
			int nSClass;														// 小分類数
			string[] strHdr2 = { "商品ID", "取込" };
			string[,] strItem2s = new string[Const.z_nSClassSyohinMax, 2];	// 商品ID,取込表示のリスト
			int nSyohin;														// 商品数

			// 基準商品HPのパス
			string strOrigSyohinHP = Const.z_pathSyohinBaseHP[Form1.m_iradioBtnDB - 1] + "p0000000001.php";

			// 全小分類の[小分類ID]と[取込],[カテゴリID]を取得
			Db.MySql.getTableItems(Const.z_strSClass2Table, strHdr1, 4, "", "", strItem1s, out nSClass);
			for (int ic1 = 0; ic1 < nSClass; ic1++)
			{
				string strCreSyohinHP;
				if (strItem1s[ic1, 1] == "0")
				{
					// 小分類の[取込]フラグがたっていないものはスキップ
					continue;
				}
				Mkdb.DispLine2(strItem1s[ic1, 3] + "の商品HP作成");
				// カテゴリURLを取得
				string strCategoryURL = Mkdb.getCategoryURL(strItem1s[ic1, 2]);
				// 小分類内の全商品の[商品ID]と[表示]を取得
				string strCond = "小分類ID=" + strItem1s[ic1, 0];
				Db.MySql.getTableItems(Const.z_strSyohin2Table, strHdr2, 2, strCond, "", strItem2s, out nSyohin);
				for (int ic2 = 0; ic2 < nSyohin; ic2++)
				{
					try
					{
                        // 作成する商品HPのパスを求めオリジナルをその名称でコピーする　("Sxxxxxxxxxx.php")
                        strCreSyohinHP = Const.z_envdir[Form1.m_iradioBtnDB - 1] + strCategoryURL +
                                         "p" + Mkdb.ConvertSyohinID10(strItem2s[ic1, 0]) + ".php";
						System.IO.File.Copy(strOrigSyohinHP, strCreSyohinHP, true);		// 上書き
					}
					catch (Exception ex)
					{
						Dbg.Utl.MessageBoxShow(ex.Message, "商品HP 作成 エラー");
						return -1;
					}
				}
			}
			Mkdb.DispLine2("商品HP作成終了");
			return ist;
		}

		// 1:小分類HPまたは2:商品HPを作成する
		public static int createSClassSyohinHP(int iHPType)
		{
			int ist = 0;
			Mkdb.DispLine2("商品HP作成開始");
			// 基準小分類HPのパス
			string strOrigCategoryHP = Const.z_pathCategoryBaseHP[Form1.m_iradioBtnDB - 1] + "s000001.php";
			// 基準商品HPのパス
			string strOrigSyohinHP = Const.z_pathSyohinBaseHP[Form1.m_iradioBtnDB - 1] + "p0000000001.php";

			// カテゴリマスタ
			string[] strHdr1 = { "カテゴリID", "表示", "表示カテゴリURL", "表示名称" };
			string[,] strItem1s = new string[Const.z_nCategoryMax, strHdr1.Length];		// カテゴリのリスト
			int nSCategory;																	// カテゴリ数

			// 大分類マスタ
			string[] strHdr2 = { "大分類ID", "表示", "表示大分類名" };
			string[,] strItem2s = new string[Const.z_nLClassMax, strHdr2.Length];		// カテゴリ内の大分類のリスト
			int nLClass;																	// 大分類数

			// 中分類マスタ
			string[] strHdr3 = { "中分類ID", "表示", "表示中分類名" };
			string[,] strItem3s = new string[Const.z_nMClassMax, strHdr3.Length];		// 大分類内の中分類のリスト
			int nMClass;																	// 中分類数

			// 小分類マスタ
			string[] strHdr4 = { "A.小分類ID", "A.取込", "A.表示小分類名" };
			string[,] strItem4s = new string[Const.z_nSClassMax, strHdr4.Length];		// 中分類内の小分類のリスト
			int nSClass;																	// 小分類数

			// 商品マスタ
			string[] strHdr5 = { "商品ID", "表示", "表示商品名" };
			string[,] strItem5s = new string[Const.z_nSClassMax, strHdr5.Length];		// 小分類内の商品のリスト
			int nSyohin;																	// 商品数

			// カテゴリマスタを取得
			Db.MySql.getTableItems(Const.z_strCategoryTable, strHdr1, strHdr1.Length, "", "",
								   strItem1s, out nSCategory);
			for (int ic1 = 0; ic1 < nSCategory; ic1++)
			{
				if (strItem1s[ic1, 1] == "0")
				{
					// カテゴリの[表示]フラグがたっていないものはスキップ
					continue;
				}
				string strCategoryURL = strItem1s[ic1,2];

				// 大分類を取得
				string strCond2 = "カテゴリID=" + strItem1s[ic1, 0].ToString() + " AND 表示!='0'"; ;
				Db.MySql.getTableItems(Const.z_strLClassTable, strHdr2, strHdr2.Length, strCond2, "",
									   strItem2s, out nLClass);
				for (int ic2 = 0; ic2 < nLClass; ic2++)
				{
					// 中分類を取得
					string strCond3 = "大分類ID=" + strItem2s[ic2, 0].ToString();
					Db.MySql.getTableItems(Const.z_strMClassTable, strHdr3, strHdr3.Length, strCond3, "",
										   strItem3s, out nMClass);
					for (int ic3 = 0; ic3 < nMClass; ic3++)
					{
						if (strItem3s[ic3, 1] == "0")
						{
							// 中分類の[表示]フラグがたっていないものはスキップ
							continue;
						}

						// 中分類の小分類IDと取込を取得
						string strTable4 = Const.z_strSClass2Table + " A, " +
										   Const.z_strMSCRelation + " B";
						string strCond4 = "B.中分類ID=" + strItem3s[ic3, 0].ToString() +
										  " AND " + "A.小分類ID=B.小分類ID" + " AND A.取込!='0'"; ;
						Db.MySql.getTableItems(strTable4, strHdr4, strHdr4.Length, strCond4, "",
											   strItem4s, out nSClass);
						for (int ic4 = 0; ic4 < nSClass; ic4++)
						{
							string strSClassID;
							int nSClassID;
							string strCreCategoryHP;

							Mkdb.DispLine2(strItem4s[ic4, 2] + "のHP作成");
							// 小分類HPの作成
							if (iHPType == 1)
							{
								try
								{
									// 0パディングした6桁の小分類IDを求める
									strSClassID = "00000" + strItem4s[ic4, 0];
									nSClassID = strSClassID.Length;
									strSClassID = strSClassID.Substring(nSClassID - 6);
									// 作成する小分類HPのパスを求めオリジナルをその名称でコピーする　("Sxxxxxx.php")
									strCreCategoryHP = Const.z_envdir[Form1.m_iradioBtnDB - 1] + strCategoryURL + "s" + strSClassID + ".php";
									System.IO.File.Copy(strOrigCategoryHP, strCreCategoryHP, true);		// 上書き
								}
								catch (Exception ex)
								{
									Dbg.Utl.MessageBoxShow(ex.Message, "小分類HP 作成 エラー");
									return -1;
								}
							}
							// 商品HPの作成
							if (iHPType == 2)
							{
								// 小分類内の全商品の[商品ID]と[表示]を取得
								string strCond5 = "小分類ID=" + strItem4s[ic4, 0] + " AND 表示!='0'";
								Db.MySql.getTableItems(Const.z_strSyohin2Table, strHdr5, strHdr5.Length, strCond5, "", strItem5s, out nSyohin);
								for (int ic5 = 0; ic5 < nSyohin; ic5++)
								{
									string strCreSyohinHP;

									try
									{
										// 作成する商品HPのパスを求めオリジナルをその名称でコピーする　("Sxxxxxxxxx.php")
										strCreSyohinHP = Kc.Const.z_envdir[Form1.m_iradioBtnDB - 1] + strCategoryURL + 
														 "p" + Mkdb.ConvertSyohinID10(strItem5s[ic5, 0]) + ".php";
										System.IO.File.Copy(strOrigSyohinHP, strCreSyohinHP, true);		// 上書き
									}
									catch (Exception ex)
									{
										Dbg.Utl.MessageBoxShow(ex.Message, "商品HP 作成 エラー");
										return -1;
									}
								}
							}
						}
					}
				}
			}
			Mkdb.DispLine2("商品HP作成終了"); 
			return ist;
		}

		// 最大商品IDの整合性チェック
		public static int checkMaxSyohinID()
		{
			int ist = 0;						// ステイタス(<0:エラー)
			int iMaxSyohinID_SClass;
			uint iMaxSyohinID_Syohin;
			string strMsg;

			Mkdb.WriteLogFile("最大商品ID チェック 開始");

			// 小分類マスタ
			string[] strHdr1 = { "小分類ID", "小分類名", "最大商品ID" };
			string[,] strItem1s = new string[Const.z_nSClassMax, 3];			// 全小分類の[小分類ID],[最大商品ID],[小分類名]のリスト
			int nSClass;														// 小分類数
			// 商品マスタ
			string[] strHdr2 = { "商品ID", "小分類ID" };
			string[,] strItem2s = new string[Const.z_nSyohinMax, 2];			// 商品ID,取込のリスト
			int nSyohin;														// 商品数
			// 商品IDを取得
			Db.MySql.getTableItems(Const.z_strSyohin2Table, strHdr2, 2,
								   "", "", strItem2s, out nSyohin);

			// 小分類IDと最大商品ID,小分類名を取得
			Db.MySql.getTableItems(Const.z_strSClass2Table, strHdr1, 3,
								   "", "", strItem1s, out nSClass);
			for (int ic1 = 0; ic1 < nSClass; ic1++)
			{
				iMaxSyohinID_SClass = int.Parse( strItem1s[ic1, 2]);
				// 小分類毎に最大商品IDをチェック
				iMaxSyohinID_Syohin = 0;
				for (int ic2 = nSyohin - 1; ic2 >= 0; ic2-- )
				{
					if (strItem2s[ic2, 1] == strItem1s[ic1, 0])
					{
						if (iMaxSyohinID_Syohin < uint.Parse(strItem2s[ic2,0]))
						{
							iMaxSyohinID_Syohin = uint.Parse(strItem2s[ic2, 0]);
						}
					}
				}

				if (iMaxSyohinID_Syohin == 0 && iMaxSyohinID_SClass % Const.z_iSClassID2Unit == 0)
				{
					// 商品無しで商品IDの下5桁==0 OK
				}
				else if (iMaxSyohinID_SClass != iMaxSyohinID_Syohin)
				{
					strMsg = "最大商品ID チェックエラー　小分類ID=" + strItem1s[ic1, 0] +
							 " 小分類名=" + strItem1s[ic1, 1] +
							 " 小分類の最大商品ID=" + strItem1s[ic1, 2] +
							 " 商品の最大商品ID=" + iMaxSyohinID_Syohin;
					Mkdb.WriteLogFile(strMsg);
					string strInquiry = "最大商品ID チェックエラー\n小分類ID=" + strItem1s[ic1, 0] +
										" 小分類名=" + strItem1s[ic1, 1] +
										"\n小分類の最大商品ID =\t" + strItem1s[ic1, 2] +
										"\n商品の最大商品ID    =\t" + iMaxSyohinID_Syohin +
										"\n\n小分類の最大商品IDを修正しますか？";
					DialogResult result = MessageBox.Show(strInquiry,
						"質問",
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Exclamation,
						MessageBoxDefaultButton.Button2);
					if (result == DialogResult.OK)
					{
						Mkdb.modMaxSyohinID(strItem1s[ic1, 0], iMaxSyohinID_Syohin);
						Mkdb.WriteLogFile("最大商品IDを修正しました。");
						ist = 1;
					}
					else
					{
						ist = -1;
						break;
					}
				}
			}
			Mkdb.WriteLogFile("最大商品ID チェック 終了");
			return ist;
		}

		// 図形チェック
		public static int checkZukeiCD(string strFirstSlassID)
		{
			int ist = 0;						// ステイタス(<0:エラー)
			// 小分類毎の図形情報
			int nZukeiURLMax = Const.z_nSClassZukeiMax;
			string[] strZukeiAttrIDs = new string[nZukeiURLMax];
			string[] strZukeiURLs = new string[nZukeiURLMax];
			string[] strZukeiCDs = new string[nZukeiURLMax];
			string[] strSyohinIDs = new string[nZukeiURLMax];
			string[] strSyohinDisps = new string[nZukeiURLMax];
			int[] iEqfs = new int[nZukeiURLMax];
			int nZukeiURL;
			// 小分類毎の同一図形判定
			string[] strSameZukeiAttrIDs = new string[nZukeiURLMax];
			string[] strSameZukeiCDs = new string[nZukeiURLMax];
			string[] strSameZukeiSyohinIDs = new string[nZukeiURLMax];
			string[] strSameZukeiSyohinDisps = new string[nZukeiURLMax];
			// 商品毎の図形情報
			int nSyohinZukeiURLMax = Const.z_nSyohinZukeiMax;
			int[] idSyohinAttrs = new int[nSyohinZukeiURLMax];
			string[] strSyohinZukeiURLs = new string[nSyohinZukeiURLMax];
			string[] strSyohinZukeiCDs = new string[nSyohinZukeiURLMax];
			int nSyohinZukeiURL;
			// 小分類マスタ
			string[] strHdrSClass = { "小分類ID", "取込", "カテゴリID", "小分類名" };
			string[,] strItemSClasss = new string[Const.z_nSClassMax, 4];			// 小分類マスタのリスト
			int nSClass;														// 小分類数
			// 商品マスタ
			string[] strHdrSyohin = { "商品ID", "取込", "表示" };
			string[,] strItemSyohins = new string[Const.z_nSClassSyohinMax, 3];	// 商品マスタのリスト
			int nSyohin;														// 商品数
			// 属性名マスタ
			string[] strHdrAttrID = { "属性ID" };
			string[] strItemAttrIDs = new string[1];									// 属性名マスタのリスト
			// 一般属性値マスタ
			string[] strHdrAttrValue = { "表示属性値" };
			string[] strItemAttrValues = new string[1];									// 一般属性値マスタのリスト
			// 制御マスター
			string[] strHdrCtrl = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
			string[,] strItemCtrls = new string[Const.z_nCtrlIDMax, strHdrCtrl.Length];	// 制御マスタの制御IDのリスト
			int nItem1;
			// 制御マスタのレコードを取得する
			string strCond0 = "制御TP>=1 AND 制御TP<=3";
			string strOrder = "制御TP,制御ID";
			// コントロールテーブルの読み込み
			Db.MySql.getTableItems(Const.z_strControlTable, strHdrCtrl, strHdrCtrl.Length,
								   strCond0, strOrder, strItemCtrls, out nItem1);
			//// temporary coding
			//string strCond1 = "制御TP=";
			//string strCond2 = "制御ID=";
			//for (int iT=0; iT<nItem1; iT++)
			//{
			//	int len = strItemCtrls[iT, 3].Length;
			//	strItemCtrls[iT, 3] = strItemCtrls[iT, 3].Insert(len - 4, "0");

			//	// mySQLのデータを修正

			//	Db.MySql.modifyMultiCondRecordItems2D(Const.z_strControlTable, strHdrCtrl, strHdrCtrl.Length,
			//												strCond1, strItemCtrls[iT, 0],
			//												strCond2, strItemCtrls[iT, 1], strItemCtrls, iT);
			//}
			//// temporary coding

			string[][] strCngZukeiCDs = new string[Const.z_nCtrlIDMax][];
			for (int ic = 0; ic < nItem1; ic++)
			{
				// 図形TP, URL, 図形ファイル名
				strCngZukeiCDs[ic] = new string[3] { strItemCtrls[ic, 0], strItemCtrls[ic, 2], strItemCtrls[ic, 3] };
			}

			// 全小分類の小分類IDと取込を取得
			Db.MySql.getTableItems(Const.z_strSClass2Table, strHdrSClass, 4, "", "", strItemSClasss, out nSClass);
			for (int ic1 = 0; ic1 < nSClass; ic1++)
			{
                string strSClassID = strItemSClasss[ic1, 0];
                if (strFirstSlassID != "" && string.Compare(strSClassID, strFirstSlassID) < 0)
                {
                    continue;
                }

                if (strItemSClasss[ic1, 1] == "0")
				{
					// 小分類の[取込]フラグが立っていないならスキップ
					continue;
				}

				// 小分類のメーカーの属性IDを取得
				string strCond3 = "小分類ID=" + strItemSClasss[ic1, 0] + " AND 属性名='メーカー'";
				Db.MySql.getRecordItems(Const.z_strAttributeNameTable, strHdrAttrID, 1, strCond3, strItemAttrIDs);
				string strMakerAttrID = strItemAttrIDs[0];

				// 小分類毎に同一図形をチェック
				nZukeiURL = 0;

				Mkdb.DispLine2("小分類ID=" + strItemSClasss[ic1, 0] + " " + strItemSClasss[ic1, 3] + "の図形調査");
				// Mkdb.WriteLogFile("\n●" + strItemSClasss[ic1, 3] + "[" + strItemSClasss[ic1, 0] + "]の図形調査●");

                string strTrace = "〇図形チェッククラス SClassID=" + strItemSClasss[ic1, 0] + " 小分類名=" + strItemSClasss[ic1, 3];
                Mkdb.WriteLogFile(strTrace);
                Mkdb.FlushLogFile();

                // 表示カテゴリURLを取得
                string strCategoryURL = Mkdb.getCategoryURL(strItemSClasss[ic1, 2]);

				// 小分類の全商品の図形URLを取得し、図形コードを変換する
				// 小分類の全商品を取得する
				string strCond = "小分類ID=" + strItemSClasss[ic1, 0];
				Db.MySql.getTableItems(Const.z_strSyohin2Table, strHdrSyohin, 3, strCond, "", strItemSyohins, out nSyohin);
				for (int ic2 = 0; ic2 < nSyohin; ic2++)
				{
					string strSyohinID = strItemSyohins[ic2, 0];
					// 商品のメーカーコードを取得する
					string strCond4 = "商品ID=" + strSyohinID + " AND 属性ID=" + strMakerAttrID;
					Db.MySql.getRecordItems(Const.z_strAttributeValue2Table, strHdrAttrValue, 1, strCond4, strItemAttrValues);
					string strMakerCD = strItemAttrValues[0];

					// 商品の図形URLを取得する
					getZukeiURLs(strSyohinID, idSyohinAttrs, strSyohinZukeiURLs, strSyohinZukeiCDs, out nSyohinZukeiURL);
					for (int isyhz = 0; isyhz < nSyohinZukeiURL; isyhz++)
					{
						strZukeiAttrIDs[nZukeiURL] = idSyohinAttrs[isyhz].ToString();	// [属性ID]
						strZukeiURLs[nZukeiURL] = strSyohinZukeiURLs[isyhz];			// [URL]
						strZukeiCDs[nZukeiURL] = strSyohinZukeiCDs[isyhz];				// [図形CD]
						strSyohinIDs[nZukeiURL] = strSyohinID;							// [商品ID]
						strSyohinDisps[nZukeiURL] = strItemSyohins[ic2, 2];				// 商品の[表示]

						// URLとメーカーコードを調べ図形コードを変換する
						changeZukeiCD(strZukeiURLs[nZukeiURL], strCategoryURL, strSyohinIDs[nZukeiURL],
									  strZukeiAttrIDs[nZukeiURL], strZukeiCDs[nZukeiURL], strMakerCD,
									  strCngZukeiCDs, nItem1);

						nZukeiURL++;
					}
				}
			}
			Mkdb.DispLine2("画像調査終了");
			return ist;
		}
		// 図形の図形コードの変更と既作成図形の削除
		private static int changeZukeiCD(string i_strZukeiURL, string i_strCategoryURL,
										 string i_strZukeiSyohinID, string i_strZukeiAttrID,
										 string i_strZukeiCD, string i_strMakerCD,
										 string[][] i_strCngZukeiCDs, int i_nItem)
		{
			int icngflg = 0;

			// 図形フォルダ
			string strZukeiDir;
			strZukeiDir = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1];			// リリース図形データ

			// 10桁商品ID
			string strZukeiSyohinID10 = Mkdb.ConvertSyohinID10(i_strZukeiSyohinID);

			// 未定義図形のチェック
			int ic1;

			// URL型属性値マスタ
			string[] strHdrAURL = {"図形CD"};

			for (ic1 = 0; ic1 < i_nItem; ic1++)
			{
				string strCngType = i_strCngZukeiCDs[ic1][0];

				if (strCngType == "1")
				{
					// 図形コードが図形無しのコードの場合は、図形を削除し代わりに"NO PHOTO"の図形に変更する
					int iclm = i_strZukeiURL.IndexOf(i_strCngZukeiCDs[ic1][1]);
					if (iclm < 0)
					{
						continue;				// 図形が有りスキップする
					}
				}
				else if (strCngType == "2")
				{
					// メーカーコードが無い場合は、図形を削除フォルダに移動し代わりに"NO PHOTO"の図形に変更する
					if (i_strMakerCD != "-" && i_strMakerCD != "")
					{
						continue;				// メーカーコードが有りスキップする
					}
				}
				else if (strCngType == "3")
				{
					// 他の図形コードが図形無しのコードの場合は、図形を削除フォルダに移動し代わりに"NO PHOTO"の図形に変更する
					int iclm = i_strZukeiURL.IndexOf(i_strCngZukeiCDs[ic1][1]);
					if (iclm < 0)
					{
						continue;				// 対象外の図形はスキップする
					}
				}
				else if (strCngType == "9")
				{
					// 調査用に、図形を調査(examination)削除フォルダにコピーする
					int iclm = i_strZukeiURL.IndexOf(i_strCngZukeiCDs[ic1][1]);
					if (iclm < 0)
					{
						continue;				// 対象外の図形はスキップする
					}
				}
				else
				{
					Dbg.Utl.CheckError(true, "対象外の図形URLです", "No Photo図形コード変換エラー");
					continue;
				}

				// 元の図形のファイルパスを求める

				// 元の図形コードを作成し、その図形ファイルがあれば削除する
				string strSuffix = "";
				if (i_strZukeiAttrID == "2000")
				{
					strSuffix = "-S000";			// 一覧表示用図形URL
				}
				else if (i_strZukeiAttrID == "2100")
				{
					strSuffix = "-S100";			// 主図形URL
				}
				string strOrigZukeiCD = "Z" + strZukeiSyohinID10 + strSuffix;
				string strToZukeiCD = i_strCngZukeiCDs[ic1][2] + strSuffix;

				string strFromFilePath = strZukeiDir + i_strCategoryURL + strOrigZukeiCD + ".jpg";  // 元の図形のファイルパス
				string strToFilePath;
				string strDelFilePath;

				if (strCngType == "1")
				{
					// 図形を削除する
					Mkdb.DeleteFile(strFromFilePath);
					// Newフォルダの図形を削除する
					strDelFilePath = strZukeiDir + "/New/" + strOrigZukeiCD + ".jpg";
					Mkdb.DeleteFile(strDelFilePath);
				}
				else if (strCngType == "2" || strCngType == "3")
				{
					// 図形を削除フォルダに移動する
					strToFilePath = strZukeiDir + "/Delete/" + strOrigZukeiCD + ".jpg";
					Mkdb.MoveFile(strFromFilePath, strToFilePath);
					// Newフォルダの図形を削除する
					strDelFilePath = strZukeiDir + "/New/" + strOrigZukeiCD + ".jpg";
					Mkdb.DeleteFile(strDelFilePath);
				}
				else if (strCngType == "9")
				{
					// 検査用フォルダにコピーする
					strToFilePath = strZukeiDir + "/examination/" + strOrigZukeiCD + ".jpg";
					Mkdb.CopyFile(strFromFilePath, strToFilePath);
					continue;
				}

				if (i_strZukeiCD != strToZukeiCD)
				{
					// 図形コードを変更する　strCngType == ("1" | "2" | "3" の時は、"NO PHOTO"図形の図形コード)
					icngflg = 1;
					// mySQLのデータを修正
					string strCond1 = "商品ID=";
					string strCond2 = "属性ID=";
					string[,] strItemAURL = { { strToZukeiCD } };
					if (Db.MySql.modifyMultiCondRecordItems2D(Const.z_strAttributeURL2Table, strHdrAURL, strHdrAURL.Length,
																strCond1, i_strZukeiSyohinID,
																strCond2, i_strZukeiAttrID, strItemAURL, 0) != 0)
					{
						Dbg.Utl.MessageBoxShow("データ修正エラー", "DBエラー");
					}
				}
			}
			return icngflg;
		}

		// 属性IDの表示属性値を得る
		//   属性IDの属性が無ければnull ("")を返す
		static string getDispAttr(string[,] i_strItemAts, string i_strIdAt, int i_idAtm)
		{
			int idAt = int.Parse(i_strIdAt) + 1;
			string strIdAt = idAt.ToString();
			for (int il = 0; il <= i_idAtm; il++)
			{
				if(i_strItemAts[il, 0] == strIdAt)
				{
					return i_strItemAts[il,2];
				}
			}
			return "";
		}

		// [取込]!="0"の小分類の全商品の属性をルールに従い変換する
		public static int checkAttr()
		{
			int ist = 0;						// ステイタス(<0:エラー)
			// 小分類マスタ
			string[] strHdr1 = { "小分類ID", "取込", "カテゴリID", "小分類名" };
			string[,] strItem1s = new string[Const.z_nSClassMax, strHdr1.Length];			// 全小分類のリスト
			int nSClass;																		// 小分類数
			// 属性名マスタ
			string[] strHdrAN = { "属性ID", "表示", "属性名", "表示属性名" };
			string[,] strItemANs = new string[Const.z_nSClassAttrNameMax, strHdrAN.Length];	// 小分類内の属性名のリスト 
			int nItemAN;																		// 属性名数
			// 商品マスタ
			string[] strHdr2 = { "商品ID", "取込", "表示", "商品名", "表示商品名", "安値", "評判", "評価", "発売時期" };
			string[,] strItem2s = new string[Const.z_nSClassSyohinMax, strHdr2.Length];		// 小分類内の商品のリスト
			int nSyohin;																		// 商品数
			// 属性値マスタ
			string[] strHdrAT = { "属性ID", "属性値", "表示属性値" };
			string[,] strItemATs = new string[Const.z_nSyohinAttrValueMax, strHdrAT.Length];	// 商品内の属性値のリスト 
			int nItemAT;																		// 属性値数
			// 商品検索用属性
			string[] strHdrAtSrch = { "最安価格", "売れ筋", "レビュー評価", "発売時期", "クチコミ件数", "注目" };
			int[] idAt = new int[strHdrAtSrch.Length];                                          // 属性名辞書の配列番号
			int idAtm = 0;
			string[] strHdrShnAtSrch = {  "安値", "評判", "評価", "発売時期" };

			// 全小分類を取得
			string strCond1 = "取込!=0";
			Db.MySql.getTableItems(Const.z_strSClass2Table, strHdr1, strHdr1.Length, strCond1, "", strItem1s, out nSClass);
			for (int ic1 = 0; ic1 < nSClass; ic1++)
			{
             //   if (strItem1s[ic1, 0] == "1230") continue;
             //   if (strItem1s[ic1, 0] == "2239") continue;

                strHdrAtSrch[1] = (strItem1s[ic1, 0] == "6201") ? "人気" : "売れ筋";
                strHdrAtSrch[5] = (strItem1s[ic1, 0] == "6201") ? "人気" : "注目";

                Mkdb.DispLine2(strItem1s[ic1, 3] + "の属性調査");

				// 小分類の[属性名]を取得
				string strCondAN = "小分類ID=" + strItem1s[ic1, 0];
				Db.MySql.getTableItems(Const.z_strAttributeNameTable, strHdrAN, strHdrAN.Length, strCondAN, "", strItemANs, out nItemAN);
				// 小分類の[表示属性名]の補正と表示OFFの設定
				modifyAttrName(strItem1s[ic1, 0], strHdrAN, strItemANs, nItemAN);
				// 属性ID→属性名の変換辞書を作成
				Dictionary<string, string> strDicAttrNms = new Dictionary<string, string>();
				for (int ic = 0; ic < nItemAN; ic++)
				{
					strDicAttrNms.Add(strItemANs[ic, 0], strItemANs[ic, 2]);
				}
				// 商品の検索用属性値の属性IDを取得
				int iat;
				for (iat = 0; iat < strHdrAtSrch.Length; iat++)
				{
					int id;
					idAt[iat] = 0;
					for (id=1; id<=nItemAN; id++)
					{
                        // if (strHdrAtSrch[iat] == strDicAttrNms[id.ToString()])
                        if ( strDicAttrNms[id.ToString()].IndexOf(strHdrAtSrch[iat]) == 0)
                        {
                            idAt[iat] = id - 1;
							break;
						}
					}
					if (Dbg.Utl.CheckError(idAt[iat] == 0, "小分類{" + strItem1s[ic1, 3] + "}に、属性名{" + strHdrAtSrch[iat] + "}が無い", "検索用属性設定エラー")) break;
					if (idAt[iat] > idAtm) idAtm = idAt[iat];
				}
				if (iat < strHdrAtSrch.Length)
				{
					break;
				}

				// 小分類内の商品を取得
				string strCond2 = "小分類ID=" + strItem1s[ic1, 0];
				Db.MySql.getTableItems(Const.z_strSyohin2Table, strHdr2, strHdr2.Length, strCond2, "", strItem2s, out nSyohin);
				for (int ic2 = 0; ic2 < nSyohin; ic2++)
				{
					//					if (strItem2s[ic2, 2] == "0") continue;     // 既に商品表示OFFの商品は前回までにチェック済みかつ表示しない商品でありさらなるチェックは無視

					bool bSyohinDisp1 = true;
					bool bSyohinDisp2 = true;

					int icngflg = 0;

					// 商品の一般属性値を取得
					string strCondAT = "商品ID=" + strItem2s[ic2, 0];
					Array.Clear(strItemATs, 0, strItemATs.Length);
					Db.MySql.getTableItems(Const.z_strAttributeValue2Table, strHdrAT, strHdrAT.Length, strCondAT, "", strItemATs, out nItemAT);

					// 商品の[表示属性値]を補正
					modifyDBAttrValue(strItem1s[ic1, 0], strItem2s[ic2, 0], strHdrAT, strItemATs, nItemAT, ref strDicAttrNms, out bSyohinDisp1);

					// 商品の表示Off判定
					if (bSyohinDisp1 && strItem2s[ic2, 2] != "0")
					{
						bSyohinDisp1 = checkSyohinDisp(strItem2s[ic2, 0], strItem2s[ic2, 3]);
					}
					// 表示商品名の修正
					string strSyohinName = strItem2s[ic2, 3];

					// "&amp;"の変換漏れを修正　　　　一時的なコーディング 14年4月末迄変換
					string strSyohinNameA = strItem2s[ic2, 3].Replace("&amp;", "&");
					if (strSyohinNameA != strSyohinName)
					{
//						Dbg.Utl.MessageBoxShow(strSyohinName, "属性値に変更ワーニング");
						strItem2s[ic2, 1] = "0";                // 取込
						strItem2s[ic2, 2] = "0";                // 表示
						strItem2s[ic2, 4] = strSyohinNameA;     // 表示商品名
						strSyohinName = strSyohinNameA;
						icngflg = 1;
					}

					int imodflg = modifyAttrValue(strItem1s[ic1, 0], "商品名", ref strSyohinName, out bSyohinDisp2);
					if (imodflg != 0 && strItem2s[ic2, 4] != strSyohinName)
					{
						strItem2s[ic2, 4] = strSyohinName;
						icngflg = 1;
					}
					// 商品の表示OFF設定
					if (bSyohinDisp1 && strItem2s[ic2, 2] != "0")
					{ 
						if (!bSyohinDisp1 || !bSyohinDisp2)
						{
							// 商品の[表示]を"0"に修正
							strItem2s[ic2, 2] = "0";
							icngflg = 1;
							string strMsg = "商品表示Off ID=" + strItem2s[ic2, 0] + ", 商品名 =" + strItem2s[ic2, 3];
							Mkdb.WriteLogFile(strMsg);
							Mkdb.DispLine2(strMsg);
						}
					}
					// 商品の"安値", "評判", "評価"の設定
					// 安値
					string strMinPriceW = strItemATs[idAt[0], 2];
					strMinPriceW = getDispAttr(strItemATs, idAt[0].ToString(), idAtm);
					string strMinPrice = "";
					if (strMinPriceW != "")
					{
						strMinPriceW = strMinPriceW.Substring(1);
						int iMinPrice = int.Parse(strMinPriceW, NumberStyles.AllowThousands);
						strMinPrice = iMinPrice.ToString();
					}
					// 評価
					string strHyoka = strItemATs[idAt[2], 2];
					strHyoka = getDispAttr(strItemATs, idAt[2].ToString(), idAtm);
					if (strHyoka == "-" || strHyoka == "")
					{
						strHyoka = "2.5";
					}
					float fHyoka = float.Parse(strHyoka);
					int iHyokaP = (int)Math.Floor((5.0 - float.Parse(strHyoka)) * 5.0 + 0.5);
					// クチコミ件数
					int iKutikomiN = Math.Min( getNum(strItemATs[idAt[4], 2]), Const.z_nKutikomiMax);
					string strKutikomiN = getDispAttr(strItemATs, idAt[4].ToString(), idAtm);
					iKutikomiN = Math.Min(getNum(strKutikomiN), Const.z_nKutikomiMax);
					int iKutikomiP = (Const.z_nKutikomiMax - iKutikomiN) * 10 / Const.z_nKutikomiMax;
					// 注目
					int iTyumokuN = getNum(strItemATs[idAt[5], 2]);
					string strTyumokuN = getDispAttr(strItemATs, idAt[5].ToString(), idAtm);
					iTyumokuN = getNum(strTyumokuN);
					if (iTyumokuN == 0 || iTyumokuN > Const.z_nTyumokuMax)
					{
						iTyumokuN = Const.z_nTyumokuMax;
					}

					// 評判(売れ筋)
					int iHyobanN = getNum(strItemATs[idAt[1], 2]);
					string strHyobanN = getDispAttr(strItemATs, idAt[1].ToString(), idAtm);
					iHyobanN = getNum(strHyobanN);
					if (iHyobanN == 0)
					{
						iHyobanN = 9999;
					}
					int iHyobanP = iHyobanN * 10 + iTyumokuN * 2 + iKutikomiP + iHyokaP;
					string strHyoban = iHyobanP.ToString();

					// 発売時期
					string strHatsubaiYMD = strItemATs[idAt[3], 2];
					strHatsubaiYMD = getDispAttr(strItemATs, idAt[3].ToString(), idAtm);

					if (strItem2s[ic2, 5] != strMinPrice || strItem2s[ic2, 6] != strHyoban ||
						strItem2s[ic2, 7] != strHyoka || strItem2s[ic2, 8] != strHatsubaiYMD)
					{
						strItem2s[ic2, 5] = strMinPrice;
						strItem2s[ic2, 6] = strHyoban;
						strItem2s[ic2, 7] = strHyoka;
						strItem2s[ic2, 8] = strHatsubaiYMD;
						icngflg = 1;
					}

					if (icngflg != 0)
					{
						// 商品データ修正
						if (Db.MySql.modifySingleCondRecordItems2D(Const.z_strSyohin2Table, strHdr2, strHdr2.Length,
																   "商品ID=" + strItem2s[ic2, 0], strItem2s, ic2) != 0)
						{
							Dbg.Utl.MessageBoxShow("データ修正エラー", "DBエラー");
						}
					}
				}   // Syohin
			}   // sClass
			Mkdb.DispLine2("属性変換終了");
			return ist;
		}

		// 文字列から先頭の数値のみ取り出す
		public static int getNum(string i_strNum)
		{
			int ir = 0;
			int i1;
			if (i_strNum == null || i_strNum == "")
			{
				return ir;
			}
			for (i1 = 0; i1 < i_strNum.Length && '0' <= i_strNum[i1] && i_strNum[i1] <= '9'; i1++);
			if (i1 > 0)
			{
				ir = int.Parse(i_strNum.Substring(0, i1));
			}
			return ir;
		}

		// 2次元文字列配列の1行を1次元文字列配列に取り出す
		public static void movStrings(string[,] i_strArys, int i_il, string[] o_strary)
		{
			for (int ic=0; ic<i_strArys.GetLength(1); ic++)
			{
				o_strary[ic] = i_strArys[i_il,ic];
			}
		}

		// 商品の表示offURL一覧(商品のメーカー製品情報URL一覧) {{属性ID}, 判定文字列}}
		// 属性ID==""の場合は全属性製品情報の属性IDを表す				現在""のみ可能 2014/03/13
		static string[,] m_strDispOffURLs = { { "", "kakaku.com" } };

		// 商品の表示判定
		// メーカー製品情報URLに判定文字列を含む商品は表示OFF
		public static bool checkSyohinDisp(
			string i_strSyohinId,				// 商品ID
			string i_strSyohinName				// 商品ID
		)
		{
			bool bSyohinDisp = true;
			// 商品毎の図形情報
			int nProductInfoURLMax = Const.z_nProductInfoMax;
			int[] idProductInfoAttrs = new int[nProductInfoURLMax];
			string[] strProductInfoURLs = new string[nProductInfoURLMax];
			int nProductInfoURL;
			// 商品のメーカー製品情報URLで判定
			getProductInfoURLs(i_strSyohinId, idProductInfoAttrs, strProductInfoURLs, out nProductInfoURL);
			for (int isyhz = 0; isyhz < nProductInfoURL; isyhz++)
			{
				for( int ic=0; ic<m_strDispOffURLs.GetLength(0); ic++)
				{
					if (strProductInfoURLs[ic].IndexOf(m_strDispOffURLs[ic,1]) >= 0)
					{
						bSyohinDisp = false;
					}
				}
			}
			if (bSyohinDisp) {
				if (i_strSyohinName.IndexOf("DOCOMO") >= 0)
				{
					bSyohinDisp = false;
				}
			}
			return bSyohinDisp;
		}

		// 商品の[表示属性値]を補正する
		private static int modifyDBAttrValue(
			string i_strSClassId,				// 小分類ID
			string i_strSyohinId,				// 商品ID
			string[] i_strHdrAT,				// 属性値データ項目名　{[属性ID],[属性値],[表示属性値]}
			string[,] io_strItemATs,			// 属性値データ
			int i_nItemAT,						// 属性値データ数
			ref Dictionary<string, string> i_strDicAttrNms,	// 属性ID → 属性名変換辞書
			out bool o_bSyohinDisp				// 商品表示フラグ	(false: 表示OFF)
			)
		{
			int ist = 0;
			int ic1;
			bool bSyohinDisp_rt;
			o_bSyohinDisp = true;
 			for (ic1 = 0; ic1 < i_nItemAT; ic1++)
			{
				string strAttrId = io_strItemATs[ic1, 0];
				if (i_strDicAttrNms.Count < int.Parse(strAttrId)) continue;
				string strAttrNm = i_strDicAttrNms[strAttrId];
				string strModifyAttrValue = io_strItemATs[ic1, 1];
				int icngflg = modifyAttrValue(i_strSClassId, strAttrNm, ref strModifyAttrValue, out bSyohinDisp_rt);
				o_bSyohinDisp = o_bSyohinDisp && bSyohinDisp_rt;

				// 仮コーディング　メーカーの属性値の最後の空白文字を取り除く(2014/01/30 当初属性値の取込みに考慮不足があり)
				// この変更がある場合は表示属性値の変更も必然的にあり、次の判定でDBに反映される
				if (strAttrNm == "メーカー")
				{
					io_strItemATs[ic1, 1] = io_strItemATs[ic1, 1].TrimEnd();
                }

				// 変更がある場合DBの対象属性値データを変換する
				if (icngflg != 0 && io_strItemATs[ic1, 2] != strModifyAttrValue)
				{
					io_strItemATs[ic1, 2] = strModifyAttrValue;
					// mySQLのデータを修正
					string strCond1 = "商品ID=";
					string strCond2 = "属性ID=";
					if (Db.MySql.modifyMultiCondRecordItems2D(Const.z_strAttributeValue2Table, i_strHdrAT, i_strHdrAT.Length,
															  strCond1, i_strSyohinId,
															  strCond2, io_strItemATs[ic1, 0], io_strItemATs, ic1) != 0)
					{
						Dbg.Utl.MessageBoxShow("データ修正エラー", "DBエラー");
					}
				}
			}
			return ist;
		}

		// 対象外の商品であるか調べる
		private static bool chkDispOffSyohin(
			string[] i_astrName,                    // 属性名リスト
			string[] i_astrValue,                   // 属性値リスト
			int i_nItemAN                           // 属性数
			)
		{
			int ic0, ic1, ic2;
			bool fDisp = true;

			for (ic0 = 0; ic0 < i_nItemAN; ic0++)
			{
				for (ic1 = 0; ic1 < m_strCngAttrTable.GetLength(0); ic1++)
				{
					if (m_strCngAttrTable[ic1][0] == "2")
					{
						// 属性値から文字列を取り除く
						for (ic2 = 2; ic2 < m_strCngAttrTable[ic1].Length; ic2++)
						{
							if (i_astrValue[ic0].IndexOf(m_strCngAttrTable[ic1][ic2]) >= 0)
							{
								fDisp = false;
								break;
							}
						}
					}
					if (!fDisp) break;
				}
				if (!fDisp) break;
			}
			return fDisp;
		}

		// 属性値に不要な文字列を含んでいるか調べログ出力する
		private static bool logNgWordInAttr(
			string[] i_astrValue,                   // 属性値リスト
			int i_nItemAN                           // 属性数
			)
		{
			bool bst = true;
			for (int iz = 0; iz < i_nItemAN; iz++)
			{
				if (i_astrValue[iz].IndexOf("価格") >= 0)
				{
					bst = false;
					string strErr = "属性値に価格を含んでいます: ";
					strErr += ("メーカー: " + i_astrValue[0] + " 商品名: " + i_astrValue[1]);
					strErr += ("属性値:" + i_astrValue[iz]);
					Mkdb.WriteLogFile(strErr);
				}
			}
			return bst;
		}

		// 属性値を補正
		// 属性値を属性値変換表の属性名に対応した変換タイプに従って変換する
		private static int modifyAttrValue(						// ステイタス 0: 変更無し、1: 変更有り
			string i_strSClassId,								// 小分類ID
			string i_strAttrName,								// 属性名
			ref string io_strAttrValue,							// 属性値
			out bool o_bsyohindisp								// 表示対象商品　(false:表示OFF)
			)
		{
			int ic1, ic2;

			int[] mrmprc = {100, 200, 500, 1000, 2000, 5000,
							10000, 20000, 50000, 100000, 200000, 500000,
							1000000, 2000000, 5000000, 10000000, 20000000, 50000000};
			int[] mrmtbl = {1,
							1, 1, 1, 2, 5, 10,
							20, 50, 100, 200, 500, 500,
							1000, 1000, 5000, 5000, 10000, 10000};
			o_bsyohindisp = true;
			int icngflg = 0;
			string strCngType = null;			// 変換タイプ
			// 属性値変換表の属性名に対応した変換タイプに従い、属性値を変換する
			for (ic1 = 0; ic1 < m_strCngAttrTable.GetLength(0); ic1++)
			{
                strCngType = m_strCngAttrTable[ic1][0];
                if (strCngType == "1")
                {
                    if (i_strAttrName.IndexOf(m_strCngAttrTable[ic1][1]) != 0)
                    {
                        continue;
                    }
                } else { 
                    if (m_strCngAttrTable[ic1][1] != "" && i_strAttrName != m_strCngAttrTable[ic1][1])
                    {
                        continue;
                    }
				}

				if (strCngType == "1")
				{
					// 最安価格　価格部分のみ取り出し
					string strAttrValue = io_strAttrValue;
					string strPrice = "";
					int ic;
					// 値段部分の文字列を取り出し値段以降の文字列を捨てる
					for (ic = 0; ic < strAttrValue.Length; ic++)
					{
						if (strAttrValue[ic] == '￥' || strAttrValue[ic] == '\\' ||
							strAttrValue[ic] == ',')
						{ }
						else if ('0' <= strAttrValue[ic] && strAttrValue[ic] <= '9')
						{
							strPrice += strAttrValue[ic];
						}
						else
						{
							break;
						}
					}
                    if (strPrice == "")
                    {
                        strPrice = "0";
                    }
					icngflg = 1;

					// 直近安値を求める
					int minPrice = (int)(int.Parse(strPrice) * Const.z_fMinKakakuHosei);
					for (ic = 0; ic < 18; ic++)
					{
						if (minPrice < mrmprc[ic])
						{
							break;
						}
					}
					int marume = mrmtbl[ic];
					minPrice = (int)Math.Ceiling((double)minPrice / marume) * marume;

					io_strAttrValue = String.Format("￥{0:#,0}", minPrice);
				}
				else if (strCngType == "2")
				{
					// 属性値から文字列を取り除く
					for (ic2 = 2; ic2 < m_strCngAttrTable[ic1].Length; ic2++)
					{
						string strCng0 = m_strCngAttrTable[ic1][ic2];
						string strCng2 = " " + m_strCngAttrTable[ic1][ic2] + " ";

						if (io_strAttrValue.IndexOf(strCng0) >= 0)
						{
							io_strAttrValue = io_strAttrValue.Replace(strCng2, " ");
							io_strAttrValue = io_strAttrValue.Replace(strCng0, "");
							icngflg = 1;
							o_bsyohindisp = false;
						}
					}
				}
				else if (strCngType == "3")
				{
					// 属性値から文字列以降を取り除く
					for (ic2 = 2; ic2 < m_strCngAttrTable[ic1].Length; ic2++)
					{
						int ic = io_strAttrValue.IndexOf(m_strCngAttrTable[ic1][ic2]);
						if (ic >= 0)
						{
							io_strAttrValue = io_strAttrValue.Substring(0, ic);
							icngflg = 1;
						}
					}
				}
				else if (strCngType == "4")
				{
					// 属性値を変更する
					for (ic2 = 2; ic2 < m_strCngAttrTable[ic1].Length-1; ic2+=2)
					{
						int ic = io_strAttrValue.IndexOf(m_strCngAttrTable[ic1][ic2]);
						if (ic >= 0)
						{
							io_strAttrValue = io_strAttrValue.Replace(m_strCngAttrTable[ic1][ic2], m_strCngAttrTable[ic1][ic2+1]);
							icngflg = 1;
						}
					}
				}
				else if (strCngType == "5")
				{
					// 最後の文字を取り除く
					for (ic2 = 2; ic2 < m_strCngAttrTable[ic1].Length; ic2++)
					{
						int ic = io_strAttrValue.Length;
						if (io_strAttrValue[ic-1] == m_strCngAttrTable[ic1][ic2][0])
						{
							io_strAttrValue = io_strAttrValue.Substring(0, ic-1);
							icngflg = 1;
						}
					}	
				}
				else if (strCngType == "6")
				{
					// メモリーの商品名の属性値を変更する　DDR2 PCdddd → DDR2 PC2-dddd, DDR3 PCdddd → DDR2 PC3-dddd
					if (i_strSClassId != m_strCngAttrTable[ic1][2])
					{
						continue;				// メモリーでない
					}

					for (ic2 = 3; ic2 < m_strCngAttrTable[ic1].Length; ic2++)
					{
						int id1 = io_strAttrValue.IndexOf(m_strCngAttrTable[ic1][ic2]);
						if (id1 >= 0)
						{
							int id2 = io_strAttrValue.IndexOf("PC", id1 + 1);
							int idn = io_strAttrValue.Length;
							if (id2 >= 0 && id2 + 6 <= idn)
							{
								if (io_strAttrValue[id2 + 2] >= '0' && io_strAttrValue[id2 + 2] <= '9' &&
									io_strAttrValue[id2 + 3] >= '0' && io_strAttrValue[id2 + 3] <= '9' &&
									io_strAttrValue[id2 + 4] >= '0' && io_strAttrValue[id2 + 4] <= '9' &&
									io_strAttrValue[id2 + 5] >= '0' && io_strAttrValue[id2 + 5] <= '9')
								{
									io_strAttrValue = io_strAttrValue.Substring(0, id2 + 2) + m_strCngAttrTable[ic1][ic2][3] +
													  "-" + io_strAttrValue.Substring(id2 + 2, idn - id2 - 2);
									icngflg = 1;
								}
							}
						}
					}
				}
                else if (strCngType == "7")
                {
                    // 属性値を正規表現で検索し変更する
                    for (ic2 = 2; ic2 < m_strCngAttrTable[ic1].Length - 1; ic2 += 2)
                    {
                        Regex reg = new Regex(m_strCngAttrTable[ic1][ic2]);
                        string strAttrValue1 = reg.Replace(io_strAttrValue, m_strCngAttrTable[ic1][ic2 + 1]);
                        if (strAttrValue1 != io_strAttrValue)
                        {
                            Regex regSp1 = new Regex(" $");
                            string strAttrValue2 = regSp1.Replace(strAttrValue1, m_strCngAttrTable[ic1][ic2 + 1]);
                            Regex regSp2 = new Regex("　$");
                            io_strAttrValue = regSp2.Replace(strAttrValue2, m_strCngAttrTable[ic1][ic2 + 1]);
                            icngflg = 1;
                        }
                    }
                }
            }
            return icngflg;
		}

		// 表示OFF/ON属性名一覧	{[属性名],[表示]}n
		static string[,] m_strSetDispAttrNms = { { "注目", "0" }, { "売れ筋", "0" }, { "クチコミ件数", "0" }, { "登録日", "0" } };

		// 属性名変換表			{[属性名], [表示属性名]}n
		static string[,] m_strCngAttrNms = {{"最安価格", "参考安値"}, {"レビュー評価", "評価"}};

		// 属性値変換表
		// {変換タイプ(1～3,5), 変換する属性名, 変換文字列1, ･･･････}
		// {変換タイプ(4), 変換する属性名, 変換文字列1_from, 変換文字列1_to, 変換文字列2_from, 変換文字列2_to, ･･･････}
		// {変換タイプ(6), 変換する属性名, 小分類ID, 変換文字列1, 変換文字列2}
		//
		// 変換する属性名=="" の 場合は、全属性を対象とする
		//
		// "1": 文字列から価格部分のみを抽出
		// "2": 文字列から変換文字列を取り除く		(商品の表示OFF対象)
		// "3": 文字列から変換文字列以降を取り除く
		// "4": 文字列の変換文字列部分を入れ替える
		// "5": 文字列の最後の指定文字を取り除く
		// "6": メモリーの商品名の属性値を変更する　(DDR2 PCdddd → DDR2 PC2-dddd, DDR3 PCdddd → DDR2 PC3-dddd)
        // "7": 文字列の正規表現変換文字列部分を入れ替える
		static string[][] m_strCngAttrTable = {
												 new string[] {"1", "最安価格"},
                                                 new string[] {"2", "商品名", "価格.com限定", "価格.com 限定", "価格.com","限定モデル"},
                                                 new string[] {"2", "メーカー", "【直販モデル】", "直販モデル", "直販"},
												 new string[] {"3", "", "絞り込む"},
												 new string[] {"3", "レビュー評価", "("},
												 new string[] {"3", "SIM情報", "格安SIMを見る"},
												 new string[] {"4", "メーカー", "メーカー問わず", "-"},
												 new string[] {"4", "ドアの開き方", "フレンチドア(観音開き)", "観音開き"},
												 new string[] {"5", "メーカー", "　"},
												 new string[] {"6", "商品名", "1259", "DDR2", "DDR3"},
                                                 new string[] {"7", "商品名", "2[0-9]{3}年.*モデル", ""}
											 };

		// 小分類の[表示属性名]の変換と[表示]OFF/ONの設定
		private static int modifyAttrName(
			string i_strSClassId,				// 小分類ID
			string[] i_strHdrAN,				// 属性名データ項目	{[属性ID],[表示],[属性名],[表示属性名]}
			string[,] io_strItemANs,			// 属性名データ
			int i_nItemAN						// 属性名データ数
			)
		{
			int ist = 0;						// ステイタス(<0:エラー)
			int ic1, ic2, ic3;
			int icngflg = 0;
			for (ic1 = 0; ic1 < i_nItemAN; ic1++)
			{
				// 属性名変換表の[属性名]を持つ属性名データの[属性名]を変換し[表示属性名]に設定する
				for (ic2=0; ic2 < m_strCngAttrNms.GetLength(0); ic2++)
				{
                    if (io_strItemANs[ic1, 2].LastIndexOf(m_strCngAttrNms[ic2, 0])>=0)
                    {
                        if (io_strItemANs[ic1, 3] != m_strCngAttrNms[ic2, 1])
						{
							io_strItemANs[ic1, 3] = m_strCngAttrNms[ic2, 1];
							icngflg = 1;
						}
						break;
					}
				}
				// 表示OFF/ON属性名一覧の[属性名]を持つ属性名データの[表示]を表示OFF("0")または表示ON("1"),表示ON("2")に設定する
				for (ic3 = 0; ic3 < m_strSetDispAttrNms.GetLength(0); ic3++)
				{
					if (io_strItemANs[ic1, 2] == m_strSetDispAttrNms[ic3, 0])
					{
						if (io_strItemANs[ic1, 1] != m_strSetDispAttrNms[ic3, 1])
						{
							io_strItemANs[ic1, 1] = m_strSetDispAttrNms[ic3, 1];
							icngflg = 1;
						}
						break;
					}
				}
				// 変更がある場合DBの対象属性名データを変換する
				if ( icngflg != 0)
				{
					// mySQLのデータを修正
					string strCond1 = "小分類ID=";
					string strCond2 = "属性ID=";
					if (Db.MySql.modifyMultiCondRecordItems2D(Const.z_strAttributeNameTable, i_strHdrAN, i_strHdrAN.Length,
															  strCond1, i_strSClassId,
															  strCond2, io_strItemANs[ic1, 0], io_strItemANs, ic1) != 0)
					{
						Dbg.Utl.MessageBoxShow("データ修正エラー", "DBエラー");
					}
				}
			}
			return ist;
		}

		// 小分類の全商品の変動項目([売れ筋],[レビュー評価],[口コミ件数])を0クリア
		public static int clrMovementAttrValue(string i_strSClassID)
		{
			int ic1, ic2;

			// 属性名マスタ
			string[] strHdrAN = { "属性ID", "属性名"};
			string[,] strItemANs = new string[Const.z_nAttrNameMax, strHdrAN.Length];		// 小分類内の属性名の[属性ID],[表示],[属性名],[表示属性名]のリスト 
			int nItemAN;																		// 属性名数

			string[] strAttrNms = { "注目", "売れ筋", "レビュー評価", "クチコミ件数"};
			string[] stAttrIDs = new string[strAttrNms.Length];
			int nAttrID;

			// 属性値マスタ
			string[] strHdrAT = { "属性値", "表示属性値" };
			string[] strItemAT = { "0", "0" };

			// 属性名を取得
			string strCond1 = "小分類ID=" + i_strSClassID;
			Db.MySql.getTableItems(Const.z_strAttributeNameTable, strHdrAN, strHdrAN.Length, strCond1, "", strItemANs, out nItemAN);
			nAttrID = 0;
			for (ic1 = 0; ic1 < strAttrNms.Length; ic1++)
			{
				for (ic2=0; ic2<nItemAN; ic2++)
				{
					if (strAttrNms[ic1] == strItemANs[ic2,1])
					{
						break;
					}
				}
				if (ic2 < nItemAN)
				{
					stAttrIDs[nAttrID] = strItemANs[ic2, 0];
					nAttrID++;
				}
			}
			if (nAttrID >= 1)
			{
				// 小分類の商品IDを取得 
				int iszSyohinID = Const.z_nSClassSyohinMax;
				string[] strSyohinID = new string[iszSyohinID];
				int nSyohinID;
				Mkdb.getSyohinIDofSClass(i_strSClassID, strSyohinID, iszSyohinID, out nSyohinID);
				if (Dbg.Utl.CheckOverflow(nSyohinID >= iszSyohinID, "小分類内の商品数", "clrMovementAttrValue")) return -1;
				// 小分類の全商品の変動項目([売れ筋],[レビュー評価],[口コミ件数])を0クリア 
				//for (ic1 = 0; ic1 < nSyohinID; ic1++)
				//{
				//	string strCond = "商品ID=" + strSyohinID[ic1] + " AND (属性ID=" + stAttrIDs[0];
				//	for (ic2 = 1; ic2 < nAttrID; ic2++)
				//	{
				//		// mySQLのデータを修正
				//		strCond += (" OR 属性ID=" + stAttrIDs[ic2]);
				//	}
				//	strCond += ")";
				//	if (Db.MySql.modifySingleCondRecordItems1D(Const.z_strAttributeValueTable, strHdrAT, strHdrAT.Length,
				//											strCond, strItemAT) != 0)
				//	{
				//		Dbg.Utl.MessageBoxShow("データ修正エラー", "DBエラー");
				//	}
				//}
				const int icadd = 20; 
				for (ic1 = 0; ic1 < nSyohinID; ic1 += icadd)
				{
					string strCond = "(商品ID=" + strSyohinID[ic1];
					int ica = (nSyohinID - ic1 < icadd) ? nSyohinID - ic1: icadd;
					for (int ic = 0; ic < ica; ic++)
					{
						strCond += (" OR 商品ID=" + strSyohinID[ic1 + ic]);
					}
					strCond += ")";
					strCond += (" AND (属性ID=" + stAttrIDs[0]);
					for (ic2 = 1; ic2 < nAttrID; ic2++)
					{
						strCond += (" OR 属性ID=" + stAttrIDs[ic2]);
					}
					strCond += ")";
					// mySQLのデータを修正
					if (Db.MySql.modifySingleCondRecordItems1D(Const.z_strAttributeValue2Table, strHdrAT, strHdrAT.Length,
															   strCond, strItemAT) != 0)
					{
						Dbg.Utl.MessageBoxShow("データ修正エラー", "DBエラー");
					}
				}
			}
			return 0;
		}
		// 属性名の[表示]フラグと表示属性名を保存する
		public static int saveAttrNameDispFlag(string i_strSClassID, string[] i_strHdrAN,
											   string[,] o_strItemANs, out int o_nItemAN)
		{
			int ist;

			// 属性名マスタ
			// string[] strHdrAN = { "属性ID", "表示", "属性名", "表示属性名", "表示巾" };
			// string[,] strItemANs = new string[Const.z_nSClassAttrNameMax, strHdrAN.Length];	// 小分類内の属性名の[属性ID],[表示],[属性名],[表示属性名]のリスト 
			// int nItemAN;																			// 属性名数

			// 小分類の[属性名]を取得
			string strCond = "小分類ID=" + i_strSClassID;
			ist = Db.MySql.getTableItems(Const.z_strAttributeNameTable, i_strHdrAN, i_strHdrAN.Length,
										 strCond, "", o_strItemANs, out o_nItemAN);

			return ist;
		}
		// 属性名の[表示]フラグと表示属性名を再設定する
		public static int loadAttrNameDispFlag(string i_strSClassID, string[] i_strHdrAN,
											   string[,] i_strItemANs, int i_nItemAN)
		{
			int ist;

			// 属性名マスタ
			string[,] strItemANs = new string[Const.z_nSClassAttrNameMax, i_strHdrAN.Length];	// 小分類内の属性名の[属性ID],[表示],[属性名],[表示属性名]のリスト 
			int nItemAN;																			// 属性名数

			// 小分類の[属性名]を取得
			string strCond = "小分類ID=" + i_strSClassID;
			ist = Db.MySql.getTableItems(Const.z_strAttributeNameTable, i_strHdrAN, i_strHdrAN.Length,
										 strCond, "", strItemANs, out nItemAN);

			// 表示フラグと表示属性名のマージ
			for (int ic1 = 0; ic1 < i_nItemAN; ic1++)
			{
				for (int ic2=0; ic2 < nItemAN; ic2++)
				{
					if (strItemANs[ic2,2] == i_strItemANs[ic1,2])
					{
						if ( strItemANs[ic2, 1] != i_strItemANs[ic1, 1] ||
							 strItemANs[ic2, 3] != i_strItemANs[ic1, 3] ||
							 strItemANs[ic2, 4] != i_strItemANs[ic1, 4] )
						{
							strItemANs[ic2, 1] = i_strItemANs[ic1, 1];
							strItemANs[ic2, 3] = i_strItemANs[ic1, 3];
							strItemANs[ic2, 4] = i_strItemANs[ic1, 4];

							string strCond1 = "小分類ID=";
							string strCond2 = "属性ID=";
							ist = Db.MySql.modifyMultiCondRecordItems2D(Const.z_strAttributeNameTable, i_strHdrAN, i_strHdrAN.Length,
																		strCond1, i_strSClassID, strCond2, strItemANs[ic2, 0],
																		strItemANs, ic2);
						}
						break;
					}
				}
			}
			return 0;
		}

		// 削除図形のURLを追加する
		public static int InsDeleteFigURL(string[] i_strFilePaths)
		{
			int ist = 0;

			// 図形フォルダ
			string strZukeiDir;
			strZukeiDir = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1];			// 図形データ

			// 制御マスタ
			string[] strHdr1 = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
			string[] strItem1s = new string[6];
			string[] strHdr2 = { "制御ID", "引数1" };
			string[,] strItem2s = new string[Const.z_nCtrlIDMax, strHdr2.Length];	// 制御マスタの制御IDのリスト
			int nItem2;

			// 制御マスタのレコードを取得し最大制御IDを求める
			string strCond2 = "制御TP=3";
			Db.MySql.getTableItems(Const.z_strControlTable, strHdr2, strHdr2.Length,
								   strCond2, "", strItem2s, out nItem2);
			int nMaxCtrlID = 0;
			for (int ic = 0; ic < nItem2; ic++)
			{
				string strCtrlID = strItem2s[ic,0];
				int idCtrl = int.Parse(strCtrlID);
				if (nMaxCtrlID < idCtrl)
				{
					nMaxCtrlID = idCtrl;
				}
			}

			foreach (string strFilePath in i_strFilePaths)
			{
				// 図形のファイル名から、商品ID, 属性ID, URLを取得する
				
				string strFileName = Path.GetFileName(strFilePath);
				if ((strFileName[0] != 'z' && strFileName[0] != 'Z') ||
					strFileName.Substring(11,2) != "-S" || strFileName.Length < 15)
				{
					continue;									// 対象外のファイル名
				}
				// 商品ID
				string strSyohinID10 = strFileName.Substring(1, 10);
				if (strSyohinID10.Substring(0,5) == "00000")
				{
					continue;									// 既に削除済み
				}
				// 属性ID
				string strAttrID = "2" + strFileName.Substring(13, 3);

				// URL型属性値マスタからURLを取得
				string[] strHdrURL = { "属性ID", "URL", "図形CD" };
				string[] strItemURLs = new string[3];				// 全URL型属性値の[属性ID],[URL],[図形CD]のリスト
				string strCond = "商品ID=" + strSyohinID10 + " AND 属性ID=" + strAttrID;
				Db.MySql.getRecordItems(Const.z_strAttributeURL2Table, strHdrURL, strHdrURL.Length, strCond, strItemURLs);
				string strZukeiURL = strItemURLs[1];

				if (strZukeiURL != null && strZukeiURL != "")
				{
					// 削除URLのファイル名を登録する
					string strPhotoFileName = Path.GetFileName(strZukeiURL);
					if (strPhotoFileName.Length <= 3)
					{
						continue;									// 登録対象外のURLのファイル名
					}
					// 制御マスターに登録するURLファイル名はファイル名の直前の5文字(dir部分)を追加したものを登録
					int iUrlFileC = strZukeiURL.IndexOf(strPhotoFileName);
					int iPFNlng = strPhotoFileName.Length;
					if (iPFNlng < 15 && iUrlFileC >= 10)
					{
						strPhotoFileName = strZukeiURL.Substring(iUrlFileC - 5);
					}

					int ic;
					for (ic = 0; ic < nItem2; ic++)
					{
						string strCtrlURLFileName = strItem2s[ic, 1];
						if (strPhotoFileName == strCtrlURLFileName)
						{
							break;
						}
					}
					if (ic == nItem2)
					{
						// 未登録のURLのファイル名であり登録する

						string strNewZukeiCD = "Z" + "0000000003";
						nMaxCtrlID++;

						// 制御マスタに削除URLを追加する
						// 制御マスタ
						// string[] strHdr1 = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
						strItem1s[0] = "3";
						strItem1s[1] = nMaxCtrlID.ToString();
						strItem1s[2] = strPhotoFileName;
						strItem1s[3] = strNewZukeiCD;
						Db.MySql.insRecord(Const.z_strControlTable, strItem1s, strHdr1.Length);

						strItem2s[nItem2, 0] = nMaxCtrlID.ToString();
						strItem2s[nItem2, 1] = strPhotoFileName;
						nItem2++;
					}
				}
				// 図形を削除フォルダに移動する
				string strToFolder = strZukeiDir + "/DELETE/" + "Z" + strSyohinID10 + strFileName.Substring(11);

				Mkdb.MoveFile(strFilePath, strToFolder);
			}
			return ist;
		}
		// テキストボックスに入力された削除図形のURLをcontrolテーブルに追加する
		public static int InsCtrlUrlFN(string i_strType, string i_strstrCtrlUrlFileName)
		{
			int ist = 0;

			// 制御マスタ
			string[] strHdr1 = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
			string[] strItem1s = { i_strType, "", "", "", "", "" };
			string[] strHdr2 = { "制御ID", "引数1" };
			string[,] strItem2s = new string[Const.z_nCtrlIDMax, strHdr2.Length];	// 制御マスタの制御IDのリスト
			int nItem2;

			// 制御マスタのレコードを取得し最大制御IDを求める
			string strCond2 = "制御TP=" + i_strType;
			Db.MySql.getTableItems(Const.z_strControlTable, strHdr2, strHdr2.Length,
								   strCond2, "", strItem2s, out nItem2);
			int nMaxCtrlID = 0;
			for (int ic = 0; ic < nItem2; ic++)
			{
				string strCtrlID = strItem2s[ic, 0];
				int idCtrl = int.Parse(strCtrlID);
				if (nMaxCtrlID < idCtrl)
				{
					nMaxCtrlID = idCtrl;
				}
			}

			// 未登録のURLのファイル名であり登録する

			string strNewZukeiCD = "Z" + "000000000" + i_strType;
			nMaxCtrlID++;

			// 制御マスタに削除URLを追加する
			// 制御マスタ
			// string[] strHdr1 = { "制御TP", "制御ID", "引数1", "引数2", "引数3", "引数4" };
			strItem1s[1] = nMaxCtrlID.ToString();
			strItem1s[2] = i_strstrCtrlUrlFileName;
			strItem1s[3] = strNewZukeiCD;
			Db.MySql.insRecord(Const.z_strControlTable, strItem1s, strHdr1.Length);
			return ist;
		}

		// サイトマップを作成する
		public static int SiteMap(int i_itype, out int[] o_icntPri, out string[] o_strTtl, out string[] o_strPri)
		{
			// double RestrictNum = 0.6065;
			double RestrictNum = 0.07660;
			if (i_itype == 0)
			{
				// RestrictNum = 0.18831;
				RestrictNum = 0.12200;
			}
			else
			{
				RestrictNum = 0.99999;
			}

			int ist = 0;

			Sitemap.Open(i_itype);
			Sitemap.Write_Head_Urlset();
			string strHPUrl = Const.z_strHPUrl[i_itype] + "/";
			//	strDay 形式 "2014-03-01T05:57:38+09:00"
			DateTime DateC = DateTime.Now;
			string strDay = DateC.ToString("s") + "+09:00";


			string strCngFreq = "weekly";
			// string[] strPri = { "1.00", "0.80", "0.64", "0.51", "0.41", "0.33", "0.26"  };
			string[] strPri = { "1.00", "0.80", "0.80", "0.64", "0.51", "0.41", "0.33" };	// 直接商品からの検索が多いので優先順を変更
			string[] strTtl = { "Top", "小分類", "商品1", "商品2", "商品3", "商品4", "商品5" }; 

			o_icntPri = new int[strPri.Length];												// 優先順別のHPカウンタ

			// sitemapにトップページを書き込む
			Sitemap.Write_Url(strHPUrl, strDay, strCngFreq, strPri[0]);
			o_icntPri[0]++;

			o_strTtl = new string[strPri.Length];
			strTtl.CopyTo(o_strTtl, 0);
			o_strPri = new string[strPri.Length];
			strPri.CopyTo(o_strPri, 0);

			Mkdb.DispLine2("商品HP作成開始");
			// 基準小分類HPのパス
			string strOrigCategoryHP = Const.z_pathCategoryBaseHP[Form1.m_iradioBtnDB - 1] + "s000001.php";
			// 基準商品HPのパス
			string strOrigSyohinHP = Const.z_pathSyohinBaseHP[Form1.m_iradioBtnDB - 1] + "p0000000001.php";

			// カテゴリマスタ
			string[] strHdr1 = { "カテゴリID", "表示", "表示カテゴリURL", "表示名称" };
			string[,] strItem1s = new string[Const.z_nCategoryMax, strHdr1.Length];		// カテゴリのリスト
			int nSCategory;																	// カテゴリ数

			// 大分類マスタ
			string[] strHdr2 = { "大分類ID", "表示", "表示大分類名" };
			string[,] strItem2s = new string[Const.z_nLClassMax, strHdr2.Length];		// カテゴリ内の大分類のリスト
			int nLClass;																	// 大分類数

			// 中分類マスタ
			string[] strHdr3 = { "中分類ID", "表示", "表示中分類名" };
			string[,] strItem3s = new string[Const.z_nMClassMax, strHdr3.Length];		// 大分類内の中分類のリスト
			int nMClass;																	// 中分類数

			// 小分類マスタ
			string[] strHdr4 = { "A.小分類ID", "A.取込", "A.表示小分類名" };
			string[,] strItem4s = new string[Const.z_nSClassMax, strHdr4.Length];		// 中分類内の小分類のリスト
			int nSClass;																	// 小分類数

			// 商品マスタ
			string[] strHdr5 = { "商品ID", "表示", "表示商品名", "評判" };
			string[,] strItem5s = new string[Const.z_nSyohinMax, strHdr5.Length];		// 小分類内の商品のリスト
			int nSyohin;																	// 商品数

			// トレースファイルをオープン

			// カテゴリを取得
			string strCond1 = "表示!='0'";
			Db.MySql.getTableItems(Const.z_strCategoryTable, strHdr1, strHdr1.Length, strCond1, "",
								   strItem1s, out nSCategory);
			// トレース用
			string strOut = "";

			for (int ic1 = 0; ic1 < nSCategory; ic1++)
			{
				int iCategoryID = int.Parse(strItem1s[ic1, 0]);
				string strCategoryURL = Const.z_strHPUrl[i_itype] + strItem1s[ic1, 2];

				//// sitemapにカテゴリページを書き込む
				//Sitemap.Write_Url(strCategoryURL, strDay, strCngFreq, strPri[1]);
				//o_icntPri[1]++;

				// 大分類を取得
				string strCond2 = "カテゴリID=" + strItem1s[ic1, 0].ToString() + " AND 表示!='0'";
				Db.MySql.getTableItems(Const.z_strLClassTable, strHdr2, strHdr2.Length, strCond2, "",
									   strItem2s, out nLClass);
				for (int ic2 = 0; ic2 < nLClass; ic2++)
				{
					// 中分類を取得
					string strCond3 = "大分類ID=" + strItem2s[ic2, 0].ToString() + " AND 表示!='0'";
					Db.MySql.getTableItems(Const.z_strMClassTable, strHdr3, strHdr3.Length, strCond3, "",
										   strItem3s, out nMClass);
					for (int ic3 = 0; ic3 < nMClass; ic3++)
					{

						// 中分類の小分類IDと取込を取得
						string strTable4 = Const.z_strSClass2Table + " A, " +
										   Const.z_strMSCRelation + " B";
						string strCond4 = "B.中分類ID=" + strItem3s[ic3, 0].ToString() +
										  " AND " + "A.小分類ID=B.小分類ID" + " AND A.取込!='0'";
						Db.MySql.getTableItems(strTable4, strHdr4, strHdr4.Length, strCond4, "",
											   strItem4s, out nSClass);
						for (int ic4 = 0; ic4 < nSClass; ic4++)
						{
							string strSClassID6;
							int nSClassID;

							int iCategoryN = int.Parse(strItem4s[ic4,0]) / 1000;
							if (iCategoryN != iCategoryID)
							{
								continue;
							}

							Mkdb.DispLine2(strItem4s[ic4, 2] + "のHP作成");
							// 小分類HPの<URL>作成
							// 0パディングした6桁の小分類IDを求める
							strSClassID6 = "00000" + strItem4s[ic4, 0];
							nSClassID = strSClassID6.Length;
							strSClassID6 = strSClassID6.Substring(nSClassID - 6);

							// sitemapに小分類ページを書き込む
							string strSClassUrl = strCategoryURL + "s" + strSClassID6 + ".php";
							Sitemap.Write_Url(strSClassUrl, strDay, strCngFreq, strPri[1]);
							o_icntPri[1]++;

							// Homepageのサイトマップを作成
							// 小分類内の全商品の[商品ID]と[表示]を取得
							string strCond5 = "小分類ID=" + strItem4s[ic4, 0] + " AND 表示!='0'";
							string strCondo = "`評判`";
							Db.MySql.getTableItems(Const.z_strSyohin2Table, strHdr5, strHdr5.Length,
												   strCond5, strCondo, strItem5s, out nSyohin);
							// 商品数からPRI毎の商品HPの<URL>作成数を求める
							int[] nUrlPerPri = new int[7];
							int nSzUPP;

							int iType = 2;
							FindNofUrlPerPri(iType, nSyohin, RestrictNum, nUrlPerPri, out nSzUPP);

							// トレース
							strOut += (iType + " " + strSClassID6 + " " + String.Format("{0, 6}", nSyohin + " : "));
							for (int jc = 0; jc < nSzUPP; jc++)
							{
								strOut += String.Format("{0, 6}", nUrlPerPri[jc]);
							}
							strOut += "\r\n";

							for (int ic5 = 0; ic5 < nSyohin; ic5++)
							{
								// 0パディングした10桁の商品IDを求める
								string strSyohinID10 = Mkdb.ConvertSyohinID10(strItem5s[ic5, 0]);

								// url用の下位互換の商品IDを求める(追番が4桁以下の場合は9桁のコード)
								string strSyohinIDUrl;
								if (strSyohinID10.Substring(5,1) == "0")
								{
									strSyohinIDUrl = strSyohinID10.Substring(0, 5) + strSyohinID10.Substring(6, 4);
								}
								else
								{
									strSyohinIDUrl = strSyohinID10;
								}

								// sitemapに商品ページを書き込む
								string strSyohinUrl = strCategoryURL + "p" + strSyohinIDUrl + ".php";
								int ipri;
								if (ic5 < nUrlPerPri[2])
								{
									ipri = 2;
								}
								else if (ic5 < nUrlPerPri[3])
								{
									ipri = 3;
								}
								else if (ic5 < nUrlPerPri[4])
								{
									ipri = 4;
								}
								else if (ic5 < nUrlPerPri[5])
								{
									ipri = 5;
								}
								else if (ic5 < nUrlPerPri[6])
								{
									ipri = 6;
								}
								else
								{
									break;
								}
								Sitemap.Write_Url(strSyohinUrl, strDay, strCngFreq, strPri[ipri]);
								o_icntPri[ipri]++;
							}
						}
					}
				}
			}
			Sitemap.Write_Tail_Urlset();
			Sitemap.Close();

			Mkdb.DispLine2("SiteMap作成終了");
			// トレース出力
			MessageBox.Show(strOut);

			return ist;
		}
		// 商品数からPRI毎の商品HPの<URL>作成数を求める
		/*
								if (ic5<nSyohin / 25 && ic5 <= 100)	ipri = 2;
								else if (ic5<nSyohin / 11)ipri = 3;
								}
								else if (ic5<nSyohin / 5)
								{
									ipri = 4;
								}
								else if (ic5<nSyohin / 2)
								{
									ipri = 5;
								}
								else if (ic5<(nSyohin* RestrictNum))
								{
									ipri = 6;
								}
								else
								{
									break;
								}
		*/
		static double[] rn1Pri = { 0.0, 0.0, 1 / 25.0, 1 / 11.0, 1 / 5.0, 1 / 2.0, 1.0, 1.0, 1.0 };
		// static double[] rn2Pri = { 0.9, 0.8, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1 };
		static double[] rn2Pri = { 1.0, 1.0, 0.908, 0.764, 0.530, 0.210, 0.05, 0.05, 0.05 };
		// 総商品数に対応した優先順位毎の(上位優先順の商品を含む)商品数を求める
		public static void FindNofUrlPerPri(
						int		i_iType,		// 作成数算出タイプ(1: 従来の比率型, 2:対数型)
						int		i_nSyohinm,		// 総商品数
						double	i_rRestrictNum,	// 境界値
						int[]	o_nUrlPerPri,	// Priority毎のURL作成数の配列
						out int o_nSzUPP)		// 同配列内のデータ数			
		{
			if (i_iType == 1)
			{
				for (int ic = 0; ic < o_nUrlPerPri.Length; ic++)
				{
					o_nUrlPerPri[ic] = (int)(rn1Pri[ic] * i_nSyohinm);
				}
				if (o_nUrlPerPri[2] > 101) o_nUrlPerPri[2] = 101;
				o_nUrlPerPri[6] = (int)System.Math.Ceiling(i_rRestrictNum * i_nSyohinm);
				o_nSzUPP = 7;
			}
			else
			{
				double a = System.Math.Log(i_nSyohinm) / System.Math.Log(8);
				int nic = o_nUrlPerPri.Length;
				for (int ic = 0; ic < nic; ic++)
				{
					double y = rn2Pri[ic];
					if (y <= i_rRestrictNum)
					{
						y = i_rRestrictNum;
						nic = ic + 1;
					}
					// double b = 4 * System.Math.Log(y) / System.Math.Pow(2, a);
					// double c = System.Math.Pow(System.Math.E, b);
					double b = System.Math.Pow(2, 2.0 - a);
					double c = System.Math.Pow(y, b);
					o_nUrlPerPri[ic] = (int)((1.0 - c) * i_nSyohinm);
				}
				o_nSzUPP = nic;
			}
		}
		// FindNofUrlPerPri 関数のテスト
		public static void testFindNofUrlPerPri()
		{
			int[] nTest = { 2, 3, 5, 7, 10, 20, 30, 50, 70, 100, 200, 300, 500, 700, 1000, 10000, 20000 };
			int[] nUrlPerPri = new int[rn2Pri.Length];
			int nSzUPP;
			double[] rR = { 0.58, 0.08 };
			string strOut = "";

			for (int iType = 1; iType <= 2; iType++)
			{

				strOut += "タイプ 総数 ";
				if (iType == 1)
				{
					for (int jc = 0; jc < rn1Pri.Length; jc++)
					{
						strOut += String.Format("{0, 6}", rn1Pri[jc]);
					}
					strOut += "\r\n";
				}
				else
				{
					for (int jc = 0; jc < rn2Pri.Length; jc++)
					{
						strOut += String.Format("{0, 6}", rn2Pri[jc]);
					}
					strOut += "\r\n";
				}

				for (int ic = 0; ic < nTest.Length; ic++)
				{
					FindNofUrlPerPri(iType, nTest[ic], rR[iType-1], nUrlPerPri, out nSzUPP);
					strOut += (iType + " " + String.Format("{0, 6}", nTest[ic]) + ": ");
					for (int jc = 0; jc < nSzUPP; jc++)
					{
						strOut += String.Format("{0, 6}", nUrlPerPri[jc]);
					}
					strOut += "\r\n";
					// listBox1.Items.Add(strOut);
				}
			}
			MessageBox.Show(strOut);
		}

		// 小分類全取込Set
		public static int SetAllSClassTorikomi()
		{
			int ist = 0;
			// 小分類マスタ
			string[] strHdr1 = { "取込" };
			string[] strItem1s = { "1" };
			string strCond1 = "表示=1 AND 取込=0";
			ist = Db.MySql.modifySingleCondRecordItems1D(Const.z_strSClass2Table, strHdr1, 1, strCond1, strItem1s);

			return ist;
		}

		// 小分類全取込Clr
		public static int ClrAllSClassTorikomi()
		{
			int ist = 0;
			// 小分類マスタ
			string[] strHdr1 = { "取込" };
			string[] strItem1s = { "0" };
			string strCond1 = "表示=1 AND 取込=1";
			ist = Db.MySql.modifySingleCondRecordItems1D(Const.z_strSClass2Table, strHdr1, 1, strCond1, strItem1s);
			return ist;
		}

		// 商品全取込Clr
		public static int ClrAllSyohinTorikomi()
		{
			int ist = 0;
			// 小分類マスタ
			string[] strHdr1 = { "取込" };
			string[] strItem1s = { "0" };
			string strCond1 = "表示=1 AND 取込=1";
			ist = Db.MySql.modifySingleCondRecordItems1D(Const.z_strSyohin2Table, strHdr1, 1, strCond1, strItem1s);
			return ist;
		}

		// 商品表示Off
		public static int ClrSyohinDisp(string strSyohinID)
		{
			int ist = 0;
			// 小分類マスタ
			string[] strHdr1 = { "表示" };
			string[] strItem1s = { "0" };
			string strCond1 = "商品ID=" + strSyohinID;
			ist = Db.MySql.modifySingleCondRecordItems1D(Const.z_strSyohin2Table, strHdr1, 1, strCond1, strItem1s);
			return ist;
		}

		// 商品表示Off
		public static int SetSyohinDisp(string strSyohinID)
		{
			int ist = 0;
			// 小分類マスタ
			string[] strHdr1 = { "表示" };
			string[] strItem1s = { "1" };
			string strCond1 = "商品ID=" + strSyohinID;
			ist = Db.MySql.modifySingleCondRecordItems1D(Const.z_strSyohin2Table, strHdr1, 1, strCond1, strItem1s);
			return ist;
		}

		// フォルダの全図形を調べ赤枠を取り除いた図形ファイルを"NoRed"フォルダに作成する
		public static void cutFldrZukeiWaku(string i_strCategoryName)
		{
			string strInFld = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + "\\" + i_strCategoryName + "\\";
			string strOutFld = Const.z_pathZukeiData[Form1.m_iradioBtnDB - 1] + Const.z_pathZukeiNoRed;

			//".jpg"ファイルをすべて取得する
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(strInFld);
			System.IO.FileInfo[] files =
				di.GetFiles("*.jpg", System.IO.SearchOption.AllDirectories);

			//赤枠を取り除いた図形ファイルを"NoRed"フォルダに作成する
			foreach (System.IO.FileInfo f in files)
			{
				string strOutFile = strOutFld + f.Name;
				cutZukeiWaku(f.FullName, strOutFile);
			}
		}

		// 図形ファイルの図形から赤枠を取り除いた図形ファイルを作成する
		private static int cutZukeiWaku(        // 戻り値	(0:正常, 1:エラー)
			string i_strFileIn,                 // 読み込み図形ファイル 
			string i_strFileOut                 // 作成図形ファイル
			)
		{
			int ist = 0;
			try
			{
				// 図形データURLの図形取込み
				Bitmap bmp1 = (Bitmap)Image.FromFile(i_strFileIn);
				Bitmap bmp2 = new Bitmap(bmp1);

				// 図形の赤枠クリア
				bool bCng = HVClear(ref bmp2);

				// 図形データファイルの作成
				if (bCng) bmp2.Save(i_strFileOut);

				bmp1.Dispose();
				bmp2.Dispose();
			}
			catch (Exception ex)
			{
				// エラーログ
				string strErrZukei = "図形入力ファイル:" + i_strFileIn + " 図形出力ファイル:" + i_strFileOut;
				string strErrLog = "図形取込みエラー: " + strErrZukei + "\n" + ex.ToString();

				if (!Form1.m_bChkErrorSkip)						//エラースキップ
				{
					string strInquiry = strErrZukei + "\n" + ex.ToString() + "\n 次の図形取込みエラーはスキップしますか？";
					DialogResult result = MessageBox.Show(strInquiry,
						"質問",
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Exclamation,
						MessageBoxDefaultButton.Button2);
					if (result == DialogResult.OK)
					{
						Form1.m_bChkErrorSkip = true;           // 図形取込みエラースキップ
					}
				}

				ist = 1;
			}
			return ist;
		}	

		private static void HClear(ref Bitmap b, int iv)
		{
			int nW = b.Width;

			for (int ic = 0; ic < nW; ic++)
			{
				b.SetPixel(ic, iv, Color.White);
			}
		}

		private static void VClear(ref Bitmap b, int ih)
		{
			int nH = b.Height;

			for (int ic = 0; ic < nH; ic++)
			{
				b.SetPixel(ih, ic, Color.White);
			}
		}

		private static bool HVClear(ref Bitmap b)
		{
			bool bRt = false;
			int nW = b.Width;
			int nH = b.Height;
			int nC = 11;
			int[,] n1 = new int[4, nC];

			// 指定位置の左端から順にピクセルの色を求める
			// 左端から順に縦方向に赤色のピクセルの数を求める(n1, n2)
			for (int ic = 0, jc = nW - 1; ic < nC; ic++, jc--)
			{
				VCount(ref b, ic, out n1[0, ic]);
				VCount(ref b, jc, out n1[1, ic]);
			}

			for (int ic = 0, jc = nH - 1; ic < nC; ic++, jc--)
			{
				HCount(ref b, ic, out n1[2, ic]);
				HCount(ref b, jc, out n1[3, ic]);
			}

			float av = 0.8F;
			int iCntRL0 = 0;
			int iCntRL1 = 0;
			int iCntRL2 = 0;
			int iCntRL3 = 0;

			for (int ic = 0; ic < nC; ic++)
			{
				if (n1[0, ic] > av * nW) iCntRL0++;
				if (n1[1, ic] > av * nH) iCntRL1++;
				if (n1[2, ic] > av * nW) iCntRL2++;
				if (n1[3, ic] > av * nH) iCntRL3++;
			}
			if ((iCntRL0 >= 2 && iCntRL1 >= 2 && iCntRL2 >= 2 && iCntRL3 >= 2) &&
				(iCntRL0 + iCntRL0 + iCntRL0 + iCntRL0) >= 12)
			{
				for (int ic = 0; ic < nC; ic++)
				{
					if (n1[0, ic] > av * nW) VClear(ref b, ic);
					if (n1[1, ic] > av * nH) HClear(ref b, ic);
					if (n1[2, ic] > av * nW) VClear(ref b, nW - ic - 1);
					if (n1[3, ic] > av * nH) HClear(ref b, nH - ic - 1);
				}
				bRt = true;
			}
			return bRt;
		}

		private static void HCount(ref Bitmap b, int iv, out int n1)
		{
			Color color1;
			int nW = b.Width;

			n1 = 0;
			for (int ic = 0; ic < nW; ic++)
			{
				color1 = b.GetPixel(ic, iv);
				if ((color1.R > 120 && color1.G < 60 && color1.B < 60) ||
					(color1.R > 250 && color1.G < color1.R - 5 && color1.B < color1.R - 5))
					n1++;
			}
		}

		private static void VCount(ref Bitmap b, int ih, out int n1)
		{
			Color color1;
			int nH = b.Height;

			n1 = 0;
			for (int ic = 0; ic < nH; ic++)
			{
				color1 = b.GetPixel(ih, ic);
				if ((color1.R > 120 && color1.G < 60 && color1.B < 60) ||
					(color1.R > 250 && color1.G < color1.R - 5 && color1.B < color1.R - 5))
					n1++;
			}
		}
	}
}