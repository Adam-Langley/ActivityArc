// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Example
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch activeSwitch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        ActivityArc.ActivityArcView activityArc { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch indeterminateSwitch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISlider progressValue { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (activeSwitch != null) {
                activeSwitch.Dispose ();
                activeSwitch = null;
            }

            if (activityArc != null) {
                activityArc.Dispose ();
                activityArc = null;
            }

            if (indeterminateSwitch != null) {
                indeterminateSwitch.Dispose ();
                indeterminateSwitch = null;
            }

            if (progressValue != null) {
                progressValue.Dispose ();
                progressValue = null;
            }
        }
    }
}