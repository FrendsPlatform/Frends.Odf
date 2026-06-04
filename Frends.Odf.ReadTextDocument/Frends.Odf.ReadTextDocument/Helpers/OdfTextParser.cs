using System;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Frends.Odf.ReadTextDocument.Helpers
{
    internal static class OdfTextParser
    {
        /// <summary>
        /// Parses an ODF paragraph or heading, converting ODF XML formatting tags into standard strings.
        /// </summary>
        /// <param name="element">XML element to parse.</param>
        /// <param name="textNamespace">Standard ODF text namespace.</param>
        /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
        /// <returns>A string containing the extracted text with converted whitespaces, tabs, and line breaks.</returns>
        internal static string ParseOdfElements(XElement element, XNamespace textNamespace, CancellationToken cancellationToken)
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
                        // Initialise as 1 in case of malformatting in the XML.
                        int whitespaceCount = 1;

                        // Check for 'c' attribute in multiple whitespace tag <text:s text:c="X"/>.
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