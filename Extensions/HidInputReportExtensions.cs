using Windows.Gaming.Input;

namespace Windows.Devices.HumanInterfaceDevice
{
    public static class HidInputReportExtensions
    {
        public static GamepadReading ToGamepadReading(this HidInputReport report)
        {
            var buttons = GamepadButtons.None;
            if (report.GetBooleanControl(9, 1).IsActive)
                buttons |= GamepadButtons.A;
            if (report.GetBooleanControl(9, 2).IsActive)
                buttons |= GamepadButtons.B;
            if (report.GetBooleanControl(9, 3).IsActive)
                buttons |= GamepadButtons.X;
            if (report.GetBooleanControl(9, 4).IsActive)
                buttons |= GamepadButtons.Y;

            var leftThumbstickX = report.GetNumericControl(1, 0x30).Value;
            var leftThumbstickY = report.GetNumericControl(1, 0x31).Value;

            return new GamepadReading
            {
                Buttons = buttons,
                LeftThumbstickX = (leftThumbstickX - 32768) / 32768.0,
                LeftThumbstickY = (leftThumbstickY - 32768) / -32768.0
            };
        }
    }
}
