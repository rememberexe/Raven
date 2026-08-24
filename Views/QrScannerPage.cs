using Microsoft.Maui.Controls.Shapes;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace RavenMobile.Views;

public class QrScannerPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _resultSource;
    private readonly CameraBarcodeReaderView _reader;
    private readonly BoxView _scanLine;

    private bool _handled;
    private bool _animationRunning;

    public QrScannerPage(TaskCompletionSource<string?> resultSource)
    {
        _resultSource = resultSource;

        NavigationPage.SetHasNavigationBar(this, false);

        BackgroundColor = Color.FromArgb("#05060A");

        _reader = new CameraBarcodeReaderView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            IsDetecting = true,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.TwoDimensional,
                AutoRotate = true,
                Multiple = false
            }
        };

        _reader.BarcodesDetected += OnBarcodesDetected;

        _scanLine = new BoxView
        {
            HeightRequest = 3,
            WidthRequest = 220,
            Color = Color.FromArgb("#2D6BFF"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Opacity = 0.95,
            Shadow = new Shadow
            {
                Brush = Color.FromArgb("#2D6BFF"),
                Radius = 18,
                Offset = new Point(0, 0),
                Opacity = 0.8f
            }
        };

        Content = BuildLayout();
    }

    private View BuildLayout()
    {
        var root = new Grid();

        root.Children.Add(_reader);

        root.Children.Add(new BoxView
        {
            BackgroundColor = Color.FromArgb("#88000000"),
            InputTransparent = true
        });

        root.Children.Add(BuildGlowBackground());

        root.Children.Add(BuildTopBar());

        root.Children.Add(BuildScannerFrame());

        root.Children.Add(BuildBottomPanel());

        return root;
    }

    private View BuildGlowBackground()
    {
        return new Grid
        {
            InputTransparent = true,
            Children =
            {
                new Border
                {
                    WidthRequest = 280,
                    HeightRequest = 280,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, -120, -120, 0),
                    Opacity = 0.35,
                    StrokeThickness = 0,
                    Background = new RadialGradientBrush
                    {
                        Center = new Point(0.5, 0.5),
                        Radius = 0.75,
                        GradientStops =
                        {
                            new GradientStop(Color.FromArgb("#2D6BFF"), 0),
                            new GradientStop(Color.FromArgb("#002D6BFF"), 1)
                        }
                    },
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(180)
                    }
                },

                new Border
                {
                    WidthRequest = 240,
                    HeightRequest = 240,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(-130, 0, 0, 0),
                    Opacity = 0.22,
                    StrokeThickness = 0,
                    Background = new RadialGradientBrush
                    {
                        Center = new Point(0.5, 0.5),
                        Radius = 0.75,
                        GradientStops =
                        {
                            new GradientStop(Color.FromArgb("#8A4DFF"), 0),
                            new GradientStop(Color.FromArgb("#008A4DFF"), 1)
                        }
                    },
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(180)
                    }
                }
            }
        };
    }

    private View BuildTopBar()
    {
        var closeButton = new Button
        {
            Text = "✕",
            WidthRequest = 46,
            HeightRequest = 46,
            CornerRadius = 18,
            BackgroundColor = Color.FromArgb("#CC111722"),
            BorderColor = Color.FromArgb("#2B3448"),
            BorderWidth = 1,
            TextColor = Colors.White,
            FontSize = 18,
            Padding = 0
        };

        closeButton.Clicked += async (_, _) =>
        {
            _resultSource.TrySetResult(null);
            await Navigation.PopAsync();
        };

        return new Grid
        {
            Padding = new Thickness(20, 22, 20, 0),
            VerticalOptions = LayoutOptions.Start,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        new Label
                        {
                            Text = "QR okut",
                            TextColor = Colors.White,
                            FontSize = 30,
                            FontAttributes = FontAttributes.Bold
                        },
                        new Label
                        {
                            Text = "Alıcı cihazdaki Raven QR kodunu kameraya göster.",
                            TextColor = Color.FromArgb("#A2ABBD"),
                            FontSize = 13
                        }
                    }
                },

                closeButton
            }
        }.Also(grid =>
        {
            Grid.SetColumn(closeButton, 1);
        });
    }

    private View BuildScannerFrame()
    {
        var frameSize = 278;

        var frame = new Grid
        {
            WidthRequest = frameSize,
            HeightRequest = frameSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        frame.Children.Add(new Border
        {
            WidthRequest = frameSize,
            HeightRequest = frameSize,
            BackgroundColor = Color.FromArgb("#22000000"),
            Stroke = Color.FromArgb("#355BFF"),
            StrokeThickness = 1.2,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(38)
            },
            Shadow = new Shadow
            {
                Brush = Color.FromArgb("#2D6BFF"),
                Radius = 26,
                Offset = new Point(0, 0),
                Opacity = 0.35f
            }
        });

        frame.Children.Add(BuildCorner("TopLeft"));
        frame.Children.Add(BuildCorner("TopRight"));
        frame.Children.Add(BuildCorner("BottomLeft"));
        frame.Children.Add(BuildCorner("BottomRight"));

        frame.Children.Add(_scanLine);

        frame.Children.Add(new Border
        {
            WidthRequest = 72,
            HeightRequest = 28,
            BackgroundColor = Color.FromArgb("#DD101722"),
            Stroke = Color.FromArgb("#2D6BFF"),
            StrokeThickness = 1,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, -14, 0, 0),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(14)
            },
            Content = new Label
            {
                Text = "RAVEN",
                TextColor = Color.FromArgb("#8FB0FF"),
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        });

        return frame;
    }

    private View BuildCorner(string position)
    {
        var vertical = new BoxView
        {
            WidthRequest = 4,
            HeightRequest = 42,
            Color = Color.FromArgb("#2D6BFF"),
            CornerRadius = 2
        };

        var horizontal = new BoxView
        {
            WidthRequest = 42,
            HeightRequest = 4,
            Color = Color.FromArgb("#2D6BFF"),
            CornerRadius = 2
        };

        var corner = new Grid
        {
            WidthRequest = 52,
            HeightRequest = 52,
            Children =
            {
                vertical,
                horizontal
            }
        };

        switch (position)
        {
            case "TopLeft":
                corner.HorizontalOptions = LayoutOptions.Start;
                corner.VerticalOptions = LayoutOptions.Start;
                vertical.HorizontalOptions = LayoutOptions.Start;
                vertical.VerticalOptions = LayoutOptions.Start;
                horizontal.HorizontalOptions = LayoutOptions.Start;
                horizontal.VerticalOptions = LayoutOptions.Start;
                break;

            case "TopRight":
                corner.HorizontalOptions = LayoutOptions.End;
                corner.VerticalOptions = LayoutOptions.Start;
                vertical.HorizontalOptions = LayoutOptions.End;
                vertical.VerticalOptions = LayoutOptions.Start;
                horizontal.HorizontalOptions = LayoutOptions.End;
                horizontal.VerticalOptions = LayoutOptions.Start;
                break;

            case "BottomLeft":
                corner.HorizontalOptions = LayoutOptions.Start;
                corner.VerticalOptions = LayoutOptions.End;
                vertical.HorizontalOptions = LayoutOptions.Start;
                vertical.VerticalOptions = LayoutOptions.End;
                horizontal.HorizontalOptions = LayoutOptions.Start;
                horizontal.VerticalOptions = LayoutOptions.End;
                break;

            case "BottomRight":
                corner.HorizontalOptions = LayoutOptions.End;
                corner.VerticalOptions = LayoutOptions.End;
                vertical.HorizontalOptions = LayoutOptions.End;
                vertical.VerticalOptions = LayoutOptions.End;
                horizontal.HorizontalOptions = LayoutOptions.End;
                horizontal.VerticalOptions = LayoutOptions.End;
                break;
        }

        return corner;
    }

    private View BuildBottomPanel()
    {
        return new Border
        {
            Margin = new Thickness(20, 0, 20, 28),
            Padding = new Thickness(18),
            BackgroundColor = Color.FromArgb("#E810121B"),
            Stroke = Color.FromArgb("#2A3550"),
            StrokeThickness = 1,
            VerticalOptions = LayoutOptions.End,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(28)
            },
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Radius = 22,
                Offset = new Point(0, 8),
                Opacity = 0.45f
            },
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = GridLength.Star }
                        },
                        ColumnSpacing = 12,
                        Children =
                        {
                            new Border
                            {
                                WidthRequest = 42,
                                HeightRequest = 42,
                                BackgroundColor = Color.FromArgb("#182746"),
                                Stroke = Color.FromArgb("#2D6BFF"),
                                StrokeThickness = 1,
                                StrokeShape = new RoundRectangle
                                {
                                    CornerRadius = new CornerRadius(17)
                                },
                                Content = new Label
                                {
                                    Text = "⌁",
                                    TextColor = Color.FromArgb("#82A9FF"),
                                    FontSize = 24,
                                    FontAttributes = FontAttributes.Bold,
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center
                                }
                            },

                            new VerticalStackLayout
                            {
                                Spacing = 3,
                                Children =
                                {
                                    new Label
                                    {
                                        Text = "Alıcı QR kodunu hizala",
                                        TextColor = Colors.White,
                                        FontSize = 17,
                                        FontAttributes = FontAttributes.Bold
                                    },
                                    new Label
                                    {
                                        Text = "Kod algılanınca bağlantı otomatik başlar.",
                                        TextColor = Color.FromArgb("#8E98AD"),
                                        FontSize = 13
                                    }
                                }
                            }.Also(v => Grid.SetColumn(v, 1))
                        }
                    },

                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                        ColumnSpacing = 8,
                        Children =
                        {
                            new Label
                            {
                                Text = "●",
                                TextColor = Color.FromArgb("#58D68D"),
                                FontSize = 13,
                                VerticalOptions = LayoutOptions.Center
                            },

                            new Label
                            {
                                Text = "Kamera aktif",
                                TextColor = Color.FromArgb("#B7C1D6"),
                                FontSize = 12,
                                VerticalOptions = LayoutOptions.Center
                            }.Also(v => Grid.SetColumn(v, 1)),

                            new Label
                            {
                                Text = "Raven Scan",
                                TextColor = Color.FromArgb("#5F6B82"),
                                FontSize = 12,
                                FontAttributes = FontAttributes.Bold,
                                VerticalOptions = LayoutOptions.Center
                            }.Also(v => Grid.SetColumn(v, 2))
                        }
                    }
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.RequestAsync<Permissions.Camera>();

        if (status != PermissionStatus.Granted)
        {
            _resultSource.TrySetResult(null);
            await Navigation.PopAsync();
            return;
        }

        _reader.IsDetecting = true;

        StartScanAnimation();
    }

    protected override void OnDisappearing()
    {
        _animationRunning = false;
        _reader.IsDetecting = false;

        base.OnDisappearing();
    }

    private async void StartScanAnimation()
    {
        if (_animationRunning)
            return;

        _animationRunning = true;

        await Task.Delay(250);

        while (_animationRunning && !_handled)
        {
            _scanLine.TranslationY = -105;
            await _scanLine.TranslateTo(0, 105, 1250, Easing.CubicInOut);

            if (!_animationRunning || _handled)
                break;

            await _scanLine.FadeTo(0.35, 120);
            _scanLine.TranslationY = -105;
            await _scanLine.FadeTo(0.95, 120);
        }
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_handled)
            return;

        var value = e.Results?.FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(value))
            return;

        _handled = true;
        _animationRunning = false;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _reader.IsDetecting = false;
            _resultSource.TrySetResult(value);

            await Navigation.PopAsync();
        });
    }

    protected override bool OnBackButtonPressed()
    {
        _animationRunning = false;
        _resultSource.TrySetResult(null);
        return base.OnBackButtonPressed();
    }
}

public static class ViewExtensions
{
    public static T Also<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}