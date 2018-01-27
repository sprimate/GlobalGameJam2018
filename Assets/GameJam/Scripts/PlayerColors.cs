using UnityEngine;

public class PlayerColor
{
	public const int ColorID1 = 1;
	public const int ColorID2 = 2;

	//blue
	//	public static Color PColor1 = new Color(0f, 0f, 1f, 0.5f);

	//red
	//	public static Color PColor2 = new Color(1f, 0f, 0f, 0.5f);

	//zombunny teal
	public static Color PColor1 = new Color(.04f, .93f, .95f, 0.5f);

	//zombear pink
	public static Color PColor2 = new Color(.97f, .02f, .73f, 0.5f);

	public static Color getColorForId(int id){
		switch (id) {
		case ColorID1:
			return PColor1;
		case ColorID2:
			return PColor2;
		default:
			return PColor1;
		}
	}
}