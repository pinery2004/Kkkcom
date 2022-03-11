using System;
using System.Collections;

namespace HTML
{
  /// <summary>
  /// Base class for parsing tag based files, such as HTML,
  /// HTTP headers, or XML.
  ///
  /// This source code may be used freely under the
  /// Limited GNU Public License(LGPL).
  ///
  /// Written by Jeff Heaton (http://www.jeffheaton.com)
  /// </summary>
  public class Parse:AttributeList
  {
    /// <summary>
    /// The source text that is being parsed.
    /// </summary>
    private string m_source;

    /// <summary>
    /// The current position inside of the text that
    /// is being parsed.
    /// </summary>
    private int m_idx;

	/// <summary>
	/// 行毎の先頭文字位置
	/// </summary>
	private ArrayList m_line_idx;

    /// <summary>
    /// The most recently parsed attribute delimiter.
    /// </summary>
    private char m_parseDelim;

    /// <summary>
    /// This most recently parsed attribute name.
    /// </summary>
    private string m_parseName;

    /// <summary>
    /// The most recently parsed attribute value.
    /// </summary>
    private string m_parseValue;

    /// <summary>
    /// The most recently parsed tag.
    /// </summary>
    public string m_tag;

    /// <summary>
    /// Determine if the specified character is whitespace or not.
    /// </summary>
    /// <param name="ch">A character to check</param>
    /// <returns>true if the character is whitespace</returns>
    public static bool IsWhiteSpace(char ch)
    {
      return( " ".IndexOf(ch) != -1 );
    }


    /// <summary>
    /// Advance the index until past any whitespace.
    /// </summary>
    public void EatWhiteSpace()
    {
      while ( !Eof() )
      {
        if ( !IsWhiteSpace(GetCurrentChar()) )
          return;
        m_idx++;
      }
    }

    /// <summary>
    /// Determine if the end of the source text has been reached.
    /// </summary>
    /// <returns>True if the end of the source text has been
    /// reached.</returns>
    public bool Eof()
    {
      return(m_idx>=m_source.Length );
    }

    /// <summary>
    /// Parse the attribute name.
    /// </summary>
    public void ParseAttributeName()
    {
      EatWhiteSpace();
      // get attribute name
	  int iTypeDQ = 0;
      while ( !Eof() )
      {
		char cch = GetCurrentChar();
		if (iTypeDQ == 1)
		{
		  if (cch == '\"')
			iTypeDQ = 0;
		}
		else
		{
		  if (cch == '\"')
			iTypeDQ = 1;

		  else if (IsWhiteSpace(cch) ||
				   (cch == '=') ||
				   (cch == '>'))
				  break;
		}
        m_parseName+=cch;
        m_idx++;
      }

      EatWhiteSpace();
    }


    /// <summary>
    /// Parse the attribute value
    /// </summary>
    public void ParseAttributeValue()
    {
      if ( m_parseDelim!=0 )
        return;

      if ( GetCurrentChar()=='=' )
      {
        m_idx++;
        EatWhiteSpace();
        if ( (GetCurrentChar()=='\'') ||	// 修正OK?
          (GetCurrentChar()=='\"') )		// 修正OK?
        {
          m_parseDelim = GetCurrentChar();
          m_idx++;
          while (( GetCurrentChar()!=m_parseDelim) && (m_idx<999999))
          {
            m_parseValue+=GetCurrentChar();
            m_idx++;
          }
          m_idx++;
        }
        else
        {
          while ( !Eof() &&
            !IsWhiteSpace(GetCurrentChar()) &&
            (GetCurrentChar()!='>') &&(m_idx<2000))

          {
            m_parseValue+=GetCurrentChar();
            m_idx++;
          }
        }
        EatWhiteSpace();
      }
    }

    /// <summary>
    /// Add a parsed attribute to the collection.
    /// </summary>
    public void AddAttribute()
    {
      Attribute a = new Attribute(m_parseName,
        m_parseValue,m_parseDelim);
      Add(a);
    }


    /// <summary>
    /// Get the current character that is being parsed.
    /// </summary>
    /// <returns></returns>
    public char GetCurrentChar()

    {

      return GetCurrentChar(0);

    }

	/// <summary>
	/// カレント文字位置の行番号
	/// </summary>
	/// <returns></returns>
	public int GetCurrentLine()
	{
		int iline;
		for (iline = 0; iline < m_line_idx.Count; iline++)
		{
			if (m_idx < (int)m_line_idx[iline])
			{
				break;
			}
		}
		return ++iline;
	}

	/// <summary>
	/// Get a few characters ahead of the current character.
	/// </summary>
	/// <param name="peek">How many characters to peek ahead
	/// for.</param>
	/// <returns>The character that was retrieved.</returns>
	public char GetCurrentChar(int peek)

    {
      if( (m_idx+peek)<m_source.Length )
        return m_source[m_idx+peek];
      else
        return (char)0;
    }



    /// <summary>
    /// Obtain the next character and advance the index by one.
    /// </summary>
    /// <returns>The next character</returns>
    public char AdvanceCurrentChar()

    {
      return m_source[m_idx++];
    }



    /// <summary>
    /// Move the index forward by one.
    /// </summary>
    public void Advance()
    {
      m_idx++;
    }


    /// <summary>
    /// The last attribute name that was encountered.
    /// <summary>
    public string ParseName
    {
      get
      {
        return m_parseName;
      }

      set
      {
        m_parseName = value;
      }
    }

    /// <summary>
    /// The last attribute value that was encountered.
    /// <summary>
    public string ParseValue
    {
      get
      {
        return m_parseValue;
      }

      set
      {
        m_parseValue = value;
      }
    }

    /// <summary>
    /// The last attribute delimeter that was encountered.
    /// <summary>
    public char ParseDelim
    {
      get
      {
        return m_parseDelim;
      }

      set
      {
        m_parseDelim = value;
      }
    }

    /// <summary>
    /// The text that is to be parsed.
    /// <summary>
    public string Source
    {
      get
      {
        return m_source;
      }

      set
      {
        m_source = value;
		//
		m_line_idx = new ArrayList();
		for (int ic1 = 0; ic1 < m_source.Length; ic1++)
		{
		  if (m_source[ic1] == '\a')
		  {
			m_line_idx.Add(ic1);
		  }
		}
      }
    }
  }
}
