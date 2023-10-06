using System;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

public class P7mExtractor
{
	public static void ExtractAndSaveContent(string inputFilePath, string outputFolder)
	{
		try
		{
			// Attempt to extract content using Method 1 (ContentInfo)
			if (TryExtractUsingContentInfo(inputFilePath, outputFolder))
			{
				return; // Content successfully extracted
			}

			// If Method 1 fails, attempt Method 2 (Base64)
			if (TryExtractUsingBase64(inputFilePath, outputFolder))
			{
				return; // Content successfully extracted
			}

			// If both methods fail, report an error
			Console.WriteLine($"Failed to extract content from '{inputFilePath}'.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error processing file '{inputFilePath}': {ex.Message}");
		}
	}

	private static bool TryExtractUsingContentInfo(string inputFilePath, string outputFolder)
	{
		try
		{
			byte[] p7mData = File.ReadAllBytes(inputFilePath);
			ContentInfo contentInfo = new ContentInfo(p7mData);

			SignedCms signedCms = new SignedCms();
			signedCms.Decode(contentInfo.Content);

			byte[] content = signedCms.ContentInfo.Content;
			string outputFilePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(inputFilePath));
			File.WriteAllBytes(outputFilePath, content);
			Console.WriteLine($"Extracted content from '{inputFilePath}' to '{outputFilePath}'");
			return true; // Extraction succeeded
		}
		catch
		{
			return false; // Extraction failed
		}
	}

	private static bool TryExtractUsingBase64(string inputFilePath, string outputFolder)
	{
		try
		{
			string base64Data = File.ReadAllText(inputFilePath);
			byte[] p7mData = Convert.FromBase64String(base64Data);

			SignedCms signedCms = new SignedCms();
			signedCms.Decode(p7mData);

			byte[] content = signedCms.ContentInfo.Content;
			string outputFilePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(inputFilePath));
			File.WriteAllBytes(outputFilePath, content);
			Console.WriteLine($"Extracted content from '{inputFilePath}' to '{outputFilePath}'");
			return true; // Extraction succeeded
		}
		catch
		{
			return false; // Extraction failed
		}
	}
}
