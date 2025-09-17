// SPDX-FileCopyrightText: Copyright (c) 2025 Logan Bussell
// SPDX-License-Identifier: MIT

#:package SunCalcNet@1.2.3
#:package ConsoleAppFramework@5.6.1
#:package CliWrap@3.9.0

using CliWrap;
using ConsoleAppFramework;
using SunCalcNet;
using static System.Console;

// Run 'ddcutil capabilities' to check if your monitor supports brightness controls
const string BrightnessFeatureCode = "10";

// Brightness settings
const int MinBrightness = 20;
const int MaxBrightness = 80;
const int MinAltitudeDegrees = -15;
const int MaxAltitudeDegrees = 60;

// Run 'plasma-apply-colorscheme --list' to see available color schemes
const string LightTheme = "BreezeLight";
const string DarkTheme = "BreezeDark";

await ConsoleApp.RunAsync(args, async (decimal latitude, decimal longitude) =>
{
    var sunAltitudeDegrees = GetSunAltitudeDegrees(latitude, longitude);
    var brightnessDouble = SunAltitudeToBrightness(sunAltitudeDegrees);
    var brightness = (int)Math.Round(brightnessDouble);

    // Switch to dark theme after sunset
    var theme = sunAltitudeDegrees >= 0 ? LightTheme : DarkTheme;
    await SetTheme(theme);

    // Increase brightness a little to compensate for dark mode
    if (theme == DarkTheme) brightness += 15;
    await SetBrightness(brightness);

    WriteLine($"Sun altitude: {sunAltitudeDegrees}");
    WriteLine($"Brightness set to {brightness}%");
});

static double GetSunAltitudeDegrees(decimal latitude, decimal longitude)
{
    var sunPosition = SunCalc.GetSunPosition(
        DateTime.Now,
        decimal.ToDouble(latitude),
        decimal.ToDouble(longitude));

    return double.RadiansToDegrees(sunPosition.Altitude);
}

static double SunAltitudeToBrightness(double altitudeDegrees) => altitudeDegrees switch
{
    <= MinAltitudeDegrees => MinBrightness,
    >= MaxAltitudeDegrees => MaxBrightness,
    _ => double.Lerp(
        value1: MinBrightness,
        value2: MaxBrightness,
        amount: altitudeDegrees / MaxAltitudeDegrees)
};

static Task SetBrightness(int value) =>
    Cli.Wrap("ddcutil")
        .WithArguments([
            "setvcp",
            BrightnessFeatureCode,
            Math.Clamp(value, 0, 100).ToString()
        ])
        .ExecuteAsync();

static Task SetTheme(string theme) =>
    Cli.Wrap("plasma-apply-colorscheme")
        .WithArguments([theme])
        .ExecuteAsync();
