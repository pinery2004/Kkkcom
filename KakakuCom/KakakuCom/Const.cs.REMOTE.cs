using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kc
{
	public class Const
	{
        public static string z_strDrive = "D:";                 // 実行モジュール(とデータ)のドライブ名

        public static string z_strDb1 = "mf1";					// データベース1
        public static string z_strDb2 = "mf2";					// データベース2
		public static string z_strDbLog = "log";				// LOGデータベース
		public static string z_envmf = z_strDb1;				// 環境設定　"mf1" OR "mf2"
		public static string z_strHPUrl = "http://omoto.me";	// おもとめ　ホームページURL

		public static bool DEBUG = false;
		public static string[] z_pathDBFile = { "D100000.txt", "D200000.txt", "D300000.txt", "D400000.txt" };
		public static int z_nFilePath = z_pathDBFile.Count();
		public static string z_pathLogFile = "MatukenLog";
		public static string z_rpathKakakuData = "\\xampp\\htdocs\\Af0\\tmp\\";

		// 補正値
		public static double z_fMinKakakuHosei = 1.00;			// 最安値補正値

		// MAX値
		public static int z_nCategoryMax = 100;					// 全カテゴリ最大数
		public static int z_nLClassMax = 200;					// 全大分類最大数
		public static int z_nMClassMax = 350;					// 全中分類最大数
		public static int z_nSClassMax = 7000;					// 全小分類最大数
		public static int z_nMSCRelationMax = 7000;				// 全中小分類最大数
		public static int z_nAttrNameMax = 5000;				// 全属性名最大数
		public static int z_nAttrValueMax = 1650000;			// 全属性値最大数
		public static int z_nURLMax = 500000;					// 全URL最大数
		public static int z_nSyohinMax = 100000;				// 全商品最大数

		public static int z_nSClassSyohinMax = 7000;			// 小分類毎の商品最大数
		public static int z_nSClassAttrNameMax = 200;			// 小分類毎の[属性名]と[属性ID]の最大数 と 一般属性IDの最大番号(<1000)
		public static int z_nSClassAttrValueMax = 100000;		// 小分類HP内の属性最大数
		public static int z_nSClassZukeiMax = 30000;			// 小分類毎の図形最大数
		public static int z_nSyohinAttrNameMax = z_nSClassAttrNameMax;	// 商品毎の属性名最大数
		public static int z_nSyohinAttrValueMax = z_nSClassAttrNameMax;	// 商品毎の属性名最大数
		public static int z_nSyohinZukeiMax = 50;				// 商品毎の図形最大数
		public static int z_nProductInfoMax = 4;				// 商品毎のメーカー製品情報最大数
		public static int z_nCtrlIDMax = 1000;					// 制御レコード最大数

		public static int z_nTyumokuMax = 50;					// 商品の評判番号を算出用の最大注目番号
		public static int z_nKutikomiMax = 2000;				// 商品の評判番号を算出用の最大クチコミ件数
		// IDバイアス
		public static int z_iCategoryIDUnit = 1000;				// 大中小分類IDに付加するカテゴリIDの乗数
		public static int z_iLClassIDBias = 0;					// 大分類IDの開始番号
		public static int z_iMClassIDBias = 100;				// 中分類IDの開始番号
		public static int z_iSClassIDBias = 200;				// 小分類IDの開始番号
		public static int z_iSClassIDUnit = 10000;				// 商品IDに付加する小分類IDの乗数
		// ID先頭
		public static int z_idAttrURL = 1000;					// メーカー情報URL(TopPage(+0),製品情報(+1),仕様表(+2),プレリリース(+3))
		public static int z_idAttrTZukei = 2000;				// 一覧表示用図形URL
		public static int z_idAttrSZukei = 2100;				// 図形URL (主図形(+0),サブ図形(+1～+99)
		public static int z_idAttrLZukei = 2200;				// フルスケール図形URL (主図形(+0),サブ図形(+1～+99)
		public static int z_idAttrLPZukei = 3000;				// フルスケール表示ページURL (主図形(+0),サブ図形(+1～+99)
		// 名称
		public static string z_strCategoryTable = "category";
		public static string z_strLClassTable = "lclass";
		public static string z_strMClassTable = "mclass";
		public static string z_strSClassTable = "sclass";
		public static string z_strMSCRelation = "mscrelation";
		public static string z_strAttributeNameTable = "attrname";
		public static string z_strAttributeValueTable = "attrvalue";
		public static string z_strAttributeURLTable = "attrurl";
		public static string z_strSyohinTable = "syohin";
		public static string z_strControlTable = "control";
		public static string z_strLogTable = "pageview";
		public static string z_strUserAgentTable = "useragent";

		public static string z_urlKakakuDataHomePage = "http://kakaku.com/";			// ホームページURL

		public static string z_urlKakakuHome = "http://kakaku.com";						// 次ページURL作成用
		public static int z_nKakakuHomeUrl = z_urlKakakuHome.Length;					// 文字数

		public static string z_urlKakakuAllSyohin = "itemlist.aspx?pdf_so=s1";			// 全商品

		public static string z_rpathBackupDBShell = "\\xampp\\htdocs\\cf0\\Bin\\";		// DB Backup シェル　フォルダ
		public static string[,] z_strSaveBat = { { "winmf1db_save.bat", "centmf1db_save.bat" }, { "winmf2db_save.bat", "centmf2db_save.bat" } };
		public static string[,] z_strSaveMsgs = { { "Windows mf1DB(mf1)", "CentOS mf1DB(mf1)" }, { "Windows mf2DB(mf2)", "CentOS リリースDB(mf2)" } };
		public static string[,] z_strLoadBat = { { "winmf1db_load.bat", "centmf1db_load.bat" }, { "winmf2db_load.bat", "centmf2db_load.bat" } };
		public static string[,] z_strLoadMsgs = { { "Windows mf1DB(mf1)", "CentOS mf1DB(mf1)" }, { "Windows mf2DB(mf2)", "CentOS リリースDB(mf2)" } };
        public static string z_strSaveLogBat = "winlogdb_save.bat";
        public static string z_strSaveLogMsgs = "Windows logDB";
        public static string z_strLoadLogBat = "winlogdb_load.bat";
		public static string z_strLoadLogMsgs = "Windows logDB";

		public static string[] z_renvdir = { "\\xampp\\htdocs\\mf1", "\\xampp\\htdocs\\mf2" };			// 環境 ディレクトリ　
		public static string[] z_rpathSyohinBaseHP = { z_renvdir[0] + "\\prg\\", z_renvdir[1] + "\\prg\\" };	// 基準商品Hフォルダ

		// 図形と商品HPはmf1以下にまとめる
		public static string[] z_rpathZukeiData = { z_renvdir[0] + "\\z", z_renvdir[1] + "\\z" };				// 図形データフォルダ
		public static string z_pathZukeiIns = "\\new\\";													// 新規追加図形フォルダ

		public static string[] z_rpathCategoryBaseHP = { z_renvdir[0] + "\\prg\\", z_renvdir[1] + "\\prg\\" };	// 基準カテゴリHPフォルダ

		public static string z_rpathBackupDBFolder = "\\DB\\Backup\\";										// DB Backup フォルダ
		public static string[,] z_pathBackupSelectFile = { { "winmysqlmf1.sql", "centmysqlmf1.sql" }, { "winmysqlmf2.sql", "centmysqlmf2.sql" } };
		public static string[,] z_pathBackupTempFile = { { "temp_winmysqlmf1.sql", "temp_centmysqlmf1.sql" }, { "temp_winmysqlmf2.sql", "temp_centmysqlmf2.sql" } };
		public static string z_pathBackupLogSelectFile = "winmysqllog.sql";
		public static string z_pathBackupLogTempFile = "temp_winmysqllog.sql";

		public static string z_strSitemapFolder = "\\sitemap.xml";											// サイトマップXMLファイル
		public static string z_strDBBackupFilter = " DB Backup File(*.sql)|*.sql";
		public static string z_rpathMatukenDB = "\\DB\\Matsuken\\";										// Text DB / Dump フォルダ
		public static string[,] z_pathDumpFile = { { "Winmf1Dump.dmp", "Centmf1Dump.dmp" }, { "Winmf2Dump.dmp", "Centmf2Dump.dmp" } };
		public static string z_strDBDumpFilter = " DB Dump File(*.dmp)|*.dmp";
	}
}
