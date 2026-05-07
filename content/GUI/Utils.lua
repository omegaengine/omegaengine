function return_to_menu()
	Me:Close()
	if IsMainMenu then
		LoadDialog("MainMenu")
	end
	if IsPauseMenu then
		LoadDialog("PauseMenu")
	end
end

function msgbox(message)
	local dialog = LoadModalDialogCentered("MsgBox/OK")
	dialog:GetControl("Message").Text = message
end

function msgbox_yesno(message, onYes)
	local dialog = LoadModalDialogCentered("MsgBox/YesNo")
	dialog:GetControl("Message").Text = message
	dialog:GetControl("Yes").OnClick = "Me:Close(); " .. onYes
end
