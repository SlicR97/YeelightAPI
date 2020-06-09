using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using YeelightAPI.Models;
using System.Diagnostics;

namespace YeelightAPI.UnitTests
{
    public class YeelightUnitTest
    {
        private readonly IConfigurationRoot _config;
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public YeelightUnitTest(Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            _config = new ConfigurationBuilder()
             .AddJsonFile("config.json")
             .Build();

            _output = testOutputHelper;
        }

        [Fact]
        public async Task Discovery_should_find_devices()
        {
            var expectedDevicesCount = GetConfig<int>("discovery_devices_expected");
            var devices = await DeviceLocator.Discover();

            Assert.Equal(expectedDevicesCount, devices?.Count);
        }

        [Fact]
        public async Task Discovery_should_not_last_long()
        {
            var sw = Stopwatch.StartNew();
            _ = await DeviceLocator.Discover();
            sw.Stop();

            Assert.InRange(sw.ElapsedMilliseconds, 0, 1500);
        }

        [Fact]
        public async Task Device_should_turn_on_and_turn_off()
        {
            var testedDevice = await GetRandomConnectedDevice();
            await testedDevice.TurnOn();
            Assert.Equal("on", await testedDevice.GetProp(Properties.Power));
            await testedDevice.TurnOff();
            Assert.Equal("off", await testedDevice.GetProp(Properties.Power));
        }

        [Fact]
        public async Task Device_should_change_rgb_color_to_red() => await DoWithRandomDevice(async (device) =>
        {
            await device.SetRgbColor(255, 0, 0);
            Assert.Equal((255 << 16).ToString(), await device.GetProp(Properties.Rgb));
        }, Methods.SetRgbColor);

        [Fact]
        public async Task Device_should_change_hsv_color_to_red() => await DoWithRandomDevice(async (device) =>
        {
            await device.SetHsvColor(0, 100);
            Assert.Equal((255 << 16).ToString(), await device.GetProp(Properties.Rgb));

        }, Methods.SetHsvColor);

        [Fact]
        public async Task Device_should_change_brightness() => await DoWithRandomDevice(async (device) =>
        {
            await device.SetBrightness(52);
            Assert.Equal(52, await device.GetProp(Properties.Bright));

        }, Methods.SetBrightness);

        [Fact]
        public async Task Device_should_change_color_temperature() => await DoWithRandomDevice(async (device) =>
        {
            await device.SetColorTemperature(4654);
            Assert.Equal(4654, await device.GetProp(Properties.Ct));

        }, Methods.SetBrightness);

        private async Task DoWithRandomDevice(Action<Device> a, Methods? supportedMethod = null)
        {
            var testedDevice = await GetRandomConnectedDevice(supportedMethod);
            await testedDevice.TurnOn();

            a?.Invoke(testedDevice);

            await testedDevice.TurnOff();
        }

        private async Task<Device> GetRandomConnectedDevice(Methods? supportedMethod = null)
        {
            var devices = (await DeviceLocator.Discover()).FindAll(
                discoveredDevice => !supportedMethod.HasValue || discoveredDevice.SupportedOperations.Contains(supportedMethod.Value)
            );

            Assert.NotEmpty(devices);

            var randomIndex = new Random().Next(0, devices.Count);
            var device = devices.ElementAt(randomIndex);
            _output.WriteLine($"Used device : {device}");
            await device.Connect();
            return device;
        }

        private T GetConfig<T>(string key)
        {
            var t = typeof(T);
            var value = _config[key];

            var converter = TypeDescriptor.GetConverter(t);
            try
            //if (value != null && converter.CanConvertTo(t) && converter.CanConvertFrom(typeof(string)))
            {
                return (T)converter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot convert '{value}' (key: {key}) to {t}", ex);
            }
        }
    }
}
