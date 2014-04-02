function return_to_menu()
	Me:Close()
	if State == GameState.Menu then
		LoadDialog("Menu/Main")
	end
	if State == GameState.Pause then
		LoadDialog("Menu/Pause")
	end
end