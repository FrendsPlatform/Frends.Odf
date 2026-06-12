using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Frends.Odf.ReadSpreadsheet.Helpers
{
    internal class OdfSpreadsheetParser
    {
        private const int MaxColumns = 16384;
        private const int MaxRows = 5000;
        private const string DefaultColumnPrefix = "Column";

        /// <summary>
        /// Iterates through spreadsheet rows and columns to extract data into a JSON array, handling ODF compression attributes.
        /// </summary>
        /// <param name="firstTable">The first sheet of the ODF spreadsheet as an XElement.</param>
        /// <param name="tableNamespace">Standard ODF table namespace.</param>
        /// <param name="textNamespace">Standard ODF text namespace.</param>
        /// <param name="containsHeaderRow">Boolean indicating whether the first row should be parsed as JSON keys.</param>
        /// <param name="cancellationToken">A cancellation token provided by the Frends Platform.</param>
        /// <returns>A JArray containing the mapped JSON objects for each row.</returns>
        internal static JArray ExtractData(XElement firstTable, XNamespace tableNamespace, XNamespace textNamespace, bool containsHeaderRow, CancellationToken cancellationToken)
        {
            var jsonArray = new JArray();
            var headers = new List<string>();
            bool isFirstRow = true;
            int totalProcessedRows = 0;

            foreach (var rowElement in firstTable.Elements(tableNamespace + "table-row"))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rowData = new List<string>();

                foreach (var cell in rowElement.Elements(tableNamespace + "table-cell"))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var paragraphElements = cell.Elements()
                        .Where(x => x.Name == textNamespace + "p" || x.Name == textNamespace + "h")
                        .Select(x => ParseOdfElements(x, textNamespace, cancellationToken));

                    var cellValue = string.Join(Environment.NewLine, paragraphElements);

                    // Check for repeated columns attribute.
                    var repeatedAttribute = cell.Attribute(tableNamespace + "number-columns-repeated");
                    int repeatCount = 1;

                    if (repeatedAttribute != null && int.TryParse(repeatedAttribute.Value, out int repeatedValue))
                    {
                        repeatCount = Math.Min(repeatedValue, MaxColumns);
                    }

                    // Check for spanned columns attribute indicating merged cells.
                    var spanAttribute = cell.Attribute(tableNamespace + "number-columns-spanned");
                    int spanCount = 1;

                    if (spanAttribute != null && int.TryParse(spanAttribute.Value, out int spanValue))
                    {
                        if (spanValue > 1)
                            spanCount = spanValue;
                        else
                            spanCount = 1;
                    }

                    for (int i = 0; i < repeatCount && rowData.Count < MaxColumns; i++)
                    {
                        rowData.Add(cellValue);

                        for (int j = 1; j < spanCount && rowData.Count < MaxColumns; j++)
                        {
                            rowData.Add(string.Empty);
                        }
                    }
                }

                for (int i = rowData.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(rowData[i]))
                        rowData.RemoveAt(i);
                    else
                        break;
                }

                if (rowData.Count == 0)
                    continue;

                // Check for repeated row attribute.
                var rowRepeatedAttribute = rowElement.Attribute(tableNamespace + "number-rows-repeated");
                int rowRepeatCount = 1;

                if (rowRepeatedAttribute != null && int.TryParse(rowRepeatedAttribute.Value, out int rowRepeatedValue))
                {
                    if (rowRepeatedValue < MaxRows)
                        rowRepeatCount = rowRepeatedValue;
                    else
                        rowRepeatCount = MaxRows;
                }

                for (int i = 0; i < rowRepeatCount; i++)
                {
                    if (totalProcessedRows >= MaxRows)
                        break;

                    if (isFirstRow)
                    {
                        if (containsHeaderRow)
                        {
                            headers = ParseHeaders(rowData);
                        }
                        else
                        {
                            headers = new List<string>();
                            jsonArray.Add(CreateJsonRow(rowData, headers));
                        }

                        isFirstRow = false;

                        if (containsHeaderRow)
                            break;
                    }
                    else
                    {
                        jsonArray.Add(CreateJsonRow(rowData, headers));
                    }

                    totalProcessedRows++;
                }

                if (totalProcessedRows >= MaxRows)
                    break;
            }

            return jsonArray;
        }

        /// <summary>
        /// Parses the first row into JSON keys, handling duplicate names and missing headers.
        /// </summary>
        private static List<string> ParseHeaders(List<string> rowData)
        {
            var parsedHeaders = new List<string>();

            for (int i = 0; i < rowData.Count; i++)
            {
                string headerName;

                if (string.IsNullOrWhiteSpace(rowData[i]))
                {
                    headerName = $"{DefaultColumnPrefix}_{i + 1}";
                }
                else
                {
                    headerName = rowData[i];
                }

                string header = headerName;
                int suffix = 1;

                while (parsedHeaders.Contains(header))
                {
                    header = $"{headerName}_{suffix}";
                    suffix++;
                }

                parsedHeaders.Add(header);
            }

            return parsedHeaders;
        }

        /// <summary>
        /// Maps row data to headers to create a JSON object.
        /// </summary>
        private static JObject CreateJsonRow(List<string> rowData, List<string> headers)
        {
            var jObject = new JObject();

            int columnCount;

            if (rowData.Count > headers.Count)
            {
                columnCount = rowData.Count;
            }
            else
            {
                columnCount = headers.Count;
            }

            for (int i = 0; i < columnCount; i++)
            {
                string key;

                if (i < headers.Count)
                {
                    key = headers[i];
                }
                else
                {
                    key = $"{DefaultColumnPrefix}_{i + 1}";
                }

                string uniqueKey = key;
                int suffix = 1;

                while (jObject.ContainsKey(uniqueKey))
                {
                    uniqueKey = $"{key}_{suffix}";
                    suffix++;
                }

                string value;

                if (i < rowData.Count)
                {
                    value = rowData[i];
                }
                else
                {
                    value = string.Empty;
                }

                jObject.Add(uniqueKey, value);
            }

            return jObject;
        }

        /// <summary>
        /// Parses an ODF paragraph or heading, converting ODF XML formatting tags into standard strings.
        /// </summary>
        private static string ParseOdfElements(XElement element, XNamespace textNamespace, CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder();

            foreach (var node in element.DescendantNodes())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (node is XText textNode)
                {
                    stringBuilder.Append(textNode.Value);
                }
                else if (node is XElement xElement)
                {
                    // Check for whitespace tag <text:s>.
                    if (xElement.Name == textNamespace + "s")
                    {
                        int whitespaceCount = 1;
                        var countAttribute = xElement.Attribute(textNamespace + "c");

                        if (countAttribute != null && int.TryParse(countAttribute.Value, out int c) && c > 0)
                        {
                            whitespaceCount = c;
                        }

                        stringBuilder.Append(new string(' ', whitespaceCount));
                    }

                    // Check for tab tag <text:tab>.
                    else if (xElement.Name == textNamespace + "tab")
                    {
                        stringBuilder.Append('\t');
                    }

                    // Check for line break tag <text:line-break>.
                    else if (xElement.Name == textNamespace + "line-break")
                    {
                        stringBuilder.Append(Environment.NewLine);
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}