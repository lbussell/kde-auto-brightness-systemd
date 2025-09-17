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
    var sunPosition = SunCalc.GetSunPosition(
        DateTime.Now,
        lat: decimal.ToDouble(latitude),
        lng: decimal.ToDouble(longitude)
    );

    var sunAltitudeDegrees = RadiansToDegrees(sunPosition.Altitude);
    var brightness = SunAltitudeToBrightness(sunAltitudeDegrees);
    await SetBrightness(brightness);

    var theme = sunAltitudeDegrees >= 0 ? LightTheme : DarkTheme;
    await SetTheme(theme);

    WriteLine($"Sun altitude: {sunAltitudeDegrees}");
    WriteLine($"Brightness set to {brightness}%");
});

static int SunAltitudeToBrightness(
    double sunAltitudeDegrees,
    int minBrightness = MinBrightness,
    int maxBrightness = MaxBrightness)
{
    if (sunAltitudeDegrees <= MinAltitudeDegrees)
    {
        return minBrightness;
    }
    else if (sunAltitudeDegrees >= MaxAltitudeDegrees)
    {
        return maxBrightness;
    }
    else
    {
        var t = (sunAltitudeDegrees - MinAltitudeDegrees) / (MaxAltitudeDegrees - MinAltitudeDegrees);
        return (int)Math.Round(minBrightness + t * (maxBrightness - minBrightness));
    }
}

// Value should be between 0 and 100
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

static double RadiansToDegrees(double radians) => radians * (180.0 / Math.PI);
