# Yangi Alifbo v3 — Bloknotda avtomatik sinov (sinov rejimi: AvtoTestInjected=1, keyin o'chiriladi)
param([string]$Exe = "$PSScriptRoot\..\out\YangiAlifbo.exe")
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System; using System.Runtime.InteropServices; using System.Text;
public static class W {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr FindWindowEx(IntPtr p, IntPtr a, string c, string t);
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, StringBuilder l);
 [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, IntPtr l);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, int m, IntPtr w, IntPtr l);
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr LoadKeyboardLayout(string id, uint f);
 [DllImport("user32.dll")] public static extern short VkKeyScanEx(char c, IntPtr hkl);
 [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint c, uint t);
 [DllImport("user32.dll")] static extern uint SendInput(uint n, INPUT[] i, int s);
 [StructLayout(LayoutKind.Sequential)] struct KI { public ushort vk, scan; public uint flags, time; public IntPtr extra; }
 [StructLayout(LayoutKind.Sequential)] struct MI { public int dx, dy; public uint d, f, t; public IntPtr e; }
 [StructLayout(LayoutKind.Explicit)] struct U { [FieldOffset(0)] public MI m; [FieldOffset(0)] public KI k; }
 [StructLayout(LayoutKind.Sequential)] struct INPUT { public int type; public U u; }
 static INPUT K(int vk, bool up){ var i=new INPUT{type=1}; i.u.k=new KI{vk=(ushort)vk, scan=(ushort)MapVirtualKey((uint)vk,0), flags=up?2u:0u}; return i; }
 public static void Key(int vk){ SendInput(2,new[]{K(vk,false),K(vk,true)},Marshal.SizeOf(typeof(INPUT))); }
 public static void Char(char c, IntPtr hkl){ short s=VkKeyScanEx(c,hkl); int vk=s&0xFF; bool sh=(s&0x100)!=0;
   var l=new System.Collections.Generic.List<INPUT>(); if(sh)l.Add(K(0x10,false)); l.Add(K(vk,false)); l.Add(K(vk,true)); if(sh)l.Add(K(0x10,true));
   SendInput((uint)l.Count,l.ToArray(),Marshal.SizeOf(typeof(INPUT))); }
 public static string Text(IntPtr h){ int n=(int)SendMessage(h,0x0E,IntPtr.Zero,IntPtr.Zero); var sb=new StringBuilder(n+1); SendMessage(h,0x0D,(IntPtr)(n+1),sb); return sb.ToString(); }
}
"@
New-Item -Force HKCU:\Software\YangiAlifbo | Out-Null
Set-ItemProperty HKCU:\Software\YangiAlifbo AvtoTestInjected 1 -Type DWord
Set-ItemProperty HKCU:\Software\YangiAlifbo AvtoEnabled 1 -Type DWord
Set-ItemProperty HKCU:\Software\YangiAlifbo AvtoWelcomeShown 1 -Type DWord
Get-Process YangiAlifbo -ErrorAction SilentlyContinue | Stop-Process -Force
$tray = Start-Process $Exe -PassThru
Start-Sleep 2
$np = Start-Process notepad -PassThru
Start-Sleep 2
$np.Refresh()
$edit = [W]::FindWindowEx($np.MainWindowHandle, [IntPtr]::Zero, "Edit", $null)
if ($edit -eq [IntPtr]::Zero) { $edit = [W]::FindWindowEx($np.MainWindowHandle, [IntPtr]::Zero, "RichEditD2DPT", $null) }
$us = [W]::LoadKeyboardLayout("00000409", 1)
[W]::PostMessage($np.MainWindowHandle, 0x50, [IntPtr]::Zero, $us) | Out-Null  # WM_INPUTLANGCHANGEREQUEST -> inglizcha (AQSh)
Start-Sleep -Milliseconds 500
$keys = @{ "{ENTER}" = 0x0D; "{BACKSPACE}" = 0x08 }
[W]::SetForegroundWindow($np.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 500
[W]::Char([char]'x', $us); Start-Sleep -Milliseconds 300   # isitish: Bloknot fokusini tekshirish
$cases = @(
 @("o'zbek shahar ", "özbek şahar "),
 @("bog', tog' va choy ", "boğ, toğ va çoy "),
 @("Windows chat shoh.uz ab12 ", "Windows chat shoh.uz ab12 "),
 @("salom shaxsan{ENTER}", "salom şaxsan`r`n"),
 @("ashot{ENTER}", "ashot`r`n"),
 @("shopa {BACKSPACE}", "shopa ")
)
$ok = 0; $fail = 0
foreach ($c in $cases) {
  [W]::SetForegroundWindow($np.MainWindowHandle) | Out-Null
  Start-Sleep -Milliseconds 300
  [W]::SendMessage($edit, 0x0C, [IntPtr]::Zero, (New-Object System.Text.StringBuilder "")) | Out-Null  # WM_SETTEXT: tozalash
  Start-Sleep -Milliseconds 200
  foreach ($part in [regex]::Split($c[0], '(\{[A-Z]+\})')) {
    if ($part -match '^\{') { [W]::Key($keys[$part]) }
    else { foreach ($ch in $part.ToCharArray()) { [W]::Char($ch, $us); Start-Sleep -Milliseconds 20 } }
    Start-Sleep -Milliseconds 150
  }
  Start-Sleep -Milliseconds 400
  $got = [W]::Text($edit)
  $exp = $ExecutionContext.InvokeCommand.ExpandString($c[1])
  if ($got -ceq $exp) { $ok++; "OK   [$($c[0])] -> [$got]" } else { $fail++; "FAIL [$($c[0])] kutilgan [$exp] olindi [$got]" }
}
Stop-Process $np -Force
Remove-ItemProperty HKCU:\Software\YangiAlifbo AvtoTestInjected
Stop-Process $tray -Force
$wf = "$env:APPDATA\YangiAlifbo\avto_istisno.txt"
if (Test-Path $wf) { (Get-Content $wf -Encoding UTF8) | Where-Object { $_ -ne "shopa" } | Set-Content $wf -Encoding UTF8 }
"OK: $ok, FAIL: $fail"
