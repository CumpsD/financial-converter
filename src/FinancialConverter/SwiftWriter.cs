namespace FinancialConverter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using FinancialConverter.Statements;

    public class SwiftWriter
    {
        private static readonly CultureInfo ExportCulture = CultureInfo.CreateSpecificCulture("nl-BE");

        private StreamWriter _writer;

        public SwiftWriter(StreamWriter writer)
            => _writer = writer;

        public void WriteRecords(IEnumerable<SwiftStatement> swiftStatements)
        {
            WriteHeader();
            WriteStatements(swiftStatements);
        }

        private void WriteHeader()
            => _writer.WriteLine(":940:");

        private void WriteStatements(IEnumerable<SwiftStatement> swiftStatements)
        {
            var runningTotal = 0m;
            foreach (var swiftStatement in swiftStatements)
                runningTotal = WriteStatement(runningTotal, swiftStatement);
        }

        private decimal WriteStatement(decimal runningTotal, SwiftStatement swiftStatement)
        {
            var date = swiftStatement.Date.Substring(2); // YYMMDD

            _writer.WriteLine($":20:940S{date}");
            _writer.WriteLine($":25:{swiftStatement.Account} EUR");
            _writer.WriteLine($":28C:0");
            _writer.WriteLine($":60F:{(runningTotal > 0 ? 'C' : 'D')}{date}EUR{Math.Abs(runningTotal).ToString("000000000000.00", ExportCulture)}");

            foreach (var swiftLine in swiftStatement.Lines)
            {
                runningTotal = swiftLine.IsCredit
                    ? runningTotal + swiftLine.Amount
                    : runningTotal - swiftLine.Amount;

                WriteLine(date, swiftLine);
            }

            _writer.WriteLine($":62F:{(runningTotal > 0 ? 'C' : 'D')}{date}EUR{Math.Abs(runningTotal).ToString("000000000000.00", ExportCulture)}");

            return runningTotal;
        }

        private void WriteLine(string date, SwiftStatementLine swiftLine)
        {
            var amount = swiftLine.Amount.ToString("000000000000.00", ExportCulture);

            if (swiftLine.IsCredit)
            {
                _writer.WriteLine($":61:{date}C{amount}N127NONREF");
            }
            else
            {
                _writer.WriteLine($":61:{date}D{amount}N035NONREF");
            }

            var reference = Split($":86:/BENM//NAME/{swiftLine.Name}/REMI/{swiftLine.Reference}", 65);
            foreach (var line in reference)
                _writer.WriteLine(line);
        }

        /// <summary>Returns a string array that contains the substrings in this string that are seperated a given fixed length.</summary>
        /// <param name="s">This string object.</param>
        /// <param name="length">Size of each substring.
        ///     <para>CASE: length &gt; 0 , RESULT: String is split from left to right.</para>
        ///     <para>CASE: length == 0 , RESULT: String is returned as the only entry in the array.</para>
        ///     <para>CASE: length &lt; 0 , RESULT: String is split from right to left.</para>
        /// </param>
        /// <returns>String array that has been split into substrings of equal length.</returns>
        /// <example>
        ///     <code>
        ///         string s = "1234567890";
        ///         string[] a = s.Split(4); // a == { "1234", "5678", "90" }
        ///     </code>
        /// </example>
        private static string[] Split(string s, int length)
        {
            var str = new StringInfo(s);
            int lengthAbs = Math.Abs(length);

            if (str == null || str.LengthInTextElements == 0 || lengthAbs == 0)
                return new string[0];

            if (str.LengthInTextElements <= lengthAbs)
                return new string[] { str.String };

            string[] array = new string[(str.LengthInTextElements % lengthAbs == 0 ? str.LengthInTextElements / lengthAbs: (str.LengthInTextElements / lengthAbs) + 1)];

            if (length > 0)
            {
                for (int iStr = 0, iArray = 0; iStr < str.LengthInTextElements && iArray < array.Length; iStr += lengthAbs, iArray++)
                    array[iArray] = str.SubstringByTextElements(iStr, (str.LengthInTextElements - iStr < lengthAbs ? str.LengthInTextElements - iStr : lengthAbs));
            }
            else // if (length < 0)
            {
                for (int iStr = str.LengthInTextElements - 1, iArray = array.Length - 1; iStr >= 0 && iArray >= 0; iStr -= lengthAbs, iArray--)
                    array[iArray] = str.SubstringByTextElements((iStr - lengthAbs < 0 ? 0 : iStr - lengthAbs + 1), (iStr - lengthAbs < 0 ? iStr + 1 : lengthAbs));
            }

            return array;
        }
    }
}
