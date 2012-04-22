-- Import .NET enumerations
luanet.load_assembly("OmegaEngine");
Quality = luanet.import_type("OmegaEngine.Quality");
WaterEffectsType = luanet.import_type("OmegaEngine.WaterEffectsType");

-- Default to English language, provide German as an option
if Settings.General.Language == "de" then
  Language.SelectedItem = "Deutsch";
end;

PlayMusic.Checked = Settings.Sound.PlayMusic;
InvertMouse.Checked = Settings.Controls.InvertMouse;

-- Check for each supported resolution if the graphics card can handle it and if so add it to the list
function ResolutionHelper(width, height)
  if Engine:CheckResolution(width, height) then Resolution.Items:Add(width.."x"..height) end;
end;
ResolutionHelper(800, 600); ResolutionHelper(1024, 600); ResolutionHelper(1024, 768); ResolutionHelper(1152, 864);
ResolutionHelper(1280, 720); ResolutionHelper(1280, 768); ResolutionHelper(1280, 800); ResolutionHelper(1280, 960); ResolutionHelper(1280, 1024);
ResolutionHelper(1360, 768); ResolutionHelper(1360, 1024); ResolutionHelper(1366, 768); ResolutionHelper(1400, 1050);
ResolutionHelper(1440, 900); ResolutionHelper(1440, 960); ResolutionHelper(1440, 1024); ResolutionHelper(1440, 1080);
ResolutionHelper(1600, 768); ResolutionHelper(1600, 900); ResolutionHelper(1600, 1024); ResolutionHelper(1600, 1200);
ResolutionHelper(1680, 1050); ResolutionHelper(1792, 1344); ResolutionHelper(1800, 1440); ResolutionHelper(1856, 1392);
ResolutionHelper(1900, 1200); ResolutionHelper(1920, 1080); ResolutionHelper(1920, 1200); ResolutionHelper(1920, 1400); ResolutionHelper(1920, 1440);
ResolutionHelper(2048, 1152); ResolutionHelper(2048, 1536);

-- Reselect the previously chosen resolution
if Resolution.Items:Contains(Settings.Display.ResolutionText) then
  Resolution.SelectedItem = Settings.Display.ResolutionText;
end;

-- Check for each supported AA level if the graphics card can handle it and if so add it to the list
for level=2,16,2 do
  if Engine:CheckAA(level) then AntiAliasing.Items:Add(level.."x") end;
end;

-- Reselect the previously chosen AA level
if Settings.Display.AntiAliasing ~= 0 and AntiAliasing.Items:Contains(Settings.Display.AntiAliasingText) then
  AntiAliasing.SelectedItem = Settings.Display.AntiAliasingText;
end;

Anisotropic.Enabled = Engine.SupportsAnisotropic;
Anisotropic.Checked = Settings.Graphics.Anisotropic;
Fullscreen.Checked = Settings.Display.Fullscreen;
VSync.Checked = Settings.Display.VSync;

PostScreenEffects.Enabled = Engine.SupportsPerPixelEffects;
PostScreenEffects.Checked = Settings.Graphics.PostScreenEffects;
Shadows.Enabled = Engine.SupportsPerPixelEffects;
Shadows.Checked = Settings.Graphics.Shadows;
TerrainDoubleSampling.Enabled = Engine.SupportsDoubleSampling;
TerrainDoubleSampling.Checked = Settings.Graphics.DoubleSampling;

WaterEffectsLabel.Enabled = Engine.SupportsPerPixelEffects;
WaterEffects.Enabled = WaterEffectsLabel.Enabled;
WaterEffects.SelectedItem = "["..Settings.Graphics.WaterEffects:ToString().."]";
ParticleSystemQuality.SelectedItem = "["..Settings.Graphics.ParticleSystemQuality:ToString().."]";

-- Only allow the user to change the language in the main menu
if State ~= GameState.Menu then
  LanguageLabel.Enabled = false;
  Language.Enabled = false;
end;
