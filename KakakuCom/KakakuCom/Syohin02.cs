using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Add
using System.IO;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace Kc
{
	public partial class Syohin
	{
		/// <summary>
		/// 商品を取込む
		/// </summary>
		/// <param name="strFirstSlassID">"":先頭から、"nnnn":取込み開始小分類ID</param>
		/// <returns></returns>
		public static int ImportSyohin(string strFirstSlassID)
		{
			int ist = 0;

			// LogFile取込み開始を書き、(カテゴリ、大中小分類、または) 属性を取り込む
			string strMsg = "";
            Mkdb.WriteLogFile("商品取込開始");

			// チェックボックスの選択状況により(カテゴリ、大中小分類、または)属性を取り込む
            if (Form1.m_bChkCategory)							// カテゴリ
			{
				// カテゴリリストをD/Bに書き込む
				strMsg = "カテゴリリストをD/Bに書き込み";
				Mkdb.DispLine2(strMsg);
				Mkdb.WriteLogFile(strMsg + "開始");

				string strHomePageURL = Kc.Const.z_urlKakakuDataHomePage;		// ホームページのURL
				Syohin.importCategoryList();

				Mkdb.WriteLogFile(strMsg + "終了");
			}
			if (Form1.m_bChkClass)								// 大中小分類
			{
				// 全小分類リストをD/Bに書き込む
				strMsg = "全小分類リストをD/Bに書き込み";
				Mkdb.DispLine2(strMsg);
				Mkdb.WriteLogFile(strMsg + "開始");

				Syohin.importAllSClassList();

				Mkdb.WriteLogFile(strMsg + "終了");
			}
			if (Form1.m_bChkAttr)								// 属性
			{
				// 小分類のURLで指定したホームページをテーブルに読み込み
				// 小分類に含まれる全商品の属性データをD/Bに書き込む
				strMsg = "小分類に含まれる全商品の属性データをD/Bに書き込み";
				Mkdb.DispLine2(strMsg + "開始");
				Mkdb.WriteLogFile(strMsg + "開始");

				bool bFirst = true;
				bool bDeleteSClassAttr = false;								// 小分類の全属性データを削除しない
				bool bInheritDispFlg = false;								// 属性名の[表示]フラグを継承しない

				// 全小分類の 小分類ID、取込、と小分類名 を取得する
				string[] strHdr = { "小分類ID", "取込", "小分類名" };
				string[,] strItems = new string[Kc.Const.z_nSClassMax, 3];		// 全小分類の[小分類ID],[取込],[小分類名]のリスト
				int nSClass;													// 小分類数
				Db.MySql.getTableItems(Kc.Const.z_strSClass2Table, strHdr, 3, "", "", strItems, out nSClass);

				for (int ic = 0; ic < nSClass; ic++)
				{
					// 指定された開始小分類ID以降の[取込]が"1" の小分類を取込み対象とする
					string strSClassID = strItems[ic, 0];
                    if (strFirstSlassID != "" && string.Compare(strSClassID, strFirstSlassID) < 0)
                    {
                        continue;
                    }
					string strSClassGetFlg = strItems[ic, 1];                   // [取込]
                    if (strSClassGetFlg != "1") continue;                       // 取込対象外はスキップ

					// ログに取込む小分類IDを書き込む
					Mkdb.m_strCurSClassID = strItems[ic, 0];					// 処理経過表示用 小分類ID
					Mkdb.m_strCurSClassName = strItems[ic, 2];					// 処理経過表示用 小分類名

                    string strTrace = "〇商品取り込みクラス SClassID=" + Mkdb.m_strCurSClassID + " 小分類名=" + Mkdb.m_strCurSClassName;
                    Mkdb.WriteLogFile(strTrace);
                    Mkdb.FlushLogFile();

                    // [取込]が1である小分類の全商品の属性リストをD/Bに書き込む
                    // D/Bから小分類の商品を１件取得する
                    string[] strSyohinIDs = new string[1];
					int nSyohinID;
					Mkdb.getSyohinIDofSClass(strSClassID, strSyohinIDs, 1, out nSyohinID);
					if (nSyohinID != 0)
					{
						// 小分類に商品あり
						if (Form1.m_iradioBtnHenkoMode == 1)		// 変更モード: スキップ(1)
						{
							// 1件でも既に取得済みであるならスキップ
							continue;
						}
						else if (Form1.m_iradioBtnHenkoMode == 2)   // 変更モード: 追加(2)
						{
							// 小分類の全商品の変動項目([売れ筋],[レビュー評価],[口コミ件数])は一旦0クリア
							Syohin.clrMovementAttrValue(strSClassID);
						}
						else if (Form1.m_iradioBtnHenkoMode == 3)   // 変更モード: 削除後追加(3)
						{
							if (bFirst)
							{
								// 小分類の全商品を削除
								string strInquiry = "小分類(" + strItems[ic, 2] + ")の全商品を消去しますか？";
								DialogResult result = MessageBox.Show(strInquiry,
									"質問",
									MessageBoxButtons.OKCancel,
									MessageBoxIcon.Exclamation,
									MessageBoxDefaultButton.Button2);
								if (result == DialogResult.OK)
								{
									bDeleteSClassAttr = true;					// 小分類の全属性データ削除する
									bFirst = false;
								}
								else
								{
									break;										// Cancelの場合は削除と取り込みの両方終了
								}
								strInquiry = "小分類(" + strItems[ic, 2] + ")の属性名の[表示]フラグを継承しますか？";
								result = MessageBox.Show(strInquiry,
									"質問",
									MessageBoxButtons.OKCancel,
									MessageBoxIcon.Exclamation,
									MessageBoxDefaultButton.Button2);
								if (result == DialogResult.OK)
								{
									bInheritDispFlg = true;						// 属性名の[表示]フラグを継承する
								}
							}
						}
					}

					string[] strHdrAN = { "属性ID", "表示", "属性名", "表示属性名", "表示巾" };
					string[,] strItemANs = new string[Kc.Const.z_nSClassAttrNameMax, strHdrAN.Length];		// 小分類内の属性名の[属性ID],[表示],[属性名],[表示属性名]のリスト 
					int nItemAN = 0;																		// 属性名数
					if (bInheritDispFlg)        // 継承処理
					{
						// 属性名の[表示],[表示属性名],[表示巾]を保存する
						ist = Syohin.saveAttrNameDispFlag(strSClassID, strHdrAN, strItemANs, out nItemAN);
						if (Dbg.Utl.CheckError(ist != 0, "属性名の[表示],[表示属性名],[表示巾]の保存エラー", "saveAttrNameDispFlag")) break;
					}

					if (bDeleteSClassAttr)
					{
						Syohin.deleteSClass(strSClassID);			// 小分類の全属性データを削除
						DbAttr.clrAttrNameSearchTable();			// 属性名検索テーブルのクリア
					}

					////////////////////////////////////////////////
					// 小分類の全商品の属性リストをD/Bに取り込む  //
					////////////////////////////////////////////////
					if (strSClassID != "1230")
					{
						// 小分類の全商品の属性リストをD/Bに取り込む
						ist = Syohin.importSClassProducts(strSClassID);
						if (Dbg.Utl.CheckError(ist != 0, "商品の属性リスト取込みエラー", "importSClassProducts")) break;
					}

					if (bInheritDispFlg)		// 継承処理
					{
						// 属性名の[表示],[表示属性名],[表示巾]を継承し、再設定する
						ist = Syohin.loadAttrNameDispFlag(strSClassID, strHdrAN, strItemANs, nItemAN);
						if (Dbg.Utl.CheckError(ist != 0, "属性名の[表示],[表示属性名],[表示巾]の再設定エラー", "saveAttrNameDispFlag")) break;
					}
				}

				Mkdb.WriteLogFile(strMsg + "終了");
			}

			strMsg = "商品データ, カテゴリ・分類取込み 終了";
			Mkdb.DispLine2(strMsg);
			return ist;
		}
		public static int GarbageSyohin()
		{
			int ist = 0;
			string strMsg = "";
			Mkdb.WriteLogFile("対象外商品の表示Off開始");

			// 小分類のURLで指定したホームページをテーブルに読み込み
			// 小分類に含まれる全商品の属性データをD/Bに書き込む
			strMsg = "対象外商品の表示Off";
			Mkdb.DispLine2(strMsg + "開始");
			Mkdb.WriteLogFile(strMsg + "開始");

			// 全小分類の小分類IDと取込を取得する
			string[] strHdr = { "小分類ID", "取込", "小分類名" };
			string[,] strItems = new string[Kc.Const.z_nSClassMax, 3];      // 全小分類の[小分類ID],[取込],[小分類名]のリスト
			int nSClass;                                                    // 小分類数
			Db.MySql.getTableItems(Kc.Const.z_strSClass2Table, strHdr, 3, "", "", strItems, out nSClass);

			string strDispOffSelYear = "2011";								// 間引き最大年(含む)

			for (int ic = 0; ic < nSClass; ic++)
			{
				string strSClassID = strItems[ic, 0];
				string strSClassGetFlg = strItems[ic, 1];                   // [取込]
				if (strSClassGetFlg != "1") continue;                       // 取込対象外はスキップ

				Mkdb.m_strCurSClassID = strItems[ic, 0];                    // 処理経過表示用 小分類ID
				Mkdb.m_strCurSClassName = strItems[ic, 2];                  // 処理経過表示用 小分類名

				string strTrace = "〇対象外商品の表示Off SClassID=" + Mkdb.m_strCurSClassID + " 小分類名=" + Mkdb.m_strCurSClassName;
				Mkdb.WriteLogFile(strTrace);
				Mkdb.FlushLogFile();
				// [取込]が1である小分類の全商品の属性リストをD/Bに書き込む
				// D/Bから小分類の商品を１件取得する
				clearSyohinDispFlg(strSClassID, strDispOffSelYear);
			}

			Mkdb.WriteLogFile(strMsg + "終了");

			strMsg = "該当外商品の表示Off開始 終了";
			Mkdb.DispLine2(strMsg);
			return ist;
		}

		static public void clearSyohinDispFlg(string strSClassID, string strDispOffSelYear)
		{
			// 検索条件
			//string strSClassID = "1211";
			//string strDispOffSelYear = "2011";                                      // 表示="0"にする商品の最大年

			int lngDispOffDispSyohinName = 4;                                       // 表示="0"にする表示商品名の最大文字数

			// 商品マスタ
			string[] strHdr = { "商品ID", "取込", "表示", "小分類ID", "商品名", "表示商品名", "安値", "評判", "評価", "発売時期" };


			int szItemBf = Kc.Const.z_nSyohinMax;                                   // 商品レコード最大数
			string strTableNm2 = Kc.Const.z_strSyohin2Table;                        // 商品テーブル名

			//int ihIdn = 0;
			int ihSyohinID = Array.IndexOf(strHdr, "商品ID");
			int ihDisp = Array.IndexOf(strHdr, "表示");                               // ヘッダーの"表示"の項目番号
			int ihDispSyohinName = Array.IndexOf(strHdr, "表示商品名");                      // ヘッダーの"発売時期"の項目番号
			int ihSelDate = Array.IndexOf(strHdr, "発売時期");                      // ヘッダーの"発売時期"の項目番号

			// 商品テーブルの全レコードを取得する
			int szHdr = strHdr.Length;
			string[,] strItemBf = new string[szItemBf, szHdr];                      // 全レコードの項目のリスト
			string strCondW = "小分類ID=" + strSClassID;
			int nRec;                                                               // レコード数
			Db.MySql.getTableItems(strTableNm2, strHdr, szHdr, strCondW, "商品ID", strItemBf, out nRec);

			// 全レコードの商品IDと図形コードを修正して新しいテーブルに追加する
			int szItemCache = 100;                                                  // 追加レコード一まとめ数
			string[,] strItemCache = new string[szItemCache, szHdr];

			for (int irCs = 0; irCs < nRec; irCs += szItemCache)
			{
				int nrC = (szItemCache < (nRec - irCs)) ? szItemCache : (nRec - irCs);

				bool fChange = false;
				for (int irC = 0; irC < nrC; irC++)
				{
					int ir = irCs + irC;

					//if (strItemss[ir, 0] != "12010046")
					//{
					//	continue;
					//}

					for (int it = 0; it < szHdr; it++)
					{
						strItemCache[irC, it] = strItemBf[ir, it];
					}

					fChange = false;
					// 発売時期の年が"-"または判定年以下なら、表示を"1"から"0"に変更する
					if (strItemCache[irC, ihDisp] == "1")
					{
						// 表示商品名が判定文字数以下なら、表示を"1"から"0"に変更する
						if (strItemCache[irC, ihDispSyohinName].Length <= lngDispOffDispSyohinName)
						{
							strItemCache[irC, ihDisp] = "0";
							fChange = true;
						}
						else
						{
							string strSelDate = strItemCache[irC, ihSelDate];
							if (strSelDate == "未定" || strSelDate == "")
                            {

                            }
							else if (strSelDate == "-")
							{
								// 発売時期の年が"-"なら、表示を"1"から"0"に変更する
								strItemCache[irC, ihDisp] = "0";
								fChange = true;
							}
							else
							{
								// 発売時期の年が判定年以下なら、表示を"1"から"0"に変更する
								string strSelYear = strSelDate.Substring(0, 4);
								if (strSelYear.CompareTo(strDispOffSelYear) <= 0)
								{
									strItemCache[irC, ihDisp] = "0";
									fChange = true;
								}
							}
						}
					}

					if (fChange)
					{
						int ist = 0;
						// 小分類マスタ
						string[] strHdr1 = { "表示" };
						string[] strItem1s = { "0" };
						string strCond1 = "商品ID=" + strItemCache[irC, ihSyohinID];
						ist = Db.MySql.modifySingleCondRecordItems1D(Const.z_strSyohin2Table, strHdr1, 1, strCond1, strItem1s);
						if (ist != 0)
						{
							string strMsg = "商品テーブル　商品ID=" + strItemCache[irC, ihSyohinID] + " 表示=0 処理エラー";
							Dbg.Utl.MessageBoxShow(strMsg, "DB処理 エラー");
							return;
						}
					}
				}

				//if (fChange)
				//            {
				//                Db.MySql.insMultiRecord(strTableNm2, strItemCache, nrC, szHdr);
				//            }
			}
		}

	}

}
