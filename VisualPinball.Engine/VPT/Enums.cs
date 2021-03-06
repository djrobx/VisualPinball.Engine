namespace VisualPinball.Engine.VPT
{
	public static class BackglassIndex
	{
		public const int Desktop = 0;
		public const int Fullscreen = 1;
		public const int FullSingleScreen = 2;
	}

	public static class ItemType
	{
		public const int Surface = 0;
		public const int Flipper = 1;
		public const int Timer = 2;
		public const int Plunger = 3;
		public const int Textbox = 4;
		public const int Bumper = 5;
		public const int Trigger = 6;
		public const int Light = 7;
		public const int Kicker = 8;
		public const int Decal = 9;
		public const int Gate = 10;
		public const int Spinner = 11;
		public const int Ramp = 12;
		public const int Table = 13;
		public const int LightCenter = 14;
		public const int DragPoint = 15;
		public const int Collection = 16;
		public const int DispReel = 17;
		public const int LightSeq = 18;
		public const int Primitive = 19;
		public const int Flasher = 20;
		public const int Rubber = 21;
		public const int HitTarget = 22;
		public const int Count = 23;
		public const uint Invalid = 0xffffffff;
	}

	public static class LightStatus
	{
		public const int LightStateOff = 0;
		public const int LightStateOn = 1;
		public const int LightStateBlinking = 2;
	}

	public static class DecalType
	{
		public const int DecalText = 0;
		public const int DecalImage = 1;
	}

	public static class SizingType
	{
		public const int AutoSize = 0;
		public const int AutoWidth = 1;
		public const int ManualSize = 2;
	}

	public static class ImageAlignment
	{
		public const int ImageAlignWorld = 0;
		public const int ImageAlignTopLeft = 1;
		public const int ImageAlignCenter = 2;
	}

	public static class Filters
	{
		public const int Filter_None = 0;
		public const int Filter_Additive = 1;
		public const int Filter_Overlay = 2;
		public const int Filter_Multiply = 3;
		public const int Filter_Screen = 4;
	}

	public static class GateType
	{
		public const int GateWireW = 1;
		public const int GateWireRectangle = 2;
		public const int GatePlate = 3;
		public const int GateLongPlate = 4;
	}

	public static class TargetType
	{
		public const int DropTargetBeveled = 1;
		public const int DropTargetSimple = 2;
		public const int HitTargetRound = 3;
		public const int HitTargetRectangle = 4;
		public const int HitFatTargetRectangle = 5;
		public const int HitFatTargetSquare = 6;
		public const int DropTargetFlatSimple = 7;
		public const int HitFatTargetSlim = 8;
		public const int HitTargetSlim = 9;
	}

	public static class KickerType
	{
		public const int KickerInvisible = 0;
		public const int KickerHole = 1;
		public const int KickerCup = 2;
		public const int KickerHoleSimple = 3;
		public const int KickerWilliams = 4;
		public const int KickerGottlieb = 5;
		public const int KickerCup2 = 6;
	}

	public static class PlungerType
	{
		public const int PlungerTypeModern = 1;
		public const int PlungerTypeFlat = 2;
		public const int PlungerTypeCustom = 3;
	}

	public static class RampImageAlignment
	{
		public const int ImageModeWorld = 0;
		public const int ImageModeWrap = 1;
	}

	public static class RampType
	{
		public const int RampTypeFlat = 0;
		public const int RampType4Wire = 1;
		public const int RampType2Wire = 2;
		public const int RampType3WireLeft = 3;
		public const int RampType3WireRight = 4;
		public const int RampType1Wire = 5;
	}

	public static class TextAlignment
	{
		public const int TextAlignLeft = 0;
		public const int TextAlignCenter = 1;
		public const int TextAlignRight = 2;
	}

	public static class TriggerShape
	{
		public const int TriggerNone = 0;
		public const int TriggerWireA = 1;
		public const int TriggerStar = 2;
		public const int TriggerWireB = 3;
		public const int TriggerButton = 4;
		public const int TriggerWireC = 5;
		public const int TriggerWireD = 6;
	}

	public static class SoundOutTypes
	{
		public const int Table = 0;
		public const int Backglass = 1;
	}

}
