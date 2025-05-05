using System;
using System.Runtime.InteropServices;

public static class PopUp
{
    [DllImport("popup", CallingConvention = CallingConvention.Cdecl)]
    public static extern void create_popup(string message);
}