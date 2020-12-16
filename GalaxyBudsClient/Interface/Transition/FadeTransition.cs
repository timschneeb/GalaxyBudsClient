using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;

namespace GalaxyBudsClient.Interface.Transition
{
    /// <summary>
    /// Defines a cross-fade animation between two <see cref="IVisual"/>s.
    /// </summary>
    public class FadeTransition : IPageTransition
    {
        public EventHandler? FadeOutComplete;
        public EventHandler? FadeInComplete;
        public EventHandler? FadeOutBegin;
        public EventHandler? FadeInBegin;
        
        private readonly Animation _fadeOutAnimation;
        private readonly Animation _fadeInAnimation;

        /// <summary>
        /// Initializes a new instance of the <see cref="FadeTransition"/> class.
        /// </summary>
        public FadeTransition()
        {
            _fadeOutAnimation = new Animation
            {   
                Duration = TimeSpan.Parse("0:0:.25"),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                }
            };
            _fadeInAnimation = new Animation
            {
                Duration = TimeSpan.Parse("0:0:.10"),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                }
            };
        }

        /// <summary>
        /// Gets or sets element entrance easing.
        /// </summary>
        public Easing FadeInEasing
        {
            get => _fadeInAnimation.Easing;
            set => _fadeInAnimation.Easing = value;
        }

        /// <summary>
        /// Gets or sets element exit easing.
        /// </summary>
        public Easing FadeOutEasing
        {
            get => _fadeOutAnimation.Easing;
            set => _fadeOutAnimation.Easing = value;
        }

        /// <summary>
        /// Starts the animation.
        /// </summary>
        /// <param name="from">
        /// The control that is being transitioned away from. May be null.
        /// </param>
        /// <param name="to">
        /// The control that is being transitioned to. May be null.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that tracks the progress of the animation.
        /// </returns>
        public async Task Start(Visual? from, Visual? to)
        {
            if (to != null)
            {
                to.Opacity = 0;
            }
            
            if (from != null)
            {
                FadeOutBegin?.Invoke(this, EventArgs.Empty);
                await _fadeOutAnimation.RunAsync(from);
                FadeOutComplete?.Invoke(this, EventArgs.Empty);
            }
            
            if (from != null)
            {
                from.IsVisible = false;
            }

            await Task.Delay(100);
            
            if (to != null)
            {
                to.IsVisible = true;
                FadeInBegin?.Invoke(this, EventArgs.Empty);
                await _fadeInAnimation.RunAsync(to);
                FadeInComplete?.Invoke(this, EventArgs.Empty);
            }
            
            if (to != null)
            {
                to.Opacity = 1;
            }
        }

        /// <summary>
        /// Starts the animation.
        /// </summary>
        /// <param name="from">
        /// The control that is being transitioned away from. May be null.
        /// </param>
        /// <param name="to">
        /// The control that is being transitioned to. May be null.
        /// </param>
        /// <param name="forward">
        /// Unused for cross-fades.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that tracks the progress of the animation.
        /// </returns>
        Task IPageTransition.Start(Visual? from, Visual? to, bool forward)
        {
            return Start(from, to);
        }
    }
}