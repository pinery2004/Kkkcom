using System;

namespace Kc
{
	public class Sitemap
	{
		public Sitemap()
		{

		}
		public static int Open(int i_itype)
		{
			int ist = 0;
			string strFilePath = Const.z_envdir[Form1.m_iradioBtnDB - 1] + Const.z_strSitemapFolder[i_itype];
			Mkdb.OpenWriteFile(strFilePath, false);				// 新規追加
			return ist;
		}
		public static int WriteLine(string i_strLine)
		{
			int ist = 0;
			Mkdb.WriteLine(i_strLine);
			return ist;
		}

		public static int Write_Head_Urlset()
		{
			int ist = 0;

			WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			WriteLine("<urlset");
			WriteLine("      xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"");
			WriteLine("      xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
			WriteLine("      xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9");
			WriteLine("            http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\">");
//			WriteLine("<!-- created with Free Online Sitemap Generator www.xml-sitemaps.com -->");
			return ist;
		}

		public static int Write_Url(string i_strHPUrl, string i_strDay, string i_strCngFreq, string i_strPri)
		{
			int ist = 0;
			string strHPUrl = i_strHPUrl;
//			if (strHPUrl.IndexOf("sumafo") >= 0)
//			{
				int icn = strHPUrl.IndexOf(".php");
				if (icn > 20)
				{
					strHPUrl = i_strHPUrl.Substring(0,icn);
				}
//			}
			WriteLine("<url>");
			WriteLine("<loc>" + strHPUrl + "</loc>");
			WriteLine("  <lastmod>" + i_strDay + "</lastmod>");
			WriteLine("  <changefreq>" + i_strCngFreq + "</changefreq>");
			WriteLine("  <priority>" + i_strPri + "</priority>");
			WriteLine("</url>");
			return ist;
		}

		public static int Write_Tail_Urlset()
		{
			int ist = 0;

			WriteLine("</urlset>");
			return ist;
		}

		public static int Close()
		{
			int ist = 0;
			Mkdb.CloseWriteFile();
			return ist;
		}
	}
}
