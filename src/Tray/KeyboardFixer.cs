using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using YangiAlifbo.Core;
using YangiAlifbo.Office;

namespace YangiAlifbo.Tray
{
    /// <summary>
    /// Klaviatura qobig'i: WH_KEYBOARD_LL dan harflarni AutoEngine ga beradi va uning buyruqlarini SendInput bilan bajaradi.
    /// Xotirada faqat joriy bo'lak (≤ 60 belgi). Hech narsa diskka yozilmaydi va tarmoqqa yuborilmaydi.
    /// O'z yuborgan tugmalariga (Marker) va boshqa dasturlarning sun'iy tugmalariga qaramaydi.
    /// </summary>
    internal sealed class KeyboardFixer : IDisposable
    {
        public readonly AutoEngine Engine = new AutoEngine();
        public Func<bool> CanWork = () => true;   // yoqilgan + litsenziya
        public Action TogglePressed;               // Pause
        public Func<string, bool> IsExcludedApp = p => false;
        /// <summary>Sinov rejimi (HKCU\Software\YangiAlifbo\AvtoTestInjected=1): boshqa dasturlarning sun'iy tugmalari ham qabul qilinadi.</summary>
        public bool AcceptInjected;
        static readonly IntPtr Marker = (IntPtr)0x59414C46; // "YALF" — o'zimiz yuborgan tugmalar

        readonly Native.HookProc kbProc, msProc; // GC yig'ib olmasligi uchun maydonda
        IntPtr kbHook, msHook;
        IntPtr lastWindow;
        bool lastWindowExcluded;
        ushort lastEnderVk, lastEnderScan; uint lastEnderFlags;

        const int VK_BACK = 0x08, VK_TAB = 0x09, VK_RETURN = 0x0D, VK_SHIFT = 0x10, VK_CONTROL = 0x11, VK_MENU = 0x12,
                  VK_PAUSE = 0x13, VK_CAPITAL = 0x14, VK_SPACE = 0x20, VK_LWIN = 0x5B, VK_RWIN = 0x5C;

        public KeyboardFixer()
        {
            kbProc = OnKey;
            msProc = OnMouse;
        }

        public void Start()
        {
            IntPtr mod = Native.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            kbHook = Native.SetWindowsHookEx(Native.WH_KEYBOARD_LL, kbProc, mod, 0);
            msHook = Native.SetWindowsHookEx(Native.WH_MOUSE_LL, msProc, mod, 0);
            Connect.Log("Avto: hook=" + kbHook + " mouse=" + msHook);
            if (kbHook == IntPtr.Zero) throw new InvalidOperationException("Klaviatura ilgagi örnatilmadi (xato " + Marshal.GetLastWin32Error() + ").");
        }

        public void Dispose()
        {
            if (kbHook != IntPtr.Zero) { Native.UnhookWindowsHookEx(kbHook); kbHook = IntPtr.Zero; }
            if (msHook != IntPtr.Zero) { Native.UnhookWindowsHookEx(msHook); msHook = IntPtr.Zero; }
        }

        IntPtr OnMouse(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int m = wParam.ToInt32();
                if (m == Native.WM_LBUTTONDOWN || m == Native.WM_RBUTTONDOWN || m == Native.WM_MBUTTONDOWN) Engine.Reset();
            }
            return Native.CallNextHookEx(msHook, nCode, wParam, lParam);
        }

        static bool Down(int vk) { return Native.GetAsyncKeyState(vk) < 0; }

