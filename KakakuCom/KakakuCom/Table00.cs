using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
//using System.Windows.Forms;
namespace HTML
{
	public partial class Table
	{
		public const int iszTags = 35000;
		public const int NO_CHECK = 99999;
		static int m_itp = 1;													// Debugウォッチ用
		// タグテーブル
		static int m_nTags = 0;													// Tagの数
		static AttributeList[] m_Tags = new AttributeList[iszTags];
		static string[] m_strTagValues = new string[iszTags];					// Tagに続くText
		static int[] m_iTagLines = new int[iszTags];							// Tagの行番号

		// タグテーブルの初期クリア
        private static void clearTable()
        {
            for (int it = 0; it < iszTags; it++)
            {
                m_strTagValues[it] = "";
            }
            m_nTags = 0;
			int m_dummy = m_itp;												// Debugウォッチ用 の変数　warning取り除くため
        }
		// 指定URLのページデータ(ソース)をタグテーブルに取り込み、タグの特殊文字コードを変換する
		public static void loadTable(string i_strUrl)
		{
			clearTable();
			// 指定URLのページデータ(ソース)をタグテーブルに取り込む
			string url;
			url = i_strUrl;
			string page = GetPage(url);
			if (page == null)
			{
				Kc.Mkdb.DispLine2("Can’t process that type of file,"
										  +
										  "please specify an HTML file URL."
										  );
				return;
			}

			ParseHTML parse = new ParseHTML();
			parse.Source = page;
			while (!parse.Eof())
			{
				int iLine = parse.GetCurrentLine();
				char ch = parse.Parse();
				if (ch == 0)
				{
					if (m_nTags == iszTags)
					{
						Dbg.Utl.MessageBoxShow("o_strAttrNames バッファオーバーフロー", "getAttrNameエラー");
						break;
					}
					m_Tags[m_nTags] = parse.GetTag();
					m_iTagLines[m_nTags] = iLine;
					m_nTags++;
				}
				else
				{
					if (m_nTags != 0 && ch != '\a')
						m_strTagValues[m_nTags - 1] += ch;
				}
			}
			// Valueの特殊文字コードを変換する
			string strTagValue;
			for (int it = 0; it < m_nTags; it++)
			{
				if (m_strTagValues[it] != null)
				{
					strTagValue = "";
					int nc = m_strTagValues[it].Count();
					for (int ic = 0; ic < nc; ic++)
					{
						char cTV = m_strTagValues[it][ic];
						if (ic == 0 && cTV == '　')
						{
							continue;
						}
						if ((cTV == '&') && ic < (nc-4))
						{
							int iadv;
							string str5ch = m_strTagValues[it].Substring(ic, 5);
							if (str5ch == "&#39;")
							{
								strTagValue += "\\\'";
								iadv = 4;
							}
							else if (str5ch == "&amp;")
							{
								strTagValue += "&";
								iadv = 4;
							}
							else if (ic < (nc - 5))
							{
								string str6ch = m_strTagValues[it].Substring(ic, 6);
								iadv = 5;
								if (str6ch == "&nbsp;")
								{
									strTagValue += " ";
								}
								else if (str6ch == "&#165;")
								{
									strTagValue += "￥";
								}
								else if (str6ch == "&#177;")
								{
									strTagValue += "±";
								}
								else if (str6ch == "&#215;")
								{
									strTagValue += "×";
								}
								else
								{
									strTagValue += cTV;
									iadv = 0;
								}
							}
							else
							{
								strTagValue += cTV;
								iadv = 0;
							}
							ic += iadv;
						}
						else
						{
							strTagValue += cTV;
						}

					}
					m_strTagValues[it] = strTagValue;
				}
			}
		}

		/*
		// タグテーブルから指定番目の<table>タグ位置を返す
		// 返値 n>0: タグ位置,  -1: なし
        private static int getTableTagn(
			int iTbln							// 番号(先頭からの番目)
			)
		{
			int iTblc = 0;
			for (int it = 0; it < m_nTags; it++)
			{
				if (m_Tags[it].Name == "table")
				{
					iTblc++;
					if (iTblc == iTbln)
						return it;
				}
			}
			return -1;
		}
		*/

