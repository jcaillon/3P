using System.Runtime.InteropServices;

namespace _3PA.Lib {
    public static class HtmlHelpInterop {

        public const int HhDisplayIndex = 0x0002;

        // This overload is for passing a string as the dwData parameter (for example, for the HH_DISPLAY_INDEX command)
        [DllImport("hhctrl.ocx", CharSet = CharSet.Unicode, EntryPoint = "HtmlHelpW")]
        public static extern int HtmlHelp(int caller, string file, uint command, string str);

        public static int DisplayIndex(int caller, string file, string index) {
            return HtmlHelp(caller, file, HhDisplayIndex, index);
        }

        #region Full api

        /*
        // Constants
        const int HH_MAX_TABS = 19; // maximum number of tabs

        // commands
        protected const int HH_DISPLAY_TOPIC = 0x0000;
        protected const int HH_DISPLAY_INDEX = 0x0002;
        protected const int HH_SET_WIN_TYPE = 0x0004;  // [Use HtmlHelp_SetWinType()]
        protected const int HH_GET_WIN_TYPE = 0x0005;  // 

        // parameter info used with HH_WINTYPE struct
        public const int HHWIN_PARAM_PROPERTIES = (1 << 1);    // valid fsWinProperties
        public const int HHWIN_PARAM_STYLES = (1 << 2);    // valid dwStyles
        public const int HHWIN_PARAM_EXSTYLES = (1 << 3);    // valid dwExStyles
        public const int HHWIN_PARAM_RECT = (1 << 4);    // valid rcWindowPos
        public const int HHWIN_PARAM_NAV_WIDTH = (1 << 5);    // valid iNavWidth
        public const int HHWIN_PARAM_SHOWSTATE = (1 << 6);    // valid nShowState
        public const int HHWIN_PARAM_INFOTYPES = (1 << 7);    // valid apInfoTypes
        public const int HHWIN_PARAM_TB_FLAGS = (1 << 8);    // valid fsToolBarFlags
        public const int HHWIN_PARAM_EXPANSION = (1 << 9);    // valid fNotExpanded
        public const int HHWIN_PARAM_TABPOS = (1 << 10);   // valid tabpos
        public const int HHWIN_PARAM_TABORDER = (1 << 11);   // valid taborder
        public const int HHWIN_PARAM_HISTORY_COUNT = (1 << 12);   // valid cHistory
        public const int HHWIN_PARAM_CUR_TAB = (1 << 13);   // valid curNavType

        // property values used with HH_WINTYPE struct
        public const int HHWIN_PROP_TAB_AUTOHIDESHOW = (1 << 0);    // Automatically hide/show tri-pane window
        public const int HHWIN_PROP_ONTOP = (1 << 1);    // Topmost window
        public const int HHWIN_PROP_NOTITLEBAR = (1 << 2);    // no title bar
        public const int HHWIN_PROP_NODEF_STYLES = (1 << 3);    // no default window styles (only HH_WINTYPE.dwStyles)
        public const int HHWIN_PROP_NODEF_EXSTYLES = (1 << 4);    // no default extended window styles (only HH_WINTYPE.dwExStyles)
        public const int HHWIN_PROP_TRI_PANE = (1 << 5);    // use a tri-pane window
        public const int HHWIN_PROP_NOTB_TEXT = (1 << 6);    // no text on toolbar buttons
        public const int HHWIN_PROP_POST_QUIT = (1 << 7);    // post WM_QUIT message when window closes
        public const int HHWIN_PROP_AUTO_SYNC = (1 << 8);    // automatically ssync contents and index
        public const int HHWIN_PROP_TRACKING = (1 << 9);    // send tracking notification messages
        public const int HHWIN_PROP_TAB_SEARCH = (1 << 10);   // include search tab in navigation pane
        public const int HHWIN_PROP_TAB_HISTORY = (1 << 11);   // include history tab in navigation pane
        public const int HHWIN_PROP_TAB_FAVORITES = (1 << 12);   // include favorites tab in navigation pane
        public const int HHWIN_PROP_CHANGE_TITLE = (1 << 13);   // Put current HTML title in title bar
        public const int HHWIN_PROP_NAV_ONLY_WIN = (1 << 14);   // Only display the navigation window
        public const int HHWIN_PROP_NO_TOOLBAR = (1 << 15);   // Don't display a toolbar
        public const int HHWIN_PROP_MENU = (1 << 16);   // Menu
        public const int HHWIN_PROP_TAB_ADVSEARCH = (1 << 17);   // Advanced FTS UI.
        public const int HHWIN_PROP_USER_POS = (1 << 18);   // After initial creation, user controls window size/position
        public const int HHWIN_PROP_TAB_CUSTOM1 = (1 << 19);   // Use custom tab #1
        public const int HHWIN_PROP_TAB_CUSTOM2 = (1 << 20);   // Use custom tab #2
        public const int HHWIN_PROP_TAB_CUSTOM3 = (1 << 21);   // Use custom tab #3
        public const int HHWIN_PROP_TAB_CUSTOM4 = (1 << 22);   // Use custom tab #4
        public const int HHWIN_PROP_TAB_CUSTOM5 = (1 << 23);   // Use custom tab #5
        public const int HHWIN_PROP_TAB_CUSTOM6 = (1 << 24);   // Use custom tab #6
        public const int HHWIN_PROP_TAB_CUSTOM7 = (1 << 25);   // Use custom tab #7
        public const int HHWIN_PROP_TAB_CUSTOM8 = (1 << 26);   // Use custom tab #8
        public const int HHWIN_PROP_TAB_CUSTOM9 = (1 << 27);   // Use custom tab #9
        public const int HHWIN_TB_MARGIN = (1 << 28);   // the window type has a margin


        public const int HHWIN_BUTTON_EXPAND = (1 << 1);    // Expand/contract button
        public const int HHWIN_BUTTON_BACK = (1 << 2);    // Back button
        public const int HHWIN_BUTTON_FORWARD = (1 << 3);    // Forward button
        public const int HHWIN_BUTTON_STOP = (1 << 4);    // Stop button
        public const int HHWIN_BUTTON_REFRESH = (1 << 5);    // Refresh button
        public const int HHWIN_BUTTON_HOME = (1 << 6);    // Home button

        public const int HHWIN_BUTTON_SYNC = (1 << 11);   // Sync button
        public const int HHWIN_BUTTON_OPTIONS = (1 << 12);   // Options button
        public const int HHWIN_BUTTON_PRINT = (1 << 13);   // Print button

        public const int HHWIN_BUTTON_JUMP1 = (1 << 18);
        public const int HHWIN_BUTTON_JUMP2 = (1 << 19);
        public const int HHWIN_BUTTON_ZOOM = (1 << 20);
        public const int HHWIN_BUTTON_TOC_NEXT = (1 << 21);
        public const int HHWIN_BUTTON_TOC_PREV = (1 << 22);

        public const int HHWIN_DEF_BUTTONS = HHWIN_BUTTON_EXPAND | HHWIN_BUTTON_BACK |
                                             HHWIN_BUTTON_OPTIONS | HHWIN_BUTTON_PRINT;



        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct Point {
            public int x;
            public int y;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect {
            public int left;
            public int top;
            public int right;
            public int bottom;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct HH_WINTYPE {
            public int cbStruct;        // IN: size of this structure including all Information Types

            [MarshalAs(UnmanagedType.Bool)]
            public bool fUniCodeStrings; // IN/OUT: TRUE if all strings are in Unicode

            [MarshalAs(UnmanagedType.LPStr)]
            public String pszType;         // IN/OUT: Name of a type of window
            public uint fsValidMembers;  // IN: Bit flag of valid members (HHWIN_PARAM_)
            public uint fsWinProperties; // IN/OUT: Properties/attributes of the window (HHWIN_)
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszCaption;      // IN/OUT: Window title
            public uint dwStyles;        // IN/OUT: Window styles
            public uint dwExStyles;      // IN/OUT: Extended Window styles
            public Rect rcWindowPos;     // IN: Starting position, OUT: current position
            public int nShowState;      // IN: show state (for example, SW_SHOW)

            public int hwndHelp;          // OUT: window handle
            public int hwndCaller;        // OUT: who called this window

            //            HH_INFOTYPE* paInfoTypes;  // IN: Pointer to an array of Information Types
            public int paInfoTypes; // WARNING: this array is not marshalled!

            // The following members are only valid if HHWIN_PROP_TRI_PANE is set:

            public int hwndToolBar;      // OUT: toolbar window in tri-pane window
            public int hwndNavigation;   // OUT: navigation window in tri-pane window
            public int hwndHTML;         // OUT: window displaying HTML in tri-pane window
            public int iNavWidth;        // IN/OUT: width of navigation window
            public Rect rcHTML;           // OUT: HTML window coordinates

            [MarshalAs(UnmanagedType.LPStr)]
            public String pszToc;         // IN: Location of the table of contents file
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszIndex;       // IN: Location of the index file
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszFile;        // IN: Default location of the html file
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszHome;        // IN/OUT: html file to display when Home button is clicked
            public uint fsToolBarFlags; // IN: flags controlling the appearance of the toolbar
            [MarshalAs(UnmanagedType.Bool)]
            public bool fNotExpanded;   // IN: TRUE/FALSE to contract or expand, OUT: current state
            public int curNavType;     // IN/OUT: UI to display in the navigational pane
            public int tabpos;         // IN/OUT: HHWIN_NAVTAB_TOP, HHWIN_NAVTAB_LEFT, or HHWIN_NAVTAB_BOTTOM
            public int idNotify;       // IN: ID to use for WM_NOTIFY messages

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HH_MAX_TABS + 1, ArraySubType = UnmanagedType.U1)]
            public byte[] tabOrder;    // IN/OUT: tab order: Contents, Index, Search, History, Favorites, Reserved 1-5, Custom tabs

            public int cHistory;       // IN/OUT: number of history items to keep (default is 30)
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszJump1;       // Text for HHWIN_BUTTON_JUMP1
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszJump2;       // Text for HHWIN_BUTTON_JUMP2
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszUrlJump1;    // URL for HHWIN_BUTTON_JUMP1
            [MarshalAs(UnmanagedType.LPStr)]
            public String pszUrlJump2;    // URL for HHWIN_BUTTON_JUMP2
            public Rect rcMinSize;      // Minimum size for window (ignored in version 1)
            public int cbInfoTypes;    // size of paInfoTypes;

            // WARNING: this undocumented field is not marshalled
            public int pszCustomTabs;  // multiple zero-terminated strings
        };


        // Function calls

        // internal interop helpers
        public static int HtmlHelp_DisplayTopic(
            int caller,
            String file) {
            return HtmlHelp(caller, file, HH_DISPLAY_TOPIC, 0);
        }

        // This helper is for getting a ptr to an HH_WINTYPE struct OUT as the dwData parameter. This is
        // used with the HH_GET_WIN_TYPE command.
        [DllImport("hhctrl.ocx", CharSet = CharSet.Unicode, EntryPoint = "HtmlHelpW")]
        protected static extern int HtmlHelp_IntPtr_Helper(
            int caller,
            String file,
            uint command,
            ref IntPtr ps
            );

        // This overload is for performing the HH_SET_WIN_TYPE command, which passes an 
        // HH_WINTYPE value IN as the dwData parameter.
        [DllImport("hhctrl.ocx", CharSet = CharSet.Unicode, EntryPoint = "HtmlHelpW")]
        protected static extern int HtmlHelp_SetWinType_Helper(
            int caller,
            String file,
            uint command,
            ref HH_WINTYPE wintype
            );


        // This overload is for passing a single uint value as the dwData parameter.
        [DllImport("hhctrl.ocx", CharSet = CharSet.Unicode, EntryPoint = "HtmlHelpW")]
        protected static extern int HtmlHelp(
            int caller,
            String file,
            uint command,
            uint data
            );

        // public entrypoints

        // This overload is for performing the HH_SET_WIN_TYPE command, which passes an 
        // HH_WINTYPE value IN as the dwData parameter.
        public static int HtmlHelp_SetWinType(
            int caller,
            String file,
            ref HH_WINTYPE wintype
            ) {
            wintype.cbStruct = Marshal.SizeOf(wintype);
            wintype.fUniCodeStrings = false; // NOTE: this should be set to zero for proper 2-way marshalling

            return HtmlHelp_SetWinType_Helper(
                caller,
                file,
                HH_SET_WIN_TYPE,
                ref wintype);
        }

        public static int HtmlHelp_GetWinType(
            int caller,
            String file,
            ref HH_WINTYPE wintype) {
            IntPtr pwt = new IntPtr(0);

            int retval = HtmlHelp_IntPtr_Helper(
                caller,
                file,
                HH_GET_WIN_TYPE,
                ref pwt);

            // otherwise, let's try to marshal it

            wintype = (HH_WINTYPE)Marshal.PtrToStructure(pwt, typeof(HH_WINTYPE));

            return retval;
        }
        */

        #endregion

    }
}
