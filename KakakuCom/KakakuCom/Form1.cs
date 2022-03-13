using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;
using System.Collections;

namespace Kc
{
	public partial class Form1 : Form
	{
		private static Form1 _form1Instance;

		public static Form1 Form1Instance
		{
			get { return _form1Instance; }
			set { _form1Instance = value; }
		}
		public string TextBox1Text
		{
			get { return textBox1.Text; }
			set { textBox1.Text = value; }
		}
		public string TextBox2Text
		{
			get { return textBox2.Text; }
			set { textBox2.Text = value; }
		}

		public static bool m_bDbConnect { get; set; }			// mySQL DB接続フラグ
		public static int m_iradioBtnHenkoMode { get; set; }	// mySQL 同一商品の変更モード (1:スキップ,2:新規商品のみ追加, 3:削除追加)
		public static int m_iradioBtnDB { get; set; }			// mySQL 処理対象DBフラグ (1:\mf1, 2:mf2)
		public static int m_iradioBtnServer { get; set; }		// mySQL 処理対象サーバーフラグ (1:windows,2:CENTOS)
		public static int m_iradioBtnIP { get; set; }			// mySQL 処理対象サーバーIP(192.168.1.x) (1:201,2:202)
		public static bool m_bChkErrorSkip { get; set; }		// 図形取込み時のエラースキップ
		public static bool m_bChkCategory { get; set; }			// カテゴリマスタ作成フラグ
		public static bool m_bChkClass { get; set; }			// 大中小分類マスタ作成フラグ
		public static bool m_bChkAttr { get; set; }				// 属性マスタ作成フラグ
		public static bool m_bChkURL { get; set; }				// URLマスタ作成フラグ
		public static bool m_bChkLogGoogle { get; set; }		// Google Log 取得フラグ
		public static bool m_bChkLogMy { get; set; }			// 自信 Log 取得フラグ
		public static bool m_bChkLogCustomer { get; set; }		// 顧客 Log 取得フラグ
		public static bool m_bChkLogOther { get; set; }			// その他 Log 取得フラグ
		public static bool m_bChkLogAll { get; set; }			// 全日 Log 取得フラグ
		public static int m_iradioBtnTablen { get; set; }		// テーブル (1:カテゴリ, 2:大分類, 3:中分類, 4:小分類, 5:中小分類関係,
																//			 6:属性名, 7:一般属性値, 8:URL型属性値, 9:商品, 10:制御, 11:LOG)
		public static string m_strTextSClassID { get; set; }	// 小分類ID
		public static string m_strTextSyohinID { get; set; }    // 商品ID

//        public static string[] m_strTableNames1 = {Kc.Const.z_strCategoryTable, Kc.Const.z_strLClassTable,
//                                                  Kc.Const.z_strMClassTable, Kc.Const.z_strSClass1Table,
//                                                  Kc.Const.z_strMSCRelation, Kc.Const.z_strAttributeNameTable,
//                                                  Kc.Const.z_strAttributeValue1Table, Kc.Const.z_strAttributeURL1Table,
//                                                  Kc.Const.z_strSyohin1Table, Kc.Const.z_strControlTable,
//                                                  Kc.Const.z_strLogTable, Kc.Const.z_strUserAgentTable};

        public static string[] m_strTableNames2 = {Kc.Const.z_strCategoryTable, Kc.Const.z_strLClassTable,
                                                  Kc.Const.z_strMClassTable, Kc.Const.z_strSClass2Table,
                                                  Kc.Const.z_strMSCRelation, Kc.Const.z_strAttributeNameTable,
                                                  Kc.Const.z_strAttributeValue2Table, Kc.Const.z_strAttributeURL2Table,
                                                  Kc.Const.z_strSyohin2Table, Kc.Const.z_strControlTable,
                                                  Kc.Const.z_strLogTable, Kc.Const.z_strUserAgentTable};

        // 初期化
        public Form1()
		{
			InitializeComponent();
 
            // 出力例：c:\c#\tips\016exepath\exepath2.exe

            btnClose.Enabled = true;
			btnConnect.Enabled = true;
			btnDisConnect.Enabled = false;
			btnDispTbl.Enabled = false;
			btnWriteTbl.Enabled = false;
			btnHtml.Enabled = false;
			btnCategory.Enabled = false;
			btnClass.Enabled = false;
			btnSyohin.Enabled = false;
			btnImpPic.Enabled = false;
			btnCreSClassHP.Enabled = false;
			btnCreSyohinHP.Enabled = false;
			btnDBSave.Enabled = true;
			btnDBLoad.Enabled = true;
			btnDeleteAttr.Enabled = false;
			btnDump.Enabled = true;
			btnCheck.Enabled = false;
			btnCheckZukei.Enabled = false;
			btnModifyAttr.Enabled = false;
			btnInsCtrl.Enabled = false;
			btnSiteMap.Enabled = false;
			btnSetSClassTorikomi.Enabled = false;
			btnClrSClassTorikomi.Enabled = false;
			btnClrSyohinTorikomi.Enabled = false;
			btnSyohinDispOff.Enabled = false;
			btnSyohinDispOn.Enabled = false;
			btnDispLog.Enabled = false;
			btnLogRecDel.Enabled = false;
			btnImportAll.Enabled = false;

			listBox1.Enabled = false;

			m_bDbConnect = false;				// mySQL DB 接続フラグ
			m_iradioBtnHenkoMode = 2;			// mySQL 同一商品の処理フラグ (1:スキップ,2:新規商品のみ追加, 3:削除追加)
			m_iradioBtnDB = 2;					// mySQL 処理対象DBフラグ (2:mf2(リリース),1:mf1(テスト))
			m_iradioBtnServer = 1;				// mySQL 処理対象サーバーフラグ (1:windows,2:CENTOS)
			m_iradioBtnIP = 1;					// mySQL 処理対象サーバーIP(192.168.1.x) (1:201,2:202)
			m_bChkErrorSkip = false;			// 図形取込み時のエラースキップ
			m_bChkCategory = false;				// カテゴリマスタ作成
			m_bChkClass = false;				// 大中小分類マスタ作成
			m_bChkAttr = true;					// 属性マスタ作成
			m_bChkURL = true;					// URLマスタ作成
			m_iradioBtnTablen = 4;				// 小分類
			m_bChkLogGoogle = false;			// Google Log 取得フラグ
			m_bChkLogMy = false;				// 自信 Log 取得フラグ
			m_bChkLogCustomer = true;			// 客 Log 取得フラグ
			m_bChkLogOther = false;				// その他 Log 取得フラグ
			m_bChkLogAll = false;				// 全日 Log 取得フラグ

			// ログ選択コンボボックス初期設定
			initLogCombo();

			// ログファイルオープン
			Syohin.OpenLogFile();

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Form1.Form1Instance = this;

			//外観をトグルボタンにする
			radioBtnSakujoTuika.Appearance = Appearance.Button;
			radioBtnSinkiTuika.Appearance = Appearance.Button;
			radioBtnSyohinSkip.Appearance = Appearance.Button;
			radioBtnWindows.Appearance = Appearance.Button;
			radioBtnCentos.Appearance = Appearance.Button;
			radioBtnMf1.Appearance = Appearance.Button;
			radioBtnMf2.Appearance = Appearance.Button;

			//Checked値と外観を手動で設定できるようにFalseにする
			radioBtnWindows.AutoCheck = true;
			radioBtnCentos.AutoCheck = true;
		}

