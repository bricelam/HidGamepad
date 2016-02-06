using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;

namespace Windows.Gaming.Input
{
    public sealed class HidGamepad : IDisposable
    {
        private static EventWaitHandle _gamepadsWaitHandle = new ManualResetEvent(false);
        private static readonly DeviceWatcher _watcher;
        private static readonly IDictionary<string, HidGamepad> _gamepads = new Dictionary<string, HidGamepad>();

        private readonly DeviceInformation _deviceInformation;
        GamepadReading _currentReading;

        static HidGamepad()
        {
            var deviceSelector = HidDevice.GetDeviceSelector(0x01, 0x05);
            _watcher = DeviceInformation.CreateWatcher(deviceSelector);
            _watcher.Added += HandleDeviceAdded;
            _watcher.Updated += HandleDeviceUpdated;
            _watcher.Removed += HandleDeviceRemoved;
            _watcher.EnumerationCompleted += HandleEnumerationCompleted;
            _watcher.Start();
        }

        private static async void HandleDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            var device = await HidDevice.FromIdAsync(args.Id, FileAccessMode.Read);
            var gamepad = new HidGamepad(args, device);

            _gamepads.Add(args.Id, gamepad);
            GamepadAdded?.Invoke(sender, gamepad);
        }

        private static void HandleDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
            => _gamepads[args.Id]._deviceInformation.Update(args);

        private static void HandleDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            var gamepad = _gamepads[args.Id];

            _gamepads.Remove(args.Id);
            GamepadRemoved?.Invoke(sender, gamepad);
        }

        private static void HandleEnumerationCompleted(DeviceWatcher sender, object args)
        {
            _gamepadsWaitHandle.Set();
            sender.EnumerationCompleted -= HandleEnumerationCompleted;
        }

        HidDevice _device;

        private HidGamepad(DeviceInformation deviceInformation, HidDevice device)
        {
            _deviceInformation = deviceInformation;

            _device = device;
            _device.InputReportReceived += HandleInputReportRecieved;
        }

        public static event EventHandler<HidGamepad> GamepadAdded;
        public static event EventHandler<HidGamepad> GamepadRemoved;

        public static IReadOnlyList<HidGamepad> Gamepads
        {
            get
            {
                _gamepadsWaitHandle.WaitOne();

                return _gamepads.Values.ToList();
            }
        }

        public GamepadReading GetCurrentReading()
            => _currentReading;

        void HandleInputReportRecieved(HidDevice sender, HidInputReportReceivedEventArgs args)
            => _currentReading = args.Report.ToGamepadReading();

        public void Dispose()
        {
            if (_device != null)
                _device.Dispose();
        }
    }
}
