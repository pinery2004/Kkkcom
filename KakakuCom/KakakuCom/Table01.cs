using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTML
{
	public partial class Table
	{
		/// <summary>
		/// 文字列の先頭から指定した長さの文字列を取得する
		/// </summary>
		/// <param name="str">文字列</param>
		/// <param name="len">長さ</param>
		/// <returns>取得した文字列</returns>
		public static string Left(string str, int len)
		{
			if (len < 0)
			{
				throw new ArgumentException("引数'len'は0以上でなければなりません。");
			}
			if (str == null)
			{
				return "";
			}
			if (str.Length <= len)
			{
				return str;
			}
			return str.Substring(0, len);
		}
		// ホームページデータのテーブルからカテゴリ情報を取得
		public static void getCategoryPage(
			string[] o_strCategoryNameBf,		// カテゴリ名
			string[] o_strCategoryURLBf,		// カテゴリURL
			string[] o_strCategoryDescriptBf,	// カテゴリ内容
			out int o_nCategory)				// カテゴリ数
		{
			int iszCategoryBf = o_strCategoryNameBf.Length;
			int irtn;
			// 初期クリア
			for (int ic = 0; ic < iszCategoryBf; ic++)
			{
				o_strCategoryNameBf[ic] = "";
				o_strCategoryURLBf[ic] = "";
			}
			o_nCategory = 0;

			// カテゴリのURLを取得
			int iTagDiv1Ln, iTagDiv1Rn;
			int iTagPLn, iTagPRn;
			irtn = getNextTagPair(0, 0, "div", "class", "category", NO_CHECK, out iTagDiv1Ln, out iTagDiv1Rn);
			int iendTagPn = iTagDiv1Rn;
			if (irtn >= 0)
			{
				for (int istartTagPn=iTagDiv1Ln; ; istartTagPn=iTagPRn+1)
				{
					// <p>～</p>のカテゴリ情報を取得する
					irtn = getNextTagPair( istartTagPn, iendTagPn, "p", "", "", 1, out iTagPLn, out iTagPRn); 
					if (irtn < 0)
						break;
					if (o_nCategory >= iszCategoryBf - 1)
					{
						Dbg.Utl.MessageBoxShow("カテゴリ名読み込み数オーバー", "オーバーフロー");
						break;
					}
					// カテゴリURLを取得
					if (!checkTagName(iTagPLn + 1, "a") ||
						!getTagAttrValue(iTagPLn + 1, "href", out o_strCategoryURLBf[o_nCategory]))
					{
						Dbg.Utl.MessageBoxShow("カテゴリURLがない", "フォーマットエラー");
						break;
					}
					// カテゴリ名を取得
					int iTagSn, iTagSRn;
					irtn = getNextTagPair(iTagPLn+1, iTagPRn-1, "strong", "", "", 1, out iTagSn, out iTagSRn);
					if (irtn < 0)
					{
						Dbg.Utl.MessageBoxShow("カテゴリ名がない", "フォーマットエラー");
						break;
					}
					// カテゴリ名を取得
					o_strCategoryNameBf[o_nCategory] = getTagValue(iTagSn);
					// カテゴリ内容を取得
					o_strCategoryDescriptBf[o_nCategory] = getTagValue(iTagSRn);
					string strCategoryDescript;
					if (getValueOfTagName(iTagSRn + 1, "br", out strCategoryDescript))
					{
						o_strCategoryDescriptBf[o_nCategory] += strCategoryDescript;
					}
					//if (!getValueOfTagName(iTagPLn + 2, "strong", out o_strCategoryNameBf[o_nCategory]))
					//{
					//	Dbg.Utl.MessageBoxShow("カテゴリ名がない", "フォーマットエラー");
					//	break;
					//}
					//// カテゴリ内容を取得
					//if (getValueOfTagName(iTagPLn + 3, "/strong", out o_strCategoryDescriptBf[o_nCategory]))
					//{
					//	string strCategoryDescript;
					//	if(getValueOfTagName(iTagPLn+4,"br",out strCategoryDescript))
					//	{
					//		o_strCategoryDescriptBf[o_nCategory] += strCategoryDescript;
					//	}
					//}
					o_nCategory++;
				}
			}
		}

		// カテゴリページデータのテーブルから大中小の分類名と小分類表示URLリストを取得
		public static void getSClassListOfCategory(
			string[] o_strLClassNameBf,			// 大分類リスト
			string[] o_strMClassNameBf,			// 中分類リスト
			string[] o_strSClassNameBf,			// 小分類リスト
			string[] o_strSClassURLBf,			// 小分類URLリスト
			out int o_nSClass					// 小分類数
			)
		{
			int iszSClassBf = o_strSClassNameBf.Length;
			int irtn;
			// 初期クリア
			for (int ic = 0; ic < iszSClassBf; ic++)
			{
				o_strLClassNameBf[ic] = "";
				o_strMClassNameBf[ic] = "";
				o_strSClassNameBf[ic] = "";
				o_strSClassURLBf[ic] = "";
			}
			o_nSClass = 0;

			// 小分類のURLを取得
			int iTagDiv1Ln, iTagDiv1Rn;
			int iCountLC = 0;
			irtn = getNextTagPair(0, 0, "div", "id", "menu", NO_CHECK, out iTagDiv1Ln, out iTagDiv1Rn);
			int iendTagDiv2n = iTagDiv1Rn;
			if (irtn >= 0)
			{
				int iTagDiv2Ln, iTagDiv2Rn;
				for (int istartTagDiv2n = iTagDiv1Ln + 1; ; istartTagDiv2n = iTagDiv2Rn + 1)
				{
					irtn = getNextTagPair(istartTagDiv2n, iendTagDiv2n, "div", "class", "menuBoxBtm", 1,
										  out iTagDiv2Ln, out iTagDiv2Rn);
					if (irtn < 0)
						break;
					int iTagDiv3n, iTagDiv3Rn;
					irtn = getNextTagPair(iTagDiv2Ln+1, iendTagDiv2n, "div", "class", "menuBox01", 1,
										  out iTagDiv3n, out iTagDiv3Rn);
					if (irtn < 0)
						break;
					if (!checkTagName(iTagDiv3n+1, "h2") ||
						!checkTagAttr(iTagDiv3n+1, "",""))
					{
						if ( iCountLC>0)
							Dbg.Utl.MessageBoxShow("importSClassListOfCategoryで大分類名がない", "フォーマットエラー");
						iCountLC++;
						continue;
					}

					string strLClassName = getTagValue(iTagDiv3n + 1);
					string strMClassName = "";
					int iTagLiLn, iTagLiRn;
					int iendTagLin = iTagDiv2Rn;
					for (int istartTagLin = iTagDiv2Ln + 2; ; istartTagLin = iTagLiRn + 1)
					{
						irtn = getNextTagPair(istartTagLin, iendTagLin, "li", "", "", 1, out iTagLiLn, out iTagLiRn);
						if (irtn < 0)
							break;
						// 中分類名を取得
						if (checkTagAttr(iTagLiLn, "class", "subTitle"))
						{
							strMClassName = getTagValue(iTagLiLn);
							continue;
						}
						if (checkTagAttr(iTagLiLn, "class", "submenuTtl"))
						{
							strMClassName = getTagValue(iTagLiLn);
							continue;
						}
						// 小分類URLを取得
						if (o_nSClass >= iszSClassBf)
						{
							Dbg.Utl.MessageBoxShow("小分類名バッファオーバー", "オーバーフローエラー");
							break;
						}
						if (!checkTagName(iTagLiLn + 1, "a") ||
							!getTagAttrValue(iTagLiLn + 1, "href", out o_strSClassURLBf[o_nSClass]))
						{
							Dbg.Utl.MessageBoxShow("小分類URLがない", "フォーマットエラー");
							break;
						}
						// 小分類名を取得
						o_strSClassNameBf[o_nSClass] = getTagValue(iTagLiLn + 1);
						o_strMClassNameBf[o_nSClass] = strMClassName;
						o_strLClassNameBf[o_nSClass] = strLClassName;
						o_nSClass++;
					}
				}
			}
		}
	}
}
