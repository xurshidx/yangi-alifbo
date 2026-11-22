using System;
using System.Runtime.InteropServices;

// Office PIA / extensibility.dll ishlatmaslik uchun interfeyslar shu yerda e'lon qilingan.
namespace YangiAlifbo.Office
{
    // DIQQAT: "custom" parametri SAFEARRAY(VARIANT)* — MarshalAs majburiy. Busiz .NET System.Array ni
    // interfeys ko'rsatkichi deb uzatadi va Office 2007 clr.dll ichida qulaydi (0xc0000005, 2026-11-22).
    // Imzo Microsoft extensibility.dll dagi bilan bir xil.
    [ComImport, Guid("B65AD801-ABAF-11D0-BB8B-00A0C90F2744"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDTExtensibility2
    {
        [DispId(1)] void OnConnection([In, MarshalAs(UnmanagedType.IDispatch)] object application, [In] int connectMode,
                                      [In, MarshalAs(UnmanagedType.IDispatch)] object addInInst,
                                      [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Array custom);
        [DispId(2)] void OnDisconnection([In] int removeMode,
                                         [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Array custom);
        [DispId(3)] void OnAddInsUpdate([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Array custom);
        [DispId(4)] void OnStartupComplete([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Array custom);
        [DispId(5)] void OnBeginShutdown([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Array custom);
    }

    [ComImport, Guid("000C0396-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IRibbonExtensibility
    {
        [DispId(1)] [return: MarshalAs(UnmanagedType.BStr)] string GetCustomUI([In, MarshalAs(UnmanagedType.BStr)] string ribbonId);
    }

    [ComImport, Guid("000C0395-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IRibbonControl
    {
        [DispId(1)] string Id { get; }
        [DispId(2)] object Context { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
        [DispId(3)] string Tag { get; }
    }
}
