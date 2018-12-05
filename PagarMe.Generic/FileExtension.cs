using System;
using System.IO;
using System.Linq;

namespace PagarMe.Generic
{
    public class FileExtension
    {
	    public static void DeleteIfExists(String path)
	    {
		    if (File.Exists(path))
		    {
			    File.Delete(path);
		    }
	    }

	    public static void Delete(String path, String pattern)
	    {
			Directory.EnumerateFiles(path, pattern)
				.ToList().ForEach(File.Delete);
	    }
    }
}
