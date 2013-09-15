-- Import .NET enumerations
luanet.load_assembly("Common")
Quality = luanet.import_type("Common.Values.Quality")
WaterEffectsType = luanet.import_type("Common.Values.WaterEffectsType")

-- Default to English language, provide German as an option
if Settings.General.Language == "de" then
  Language.SelectedItem = "Deutsch"
end

PlayMusic.Checked = Settings.Sound.PlayMusic
InvertMouse.Checked = Settings.Controls.InvertMouse

-- Check for each supported resolution if the graphics card can handle it and if so add it to the list
function resolution_helper(width, height)
  if Engine.Capabilities:CheckResolution(width, height) then Resolution.Items:Add(width.."x"..height); end
end
resolution_helper(800, 600); resolution_helper(1024, 600); resolution_helper(1024, 768); resolution_helper(1152, 864);
resolution_helper(1280, 720); resolution_helper(1280, 768); resolution_helper(1280, 800); resolution_helper(1280, 960); resolution_helper(1280, 1024);
resolution_helper(1360, 768); resolution_helper(1360, 1024); resolution_helper(1366, 768); resolution_helper(1400, 1050);
resolution_helper(1440, 900); resolution_helper(1440, 960); resolution_helper(1440, 1024); resolution_helper(1440, 1080);
resolution_helper(1600, 768); resolution_helper(1600, 900); resolution_helper(1600, 1024); resolution_helper(1600, 1200);
resolution_helper(1680, 1050); resolution_helper(1792, 1344); resolution_helper(1800, 1440); resolution_helper(1856, 1392);
resolution_helper(1900, 1200); resolution_helper(1920, 1080); resolution_helper(1920, 1200); resolution_helper(1920, 1400); resolution_helper(1920, 1440);
resolution_helper(2048, 1152); resolution_helper(2048, 1536);

-- Reselect the previously chosen resolution
if Resolution.Items:Contains(Settings.Display.ResolutionText) then
  Resolution.SelectedItem = Settings.Display.ResolutionText
end

-- Check for each supported AA level if the graphics card can handle it and if so add it to the list
for level=2,16,2 do
  if Engine.Capabilities:CheckAA(level) then AntiAliasing.Items:Add(level.."x"); end
end

-- Reselect the previously chosen AA level
if Settings.Display.AntiAliasing ~= 0 and AntiAliasing.Items:Contains(Settings.Display.AntiAliasingText) then
  AntiAliasing.SelectedItem = Settings.Display.AntiAliasingText
end

Anisotropic.Enabled = Engine.Capabilities.Anisotropic
Anisotropic.Checked = Settings.Graphics.Anisotropic
Fullscreen.Checked = Settings.Display.Fullscreen
VSync.Checked = Settings.Display.VSync

PostScreenEffects.Enabled = Engine.Capabilities.PerPixelEffects
PostScreenEffects.Checked = Settings.Graphics.PostScreenEffects
Shadows.Enabled = Engine.Capabilities.PerPixelEffects
Shadows.Checked = Settings.Graphics.Shadows
TerrainDoubleSampling.Enabled = Engine.Capabilities.DoubleSampling
TerrainDoubleSampling.Checked = Settings.Graphics.DoubleSampling

WaterEffectsLabel.Enabled = Engine.Capabilities.PerPixelEffects
WaterEffects.Enabled = WaterEffectsLabel.Enabled
WaterEffects.SelectedItem = "["..Settings.Graphics.WaterEffects:ToString().."]"
ParticleSystemQuality.SelectedItem = "["..Settings.Graphics.ParticleSystemQuality:ToString().."]"

-- Only allow the user to change the language in the main menu
if State ~= GameState.Menu then
  LanguageLabel.Enabled = false
  Language.Enabled = false
end