		// 商品データ取込み, カテゴリ・分類取込み
		private void btnHtml_Click(object sender, EventArgs e)
		{
			btnWriteTbl.Enabled = false;
			
			// ヘッダー表示
			string strMsg = "商品データ, カテゴリ・分類取込み 開始";
			Mkdb.DispLine1(strMsg);

			// 取込開始小分類ID(SClassID)を取り込む
            string strFisrstSClassID = textBoxCtrlUrlFN.Text;
            if (strFisrstSClassID.Length > 0)
            {
                string strTp = strFisrstSClassID.Substring(0, 1);
                if (strTp == "g" || strTp == "G")
                {
                    strFisrstSClassID = "9999";
                }
                else if (strTp == "s" || strTp == "S")
                {
                    strFisrstSClassID = strFisrstSClassID.Substring(1, 4);
                }
            }

			// 商品取込み(指定取込み開始小分類ID以降のIDの商品を取り込む)
			Syohin.ImportSyohin(strFisrstSClassID);

			strMsg = "商品データ, カテゴリ・分類取込み 終了";
			Mkdb.DispLine2(strMsg);
			Mkdb.DispLine1(strMsg);
		}

		// 終了
		private void btnClose_Click(object sender, EventArgs e)
		{
			// ログファイルクローズ
			Syohin.CloseLogFile();

			Close();
		}

		// MySql DB オープン
		private void btnConnect_Click(object sender, EventArgs e)
		{
			string[] strDB = { Const.z_strDb1, Const.z_strDb2 };

			// DBオープン
			string strServer = (m_iradioBtnServer == 1) ? "Windows" : "Centos";
			string strSDMsg = strServer + " " + strDB[m_iradioBtnDB - 1] + " ";
			Mkdb.DispLine1(strSDMsg + "DBオープン開始");
			if (Db.MySql.Connect(strDB[m_iradioBtnDB - 1]) == 0)
			{
				radioBtnCentos.Enabled = false;
				radioBtnWindows.Enabled = false;
				radioBtnMf2.Enabled = false;
				radioBtnMf1.Enabled = false;
				//
				m_bDbConnect = true;
				btnClose.Enabled = false;
				btnConnect.Enabled = false;
				btnDisConnect.Enabled = true;
				btnDispTbl.Enabled = true;
				btnWriteTbl.Enabled = false;
				btnHtml.Enabled = true;
				btnCategory.Enabled = true;
				btnClass.Enabled = true;
				btnSyohin.Enabled = true;
				btnImpPic.Enabled = true;
				btnCreSClassHP.Enabled = true;
//				btnCreSyohinHP.Enabled = true;
				btnDBSave.Enabled = false;
				btnDBLoad.Enabled = false;
				btnDeleteAttr.Enabled = true;
				btnDump.Enabled = false;
				btnCheck.Enabled = true;
				btnCheckZukei.Enabled = true;
				btnModifyAttr.Enabled = true;
				btnInsCtrl.Enabled = true;
				btnSiteMap.Enabled = true;
				btnSetSClassTorikomi.Enabled = true;
				btnClrSClassTorikomi.Enabled = true;
				btnClrSyohinTorikomi.Enabled = true;
				btnSyohinDispOff.Enabled = true;
				btnSyohinDispOn.Enabled = true;
				btnDispLog.Enabled = true;
				btnLogRecDel.Enabled = true;
				btnImportAll.Enabled = true;

				listBox1.Enabled = true;
			}
			Db.MySql.setTimeoutTime();
			Mkdb.DispLine1(strSDMsg + "DBオープン終了");
		}

		// MySql DB クローズ
		private void btnDisConnect_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("DBクローズ開始");
			if (Db.MySql.DisConnect() == 0)
			{
				radioBtnCentos.Enabled = true;
				radioBtnWindows.Enabled = true;
				radioBtnMf1.Enabled = true;
				radioBtnMf2.Enabled = true;

				m_bDbConnect = false;
				btnClose.Enabled = true;
				btnConnect.Enabled = true;
				btnDisConnect.Enabled = false;
				btnDispTbl.Enabled = false;
				btnWriteTbl.Enabled = false;
				btnHtml.Enabled = false;
				btnCategory.Enabled = false;
				btnClass.Enabled = false;
				btnSyohin.Enabled = false;
				btnImpPic.Enabled = false;
				btnCreSClassHP.Enabled = false;
				btnCreSyohinHP.Enabled = false;
				btnDBSave.Enabled = true;
				btnDBLoad.Enabled = true;
				btnDeleteAttr.Enabled = false;
				btnDump.Enabled = true;
				btnCheck.Enabled = false;
				btnCheckZukei.Enabled = false;
				btnModifyAttr.Enabled = false;
				btnInsCtrl.Enabled = false;
				btnSiteMap.Enabled = false;
				btnSetSClassTorikomi.Enabled = false;
				btnClrSClassTorikomi.Enabled = false;
				btnClrSyohinTorikomi.Enabled = false;
				btnSyohinDispOff.Enabled = false;
				btnSyohinDispOn.Enabled = false;
				btnDispLog.Enabled = false;
				btnLogRecDel.Enabled = false;
				btnImportAll.Enabled = false;

				listBox1.Enabled = false;
			}
			Mkdb.DispLine1("DBクローズ終了");
		}

