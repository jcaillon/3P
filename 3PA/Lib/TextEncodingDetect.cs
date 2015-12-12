#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (TextEncodingDetect.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.IO;
using System.Text;

namespace _3PA.Lib {

    /// <summary>
    /// This class offers a simple detection of a text file encoding
    /// </summary>
    /// <remarks>Credits go to: https://github.com/AutoIt/text-encoding-detect</remarks>
    class TextEncodingDetect {
        #region Fields

        private readonly byte[] _utf16Lebom = { 0xFF, 0xFE };
        private readonly byte[] _utf16Bebom = { 0xFE, 0xFF };
        private readonly byte[] _utf8Bom = { 0xEF, 0xBB, 0xBF };

        private bool _nullSuggestsBinary = true;
        private double _utf16ExpectedNullPercent = 70;
        private double _utf16UnexpectedNullPercent = 10;

        #endregion

        #region Enums

        public enum EncodingEnum {
            None,               // Unknown or binary
            Ansi,               // 0-255
            Ascii,              // 0-127
            Utf8Bom,           // UTF8 with BOM
            Utf8Nobom,         // UTF8 without BOM
            Utf16LeBom,       // UTF16 LE with BOM
            Utf16LeNobom,     // UTF16 LE without BOM
            Utf16BeBom,       // UTF16-BE with BOM
            Utf16BeNobom      // UTF16-BE without BOM
        }

        #endregion

        #region Properties

        public bool NullSuggestsBinary {
            set {
                _nullSuggestsBinary = value;
            }
        }

        public double Utf16ExpectedNullPercent {
            set {
                if (value > 0 && value < 100) {
                    _utf16ExpectedNullPercent = value;
                }
            }
        }

