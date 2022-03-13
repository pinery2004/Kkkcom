using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTML
{
	public partial class Table
	{
		/// <summary>
		/// 小分類ページデータのテーブルから商品の属性名リストを取得
		/// </summary>
		/// <param name="o_strAttrNames">属性名</param>
		/// <param name="o_nAttrNames">属性名数</param>
		/// <returns>tableのtag番号</returns>
		public static int getAttrName( string[] o_strAttrNames, out int o_nAttrNames)
		{
			int irt;
			int iTagTableLn = 0;
			int iTagTableRn = 0;
			int iTagTrLn = 0;
			int iTagTrRn = 0;
			int iTagThLn = 0;
			int iTagThRn = 0;
			int inTr = 0;                       // 属性検出後の<tr>カウント　(属性名取得+1カウントのみ検査)
			string strClass;
			// バッファクリア
			o_nAttrNames = 0;
			int iszAttrNames = o_strAttrNames.Length;
			Array.Clear(o_strAttrNames, 0, iszAttrNames);
			// o_strAttrNames[0] = "商品名";
			// o_nAttrNames = 1;

			// 属性名取得
			for (int iTagTablen = 0; ; iTagTablen = iTagTableRn + 1)
			{
				// <table>
				// 先頭または</table>の次から検索し、レベル1の<table>と</table>の位置を検索する
				irt = getNextTagPair(iTagTablen, 0, "table", "", "", 1, out iTagTableLn, out iTagTableRn);
				if (irt < 0)
					break;

				// <table>内の<tr>を検索 (最初はclass="tr-border", 次はclassのチェックなし)
				string[] srcTrClass = { "class", "tr-border theadItemRow" };
				int iendTagTrn = iTagTableRn;
				for (int istartTagTrn = iTagTableLn + 1; ; istartTagTrn = iTagTrRn + 1)
				{
					// 先頭または</tr>の次から検索し、1レベル下(</tr>と同レベル)の<tr>と</tr>の位置を検索する
					//					int isrchTrstart = iTagTrRn + 1;
					irt = getNextTagPair(istartTagTrn, iendTagTrn, "tr", srcTrClass[0], srcTrClass[1], 1, out iTagTrLn, out iTagTrRn);
					if (irt < 0)
						break;
					srcTrClass[0] = "";							// 次はclassのチェックなし
					srcTrClass[1] = "";
					// <table>内の<th>を検索し属性名を取得
					int iendTagThn = iTagTrRn;
					for (int istartTagThn = iTagTrLn + 1; ; istartTagThn = iTagThRn + 1)
					{
						// 先頭または</th>の次から検索し、1レベル下(</th>と同レベル)の<th>と</th>の位置を検索する
						//						int isrchThstart = iTagThRn + 1;
						irt = getNextTagPair(istartTagThn, iendTagThn, "th", "", "", 1, out iTagThLn, out iTagThRn);
						if (irt < 0)
							break;
						if (getTagAttrValue(iTagThLn, "class", out strClass) == false)
							continue;
						if (strClass == "thHeaderCheck left")
							continue;
						if (Left(strClass,8) == "thHeader" || Left(strClass,12) == "sub thHeader")
						{
							// <th>～</th>のTextをひとつにまとめて<th>のValueとする
							if (o_nAttrNames >= iszAttrNames)
							{
								//							Debug.Assert(true, "o_strAttrNames バッファオーバーフロー");
								Dbg.Utl.MessageBoxShow("o_strAttrNames バッファオーバーフロー", "getAttrNameエラー");
								break;
							}
							for (int iTagn = iTagThLn; iTagn < iTagThRn; iTagn++)
							{
								o_strAttrNames[o_nAttrNames] += getTagValue(iTagn);
							}
							o_nAttrNames++;
						}
					}
					if (o_nAttrNames > 0)
					{
						inTr++;
						if (inTr == 2)							// 属性名が入っている<tr>は2つのみ検索
							break;
					}
				}
				if (o_nAttrNames > 0)
					break;										// 属性名が入っている<table>は2つのみ検索
			}
			return iTagTableLn;
		}

        // 商品の属性名リストの補正
        // 2018/05/24 追加. (最安価格（新品）を　最安価格に変更機能)
        public static void modAttrName(
			string[] io_strAttrNames,			// 属性名
			ref int io_nAttrNames)				// 属性名数
		{
            for (int iz = io_nAttrNames - 1; iz >= 0; iz--)
			{
                if (io_strAttrNames[iz].Equals("最安価格（新品）"))
                {
                    io_strAttrNames[iz] = "最安価格";                                       // 2018/05/24
                }

                io_strAttrNames[iz + 2] = io_strAttrNames[iz].Replace("\r\n", "").Trim();
			}
			io_strAttrNames[0] = "メーカー";
			io_strAttrNames[1] = "商品名";
			io_nAttrNames += 2;
		}

		// 小分類ページデータのテーブルから複数の商品の属性値リストを取得
		public static int getAttrValue(			// tableのtag番号
			string[] i_strAttrNames,			// 主属性名
			int i_nAttrNames,					// 属性名数
			string[] o_strAttrNameBf,			// 属性名バッファ
			string[] o_strAttrValueBf,			// 属性値バッファ
			int[] o_iflgAttrValueBf,			// 属性先頭フラグ	1:商品の先頭属性
			out int o_nAttrValue,				// 属性値数
			string[] o_strPhotoURLBf,			// 一覧表示用商品写真URLバッファ
			string[] o_strAttrURLBf,			// 商品紹介URLバッファ
			out int o_nAttrURL)					// 商品紹介URL数
		{
			int irt;
			int iTagTdLn = 0;
			int iTagTdRn = 0;
			//			Attribute atr1, atr2;
			string strClass;
			string strHref;
			string strSrc;




			bool bAttrflg = false;				// 商品の属性の先頭フラグ
			string[] strAttrXpName = new string[100];
			string[] strAttrXpValue = new string[100];
			int nAttrXp = 0;
			int iTagAttrStart = 0;
			int iTagAttrEnd = 0;
			int irt2;
			int iTagSpanVbLn, iTagSpanVbRn;

			// 属性バッファクリア
			int iszAttrValues = o_strAttrValueBf.Length;
			Array.Clear(o_strAttrNameBf, 0, o_strAttrNameBf.Length);
			Array.Clear(o_strAttrValueBf, 0, o_strAttrValueBf.Length);
			Array.Clear(o_iflgAttrValueBf, 0, o_iflgAttrValueBf.Length);
			o_nAttrValue = 0;

			int iszAttrURLBf = o_strAttrURLBf.Length;
			Array.Clear(o_strPhotoURLBf, 0, o_strPhotoURLBf.Length);
			Array.Clear(o_strAttrURLBf, 0, o_strAttrURLBf.Length);
			o_nAttrURL = -1;

			// 属性値取得
			int iAttrn = 0;
			int iRanktd1 = 1;
			int iTagTableLn = 0;
			int iTagTableRn = 0;
			for (int iTagTablen = 0; ; iTagTablen = iTagTableRn + 1)
			{
				// 1. 全html文から最上位レベルの<table>を検索し
				// <table>～</table>内にひとかたまりの商品の属性群を探す、商品の属性群がなければ次のテーブル内を探す
				irt = getNextTagPair(iTagTablen, 0, "table", "", "", 1, out iTagTableLn, out iTagTableRn);
				if (irt < 0)
					break;

				// 2. <table>～</table>内のclass="ckitemLink"の<td>を検索
				// <Table>の次または</td>の次から検索し、1レベル下(</td>と同レベル)の<td>と</td>の位置を検索する

				// <td>を検索し属性名を取得
				int iendTagTdn = iTagTableRn;
				for (int istartTagTdn = iTagTableLn + 1; istartTagTdn < iendTagTdn; istartTagTdn = iTagTdRn + 1)
				{
					// </td>の次から検索し、1レベル下(</td>と同レベル)の<td>と</td>の位置を検索する
					irt = getNextTagPair(istartTagTdn, iendTagTdn, "td", "", "", iRanktd1, out iTagTdLn, out iTagTdRn);
					if (irt < 0)
						break;
												// オーバーフローチェック
												if (o_nAttrValue >= iszAttrValues-10)
												{
													Dbg.Utl.MessageBoxShow("o_strAttrValues バッファオーバーフロー", "getAttrValueエラー");
													break;
												}
					o_iflgAttrValueBf[o_nAttrValue] = 0;
					iTagAttrStart = iTagTdLn;
					iTagAttrEnd = iTagTdRn;

					// class属性による判定
					if (getTagAttrValue(iTagTdLn, "class", out strClass))
					{
						// 商品の先頭属性
						if (strClass == "end checkItem")
						{
							iRanktd1 = 1;						// </TD>が次の<TD call=ckitemLink></TD>の後にあるので、次は一段奥のレベルのものを検索
							iTagTdRn = iTagTdLn;				// 次は<TD>に続くタグから検索
							continue;
						}
						// 商品ページへのリンクURLを取得
						else if (strClass == "ckitemLink")
						{
							iRanktd1 = 0;						// 次の属性までの間に</TD>が2つあるので、次は一段手前のレベルのものを検索
							bAttrflg = true;													// 商品の属性の先頭フラグをセットし以降の属性を取得する
							// 商品紹介URLを取得
												if (Dbg.Utl.CheckOverflow(o_nAttrURL >= iszAttrURLBf - 1, "o_strAttrURLBf", "getAttrValue")) break;
							if (getTagName(iTagTdLn + 1) == "a")
							{
																					//atr2 = m_Tags[iTagTdLn + 1]["href"];
								//if (atr2 != null)
								if (getTagAttrValue(iTagTdLn + 1, "href", out strHref))
								{
									o_nAttrURL++;
									o_strAttrURLBf[o_nAttrURL] = strHref;						// 商品ページへのリンクURL
								}
							}
							// メーカーと商品名の属性を取得し商品の属性の先頭フラグをセットする
							// if (m_Tags[iTagTdLn + 2].Name == "span" && m_Tags[iTagTdLn + 3].Name == "/span")
							if (checkTagName(iTagTdLn + 2, "span") && checkTagName(iTagTdLn + 3, "/span"))
							{
								iAttrn = 0;
								o_strAttrNameBf[o_nAttrValue] = i_strAttrNames[iAttrn];
								o_strAttrValueBf[o_nAttrValue] = getTagValue(iTagTdLn + 2);		// 商品の最初の属性値　(メーカー)
																								// 最後の空白文字を取り除く
								int isz = o_strAttrValueBf[o_nAttrValue].Length;
								if (o_strAttrValueBf[o_nAttrValue][isz - 1] == '　')
								{
									o_strAttrValueBf[o_nAttrValue] = o_strAttrValueBf[o_nAttrValue].Substring(0, isz - 1);
								}
								o_iflgAttrValueBf[o_nAttrValue] = 1;                            // 商品属性の属性の先頭フラグをセットする
								iAttrn++;
								o_nAttrValue++;

								o_strAttrNameBf[o_nAttrValue] = i_strAttrNames[iAttrn];
								o_strAttrValueBf[o_nAttrValue] = m_strTagValues[iTagTdLn + 3];  // 商品の2番目の属性値　(商品名)
								iAttrn++;
								o_nAttrValue++;
								continue;
							}
							// else if (m_Tags[iTagTdLn + 2].Name == "span" && m_Tags[iTagTdLn + 3].Name == "span" &&
							//	 	 m_Tags[iTagTdLn + 4].Name == "/span" && m_Tags[iTagTdLn + 5].Name == "/span")
							if (checkTagName(iTagTdLn + 2, "span") && checkTagName(iTagTdLn + 3, "span") &&
								checkTagName(iTagTdLn + 4, "/span") && checkTagName(iTagTdLn + 5, "/span"))

							{
								iAttrn = 0;
								o_strAttrNameBf[o_nAttrValue] = i_strAttrNames[iAttrn];
								o_strAttrValueBf[o_nAttrValue] = getTagValue(iTagTdLn + 2) +
																 getTagValue(iTagTdLn + 3);	// 商品の最初の属性値　(メーカー)
								o_iflgAttrValueBf[o_nAttrValue] = 1;						// 商品属性の属性の先頭フラグをセットする
								iAttrn++;
								o_nAttrValue++;

								o_strAttrNameBf[o_nAttrValue] = i_strAttrNames[iAttrn];
								o_strAttrValueBf[o_nAttrValue] = getTagValue(iTagTdLn + 4) +
																 getTagValue(iTagTdLn + 5);	// 商品の2番目の属性値　(商品名)
								iAttrn++;
								o_nAttrValue++;
								continue;
							}
							else
							{
								Dbg.Utl.MessageBoxShow("メーカー・商品名エラー", "getAttrValueエラー");
							}
						}
						else if (strClass == "alignC")
						{
							iRanktd1 = 1;
							// 一覧表示用商品写真URL
							if (getTagName(iTagTdLn + 2) == "img")
							{
								// atr2 = m_Tags[iTagTdLn + 2]["src"];
								//if (atr2 != null)
								if (getTagAttrValue(iTagTdLn + 2, "src", out strSrc))
									o_strPhotoURLBf[o_nAttrURL] = strSrc;
							}
							continue;
						}
						else if (strClass == "td-price")
						{
							// 最安価格
							iRanktd1 = 1;
						}
						else if (strClass == "end ckitemSpec")
						{
							iRanktd1 = 1;
							// 補助属性の取得
							getAttrXp(iTagTdLn, iTagTdRn, strAttrXpName, strAttrXpValue, out nAttrXp);
							if (nAttrXp >= 1)
							{
								o_iflgAttrValueBf[o_nAttrValue] = 2;
								for (int iz = 0; iz < nAttrXp; iz++)
								{
									if (Dbg.Utl.CheckOverflow(o_nAttrValue >= iszAttrValues, "o_strAttrValueBf", "getAttrValue")) break;
									o_strAttrNameBf[o_nAttrValue] = strAttrXpName[iz];
									o_strAttrValueBf[o_nAttrValue] = strAttrXpValue[iz];
									o_nAttrValue++;
								}
							}
						}
						else
						{
							// その他のクラス名
							iRanktd1 = 1;
						}
					}
					else
					{
						// クラス名なし
						iRanktd1 = 1;
						// <span></span>内の class="variBlnHide"の<span>を検索
						irt2 = getNextTagPair(iTagTdLn + 1, iTagTdRn, "span", "class", "variBlnHide", 2, out iTagSpanVbLn, out iTagSpanVbRn);
						if (irt2 >= 0)
						{
							iTagAttrEnd = iTagSpanVbLn - 1;
						}
					}
					// 属性値を設定する
					if (bAttrflg)
					{
						if (Dbg.Utl.CheckOverflow(o_nAttrValue >= iszAttrValues, "o_strAttrValueBf", "getAttrValue")) break;
						o_strAttrNameBf[o_nAttrValue] = i_strAttrNames[iAttrn];
						iAttrn++;
						// <td>～</td>のTextをひとつにまとめて<td>のValueとする
						for (int iTagn = iTagAttrStart; iTagn < iTagAttrEnd; iTagn++)
						{
							if (getTagName(iTagn) == "/li")
							{
								o_strAttrValueBf[o_nAttrValue] += "|";
							}
							o_strAttrValueBf[o_nAttrValue] += getTagValue(iTagn);
						}
																							//// Debug 用　4月末までコーディングを挿入
																							//if (o_strAttrValueBf[o_nAttrValue].IndexOf("&amp;")>=0)
																							//{
																							//	Dbg.Utl.MessageBoxShow(o_strAttrValueBf[o_nAttrValue], "&amp;");
																							//}
																							//// ↑Debug用
						if (nAttrXp > 0)
						{
							o_iflgAttrValueBf[o_nAttrValue] = 3;
							nAttrXp = 0;
						}
						o_nAttrValue++;
					}
				}
				if (o_nAttrValue > 0)
					break;
			}
			o_nAttrURL++;
			return iTagTableLn;
		}

		// 小分類データのテーブルから次ページのURLを取得
		public static void getNextPageURL(
			out string o_strNextURL)			// 次ページURL (null:無し)
		{
			int irtn;
			int iTagn;
			o_strNextURL = "";
			for (int istartTagn = 1; ; istartTagn = iTagn + 1)
			{
				irtn = getNextTagSingle(1, 0, "img", "class", "pageNextOn", 1, out iTagn);
				if (irtn >= 0)
				{
					if (getTagName(iTagn - 1) == "a")
					{
						getTagAttrValue(iTagn - 1, "href", out o_strNextURL);
						break;
					}
					else
					{
						Dbg.Utl.MessageBoxShow("getNextPageURLでhrefなしエラー", "フォーマットエラー");
					}
				}
				else
				{
					break;
				}
			}
		}

		// 商品ページデータのテーブルから商品の仕様と図形のURLを取得
		public static void getSiyoURL(
			out string o_strMakerTopPageURL,		// メーカートップページ
			out string o_strSeihinInfoURL,			// メーカー製品情報URL
			out string o_strSeihinSiyoURL,			// メーカー仕様表URL
			out string o_strPreReleaseURL,			// プレリリース
			string[] o_strProductPictURL,			// 製品写真URL(主図形,サブ1図形,サブ2図形,･･･)
			string[] o_strFullSizeProductPictHPURL,	// フルスケール製品写真掲載ホームページのURL
			out int o_nProductPictURL				// 製品写真数
			)
		{
			// 仕様URL取得
			int irtn1, irtn2;
			int iTagUlLn, iTagUlRn;
			int iTagLiLn, iTagLiRn;
			string strAnVal;
			string strHref;

			// 初期化
			o_strMakerTopPageURL = "";
			o_strSeihinInfoURL = "";
			o_strSeihinSiyoURL = "";
			o_strPreReleaseURL = "";
			o_nProductPictURL = 0;

			irtn1 = getNextTagPair(0, 0, "ul", "id", "specInfo", 1, out iTagUlLn, out iTagUlRn);
			int iendTagLin = iTagUlRn;
			if (irtn1 >= 0)
			{
				for (int istartTagLin = iTagUlLn; ; istartTagLin = iTagLiRn + 1)
				{
					irtn2 = getNextTagPair(istartTagLin, iendTagLin, "li", "", "", 1, out iTagLiLn, out iTagLiRn);
					if (irtn2 < 0)
						break;
					int iTagAn = iTagLiLn + 1;
					if (getTagName(iTagAn) == "a")
					{
						if (getTagAttrValue(iTagAn, "href", out strHref))
						{
							strAnVal = getTagValue(iTagAn);
							if (strAnVal == "メーカー製品情報ページ")
							{
								o_strSeihinInfoURL = strHref;
							}
							else if (strAnVal == "メーカー仕様表")
							{
								o_strSeihinSiyoURL = strHref;
							}
							else if (strAnVal == "プレスリリース")
							{
								o_strPreReleaseURL = strHref;
							}
                            else if (strAnVal == "キャリア製品情報ページ")
                            {
								// o_strCarrierSeihinInfoURL = strHref;
							}
							else if (strAnVal == "メーカートップページ")
							{
								o_strMakerTopPageURL = strHref;
							}
							else
							{
								Dbg.Utl.MessageBoxShow("メーカー仕様URL取得で対象外のコード:" + strHref,
									"データエラー");
							}
						}
					}
					else
					{
						Dbg.Utl.MessageBoxShow("getSiyoURLでhrefなしエラー", "フォーマットエラー");
					}
				}
			}
			// 製品の図形のURLを取得
			getProductPictURL(o_strProductPictURL, o_strFullSizeProductPictHPURL, out o_nProductPictURL);

		}

		// 小分類ページデータのテーブルから小分類の商品の属性を取得
		public static int getSyohinAttrOfSClassPage(
			string[] o_strAttrNameBf,           // 商品の属性名
			string[] o_strAttrValueBf,          // 商品の属性値
			int[] o_iflgAttrValueBf,            // 商品の属性先頭フラグ	1:商品属性の先頭 2:補助属性の先頭 3:補助属性サマリ(debug用)
			out int o_nAttr,                    // 商品の属性数
			string[] o_strPhotoURLBf,           // 一覧表示用商品写真URL
			string[] o_strSyohinExpURLBf,       // 商品説明URL
			out int o_nSyohin,                  // 商品数
			out string o_strNextPageURL         // 次ページURL
			)
		{
			int ist = 0;
			o_strNextPageURL = "";
			o_nSyohin = 0;
			o_nAttr = 0;

			// 小分類ページデータのテーブルから商品の属性名リストを取得
			const int iszAttrNames = 500;
			string[] strAttrNames = new string[iszAttrNames];
			int nAttrNames;
			HTML.Table.getAttrName(strAttrNames, out nAttrNames);

			// 属性名出力
			if (Kc.Const.DEBUG)
			{
				for (int iz = 0; iz < nAttrNames; iz++)
				{
					Kc.Mkdb.DispLine2("\"" + strAttrNames[iz] + "\",");
				}
			}

			// 属性名補正
			HTML.Table.modAttrName(strAttrNames, ref nAttrNames);

			// 属性値取得
			HTML.Table.getAttrValue(strAttrNames, nAttrNames,
				o_strAttrNameBf, o_strAttrValueBf, o_iflgAttrValueBf, out o_nAttr,
				o_strPhotoURLBf, o_strSyohinExpURLBf, out o_nSyohin);

			// 属性出力
			if (Kc.Const.DEBUG)
			{
				for (int iz = 0; iz < o_nAttr; iz++)
				{
					//				Mkdb.DispLine2("[" + (iz + 1) + "] : {" + strAttrValues[iz] + "}");
					Kc.Mkdb.DispLine2("\"" + o_strAttrNameBf[iz] + "\":\"" + o_strAttrValueBf[iz] + "\",");
				}
				for (int iu = 0; iu < o_nSyohin; iu++)
				{
					Kc.Mkdb.DispLine2("\"一覧表示用商品写真URL\":\"" + o_strPhotoURLBf[iu] + "\",");
					Kc.Mkdb.DispLine2("\"商品紹介　HomePageURL\":\"" + o_strSyohinExpURLBf[iu] + "\",");
				}
			}
			// 次ページURL取得
			string strNextPageURL = "";
			o_strNextPageURL = "";
			HTML.Table.getNextPageURL(out strNextPageURL);
			if (strNextPageURL != "")
				o_strNextPageURL = Kc.Const.z_urlKakakuHome + strNextPageURL;

			// 次ページURL出力
			if (Kc.Const.DEBUG)
			{
				Kc.Mkdb.DispLine2("href = " + o_strNextPageURL);
			}
			return ist;
		}
	}
}
