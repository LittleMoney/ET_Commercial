using NLog;

namespace ETModel
{
	public class ConsoleAdapter : ILog
	{
		public void Trace(string message)
		{
			System.Console.WriteLine($"Trace:{message}");
		}

		public void Warning(string message)
		{
			System.Console.WriteLine($"Warning:{message}");
		}

		public void Info(string message)
		{
			System.Console.WriteLine($"Info:{message}");
		}

		public void Debug(string message)
		{
			System.Console.WriteLine($"Debug:{message}");
		}

		public void Error(string message)
		{
			System.Console.WriteLine($"Error:{message}");
		}

		public void Fatal(string message)
		{
			System.Console.WriteLine($"Fatal:{message}");
		}

		public void Trace(string message, params object[] args)
		{
			System.Console.WriteLine($"Trace:{message}", args);
		}

		public void Warning(string message, params object[] args)
		{
			System.Console.WriteLine($"Warning:{message}", args);
		}

		public void Info(string message, params object[] args)
		{
			System.Console.WriteLine($"Info:{message}", args);
		}

		public void Debug(string message, params object[] args)
		{
			System.Console.WriteLine($"Debug:{message}", args);
		}

		public void Error(string message, params object[] args)
		{
			System.Console.WriteLine($"Error:{message}", args);
		}

		public void Fatal(string message, params object[] args)
		{
			System.Console.WriteLine($"Fatal:{message}", args);
		}
	}
}