		// グリッドテーブルにMySql DBから取り込む
		private void btnDispTbl_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1(m_strTableNames2[m_iradioBtnTablen - 1] + "テーブル表示開始");
			Mkdb.DispLine2("テーブル表示開始");

			DispGridTbl(m_strTableNames2[m_iradioBtnTablen - 1]);

			btnClose.Enabled = false;
			btnConnect.Enabled = false;
			btnDisConnect.Enabled = true;
			btnDispTbl.Enabled = true;
			btnWriteTbl.Enabled = true;

			Mkdb.DispLine2("テーブル表示終了");
			Mkdb.DispLine1(m_strTableNames2[m_iradioBtnTablen - 1] + "テーブル表示終了");
		}

		// グリッドテーブルをMySql D/Bに書き込む
		private void btnWriteTbl_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("テーブル書込み開始");
			Mkdb.DispLine2("テーブル書込み開始");

			WriteGridTbl();

			Mkdb.DispLine2("テーブル書込み終了");
			Mkdb.DispLine1("テーブル書込み終了");
		}

		// サーバー Windows
		private void radioBtnWindows_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnServer = 1;
		}

		// サーバー CENTOS
		private void radioBtnCentos_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnServer = 2;
		}

		// リリース DB
		private void radioBtnMf2_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnDB = 2;
		}

		// テスト DB
		private void radioBtnMf1_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnDB = 1;
		}

		private void radioBtnSyohinSkip_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnHenkoMode = 1;
		}
		private void radioBtnSinkiTuika_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnHenkoMode = 2;
		}

		private void radioBtnSakujiTuika_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnHenkoMode = 3;
		}

		private void chkBoxCategory_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkCategory = (chkBoxCategory.CheckState == CheckState.Checked);
		}

		private void chkBoxClass_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkClass = (chkBoxClass.CheckState == CheckState.Checked);
		}

		private void chkBoxAttr_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkAttr = (chkBoxAttr.CheckState == CheckState.Checked);
		}

		private void chkBoxURL_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkURL = (chkBoxURL.CheckState == CheckState.Checked);
		}

		// DBの内容をファイルに印書する
		private void btnDump_Click(object sender, EventArgs e)
		{
			// ダンプ開始
			Mkdb.DispLine1("Dump開始");

			string strMsg;
			string strDBFilePathOrig;
			string strDBFilePath;

			string[,] strMsgs = { { "Windows mf1DB(mf1)", "CentOS mf1DB(mf1)" },
								  { "Windows mf2DB(mf2)", "CentOS mf2DB(mf2)" } };

			int iDB_No = m_iradioBtnDB - 1;
			int iCentOs = m_iradioBtnServer - 1;

			strMsg = strMsgs[iDB_No, iCentOs];
			strDBFilePathOrig = Kc.Const.z_pathMatukenDB + Kc.Const.z_pathDumpFile[iDB_No, iCentOs];

			getSaveFileName(false, strMsg + " Dumpファイル名の入力", strDBFilePathOrig,
						Kc.Const.z_strDBDumpFilter, out strDBFilePath);
			if (strDBFilePath != "")
			{
				try
				{
					Syohin.Dump(strDBFilePath);
				}
				catch (Exception ex)
				{
					Dbg.Utl.MessageBoxShow(ex.ToString(), "データベースDumpエラー");
				}
			}

			Mkdb.DispLine2("Dump終了");
			Mkdb.DispLine1("Dump終了");
		}

		// カテゴリマスタのテーブルを削除後再生成
		// 2021/11/03 起動用のボタンを削除
		private void btnCategory_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("カテゴリマスタを消去開始");

			DialogResult result = MessageBox.Show("カテゴリマスタを消去します。よろしいですか？",
				"質問",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button2);
			if (result == DialogResult.OK)
			{
				Syohin.OpenMySqlDB(1);
			}

			Mkdb.DispLine1("カテゴリマスタの消去終了");
		}

		// 大中小分類マスタのテーブルを削除後再生成
		// 2021/11/03 起動用のボタンを削除
		private void btnClass_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("大中小分類マスタを消去開始");

			DialogResult result = MessageBox.Show("大中小分類マスタを消去します。よろしいですか？",
				"質問",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button2);
			if (result == DialogResult.OK)
			{
				Syohin.OpenMySqlDB(2);
			}

			Mkdb.DispLine1("大中小分類マスタの消去終了");
		}

		// 商品と属性マスタのテーブルを削除後再生成
		// 2021/11/03 起動用のボタンを削除
		private void btnSyohin_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("商品と属性マスタを消去開始");

			DialogResult result = MessageBox.Show("商品と属性マスタを消去します。よろしいですか？",
				"質問",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button2);
			if (result == DialogResult.OK)
			{
				Syohin.OpenMySqlDB(3);
			}

			Mkdb.DispLine1("商品と属性マスタの消去終了");
		}

		// DB保存
		private void btnDBSave_Click(object sender, EventArgs e)
		{
			// DB保存開始
			Mkdb.DispLine1("DB保存開始");

			System.Diagnostics.Process prog = new System.Diagnostics.Process();
			string strMsg;

			string strDBFilePathTemp, strDBFilePathOrig;

			int iDB_No = m_iradioBtnDB - 1;
			int iCentOs = m_iradioBtnServer - 1;

			prog.StartInfo.FileName = Kc.Const.z_pathBackupDBShell + Kc.Const.z_strSaveBat[iDB_No, iCentOs];

			strMsg = Kc.Const.z_strSaveMsgs[iDB_No, iCentOs];

			Assembly myAssembly = Assembly.GetEntryAssembly();
			string drive = myAssembly.Location.Substring(0, 2);

			strDBFilePathTemp = drive + Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupTempFile[iDB_No, iCentOs];
			strDBFilePathOrig = drive + Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupSelectFile[iDB_No, iCentOs];

			bool bBackupFlg = false;
			if (m_iradioBtnServer == 2)
			{
				Dbg.Utl.MessageBoxShow("CentOsは保存対象外です", "btnDBLoad_Click");
			}
			else if (System.IO.File.Exists(strDBFilePathTemp))
			{
				// Dbg.Utl.MessageBoxShow("作業用一時ファイル:" + strDBFilePathTemp + " が存在し保存作業不可です", "btnDBLoad_Click");
				DialogResult result = MessageBox.Show("作業用一時ファイル:" + strDBFilePathTemp + " が存在し保存作業不可です\n" +
													  "作業用一時ファイルを削除します。よろしいですか？",
													  "質問",
													  MessageBoxButtons.OKCancel,
													  MessageBoxIcon.Exclamation,
													  MessageBoxDefaultButton.Button2);
				if (result == DialogResult.OK)
				{
					File.Delete(strDBFilePathTemp);
				}
			}
            else
            {
                DialogResult result = MessageBox.Show(strMsg + "を保存しますか？",
                    "質問",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.OK)
                {
                    bBackupFlg = true;
                }
            }
			if (bBackupFlg)
			{
				string strDBFilePath;
				string strOutDBFile;
				getSaveFileName(true, strMsg + "データベース保存　ファイル名の入力", strDBFilePathOrig,
							Kc.Const.z_strDBBackupFilter, out strDBFilePath);
				if(strDBFilePath != "")
				{
					try
					{
						prog.Start();
						prog.WaitForExit();
					}
					catch (Exception ex)
					{
						Dbg.Utl.MessageBoxShow(ex.ToString(), "データベース保存エラー");
					}
					try
					{
						if (strDBFilePathOrig != strDBFilePath)
						{
							// ファイルが存在するなら削除する
							Mkdb.DeleteFile(strDBFilePath);
							// ファイル名を作業用から保存用に変更する
							System.IO.File.Move(strDBFilePathTemp, strDBFilePath);
							// ファイルをコピーする
							strOutDBFile = drive + Kc.Const.z_pathTransDB[iDB_No] + Path.GetFileName(strDBFilePath);
                            System.IO.File.Copy(strDBFilePath, strOutDBFile, true);
						}
					}
					catch (Exception ex)
					{
						Dbg.Utl.MessageBoxShow(ex.ToString(), "ファイル名を作業用から保存用に変更エラー");

					}
				}
			}

			Mkdb.DispLine1("DB保存終了");
		}

		// DB回復
		private void btnDBLoad_Click(object sender, EventArgs e)
		{
			// DB回復開始
			Mkdb.DispLine1("DB回復開始");

			System.Diagnostics.Process prog = new System.Diagnostics.Process();
			string strMsg;
			string strDBFilePathTemp, strDBFilePathOrig;

			int iDB_No = m_iradioBtnDB - 1;
			int iCentOs = m_iradioBtnServer - 1;

			prog.StartInfo.FileName = Kc.Const.z_pathBackupDBShell + Kc.Const.z_strLoadBat[iDB_No, iCentOs];

			strMsg = Kc.Const.z_strLoadMsgs[iDB_No, iCentOs];
			strDBFilePathTemp = Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupTempFile[iDB_No, iCentOs];
			strDBFilePathOrig = Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupSelectFile[iDB_No, iCentOs];

			bool bRecoveryFlg = false;
			if (m_iradioBtnServer == 2)
			{
				Dbg.Utl.MessageBoxShow("CentOsは回復対象外です", "btnDBLoad_Click");
			}
			else if (System.IO.File.Exists(strDBFilePathTemp))
			{
				Dbg.Utl.MessageBoxShow("作業用一時ファイル:" + strDBFilePathTemp + " が存在し回復作業不可です", "btnDBLoad_Click");
			}
			else
			{
				DialogResult result = MessageBox.Show(strMsg + "を回復します。よろしいですか？",
					"質問",
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button2);
				if (result == DialogResult.OK)
				{
                    bRecoveryFlg = true;
				}
			}
			if (bRecoveryFlg)
			{
				string strDBFilePath;
				getOpenFileName(true, strMsg + " データベース回復　ファイル名の入力", strDBFilePathOrig,
							Kc.Const.z_strDBBackupFilter, out strDBFilePath);
				if (strDBFilePath != "")
				{
					// ファイル名をロード用に変更
					bool bCngFileName = false;					// 回復用ファイル名変更フラグ
					try
					{
						if (strDBFilePath != strDBFilePathTemp)
						{
							System.IO.File.Move(strDBFilePath, strDBFilePathTemp);
							bCngFileName = true;
						}
					}
					catch (Exception ex)
					{
						Dbg.Utl.MessageBoxShow(ex.ToString(), "ファイル名をロード用に変更エラー");
					}

					// データベース回復
					try
					{
						prog.Start();
						prog.WaitForExit();
					}
					catch (Exception ex)
					{
						Dbg.Utl.MessageBoxShow(ex.ToString(), "データベース回復エラー");
					}

					// ファイル名を元に戻す
					try
					{
						if (bCngFileName)
						{
							System.IO.File.Move(strDBFilePathTemp, strDBFilePath);
						}
					}
					catch (Exception ex)
					{
						Dbg.Utl.MessageBoxShow(ex.ToString(), "ファイル名を元に戻しエラー");
					}

				}
			}

			Mkdb.DispLine1("DB回復終了");
		}

		// 図形取得
		private void btnImpPic_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("図形取得開始");

			// 図形URLの図形を取得
			Syohin.importAllZukeiData();

			Mkdb.DispLine2("図形取得終了");
			Mkdb.DispLine1("図形取得終了");
		}

		// ファイル名を入力するダイアログを表示する
		private void getOpenFileName(
			bool i_bCheckFlag,					// 無ファイル名チェックフラグ (false:無チェック, true:チェック)
			string i_strTitle,					// タイトル
			string i_strFilePath,				// 初期表示ファイルパス
			string i_strFilter,					// ファイル選択フィルタ
			out string o_pathFile				// 入力ファイルパス
			)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			//はじめのファイル名を指定する
			//はじめに「ファイル名」で表示される文字列を指定する
			ofd.FileName = System.IO.Path.GetFileName(i_strFilePath);
			//はじめに表示されるフォルダを指定する
			//指定しない（空の文字列）の時は、現在のディレクトリが表示される
			ofd.InitialDirectory = System.IO.Path.GetDirectoryName(i_strFilePath);
			//[ファイルの種類]に表示される選択肢を指定する
			//指定しないとすべてのファイルが表示される
			ofd.Filter = i_strFilter;
			//[ファイルの種類]ではじめに
			//「すべてのファイル」が選択されているようにする
			ofd.FilterIndex = 2;
			//タイトルを設定する
			ofd.Title = i_strTitle;
			//ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
			ofd.RestoreDirectory = true;		// 
			//存在しないファイルの名前が指定されたとき警告を表示する
			//デフォルトでTrueなので指定する必要はない
			ofd.CheckFileExists = i_bCheckFlag;
			//存在しないパスが指定されたとき警告を表示する
			//デフォルトでTrueなので指定する必要はない
			ofd.CheckPathExists = true;

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				o_pathFile = ofd.FileName;
			}
			else
			{
				o_pathFile = "";
			}
		}

		// ファイル名を入力するダイアログを表示する
		private void getSaveFileName(
			bool i_bCheckFlag,					// 存在ファイル名チェックフラグ (false:無チェック, true:チェック)
			string i_strTitle,					// タイトル
			string i_strFilePath,				// 初期表示ファイルパス
			string i_strFilter,					// ファイル選択フィルタ
			out string o_pathFile				// 入力ファイルパス
			)
		{
			SaveFileDialog ofd = new SaveFileDialog();

			//はじめのファイル名を指定する
			//はじめに「ファイル名」で表示される文字列を指定する
			ofd.FileName = System.IO.Path.GetFileName(i_strFilePath);
			//はじめに表示されるフォルダを指定する
			//指定しない（空の文字列）の時は、現在のディレクトリが表示される
			ofd.InitialDirectory = System.IO.Path.GetDirectoryName(i_strFilePath);
			//[ファイルの種類]に表示される選択肢を指定する
			//指定しないとすべてのファイルが表示される
			ofd.Filter = i_strFilter;
			//[ファイルの種類]ではじめに
			//「すべてのファイル」が選択されているようにする
			ofd.FilterIndex = 2;
			//タイトルを設定する
			ofd.Title = i_strTitle;
			//ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
			ofd.RestoreDirectory = true;		// 
			////存在しないファイルの名前が指定されたとき警告を表示する
			//ofd.CheckFileExists = i_bCheckFlag;
			//存在するファイルの名前が指定されたとき警告を表示する
			//デフォルトでTrueなので指定する必要はない
			saveFileDialog1.OverwritePrompt = i_bCheckFlag;
			//存在しないパスが指定されたとき警告を表示する
			//デフォルトでTrueなので指定する必要はない
			ofd.CheckPathExists = true;

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				o_pathFile = ofd.FileName;
			}
			else
			{
				o_pathFile = "";
			}
		}

		private void textBoxSClassID_TextChanged(object sender, EventArgs e)
		{
			m_strTextSClassID = textBoxSClassID.Text;
		}

		private void textBoxSyohinID_TextChanged(object sender, EventArgs e)
		{
			m_strTextSyohinID = textBoxSyohinID.Text;
		}

		private void radioBtnRWCategory_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 1;
		}

		private void radioBtnRWLClass_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 2;
		}

		private void radioBtnRWMClass_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 3;
		}

		private void radioBtnRWSClass_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 4;
		}

		private void radioBtnRWMSCRelation_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 5;
		}

		private void radioBtnRWAttrname_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 6;
		}

		private void radioBtnRWAttrvalue_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 7;
		}

		private void radioBtnRWAttrURL_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 8;
		}

		private void radioBtnRWSyohin_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 9;
		}

		// グリッドテーブルの全行の[取込]を0クリア
		private void btnClrTorikomi_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("テーブルの[取込]を0クリア開始");

			clrAllTorikomi();

			Mkdb.DispLine1("テーブルの[取込]を0クリア終了");
		}

		// グリッドテーブルに小分類または商品が表示中なら[表示]>0の行の[取込]を1にセット
		private void btnSetTorikomi_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("テーブルの[表示]>0の行の[取込]を1にセット開始");

			setTorikomi();

			Mkdb.DispLine1("テーブルの[表示]>0の行の[取込]を1にセット終了");
		}

		// 商品HP作成
		private void btnCreSyohinHP_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("商品HP作成開始");

			Syohin.createSClassSyohinHP(2);

			Mkdb.DispLine1("商品HP作成終了");
		}

		// 小分類HP作成
		private void btnCreSClassHP_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("カテゴリHP作成開始");

			Syohin.createSClassSyohinHP(1);

			Mkdb.DispLine1("カテゴリHP作成終了");
		}

		// 図形取込みエラースキップボタン
		private void chkBoxErrorSkip_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkErrorSkip = (chkBoxErrorSkip.CheckState == CheckState.Checked);
			Mkdb.WriteLogFile("図形取込みエラースキップ設定:" + System.Convert.ToString(m_bChkErrorSkip));
		}

		private void btnTemp_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("Temp処理開始");

			//tempLearn();
			//deleteAttrName();

			//convertSyohinID();				// 旧テーブルの商品IDの追番と図形コードの商品ID部分の追番を1桁増やし新テーブルにコピーする
			//convertZukeiCode();				// 制御テーブルの図形コードの商品ID部分の追番を1桁増やす
			//diffSyohinID();					// 旧テーブルの商品IDの追番と図形コードの商品ID部分の追番を1桁増やし新テーブルと比較する
			//convertZukeiFileName();			// 制御テーブルの図形コードの商品ID部分の追番を1桁増やす
			//Syohin.testFindNofUrlPerPri();	// FindNofUrlPerPri 関数のテスト
			//correctMaxSyohinID();             // 最大商品IDの誤りを修正する

			bool FLGONESCLASS = true;
			if (FLGONESCLASS)
            {
				clearSyohinDispFlg1();          // 単一小分類の商品の表示フラグをOFF
			}
			else
            {
				Syohin.GarbageSyohin();			// 全小分類の商品の表示フラグをOFF

			}

			Mkdb.DispLine1("Temp処理終了");
		}

		private void btnCheck_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("チェック開始");

			// 最大商品IDの整合性チェック
			Syohin.checkMaxSyohinID();

			Mkdb.DispLine1("チェック終了");
		}

		private void btnCheckZukei_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("画像調査開始");

            string strFisrstSClassID = textBoxCtrlUrlFN.Text;
            if (strFisrstSClassID.Length > 0)
            {
                string strTp = strFisrstSClassID.Substring(0, 1);
                if (strTp == "g" || strTp == "G")
                {
                    strFisrstSClassID = strFisrstSClassID.Substring(1, 4);
                }
                else
                {
                    strFisrstSClassID = "";
                }
            }
            Syohin.checkZukeiCD(strFisrstSClassID);

			Mkdb.DispLine1("画像調査終了");
		}

		private void btnCheckAttr_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("属性変換開始");

			Syohin.checkAttr();

			Mkdb.DispLine1("属性変換終了");
		}

		private void listBox1_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.All;
		}
		private void listBox1_DragDrop(object sender, DragEventArgs e)
		{
			// string strType = textBoxCtrl.Text;
		    if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// DragDropしたファイル名をlistBox1に表示する
				foreach (string strFilePath in (string[])e.Data.GetData(DataFormats.FileDrop))
				{
					string strDelFile = Path.GetFileName(strFilePath);
					listBox1.Items.Add(strDelFile);
 				}
 			}
			// DragDropしたファイル名をDeleteフォルダに移動し
			Syohin.InsDeleteFigURL((string[])e.Data.GetData(DataFormats.FileDrop));
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void radioBtnCtrl_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 10;
		}

		private void btnInsCtrl_Click(object sender, EventArgs e)
		{
			string strType = textBoxCtrl.Text;
			string strCtrlUrlFN = textBoxCtrlUrlFN.Text;
			Syohin.InsCtrlUrlFN(strType, strCtrlUrlFN);
		}

		private void btnClrSyohinTorikomi_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("商品全取込Clr開始");

			Syohin.ClrAllSyohinTorikomi();

			Mkdb.DispLine1("商品全取込Clr終了");
		}

		private void btnSiteMap_Click(object sender, EventArgs e)
		{
			int[] icntPri;
			string[] strTtl;
			string[] strPri;
			string strItem;

			Mkdb.DispLine1("SiteMap作成開始");
			// https://omoto.me
            Syohin.SiteMap(0, out icntPri, out strTtl, out strPri);

            listBox1.Items.Clear();
			listBox1.Items.Add("sitemap");
			int sumInctPri = 0;
			for (int ic = 0; ic < icntPri.Length; ic++)
			{
				strItem = string.Format("{2}\tP{3}:{1,5}", ic, icntPri[ic], strTtl[ic], strPri[ic]);
				listBox1.Items.Add(strItem);
				sumInctPri += icntPri[ic];
			}

			strItem = string.Format("\t合計　{0}個", sumInctPri);
			listBox1.Items.Add(strItem);

			Mkdb.DispLine1("SiteMap作成終了");
			// http://omoto.me
			Syohin.SiteMap(1, out icntPri, out strTtl, out strPri);
		}

		private void radioBtnLog_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 11;
		}

		private void btnSetSClassTorikomi_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("小分類全取込Set開始");

			Syohin.SetAllSClassTorikomi();

			Mkdb.DispLine1("小分類全取込Set終了");
		}

		private void btnClrSClassTorikomi_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("小分類全取込Clr開始");

			Syohin.ClrAllSClassTorikomi();

			Mkdb.DispLine1("小分類全取込Clr終了");
		}

		// Log表示
		private void btnDispLog_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("LOG表示開始");
			btnWriteTbl.Enabled = false;

			DispLog();

			Mkdb.DispLine1("LOG表示終了");
		}

		// Google Log 取得フラグ
		private void chkLogGoogle_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkLogGoogle = (chkLogGoogle.CheckState == CheckState.Checked);

		}

		// 自信 Log 取得フラグ
		private void chkLogMy_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkLogMy = (chkLogMy.CheckState == CheckState.Checked);
			

		}

		// 客 Log 取得フラグ
		private void chkLogCustomer_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkLogCustomer = (chkLogCustomer.CheckState == CheckState.Checked);

		}

		// 全日 Log 取得フラグ
		private void chkLogAll_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkLogAll = (chkLogAll.CheckState == CheckState.Checked);
		}

		private void chkLogOther_CheckedChanged(object sender, EventArgs e)
		{
			m_bChkLogOther = (chkLogOther.CheckState == CheckState.Checked);
		}

		// 全商品取込
		private void btnImportAll_Click(object sender, EventArgs e)
		{
			// 待ち時間後に実行開始
			int slHH = 0;
			int slMM = 0;
			for (; slHH >= 0; slHH--) {
				string strMsgW = slHH + "時間" + slMM + "分後に全商品取り込み開始";
				Mkdb.DispLine1(strMsgW);
				System.Threading.Thread.Sleep((Math.Min(slHH,1) * 60 + slMM) * 60 * 1000);
			}

			string strMsg = "全商品取込み 開始";
			Mkdb.DispLine1(strMsg);

			// 図形取込時のエラースキップ
			m_bChkErrorSkip = true;
            // 1. 商品データ取込み
            strMsg = "全商品取込み 商品データ取り込み";
            Mkdb.DispLine1(strMsg);
            string strFisrstSClassID = textBoxCtrlUrlFN.Text;
            if (strFisrstSClassID.Length > 0)
            {
                string strTp = strFisrstSClassID.Substring(0, 1);
                if (strTp == "g" || strTp == "G")
                {
                    strFisrstSClassID = "9999";
                }
                else if (strTp == "s" || strTp == "S")
                {
                    strFisrstSClassID = strFisrstSClassID.Substring(1, 4);
                }
            }
            Syohin.ImportSyohin(strFisrstSClassID);
            // 2. 画像調査
            strMsg = "全商品取込み 画像調査";
            Mkdb.DispLine1(strMsg);
            strFisrstSClassID = textBoxCtrlUrlFN.Text;
            if (strFisrstSClassID.Length > 0)
            {
                string strTp = strFisrstSClassID.Substring(0, 1);
                if (strTp == "g" || strTp == "G")
                {
                    strFisrstSClassID = strFisrstSClassID.Substring(1, 4);
                }
                else
                {
                    strFisrstSClassID = "";
                }
            }
            Syohin.checkZukeiCD(strFisrstSClassID);
            // 3. 図形URLの図形を取得
            strMsg = "全商品取込み 図形を取得";
            Mkdb.DispLine1(strMsg);
            Syohin.importAllZukeiData();
            // 4. 属性変換
            strMsg = "全商品取込み 属性変換";
            Mkdb.DispLine1(strMsg);
            Syohin.checkAttr();
			// 5. 商品HP作成開始
			//Syohin.createSClassSyohinHP(2);

			strMsg = "全商品取込み 終了";
			Mkdb.DispLine2(strMsg);
			Mkdb.DispLine1(strMsg);
		}

		private void radioBtnUserA_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnTablen = 12;
		}

		// ログ選択コンボボックスの初期設定
		private void initLogCombo()
		{
			ComboBoxItem item;
			string[,] cmbLogData = {
										{"クリア", ""},
										{"メーカ付", "text1 like 'D9M9'"},
										{"価格1.com", "118.103.17/,118.103.17."},
										{"中国", "text2 = 'china'"},
										{"楽天", "host like 'rakuten'"},
										{"Yahoo", "host like 'yahoo'"},
										{"Amazon", "host like 'amazon'"},
										{"Google", "host like 'google'"},
										{"Microsoft", "host like 'msnbot'"},
										{"TrendMicro", "host like 'trendmicro'"},
										{"正常ｸﾘｯｸ", "sel < '8'"},
										{"エラーｸﾘｯｸ", "sel > '899'"}
									};
			for (int ic = 0; ic < cmbLogData.GetLength(0); ic++)
			{
				item = new ComboBoxItem(cmbLogData[ic,1], cmbLogData[ic,0]);
				cmbLog.Items.Add(item);
			}
		}
		private void cmbLog_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBoxItem item = (ComboBoxItem)cmbLog.SelectedItem;
			textBoxCtrlUrlFN.Text = item.Id.ToString();

			if (item.Id.ToString() == "") {
				chkLogGoogle.Checked = false;		// Google Log 取得
				chkLogMy.Checked = false;			// 自信 Log 取得
				chkLogCustomer.Checked = true;		// 客 Log 取得
				chkLogOther.Checked = false;		// その他 Log 取得
				chkLogAll.Checked = false;			// 全日 Log 取得
			}
		}

        // LogDB保存
        private void btnLogDBSave_Click(object sender, EventArgs e)
        {
            // LogDB保存開始
            Mkdb.DispLine1("LogDB保存開始");

            System.Diagnostics.Process prog = new System.Diagnostics.Process();
            string strMsg;

            string strDBFilePathTemp, strDBFilePathOrig;

            //			int iDB_No = m_iradioBtnDB - 1;
            //			int iCentOs = m_iradioBtnServer - 1;

            prog.StartInfo.FileName = Kc.Const.z_pathBackupDBShell + Kc.Const.z_strSaveLogBat;

            strMsg = Kc.Const.z_strSaveLogMsgs;
            strDBFilePathTemp = Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupLogTempFile;
            strDBFilePathOrig = Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupLogSelectFile;

            bool bBackupFlg = false;
            if (m_iradioBtnServer == 2)
            {
                Dbg.Utl.MessageBoxShow("CentOsは保存対象外です", "btnLogDBLoad_Click");
            }
            else if (System.IO.File.Exists(strDBFilePathTemp))
            {
                // Dbg.Utl.MessageBoxShow("作業用一時ファイル:" + strDBFilePathTemp + " が存在し保存作業不可です", "btnLogDBLoad_Click");
                DialogResult result = MessageBox.Show("作業用一時ファイル:" + strDBFilePathTemp + " が存在し保存作業不可です\n" +
                                                      "作業用一時ファイルを削除します。よろしいですか？",
                                                      "質問",
                                                      MessageBoxButtons.OKCancel,
                                                      MessageBoxIcon.Exclamation,
                                                      MessageBoxDefaultButton.Button2);
                if (result == DialogResult.OK)
                {
                    File.Delete(strDBFilePathTemp);
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(strMsg + "を保存しますか？",
                    "質問",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.OK)
                {
                    bBackupFlg = true;
                }
            }
            if (bBackupFlg)
            {
                string strDBFilePath;
                getSaveFileName(true, strMsg + "データベース保存　ファイル名の入力", strDBFilePathOrig,
                            Kc.Const.z_strDBBackupFilter, out strDBFilePath);
                if (strDBFilePath != "")
                {
                    try
                    {
                        prog.Start();
                        prog.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Dbg.Utl.MessageBoxShow(ex.ToString(), "データベース保存エラー");
                    }
                    try
                    {
                        if (strDBFilePathOrig != strDBFilePath)
                        {
                            // ファイルが存在するなら削除する
                            Mkdb.DeleteFile(strDBFilePath);
                            // ファイル名を作業用から保存用に変更する
                            System.IO.File.Move(strDBFilePathTemp, strDBFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Dbg.Utl.MessageBoxShow(ex.ToString(), "ファイル名を作業用から保存用に変更エラー");

                    }
                }
            }

            Mkdb.DispLine1("LogDB保存終了");
        }

        // LogDB回復
        private void btnLogDBLoad_Click(object sender, EventArgs e)
        {
            // DB回復開始
            Mkdb.DispLine1("LogDB回復開始");

            System.Diagnostics.Process prog = new System.Diagnostics.Process();
            string strMsg;
            string strDBFilePathTemp, strDBFilePathOrig;

            prog.StartInfo.FileName = Kc.Const.z_pathBackupDBShell + Kc.Const.z_strLoadLogBat;

            strMsg = Kc.Const.z_strLoadLogMsgs;
            strDBFilePathTemp = Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupLogTempFile;
            strDBFilePathOrig = Kc.Const.z_pathBackupDBFolder + Kc.Const.z_pathBackupLogSelectFile;

            bool bRecoveryFlg = false;
            if (m_iradioBtnServer == 2)
            {
                Dbg.Utl.MessageBoxShow("CentOsは回復対象外です", "btnDBLoad_Click");
            }
            else if (System.IO.File.Exists(strDBFilePathTemp))
            {
                Dbg.Utl.MessageBoxShow("作業用一時ファイル:" + strDBFilePathTemp + " が存在し取込作業不可です", "btnDBLoad_Click");
            }
            else
            {
                DialogResult result = MessageBox.Show(strMsg + "を取込ます。よろしいですか？",
                    "質問",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.OK)
                {
                    bRecoveryFlg = true;
                }
            }
            if (bRecoveryFlg)
            {
                string strDBFilePath;
                getOpenFileName(true, strMsg + " Logデータベース取込　ファイル名の入力", strDBFilePathOrig,
                            Kc.Const.z_strDBBackupFilter, out strDBFilePath);
                if (strDBFilePath != "")
                {
                    // ファイル名をロード用に変更
                    bool bCngFileName = false;                  // 回復用ファイル名変更フラグ
                    try
                    {
                        if (strDBFilePath != strDBFilePathTemp)
                        {
                            System.IO.File.Move(strDBFilePath, strDBFilePathTemp);
                            bCngFileName = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Dbg.Utl.MessageBoxShow(ex.ToString(), "ファイル名をロード用に変更エラー");
                    }

                    // Logデータベース取込
                    try
                    {
                        prog.Start();
                        prog.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Dbg.Utl.MessageBoxShow(ex.ToString(), "Logデータベース取込エラー");
                    }

                    // ファイル名を元に戻す
                    try
                    {
                        if (bCngFileName)
                        {
                            System.IO.File.Move(strDBFilePathTemp, strDBFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Dbg.Utl.MessageBoxShow(ex.ToString(), "ファイル名を元に戻しエラー");
                    }

                }
            }

            Mkdb.DispLine1("LogDB回復終了");
        }

        private void radioBtn201_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnIP = 1;
		}

		private void radioBtn202_CheckedChanged(object sender, EventArgs e)
		{
			m_iradioBtnIP = 2;
		}

		private void btnCutRedLine_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("赤色枠線の消去開始");
			Syohin.cutFldrZukeiWaku("pc");
			Syohin.cutFldrZukeiWaku("kaden");
			Syohin.cutFldrZukeiWaku("camera");
			Mkdb.DispLine1("赤色枠線の消去終了");
		}

		private void btnLogRecDel_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("LOGレコード削除開始");
			btnWriteTbl.Enabled = false;

			DelLogRec();

			Mkdb.DispLine1("LOGレコード削除終了");
		}

		private void btnDeleteAttr_Click(object sender, EventArgs e)
		{
			Mkdb.DispLine1("小分類の属性名称、一般属性、URL属性を削除開始");
			btnWriteTbl.Enabled = false;

			DelAttr();

			Mkdb.DispLine1("小分類の属性名称、一般属性、URL属性を削除終了");
		}

		// 商品IDの商品の表示をOff(0)にする
        private void btnSyohinDispOff_Click(object sender, EventArgs e)
        {
			string strSyohinID = Form1.m_strTextSyohinID;
			Mkdb.DispLine1("商品ID=" + strSyohinID + " 商品の表示Off開始");
			Mkdb.DispLine2("商品の表示Off開始");

			if (strSyohinID != null && strSyohinID.CompareTo("120100000") == 1 &&
				 strSyohinID.CompareTo("322200000") == -1 )
			{
				Syohin.ClrSyohinDisp(strSyohinID);

				Mkdb.DispLine2("商品の表示Off終了");
				Mkdb.DispLine1("商品ID=" + strSyohinID + " 商品の表示Off終了");
			}
			else
            {
				Mkdb.DispLine2("商品ID誤りがあり商品の表示Off中断");
				Mkdb.DispLine1("商品ID=" + strSyohinID + " 商品の表示Off中断");
			}
		}

		// 商品IDの商品の表示をOff(0)にする
		private void btnSyohinDispOn_Click(object sender, EventArgs e)
        {
			string strSyohinID = Form1.m_strTextSyohinID;
			Mkdb.DispLine1("商品ID=" + strSyohinID + " 商品の表示On開始");
			Mkdb.DispLine2("商品の表示On開始");

			if (strSyohinID != null && strSyohinID.CompareTo("120100000") == 1 &&
				 strSyohinID.CompareTo("322200000") == -1)
			{
				Syohin.SetSyohinDisp(strSyohinID);

				Mkdb.DispLine2("商品の表示On終了");
				Mkdb.DispLine1("商品ID=" + strSyohinID + " 商品の表示On終了");
			}
			else
			{
				Mkdb.DispLine2("商品ID誤りがあり商品の表示On中断");
				Mkdb.DispLine1("商品ID=" + strSyohinID + " 商品の表示On中断");
			}
		}
	}
}
