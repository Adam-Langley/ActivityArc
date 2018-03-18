using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace ActivityArc
{
    [Register("ActivityArc")]
    public class ActivityArcView : UIView
    {
        const string ANIMATION_NAME_STROKE = "STROKE";
        const string ANIMATION_NAME_ROTATION = "ROTATION";
        const string ANIMATION_NAME_COLOR = "COLOR";

        CAShapeLayer _shadowLayer;
        CAShapeLayer _ringLayer;
        CATextLayer _textLayer;

        CABasicAnimation _rotationAnimation;
        CABasicAnimation _strokeAnimation;
        CABasicAnimation _colorAnimation;

        bool _isIndeterminate = true;

        private bool _hideZeroPercent;
        private bool HideZeroPercent
        {
            get
            {
                return _hideZeroPercent;
            }
            set
            {
                if (value != _hideZeroPercent)
                {
                    _hideZeroPercent = value;
                    UpdateLayers();
                }
            }
        }

        private UIColor _shadowStrokeColor;
        public UIColor ShadowStrokeColor
        {
            get
            {
                return _shadowStrokeColor;
            }
            set
            {
                _shadowStrokeColor = value;
                _shadowLayer.StrokeColor = _shadowStrokeColor.CGColor;
            }
        }

        private UIColor _ringStrokeColor;
        public UIColor RingStrokeColor
        {
            get
            {
                return _ringStrokeColor;
            }
            set
            {
                _ringStrokeColor = value;
                _ringLayer.StrokeColor = _ringStrokeColor.CGColor;
            }
        }

        private nfloat _progressValue;
        public nfloat ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                var newProgressValue = (nfloat)Math.Round(value, 2);
                if (newProgressValue != _progressValue)
                {
                    _progressValue = newProgressValue;
                    UpdateLayers();
                }
            }
        }

        public bool IsIndeterminate
        {
            get
            {
                return _isIndeterminate;
            }
            set
            {
                if (value != _isIndeterminate)
                {
                    _isIndeterminate = value;
                    UpdateLayers();
                }
            }
        }

        private void SetupStrokeAnimation()
        {
            _strokeAnimation = _strokeAnimation ?? CABasicAnimation.FromKeyPath("strokeEnd");

            var presentationLayer = _ringLayer.PresentationLayer as CAShapeLayer ?? _ringLayer;
            if (presentationLayer.StrokeEnd > 0.75f)
            {
                _strokeAnimation.From = NSNumber.FromNFloat(presentationLayer.StrokeEnd);//NSNumber.FromNFloat(0.1f);
                _strokeAnimation.To = NSNumber.FromNFloat(0.1f);
            }
            else if (presentationLayer.StrokeEnd > 0.1f)
            {
                _strokeAnimation.From = NSNumber.FromNFloat(presentationLayer.StrokeEnd);//NSNumber.FromNFloat(0.75f);
                _strokeAnimation.To = NSNumber.FromNFloat(0.1f);
            }
            else
            {
                _strokeAnimation.From = NSNumber.FromNFloat(presentationLayer.StrokeEnd);//NSNumber.FromNFloat(0.1f);
                _strokeAnimation.To = NSNumber.FromNFloat(0.75f);
            }

            _strokeAnimation.Duration = _rotationAnimation.Duration * 1.5f;
            _strokeAnimation.Cumulative = true;
            _strokeAnimation.FillMode = CAFillMode.Forwards;
            _strokeAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
            _strokeAnimation.RemovedOnCompletion = false;
        }

        private void SetupRotationAnimation()
        {
            _rotationAnimation = _rotationAnimation ?? CABasicAnimation.FromKeyPath("transform.rotation.z");
            _rotationAnimation.To = NSNumber.FromFloat((float)Math.PI * 2);
            _rotationAnimation.Duration = 0.78f;
            _rotationAnimation.Cumulative = true;
            _rotationAnimation.FillMode = CAFillMode.Forwards;
            _rotationAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
            _rotationAnimation.RemovedOnCompletion = false;
        }

        private void SetupColorAnimation()
        {
            _colorAnimation = _colorAnimation ?? CABasicAnimation.FromKeyPath("strokeColor");

            _colorAnimation.SetFrom(_shadowLayer.StrokeColor);

            _colorAnimation.SetTo(UIColor.FromRGBA(_shadowStrokeColor.CGColor.Components[0], _shadowStrokeColor.CGColor.Components[1], _shadowStrokeColor.CGColor.Components[2], 0.2f).CGColor);
            _colorAnimation.Duration = 1f;
            _colorAnimation.FillMode = CAFillMode.Forwards;
            _colorAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
            _colorAnimation.RemovedOnCompletion = false;
            _colorAnimation.AutoReverses = true;
        }

        public ActivityArcView(IntPtr ptr)
            : base(ptr)
        {
            _shadowStrokeColor = UIColor.FromRGB(0xe0, 0xe0, 0xe0);
            _ringStrokeColor = UIColor.Black;

            _ringLayer = new CAShapeLayer()
            {
                StrokeColor = _ringStrokeColor.CGColor
            };
            _shadowLayer = new CAShapeLayer()
            {
                StrokeColor = _shadowStrokeColor.CGColor,
                FillColor = _ringLayer.FillColor = null,
                StrokeStart = _ringLayer.StrokeStart = 0f,
                LineWidth = _ringLayer.LineWidth = 4f,
                LineCap = _ringLayer.LineCap = CAShapeLayer.CapRound,
            };
            _textLayer = new CATextLayer()
            {
                Hidden = true,
                TextAlignmentMode = CATextLayerAlignmentMode.Center,
                ForegroundColor = UIColor.DarkTextColor.CGColor
            };
            _textLayer.ContentsScale
                = _textLayer.RasterizationScale
                = _shadowLayer.RasterizationScale
                = _ringLayer.RasterizationScale = UIScreen.MainScreen.Scale;

            Layer.AddSublayer(_shadowLayer);
            Layer.AddSublayer(_ringLayer);
            Layer.AddSublayer(_textLayer);

            SetupRotationAnimation();
            SetupStrokeAnimation();

            _strokeAnimation.AnimationStopped += (x, y) =>
            {
                var presentationLayer = _ringLayer.PresentationLayer as CAShapeLayer ?? _ringLayer;

                SetupStrokeAnimation();

                if (IsIndeterminate)
                {
                    _ringLayer.AddAnimation(_strokeAnimation, ANIMATION_NAME_STROKE);
                }
                else if (null != _ringLayer.AnimationForKey(ANIMATION_NAME_STROKE))
                {
                    _strokeAnimation.To = NSNumber.FromNFloat(this.ProgressValue);
                    _strokeAnimation.Duration = _rotationAnimation.Duration * 1.5f;
                    _strokeAnimation.RemovedOnCompletion = true;
                    _ringLayer.AddAnimation(_strokeAnimation, ANIMATION_NAME_STROKE);
                }
            };

            _rotationAnimation.AnimationStopped += (x, y) =>
            {
                SetupRotationAnimation();

                if (IsIndeterminate)
                {
                    SetupRotationAnimation();
                    _ringLayer.AddAnimation(_rotationAnimation, ANIMATION_NAME_ROTATION);
                }
                else if (null != _ringLayer.AnimationForKey(ANIMATION_NAME_ROTATION))
                {
                    _rotationAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
                    _rotationAnimation.RemovedOnCompletion = true;
                    _ringLayer.AddAnimation(_rotationAnimation, ANIMATION_NAME_ROTATION);
                }
            };

            SetupColorAnimation();

            _colorAnimation.AnimationStopped += (x, y) =>
            {
                if (IsIndeterminate)
                    _shadowLayer.RemoveAnimation(ANIMATION_NAME_COLOR);
                else
                    _shadowLayer.AddAnimation(_colorAnimation, ANIMATION_NAME_COLOR);
            };
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _textLayer.Frame = _shadowLayer.Frame = _ringLayer.Frame = Bounds;

            // get the minimum bounds
            nfloat minimumPlane = (float)Math.Min(Frame.Width, Frame.Height);

            _textLayer.FontSize = Bounds.Height / 2.75f * 0.75f;

            minimumPlane -= _ringLayer.LineWidth;
            var boundingRect = new CGRect(Bounds.Width / 2 - minimumPlane / 2, Bounds.Height / 2 - minimumPlane / 2, minimumPlane, minimumPlane);
            var path = new UIBezierPath();
            path.AddArc(new CGPoint(Bounds.Width / 2, Bounds.Height / 2), minimumPlane / 2, 1.5f * (float)Math.PI, 3.5f * (float)Math.PI, true);

            _ringLayer.Path = path.CGPath;

            _shadowLayer.Path = path.CGPath;

            UpdateLayers();
        }

        public void UpdateLayers()
        {
            UIView step = this;
            while (null != step)
            {
                if (step.Hidden || step.Alpha == 0)
                {
                    CATransaction.DisableActions = true;
                    break;
                }
                else
                    step = step.Superview;
            }

            UIFont textFont = UIFont.SystemFontOfSize(_textLayer.FontSize);

            _textLayer.SetFont(textFont.Name);
            _textLayer.String = $"{_progressValue * 100}%";
            var offset = (Bounds.Height - textFont.LineHeight) / 2;
            _textLayer.Frame = new CGRect(0, offset, Bounds.Width, Bounds.Height);

            if (IsIndeterminate)
            {
                _textLayer.Hidden = true;

                if (null == _ringLayer.AnimationForKey(ANIMATION_NAME_ROTATION))
                {
                    SetupRotationAnimation();

                    _rotationAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseIn);
                    _ringLayer.AddAnimation(_rotationAnimation, ANIMATION_NAME_ROTATION);
                }

                if (null == _ringLayer.AnimationForKey(ANIMATION_NAME_STROKE))
                {
                    SetupStrokeAnimation();

                    _ringLayer.AddAnimation(_strokeAnimation, ANIMATION_NAME_STROKE);
                }
            }
            else
            {
                HideZeroPercent = true;
                _textLayer.Hidden = HideZeroPercent && _progressValue == 0;
                _ringLayer.StrokeEnd = ProgressValue;

                if (null == _shadowLayer.AnimationForKey(ANIMATION_NAME_COLOR))
                    _shadowLayer.AddAnimation(_colorAnimation, ANIMATION_NAME_COLOR);
            }

            CATransaction.DisableActions = false;
        }
    }
}
