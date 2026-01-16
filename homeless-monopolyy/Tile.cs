using Godot;

public partial class Tile : Node2D
{
	[Export] public string HouseName { get; set; } = "HouseName";

	[ExportGroup("Purchase")]
	[Export] public int Price { get; set; } = 100;
	[Export] public int Mortgage { get; set; } = 50;

	[ExportGroup("Building costs")]
	[Export] public int HouseCost { get; set; } = 200;
	[Export] public int HotelCost { get; set; } = 300;

	[ExportGroup("Rent")]
	[Export] public int BaseRent { get; set; } = 2;

	private void ButtonPressed()
	{
		GD.Print(
			$"{HouseName}\n" +
			$"Price: {Price}\n" +
			$"Mortgage: {Mortgage}\n" +
			$"HouseCost: {HouseCost}\n" +
			$"HotelCost: {HotelCost}\n" +
			$"BaseRent: {BaseRent}"
		);
	}
}
