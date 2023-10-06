using System;
using System.IO;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509.Store;

public class P7mExtractorNetFramework
{
	public static void ExtractAndSaveContent(string inputFilePath, string outputFolder)
	{
		try
		{
			
			if (TryExtractUsingCmsSignedData(inputFilePath, outputFolder))
			{
				return;
			}

			
			if (TryExtractUsingBase64(inputFilePath, outputFolder))
			{
				return;
			}

			
			Console.WriteLine($"Failed to extract content from '{inputFilePath}'.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error processing file '{inputFilePath}': {ex.Message}");
		}
	}

	private static bool TryExtractUsingCmsSignedData(string inputFilePath, string outputFolder)
	{
		try
		{
			
			byte[] p7mData = File.ReadAllBytes(inputFilePath);

			// Create a CmsSignedData object from the p7m data
			CmsSignedData signedData = new CmsSignedData(p7mData);

			// Get the original content
			byte[] content = signedData.GetEncoded();

			// Generate the output file path
			string outputFilePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(inputFilePath));

			// Save the extracted content to the output file
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
			
			byte[] p7mData = File.ReadAllBytes(inputFilePath);

			// Convert the Base64 data to a byte array
			string base64Data = System.Text.Encoding.UTF8.GetString(p7mData);
			byte[] decodedData = Convert.FromBase64String(base64Data);

			// Generate the output file path
			string outputFilePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(inputFilePath));

			// Save the decoded content to the output file
			File.WriteAllBytes(outputFilePath, decodedData);

			Console.WriteLine($"Extracted content from '{inputFilePath}' to '{outputFilePath}'");
			return true; // Extraction succeeded
		}
		catch
		{
			return false; // Extraction failed
		}
	}
}
