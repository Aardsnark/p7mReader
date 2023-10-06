using System;
using System.IO;

class Program
{
	static void Main()
	{
		string inputFolder = "../../data/input";
		string outputFolder = "../../data/output";

		// Ensure the output folder exists
		Directory.CreateDirectory(outputFolder);

		// Get a list of all p7m files in the input folder
		string[] p7mFiles = Directory.GetFiles(inputFolder, "*.p7m");

		foreach (string p7mFile in p7mFiles)
		{
			//P7mExtractorNetStandard.ExtractAndSaveContent(p7mFile, outputFolder);
			P7mExtractorNetFramework.ExtractAndSaveContent(p7mFile, outputFolder);
		}
	}
}
