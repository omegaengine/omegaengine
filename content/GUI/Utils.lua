function return_to_menu()
	Me:Close()
	if State == GameState.Menu then
		LoadDialog("MainMenu")
	end
	if State == GameState.Pause then
		LoadDialog("PauseMenu")
	end
end