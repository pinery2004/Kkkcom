using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.IO;
//using System.Diagnostics;
using System.Windows.Forms;

namespace Dbg
{
	class Utl
	{
		// エラー表示
		public static void MessageBoxShow(string i_strMsg, string i_strTitle)
		{
			MessageBox.Show(i_strMsg, i_strTitle, MessageBoxButtons.OK);
			return;
		}

		// オーバーフローチェック
		public static bool CheckOverflow(
			bool i_bOverflow,					// オーバーフロー判定
			string i_strMsg,					// エラーメッセージ
			string i_strTitle					// エラータイトル
			)
		{
			if(i_bOverflow)
			{
				MessageBoxShow(i_strMsg + "バッファがオーバーフローしています", i_strTitle + "エラー");
			}
			return i_bOverflow;
		}

		// エラーチェック
		public static bool CheckError(
			bool i_bError,						// エラー判定
			string i_strMsg,					// エラーメッセージ
			string i_strTitle					// エラータイトル
			)
		{
			if (i_bError)
			{
				MessageBoxShow(i_strMsg + "エラーが発生しました", i_strTitle + "エラー");
			}
			return i_bError;
		}
	}
}
