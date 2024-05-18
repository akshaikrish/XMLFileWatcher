using Xunit;
using FileWatcher;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Xml;
using System.Text.RegularExpressions;

namespace FileWatcherTest
{
    public class FileProcessorTests
    {
        [Theory]
        [InlineData("<root><mrn>1234567A</mrn></root>", true)]
        [InlineData("<root><mrn>1234567Z</mrn></root>", true)]
        [InlineData("<root><mrn>12345678</mrn></root>", false)]
        [InlineData("<root><mrn>1234567</mrn></root>", false)]
        [InlineData("<root><mrn>ABCDEFGH</mrn></root>", false)]
        [InlineData("<root><mrn></mrn></root>", false)]
        [InlineData("<root></root>", false)]
        public void ValidateMrnFormat_ShouldReturnExpectedResult(string xmlContent, bool expectedResult)
        {
            
            bool result = ValidateMrnFormat(xmlContent);

            // Assert
            Assert.Equal(expectedResult, result);
        }
        
        public static bool ValidateMrnFormat(string xmlContent)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                XmlNode? mrnNode = doc.SelectSingleNode("//mrn");

                if (mrnNode == null)
                {
                    return false;
                }

                string mrn = mrnNode.InnerText.Trim(); // Trim whitespace

                if (string.IsNullOrWhiteSpace(mrn)) // Check if empty or contains only whitespace
                {
                    return false;
                }

                return Regex.IsMatch(mrn, @"^\d{7}[A-Za-z]$");
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
