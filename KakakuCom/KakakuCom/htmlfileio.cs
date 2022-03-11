using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kc
{
	public class htmlfileio
	{
		public int openfile(string strFilePath, out string strMessage)
		{
			String strBuff;
			string result = string.Empty;
			int iSt;

			if (System.IO.File.Exists(strFilePath))
			{
				System.IO.StreamReader reader = (new System.IO.StreamReader(strFilePath, System.Text.Encoding.GetEncoding("shift_jis")));
				while (reader.Peek() >= 0)
				{
					strBuff = reader.ReadLine();
					result += strBuff + System.Environment.NewLine;
				}
				reader.Close();
				iSt = 0;
				strMessage = result;
			}
			else
			{
				iSt = 1;
				strMessage = "";
			}
			return iSt;
		}
	}
}
