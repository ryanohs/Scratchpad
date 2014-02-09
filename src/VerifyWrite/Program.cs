namespace VerifyWrite
{
	using System;
	using System.IO;
	using System.Linq;

	class Program
	{
		static void Main(string[] args)
		{
			// I wrote this file using Write_Through | NoBuffering
			//var filename = @"C:\Projects\FileTests\Scratchpad\Managed\bin\Debug\0d11fa18-51b6-403d-832a-700424ff35e5";

			// I wrote this file using File.Open()
			var filename = @"C:\Projects\FileTests\Scratchpad\Managed\bin\Debug\23bddbec-f05d-4189-af01-cbad924e38f9";
			
			using(var fs = File.OpenRead(filename))
			{
				var buffer = new byte[4*1024];
				var offset = 0;
				while(offset < fs.Length)
				{
					fs.Read(buffer, 0, buffer.Length);
					if(buffer.Any(b => b != 137))
					{
						break;
					}
					offset += buffer.Length;
				}
				fs.Close();
				Console.WriteLine("Verified {0:0,0} bytes", offset); // within 4 kB of the end of data
				Console.ReadLine();

				// I didn't kill the processes at the exact same time.
				// The first file had 148MB of data
				// The second file at 150MB of data
				// Both approaches seem to get file to disk quickly.
				// I wonder how much of it is the OS cache getting written after my process dies.
				// How can I find this out?
			}
		}
	}
}
