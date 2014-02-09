namespace Mapping
{
	using System;
	using System.IO;
	using System.IO.MemoryMappedFiles;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.Win32.SafeHandles;

	public class Program
	{
		// After getting the solution to work with managed code, I used this to turn on Write_Through | NoBuffering
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern SafeFileHandle CreateFile(string FileName,
		                                               uint DesiredAccess,
		                                               uint ShareMode,
		                                               IntPtr SecurityAttributes,
		                                               uint CreationDisposition,
		                                               uint FlagsAndAttributes,
		                                               IntPtr hTemplateFile);

		public static uint GenericRead = 0x80000000;
		public static uint GenericWrite = 0x40000000;
		public static uint Read = 0x00000001;
		public static uint OpenAlways = 4;
		public static uint Write_Through = 0x80000000;
		public static uint NoBuffering = 0x20000000;
		public static uint DeleteOnClose = 0x04000000;

		public static void Main(string[] args)
		{
			var numberOfBytesToWrite = 4*4096;
			byte[] buffer = new byte[numberOfBytesToWrite];
			for(int i = 0; i < numberOfBytesToWrite; i++)
			{
				buffer[i] = 137;
			}

			var g = Guid.NewGuid().ToString();

			//var safeHandle = CreateFile(g,
			//    GenericRead | GenericWrite,
			//    Read,
			//    IntPtr.Zero,
			//    OpenAlways,
			//    Write_Through | NoBuffering,
			//    IntPtr.Zero);

			//if(safeHandle.IsInvalid)
			//{
			//    throw new Win32Exception();
			//}

			//var fs = new FileStream(safeHandle, FileAccess.ReadWrite);
			var fs = File.Open(g, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			long fileLength = 512*1024*1024;
			fs.SetLength(fileLength); // 0.5 gb
			var mmf = MemoryMappedFile.CreateFromFile(fs, "test", fs.Length, MemoryMappedFileAccess.ReadWrite, null,
			                                          HandleInheritability.None, false);
			var readOnly = mmf.CreateViewAccessor(0, fs.Length, MemoryMappedFileAccess.Read);

			Task.Factory.StartNew(() =>
			                      	{
			                      		var offset = 0;
			                      		var count = 0;
										while (offset < fileLength)
										{
											while (offset < fileLength)
											{
												if (readOnly.ReadByte(offset) != 137)
												{
													break;
												}
												offset += 1024;
												count++;
											}
											Console.WriteLine();
											Console.WriteLine("{0:0,0} kB ({1:0,0} MB) Verified", count, count/1024);
											Console.WriteLine();
											Thread.Sleep(2000);
										}
			                      	});

			var writeOnly = mmf.CreateViewAccessor(0, fs.Length, MemoryMappedFileAccess.Write);
			for(int i = 0; i < 1024*32; i++)
			{
				var offset = i*numberOfBytesToWrite;
				writeOnly.WriteArray(offset, buffer, 0, numberOfBytesToWrite);
				if (i % 100 == 0)
				{
					Console.Write("\r{0:0,0} kb", offset / 1024);
				}
			}

			Console.ReadLine();
			writeOnly.Dispose();
			readOnly.Dispose();
			mmf.Dispose();
			fs.Dispose();
		}
	}
}