		// 指定位置のタグのタグ名を確認する
		private static bool checkTagName(
			int i_iTagn,						// タグ位置
			string i_strTagName					// タグ名
			)
		{
			return (m_Tags[i_iTagn].Name == i_strTagName);
		}

		// 指定位置のタグのタグ名を取得する
		private static string getTagName(
			int i_iTagn							// タグ位置
			)
		{
			return m_Tags[i_iTagn].Name;
		}

		// 指定位置のタグのタグ名を確認しタグ値を取得
		// 返値　true:正しいタグ名でありタグ値を返す、false:タグ類誤り
		private static bool getValueOfTagName(
			int i_iTagn,						// タグ位置
			string i_strTagName,				// タグ名
			out string o_strTagValue			// タグ値
			)
		{
			o_strTagValue = "";
			bool bSt = (m_Tags[i_iTagn].Name == i_strTagName);
			if (bSt)
			{
				o_strTagValue = m_strTagValues[i_iTagn];
			}
			return bSt;
		}

		// 指定位置のタグのタグ値を取得
		// 返値　タグ値
		private static string getTagValue(
			int i_iTagn							// タグ位置
			)
		{
			return m_strTagValues[i_iTagn];
		}

		// 指定位置のタグの属性名の属性値を確認する(属性名が""の場合は属性が無いことを確認)
		// 返値　true:属性値一致または属性名が""で属性なし、false:属性値不一致
		private static bool checkTagAttr(
			int i_iTagn,						// タグ位置
			string i_strAttrName,				// 属性名	(""の場合は属性が無いことを確認)
			string i_strAttrValue				// 属性値
			)
		{
			if (String.IsNullOrEmpty(i_strAttrName))
			{
				return (m_Tags[i_iTagn].Count == 0);
			}
			else
			{
				Attribute atr = m_Tags[i_iTagn][i_strAttrName];
				return (atr != null && atr.Value == i_strAttrValue);
			}
		}

		// 指定位置のタグの属性名の属性値を取得
		// 返値　属性有無　(true:有り, false:無し)
		private static bool getTagAttrValue(
			int i_iTagn,						// タグ位置
			string i_strAttrName,				// 属性名
			out string o_strAttrValue			// 属性値
			)
		{
			o_strAttrValue = "";
			Attribute atr = m_Tags[i_iTagn][i_strAttrName];
			bool bSt = (atr != null);
			if (bSt)
			{
				o_strAttrValue = atr.Value;
			}
			return bSt;
		}

