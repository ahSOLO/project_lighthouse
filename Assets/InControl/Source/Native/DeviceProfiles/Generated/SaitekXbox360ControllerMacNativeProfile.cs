// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
namespace InControl.NativeDeviceProfiles
{
	// @cond nodoc
	[Preserve, NativeInputDeviceProfile]
	public class SaitekXbox360ControllerMacNativeProfile : Xbox360DriverMacNativeProfile
	{
		public override void Define()
		{
			base.Define();

			DeviceName = "Saitek Xbox 360 Controller";
			DeviceNotes = "Saitek Xbox 360 Controller on Mac";

			Matchers = new[]
			{
				new InputDeviceMatcher
				{
					VendorID = 0x0738,
					ProductID = 0xcb02,
				},
			};
		}
	}

	// @endcond
}
