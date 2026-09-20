using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GalaxyBudsClient.Interface.Controls;

public class HeadVisualizerControl : Control
{
    public static readonly StyledProperty<double> YawProperty =
        AvaloniaProperty.Register<HeadVisualizerControl, double>(nameof(Yaw));

    public static readonly StyledProperty<double> PitchProperty =
        AvaloniaProperty.Register<HeadVisualizerControl, double>(nameof(Pitch));

    public static readonly StyledProperty<double> RollProperty =
        AvaloniaProperty.Register<HeadVisualizerControl, double>(nameof(Roll));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<HeadVisualizerControl, bool>(nameof(IsActive), true);

    public double Yaw
    {
        get => GetValue(YawProperty);
        set => SetValue(YawProperty, value);
    }

    public double Pitch
    {
        get => GetValue(PitchProperty);
        set => SetValue(PitchProperty, value);
    }

    public double Roll
    {
        get => GetValue(RollProperty);
        set => SetValue(RollProperty, value);
    }

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    static HeadVisualizerControl()
    {
        AffectsRender<HeadVisualizerControl>(YawProperty, PitchProperty, RollProperty, IsActiveProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var cx = bounds.Width / 2.0;
        var cy = bounds.Height / 2.0;
        var radius = Math.Min(cx, cy) - 16.0;

        if (radius <= 10)
            return;

        // Colors
        var baseColor = IsActive ? Color.FromRgb(0, 168, 255) : Color.FromRgb(128, 128, 128);
        var accentColor = IsActive ? Color.FromRgb(0, 225, 255) : Color.FromRgb(160, 160, 160);
        var gridColor = Color.FromArgb(40, baseColor.R, baseColor.G, baseColor.B);
        var glowColor = Color.FromArgb(30, accentColor.R, accentColor.G, accentColor.B);

        var ringPen = new Pen(new SolidColorBrush(gridColor), 2);
        var accentPen = new Pen(new SolidColorBrush(accentColor), 2);
        var dimPen = new Pen(new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), 1);

        // 1. Draw outer background glow & rings
        context.DrawEllipse(new SolidColorBrush(glowColor), null, new Point(cx, cy), radius + 6, radius + 6);
        context.DrawEllipse(null, ringPen, new Point(cx, cy), radius, radius);
        context.DrawEllipse(null, ringPen, new Point(cx, cy), radius * 0.65, radius * 0.65);

        // 2. Draw compass ticks around outer ring
        for (var angle = 0; angle < 360; angle += 30)
        {
            var rad = angle * Math.PI / 180.0;
            var innerR = (angle % 90 == 0) ? radius - 10 : radius - 5;
            var p1 = new Point(cx + innerR * Math.Sin(rad), cy - innerR * Math.Cos(rad));
            var p2 = new Point(cx + radius * Math.Sin(rad), cy - radius * Math.Cos(rad));
            context.DrawLine((angle % 90 == 0) ? accentPen : dimPen, p1, p2);
        }

        // Center crosshair (Front target)
        var crossLen = 6.0;
        context.DrawLine(dimPen, new Point(cx - crossLen, cy), new Point(cx + crossLen, cy));
        context.DrawLine(dimPen, new Point(cx, cy - crossLen), new Point(cx, cy + crossLen));

        // 3. Compute 3D projection of head & earbuds
        // Convert angles to radians
        var yawRad = Yaw * Math.PI / 180.0;
        var pitchRad = Pitch * Math.PI / 180.0;
        var rollRad = Roll * Math.PI / 180.0;

        // Head center moves slightly with pitch (perspective shift)
        var pitchOffset = Math.Sin(pitchRad) * (radius * 0.25);
        var headCenterY = cy - pitchOffset;

        // Sight beam (points toward where head is facing)
        var beamLen = radius * 0.85;
        var beamX = cx + Math.Sin(yawRad) * beamLen;
        var beamY = headCenterY - Math.Cos(yawRad) * Math.Cos(pitchRad) * beamLen;
        var beamPen = new Pen(new SolidColorBrush(Color.FromArgb(140, accentColor.R, accentColor.G, accentColor.B)), 1.5,
            new DashStyle([4, 4], 0));
        context.DrawLine(beamPen, new Point(cx, headCenterY), new Point(beamX, beamY));

        // Head ellipse (affected by roll and perspective)
        using (context.PushTransform(Matrix.CreateRotation(rollRad) * Matrix.CreateTranslation(cx, headCenterY)))
        {
            var headW = radius * 0.45;
            var headH = radius * 0.55;

            // Head silhouette
            var headFill = new SolidColorBrush(Color.FromArgb(40, baseColor.R, baseColor.G, baseColor.B));
            var headPen = new Pen(new SolidColorBrush(baseColor), 2.5);
            context.DrawEllipse(headFill, headPen, new Point(0, 0), headW, headH);

            // Nose / Face indicator (shifts horizontally with yaw)
            var noseShiftX = Math.Sin(yawRad) * (headW * 0.85);
            var noseY = -headH;
            var noseBrush = new SolidColorBrush(accentColor);
            var nosePen = new Pen(noseBrush, 2);
            context.DrawEllipse(noseBrush, nosePen, new Point(noseShiftX, noseY), 4, 4);

            // Left and Right Earbuds
            // As head yaws, ears move in opposite direction on X and change scale
            var earBaseX = headW + 2;
            var earLeftX = -earBaseX * Math.Cos(yawRad);
            var earRightX = earBaseX * Math.Cos(yawRad);
            var earZLeft = Math.Sin(yawRad); // > 0 means left ear is closer

            var earRadius = 6.0;
            var earBrush = new SolidColorBrush(accentColor);

            // Left Earbud
            var leftEarScale = 1.0 + (earZLeft * 0.25);
            context.DrawEllipse(earBrush, null, new Point(earLeftX, 0), earRadius * leftEarScale, (earRadius + 2) * leftEarScale);

            // Right Earbud
            var rightEarScale = 1.0 - (earZLeft * 0.25);
            context.DrawEllipse(earBrush, null, new Point(earRightX, 0), earRadius * rightEarScale, (earRadius + 2) * rightEarScale);
        }
    }
}
