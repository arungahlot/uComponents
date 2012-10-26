﻿using uComponents.XsltExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.XPath;
using System.IO;

namespace uComponents.Core.UnitTests.XsltExtensions
{
	[TestClass()]
	public class IOTest
	{
		//[TestMethod()]
		//public void DirectoryExistsTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    bool expected = false; // TODO: Initialize to an appropriate value
		//    bool actual;
		//    actual = IO.DirectoryExists(path);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void FileExistsTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    bool expected = false; // TODO: Initialize to an appropriate value
		//    bool actual;
		//    actual = IO.FileExists(path);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void FormatFileSizeTest()
		//{
		//    long filesize = 0; // TODO: Initialize to an appropriate value
		//    string expected = string.Empty; // TODO: Initialize to an appropriate value
		//    string actual;
		//    actual = IO.FormatFileSize(filesize);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void GetDirectoriesTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    string searchPattern = string.Empty; // TODO: Initialize to an appropriate value
		//    bool allDirectories = false; // TODO: Initialize to an appropriate value
		//    XPathNodeIterator expected = null; // TODO: Initialize to an appropriate value
		//    XPathNodeIterator actual;
		//    actual = IO.GetDirectories(path, searchPattern, allDirectories);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void GetDirectoryNameTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    string expected = string.Empty; // TODO: Initialize to an appropriate value
		//    string actual;
		//    actual = IO.GetDirectoryName(path);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void GetExtensionTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    string expected = string.Empty; // TODO: Initialize to an appropriate value
		//    string actual;
		//    actual = IO.GetExtension(path);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void GetFileNameTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    string expected = string.Empty; // TODO: Initialize to an appropriate value
		//    string actual;
		//    actual = IO.GetFileName(path);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void GetFileNameWithoutExtensionTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    string expected = string.Empty; // TODO: Initialize to an appropriate value
		//    string actual;
		//    actual = IO.GetFileNameWithoutExtension(path);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void GetFileSizeTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    long expected = 0; // TODO: Initialize to an appropriate value
		//    long actual;
		//    actual = IO.GetFileSize(path);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void GetFilesTest()
		//{
		//    string path = string.Empty; // TODO: Initialize to an appropriate value
		//    string searchPattern = string.Empty; // TODO: Initialize to an appropriate value
		//    bool allDirectories = false; // TODO: Initialize to an appropriate value
		//    XPathNodeIterator expected = null; // TODO: Initialize to an appropriate value
		//    XPathNodeIterator actual;
		//    actual = IO.GetFiles(path, searchPattern, allDirectories);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Verify the correctness of this test method.");
		//}

		//[TestMethod()]
		//public void LoadFileTest()
		//{
		//    string filepath = string.Empty; // TODO: Initialize to an appropriate value
		//    string expected = string.Empty; // TODO: Initialize to an appropriate value
		//    string actual;
		//    actual = IO.LoadFile(filepath);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Inconclusive. Underlying method/test relies on HttpContext.");
		//}

		//[TestMethod()]
		//public void MapPathTest()
		//{
		//    var useHttpContext = false;
		//    var path = @"~/uComponents.Core/Resources/Images/favicon.ico";
		//    var expected = @"C:\SVN\our.umbraco.org\uComponents\uComponents.Core\Resources\Images\favicon.ico";
		//    var actual = IO.MapPath(path, useHttpContext);
		//    Assert.AreEqual(expected, actual);
		//    Assert.Inconclusive("Inconclusive. Underlying method/test relies on HttpContext.");
		//}

		[TestMethod()]
		public void PathShortenerTest()
		{
			var input = @"C:\SVN\our.umbraco.org\uComponents\uComponents.Core\Properties\AssemblyInfo.cs";
			var expected = @"C:\SVN\our.umbraco.org\...\Properties\AssemblyInfo.cs";
			var actual = IO.PathShortener(input);
			Assert.AreEqual(expected, actual);
		}
	}
}
