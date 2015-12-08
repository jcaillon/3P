#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (LocalWindowsHook.cs) is part of 3P.
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
#pragma warning disable 618

//
// I took this class from the example at http://msdn.microsoft.com/msdnmag/issues/02/10/cuttingedge
// and made a couple of minor tweaks to it - dpk
//

using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace _3PA.Interop
{
	#region Class HookEventArgs
	public class HookEventArgs : EventArgs
	{
		public int HookCode;	// Hook code
		public IntPtr wParam;	// WPARAM argument
		public IntPtr lParam;	// LPARAM argument
	}
	#endregion

	#region Enum HookType
	// Hook Types
	public enum HookType {
		WH_JOURNALRECORD = 0,
		WH_JOURNALPLAYBACK = 1,
		WH_KEYBOARD = 2,
		WH_GETMESSAGE = 3,
		WH_CALLWNDPROC = 4,
		WH_CBT = 5,
		WH_SYSMSGFILTER = 6,
		WH_MOUSE = 7,
		WH_HARDWARE = 8,
		WH_DEBUG = 9,
		WH_SHELL = 10,
		WH_FOREGROUNDIDLE = 11,
		WH_CALLWNDPROCRET = 12,		
		WH_KEYBOARD_LL = 13,
		WH_MOUSE_LL = 14
	}
	#endregion

	#region Class LocalWindowsHook
	public class LocalWindowsHook
	{
		// ************************************************************************
		// Filter function delegate
		public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);
		// ************************************************************************

		// ************************************************************************
		// Internal properties
		protected IntPtr m_hhook = IntPtr.Zero;
		protected HookProc m_filterFunc;
		protected HookType m_hookType;
		// ************************************************************************
		
		// ************************************************************************
		// Event delegate
		public delegate void HookEventHandler(object sender, HookEventArgs e);
		// ************************************************************************

		// ************************************************************************
		// Event: HookInvoked 
		public event HookEventHandler HookInvoked;
		protected void OnHookInvoked(HookEventArgs e)
		{
			if (HookInvoked != null)
				HookInvoked(this, e);
		}
		// ************************************************************************

		// ************************************************************************
		// Class constructor(s)
		public LocalWindowsHook(HookType hook)
		{
			m_hookType = hook;
			m_filterFunc = CoreHookProc; 
		}
		public LocalWindowsHook(HookType hook, HookProc func)
		{
			m_hookType = hook;
			m_filterFunc = func; 
		}		
		// ************************************************************************
	
		// ************************************************************************
		// Default filter function
		protected int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
		{
			if (code < 0)
				return CallNextHookEx(m_hhook, code, wParam, lParam);

			// Let clients determine what to do
			HookEventArgs e = new HookEventArgs();
			e.HookCode = code;
			e.wParam = wParam;
			e.lParam = lParam;
			OnHookInvoked(e);

			// Yield to the next hook in the chain
			return CallNextHookEx(m_hhook, code, wParam, lParam);
		}
		// ************************************************************************

		// ************************************************************************
		// Install the hook
		public void Install()
		{
			m_hhook = SetWindowsHookEx(
				m_hookType, 
				m_filterFunc, 
				IntPtr.Zero, 
				AppDomain.GetCurrentThreadId());
		}
		// ************************************************************************

		// ************************************************************************
		// Uninstall the hook
		public void Uninstall()
		{
			UnhookWindowsHookEx(m_hhook); 
			m_hhook = IntPtr.Zero;
		}
		// ************************************************************************

		public bool IsInstalled
		{
			get{ return m_hhook != IntPtr.Zero; }
		}

		#region Win32 Imports
		// ************************************************************************
		// Win32: SetWindowsHookEx()
		[DllImport("user32.dll")]
		protected static extern IntPtr SetWindowsHookEx(HookType code, 
			HookProc func,
			IntPtr hInstance,
			int threadID);
		// ************************************************************************

		// ************************************************************************
		// Win32: UnhookWindowsHookEx()
		[DllImport("user32.dll")]
		protected static extern int UnhookWindowsHookEx(IntPtr hhook); 
		// ************************************************************************

		// ************************************************************************
		// Win32: CallNextHookEx()
		[DllImport("user32.dll")]
		protected static extern int CallNextHookEx(IntPtr hhook, 
			int code, IntPtr wParam, IntPtr lParam);
		// ************************************************************************
		#endregion
	}
	#endregion
}
