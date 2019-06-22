enum BuildOptionType {
	DIRECTIONAL_TILE,
	ENTRANCE,
	EXIT	
}

static func get_build_option_from_button_name(name):
	if name == "BOptDirectionalTileButton":
		return BuildOptionType.DIRECTIONAL_TILE
	elif name == "BOptEntranceButton":
		return BuildOptionType.ENTRANCE
	elif name == "BOptExitButton":
		return BuildOptionType.EXIT
	else:
		print("Could not convert " + name + " to build option")