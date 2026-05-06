using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UOAIO;

public class Book : Item
{
	private string m_Title;

	private string m_Author;

	private bool m_CanWrite;

	private BookPageInfo[] m_Pages;

	private static readonly ushort[] m_BookIDs;

	public string Title
	{
		get
		{
			return this.m_Title;
		}
		set
		{
			this.m_Title = value;
		}
	}

	public string Author
	{
		get
		{
			return this.m_Author;
		}
		set
		{
			this.m_Author = value;
		}
	}

	public bool CanWrite
	{
		get
		{
			return this.m_CanWrite;
		}
		set
		{
			this.m_CanWrite = value;
		}
	}

	public int PageCount => this.m_Pages.Length;

	public BookPageInfo[] Pages
	{
		get
		{
			return this.m_Pages;
		}
		set
		{
			this.m_Pages = value;
		}
	}

	public string ContentAsString
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			BookPageInfo[] pages = this.m_Pages;
			foreach (BookPageInfo bookPageInfo in pages)
			{
				string[] lines = bookPageInfo.Lines;
				foreach (string value in lines)
				{
					stringBuilder.AppendLine(value);
				}
			}
			return stringBuilder.ToString();
		}
	}

	public string[] ContentAsStringArray
	{
		get
		{
			List<string> list = new List<string>();
			BookPageInfo[] pages = this.m_Pages;
			foreach (BookPageInfo bookPageInfo in pages)
			{
				list.AddRange(bookPageInfo.Lines);
			}
			return list.ToArray();
		}
	}

	public static bool IsBook(ushort itemID)
	{
		return Book.m_BookIDs.Contains(itemID);
	}

	public Book(int serial)
		: this(serial, 20, canWrite: true)
	{
	}

	public Book(int serial, int pageCount, bool canWrite)
		: this(serial, pageCount, canWrite, null, null)
	{
	}

	public Book(int serial, int pageCount, bool canWrite, string title, string author)
		: base(serial)
	{
	}

	static Book()
	{
		Book.m_BookIDs = new ushort[5] { 4081, 4082, 4079, 4081, 4080 };
	}
}
