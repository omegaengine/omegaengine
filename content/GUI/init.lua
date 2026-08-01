function return_to_menu()
	Me:Close()
	if IsMainMenu then
		LoadDialog("MainMenu")
	else
		LoadDialog("PauseMenu")
	end
end

function msgbox(message)
	LoadDialog("MsgBox/OK", {Modal = true, Centered = true}, {message = message})
end

function msgbox_yesno(message, on_yes)
	LoadDialog("MsgBox/YesNo", {Modal = true, Centered = true}, {message = message, on_yes = on_yes})
end