		// 指定範囲内の指定階層のタグを検索し、指定タグ名を持つ先頭のタグのタグ位置を取得
		// 返値　>0: 開始タグ位置, -1:なし
		private static int getNextTag(
			int i_istartTagn,					// 検索範囲 始点タグ位置
			int i_iendTagn,						// 検索範囲 終点グ位置 または 0:最終Tag
			string i_strTagName,                // 検索タグ名(｢開始タグ｣)
			int i_iTagRank						// 検索タグの相対階層(0:同一階層のタグのみ検索, n>0:階層がn段下のものを検索,
			)									//					  NO_CHECK:階層チェックなし)
		{
			int iendTagn = (i_iendTagn == 0) ? m_nTags-1:i_iendTagn;
			int iTagRank = 0;
			int nextTagn = -1;
			int iTagType;                       // 入力tagの種類   ( 0: "xxx" 1:"/xxx")
			string strTagNameS;
			string strTagNameE;

			if (i_iTagRank == NO_CHECK)
			{
				for (int it = i_istartTagn; it <= iendTagn; it++)
				{
					if (m_Tags[it].Name == i_strTagName)
					{
						nextTagn = it;
						break;
					}
				}
			}
			else
			{
				if (i_strTagName[0] == '/')
				{
					iTagType = 1;
					strTagNameS = i_strTagName.Substring(1);
					strTagNameE = i_strTagName;
				}
				else
				{
					iTagType = 0;
					strTagNameS = i_strTagName;
					strTagNameE = "/" + i_strTagName;
				}
				for (int it = i_istartTagn; it <= iendTagn; it++)
				{
					if (m_Tags[it].Name == strTagNameE)
					{
						iTagRank--;
/*
														if (strTagNameS == "div")
														{
															string strAttr = "";
        		    										if (m_Tags[it].Count > 0)
															{
																strAttr = m_Tags[it][0].Name + " = \"" + m_Tags[it][0].Value + "\""; 
															}
															Mkdb.DispLine2(iTagType.ToString() + ":" + strTagNameE + "[" +
																					 iTagRank.ToString() + "] (" +
																					 m_iTagLines[it].ToString() + ") " + strAttr);
														}
 */ 
						if (iTagType == 1)
						{
							if (i_iTagRank == iTagRank)
							{
								nextTagn = it;
								break;
							}
						}
					}
					else if (m_Tags[it].Name == strTagNameS)
					{
						iTagRank++;
 /* 
														if (strTagNameS == "div")
														{
															string strAttr = "";
        		    										if (m_Tags[it].Count > 0)
															{
																strAttr = m_Tags[it][0].Name + " = \"" + m_Tags[it][0].Value + "\""; 
															}
															Mkdb.DispLine2(iTagType.ToString() + ":" + strTagNameS + " [" +
																					 iTagRank.ToString() + "] (" +
																					 m_iTagLines[it].ToString() + ") " + strAttr);
														}
 */ 
						if (iTagType == 0)
						{
							if (i_iTagRank == iTagRank)
							{
								nextTagn = it;
								break;
							}
						}
					}
				}
			}
			return nextTagn;
		}
		// 指定範囲内の指定階層のタグを検索し、指定タグ名,属性名と属性値を持つ先頭のタグのタグ位置を取得
		// 返値　>0: 開始タグ位置, -1:なし
		private static int getNextTagSingle(
			int i_istartTagn,					// 検索範囲　始点タグ位置
			int i_iendTagn,						// 検索範囲　終点タグ位置 または 0:最終Tag
			string i_strTagNameS,				// 指定タグ名(｢開始タグ｣)
			string i_strAttrName,				// 指定属性名(null:属性の検査無し)
			string i_strAttrValue,				// 措定属性値
			int i_iTagRank,						// 検索タグの階層(0:同一階層のタグのみ検索, n>0:階層がn段下のものを検索,
			//									//				  NO_CHECK:階層チェックなし)
			out int o_iTagn)					// 検索されたタグの位置
		{
			int ist = 0;
			int iTagn;
			iTagn = -1;
			for (int istartTagn = i_istartTagn; ; istartTagn=iTagn+1)
			{
				iTagn = getNextTag(istartTagn, i_iendTagn, i_strTagNameS, i_iTagRank);
				if (iTagn < 0)
					break;
				if (String.IsNullOrEmpty(i_strAttrName))
				{
					break;
				}
				else
				{
					Attribute atr = m_Tags[iTagn][i_strAttrName];
					if (atr != null && atr.Value == i_strAttrValue)
						break;
				}
			}
			o_iTagn = iTagn;
			if (iTagn < 0)
				ist = -1;
			return ist;
		}
		// 指定範囲内の指定階層のタグを検索し、指定タグ名,属性名と属性値を持つ先頭のタグの始終タグ位置を取得
		// 返値　0: タグあり, -1:タグなし
		private static int getNextTagPair(
            int i_istartTagn,					// 検索開始タグ位置
            int i_iendTagn,						// 検索終了タグ位置 または 0:最終Tag
			string i_strTagNameS,				// 指定タグ名(｢開始タグ｣)
            string i_strAttrName,				// 指定属性名(null:属性の検査無し)
            string i_strAttrValue,				// 措定属性値
            int i_iTagRank,						// 検索タグの階層(0:同一階層のタグのみ検索, n>0:階層がn段下のものを検索,
            //									//				  NO_CHECK:階層チェックなし)
            out int o_iTagLn,					// 検索されたタグの開始位置
            out int o_iTagRn)					// 検索されたタグの終了位置
        {
            int ist = 0;
            int iTagRank = i_iTagRank;
            o_iTagLn = -1;
            o_iTagRn = -1;
            string strTagNameE = "/" + i_strTagNameS;

            // <table>内でclass="tr-border"の<tr>を検索
            for (int istartTagn = i_istartTagn; ; )
            {
                o_iTagLn = getNextTag(istartTagn, i_iendTagn, i_strTagNameS, i_iTagRank);
                if (o_iTagLn < 0)
                    break;
                /*
																if (i_iTagRank != NO_CHECK)
																{
																	o_iTagRn = getNextTag(o_iTagLn + 1, i_iendTagn, strTagNameE, -1);
																	if (o_iTagRn < 0)
																	{
																		MessageBoxShow("<" + i_strTagNameS + ">の後に</" + i_strTagNameS + ">が無い", "getNextTagPairエラー");

																		ist = -1;
																		break;
																	}
																}
                 */
                if (String.IsNullOrEmpty(i_strAttrName))
                {
                    break;
                }
                else
                {
                    Attribute atr = m_Tags[o_iTagLn][i_strAttrName];
                    if (atr != null && atr.Value == i_strAttrValue)
                        break;
                }
                // 異なる属性であったため続くTag以降で同一レベルのものを調べる
                if (i_iTagRank == NO_CHECK)
                {
                    istartTagn = o_iTagLn + 1;
                }
                else
                {
                    o_iTagRn = getNextTag(o_iTagLn + 1, i_iendTagn, strTagNameE, -1);
                    if (o_iTagRn < 0)
                    {
						Dbg.Utl.MessageBoxShow("<" + i_strTagNameS + ">の後に</" + i_strTagNameS + ">が無い", "getNextTagPairエラー");

                        ist = -1;
                        break;
                    }
                    iTagRank = 1;                               // 次の同一階層のタグの検索
                    istartTagn = o_iTagRn + 1;
                }
            }
            if (o_iTagLn >= 0)
            {
                o_iTagRn = getNextTag(o_iTagLn + 1, i_iendTagn, strTagNameE, -1);
                if (o_iTagRn < 0)
                {
					Dbg.Utl.MessageBoxShow("<" + i_strTagNameS + ">の後に</" + i_strTagNameS + ">が無い", "getNextTagPairエラー");

                    ist = -1;
                }
            }
            else
            {
                ist = -1;
            }
            return ist;

        }

