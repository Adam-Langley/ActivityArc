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
        const string ANIMATION_STROKE = "STROKE";
        const string ANIMATION_ROTATION = "ROTATION";
        const string ANIMATION_COLOR = "COLOR";

        const string ANIMATION_GROW = "GROW";
        const string ANIMATION_SHRINK = "SHRINK";

        const string ANIMATION_OPACITYUP = "OPACITYUP";
        const string ANIMATION_OPACITYDOWN = "OPACITYDOWN";

        CAShapeLayer _shadowLayer;
        CAShapeLayer _arcLayer;
        CATextLayer _textLayer;

        CABasicAnimation _rotationAnimation;
        CABasicAnimation _strokeAnimation;
        CABasicAnimation _colorAnimation;
        CABasicAnimation _pathGrowAnimation;
        CABasicAnimation _pathShrinkAnimation;
        CABasicAnimation _pathOpacityUpAnimation;
        CABasicAnimation _pathOpacityDownAnimation;

        bool _isIndeterminate = true;

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


        private bool _isActive = true;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    UpdateLayers();
                }
            }
        }

        private UIColor _arcStrokeColor;
        public UIColor ArcStrokeColor
        {
            get
            {
                return _arcStrokeColor;
            }
            set
            {
                _arcStrokeColor = value;
                _arcLayer.StrokeColor = _arcStrokeColor.CGColor;
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
            if (null == _strokeAnimation)
            {
                _strokeAnimation = CABasicAnimation.FromKeyPath("strokeEnd");
                _strokeAnimation.AnimationStopped += (x, y) =>
                {
                    if (_strokeAnimation.RemovedOnCompletion)
                    {
                        // Disable animation because the CALayer will attempt to animate
                        // the stroke from the PREVIOUS StrokeEnd value (not the currently animated value).
                        CATransaction.DisableActions = true;
                        _arcLayer.StrokeEnd = this.ProgressValue;
                        CATransaction.DisableActions = false;
                    }
                    else
                    {
                        // Reinitialize animation parameters
                        SetupStrokeAnimation();

                        if (IsIndeterminate)
                        {
                            _arcLayer.AddAnimation(_strokeAnimation, ANIMATION_STROKE);
                        }
                        else if (null != _arcLayer.AnimationForKey(ANIMATION_STROKE))
                        {
                            // If the animation is currently running, let's phase it out.
                            // Ending at the current 'Progress Value'. 
                            _strokeAnimation.To = NSNumber.FromNFloat(this.ProgressValue);
                            _strokeAnimation.Duration = _rotationAnimation.Duration * 1.5f;
                            // ...then have it removed so it no longer recurs.
                            _strokeAnimation.RemovedOnCompletion = true;
                            // Queue it up for one final animation...
                            _arcLayer.AddAnimation(_strokeAnimation, ANIMATION_STROKE);
                        }
                    }
                };
            }


            var presentationLayer = _arcLayer.PresentationLayer as CAShapeLayer ?? _arcLayer;
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
            if (null == _rotationAnimation)
            {
                _rotationAnimation = CABasicAnimation.FromKeyPath("transform.rotation.z");
                _rotationAnimation.AnimationStopped += (x, y) =>
                {
                    // Reinitialize any animation settings.
                    SetupRotationAnimation();

                    if (IsIndeterminate)
                    {
                        // Re-queue the animation to keep the loop going.
                        _arcLayer.AddAnimation(_rotationAnimation, ANIMATION_ROTATION);
                    }
                    else if (null != _arcLayer.AnimationForKey(ANIMATION_ROTATION))
                    {
                        // The rotation animation needs to be stopped, we we'll ease it out...
                        _rotationAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
                        // ...then have it removed so it no longer recurs.
                        _rotationAnimation.RemovedOnCompletion = true;
                        // Queue it up for one final rotation...
                        _arcLayer.AddAnimation(_rotationAnimation, ANIMATION_ROTATION);
                    }
                };
            }

            _rotationAnimation.To = NSNumber.FromFloat((float)Math.PI * 2);
            _rotationAnimation.Duration = 0.78f;
            _rotationAnimation.Cumulative = true;
            _rotationAnimation.FillMode = CAFillMode.Forwards;
            _rotationAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
            // We don't want to remove the animation when completed, because that would cause the arc to snap back
            // to its original rotation value.
            _rotationAnimation.RemovedOnCompletion = false;
            // We're not going to automatically repeat, because we want the opportunity to modify the transform
            // on its final spin. Animations cannot be altered once they have been started.
            _rotationAnimation.RepeatCount = 0;
        }

        private void SetupColorAnimation()
        {
            if (null == _colorAnimation)
            {
                _colorAnimation = CABasicAnimation.FromKeyPath("strokeColor");
                _colorAnimation.AnimationStopped += (x, y) =>
                {
                    if (IsIndeterminate)
                        _shadowLayer.RemoveAnimation(ANIMATION_COLOR);
                    else
                        _shadowLayer.AddAnimation(_colorAnimation, ANIMATION_COLOR);
                };
            }

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
            _arcStrokeColor = UIColor.Black;

            _arcLayer = new CAShapeLayer()
            {
                StrokeColor = _arcStrokeColor.CGColor
            };
            _shadowLayer = new CAShapeLayer()
            {
                StrokeColor = _shadowStrokeColor.CGColor,
                FillColor = _arcLayer.FillColor = null,
                StrokeStart = _arcLayer.StrokeStart = 0f,
                LineWidth = _arcLayer.LineWidth = 4f,
                LineCap = _arcLayer.LineCap = CAShapeLayer.CapRound,
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
                = _arcLayer.RasterizationScale = UIScreen.MainScreen.Scale;

            Layer.AddSublayer(_shadowLayer);
            Layer.AddSublayer(_arcLayer);
            Layer.AddSublayer(_textLayer);

            SetupRotationAnimation();
            SetupStrokeAnimation();
            SetupColorAnimation();

            var path = CreateArcPath(5f);
            _pathGrowAnimation = CABasicAnimation.FromKeyPath("path");
            _pathGrowAnimation.Duration = 0.5f;
            _pathGrowAnimation.RemovedOnCompletion = false;
            _pathGrowAnimation.FillMode = CAFillMode.Forwards;
            _pathGrowAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
            _pathGrowAnimation.AnimationStopped += (x, y) =>
            {
                _shadowLayer.Path = _pathGrowAnimation.GetToAs<CGPath>();
                UpdateLayers();
            };

            _pathShrinkAnimation = CABasicAnimation.FromKeyPath("path");
            _pathShrinkAnimation.Duration = 0.5f;
            _pathShrinkAnimation.RemovedOnCompletion = false;
            _pathShrinkAnimation.FillMode = CAFillMode.Forwards;
            _pathShrinkAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseIn);
            _pathShrinkAnimation.AnimationStopped += (x, y) =>
            {
                _shadowLayer.Path = _pathShrinkAnimation.GetToAs<CGPath>();
                UpdateLayers();
            };


            _pathOpacityUpAnimation = CABasicAnimation.FromKeyPath("opacity");
            _pathOpacityUpAnimation.From = NSNumber.FromNFloat(0f);
            _pathOpacityUpAnimation.To = NSNumber.FromNFloat(1f);
            _pathOpacityUpAnimation.Duration = 0.5f;
            _pathOpacityUpAnimation.FillMode = CAFillMode.Forwards;
            _pathOpacityUpAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
            _pathOpacityUpAnimation.RemovedOnCompletion = false;

            _pathOpacityDownAnimation = CABasicAnimation.FromKeyPath("opacity");
            _pathOpacityDownAnimation.From = NSNumber.FromNFloat(1f);
            _pathOpacityDownAnimation.To = NSNumber.FromNFloat(0f);
            _pathOpacityDownAnimation.Duration = 0.5f;
            _pathOpacityDownAnimation.FillMode = CAFillMode.Forwards;
            _pathOpacityDownAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseIn);
            _pathOpacityDownAnimation.RemovedOnCompletion = false;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _textLayer.Frame = _shadowLayer.Frame = _arcLayer.Frame = Bounds;

            // get the minimum bounds
            nfloat minimumPlane = (float)Math.Min(Frame.Width, Frame.Height);
            _textLayer.FontSize = Bounds.Height / 2.75f * 0.75f;
            _arcLayer.Path = _shadowLayer.Path = CreateArcPath(minimumPlane - _arcLayer.LineWidth);

            UpdateLayers();
        }

        private CGPath CreateArcPath(nfloat diameter)
        {
            _textLayer.FontSize = Bounds.Height / 2.75f * 0.75f;

            var boundingRect = new CGRect(Bounds.Width / 2 - diameter / 2, Bounds.Height / 2 - diameter / 2, diameter, diameter);
            var path = new UIBezierPath();
            path.AddArc(new CGPoint(Bounds.Width / 2, Bounds.Height / 2), diameter / 2, 1.5f * (float)Math.PI, 3.5f * (float)Math.PI, true);

            return path.CGPath;    
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
            nfloat minimumPlane = (float)Math.Min(Frame.Width, Frame.Height);

            _textLayer.FontSize = Bounds.Height / 2.75f * 0.75f;
            _textLayer.SetFont(textFont.Name);
            _textLayer.String = $"{_progressValue * 100}%";
            var offset = (Bounds.Height - textFont.LineHeight) / 2;
            _textLayer.Frame = new CGRect(0, offset, Bounds.Width, Bounds.Height);

            _textLayer.Hidden = IsIndeterminate || !IsActive;

            if (IsActive)
            {
                _shadowLayer.RemoveAnimation(ANIMATION_SHRINK);
                _arcLayer.RemoveAnimation(ANIMATION_SHRINK);
                _shadowLayer.RemoveAnimation(ANIMATION_OPACITYDOWN);
                _arcLayer.RemoveAnimation(ANIMATION_OPACITYDOWN);

                if (null == _shadowLayer.AnimationForKey(ANIMATION_GROW))
                {
                    // grow arc to full size
                    _pathGrowAnimation.SetFrom(_shadowLayer.Path);
                    _pathGrowAnimation.SetTo(CreateArcPath(minimumPlane));
                    _shadowLayer.AddAnimation(_pathGrowAnimation, ANIMATION_GROW);
                    _arcLayer.AddAnimation(_pathGrowAnimation, ANIMATION_GROW);
                    _arcLayer.AddAnimation(_pathOpacityUpAnimation, ANIMATION_OPACITYUP);
                    _shadowLayer.AddAnimation(_pathOpacityUpAnimation, ANIMATION_OPACITYDOWN);
                }

                if (IsIndeterminate)
                {
                    _textLayer.Hidden = true;

                    if (null == _arcLayer.AnimationForKey(ANIMATION_ROTATION))
                    {
                        SetupRotationAnimation();

                        _rotationAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseIn);
                        _arcLayer.AddAnimation(_rotationAnimation, ANIMATION_ROTATION);
                    }

                    if (null == _arcLayer.AnimationForKey(ANIMATION_STROKE))
                    {
                        SetupStrokeAnimation();

                        _arcLayer.AddAnimation(_strokeAnimation, ANIMATION_STROKE);
                    }
                }
                else
                {
                    // Don't force a change in the stroke if it's currently being animated.
                    // It will get set to the correct value when the animation completes anyway.
                    if (null == _arcLayer.AnimationForKey(ANIMATION_STROKE))
                        _arcLayer.StrokeEnd = ProgressValue;

                    if (null == _shadowLayer.AnimationForKey(ANIMATION_COLOR))
                        _shadowLayer.AddAnimation(_colorAnimation, ANIMATION_COLOR);
                }
            }
            else
            {
                _shadowLayer.RemoveAnimation(ANIMATION_GROW);
                _arcLayer.RemoveAnimation(ANIMATION_GROW);
                _shadowLayer.RemoveAnimation(ANIMATION_OPACITYUP);
                _arcLayer.RemoveAnimation(ANIMATION_OPACITYUP);

                if (null == _shadowLayer.AnimationForKey(ANIMATION_SHRINK))
                {
                    // shrink arc away
                    _pathShrinkAnimation.SetFrom(_shadowLayer.Path);
                    _pathShrinkAnimation.SetTo(CreateArcPath(5));
                    _shadowLayer.AddAnimation(_pathShrinkAnimation, ANIMATION_SHRINK);
                    _arcLayer.AddAnimation(_pathShrinkAnimation, ANIMATION_SHRINK);
                    _arcLayer.AddAnimation(_pathOpacityDownAnimation, ANIMATION_OPACITYDOWN);
                    _shadowLayer.AddAnimation(_pathOpacityDownAnimation, ANIMATION_OPACITYDOWN);
                }
            }

            CATransaction.DisableActions = false;
        }
    }
}
