using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace P7mExtractor.UnitTests;

[TestClass]
public class P7mExtractorNetStandardTests
{
	// Define a temporary directory for testing
	private string? tempDirectory;
	private X509Certificate2? mockCertificate;

	[TestInitialize]
	public void Initialize()
	{
		// Create a temporary directory for testing
		tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirectory);

		// Generate a self-signed certificate for testing
		mockCertificate = GenerateSelfSignedCertificate();
	}

	[TestCleanup]
	public void Cleanup()
	{
		// Clean up the temporary directory and its contents after each test
		Directory.Delete(tempDirectory!, true);
	}


	[TestMethod]
	public void TestExtractAndSaveContent_SuccessfulExtraction_ContentInfo()
	{
		// Arrange: Create a sample p7m file with known content
		string inputFilePath = Path.Combine(tempDirectory!, "test.p7m");
		string outputFilePath = Path.Combine(tempDirectory!, "test");
		byte[] contentBytes = new byte[] { 1, 2, 3, 4 };

		// Create a mock PKCS7 message with the content and the mock certificate
		byte[] mockPkcs7Message = CreateMockPkcs7Message(contentBytes, mockCertificate!);

		File.WriteAllBytes(inputFilePath, mockPkcs7Message);

		// Act: Call the method to be tested
		P7mExtractorNetStandard.ExtractAndSaveContent(inputFilePath, tempDirectory!);

		// Assert: Verify that the content was successfully extracted
		Assert.IsTrue(File.Exists(outputFilePath));
		byte[] extractedContent = File.ReadAllBytes(outputFilePath);
		CollectionAssert.AreEqual(contentBytes, extractedContent);
	}

	[TestMethod]
	public void TestExtractAndSaveContent_SuccessfulExtraction_Base64()
	{
		// Arrange: Create a sample p7m file with known content and attach the mock certificate
		string inputFilePath = Path.Combine(tempDirectory!, "test.p7m");
		string outputFilePath = Path.Combine(tempDirectory!, "test");
		byte[] contentBytes = new byte[] { 1, 2, 3, 4 };

		// Create a mock PKCS7 message with the content and the mock certificate
		byte[] mockPkcs7Message = CreateMockPkcs7Message(contentBytes, mockCertificate!);

		// Encode the mock PKCS7 message as Base64
		string base64Content = Convert.ToBase64String(mockPkcs7Message);
		File.WriteAllText(inputFilePath, base64Content);

		// Act: Call the method to be tested
		P7mExtractorNetStandard.ExtractAndSaveContent(inputFilePath, tempDirectory);

		// Assert: Verify that the content was successfully extracted
		Assert.IsTrue(File.Exists(outputFilePath));
		byte[] extractedContent = File.ReadAllBytes(outputFilePath);
		CollectionAssert.AreEqual(contentBytes, extractedContent);
	}


	[TestMethod]
	public void TestExtractAndSaveContent_Failure()
	{
		// Arrange: Create a non-p7m file
		string inputFilePath = Path.Combine(tempDirectory!, "notap7m.txt");

		// Act: Call the method to be tested
		P7mExtractorNetStandard.ExtractAndSaveContent(inputFilePath, tempDirectory);

		// Assert: Verify that the method returns false for a non-p7m file
		Assert.IsFalse(File.Exists(Path.Combine(tempDirectory!, "notap7m")));
	}

	private X509Certificate2 GenerateSelfSignedCertificate()
	{
		// Generate a self-signed certificate with the specified properties
		string subjectName = "Test Certificate";
		string issuerName = subjectName; // Self-signed
		int keySizeInBits = 2048; // Adjust key size as needed
		DateTime expirationDate = DateTime.UtcNow.AddYears(1); // Set the expiration date

		X509Certificate2 certificate = SelfSignedCertificateGenerator.GenerateSelfSignedCertificate(subjectName, issuerName, keySizeInBits, expirationDate);

		return certificate;
	}

	private byte[] CreateMockPkcs7Message(byte[] contentBytes, X509Certificate2 certificate)
	{
		ContentInfo contentInfo = new ContentInfo(contentBytes);

		// Create a SignedCms object
		SignedCms signedCms = new SignedCms(contentInfo, detached: false);

		// Create a signer using the provided certificate
		CmsSigner signer = new CmsSigner(certificate);

		// Sign the content
		signedCms.ComputeSignature(signer);

		// Encode the SignedCms object as bytes
		byte[] encodedMessage = signedCms.Encode();

		return encodedMessage;
	}
}

public class SelfSignedCertificateGenerator
{
	public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string issuerName, int keySizeInBits, DateTime expirationDate)
	{
		using (RSA rsa = RSA.Create(keySizeInBits))
		{
			var request = new CertificateRequest(
				new X500DistinguishedName($"CN={subjectName}"),
				rsa,
				HashAlgorithmName.SHA256,
				RSASignaturePadding.Pkcs1);

			// Set the issuer name the same as the subject name for a self-signed certificate
			request.CertificateExtensions.Add(
				new X509BasicConstraintsExtension(true, false, 0, true));
			request.CertificateExtensions.Add(
				new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

			// Create the self-signed certificate
			var certificate = request.CreateSelfSigned(
				DateTimeOffset.UtcNow.AddDays(-1),
				expirationDate);

			return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
		}
	}
}