		// 補助属性取得
		private static void getAttrXp(
			int i_isrchstartXp,					// 検索範囲　始点タグ位置
			int i_isrchendXp,					// 検査範囲　終了タグ位置
			string[] o_strAttrXpNames,			// 補助属性名
			string[] o_strAttrXpValues,			// 補助属性値
			out int o_nAttrXp)					// 補助属性数
		{
			int iTagSpanLn, iTagSpanRn;
			int iTagSpanVbLn, iTagSpanVbRn;
			string strClass;
			int irt1, irt2;
			int iszAttrXp = o_strAttrXpNames.Length;
			string strValue = "";
			int iBeforeTagSpanRn = 0;
			int iTagSpanRnsv = 0;

			o_nAttrXp = 0;

			int iendTagSpann = i_isrchendXp;
			// 検索範囲内の階層が１段下の<span>と</span>の位置を検索し
			// "specType" class
			for (int istartTagSpann = i_isrchstartXp + 1; istartTagSpann <= i_isrchendXp; istartTagSpann = iTagSpanRn + 1)
			{
				irt1 = getNextTagPair(istartTagSpann, iendTagSpann, "span", "", "", 1, out iTagSpanLn, out iTagSpanRn);
				if (irt1 < 0)
					break;
				iTagSpanRnsv = iTagSpanRn;
				if (getTagAttrValue(iTagSpanLn, "class", out strClass))
				{
					if (strClass == "specType")
					{
						// 前"specType"クラス<span> の 属性名と
						// 同</span>から次の"specType"クラス<span>直前までの属性値をまとめて補助属性に追加
						if (iBeforeTagSpanRn != 0)
						{
							strValue = "";
							for (int iTagn = iBeforeTagSpanRn; iTagn < iTagSpanLn; iTagn++)
							{
								strValue += getTagValue(iTagn);
							}
							o_strAttrXpValues[o_nAttrXp] = strValue;
							o_nAttrXp++;
							iBeforeTagSpanRn = 0;
						}

						if (o_nAttrXp >= iszAttrXp)
						{
							Dbg.Utl.MessageBoxShow("o_strAttrXp バッファオーバーフロー", "getAttrXpエラー");
							break;
						}
						o_strAttrXpNames[o_nAttrXp] = getTagValue(iTagSpanLn);
						iBeforeTagSpanRn = iTagSpanRn;

					}
					else if (strClass == "sortBox")
					{
						// 途中に内部に"variBlnHide"クラス<span>を持つ"sortBox"クラスがある場合は、
						// 前"specType"クラス<span> の 属性名と
						// 同</span>から次の"sortBox"クラス<span>内の"variBlnHide"クラス<span>直前までの属性値をまとめて補助属性に追加
						irt2 = getNextTagPair(iTagSpanLn + 1, iTagSpanRn, "span", "class", "variBlnHide", 1, out iTagSpanVbLn, out iTagSpanVbRn);
						if (irt2 >= 0)
						{
							if (iBeforeTagSpanRn != 0)
							{
								strValue = "";
								for (int iTagn = iBeforeTagSpanRn; iTagn < iTagSpanVbLn; iTagn++)
								{
									strValue += getTagValue(iTagn);
								}
								o_strAttrXpValues[o_nAttrXp] = strValue;
								o_nAttrXp++;
								iBeforeTagSpanRn = 0;
							}
						}
					}
				}
				//			} while (true);
			}
			// 続く"specType"クラス<span>または"variBlnHide"クラスを持つ"sortBox"クラス<span>が無い場合は
			// 前"specType"クラス<span> の 属性名と
			// 同</span>から最後の</span>までの属性値をまとめて補助属性に追加
			if (iBeforeTagSpanRn != 0)
			{
				strValue = "";
				for (int iTagn = iBeforeTagSpanRn; iTagn <= iTagSpanRnsv; iTagn++)
				{
					strValue += getTagValue(iTagn);
				}
				o_strAttrXpValues[o_nAttrXp] = strValue;
				o_nAttrXp++;
			}
			// 最後の文字が'：'または'　'の補助属性は最後の文字を取り除く
			for (int iz = 0; iz < o_nAttrXp; iz++)
			{
				int istrlen = o_strAttrXpNames[iz].Length;
				if (istrlen >= 1 && o_strAttrXpNames[iz][istrlen - 1] == '：')
				{
					o_strAttrXpNames[iz] = o_strAttrXpNames[iz].Substring(0, istrlen - 1);
				}
				istrlen = o_strAttrXpValues[iz].Length;
				if (istrlen >= 1 && o_strAttrXpValues[iz][istrlen - 1] == '　')
				{
					o_strAttrXpValues[iz] = o_strAttrXpValues[iz].Substring(0, istrlen - 1);
				}
			}
		}

