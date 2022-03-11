using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace Kc
{
	class Mkdb
	{
		// 処理経過表示用
		public static string m_strCurSClassName = "";			// 小分類名
		public static string m_strCurSClassID = "";				// 小分類ID
		public static string m_strCurSyohinName = "";			// 商品名
		public static string m_strCurSyohinID = "";				// 商品ID
		public static int m_iCurPageN = 0;						// 小分類ページ追番
		public static int m_iCurSyohinN = 0;					// 商品追番

        // D/Bファイル読み書き
        static StreamWriter sw;
        static StreamReader sr;

        public static int OpenWriteFile(		// 返値: 0:正常, !0:エラー
            string i_strFilePath,
            bool i_bAppend						// 追加モード	( true: データがファイルの末尾に追加される　false: 新しいファイルが作成される)
            )
        {
            sw = new StreamWriter(i_strFilePath, i_bAppend);
            return 0;
        }

        public static int OpenReadFile(			// 返値: 0:正常, !0:エラー
            string i_strFilePath)
        {
            Encoding utf8Enc = Encoding.GetEncoding("UTF-8");

            sr = new StreamReader(i_strFilePath, utf8Enc);
            return 0;
        }

        public static int ReadLine(				// 返値: 0:正常, !0:エラー
            out string o_strLine
            )
        {
            o_strLine = sr.ReadLine();
            return 0;
        }

        public static int WriteLine(			// 返値: 0:正常, !0:エラー
            string i_strLine
            )
        {
            Encoding utf8Enc = Encoding.GetEncoding("UTF-8");

            sw.WriteLine(i_strLine, utf8Enc);
            return 0;
        }

        public static int CloseWriteFile()
        {
            sw.Close();
            return 0;
        }

        public static int CloseReadFile()
        {
            sr.Close();
            return 0;
        }

		// LOGファイル書込み
		static StreamWriter log;

		public static int OpenLogFile(			// 返値: 0:正常, !0:エラー
			string i_strFilePath,
			bool i_bAppend						// 追加モード	( true: データがファイルの末尾に追加される　false: 新しいファイルが作成される)
			)
		{
			int ist = 0;
			string strFilePath = i_strFilePath;
			try
			{
				log = new StreamWriter(strFilePath, i_bAppend);
			}
			catch (Exception ex)
			{
				string strEx = ex.ToString();
				strFilePath = Path.GetDirectoryName(i_strFilePath) + "\\TempLog.txt";
				Dbg.Utl.MessageBoxShow("Logファイルオープンエラー\nログファイル名を\"" + strFilePath + "\"で作成します", "Log ファイルオープン");
				ist = 1;
			}
			if (ist == 1)
			{
				log = new StreamWriter(strFilePath, i_bAppend);
			}
			return 0;
		}

		public static int WriteLogFile(			// 返値: 0:正常, !0:エラー
			string i_strLine
			)
		{
			Encoding utf8Enc = Encoding.GetEncoding("UTF-8");

			log.WriteLine(i_strLine, utf8Enc);
			return 0;
		}

		public static int FlushLogFile()		// 返値: 0:正常, !0:エラー
		{
			log.Flush();
			return 0;
		}

		public static int CloseLogFile()
		{
			log.Close();
			return 0;
		}

		// Dumpファイル書込み
		static StreamWriter dump;

		public static int OpenDumpFile(			// 返値: 0:正常, !0:エラー
			string i_strFilePath,
			bool i_bAppend						// 追加モード	( true: データがファイルの末尾に追加される　false: 新しいファイルが作成される)
			)
		{
			dump = new StreamWriter(i_strFilePath, i_bAppend);
			return 0;
		}

		public static int WriteDumpFile(			// 返値: 0:正常, !0:エラー
			string i_strLine
			)
		{
			Encoding utf8Enc = Encoding.GetEncoding("UTF-8");

			dump.WriteLine(i_strLine, utf8Enc);
			return 0;
		}

		public static int CloseDumpFile()
		{
			dump.Close();
			return 0;
		}
/*S
		// カテゴリリストをファイルに書き込む
		public static void createCategoryList(
			string[] i_strCategoryNameBf,		// カテゴリ名
			string[] i_strCategoryURLBf,		// カテゴリURL
			string[] i_strCategoryDescriptBf,	// カテゴリ内容
			int i_nCategory						// カテゴリ数
			)
		{
			// カテゴリリストをファイルに書き込む
			for (int ic = 0; ic < i_nCategory; ic++)
			{
				// カテゴリ名,カテゴリURL,カテゴリ内容をファイルに書き込む
				string strDBLine = ic.ToString() + ",\"" + i_strCategoryNameBf[ic] + "\",\"" +
								 i_strCategoryURLBf[ic] + "\",\"" + i_strCategoryDescriptBf[ic] + "\"";
				WriteLine(strDBLine);
				if(Kc.Const.DEBUG)
					DispLine2(strDBLine);
			}
		}
S*/
		// カテゴリリストをMySqlマスタに書き込む
		public static void createCategoryListMst(
			string[] i_strCategoryNameBf,		// カテゴリ名
			string[] i_strCategoryURLBf,		// カテゴリURL
			string[] i_strCategoryDescriptBf,	// カテゴリ内容
			int i_nCategory						// カテゴリ数
			)
		{
			// カテゴリリストをMySqlマスタに書き込む
			for (int ic = 0; ic < i_nCategory; ic++)
			{
				// カテゴリ名,カテゴリURL,カテゴリ内容をMySql D/Bに書き込む
				int idCategoryBias = (ic + 1) * Kc.Const.z_iCategoryIDUnit;
				int idLClass = idCategoryBias + Kc.Const.z_iLClassIDBias;
				int idMClass = idCategoryBias + Kc.Const.z_iMClassIDBias;
				int idSClass = idCategoryBias + Kc.Const.z_iSClassIDBias;

				// string[] strHdr0 = { "ID", "表示",
				//						"名称", "カテゴリURL", "内容",
				//						"表示名称", "表示カテゴリURL", "表示内容",
				//						"最大大分類ID", "最大中分類ID", "最大小分類ID" };
				string[] strItems = { (ic + 1).ToString(), "0",
									  i_strCategoryNameBf[ic], i_strCategoryURLBf[ic], i_strCategoryDescriptBf[ic],
									  i_strCategoryNameBf[ic], i_strCategoryURLBf[ic], i_strCategoryDescriptBf[ic],
									  idLClass.ToString(), idMClass.ToString(), idSClass.ToString() };

				Db.MySql.insRecord(Kc.Const.z_strCategoryTable, strItems, 11);
			}
		}
/*S
		// 小分類リストをファイルに書き込む
		public static void CreateSClassList(
			int i_idCategory,					// カテゴリID	
			string i_strCategoryName,			// カテゴリ名
			string[] i_strLClassNameBf,			// 大分類名リスト
			string[] i_strMClassNameBf,			// 中分類名リスト
			string[] i_strSClassNameBf,			// 小分類名リスト
			string[] i_strSClassURLBf,			// 小分類URLリスト
			int i_nSClass						// 小分類数
			)
		{
			DispLine2("============== 小分類リスト ============== ");

			// 小分類リストをD/Bファイルに書き込む
			for (int ic = 0; ic < i_nSClass; ic++)
			{
				// 大中小分類名,小分類URLを書き込む
				string strDBLine = ic.ToString() + ",\"" + i_strCategoryName + "\",\"" +
								   i_strLClassNameBf[ic] + "\",\"" + i_strMClassNameBf[ic] + "\",\"" +
								   i_strSClassNameBf[ic] + "\",\"" + i_strSClassURLBf[ic] + "\"";
				WriteLine(strDBLine);
				if (Kc.Const.DEBUG)
					DispLine2(strDBLine);
			}
		}
S*/

		// 大中小分類リストをMySqlマスタに書き込む
		public static void  CreateSClassListMst(
			int		 i_idCategory,				// カテゴリID	
			string	 i_strCategoryName,			// カテゴリ名
			string[] i_strLClassNameBf,			// 大分類名リスト
			string[] i_strMClassNameBf,			// 中分類名リスト
			string[] i_strSClassNameBf,			// 小分類名リスト
			string[] i_strSClassURLBf,			// 小分類URLリスト
			int i_nSClass						// 小分類数
			)
		{ 
			DispLine2("============== 小分類リスト ============== ");

			string strCategoryID = i_idCategory.ToString();
			string strLClassName = "$";
			string strMClassName = "$";
			string strSClassName = "$";
			int idCategoryBias = i_idCategory * Kc.Const.z_iCategoryIDUnit;
			int idLClass = getMaxClassID(1, i_idCategory);		// 最大大分類ID
			int idMClass = getMaxClassID(2, i_idCategory);		// 最大中分類ID
			int idSClass = getMaxClassID(3, i_idCategory);		// 最大小分類ID
			for (int iS = 0; iS < i_nSClass; iS++)
			{
				if (i_strLClassNameBf[iS] != strLClassName)
				{
					// 大分類マスタ
					idLClass++;
					strLClassName = i_strLClassNameBf[iS];
					// string[] strHdr1 = { "ID", "表示", "カテゴリID", "大分類名", "表示大分類名", "内容" };
					string[] strItems = { idLClass.ToString(), "0", strCategoryID, strLClassName, strLClassName, strLClassName };
					Db.MySql.insRecord(Kc.Const.z_strLClassTable, strItems, 6);
					modMaxClassID(1, i_idCategory, idLClass);
					strMClassName = "$";
				}
				if (i_strMClassNameBf[iS] != strMClassName)
				{
					// 中分類マスタ
					idMClass++;
					strMClassName = i_strMClassNameBf[iS];
					// string[] strHdr2 = { "ID", "表示", "カテゴリID", "大分類ID", "中分類名", "表示中分類名" };
					string[] strItems = { idMClass.ToString(), "0", strCategoryID, idLClass.ToString(),
										  strMClassName, strMClassName };
					Db.MySql.insRecord(Kc.Const.z_strMClassTable, strItems, 6);
					modMaxClassID(2, i_idCategory, idMClass);
				}
				int idSClassExist = getSClassID(i_strSClassNameBf[iS]);
				if (idSClassExist == 0)
				{
					// 小分類マスタ
					idSClass++;
					strSClassName = i_strSClassNameBf[iS];
					string strSClassURL = Kc.Syohin.relURL(i_strSClassURLBf[iS]);
					uint idSyohin = (uint)idSClass * (uint)Kc.Const.z_iSClassID2Unit;         // 最大商品ID
                    // string[] strHdr3 = { "小分類ID", "取込", "表示", "カテゴリID", "大分類ID", "中分類ID",
                    //						"小分類名", "小分類URL", "表示小分類名", "表示小分類URL", "最大商品ID" };
                    string[] strItem1s = { idSClass.ToString(), "0", "0", strCategoryID, idLClass.ToString(), idMClass.ToString(),
										   strSClassName, strSClassURL, strSClassName, strSClassURL, idSyohin.ToString() };
					Db.MySql.insRecord(Kc.Const.z_strSClass2Table, strItem1s, 11);
					modMaxClassID(3, i_idCategory, idSClass);
					idSClassExist = idSClass;
				}
				// 中小分類関係マスタ
				// string[] strHdrA = { "中分類ID", "小分類ID" };
				string[] strItem2s = { idMClass.ToString(), idSClassExist.ToString() };
				Db.MySql.insRecord(Kc.Const.z_strMSCRelation, strItem2s, 2);
				
			}
		}
		/*S
				// 商品属性をファイルに書き込む
				public static void CreateSyohinAttrList(
					string i_strSClassID,				// 小分類ID
					string i_strSyohinID,				// 商品ID
					string[] i_strAttrNameS,			// 商品の属性名
					string[] i_strAttrValueS,			// 商品の属性値
					int i_nAttrS,						// 商品の属性数
					string i_strPhotoURL				// 一覧表示用商品写真URL
					)
				{
					string strSyohinId = i_strSyohinID;

					{
						// 商品属性をファイルに書き込む
						for (int iz = 0; iz < i_nAttrS; iz++)
						{
							WriteLine(strSyohinId + " \"" + i_strAttrNameS[iz] + "\",\"" + i_strAttrValueS[iz] + "\"");
						}
						WriteLine(strSyohinId + " \"一覧表示用商品写真URL\",\"" + i_strPhotoURL + "\"");
					}
				}
		S*/
		/*S
				// 仕様URL、図形URLをファイルに書き込む
				public static void CreateSyohinURLList(
					string i_strSyohinID,				// 商品ID
					string i_strMakerTopPageURL,		// メーカーTopPageURL
					string i_strSeihinInfoURL,			// メーカー製品情報URL
					string i_strSeihinSiyoURL,			// メーカー仕様表URL
					string i_strPreReleaseURL,			// プレリリースURL
					string[] i_strProductPictURL,		// 製品写真URL(主図形,サブ1図形,サブ2図形,･･･)
					string[] i_strLSizeProductPictHPURL,// フルスケール製品写真掲載ホームページのURL
					int i_nProductPictURL				// 製品写真数
					)
				{
					string strSyohinId = i_strSyohinID;

					{
						// 商品の仕様URLをファイルに書き込む
						WriteLine(strSyohinId + " \"メーカーTopPageURL\",\"" + i_strMakerTopPageURL + "\"");
						WriteLine(strSyohinId + " \"メーカー製品情報URL\",\"" + i_strSeihinInfoURL + "\"");
						WriteLine(strSyohinId + " \"メーカー仕様表URL\",\"" + i_strSeihinSiyoURL + "\"");
						WriteLine(strSyohinId + " \"プレリリースURL\",\"" + i_strPreReleaseURL + "\"");

						// 商品の図形URLをファイルに書き込む
						for (int iz = 0; iz < i_nProductPictURL; iz++)
						{
							WriteLine(strSyohinId + " \"製品写真URL" + iz.ToString() + "\",\"" +
											i_strProductPictURL[iz] + "\"");
							WriteLine(strSyohinId + " \"製品フル写真URL" + iz.ToString() + "\",\"" +
											i_strLSizeProductPictHPURL[iz] + "\"");
						}
					}
				}
		S*/

		// 商品の一般属性値、URL型属性値をMySqlマスタに書き込む
		//	(実前にsetAttrNameSearchTableで、小分類内の属性IDと属性名を検索用テーブルに設定している事)
		public static void CreateSyohinAttrListMst(
			//string i_strSClassID,				// 小分類ID
			string i_strSyohinID,				// 商品ID
			string[] i_strAttrNameS,			// 商品の属性名
			string[] i_strAttrValueS,			// 商品の属性値
			int i_nAttrS,						// 商品の属性数
			string i_strPhotoURL				// 一覧表示用商品写真URL
			)
		{
			// 商品属性をMySqlマスタに書き込む
			int idAttr;

			// 商品属性をMySqlマスタに書き込む
			for (int iz = 0; iz < i_nAttrS; iz++)
			{
				// 属性IDをもとめる
				idAttr = Kc.DbAttr.getAttrID(i_strAttrNameS[iz]);

				// 一般属性値マスタに追加する
				//string[] strHdr5 = { "商品ID", "属性ID", "属性値", "表示属性値" };
				//string[] strType5 = { "int", "int", "varchar(50)", "varchar(50)" };
				string[] strItem1vs = { i_strSyohinID, idAttr.ToString(), i_strAttrValueS[iz], i_strAttrValueS[iz] };
				Db.MySql.insRecord(Kc.Const.z_strAttributeValue2Table, strItem1vs, 4);
			}

			// URL型属性値マスタ　一覧表示用商品写真URL
			if (i_strPhotoURL != "")
			{
				string[] strCZvItems = { i_strSyohinID, (Kc.Const.z_idAttrTZukei + 0).ToString(),
										 i_strPhotoURL, "z" + ConvertSyohinID10(i_strSyohinID) + "-S000" };
				Db.MySql.insRecord(Kc.Const.z_strAttributeURL2Table, strCZvItems, 4);
			}
		}

		// MySqlマスタの商品の一般属性値、URL型属性値を修正する
		public static void ReplaceSyohinAttrListMst(
			string i_strSClassID,				// 小分類ID
			string i_strSyohinID,				// 商品ID
			string[] i_strAttrNameS,			// 商品の属性名
			string[] i_strAttrValueS,			// 商品の属性値
			int i_nAttrS,						// 商品の属性数
			string i_strPhotoURL				// 一覧表示用商品写真URL
			)
		{
			// 商品属性をMySqlマスタに書き込む
			int idAttr;

			// 1. DBから商品(商品ID)の全属性を取得する
			// 属性値マスタ
			string[] strHdrAT = { "属性ID", "属性値", "表示属性値" };
			string[,] strItemATs = new string[Kc.Const.z_nSyohinAttrValueMax, strHdrAT.Length];	// 商品内の属性値の[属性ID],[属性値],[表示属性値]のリスト 
			int nItemAT;																		// 属性値数

			// 商品の[属性ID],[属性値],[表示属性値]を取得
			string strCondS = "商品ID=" + i_strSyohinID;
			Db.MySql.getTableItems(Kc.Const.z_strAttributeValue2Table, strHdrAT, strHdrAT.Length, strCondS, "", strItemATs, out nItemAT);


			// 商品属性をMySqlマスタに書き込む
			for (int iz = 0; iz < i_nAttrS; iz++)
			{
				// 属性IDをもとめる
				idAttr = Kc.DbAttr.getAttrID(i_strAttrNameS[iz]);

				int ich;
				bool bSame = false;
				for (ich = 0; ich < nItemAT; ich++)
				{
					if (idAttr == int.Parse(strItemATs[ich,0]))
					{
						if (i_strAttrValueS[iz] == strItemATs[ich, 1])
						{
							// 属性値が同一で修正不要
							bSame = true;
						}
						break;
					}

				}
				if (bSame)
				{
					continue;
				}

				string[] strItem1vs = { i_strSyohinID, idAttr.ToString(), i_strAttrValueS[iz], i_strAttrValueS[iz] };
				if (ich == nItemAT)
				{
					// 属性IDの属性が無いので新規追加する
					Db.MySql.insRecord(Kc.Const.z_strAttributeValue2Table, strItem1vs, 4);
				}
				else
				{
					// 既に属性IDの属性が有り修正する
					Db.MySql.repRecord(Kc.Const.z_strAttributeValue2Table, strItem1vs, 4);
				}
			}

			// URL型属性値マスタ　一覧表示用商品写真URL
			if (i_strPhotoURL != "")
			{
				string[] strCZvItems = { i_strSyohinID, (Kc.Const.z_idAttrTZukei + 0).ToString(),
										 i_strPhotoURL, "z" + ConvertSyohinID10(i_strSyohinID) + "-S000" };
				Db.MySql.repRecord(Kc.Const.z_strAttributeURL2Table, strCZvItems, 4);
			}
		}
		// 0パディングした10桁の商品IDを求める
		// 返値: 0パディングした10桁の商品ID
		public static string ConvertSyohinID10(string i_strSyohinID)
		{
			string strSyohinID19 = "0000000000" + i_strSyohinID;
			int lnSyohinID19 = strSyohinID19.Length;
			string strSyohinID10 = strSyohinID19.Substring(lnSyohinID19 - 10);
			return strSyohinID10;
		}

		// 仕様URL、図形URLをURL型属性値マスタに書き込む
		public static void CreateSyohinURLListMst(
			string i_strSyohinID,				// 商品ID
			string i_strMakerTopPageURL,		// メーカーTopPageURL
			string i_strSeihinInfoURL,			// メーカー製品情報URL
			string i_strSeihinSiyoURL,			// メーカー仕様表URL
			string i_strPreReleaseURL,			// プレリリースURL
			string[] i_strProductPictURL,		// 製品写真URL(主図形,サブ1図形,サブ2図形,･･･)
			string[] i_strLSizeProductPictHPURL,// フルスケール製品写真掲載ホームページのURL
			int i_nProductPictURL				// 製品写真数
			)
		{
			string strSyohinID10 =  ConvertSyohinID10(i_strSyohinID);

			// URL型属性値マスタ
			//string[] strHdr6 = { "商品ID", "属性ID", "URL", "図形CD" };
			//string[] strType6 = { "int", "int", "varchar(255)", "varchar(20)" };
			if (i_strMakerTopPageURL != "")
			{
				string[] strSvItems = { i_strSyohinID, (Kc.Const.z_idAttrURL + 1).ToString(), i_strMakerTopPageURL, "" };
				Db.MySql.insRecord(Kc.Const.z_strAttributeURL2Table, strSvItems, 4);
			}
			if (i_strSeihinInfoURL != "")
			{
				string[] strSvItems = { i_strSyohinID, (Kc.Const.z_idAttrURL + 2).ToString(), i_strSeihinInfoURL, "" };
				Db.MySql.insRecord(Kc.Const.z_strAttributeURL2Table, strSvItems, 4);
			}
			if (i_strSeihinSiyoURL != "")
			{
				string[] strSvItems = { i_strSyohinID, (Kc.Const.z_idAttrURL + 3).ToString(), i_strSeihinSiyoURL, "" };
				Db.MySql.insRecord(Kc.Const.z_strAttributeURL2Table, strSvItems, 4);
			}
			if (i_strPreReleaseURL != "")
			{
				string[] strSvItems = { i_strSyohinID, (Kc.Const.z_idAttrURL + 4).ToString(), i_strPreReleaseURL, "" };
				Db.MySql.insRecord(Kc.Const.z_strAttributeURL2Table, strSvItems, 4);
			}

			// 商品のスモールサイズ図形URLをMySqlマスタに書き込む
			for (int iz = 0; iz < i_nProductPictURL; iz++)
			{
				string strIz = (100 + iz).ToString();
				// URL型属性値マスタ
				string[] strSZvItems = { i_strSyohinID, (Kc.Const.z_idAttrSZukei + iz).ToString(),
										 i_strProductPictURL[iz], "z" + strSyohinID10 + "-S" + strIz };
				Db.MySql.insRecord(Kc.Const.z_strAttributeURL2Table, strSZvItems, 4);
			}

			// 商品のフルスケール図形表示ページのURLをMySqlマスタに書き込む
			for (int iz = 0; iz < i_nProductPictURL; iz++)
			{
				string strIz = (300 + iz).ToString();
				// URL型属性値マスタ
				string[] strSZvItems = { i_strSyohinID, (Kc.Const.z_idAttrLPZukei + iz).ToString(), 
										 i_strLSizeProductPictHPURL[iz], strSyohinID10 + "-S" + strIz };
				Db.MySql.insRecord(Kc.Const.z_strAttributeURL2Table, strSZvItems, 4);
			}
		}

		// 商品属性データを削除
		public static void deleteSyohinAttr(
			string i_strSyohinID				// 商品ID
			)
		{
			// 商品の一般属性値を削除
			Db.MySql.delRecord(Kc.Const.z_strAttributeValue2Table, "商品ID", i_strSyohinID);
			// 商品のURL型属性値を削除
			Db.MySql.delRecord(Kc.Const.z_strAttributeURL2Table, "商品ID", i_strSyohinID);
		}

		// 商品データを削除
		public static int deleteSyohin(
			string i_strSyohinID				// 商品ID
			)
		{
			int ist = 0;
			// 商品の一般属性値を削除
			ist = Db.MySql.delRecord(Kc.Const.z_strAttributeValue2Table, "商品ID", i_strSyohinID);
			if (ist < 0) return -1;
			// 商品のURL型属性値を削除
			ist = Db.MySql.delRecord(Kc.Const.z_strAttributeURL2Table, "商品ID", i_strSyohinID);
			if (ist < 0) return -1;
			// 商品を削除
			ist = Db.MySql.delRecord(Kc.Const.z_strSyohin2Table, "商品ID", i_strSyohinID);
			if (ist < 0) return -1;
			return 0;
		}

		// 小分類データを削除
		public static int deleteSClass(
			string i_strSClassID				// 小分類ID
			)
		{
			int ist = 0;

			// 小分類の商品IDを取得 
			int iszSyohinID = Kc.Const.z_nSClassSyohinMax;
			string[] strSyohinID = new string[iszSyohinID];
			int nSyohinID;
			getSyohinIDofSClass( i_strSClassID, strSyohinID, iszSyohinID, out nSyohinID);
			if(Dbg.Utl.CheckOverflow(nSyohinID >= iszSyohinID, "小分類内の商品数", "deleteSClass")) return -1;
			// 小分類の商品の一般属性, URL属性, 商品を削除 
			for (int ic = 0; ic < nSyohinID; ic++)
			{

				ist = deleteSyohin(strSyohinID[ic]);
				if (ist < 0) return -1;
			}
			// 小分類の属性名の削除
			ist = Db.MySql.delRecord(Kc.Const.z_strAttributeNameTable, "小分類ID", i_strSClassID);
			// 小分類の最大商品IDを0に設定
			uint idSyohinMax = uint.Parse(i_strSClassID) * Kc.Const.z_iSClassID2Unit;
			ist = modMaxSyohinID(i_strSClassID, idSyohinMax);
			return ist;
		}

		// カテゴリURLの取得
		public static string getCategoryURL(
			string i_strCategoryID				// カテゴリID
			)
		{
			string strCategoryURL = "";
			string[] strHdr = { "表示カテゴリURL" };
			string[] strItems = new string[1];
			string strCond = "カテゴリID=" + i_strCategoryID;
			if (Db.MySql.getRecordItems(Kc.Const.z_strCategoryTable, strHdr, 1, strCond, strItems))
			{
				strCategoryURL = strItems[0];
			}
			else
			{
				Dbg.Utl.MessageBoxShow("getCategoryURL: カテゴリURLが未設定です", "設定エラー");
			}
			return strCategoryURL;
		}


		// 商品名から商品IDを求める
		// 返値 ステイタス　(0:未登録商品で(MaxID+1)の新規IDを作成, 1:登録済み商品で登録商品IDを返す)
		public static int getSyohinIDfromSyohinName(			// ステイタス
			string i_strSyohinName,				// 商品名
			string i_strSClassID,				// 小分類ID
			out string o_strSyohinID,			// 商品ID
			out int o_iTorikomi					// 取込
			)
		{
			int ist;
			// 商品名で商品マスターを検索し商品IDと取込を取得する
			string[] strHdr = { "商品ID", "取込" };
			string[] strItems = new string[2];
			string strCond = "商品名=\"" + i_strSyohinName+"\"";
			if (Db.MySql.getRecordItems(Kc.Const.z_strSyohin2Table, strHdr, 2, strCond, strItems))
			{
				o_strSyohinID = strItems[0];
				o_iTorikomi = int.Parse(strItems[1]);
				ist = 1;
			}
			else
			{
				// 商品無しの場合は小分類レコードの最大商品IDの次の番号を取得する
				uint idSyohinIDMax = getMaxSyohinID(i_strSClassID);
				idSyohinIDMax++;
				modMaxSyohinID(i_strSClassID, idSyohinIDMax);
				o_strSyohinID = idSyohinIDMax.ToString();
				o_iTorikomi = 0;
				ist = 0;
			}
			return ist;
		}

		// 小分類の商品IDを求める
		// 返値 ステイタス　(0:正常 <0:エラー)
		public static int getSyohinIDofSClass(	// ステイタス
			string i_strSClassID,				// 小分類ID
			string[] o_strSyohinIDs,			// 商品IDリスト
			int	i_iLimit,						// 最大取得数
			out int o_nSyohinID					// 商品ID数
			)
		{
			int ist = 0;
			// 小分類IDで商品マスターを検索し商品IDリストを取得する
			string[] strHdr = { "商品ID" };
			string[,] strItems = new string[i_iLimit,1];
//			string strCond = "小分類ID='" + i_strSClassID + "' LIMIT " + i_iLimit.ToString();
			string strCond = "小分類ID='" + i_strSClassID + "'";
			ist = Db.MySql.getTableItems(Kc.Const.z_strSyohin2Table, strHdr, 1, strCond, "", strItems, out o_nSyohinID);
			for (int ic=0; ic<o_nSyohinID; ic++)
			{
				o_strSyohinIDs[ic] = strItems[ic,0];
			}
			return ist;
		}

		static string[] m_strClassIDHdr = { "最大大分類ID", "最大中分類ID", "最大小分類ID" };

		// 最大分類IDの取得
		public static int getMaxClassID(
			int i_iClassIDType,					// 分類IDタイプ (1:最大大分類ID, 2:最大中分類ID, 3:最大小分類ID)
			int i_idCategory					// カテゴリID
			)
		{
			string[] strHdr = { m_strClassIDHdr[i_iClassIDType - 1] };
			string[] strItems = new string[1];
			string strCond = "カテゴリID=" + i_idCategory.ToString();
			if (!Db.MySql.getRecordItems(Kc.Const.z_strCategoryTable, strHdr, 1, strCond, strItems))
			{
				Dbg.Utl.MessageBoxShow("getMaxClassID: 最大値が未設定です", "設定エラー");
			}
			return int.Parse(strItems[0]);
		}

		// 最大分類IDの書込み
		public static int modMaxClassID(
			int i_iClassIDType,					// 分類IDタイプ (1:最大大分類ID, 2:最大中分類ID, 3:最大小分類ID)
			int i_idCategory,					// カテゴリID
			int i_idClasIDMax					// 最大分類ID
			)
		{
			string[] strHdr = { m_strClassIDHdr[i_iClassIDType - 1] };
			string[] strItems = { i_idClasIDMax.ToString() };
			string strCond = "カテゴリID=" + i_idCategory.ToString();
			Db.MySql.modifySingleCondRecordItems1D(Kc.Const.z_strCategoryTable, strHdr, 1, strCond, strItems);
			return 0;
		}

		// 小分類マスタから最大商品IDの取得
        // 返値: 最大商品ID
		public static uint getMaxSyohinID(
			string i_strSClassID				// 小分類ID
			)
		{
			string[] strHdr = { "最大商品ID" };
			string[] strItems = new string[1];
			string strCond = "小分類ID=" + i_strSClassID;
			if (!Db.MySql.getRecordItems(Kc.Const.z_strSClass2Table, strHdr, 1, strCond, strItems))
			{
				Dbg.Utl.MessageBoxShow("getMaxSyohinID: 最大値が未設定です", "設定エラー");
			}
			return uint.Parse(strItems[0]);
		}
		/*
		// 商品マスタから最大商品IDの取得 スピード遅く不使用
		public static int getMaxSyohinIDfromSyohin(
			string i_strSClassID				// 小分類ID
			)
		{
			// 商品マスタ
			string[] strHdr2 = { "商品ID" };
			string[,] strItem2s = new string[Kc.Const.z_nSClassSyohinMax, 1];	// 商品ID,取込のリスト
			int nSyohin;														// 商品数
			string strCond = "小分類ID=" + i_strSClassID;
			// 最大商品IDを取得
			int iMaxSyohinID = 0;
			Db.MySql.getTableItems(Kc.Const.z_strSyohin2Table, strHdr2, 1, strCond, "", strItem2s, out nSyohin);
			for (int ic1 = nSyohin - 1; ic1 >= 0; ic1--)
			{
				if (iMaxSyohinID < int.Parse(strItem2s[ic1, 0]))
				{
					iMaxSyohinID = int.Parse(strItem2s[ic1, 0]);
				}
			}
			return iMaxSyohinID;
		}
		*/
		// 最大商品IDの書込み
		public static int modMaxSyohinID(
			string i_strSClassID,				// 小分類ID
			uint i_idSyohinMax					// 最大商品ID
			)
		{
			int ist;
			string[] strHdr = { "最大商品ID" };
			string[] strItems = { i_idSyohinMax.ToString() };
			string strCond = "小分類ID=" + i_strSClassID;
			ist = Db.MySql.modifySingleCondRecordItems1D(Kc.Const.z_strSClass2Table, strHdr, 1, strCond, strItems);
			return ist;
		}

		// 小分類名から小分類IDの取得
        // 返値: 小分類ID
		public static int getSClassID(			// 小分類ID		(0:未定義)
			string i_strSClassName				// 小分類名
			)
		{
			int iRt = 0;
			string[] strHdr = { "小分類ID" };
			string[] strItems = new string[1];
			string strCond = "小分類名=" + "\"" + i_strSClassName + "\"";
			if (Db.MySql.getRecordItems(Kc.Const.z_strSClass2Table, strHdr, 1, strCond, strItems))
			{
				if (strItems[0] != null)
				{
					iRt = int.Parse(strItems[0]);
				}
			}
			return iRt;
		}

		// マスタのダンプ出力
		public static void DumpMst(
			string i_strMark,					// 出力記号
			string i_strTable,					// テーブル名
			string [] i_strHdr,					// 出力項目一覧
			string i_strOdr,					// ソートキー
			int i_nLineMax,						// 最大レコード数
			int i_nClm							// 項目数
			)
		{
			string strLine;
			string[,] strItems = new string[i_nLineMax, i_nClm];				// 全項目
			int nLine;															// レコード数

			strLine = "";
			for (int ic = 0; ic < i_nClm; ic++)
			{
				if(ic !=0) {
					strLine += ",";
				}
				strLine += ("\"" + i_strHdr[ic] + "\"");
			}
			WriteDumpFile(strLine);

			Db.MySql.getTableItems(i_strTable, i_strHdr, i_nClm, "", i_strOdr, strItems, out nLine);

			for (int il = 0; il < nLine; il++)
			{
//				strLine = i_strMark + il.ToString() + ": \"";
				strLine = i_strMark + ": \"";
				for (int ic = 0; ic < i_nClm; ic++)
				{
					if (ic != 0)
					{
						strLine += "\",\"";
					}
					strLine += strItems[il, ic];
				}
				strLine += "\"";
				WriteDumpFile(strLine);
			}
		}

		// 処理状況表示	(to TextBox1)
		const int iConsoleOutFlg = 2;
		// タイトル表示・ログファイルへ強制書込み
		public static void DispLine1(
			string i_strMsg						// メッセージ
			)
		{
			// ログファイルへメッセージを書込む
			WriteLogFile(i_strMsg + DateTime.Now.ToString(" HH:mm:ss"));
			FlushLogFile();
			// Text Box1へメッセージを書込む
			Form1.Form1Instance.TextBox1Text = i_strMsg;
			//（フォーム全体を再描画）
			Form1.Form1Instance.Update();
		}
		// 処理状況表示	(to TextBox2)
		public static void DispLine2(
			string i_strMsg						// メッセージ
			)
		{
			string strMsg = m_strCurSClassID + " ( " + m_strCurSClassName + " ) " + " Page."+m_iCurPageN + "\r\n" +
							m_strCurSyohinID + " ( " + m_strCurSyohinName + " ) " + " No." + m_iCurSyohinN + "\r\n\r\n" +
							i_strMsg;
			Form1.Form1Instance.TextBox2Text = (strMsg);
			//TextBox2を再描画する
			//（フォーム全体を再描画）
			Form1.Form1Instance.Update();
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		///     指定したファイルを削除します。</summary>
		/// <param name="stFilePath">
		///     削除するファイルまでのパス。</param>
		/// -----------------------------------------------------------------------------
		public static void DeleteFile(string stFilePath)
		{
			System.IO.FileInfo cFileInfo = new System.IO.FileInfo(stFilePath);

			// ファイルが存在しているか判断する
			if (cFileInfo.Exists)
			{
				// 読み取り専用属性がある場合は、読み取り専用属性を解除する
				if ((cFileInfo.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
				{
					cFileInfo.Attributes = System.IO.FileAttributes.Normal;
				}

				// ファイルを削除する
				cFileInfo.Delete();
			}
		}
		/// -----------------------------------------------------------------------------
		/// <summary>
		///     指定したファイルを移動します。</summary>
		/// <param name="stFromFilePath">
		///     移動元のファイルまでのパス。</param>
		/// <param name="stToFilePath">
		///     移動先のファイルまでのパス。</param>
		/// -----------------------------------------------------------------------------
		public static void MoveFile(string stFromFilePath, string stToFilePath)
		{
			System.IO.FileInfo cFileInfo = new System.IO.FileInfo(stFromFilePath);

			// ファイルが存在しているか判断する
			if (cFileInfo.Exists)
			{
				// 読み取り専用属性がある場合は、読み取り専用属性を解除する
				if ((cFileInfo.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
				{
					cFileInfo.Attributes = System.IO.FileAttributes.Normal;
				}

				// ファイルを移動する
				FileSystem.MoveFile(stFromFilePath, stToFilePath, true);
			}
		}
		/// -----------------------------------------------------------------------------
		/// <summary>
		///     指定したファイルをコピーします。</summary>
		/// <param name="stFromFilePath">
		///     コピー元のファイルまでのパス。</param>
		/// <param name="stToFilePath">
		///     コピー先のファイルまでのパス。</param>
		/// -----------------------------------------------------------------------------
		public static void CopyFile(string stFromFilePath, string stToFilePath)
		{
			System.IO.FileInfo cFileInfo = new System.IO.FileInfo(stFromFilePath);

			// ファイルが存在しているか判断する
			if (cFileInfo.Exists)
			{
				// 読み取り専用属性がある場合は、読み取り専用属性を解除する
				if ((cFileInfo.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
				{
					cFileInfo.Attributes = System.IO.FileAttributes.Normal;
				}

				// ファイルをコピーする
				FileSystem.CopyFile(stFromFilePath, stToFilePath, true);
			}
		}
	}
}