        IntPtr OnKey(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    int msg = wParam.ToInt32();
                    if (msg == Native.WM_KEYDOWN || msg == Native.WM_SYSKEYDOWN)
                    {
                        var k = (Native.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Native.KBDLLHOOKSTRUCT));
                        bool injected = (k.flags & Native.LLKHF_INJECTED) != 0;
                        bool mine = k.extra == Marker;
                        bool eaten = !mine && (!injected || AcceptInjected) && Handle(k);
                        if (eaten) return (IntPtr)1; // asl tugma yutildi
                    }
                }
            }
            catch (Exception e) { Engine.Reset(); Connect.Log("Avto: " + e.Message); }
            return Native.CallNextHookEx(kbHook, nCode, wParam, lParam);
        }

        /// <summary>true — asl tugma yutiladi (o'rniga biz yubordik).</summary>
        bool Handle(Native.KBDLLHOOKSTRUCT k)
        {
            int vk = (int)k.vkCode;
            if (vk == VK_PAUSE && !Down(VK_CONTROL) && !Down(VK_MENU))
            {
                Engine.Reset();
                if (TogglePressed != null) TogglePressed();
                return true;
            }
            if (vk == VK_SHIFT || vk == VK_CONTROL || vk == VK_MENU || (vk >= 0xA0 && vk <= 0xA5) || vk == VK_CAPITAL || vk == VK_LWIN || vk == VK_RWIN) return false;

            IntPtr fg = Native.GetForegroundWindow();
            uint pid;
            uint tid = Native.GetWindowThreadProcessId(fg, out pid);
            if (fg != lastWindow)
            {
                lastWindow = fg;
                Engine.Reset();
                lastWindowExcluded = ExcludedProcess(pid);
            }
            if (lastWindowExcluded || !CanWork()) { Engine.Reset(); return false; }

            bool ctrl = Down(VK_CONTROL), alt = Down(VK_MENU), win = Down(VK_LWIN) || Down(VK_RWIN);
            bool altGr = ctrl && alt;
            if ((ctrl || alt || win) && !altGr) { Engine.Reset(); return false; } // Ctrl+V, Alt+Tab va h.k.

            if (vk == VK_BACK) return Do(Engine.Backspace(), true);
            if (vk == VK_SPACE || vk == VK_RETURN || vk == VK_TAB)
            {
                Ender e = vk == VK_SPACE ? Ender.Space : vk == VK_RETURN ? Ender.Enter : Ender.Tab;
                if (IsPasswordField(tid)) { Engine.Reset(); return false; }
                lastEnderVk = (ushort)vk; lastEnderScan = (ushort)k.scanCode; lastEnderFlags = k.flags;
                return Do(Engine.End(e), true);
            }

            char c;
            if (TryChar(k, tid, altGr, out c)) Engine.Char(c);
            else Engine.Reset(); // strelkalar, Home/End, Delete, Esc, F1..F12 — kursor joyi noma'lum
            return false;
        }

        bool ExcludedProcess(uint pid)
        {
            try
            {
                if (pid == (uint)Process.GetCurrentProcess().Id) return true;
                using (var p = Process.GetProcessById((int)pid)) return IsExcludedApp(p.ProcessName);
            }
            catch { return true; } // administrator oynasi va h.k. — tegmaymiz
        }

        static bool IsPasswordField(uint tid)
        {
            var gi = new Native.GUITHREADINFO();
            gi.cbSize = Marshal.SizeOf(typeof(Native.GUITHREADINFO));
            if (!Native.GetGUIThreadInfo(tid, ref gi) || gi.hwndFocus == IntPtr.Zero) return false;
            var cls = new StringBuilder(64);
            Native.GetClassName(gi.hwndFocus, cls, cls.Capacity);
            return cls.ToString().IndexOf("edit", StringComparison.OrdinalIgnoreCase) >= 0 &&
                   (Native.GetWindowLong(gi.hwndFocus, Native.GWL_STYLE) & Native.ES_PASSWORD) != 0;
        }

        static bool TryChar(Native.KBDLLHOOKSTRUCT k, uint tid, bool altGr, out char c)
        {
            c = '\0';
            var state = new byte[256];
            if (Down(VK_SHIFT)) state[VK_SHIFT] = 0x80;
            if ((Native.GetKeyState(VK_CAPITAL) & 1) != 0) state[VK_CAPITAL] = 0x01;
            if (altGr) { state[VK_CONTROL] = 0x80; state[VK_MENU] = 0x80; }
            var buf = new StringBuilder(8);
            // flags 4: klaviatura holatini o'zgartirmaslik (Windows 10 1607+), aks holda e'tiborsiz
            int n = Native.ToUnicodeEx(k.vkCode, k.scanCode, state, buf, buf.Capacity, 4, Native.GetKeyboardLayout(tid));
            if (n != 1) return false;
            c = buf[0];
            return !char.IsControl(c);
        }

        bool Do(AutoAction a, bool reinjectEnder)
        {
            if (a.Kind == AutoKind.None) return false;
            var list = new List<Native.INPUT>();
            for (int i = 0; i < a.Backspaces; i++) { list.Add(Vk(VK_BACK, 0, 0)); list.Add(Vk(VK_BACK, 0, Native.KEYEVENTF_KEYUP)); }
            foreach (char ch in a.Text) { list.Add(Uni(ch, 0)); list.Add(Uni(ch, Native.KEYEVENTF_KEYUP)); }
            if (reinjectEnder && lastEnderVk != 0)
                list.Add(Vk(lastEnderVk, lastEnderScan, (lastEnderFlags & Native.LLKHF_EXTENDED) != 0 ? Native.KEYEVENTF_EXTENDEDKEY : 0));
            var arr = list.ToArray();
            uint sent = Native.SendInput((uint)arr.Length, arr, Marshal.SizeOf(typeof(Native.INPUT)));
            if (sent != arr.Length) { Connect.Log("Avto: SendInput " + sent + "/" + arr.Length); Engine.Reset(); return false; }
            return true;
        }

        static Native.INPUT Vk(int vk, ushort scan, uint flags)
        {
            var i = new Native.INPUT { type = Native.INPUT_KEYBOARD };
            i.u.ki = new Native.KEYBDINPUT { wVk = (ushort)vk, wScan = scan, dwFlags = flags, dwExtraInfo = Marker };
            return i;
        }

        static Native.INPUT Uni(char ch, uint flags)
        {
            var i = new Native.INPUT { type = Native.INPUT_KEYBOARD };
            i.u.ki = new Native.KEYBDINPUT { wVk = 0, wScan = ch, dwFlags = Native.KEYEVENTF_UNICODE | flags, dwExtraInfo = Marker };
            return i;
        }
    }
}