        public double Utf16UnexpectedNullPercent {
            set {
                if (value > 0 && value < 100) {
                    _utf16UnexpectedNullPercent = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns the encoding of the input file
        /// </summary>
        /// <param name="srcFile"></param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(string srcFile) {
            Encoding encoding = Encoding.Default;

            // Read in the file in binary
            byte[] buffer;

            try {
                buffer = File.ReadAllBytes(srcFile);
            } catch (Exception) {
                return encoding;
            }

            // Detect encoding
            var textDetect = new TextEncodingDetect();
            EncodingEnum textEnc = textDetect.DetectEncoding(buffer, buffer.Length);

            switch (textEnc) {
                case EncodingEnum.Ascii:
                    // ASCII (chars in the 0-127 range)
                    encoding = Encoding.ASCII;
                    break;
                case EncodingEnum.Ansi:
                    // ANSI (chars in the range 0-255 range)
                    encoding = Encoding.GetEncoding(1252);
                    break;
                case EncodingEnum.Utf8Bom:
                case EncodingEnum.Utf8Nobom:
                    // UTF-8
                    encoding = Encoding.UTF8;
                    break;
                case EncodingEnum.Utf16LeBom:
                case EncodingEnum.Utf16LeNobom:
                    // UTF-16 Little Endian
                    encoding = Encoding.Unicode;
                    break;
                case EncodingEnum.Utf16BeBom:
                case EncodingEnum.Utf16BeNobom:
                    // UTF-16 Big Endian
                    encoding = Encoding.BigEndianUnicode;
                    break;
            }
            return encoding;
        }

        public static int GetBomLengthFromEncodingMode(EncodingEnum encodingEnum) {
            int length = 0;

            if (encodingEnum == EncodingEnum.Utf16BeBom || encodingEnum == EncodingEnum.Utf16LeBom) {
                length = 2;
            } else if (encodingEnum == EncodingEnum.Utf8Bom) {
                length = 3;
            }

            return length;
        }

        public EncodingEnum CheckBom(byte[] buffer, int size) {
            // Check for BOM
            if (size >= 2 && buffer[0] == _utf16Lebom[0] && buffer[1] == _utf16Lebom[1]) {
                return EncodingEnum.Utf16LeBom;
            }
            if (size >= 2 && buffer[0] == _utf16Bebom[0] && buffer[1] == _utf16Bebom[1]) {
                return EncodingEnum.Utf16BeBom;
            }
            if (size >= 3 && buffer[0] == _utf8Bom[0] && buffer[1] == _utf8Bom[1] && buffer[2] == _utf8Bom[2]) {
                return EncodingEnum.Utf8Bom;
            }
            return EncodingEnum.None;
        }

        public EncodingEnum DetectEncoding(byte[] buffer, int size) {
            // First check if we have a BOM and return that if so
            EncodingEnum encodingEnum = CheckBom(buffer, size);
            if (encodingEnum != EncodingEnum.None) {
                return encodingEnum;
            }

            // Now check for valid UTF8
            encodingEnum = CheckUtf8(buffer, size);
            if (encodingEnum != EncodingEnum.None) {
                return encodingEnum;
            }

            // Now try UTF16 
            encodingEnum = CheckUtf16NewlineChars(buffer, size);
            if (encodingEnum != EncodingEnum.None) {
                return encodingEnum;
            }

            encodingEnum = CheckUtf16Ascii(buffer, size);
            if (encodingEnum != EncodingEnum.None) {
                return encodingEnum;
            }

            // ANSI or None (binary) then
            if (!DoesContainNulls(buffer, size)) {
                return EncodingEnum.Ansi;
            }
            // Found a null, return based on the preference in null_suggests_binary_
            if (_nullSuggestsBinary) {
                return EncodingEnum.None;
            }
            return EncodingEnum.Ansi;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Checks if a buffer contains valid utf8. Returns:
        // None - not valid utf8
        // UTF8_NOBOM - valid utf8 encodings and multibyte sequences
        // ASCII - Only data in the 0-127 range. 
        ///////////////////////////////////////////////////////////////////////////////

        private EncodingEnum CheckUtf8(byte[] buffer, int size) {
            // UTF8 Valid sequences
            // 0xxxxxxx  ASCII
            // 110xxxxx 10xxxxxx  2-byte
            // 1110xxxx 10xxxxxx 10xxxxxx  3-byte
            // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx  4-byte
            //
            // Width in UTF8
            // Decimal      Width
            // 0-127        1 byte
            // 194-223      2 bytes
            // 224-239      3 bytes
            // 240-244      4 bytes
            //
            // Subsequent chars are in the range 128-191
            bool onlySawAsciiRange = true;
            uint pos = 0;
            int moreChars;

            while (pos < size) {
                byte ch = buffer[pos++];

                if (ch == 0 && _nullSuggestsBinary) {
                    return EncodingEnum.None;
                }
                if (ch <= 127) {
                    // 1 byte
                    moreChars = 0;
                } else if (ch >= 194 && ch <= 223) {
                    // 2 Byte
                    moreChars = 1;
                } else if (ch >= 224 && ch <= 239) {
                    // 3 Byte
                    moreChars = 2;
                } else if (ch >= 240 && ch <= 244) {
                    // 4 Byte
                    moreChars = 3;
                } else {
                    return EncodingEnum.None;               // Not utf8
                }

                // Check secondary chars are in range if we are expecting any
                while (moreChars > 0 && pos < size) {
                    onlySawAsciiRange = false;       // Seen non-ascii chars now

                    ch = buffer[pos++];
                    if (ch < 128 || ch > 191) {
                        return EncodingEnum.None;           // Not utf8
                    }

                    --moreChars;
                }
            }

            // If we get to here then only valid UTF-8 sequences have been processed

            // If we only saw chars in the range 0-127 then we can't assume UTF8 (the caller will need to decide)
            if (onlySawAsciiRange) {
                return EncodingEnum.Ascii;
            }
            return EncodingEnum.Utf8Nobom;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Checks if a buffer contains text that looks like utf16 by scanning for 
        // newline chars that would be present even in non-english text.
        // Returns:
        // None - not valid utf16
        // UTF16_LE_NOBOM - looks like utf16 le
        // UTF16_BE_NOBOM - looks like utf16 be
        ///////////////////////////////////////////////////////////////////////////////

        private EncodingEnum CheckUtf16NewlineChars(byte[] buffer, int size) {
            if (size < 2) {
                return EncodingEnum.None;
            }

            // Reduce size by 1 so we don't need to worry about bounds checking for pairs of bytes
            size--;

            int leControlChars = 0;
            int beControlChars = 0;
            byte ch1, ch2;

            uint pos = 0;
            while (pos < size) {
                ch1 = buffer[pos++];
                ch2 = buffer[pos++];

                if (ch1 == 0) {
                    if (ch2 == 0x0a || ch2 == 0x0d) {
                        ++beControlChars;
                    }
                } else if (ch2 == 0) {
                    if (ch1 == 0x0a || ch1 == 0x0d) {
                        ++leControlChars;
                    }
                }

                // If we are getting both LE and BE control chars then this file is not utf16
                if (leControlChars > 0 && beControlChars > 0) {
                    return EncodingEnum.None;
                }
            }

            if (leControlChars > 0) {
                return EncodingEnum.Utf16LeNobom;
            }
            if (beControlChars > 0) {
                return EncodingEnum.Utf16BeNobom;
            }
            return EncodingEnum.None;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Checks if a buffer contains text that looks like utf16. This is done based
        // the use of nulls which in ASCII/script like text can be useful to identify.
        // Returns:
        // None - not valid utf16
        // UTF16_LE_NOBOM - looks like utf16 le
        // UTF16_BE_NOBOM - looks like utf16 be
        ///////////////////////////////////////////////////////////////////////////////

        private EncodingEnum CheckUtf16Ascii(byte[] buffer, int size) {
            int numOddNulls = 0;
            int numEvenNulls = 0;

            // Get even nulls
            uint pos = 0;
            while (pos < size) {
                if (buffer[pos] == 0) {
                    numEvenNulls++;
                }

                pos += 2;
            }

            // Get odd nulls
            pos = 1;
            while (pos < size) {
                if (buffer[pos] == 0) {
                    numOddNulls++;
                }

                pos += 2;
            }

            double evenNullThreshold = (numEvenNulls * 2.0) / size;
            double oddNullThreshold = (numOddNulls * 2.0) / size;
            double expectedNullThreshold = _utf16ExpectedNullPercent / 100.0;
            double unexpectedNullThreshold = _utf16UnexpectedNullPercent / 100.0;

            // Lots of odd nulls, low number of even nulls
            if (evenNullThreshold < unexpectedNullThreshold && oddNullThreshold > expectedNullThreshold) {
                return EncodingEnum.Utf16LeNobom;
            }

            // Lots of even nulls, low number of odd nulls
            if (oddNullThreshold < unexpectedNullThreshold && evenNullThreshold > expectedNullThreshold) {
                return EncodingEnum.Utf16BeNobom;
            }

            // Don't know
            return EncodingEnum.None;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Checks if a buffer contains any nulls. Used to check for binary vs text data.
        ///////////////////////////////////////////////////////////////////////////////

        private bool DoesContainNulls(byte[] buffer, int size) {
            uint pos = 0;
            while (pos < size) {
                if (buffer[pos++] == 0) {
                    return true;
                }
            }

            return false;
        }
    }
}
