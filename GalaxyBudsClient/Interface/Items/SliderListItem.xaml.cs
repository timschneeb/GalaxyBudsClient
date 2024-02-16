using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Serilog;

namespace GalaxyBudsClient.Interface.Items
{
    public class SliderListItem : UserControl
    {
        public static readonly StyledProperty<String> TextProperty =
            AvaloniaProperty.Register<SliderListItem, String>(nameof(Text));

        public static readonly StyledProperty<String> SubtitleProperty =
            AvaloniaProperty.Register<SliderListItem, String>(nameof(Subtitle));
        
        public static readonly StyledProperty<int> MinimumProperty =
            AvaloniaProperty.Register<SliderListItem, int>(nameof(Minimum), 0);
        
        public static readonly StyledProperty<int> MaximumProperty =
            AvaloniaProperty.Register<SliderListItem, int>(nameof(Maximum), 3);

        public static readonly StyledProperty<Dictionary<int, string>?> SubtitleDictionaryProperty =
            AvaloniaProperty.Register<SliderListItem, Dictionary<int, string>?>(nameof(SubtitleDictionary));
        
        public event EventHandler<int>? Changed;
        
        private readonly Slider _slider;

        public SliderListItem()
        {
            AvaloniaXamlLoader.Load(this);
            
            _slider = this.GetControl<Slider>("Slider");

            _slider.AddHandler(PointerReleasedEvent,
                (sender, args) =>
                {
                    Changed?.Invoke(this, (int) _slider.Value);
                },
                RoutingStrategies.Tunnel);

            _slider.GetObservable(RangeBase.ValueProperty).Subscribe(d =>
            {
                try
                {
                    var subtitle = SubtitleDictionary?[(int) d];
                    if (subtitle != null) Subtitle = subtitle;
                }
                catch (KeyNotFoundException)
                {
                    Log.Warning("SliderListItem: Key not in subtitle dictionary");
                    Subtitle = "";
                }
            });

            this.GetObservable(SubtitleDictionaryProperty).Subscribe(dictionary =>
            {
                var subtitle = dictionary?[(int) _slider.Value];
                if (subtitle != null) Subtitle = subtitle;
            });

            DataContext = this;
        }

        public Dictionary<int, string>? SubtitleDictionary
        {
            get => GetValue(SubtitleDictionaryProperty);
            set => SetValue(SubtitleDictionaryProperty, value);
        }

        public String Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        
        public String Subtitle
        {
            get => GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public int Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        
        public int Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        
        public int Value
        {
            get => (int)_slider.Value;
            set => _slider.Value = value;
        }
    }
}
