﻿using Android.App;
using Android.Runtime;
using Plugin.CurrentActivity;
using System;

public class MainApplication : Application
{
    public MainApplication(IntPtr handle, JniHandleOwnership transer)
        : base(handle, transer)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
    }
}