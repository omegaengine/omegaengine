-- Import .NET constructors
luanet.load_assembly("Common")
NewColorCorrection = luanet.import_type("Common.Values.ColorCorrection")
luanet.load_assembly("System.Drawing")
NewColor = luanet.import_type("System.Drawing.Color").FromArgb

-- Define helper function rounding floats and then converting them to strings
function round(num)
  return string.format("%." .. 1 .. "f", num)
end

-- Define helper functions for block-updating .NET structs
function set_color_correction_phase0()
  Universe.ColorCorrectionMidnight = NewColorCorrection(Brightness0.Value / 10, Contrast0.Value / 10 - 5, Saturation0.Value / 10 - 5, Hue0.Value)
end
function set_color_correction_phase1()
  Universe.ColorCorrectionDawn = NewColorCorrection(Brightness1.Value / 10, Contrast1.Value / 10 - 5, Saturation1.Value / 10 - 5, Hue1.Value)
end
function set_color_correction_phase2()
  Universe.ColorCorrectionNoon = NewColorCorrection(Brightness2.Value / 10, Contrast2.Value / 10 - 5, Saturation2.Value / 10 - 5, Hue2.Value)
end
function set_color_correction_phase3()
  Universe.ColorCorrectionDusk = NewColorCorrection(Brightness3.Value / 10, Contrast3.Value / 10 - 5, Saturation3.Value / 10 - 5, Hue3.Value)
end
function set_ambient_color()
  Universe.AmbientColor = NewColor(AmbientColorRed.Value, AmbientColorGreen.Value, AmbientColorBlue.Value)
end
function set_sun()
  Universe.SunColor = NewColor(SunColorRed.Value, SunColorGreen.Value, SunColorBlue.Value)
  Universe.SunInclination = SunInclination.Value
end
function set_moon()
  Universe.MoonColor = NewColor(MoonColorRed.Value, MoonColorGreen.Value, MoonColorBlue.Value)
  Universe.MoonInclination = MoonInclination.Value
end

-- Define helper functions for refreshing GUI control values
function refresh_color_correction_phase0()
  Brightness0.Value = Universe.ColorCorrectionMidnight.Brightness * 10; Brightness0Label.Text = round(Universe.ColorCorrectionMidnight.Brightness)
  Contrast0.Value = Universe.ColorCorrectionMidnight.Contrast * 10 + 50; Contrast0Label.Text = round(Universe.ColorCorrectionMidnight.Contrast)
  Saturation0.Value = Universe.ColorCorrectionMidnight.Saturation * 10 + 50; Saturation0Label.Text = round(Universe.ColorCorrectionMidnight.Saturation)
  Hue0.Value = Universe.ColorCorrectionMidnight.Hue; Hue0Label.Text = Universe.ColorCorrectionMidnight.Hue
end
function refresh_color_correction_phase1()
  Brightness1.Value = Universe.ColorCorrectionDawn.Brightness * 10; Brightness1Label.Text = round(Universe.ColorCorrectionDawn.Brightness)
  Contrast1.Value = Universe.ColorCorrectionDawn.Contrast * 10 + 50; Contrast1Label.Text = round(Universe.ColorCorrectionDawn.Contrast)
  Saturation1.Value = Universe.ColorCorrectionDawn.Saturation * 10 + 50; Saturation1Label.Text = round(Universe.ColorCorrectionDawn.Saturation)
  Hue1.Value = Universe.ColorCorrectionDawn.Hue; Hue1Label.Text = Universe.ColorCorrectionDawn.Hue
end
function refresh_color_correction_phase2()
  Brightness2.Value = Universe.ColorCorrectionNoon.Brightness * 10; Brightness2Label.Text = round(Universe.ColorCorrectionNoon.Brightness)
  Contrast2.Value = Universe.ColorCorrectionNoon.Contrast * 10 + 50; Contrast2Label.Text = round(Universe.ColorCorrectionNoon.Contrast)
  Saturation2.Value = Universe.ColorCorrectionNoon.Saturation * 10 + 50; Saturation2Label.Text = round(Universe.ColorCorrectionNoon.Saturation)
  Hue2.Value = Universe.ColorCorrectionNoon.Hue; Hue2Label.Text = Universe.ColorCorrectionNoon.Hue;
end
function refresh_color_correction_phase3()
  Brightness3.Value = Universe.ColorCorrectionDusk.Brightness * 10; Brightness3Label.Text = round(Universe.ColorCorrectionDusk.Brightness)
  Contrast3.Value = Universe.ColorCorrectionDusk.Contrast * 10 + 50; Contrast3Label.Text = round(Universe.ColorCorrectionDusk.Contrast)
  Saturation3.Value = Universe.ColorCorrectionDusk.Saturation * 10 + 50; Saturation3Label.Text = round(Universe.ColorCorrectionDusk.Saturation)
  Hue3.Value = Universe.ColorCorrectionDusk.Hue; Hue3Label.Text = Universe.ColorCorrectionDusk.Hue;
end
function refresh_ambient_color()
  AmbientColorRed.Value = Universe.AmbientColor.R; AmbientColorRedLabel.Text = Universe.AmbientColor.R
  AmbientColorGreen.Value = Universe.AmbientColor.G; AmbientColorGreenLabel.Text = Universe.AmbientColor.G
  AmbientColorBlue.Value = Universe.AmbientColor.B; AmbientColorBlueLabel.Text = Universe.AmbientColor.B
end
function refresh_sun()
  SunColorRed.Value = Universe.SunColor.R; SunColorRedLabel.Text = Universe.SunColor.R
  SunColorGreen.Value = Universe.SunColor.G; SunColorGreenLabel.Text = Universe.SunColor.G
  SunColorBlue.Value = Universe.SunColor.B; SunColorBlueLabel.Text = Universe.SunColor.B
  while Universe.SunInclination < 0 do Universe.SunInclination = Universe.SunInclination + 360; end
  if Universe.SunInclination > 359 then Universe.SunInclination = 359; end
  SunInclination.Value = Universe.SunInclination
end
function refresh_moon()
  MoonColorRed.Value = Universe.MoonColor.R; MoonColorRedLabel.Text = Universe.MoonColor.R
  MoonColorGreen.Value = Universe.MoonColor.G; MoonColorGreenLabel.Text = Universe.MoonColor.G
  MoonColorBlue.Value = Universe.MoonColor.B; MoonColorBlueLabel.Text = Universe.MoonColor.B
  while Universe.MoonInclination < 0 do Universe.MoonInclination = Universe.MoonInclination + 360; end
  if Universe.MoonInclination > 359 then Universe.MoonInclination = 359; end
  MoonInclination.Value = Universe.MoonInclination
end