		// 製品図形のURLを取得
		private static void getProductPictURL(
			string[] o_strProductPictURL,			// 製品写真URL(主図形,サブ1図形,サブ2図形,･･･)
			string[] o_strFullSizeProductPictHPURL,	// フルスケール製品写真掲載ホームページのURL
			out int o_nProductPictURL				// 製品写真数
			)
		{
			int nz = 0;
			int iaf = 0;								// 主図形のフル図形URL有無(0:無し、1:有り)
			int iszProductPicURL = o_strProductPictURL.Length;
			int irtn;
			string strHref;
			string strSrc;
			// 初期クリア
			for (int ip = 0; ip < iszProductPicURL; ip++)
			{
				o_strProductPictURL[ip] = "";
				o_strFullSizeProductPictHPURL[ip] = "";
			}
			o_nProductPictURL = 0;

			// 製品の主図形のURLを取得
			int iTagDiv1Ln, iTagDiv1Rn;
			irtn = getNextTagPair(0, 0, "div", "id", "imgBox", NO_CHECK, out iTagDiv1Ln, out iTagDiv1Rn);
			if (irtn >= 0)
			{
				if (getTagName(iTagDiv1Ln + 1) == "a")
				{
																							if( getTagAttrValue(iTagDiv1Ln + iaf + 1, "href", out strHref))
						o_strFullSizeProductPictHPURL[0] = strHref;		// フル主図形
					iaf = 1;
				}
				if (getTagName(iTagDiv1Ln + iaf + 1) == "img")
				{
					if (getTagAttrValue(iTagDiv1Ln + iaf + 1, "src", out strSrc))
					{
						o_strProductPictURL[0] = strSrc;				// 主図形
						nz++;
					}
					else
					{
						Dbg.Utl.MessageBoxShow("getProductPictURLで主図形のsrcなしエラー1", "フォーマットエラー");
					}
				}
				else
				{
					Dbg.Utl.MessageBoxShow("getProductPictURLで主フル図形のimgなしエラー2", "フォーマットエラー");
				}
			}
			else
			{
				Dbg.Utl.MessageBoxShow("getProductPictURLで主図形なし", "警告");
			}

			if (nz > 0 && iaf == 1)
			{
				// 製品のサブ図形のURLを取得
				int iTagDiv2Ln, iTagDiv2Rn;
				int iTagTdLn, iTagTdRn;
				int istartTagDivn = iTagDiv1Ln + 1;
				int iendTagDivn = iTagDiv1Rn - 1;
				irtn = getNextTagPair(istartTagDivn, 0, "div", "class", "photoSub", 1, out iTagDiv2Ln, out iTagDiv2Rn);
				if (irtn >= 0)
				{
					int iendTagTdn = iTagDiv2Rn - 1;

					for (int istartTagTdn = iTagDiv2Ln + 1; ; istartTagTdn = iTagTdRn + 1)
					{
						irtn = getNextTagPair(istartTagTdn, iendTagTdn, "td", "", "", 1, out iTagTdLn, out iTagTdRn);
						if (irtn >= 0)
						{
							if (m_Tags[iTagTdLn + 1].Name == "a")
							{
								if (getTagAttrValue(iTagTdLn + 1, "href", out strHref))
								{
									if (nz >= o_strProductPictURL.Length)
									{
										Dbg.Utl.MessageBoxShow("o_strProductPicURL バッファオーバーフロー", "getProductPictURLエラー");
										break;
									}
									o_strFullSizeProductPictHPURL[nz] = strHref;  // フル主図形
								}
								if (getTagName(iTagTdLn + 2) == "img")
								{
									if (getTagAttrValue(iTagTdLn + 2, "src", out strSrc))
									{
										o_strProductPictURL[nz] = strSrc;	// サブ図形
										nz++;
									}
									else
									{
										Dbg.Utl.MessageBoxShow("getProductPictURLでサブ図形のsrcなしエラー1", "フォーマットエラー");
										break;
									}
								}
								else
								{
									Dbg.Utl.MessageBoxShow("getProductPictURLでサブフル図形のimgなしエラー2", "フォーマットエラー");
									break;
								}
							}
						}
						else
						{
							break;
						}
					}
				}
			}
			o_nProductPictURL = nz;
		}

