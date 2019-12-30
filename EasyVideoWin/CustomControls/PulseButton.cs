// ***********************************************************************
// Assembly         : PulseButton
// Author           : Niel Morgan Thomas
// Created          : 05-22-2015
//
// Last Modified By : Niel Morgan Thomas
// Last Modified On : 07-9-2015
// ***********************************************************************
// <copyright file="PulseButton.cs" company="Niel Morgan Thomas">
//     Copyright (c) 2015. All rights reserved.
// </copyright>
// <summary>
// Custom button as Rectangle or Ellipse that emits pulses.
// </summary>
// ***********************************************************************
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EasyVideoWin.CustomControls
{
  /// <summary>
  /// Class PulseButton.
  /// </summary>
  #region PARTS
  //[TemplatePart(Name = "PART_body", Type = typeof(Grid))]
  //[TemplatePart(Name = "PART_reflex", Type = typeof(Shape))]
  //[TemplatePart(Name = "PART_button", Type = typeof(Shape))]
  [TemplatePart(Name = "PART_pulse_container", Type = typeof(Canvas))]
  #endregion
  public class PulseButton : ButtonBase
  {
    #region -- Members --
    //private Grid partBody;
    //private Shape partButton;
    //private Shape partReflex;
    /// <summary>
    /// The part pulse container
    /// </summary>
    private Grid partPulseContainer;

    #endregion

    #region -- Properties --

    public static readonly DependencyProperty PulseEasingProperty = DependencyProperty.Register(
      "PulseEasing", typeof (EasingFunctionBase), typeof (PulseButton), new PropertyMetadata(default(EasingFunctionBase), PulsesChanged));

    /// <summary>
    /// Gets or sets the easing function used with the pulses
    /// </summary>
    /// <value>The pulse easing.</value>
    public EasingFunctionBase PulseEasing
    {
      get { return (EasingFunctionBase) GetValue(PulseEasingProperty); }
      set { SetValue(PulseEasingProperty, value); }
    }

    /// <summary>
    /// The is reflective property
    /// </summary>
    public static readonly DependencyProperty IsReflectiveProperty = DependencyProperty.Register(
      "IsReflective", typeof(bool), typeof(PulseButton), new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets a value indicating whether the button has a reflex.
    /// </summary>
    /// <value><c>true</c> if this instance is reflective; otherwise, <c>false</c>.</value>
    public bool IsReflective
    {
      get { return (bool)GetValue(IsReflectiveProperty); }
      set { SetValue(IsReflectiveProperty, value); }
    }

    /// <summary>
    /// The pulse scale property.
    /// </summary>
    public static readonly DependencyProperty PulseScaleProperty = DependencyProperty.Register(
      "PulseScale", typeof(double), typeof(PulseButton), new PropertyMetadata(1.3, PulsesChanged, PulseScaleValidation));

    /// <summary>
    /// Gets or sets the pulse scale, values between 1 and 3
    /// </summary>
    /// <value>The pulse scale.</value>
    public double PulseScale
    {
      get { return (double)GetValue(PulseScaleProperty); }
      set { SetValue(PulseScaleProperty, value); }
    }

    /// <summary>
    /// The is pulsing property
    /// </summary>
    public static readonly DependencyProperty IsPulsingProperty = DependencyProperty.Register(
      "IsPulsing", typeof(bool), typeof(PulseButton), new PropertyMetadata(true, IsPulsingChanged));

    /// <summary>
    /// Gets or sets a value indicating whether the button is pulsing.
    /// </summary>
    /// <value><c>true</c> if this instance is pulsing; otherwise, <c>false</c>.</value>
    public bool IsPulsing
    {
      get { return (bool)GetValue(IsPulsingProperty); }
      set { SetValue(IsPulsingProperty, value); }
    }

    /// <summary>
    /// The pulse width property
    /// </summary>
    public static readonly DependencyProperty PulseWidthProperty = DependencyProperty.Register(
      "PulseWidth", typeof(int), typeof(PulseButton), new PropertyMetadata(5, PulsesChanged));

    /// <summary>
    /// Gets or sets the width of each individual pulse.
    /// </summary>
    /// <value>The width of the pulse.</value>
    public int PulseWidth
    {
      get { return (int)GetValue(PulseWidthProperty); }
      set { SetValue(PulseWidthProperty, value); }
    }

    /// <summary>
    /// The is ellipsis property
    /// </summary>
    public static readonly DependencyProperty IsEllipsisProperty = DependencyProperty.Register(
      "IsEllipsis", typeof(bool), typeof(PulseButton), new PropertyMetadata(default(bool), PulsesChanged));

    /// <summary>
    /// Gets or sets a value indicating whether the button is an ellipsis.
    /// </summary>
    /// <value><c>true</c> if this instance is ellipsis; otherwise, <c>false</c>.</value>
    public bool IsEllipsis
    {
      get { return (bool)GetValue(IsEllipsisProperty); }
      set { SetValue(IsEllipsisProperty, value); }
    }

    /// <summary>
    /// The radius x property
    /// </summary>
    public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(
      "RadiusX", typeof(double), typeof(PulseButton), new PropertyMetadata(0.0, RadiusChanged));

    /// <summary>
    /// Gets or sets the radius x (Rectangle).
    /// </summary>
    /// <value>The radius x.</value>
    public double RadiusX
    {
      get { return (double)GetValue(RadiusXProperty); }
      set { SetValue(RadiusXProperty, value); }
    }

    /// <summary>
    /// The radius y property
    /// </summary>
    public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(
      "RadiusY", typeof(double), typeof(PulseButton), new PropertyMetadata(0.0, RadiusChanged));

    /// <summary>
    /// Gets or sets the radius y (Rectangle).
    /// </summary>
    /// <value>The radius y.</value>
    public double RadiusY
    {
      get { return (double)GetValue(RadiusYProperty); }
      set { SetValue(RadiusYProperty, value); }
    }

    /// <summary>
    /// The pulses property
    /// </summary>
    public static readonly DependencyProperty PulsesProperty = DependencyProperty.Register(
      "Pulses", typeof(int), typeof(PulseButton), new PropertyMetadata(1, PulsesChanged));

    /// <summary>
    /// Gets or sets the number of pulses.
    /// </summary>
    /// <value>The pulses.</value>
    public int Pulses
    {
      get { return (int)GetValue(PulsesProperty); }
      set { SetValue(PulsesProperty, value); }
    }

    /// <summary>
    /// The pulse color property
    /// </summary>
    public static readonly DependencyProperty PulseColorProperty = DependencyProperty.Register(
      "PulseColor", typeof(Brush), typeof(PulseButton), new PropertyMetadata(Brushes.Red, PulsesChanged));

    /// <summary>
    /// Gets or sets the color of the pulses
    /// </summary>
    /// <value>The color of the pulse.</value>
    public Brush PulseColor
    {
      get { return (Brush)GetValue(PulseColorProperty); }
      set { SetValue(PulseColorProperty, value); }
    }

    /// <summary>
    /// The button brush property
    /// </summary>
    public static readonly DependencyProperty ButtonBrushProperty = DependencyProperty.Register(
      "ButtonBrush", typeof(Brush), typeof(PulseButton), new PropertyMetadata(
        new LinearGradientBrush(new GradientStopCollection(new[]
                                                     {
                                                       new GradientStop(Colors.LightGray,0), 
                                                       new GradientStop(Colors.DarkGray,1)
                                                     }), new Point(0, 0), new Point(0, 1)))
                                                     );

    /// <summary>
    /// Gets or sets the button brush.
    /// </summary>
    /// <value>The button brush.</value>
    public Brush ButtonBrush
    {
      get { return (Brush)GetValue(ButtonBrushProperty); }
      set { SetValue(ButtonBrushProperty, value); }
    }

    /// <summary>
    /// The button highlight brush property. 
    /// </summary>
    public static readonly DependencyProperty ButtonHighlightBrushProperty = DependencyProperty.Register(
      "ButtonHighlightBrush", typeof(Brush), typeof(PulseButton), new PropertyMetadata(new LinearGradientBrush(new GradientStopCollection(new[]
                                                     {
                                                       new GradientStop(Colors.LightBlue,0), 
                                                       new GradientStop(Colors.SkyBlue,1)
                                                     }), new Point(0, 0), new Point(0, 1))));

    /// <summary>
    /// Gets or sets the button highlight brush (Mouse over).
    /// </summary>
    /// <value>The button highlight brush.</value>
    public Brush ButtonHighlightBrush
    {
      get { return (Brush)GetValue(ButtonHighlightBrushProperty); }
      set { SetValue(ButtonHighlightBrushProperty, value); }
    }

    /// <summary>
    /// The button pressed brush property
    /// </summary>
    public static readonly DependencyProperty ButtonPressedBrushProperty = DependencyProperty.Register(
      "ButtonPressedBrush", typeof(Brush), typeof(PulseButton), new PropertyMetadata(new LinearGradientBrush(new GradientStopCollection(new[]
                                                     {
                                                       new GradientStop(Colors.SkyBlue,0), 
                                                       new GradientStop(Colors.DodgerBlue,1)
                                                     }), new Point(0, 0), new Point(0, 1))));

    /// <summary>
    /// Gets or sets the button pressed brush. (Mouse down)
    /// </summary>
    /// <value>The button pressed brush.</value>
    public Brush ButtonPressedBrush
    {
      get { return (Brush)GetValue(ButtonPressedBrushProperty); }
      set { SetValue(ButtonPressedBrushProperty, value); }
    }

    /// <summary>
    /// The button disabled brush property
    /// </summary>
    public static readonly DependencyProperty ButtonDisabledBrushProperty = DependencyProperty.Register(
      "ButtonDisabledBrush", typeof(Brush), typeof(PulseButton), new PropertyMetadata(Brushes.LightGray));

    /// <summary>
    /// Gets or sets the button disabled brush. (IsEnabled = false)
    /// </summary>
    /// <value>The button disabled brush.</value>
    public Brush ButtonDisabledBrush
    {
      get { return (Brush)GetValue(ButtonDisabledBrushProperty); }
      set { SetValue(ButtonDisabledBrushProperty, value); }
    }

    /// <summary>
    /// The pulse speed property
    /// </summary>
    public static readonly DependencyProperty PulseSpeedProperty = DependencyProperty.Register(
      "PulseSpeed", typeof(Duration), typeof(PulseButton), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(2)), PulsesChanged));

    /// <summary>
    /// Gets or sets the pulse speed ie. duration of the animation for a single pulse.
    /// </summary>
    /// <value>The pulse speed.</value>
    public Duration PulseSpeed
    {
      get { return (Duration)GetValue(PulseSpeedProperty); }
      set { SetValue(PulseSpeedProperty, value); }
    }

    #endregion

    #region -- Constructor --

    /// <summary>
    /// Initializes static members of the <see cref="PulseButton"/> class.
    /// </summary>
    static PulseButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(PulseButton), new FrameworkPropertyMetadata(typeof(PulseButton)));
    }

    #endregion

    #region -- Public Methods --

    /// <summary>
    /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      // Reference parts
      //partBody = Template.FindName("PART_body", this) as Grid;
      //partReflex = Template.FindName("PART_reflex", this) as Shape;
      //partButton = Template.FindName("PART_button", this) as Shape;
      partPulseContainer = Template.FindName("PART_pulse_container", this) as Grid;
      // Initialize Control
      PulsesChanged(this, new DependencyPropertyChangedEventArgs(PulsesProperty, 0, Pulses));
      IsPulsingChanged(this, new DependencyPropertyChangedEventArgs(IsPulsingProperty, null, IsPulsing));
      SizeChanged += OnSizeChanged;
    }

    #endregion

    #region -- Private Methods --

    /// <summary>
    /// Pulse Scale validation.
    /// </summary>
    /// <param name="d">The d.</param>
    /// <param name="value">The value.</param>
    /// <returns>System.Object.</returns>
    private static object PulseScaleValidation(DependencyObject d, object value)
    {
      double current = (double)value;
      if (current < 1) current = 1;
      if (current > 3) current = 3;
      return current;
    }

    /// <summary>
    /// Handles the <see cref="E:SizeChanged" /> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="sizeChangedEventArgs">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
    private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
    {
      PulsesChanged((DependencyObject)sender, new DependencyPropertyChangedEventArgs());
    }


    #region -- Private Methods --


    /// <summary>
    /// Determines whether IsPulsing changed.
    /// </summary>
    /// <param name="d">The d.</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void IsPulsingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PulseButton pb = (PulseButton)d;
      if (pb == null || pb.partPulseContainer == null) return;
      if (!pb.IsPulsing)
        pb.partPulseContainer.Children.Clear();
      else
        PulsesChanged(d, e);
    }

    /// <summary>
    /// Radius changed.
    /// </summary>
    /// <param name="d">The d.</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void RadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PulseButton pb = (PulseButton)d;
      if (pb == null || pb.partPulseContainer == null) return;
      if (!pb.IsEllipsis)
        PulsesChanged(d, e);
    }



    /// <summary>
    /// Pulses changed.
    /// </summary>
    /// <param name="d">The d.</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void PulsesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PulseButton pb = (PulseButton)d;
      if (pb == null || pb.partPulseContainer == null || !pb.IsPulsing) return;
      // Clear all pulses
      pb.partPulseContainer.Children.Clear();
      int items = pb.Pulses;
      // Add pulses
      for (int i = 0; i < items; i++)
      {

        Shape shape = pb.IsEllipsis ?
          (Shape)new Ellipse
                  {
                    StrokeThickness = pb.PulseWidth,
                    Stroke = pb.PulseColor,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    IsHitTestVisible = false
                  } :
          new Rectangle
          {
            RadiusX = pb.RadiusX,
            RadiusY = pb.RadiusY,
            StrokeThickness = pb.PulseWidth,
            Stroke = pb.PulseColor,
            RenderTransformOrigin = new Point(0.5, 0.5),
            IsHitTestVisible = false
          };
        pb.partPulseContainer.Children.Add(shape);
      }
      // Set animations
      pb.SetStoryBoard(pb);
    }

    /// <summary>
    /// Sets the story board for the pulses
    /// </summary>
    /// <param name="pb">The pb.</param>
    private void SetStoryBoard(PulseButton pb)
    {
      double delay = 0;

      // Correct PulseScale according to control dimensions
      double correctedFactorX = pb.PulseScale, correctedFactorY = pb.PulseScale;
      if (pb.IsMeasureValid)
      {
        if (pb.ActualHeight < pb.ActualWidth)
          correctedFactorY = (pb.PulseScale - 1) * ((pb.ActualWidth - pb.ActualHeight) / (1 + pb.ActualHeight)) + pb.PulseScale;
        else
          correctedFactorX = (pb.PulseScale - 1) * ((pb.ActualHeight - pb.ActualWidth) / (1 + pb.ActualWidth)) + pb.PulseScale;
      }
      // Add pulses
      foreach (Shape shape in pb.partPulseContainer.Children)
      {
        shape.RenderTransform = new ScaleTransform();
        // X-axis animation
        DoubleAnimation animation = new DoubleAnimation(1, correctedFactorX, pb.PulseSpeed)
                        {
                          RepeatBehavior = RepeatBehavior.Forever,
                          AutoReverse = false,
                          BeginTime = TimeSpan.FromMilliseconds(delay),
                          EasingFunction = pb.PulseEasing
                        };
        // Y-axis animation
        DoubleAnimation animation2 = new DoubleAnimation(1, correctedFactorY, pb.PulseSpeed)
                         {
                           RepeatBehavior = RepeatBehavior.Forever,
                           AutoReverse = false,
                           BeginTime = TimeSpan.FromMilliseconds(delay),
                           EasingFunction = pb.PulseEasing
                         };
        // Opacity animation
        DoubleAnimation animation3 = new DoubleAnimation(1, 0, pb.PulseSpeed)
        {
          RepeatBehavior = RepeatBehavior.Forever,
          AutoReverse = false,
          BeginTime = TimeSpan.FromMilliseconds(delay)
        };
        // Set delay between pulses
        delay += pb.PulseSpeed.TimeSpan.TotalMilliseconds / pb.Pulses;
        // Create storyboard
        Storyboard storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        storyboard.Children.Add(animation2);
        storyboard.Children.Add(animation3);
        Storyboard.SetTarget(animation, shape);
        Storyboard.SetTarget(animation2, shape);
        Storyboard.SetTarget(animation3, shape);
        if (pb.IsEllipsis)
        {
          Storyboard.SetTargetProperty(animation, new PropertyPath("(Ellipse.RenderTransform).(ScaleTransform.ScaleX)"));
          Storyboard.SetTargetProperty(animation2, new PropertyPath("(Ellipse.RenderTransform).(ScaleTransform.ScaleY)"));
          Storyboard.SetTargetProperty(animation3, new PropertyPath("(Ellipse.Opacity)"));
        }
        else
        {
          Storyboard.SetTargetProperty(animation, new PropertyPath("(Rectangle.RenderTransform).(ScaleTransform.ScaleX)"));
          Storyboard.SetTargetProperty(animation2, new PropertyPath("(Rectangle.RenderTransform).(ScaleTransform.ScaleY)"));
          Storyboard.SetTargetProperty(animation3, new PropertyPath("(Rectangle.Opacity)"));
        }
        // Start storyboard
        storyboard.Begin();
      }

    #endregion

    }

    #endregion
  }
}
