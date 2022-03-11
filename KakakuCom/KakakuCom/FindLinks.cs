using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace HTML
{
  /// <summary>
  /// FindLinks is a class that will test the HTML parser.
  /// This short example will prompt for a URL and then
  /// scan that URL for links.
  /// This source code may be used freely under the
  /// Limited GNU Public License(LGPL).
  ///
  /// Written by Jeff Heaton (http://www.jeffheaton.com)
  /// </summary>
  public class FindLinks
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void fMain(string i_strUrl)
    {
      System.Console.Write("Enter a URL address:");
	  string url = System.Console.ReadLine();
//	  url = "http://localhost/af2/ApTest2.html";
	  url = i_strUrl;
      System.Console.WriteLine("Scanning hyperlinks at: " + url );
      string page = GetPage(url);
      if(page==null)
      {
        System.Console.WriteLine("Can’t process that type of file,"
                                  +
                                  "please specify an HTML file URL."
                                  );
        return;
      }

      ParseHTML parse = new ParseHTML();
      parse.Source = page;
      while( !parse.Eof() )
      {
        char ch = parse.Parse();
        if(ch==0)
        {
          AttributeList tag = parse.GetTag();
		  if (tag["href"] != null)
		  {
			  int iLine=parse.GetCurrentLine();
			  System.Console.WriteLine("Found link["+iLine+"]: " +
										 tag["href"].Value);
		  }
        }
      }
    }

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	public static void gMain(string i_strUrl)
	{
		string strValue="";
		System.Console.Write("Enter a URL address:");
		string url = System.Console.ReadLine();
		url = i_strUrl;
		System.Console.WriteLine("Scanning hyperlinks at: " + url);
		string page = GetPage(url);
		if (page == null)
		{
			System.Console.WriteLine("Can’t process that type of file,"
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
				AttributeList tag = parse.GetTag();
				/*
								if (tag["href"] != null)
								{
									int iLine = parse.GetCurrentLine();
									System.Console.WriteLine("Found link[" + iLine + "]: " +
															   tag["href"].Value);
								}
				 */
//				System.Console.Write("[" + iLine + "] " + tag.Name + ":" + tag.Value + " ");
				System.Console.Write(tag.Name + ":" + tag.Value + " ");
				for (int it = 0; it < tag.Count; it++)
				{
					Attribute atr = tag[it];
					System.Console.Write(atr.Name + ":" + atr.Value + " ");
				}
				if (String.IsNullOrEmpty(strValue))
				{
					System.Console.WriteLine("");
				}
				else
				{
					System.Console.WriteLine("[" + strValue + "]");
					strValue = "";
				}
			}
			else
			{
//				if(ch != '\a')
					strValue += ch;
			}
		}
	}

	public static void hMain(string i_strUrl)
	{
		AttributeList[]	Tags = new AttributeList[20000];
		int nTags = 0;
		string[] strTagValues = new string[20000];
		int[] iTagLines = new int[20000];

		System.Console.Write("Enter a URL address:");
		string url = System.Console.ReadLine();
		url = i_strUrl;
		System.Console.WriteLine("Scanning hyperlinks at: " + url);
		string page = GetPage(url);
		if (page == null)
		{
			System.Console.WriteLine("Can’t process that type of file,"
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
				Tags[nTags] = parse.GetTag();
				iTagLines[nTags] = iLine;
				nTags++;
				if (nTags >= 19999)
					break;
			}
			else
			{
				if(nTags !=0)
					strTagValues[nTags-1] += ch;
			}
		}
		Tags[nTags] = Tags[nTags - 1];						// 最後の行処理をエラーにならないよう

		int iDisp = 0;				// 0:表示
		for(int ic=0; ic<nTags; ic++)
		{
															//			System.Console.Write("[" + iTagLines[ic] + "] " + Tags[ic].Name + ":" + Tags[ic].Value + " ");
			string strTagNameS = Tags[ic].Name.ToLower();
			string strNextTagName = Tags[ic+1].Name;
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
				if (strTagNameS=="table" || strTagNameS == "tr" || strTagNameS == "th" || strTagNameS == "td" || strTagNameS == "scan" )
					System.Console.WriteLine(); 

				if (strTagNameS[0] == '/')
				{
					System.Console.Write("}");
				}
				else
				{
					System.Console.Write("{" + strTagNameS + ":");
				}
				// エラー判定
				if( Tags[ic].Value != "")
					System.Console.WriteLine("**************************** " + Tags[ic].Value + " ******************************");

				for (int it = 0; it < Tags[ic].Count; it++)
				{
					Attribute atr = Tags[ic][it];
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
				if (String.IsNullOrWhiteSpace(strTagValues[ic]) == false && strTagValues[ic] != "\a" && strTagValues[ic] != "\a\a")
					System.Console.Write("[" + strTagValues[ic] + "]");
			}
		}
	}

    public static string GetPage(string url)
    {
      WebResponse response = null;
      Stream stream = null;
      StreamReader
        reader = null;

      try
      {
        HttpWebRequest request =
                       (HttpWebRequest)WebRequest.Create(url);

        response = request.GetResponse();
        stream = response.GetResponseStream();

        if( !response.ContentType.ToLower().StartsWith("text/") )
          return null;

        string buffer = "",line = "";

//        reader = new StreamReader(stream);

		reader = (new System.IO.StreamReader(stream, System.Text.Encoding.GetEncoding("shift_jis")));
		while ((line = reader.ReadLine()) != null)
        {
          buffer+=line+"\a";						// 修正OK?
        }

        return buffer;
      }
      catch(WebException e)
      {
        System.Console.WriteLine("Can’t download:" + e);
        return null;
      }
      catch(IOException e)
      {
        System.Console.WriteLine("Can’t download:" + e);
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
  }
}