		// 読み込みテーブルの出力
		private static void Print()
		{
			int iDisp = 0;				// 0:表示
			for (int iTagn = 0; iTagn < m_nTags; iTagn++)
			{
															//			System.Console.Write("[" + iTagLines[it] + "]");
				string strTagNameS = m_Tags[iTagn].Name.ToLower();
				string strNextTagName = (iTagn < m_nTags-1) ? m_Tags[iTagn + 1].Name: "";
				if (strTagNameS == "script")
				{
					if (iDisp < 1)
						iDisp = 1;
				}
				if (strTagNameS == "a" || strTagNameS == "/a" || strTagNameS == "img")
				{
					if (iDisp == 0)
						iDisp = -1;
				}
				if (iDisp == 0)
				{
					if (strTagNameS == "table" || strTagNameS == "tr" || strTagNameS == "th" || strTagNameS == "td" || strTagNameS == "scan")
						Kc.Mkdb.DispLine2("");

					if (strTagNameS[0] == '/')
					{
						System.Console.Write("}");
					}
					else
					{
						System.Console.Write("{" + strTagNameS + ":");
					}
					// エラー判定
					if (m_Tags[iTagn].Value != "")
						Kc.Mkdb.DispLine2("**************************** " + m_Tags[iTagn].Value + " ******************************");

					for (int iz = 0; iz < m_Tags[iTagn].Count; iz++)
					{
						Attribute atr = m_Tags[iTagn][iz];
						System.Console.Write(atr.Name + "=" + atr.Value + " ");
					}
				}
				if (strTagNameS == "/script")
				{
					if (iDisp == 1)
						iDisp = 0;
				}
				if (iDisp < 0)
					iDisp = 0;
				if (iDisp == 0)
				{
					if (String.IsNullOrWhiteSpace(m_strTagValues[iTagn]) == false &&
							m_strTagValues[iTagn] != "\a" &&
							m_strTagValues[iTagn] != "\a\a")
						System.Console.Write("[" + m_strTagValues[iTagn] + "]");
				}
			}
		}

/*
		private static string GetPage(string url)
		{
		  WebResponse response = null;
		  Stream stream = null;
		  StreamReader
			reader = null;

		  try
		  {
			HttpWebRequest request =
						   (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";


			response = request.GetResponse();
			stream = response.GetResponseStream();

			if( !response.ContentType.ToLower().StartsWith("text/") )
			  return null;

			string buffer = "",line = "";

			reader = (new System.IO.StreamReader(stream, System.Text.Encoding.GetEncoding("shift_jis")));
			while ((line = reader.ReadLine()) != null)
			{
			  buffer+=line+"\a";						// 修正OK?
			}

			return buffer;
		  }
		  catch(WebException e)
		  {
			Kc.Mkdb.DispLine2("Can’t download:" + e);
			return null;
		  }
		  catch(IOException e)
		  {
			  Kc.Mkdb.DispLine2("Can’t download:" + e);
			return null;
		  }
		  finally
		  {
			if( reader!=null )
			  reader.Close();

			if( stream!=null )
			  stream.Close();

			if( response!=null )
			  response.Close();
		  }
		}
*/
		// urlのhtmlソースを得る
		private static string GetPage(string url)
		{
			string html = null;
			// WebClientクラスでWebページを取得するには ・OpenReadメソッド 文字化けなし
			//		http://www.atmarkit.co.jp/fdotnet/dotnettips/302wcget/wcget.html

			WebClient wc = new WebClient();
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);

			for (var Count = 0; ; Count++)
			{
				Stream st = null;
				try
				{
					//			Stream st = wc.OpenRead("http://www.google.co.jp/");
					st = wc.OpenRead(url);

					Encoding enc = Encoding.GetEncoding("Shift_JIS");
					StreamReader sr = new StreamReader(st, enc);
					html = sr.ReadToEnd();
					sr.Close();

					st.Close();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					if (st != null)
						throw;
					if (Count == 3)     //3回目のリトライ失敗
						break;
					if (Count == 0)
                    {
						url = url.Replace("/item/", "/altitem/");
					}
					System.Threading.Thread.Sleep(1000);    //1秒待つ
					continue;
				}
				break;
			}

			return html;
		}
/*
		public static string correctURL(string i_srcURL)
		{
			string srcURL;
			if (i_srcURL[0] == '/')
			{
				srcURL = Kc.Const.z_urlKakakuHome + i_srcURL;
			}
			else
			{
				srcURL = i_srcURL;
			}
			return srcURL;
		}
*/
	}
}
