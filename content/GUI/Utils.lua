function return_to_menu()
	Me:Close()
	if State == GameState.Menu then
		LoadDialog("MainMenu")
	end
	if State == GameState.Pause then
		LoadDialog("PauseMenu")
	end
end

function msgbox(message, callback)
	local dialog = LoadModalDialog("MsgBox")
	local messageLabel = dialog:GetControl("Message")
	messageLabel.Text = message
	
	local yesButton = dialog:GetControl("YesButton")
	yesButton.OnClick = function()
		dialog:Close()
		if callback then
			callback()
		end
	end
end