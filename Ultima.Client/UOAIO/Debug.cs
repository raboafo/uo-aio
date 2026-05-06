using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SharpDX;

namespace UOAIO;

public class Debug
{
	private static StreamWriter m_Logger;

	private static int m_Indent;

	public static Stopwatch _stopwatch;

	private static bool m_Time;

	public static int Indent
	{
		get
		{
			return Debug.m_Indent;
		}
		set
		{
			Debug.m_Indent = value;
		}
	}

	public static void Dispose()
	{
		if (Debug.m_Logger != null)
		{
			Debug.m_Logger.Flush();
			Debug.m_Logger.Close();
			Debug.m_Logger = null;
		}
	}

	private static void Print()
	{
		Debug.m_Logger.WriteLine();
	}

	private static void Print(string ToWrite)
	{
		Debug.GetLogger();
		Debug.m_Logger.WriteLine(ToWrite);
	}

	private static void Print(string Format, object Obj0)
	{
		Debug.Print(string.Format(Format, Obj0));
	}

	private static void Print(string Format, object Obj0, object Obj1)
	{
		Debug.Print(string.Format(Format, Obj0, Obj1));
	}

	private static void Print(string Format, object Obj0, object Obj1, object Obj2)
	{
		Debug.Print(string.Format(Format, Obj0, Obj1, Obj2));
	}

	private static void Print(string Format, params object[] Params)
	{
		Debug.Print(string.Format(Format, Params));
	}

	public static void Trace(string Message)
	{
		Debug.GetLogger();
		Debug.m_Logger.WriteLine(new string(' ', Debug.Indent * 3) + Message);
		Debug.m_Logger.Flush();
	}

	public static void Trace(string Format, object Obj0)
	{
		Debug.Trace(string.Format(Format, Obj0));
	}

	public static void Trace(string Format, object Obj0, object Obj1)
	{
		Debug.Trace(string.Format(Format, Obj0, Obj1));
	}

	public static void Trace(string Format, object Obj0, object Obj1, object Obj2)
	{
		Debug.Trace(string.Format(Format, Obj0, Obj1, Obj2));
	}

	public static void Trace(string Format, params object[] Params)
	{
		Debug.Trace(string.Format(Format, Params));
	}

	public static void Try(string Name)
	{
		Debug.GetLogger();
		Debug.m_Logger.Write("{0}{1}...", new string(' ', Debug.Indent * 3), Name);
	}

	public static void Try(string Format, object Obj0)
	{
		Debug.Try(string.Format(Format, Obj0));
	}

	public static void Try(string Format, object Obj0, object Obj1)
	{
		Debug.Try(string.Format(Format, Obj0, Obj1));
	}

	public static void Try(string Format, object Obj0, object Obj1, object Obj2)
	{
		Debug.Try(string.Format(Format, Obj0, Obj1, Obj2));
	}

	public static void Try(string Format, params object[] Params)
	{
		Debug.Try(string.Format(Format, Params));
	}

	public static void FailTry(string msg)
	{
		Debug.GetLogger();
		Debug.m_Logger.WriteLine("failed {0}", msg);
	}

	public static void FailTry()
	{
		Debug.GetLogger();
		Debug.m_Logger.WriteLine("failed");
	}

	public static void EndTry(string msg)
	{
		Debug.GetLogger();
		Debug.m_Logger.WriteLine("done {0}", msg);
	}

	public static void EndTry(string Format, object Obj0)
	{
		Debug.EndTry(string.Format(Format, Obj0));
	}

	public static void EndTry(string Format, object Obj0, object Obj1)
	{
		Debug.EndTry(string.Format(Format, Obj0, Obj1));
	}

	public static void EndTry(string Format, object Obj0, object Obj1, object Obj2)
	{
		Debug.EndTry(string.Format(Format, Obj0, Obj1, Obj2));
	}

	public static void EndTry(string Format, params object[] Params)
	{
		Debug.EndTry(string.Format(Format, Params));
	}

	private static void GetLogger()
	{
		if (Debug.m_Logger == null)
		{
			Engine.WantDirectory("data/ultima/logs/");
			Debug.m_Logger = new StreamWriter(Engine.FileManager.CreateUnique("data/ultima/logs/playuo", ".log"));
			Debug.m_Logger.AutoFlush = true;
		}
	}

	public static void EndTry()
	{
		Debug.GetLogger();
		Debug.m_Logger.WriteLine("done");
	}

	[DebuggerHidden]
	public static void Break()
	{
		Debugger.Break();
	}

	static Debug()
	{
		Debug._stopwatch = new Stopwatch();
	}

	public static void TimeBlock(string Name)
	{
		Debug.Try(Name);
		Debug.m_Time = true;
		Debug._stopwatch.Start();
	}

	public static void Block(string Name)
	{
		Debug.Trace("{0}..", Name);
		Debug.Indent++;
	}

	public static void FailBlock()
	{
		if (Debug.m_Time)
		{
			Debug._stopwatch.Stop();
			Debug.m_Time = false;
			Debug.EndTry("( {0} )", Debug._stopwatch.Elapsed);
			Debug._stopwatch.Reset();
		}
		else
		{
			Debug.Indent--;
			Debug.Trace("Failed");
		}
	}

	public static void EndBlock()
	{
		if (Debug.m_Time)
		{
			Debug._stopwatch.Stop();
			Debug.m_Time = false;
			Debug.EndTry("( {0} )", Debug._stopwatch.Elapsed);
			Debug._stopwatch.Reset();
		}
		else
		{
			Debug.Indent--;
			Debug.Trace("Done");
		}
	}

	public static void Error(Exception ex)
	{
		if (ex is SharpDXException)
		{
			SharpDXException ex2 = (SharpDXException)ex;
			Debug.Trace("Error Code -> {0}", ex2.ResultCode.ToString());
			Debug.Trace("Error String -> {0}", ex2.Descriptor.ToString());
		}
		Debug.Trace("Type -> {0}", ex.GetType());
		Debug.Trace("Message -> {0}", ex.Message);
		Debug.Trace("Source -> {0}", ex.Source);
		Debug.Trace("Target -> {0}", ex.TargetSite);
		Debug.Trace("Inner -> {0}", ex.InnerException);
		Debug.Trace("Stack ->");
		Debug.Trace(ex.StackTrace);
	}

	public static void Error(string Message)
	{
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		bool flag = false;
		MethodBase methodBase = null;
		int frameCount = stackTrace.FrameCount;
		for (int i = 0; i < frameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			MethodBase method = frame.GetMethod();
			if (method.DeclaringType == typeof(Debug) && method.Name == "Error")
			{
				flag = true;
			}
			else if (flag)
			{
				methodBase = method;
				break;
			}
		}
		if (methodBase == null)
		{
			Debug.Print("Error in unknown module:");
			Debug.Print(" - {0}", Message.Replace("\n", "\r\n - "));
			Debug.Print(" - Stack Trace ->");
			Debug.Print(stackTrace.ToString());
			Debug.Print();
		}
		else
		{
			Debug.Print("Error in '{0}.{1}':", methodBase.DeclaringType.Name, methodBase.Name);
			Debug.Print(" - {0}", Message.Replace("\n", "\r\n - "));
			Debug.Print(" - Stack Trace ->");
			Debug.Print(stackTrace.ToString());
			Debug.Print();
		}
	}
}
