<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:myObj="urn:Helper"
xmlns:t="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"				>
  <xsl:param name="today"></xsl:param>
  <xsl:param name="results"></xsl:param>
  <xsl:param name="pass" select="'Passed'"/>
  <xsl:param name="fail" select="'Failed'"/>
  <xsl:key name="class-key" match="@className" use="."/>
  <xsl:variable name="unique-classes" select="//t:TestMethod/@className[generate-id(.)=generate-id(key('class-key',.))]" />  
  <xsl:template match="/">

    <html>
      <body style="font-family:Verdana; font-size:10pt">
<!--
        <xsl:for-each select="$unique-classes">
          <xsl:value-of select="."/>
          <br/>
        </xsl:for-each>
        -->
        <h1>Test Results Summary</h1>
        <table style="font-family:Verdana; font-size:10pt">
          <tr>
            <td>
              <b>Run Date/Time</b>
            </td>
            <td>
            </td>
          </tr>
          <tr>
            <td>
              Start Time:
            </td>
            <td>
              <xsl:value-of select="myObj:DateTimeToString(//t:TestRun/t:Times/@start)"/>
            </td>
          </tr>
          <tr>
            <td>
              End Time:
            </td>
            <td>
              <xsl:value-of select="myObj:DateTimeToString(//t:TestRun/t:Times/@finish)"/>
            </td>
          </tr>
          <tr>
              <td>
              Duration: 
              </td>
              <td>
                <xsl:value-of select="myObj:TimeSpan(//t:TestRun/t:Times/@start,//t:TestRun/t:Times/@finish)"/>
            </td>

          </tr>
          <tr>
            <td>
              <b>Results File</b>
            </td>
            <td>
              <xsl:value-of select="$results"/>
            </td>
          </tr>
        </table>
        <a href="coverage.htm">Coverage Summary</a>
        <xsl:call-template name="summary" />
		  <!--<xsl:call-template name="details" />-->
		  <xsl:call-template name="details2" />
      </body>
    </html>
  </xsl:template>
  
  <xsl:template name="summary">
    <h3>Test Summary</h3>
    <table style="width:640;border:1px solid black;font-family:Verdana; font-size:10pt">
      <tr>
        <td style="font-weight:bold">Total</td>
        <td style="font-weight:bold">Failed</td>
        <td style="font-weight:bold">Passed</td>
        <td style="font-weight:bold">Inconclusive</td>
      </tr>

      <tr>
        <td >
          <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@total"/>
        </td>
        <td style="background-color:pink;">
          <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@failed"/>
        </td>
        <td style="background-color:lightgreen;">
          <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@passed"/>
        </td>
        <td style="background-color:yellow;">
          <xsl:value-of select="/t:TestRun/t:ResultSummary/t:Counters/@inconclusive"/>
        </td>
      </tr>
    </table>

  </xsl:template>

  <xsl:template name="details">

    <h3>Unit Test Results</h3>
    <table style="width:640;border:1px solid black;font-family:Verdana; font-size:10pt;">
      <tr>
        <td style="font-weight:bold">Test Name</td>
        <td style="font-weight:bold">Result</td>
        <td style="font-weight:bold">Duration</td>
      </tr>
      <xsl:for-each select="/t:TestRun/t:Results/t:UnitTestResult">
        <xsl:sort select="@testName"/>
        <tr>
          <xsl:attribute name="style">
            <xsl:choose>
              <xsl:when test="@outcome='Failed'">background-color:pink;</xsl:when>
              <xsl:when test="@outcome='Passed'">background-color:lightgreen;</xsl:when>
              <xsl:otherwise>background-color:yellow;</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <td>
            <xsl:value-of select="@testName"/>
          </td>
          <td>
            <xsl:choose>
              <xsl:when test="@outcome='Failed'">FAILED</xsl:when>
              <xsl:when test="@outcome='Passed'">Passed</xsl:when>
              <xsl:otherwise>Inconclusive</xsl:otherwise>
            </xsl:choose>
          </td>
          <td>
            <xsl:value-of select="@duration"/>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="details2">

    <h3>Unit Test Results</h3>
    <table border="0" style="width:640;border:1px solid black;font-family:Verdana; font-size:10pt;">
      <xsl:for-each select="$unique-classes">
        <xsl:sort />
        <xsl:variable name="curClass" select="."/>
        <xsl:variable name="return" select="myObj:GetClassInformation($curClass)"/>
        <!--<xsl:for-each select="//TestRun/tests/value/testMethod[className=$curClass]">-->

        <tr>
          <td valign="bottom" style="background-color:beige;font-weight:bold;" colspan="3">
            <font>
              <xsl:value-of select="concat('',$return/className)"/>
            </font>
          </td>
        </tr>
        <tr>
          <td style="font-weight:bold">Test Name</td>
          <td style="font-weight:bold">Result</td>
          <td style="font-weight:bold">Duration</td>
        </tr>
          <xsl:for-each select="//t:UnitTest/t:TestMethod[@className=$curClass]">
          <xsl:sort select="@name"/>
          <xsl:variable name="testid" select="../@id"/>
          <xsl:for-each select="//t:UnitTestResult[@testId=$testid]">
            <xsl:call-template name="classRunsDetail">
            <xsl:with-param name="testid" select="."/>
          </xsl:call-template>
          </xsl:for-each>
        </xsl:for-each>
        <tr>
          <td style="border-bottom:0px solid black;height:1px;background-color:black" colspan="3"></td>
        </tr>

      </xsl:for-each>
    </table>      
  </xsl:template>

  <xsl:template name="classRunsDetail">
    <xsl:param name="testid"/>
        <tr>
          <xsl:attribute name="style">
            <xsl:choose>
              <xsl:when test="@outcome = $fail">background-color:pink;</xsl:when>
              <xsl:when test="@outcome = $pass">background-color:lightgreen;</xsl:when>
              <xsl:otherwise>background-color:yellow;</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <td>
            <xsl:value-of select="@testName"/>
          </td>
          <td>
            <xsl:choose>
              <xsl:when test="@outcome = $fail">FAILED</xsl:when>
              <xsl:when test="@outcome = $pass">Passed</xsl:when>
              <xsl:otherwise>Inconclusive</xsl:otherwise>
            </xsl:choose>
          </td>
          <td>
            <xsl:value-of select="@duration"/>
          </td>
        </tr>
      
  </xsl:template>
  
</xsl:stylesheet>