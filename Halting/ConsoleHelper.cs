/*  ************************************************************************
 *  Copyright 2015 Charles Young
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  ************************************************************************/

namespace Halting
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Helper class to handle Console configuration, including Font selection.
    /// </summary>
	public static class ConsoleHelper 
    {
        /// <summary>
        /// Indicates use of the standard output device. Initially, this is the 
        /// active console screen buffer, CONOUT$.
        /// </summary>
        private const int STD_OUTPUT_HANDLE = -11;

        /// <summary>
        /// Bitflag for testing discover of TrueType fonts.
        /// </summary>
        private const int TMPF_TRUETYPE = 4;

        /// <summary>
        /// Pointer value indicating invalid handle.
        /// </summary>
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /// <summary>
        /// Buffer size for font face character array.
        /// </summary>
        private const int LF_FACESIZE = 32;

        /// <summary>
        /// Sets the font used for the console.
        /// </summary>
        /// <param name="output">Handle for the standard output stream.</param>
        /// <param name="index">Font f the font to be set.</param>
        /// <returns>True, if font sucessfully set; otherwise false.</returns>
		[DllImport("kernel32")]
		private static extern  bool SetConsoleFont(IntPtr hOutput, uint index);

        /// <summary>
        /// Retrieves a handle to the specified standard device (standard input, 
        /// standard output, or standard error).
        /// </summary>
        /// <param name="nStdHandle">
        /// The standard device. This parameter can be one of the following values.
        /// </param>
        /// <returns>
        /// <para>
        /// If the function succeeds, the return value is a handle to the specified 
        /// device, or a redirected handle set by a previous call to SetStdHandle. 
        /// The handle has GENERIC_READ and GENERIC_WRITE access rights, unless the a
        /// pplication has used SetStdHandle to set a standard handle with lesser access.
        /// </para>
        /// <para>
        /// If the function fails, the return value is INVALID_HANDLE_VALUE. To get 
        /// extended error information, call GetLastError.
        /// </para>
        /// <para>
        /// If an application does not have associated standard handles, such as a 
        /// service running on an interactive desktop, and has not redirected them, the 
        /// return value is NULL.
        /// </para>
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consoleOutput">
        /// A handle to the console screen buffer. The handle must have the GENERIC_WRITE 
        /// access right. For more information, see Console Buffer Security and Access Rights.
        /// </param>
        /// <param name="maximumWindow">
        /// If this parameter is TRUE, font information is obtained for the maximum window size. 
        /// If this parameter is FALSE, font information is set obtained the current window size.
        /// </param>
        /// <param name="count">The number of fonts selected into the fonts array.</param>
        /// <param name="fonts">The fonts array.  This is populated by the call.</param>
        /// <returns>True, if fonts are found; otherwise false.</returns>
        [DllImport("kernel32")]
        private static extern bool GetConsoleFontInfo(
            IntPtr consoleOutput,
            [MarshalAs(UnmanagedType.Bool)]bool maximumWindow,
            uint count,
            [MarshalAs(UnmanagedType.LPArray), Out] ConsoleFontInfo[] fonts);

        /// <summary>
        /// Gets the number of console fonts.
        /// </summary>
        /// <returns>The number of console fonts.</returns>
        [DllImport("kernel32")]
        private static extern uint GetNumberOfConsoleFonts();

        /// <summary>
        /// Sets console icon,
        /// </summary>
        /// <param name="hIcon">Handle of icon (<see cref="Icon.Handle"/>).</param>
        /// <returns>True of succes, false on error.</returns>
		[DllImport("kernel32")]
        private static extern bool SetConsoleIcon(IntPtr hIcon);

        /// <summary>
        /// Gets extended information for the current console font.
        /// </summary>
        /// <param name="consoleOutput">
        /// A handle to the console screen buffer. The handle must have the GENERIC_WRITE 
        /// access right. For more information, see Console Buffer Security and Access Rights.
        /// </param>
        /// <param name="maximumWindow">
        /// If this parameter is TRUE, font information is obtained for the maximum window size. 
        /// If this parameter is FALSE, font information is set obtained the current window size.
        /// </param>
        /// <param name="consoleCurrentFontEx">
        /// A pointer to a CONSOLE_FONT_INFOEX structure that contains the font information.
        /// </param>
        /// <returns>True, if the console icon is set successfully; otherwise false.</returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetCurrentConsoleFontEx(
               IntPtr consoleOutput,
               bool maximumWindow,
               ref ConsoleFontInfoEx lpConsoleCurrentFontEx);

        /// <summary>
        /// Sets extended information about the current console font.
        /// </summary>
        /// <param name="consoleOutput">
        /// A handle to the console screen buffer. The handle must have the GENERIC_WRITE 
        /// access right. For more information, see Console Buffer Security and Access Rights.
        /// </param>
        /// <param name="maximumWindow">
        /// If this parameter is TRUE, font information is set for the maximum window size. 
        /// If this parameter is FALSE, font information is set for the current window size.
        /// </param>
        /// <param name="consoleCurrentFontEx">
        /// A pointer to a CONSOLE_FONT_INFOEX structure that contains the font information.
        /// </param>
        /// <returns>
        /// True, if the console font information is set successfully; otherwise false.
        /// </returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetCurrentConsoleFontEx(
            IntPtr consoleOutput,
            bool maximumWindow,
            ref ConsoleFontInfoEx consoleCurrentFontEx);

        /// <summary>
        /// Selects the first TrueType font found in the collection of console fonts.
        /// </summary>
        /// <returns>True, if a TrueType font is selected; otherwise false.</returns>
        private static bool SelectFirstTrueTypeFont()
        {
            IntPtr stdout = GetStdHandle(STD_OUTPUT_HANDLE);
            uint nFont = 0;

            // First see if we are truetype already
            if (IsConsoleFontTrueType())
            {
                return true;
            }

            // Get the current index to set it back if no font is found.
            nFont = GetCurrentConsoleFont().Font;

            // Get a collection of all fonts
            var fonts = Fonts;

            // Try to set each font until a TrueType one is found.
            for (int idxFont = 0; idxFont < fonts.Length; idxFont++)
            {
                if (SetConsoleFont(stdout, fonts[idxFont].Font))
                {
                    if (IsConsoleFontTrueType())
                    {
                        return true;
                    }
                }
            }

            // Could not find a font, so give up
            SetConsoleFont(stdout, nFont);
            return false;
        }

        /// <summary>
        /// Indicates if the current console font is a TrueType font.
        /// </summary>
        /// <returns>
        /// True, if the current console font is a Truetype font; otherwise false.
        /// </returns>
        private static bool IsConsoleFontTrueType()
        {
            return ((GetCurrentConsoleFont().FontFamily & TMPF_TRUETYPE) == TMPF_TRUETYPE);
        }

        /// <summary>
        /// Set the console window icon.
        /// </summary>
        /// <param name="icon">
        /// The icon.
        /// </param>
        /// <returns>True, if the icon is set successfully; otherwise false.</returns>
        public static bool SetIcon(Icon icon) 
        {
			return SetConsoleIcon(icon.Handle);
		}

        /// <summary>
        /// Set the console font using the registered index number for that font.
        /// </summary>
        /// <param name="index">The font index number.</param>
        /// <returns>True, if the console font is set successfully; otherwise false.</returns>
        public static bool SetFont(uint index)
        {
            return SetConsoleFont(GetStdHandle(STD_OUTPUT_HANDLE), index);
        }

        /// <summary>
        /// The number of registered console fonts.
        /// </summary>
        public static uint FontsCount
        {
            get
            {
                return GetNumberOfConsoleFonts();
            }
        }

        /// <summary>
        /// The collection of console fonts.
        /// </summary>
        public static ConsoleFontInfo[] Fonts
        {
            get
            {
                ConsoleFontInfo[] fonts = new ConsoleFontInfo[GetNumberOfConsoleFonts()];

                if (fonts.Length > 0)
                {
                    GetConsoleFontInfo(GetStdHandle(STD_OUTPUT_HANDLE), false, (uint)fonts.Length, fonts);
                }

                return fonts;
            }
        }

        /// <summary>
        /// Gets the font information for the current console font.
        /// </summary>
        /// <returns>A structure containing console font information.</returns>
        public static ConsoleFontInfoEx GetCurrentConsoleFont()
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);

            if (hnd != INVALID_HANDLE_VALUE)
            {
                ConsoleFontInfoEx info = new ConsoleFontInfoEx();
                info.Size = (uint)Marshal.SizeOf(info);

                if (GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    return info;
                }
            }

            return new ConsoleFontInfoEx
            {
                Size = (uint)Marshal.SizeOf(typeof(ConsoleFontInfoEx))
            };
        }

        /// <summary>
        /// Set the font for the console.
        /// </summary>
        /// <param name="faceName">
        /// The name of a TrueType font face.  Pass a Null or empty string for 'raster'.
        /// </param>
        /// <returns>True, if the console font is set correctly; otherwise false.</returns>
        public static bool SetCurrentConsoleFont(string faceName)
        {
            ConsoleFontInfoEx info = GetCurrentConsoleFont();
            return SetCurrentConsoleFont(faceName, (ushort)info.FontSize.Y, false);
        }

        /// <summary>
        /// Set the font for the console.
        /// </summary>
        /// <param name="faceName">
        /// The name of a TrueType font face.  Pass a Null or empty string for 'raster'.
        /// </param>
        /// <param name="fontSize">The font size (points).</param>
        /// <returns>True, if the console font is set correctly; otherwise false.</returns>
        public static bool SetCurrentConsoleFont(string faceName, ushort fontSize)
        {
            ConsoleFontInfoEx info = GetCurrentConsoleFont();
            return SetCurrentConsoleFont(faceName, fontSize, false);
        }

        /// <summary>
        /// Set the font for the console.
        /// </summary>
        /// <param name="faceName">
        /// The name of a TrueType font face.  Pass a Null or empty string for 'raster'.
        /// </param>
        /// <param name="bold">
        /// Indicates if a bold font should be used.
        /// </param>
        /// <returns>True, if the console font is set correctly; otherwise false.</returns>
        public static bool SetCurrentConsoleFont(string faceName, bool bold)
        {
            ConsoleFontInfoEx info = GetCurrentConsoleFont();
            return SetCurrentConsoleFont(faceName, (ushort)info.FontSize.Y, bold);
        }

        /// <summary>
        /// Set the font for the console.
        /// </summary>
        /// <param name="faceName">
        /// The name of a TrueType font face.  Pass a Null or empty string for 'raster'.
        /// </param>
        /// <param name="fontSize">The font size (points).</param>
        /// <param name="bold">
        /// Indicates if a bold font should be used.
        /// </param>
        /// <returns>True, if the console font is set correctly; otherwise false.</returns>
        public static bool SetCurrentConsoleFont(string faceName, ushort fontSize, bool bold)
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);

            if (string.IsNullOrWhiteSpace(faceName))
            {
                if (SelectFirstTrueTypeFont())
                {
                    faceName = GetCurrentConsoleFont().FaceName;
                }
            }

            if (hnd != INVALID_HANDLE_VALUE)
            {
                // Set console font.
                ConsoleFontInfoEx newInfo = new ConsoleFontInfoEx
                {
                    FontSize = new Coord(0, (short)fontSize),
                    FontWeight = bold ? 700 : 400,
                    FaceName = faceName,
                    Size = (uint)Marshal.SizeOf(typeof(ConsoleFontInfoEx))
                };

                return SetCurrentConsoleFontEx(hnd, false, ref newInfo);
            }

			return false;
        }

        /// <summary>
        /// Contains information for a console font.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ConsoleFontInfo
        {
            /// <summary>
            /// The index of the font in the system's console font table.
            /// </summary>
            public uint Font;

            /// <summary>
            /// A COORD structure that contains the width and height of each character in the font, 
            /// in logical units. The X member contains the width, while the Y member contains the height.
            /// </summary>
            public Coord FontSize;
        }

        /// <summary>
        /// Contains extended information for a console font.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ConsoleFontInfoEx
        {
            /// <summary>
            /// The size of this structure, in bytes.
            /// </summary>
            public uint Size;

            /// <summary>
            /// The index of the font in the system's console font table.
            /// </summary>
            public uint Font;

            /// <summary>
            /// A COORD structure that contains the width and height of each character in the font, 
            /// in logical units. The X member contains the width, while the Y member contains the height.
            /// </summary>
            public Coord FontSize;

            /// <summary>
            /// The font pitch and family. For information about the possible values for this member, see 
            /// the description of the tmPitchAndFamily member of the TEXTMETRIC structure.
            /// </summary>
            public int FontFamily;

            /// <summary>
            /// The font weight. The weight can range from 100 to 1000, in multiples of 100. For example, 
            /// the normal weight is 400, while 700 is bold.
            /// </summary>
            public int FontWeight;

            /// <summary>
            /// To obtain the size of the font, pass the font index to the GetConsoleFontSize function.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
            public string FaceName;
        }

        /// <summary>
        /// Defines the coordinates of a character cell in a console screen buffer. The origin of the 
        /// coordinate system (0,0) is at the top, left cell of the buffer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Coord"/> structure.
            /// </summary>
            /// <param name="X">The horizontal coordinate or column value.</param>
            /// <param name="Y">The vertical coordinate or row value.</param>
            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }

            /// <summary>
            /// The horizontal coordinate or column value. The units depend on the function call.
            /// </summary>
            public short X;

            /// <summary>
            /// The vertical coordinate or row value. The units depend on the function call.
            /// </summary>
            public short Y;
        }
	}
}