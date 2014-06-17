﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Terminal
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Terminal"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Terminal;assembly=Terminal"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:VideoTerminal/>
    ///
    /// </summary>
    public class VideoTerminal : UserControl
    {
        static VideoTerminal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoTerminal), new FrameworkPropertyMetadata(typeof(VideoTerminal)));
        }
        class Caret : FrameworkElement
        {
            System.Threading.Timer timer;
            public double CaretHeight { get; set; }
            int blinkPeriod = 500;
            Pen pen = new Pen(Brushes.Black, 1);

            public static readonly DependencyProperty VisibleProperty =
              DependencyProperty.Register("Visible", typeof(bool),
              typeof(Caret), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty LocationPropertyX =
              DependencyProperty.Register("LocationX", typeof(int),
              typeof(Caret), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
            public static readonly DependencyProperty LocationPropertyY =
              DependencyProperty.Register("LocationY", typeof(int),
              typeof(Caret), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
            public Caret()
            {
                pen.Freeze();
                CaretHeight = 14;
                Visible = true;
                timer = new System.Threading.Timer(blinkCaret, null, 0, blinkPeriod);
            }
            Point location;
            protected override void OnRender(DrawingContext dc)
            {
                if (Visible)
                {
                    location.Y = (double)LocationY * 14;
                    location.X = LocationX * 7;
                    dc.DrawLine(pen, location, new Point(location.X, location.Y + CaretHeight));
                }
            }


            public int LocationX
            {
                get
                {
                    return (int)GetValue(LocationPropertyX);
                }
                set
                {
                    SetValue(LocationPropertyX, value);
                }
            }

            public int LocationY
            {
                get
                {
                    return (int)GetValue(LocationPropertyY);
                }
                set
                {
                    SetValue(LocationPropertyY, value);
                }
            }

            bool Visible
            {
                get
                {
                    return (bool)GetValue(VisibleProperty);
                }
                set
                {
                    SetValue(VisibleProperty, value);
                }
            }

            void blinkCaret(Object state)
            {
                Dispatcher.Invoke(new Action(delegate { Visible = !Visible; }));
            }
        }

        

        class Screen : FrameworkElement
        {
            string content = "";

            public VideoTerminalChar[,] Buffer
            {
                get
                {
                    return (VideoTerminalChar[,])GetValue(BufferProperty);
                }
                set
                {
                    SetValue(BufferProperty, value);
                }
            }

            public bool Visible
            {
                get
                {
                    return (bool)GetValue(VisibleProperty);
                }
                set
                {
                    SetValue(VisibleProperty, value);
                }
            }

            public Screen()
            {
                Buffer = new VideoTerminalChar[80, 24];
                for (int i = 0; i < 80; i++)
                {
                    for (int j = 0; j < 24; j++)
                    {
                        Buffer[i, j] = new VideoTerminalChar();
                    }
                }
            }

            public static readonly DependencyProperty BufferProperty =
              DependencyProperty.Register("Buffer", typeof(VideoTerminalChar[,]),
              typeof(Screen), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty VisibleProperty =
              DependencyProperty.Register("Visible", typeof(bool),
              typeof(Screen), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                if (content == "")
                {
                    string testString = "Terminal Sharp\r\nA full C# based ssh2 terminal";

                    // Create the initial formatted text string.
                    FormattedText formattedText = new FormattedText(
                        testString,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        32,
                        Brushes.Black);
                    formattedText.SetFontSize(36 * (96.0 / 72.0), 0, 15);
                    formattedText.SetFontWeight(FontWeights.Bold, 0, 8);
                    formattedText.SetForegroundBrush(new LinearGradientBrush(Colors.Orange, Colors.Teal, 90.0), 9, 5);
                    drawingContext.DrawText(formattedText, new Point(10, 0));

                    content = "1";
                }
                else
                {

                    for (int i = 0; i < 80; i++)
                    {
                        for (int j = 0; j < 24; j++)
                        {
                            drawingContext.DrawText(Buffer[i, j].GetFormattedText(), new Point(i * 7, j * 14));
                        }
                    }
                }
            }
        }


        Caret caret = new Caret();
        Screen screen = new Screen();
        public VideoTerminal()
        {

            //this.AddChild(screen);
            this.AddVisualChild(caret);
            this.AddVisualChild(screen);
            this.Background = null;
        }

        protected override int VisualChildrenCount
        {
            get { return 2; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            switch (index)
            {
                default:
                case 1:
                    return caret;
                case 0:
                    return screen;
            }
        }
        /*
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.VisualChildrenCount > 0)
            {
                UIElement child = this.GetVisualChild(0) as UIElement;
                child.Measure(availableSize);
                return child.DesiredSize;
            }

            return availableSize;
        }*/

        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect arrangeRect = new Rect()
            {
                Width = finalSize.Width,
                Height = finalSize.Height
            };

            if (this.VisualChildrenCount > 0)
            {
                UIElement child = this.GetVisualChild(0) as UIElement;
                child.Arrange(arrangeRect);

                child = this.GetVisualChild(1) as UIElement;
                child.Arrange(arrangeRect);
            }

            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
        }


        public void HandleServerData(string data)
        {

            screen.Dispatcher.Invoke(DispatcherPriority.Render, (Action)delegate() { });
            screen.Dispatcher.BeginInvoke((Action)delegate()
            {
            char[] chars = data.ToCharArray();
            foreach (char c in chars)
            {
                if (c == '\r')
                {
                    caret.LocationX = 0;
                }
                else if (c == '\n')
                {
                    caret.LocationY++;
                }
                else if (c == '\b')
                {

                }
                else if (c == 033)
                {
                    //
                }
                else
                {
                    screen.Buffer[caret.LocationX, caret.LocationY] = new VideoTerminalChar(c);
                    caret.LocationX++;
                }


                if (caret.LocationY == 24)
                {
                    for (int j = 1; j < 24; j++)
                    {
                        for (int i = 0; i < 80; i++)
                        {
                            screen.Buffer[i, j - 1] = screen.Buffer[i, j];
                        }
                    }
                    for (int i = 0; i < 80; i++)
                    {
                        screen.Buffer[i, 23] = new VideoTerminalChar();
                    }
                    caret.LocationY--;
                }
                if (caret.LocationX == 80)
                {
                    caret.LocationX = 0;
                    caret.LocationY++;
                }
                if (caret.LocationY == 24)
                {
                    for (int j = 1; j < 24; j++)
                    {
                        for (int i = 0; i < 80; i++)
                        {
                            screen.Buffer[i, j - 1] = screen.Buffer[i, j];
                        }
                    }
                    for (int i = 0; i < 80; i++)
                    {
                        screen.Buffer[i, 23] = new VideoTerminalChar();
                    }
                    caret.LocationY--;
                }
            }
            VideoTerminalChar[,] XXX = (VideoTerminalChar[,])screen.Buffer.Clone();
            screen.Visible = false;
            screen.Visible = true;
            screen.Buffer = XXX;
            this.InvalidateVisual();
            });
        }

        string buff = "";
        public void HandleClientData(Key data)
        {
            if (data == Key.Return)
            {
                buff += "\n";
            }
            else if (data == Key.Space)
            {
                buff += " ";
            }
            else
            {
                buff += (char)KeyInterop.VirtualKeyFromKey(data);
            }

        }

        public void HandleClientData(string data)
        {
            buff += data;
        }

        public string GetClientData()
        {
            string temp = buff;
            buff = "";
            return temp;
        }



        class VideoTerminalChar
        {
            static CultureInfo cultureinfo = CultureInfo.GetCultureInfo("en-us");
            static FlowDirection flowdirection = FlowDirection.LeftToRight;
            static Typeface typeface = new Typeface("Consolas, Simsun");
            static double fontsize = 12;
            static Brush fontcolor = Brushes.Black;

            FormattedText formattedText;
            string value;
            int width; // such as 0, 1, 2
            public VideoTerminalChar()
            {
                value = "";
                width = 0;
                UpdateFormattedText();
            }
            public VideoTerminalChar(char c)
            {
                value = c.ToString();
                width = 1;
                UpdateFormattedText();
            }
            public void UpdateFormattedText()
            {
                formattedText = new FormattedText(value, cultureinfo, flowdirection, typeface, fontsize, fontcolor);
            }
            public FormattedText GetFormattedText()
            {
                return formattedText;
            }
        }
    }
}
