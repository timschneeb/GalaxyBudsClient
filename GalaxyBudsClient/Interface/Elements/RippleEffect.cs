using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;

/*
 * Based on https://github.com/Roflyanochka/AvaloniaRipple
 */

namespace Ripple.RippleEffect
{
    public class RippleEffect : ContentControl
    {
        #region Styled properties

        public static readonly StyledProperty<Brush> RippleFillProperty =
        AvaloniaProperty.Register<RippleEffect, Brush>(nameof(RippleFill));

        public Brush RippleFill
        {
            get { return GetValue(RippleFillProperty); }
            set { SetValue(RippleFillProperty, value); }
        }

        public static readonly StyledProperty<double> RippleOpacityProperty =
       AvaloniaProperty.Register<RippleEffect, double>(nameof(RippleOpacity));

        public double RippleOpacity
        {
            get { return GetValue(RippleOpacityProperty); }
            set { SetValue(RippleOpacityProperty, value); }
        }

        #endregion Styled properties

        private Ellipse _circle;
        private Animation _ripple;
        private IAnimationSetter _toWidth;
        private IAnimationSetter _fromMargin;
        private IAnimationSetter _toMargin;
        private IAnimationSetter _midOpacity;
        private bool _isRunning;

        public RippleEffect()
        {
            AddHandler(PointerPressedEvent, async (s, e) =>
            {
                if (_isRunning)
                {
                    return;
                }
                var pointer = e.GetPosition(this);
                _isRunning = true;
                var maxWidth = Math.Max(Bounds.Width, Bounds.Height) * 2.2D;
                _toWidth.Value = maxWidth;
                _fromMargin.Value = _circle.Margin = new Thickness(pointer.X, pointer.Y, 0, 0);
                _toMargin.Value = new Thickness(pointer.X - maxWidth / 2, pointer.Y - maxWidth / 2, 0, 0);
                _midOpacity.Value = RippleOpacity / 2;     
                
                await _ripple.RunAsync(_circle);

                _isRunning = false;
            });
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
            _circle = e.NameScope.Find<Ellipse>("Circle");

            var style = _circle.Styles[0] as Style;
            _ripple = style?.Animations[0] as Animation;
            _midOpacity = _ripple?.Children[1].Setters[0];
            
            _toWidth = _ripple?.Children[2].Setters[1];
            _fromMargin = _ripple?.Children[0].Setters[0];
            _toMargin = _ripple?.Children[2].Setters[0];

            style?.Animations.Remove(_ripple);
        }
    }
}