using System;

using UIKit;

namespace Example
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            this.activeSwitch.ValueChanged += (x, y) =>
            {
                this.activityArc.IsActive = (x as UISwitch).On;
            };

            this.indeterminateSwitch.ValueChanged += (x, y) =>
            {
                this.activityArc.IsIndeterminate = (x as UISwitch).On;
            };

            this.progressValue.ValueChanged += (x, y) =>
            {
                this.activityArc.ProgressValue = (x as UISlider).Value;
            };
        }


        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
