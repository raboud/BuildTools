using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace RandREng.MsBuildTasks
{
    public class VerifyTestsPassedTask : Task
    {
        string _testResultsFile=string.Empty;
        [Required]
        public string TestResultsFile
        {
            set { _testResultsFile = value; }
        }

        public override bool Execute()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_testResultsFile);

            XmlNode nav = xdoc.SelectSingleNode("/Tests");

            bool vs2005 = nav != null;

            if (vs2005)
            {
                return processTrx2005();
            }
            else
            {
                return processTrx2008();
            }
        }
                
        private bool processTrx2005()
        {
            XPathDocument xdoc = new XPathDocument(_testResultsFile);

            xdoc.CreateNavigator().SelectSingleNode("/Tests");

            XPathNodeIterator navUnitTest = xdoc.CreateNavigator().Select("//UnitTestResult[outcome/value__=1]"); //failed
            XPathNavigator nav2 = xdoc.CreateNavigator().SelectSingleNode("//UnitTestResult[outcome/value__!=1 and value__!=10]"); //inconclusive
            string testId;
            string className;
            if (navUnitTest.Count > 0)
            {
                while (navUnitTest.MoveNext())
                {
                    XPathNavigator navTestName = navUnitTest.Current.SelectSingleNode("./testName");
                    XPathNavigator navstdOut = navUnitTest.Current.SelectSingleNode("stdout");
                    string errorMessage = navUnitTest.Current.SelectSingleNode("errorInfo/message").Value;
                    string stackTrace = navUnitTest.Current.SelectSingleNode("errorInfo/stackTrace").Value;

                    XPathNavigator navId = navUnitTest.Current.SelectSingleNode("./id/testId/id");
                    testId = navId.Value;
                    XPathNavigator navTest = xdoc.CreateNavigator().SelectSingleNode(string.Format("/Tests/TestRun/tests/value/testMethod/className[../../id/id='{0}']", testId));
                    className = GetClassName(navTest.Value);

                    string finalString = string.Format("Unit Test {0}.{1} FAILED", className, navTestName.Value, errorMessage, stackTrace);
                    //We have to filter out the carriage returns bacause msbuild will prepend the text following the carriate return with extra verbiage.
                    finalString = finalString.Replace("\r\n", "");
                    Log.LogError(finalString);
                    //Log.LogError("<a href=\"www.microsoft.com\">Click Here</a>");
                }
                return false;   
            }
            return true;
        }
        
        private bool processTrx2008()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_testResultsFile);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("ns", "http://microsoft.com/schemas/VisualStudio/TeamTest/2006");

            XmlNodeList navUnitTest = xdoc.SelectNodes("//ns:UnitTestResult[@outcome!='Passed']", nsmgr); //failed
            string testId;
            string className;
            if (navUnitTest.Count > 0)
            {
                foreach (XmlNode node in navUnitTest)
                {
                    string errorMessage = string.Empty;
                    string stackTrace = string.Empty;

                    XmlNode navTestName = node.SelectSingleNode("@testName", nsmgr);
                    XmlNode navstdOut = node.SelectSingleNode("ns:Output/ns:ErrorInfo", nsmgr);
                    if (navstdOut != null)
                    {
                        errorMessage = navstdOut.SelectSingleNode("ns:Message", nsmgr).InnerText;
                        stackTrace = navstdOut.SelectSingleNode("ns:StackTrace", nsmgr).InnerText;
                    }
                    string status = node.SelectSingleNode("@outcome").InnerText;

                    testId = node.SelectSingleNode("@testId", nsmgr).InnerText;
                    XmlNode navTest = xdoc.SelectSingleNode(string.Format("/ns:TestRun/ns:TestDefinitions/ns:UnitTest/ns:TestMethod/@className[../../@id='{0}']", testId), nsmgr);
                    className = GetClassName(navTest.Value);

                    string finalString = string.Format("Unit Test {0}.{1} {4}", className, navTestName.Value, errorMessage, stackTrace, status);
                    //We have to filter out the carriage returns bacause msbuild will prepend the text following the carriate return with extra verbiage.
                    finalString = finalString.Replace("\r\n", "");
                    Log.LogError(finalString);
                }
                return false;
            }
            return true;
        }

        private string GetClassName(string classInfo)
        {
            Regex regex = new Regex(@"[,=]", RegexOptions.IgnorePatternWhitespace);
            string[] tokens = regex.Split(classInfo);
        
            string className = tokens[0].Trim();
            return className;

        }
    }
}
