using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kc
{
	class DbAttr
	{
		public static string m_strCurSClassID = "";										// カレント小分類ID
		public static int[] m_idAttrNames = new int[Kc.Const.z_nSClassAttrNameMax];			// 小分類内の[属性ID]のリスト
		public static string[] m_strAttrNames = new string[Kc.Const.z_nSClassAttrNameMax];	// 小分類内の[属性名]のリスト
		public static int m_nAttrID;													// 属性ID(属性名)数
		public static int m_idAttrMax;													// 一般属性(<1000)の最大属性ID

		public static string[] m_strFixAttrNames = { "メーカーTopPage", "メーカー製品情報", "メーカー仕様表", "プレリリース" };
		public static int m_nFixAttrName = 4;
		public static int[] m_idFixAttrNames = { 1001, 1002, 1003, 1004 };
		public static string[] m_strZukeiAttrNames = { "C図形", "S図形", "L図形", "P図形" };
		public static int m_nZukeiAttrName = 4;
		public static int[] m_idZukeiAttrNames = { 2000, 2100, 2200, 3200 };

		// 属性名検索用の初期設定
		// 小分類内の属性IDと属性名を検索用テーブルをクリア
		public static void clrAttrNameSearchTable()
		{
			m_strCurSClassID = "";
		}

		// 小分類内の属性IDと属性名を検索用テーブルに設定する
		public static int setAttrNameSearchTable(
			string i_strSClassID				// 小分類ID
			)
		{
			if (i_strSClassID != m_strCurSClassID)
			{
				m_strCurSClassID = i_strSClassID;

				// 小分類内の属性IDと属性名を取得する
				string strCond = "小分類ID=" + m_strCurSClassID;
				string[] strHdr = { "属性ID", "属性名" };
				string[,] strAttrItems = new string[Kc.Const.z_nSClassAttrNameMax, 2];		// 属性IDと属性名のリスト
				Db.MySql.getTableItems(Kc.Const.z_strAttributeNameTable, strHdr, 2, strCond, "", strAttrItems, out m_nAttrID);

				// URLと図形を除く一般属性の最大属性IDを求める
				m_idAttrMax = 0;
				for (int i = 0; i < m_nAttrID; i++)
				{
					int idAttr = int.Parse(strAttrItems[i, 0]);
					m_idAttrNames[i] = idAttr;
					m_strAttrNames[i] = strAttrItems[i, 1];
					if (idAttr < 1000 && idAttr > m_idAttrMax)
					{
						m_idAttrMax = idAttr;
					}
				}
			}
			return m_nAttrID;
		}

		// 属性IDをもとめる
		// 属性名のリストを検索し属性IDを求める、なければMax番号+1を追加する
		public static int getAttrID(
			string i_strAttrName				// 属性名
			)
		{
			int idAttr;
			int ia;
			// 1. 固定タイプの属性名であるか調べ属性IDを求める
			for (ia = 0; ia < m_nFixAttrName; ia++ )
			{
				if(i_strAttrName == m_strFixAttrNames[ia])
				{
					break;
				}
			}
			if (ia < m_nFixAttrName)
			{
				idAttr = m_idFixAttrNames[ia];
			}
			else
			{
				// 2. 図形タイプの属性名であるか調べ属性IDを求める
				for (ia=0; ia<m_nZukeiAttrName; ia++)
				{
					if (i_strAttrName.StartsWith(m_strZukeiAttrNames[ia]))
					{
						break;
					}
				}
				if (ia < m_nZukeiAttrName)
				{
					string strN = i_strAttrName.Substring(3);
					idAttr = m_idZukeiAttrNames[ia] + int.Parse(strN);
				}
				else
				{
					// 3. 一般の可変タイプの属性名を検索用テーブルで調べ属性IDを求める
					for (ia = 0; ia < m_nAttrID; ia++)
					{
						if (i_strAttrName == m_strAttrNames[ia])
						{
							break;
						}
					}
					if (ia < m_nAttrID)
					{
						idAttr = m_idAttrNames[ia];				// 設定されている属性ID
					}
					else
					{
						// 4. 属性名の属性が未設定なので小分類内の最大属性ID+1を求め追加する
						m_idAttrMax++;
						// 属性名と属性IDを求めMySqlの属性名マスタに追加する
						// string[] strHdr4 = { "小分類ID", "属性ID", "表示", "属性名", "表示属性名", "表示巾" };
						// string[] strType4 = { "int", "int", "tinyint", "varchar(50)", "varchar(50)", "int" };
						string[] strItem1ns = { m_strCurSClassID, m_idAttrMax.ToString(), "1", i_strAttrName, i_strAttrName, "" };
						Db.MySql.insRecord(Kc.Const.z_strAttributeNameTable, strItem1ns, 6);
						idAttr = m_idAttrMax;
						// 可変タイプの属性名を検索用テーブルに追加する
						m_strAttrNames[m_nAttrID] = i_strAttrName;
						m_idAttrNames[m_nAttrID] = m_idAttrMax;
						m_nAttrID++;
					}
				}
			}
			return idAttr;
		}
	}
}
