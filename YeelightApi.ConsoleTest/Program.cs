using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YeelightAPI;
using YeelightAPI.Events;
using YeelightAPI.Interfaces;
using YeelightAPI.Models;
using YeelightAPI.Models.ColorFlow;

namespace YeelightApi.ConsoleTest
{
    public static class Program
    {
        public static async Task Main()
        {
            try
            {
                Console.WriteLine("Choose a test mode, type 'd' for discovery mode, 's' for a static IP address : ");
                var keyInfo = Console.ReadKey();
                Console.WriteLine();

                while (keyInfo.Key != ConsoleKey.D && keyInfo.Key != ConsoleKey.S)
                {
                    Console.WriteLine($"'{keyInfo.KeyChar}' is not a valid key !");
                    Console.WriteLine("Choose a test mode, type 'd' for discovery mode, 's' for a static IP address : ");
                    keyInfo = Console.ReadKey();
                    Console.WriteLine();
                }

                if (keyInfo.Key == ConsoleKey.D)
                {
                    DeviceLocator.OnDeviceFound += (sender, arg) =>
                    {
                        WriteLineWithColor($"Device found : {arg.Device}", ConsoleColor.Blue);
                    };
                    var devices = await DeviceLocator.Discover();
                    
                    if (devices != null && devices.Count >= 1)
                    {
                        Console.WriteLine($"{devices.Count} device(s) found !");
                        using var group = new DeviceGroup(devices);
                        await group.Connect();

                        foreach (var device in group)
                        {
                            device.OnNotificationReceived += Device_OnNotificationReceived;
                            device.OnError += Device_OnError;
                        }

                        var success = true;

                        //without smooth value (sudden)
                        WriteLineWithColor("Processing tests", ConsoleColor.Cyan);
                        success &= await ExecuteTests(group);

                        //with smooth value
                        WriteLineWithColor("Processing tests with smooth effect", ConsoleColor.Cyan);
                        success &= await ExecuteTests(group, 1000);

                        if (success)
                        {
                            WriteLineWithColor("All Tests are successful", ConsoleColor.Green);
                        }
                        else
                        {
                            WriteLineWithColor("Some tests have failed", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        WriteLineWithColor("No devices Found via SSDP !", ConsoleColor.Red);
                    }
                }
                else
                {
                    Console.Write("Give a hostname or IP address to connect to the device : ");
                    var hostname = Console.ReadLine();
                    Console.WriteLine();
                    Console.Write("Give a port number (or leave empty to use default port) : ");
                    Console.WriteLine();

                    if (!int.TryParse(Console.ReadLine(), out var port))
                    {
                        port = 55443;
                    }

                    using var device = new Device(hostname, port);
                    var success = true;

                    Console.WriteLine("connecting device ...");
                    success &= await device.Connect();

                    device.OnNotificationReceived += Device_OnNotificationReceived;
                    device.OnError += Device_OnError;

                    //without smooth value (sudden)
                    WriteLineWithColor("Processing tests", ConsoleColor.Cyan);
                    success &= await ExecuteTests(device);

                    //with smooth value
                    WriteLineWithColor("Processing tests with smooth effect", ConsoleColor.Cyan);
                    success &= await ExecuteTests(device, 1000);

                    if (success)
                    {
                        WriteLineWithColor("All Tests are successful", ConsoleColor.Green);
                    }
                    else
                    {
                        WriteLineWithColor("Some tests have failed", ConsoleColor.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLineWithColor($"An error has occurred : {ex.Message}", ConsoleColor.Red);
            }

            Console.WriteLine("Press Enter to continue ;)");
            Console.ReadLine();
        }

        private static void Device_OnError(object sender, UnhandledExceptionEventArgs e)
        {
            WriteLineWithColor($"An Error occurred !! {e.ExceptionObject}", ConsoleColor.Red);
        }

        private static void Device_OnNotificationReceived(object sender, NotificationReceivedEventArgs arg)
        {
            WriteLineWithColor($"Notification received !! value : {JsonConvert.SerializeObject(arg.Result)}", ConsoleColor.DarkGray);
        }

        private static async Task<bool> ExecuteTests(IDeviceController device, int? smooth = null)
        {
            bool success = true, globalSuccess = true;
            const int delay = 1500;

            await Try(async () =>
            {
                Console.WriteLine("powering on ...");
                success = await device.SetPower(true, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("turn off ...");
                success = await device.TurnOff(smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("turn on ...");
                success = await device.TurnOn(smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("add cron ...");
                success = await device.CronAdd(15);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            if (device is IDeviceReader deviceReader)
            {
                await Try(async () =>
                {
                    Console.WriteLine("get cron ...");
                    var cronResult = await deviceReader.CronGet();
                    globalSuccess &= (cronResult != null);
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                string name = null;
                await Try(async () =>
                {
                    Console.WriteLine("getting current name ...");
                    name = (await deviceReader.GetProp(Properties.Name))?.ToString();
                    Console.WriteLine($"current name : {name}");
                });
                await Task.Delay(delay);

                await Try(async () =>
                {
                    Console.WriteLine("setting name 'test' ...");
                    success &= await deviceReader.SetName("test");
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(2000);
                });
                await Task.Delay(delay);

                await Try(async () =>
                {
                    Console.WriteLine("restoring name '{0}' ...", name);
                    success &= await deviceReader.SetName(name);
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(2000);
                });

                await Try(async () =>
                {
                    Console.WriteLine("getting all props ...");
                    Dictionary<Properties, object> result = await deviceReader.GetAllProps();
                    Console.WriteLine($"\tprops : {JsonConvert.SerializeObject(result)}");
                    await Task.Delay(2000);
                });
            }

            await Try(async () =>
            {
                Console.WriteLine("delete cron ...");
                success = await device.CronDelete();
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            if (device is IBackgroundDeviceController backgroundDevice)
            {
                await Try(async () =>
                {
                    Console.WriteLine("powering on ...");
                    success = await backgroundDevice.BackgroundSetPower(true, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("turn off ...");
                    success = await backgroundDevice.BackgroundTurnOff(smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("turn on ...");
                    success = await backgroundDevice.BackgroundTurnOn(smooth, PowerOnMode.Rgb);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting Brightness to One...");
                    success = await backgroundDevice.BackgroundSetBrightness(1, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting brightness increase...");
                    success = await backgroundDevice.BackgroundSetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Increase, YeelightAPI.Models.Adjust.AdjustProperty.Bright);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting Brightness to 100 %...");
                    success = await backgroundDevice.BackgroundSetBrightness(100, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting brightness decrease...");
                    success = await backgroundDevice.BackgroundSetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Decrease, YeelightAPI.Models.Adjust.AdjustProperty.Bright);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting Brightness to 50 %...");
                    success = await backgroundDevice.BackgroundSetBrightness(50, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting RGB color to red ...");
                    success = await backgroundDevice.BackgroundSetRgbColor(255, 0, 0, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting RGB color to green...");
                    success = await backgroundDevice.BackgroundSetRgbColor(0, 255, 0, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting color increase circle...");
                    success = await backgroundDevice.BackgroundSetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Circle, YeelightAPI.Models.Adjust.AdjustProperty.Color);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting RGB color to blue...");
                    success = await backgroundDevice.BackgroundSetRgbColor(0, 0, 255, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting HSV color to red...");
                    success = await backgroundDevice.BackgroundSetHsvColor(0, 100, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting HSV color to green...");
                    success = await backgroundDevice.BackgroundSetHsvColor(120, 100, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting HSV color to blue...");
                    success = await backgroundDevice.BackgroundSetHsvColor(240, 100, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting Color Temperature to 1700k ...");
                    success = await backgroundDevice.BackgroundSetColorTemperature(1700, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting color temperature increase ...");
                    success = await backgroundDevice.BackgroundSetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Increase, YeelightAPI.Models.Adjust.AdjustProperty.Ct);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Setting Color Temperature to 6500k ...");
                    success = await backgroundDevice.BackgroundSetColorTemperature(6500, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Starting color flow ...");
                    const int repeat = 0;
                    var flow = new ColorFlow(repeat, ColorFlowEndAction.Restore)
                    {
                        new ColorFlowRgbExpression(255, 0, 0, 1, 500),
                        new ColorFlowRgbExpression(0, 255, 0, 100, 500),
                        new ColorFlowRgbExpression(0, 0, 255, 50, 500),
                        new ColorFlowSleepExpression(2000),
                        new ColorFlowTemperatureExpression(2700, 100, 500),
                        new ColorFlowTemperatureExpression(5000, 1, 500)
                    };
                    success = await backgroundDevice.BackgroundStartColorFlow(flow);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(10 * 1000);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Stopping color flow ...");
                    success = await backgroundDevice.BackgroundStopColorFlow();
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Starting fluent color flow ...");
                    var fluentFlow = await backgroundDevice.BackgroundFlow()
                        .RgbColor(255, 0, 0, 50, 1000)
                        .Sleep(2000)
                        .RgbColor(0, 255, 0, 50, 1000)
                        .Sleep(2000)
                        .RgbColor(0, 0, 255, 50, 1000)
                        .Sleep(2000)
                        .Temperature(2700, 100, 1000)
                        .Sleep(2000)
                        .Temperature(6500, 100, 1000)
                        .Play(ColorFlowEndAction.Keep);

                    await fluentFlow.StopAfter(5000);

                    WriteLineWithColor("Color flow ended", ConsoleColor.DarkCyan);
                });

                await Try(async () =>
                {
                    Console.WriteLine("adjust brightness ++");
                    success = await backgroundDevice.BackgroundAdjustBright(50, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("adjust brightness --");
                    success = await backgroundDevice.BackgroundAdjustBright(-50, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("adjust color ++");
                    success = await backgroundDevice.BackgroundAdjustColor(50, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("adjust color --");
                    success = await backgroundDevice.BackgroundAdjustColor(-50, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("adjust color temperature ++");
                    success = await backgroundDevice.BackgroundAdjustColorTemperature(50, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("adjust color temperature --");
                    success = await backgroundDevice.BackgroundAdjustColorTemperature(-50, smooth);
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });

                await Try(async () =>
                {
                    Console.WriteLine("Toggling bulb state...");
                    success = await backgroundDevice.BackgroundToggle();
                    globalSuccess &= success;
                    WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                    await Task.Delay(delay);
                });
            }

            await Try(async () =>
            {
                Console.WriteLine("Setting Brightness to One...");
                success = await device.SetBrightness(1, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting brightness increase...");
                success = await device.SetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Increase, YeelightAPI.Models.Adjust.AdjustProperty.Bright);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting Brightness to 100 %...");
                success = await device.SetBrightness(100, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting brightness decrease...");
                success = await device.SetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Decrease, YeelightAPI.Models.Adjust.AdjustProperty.Bright);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting Brightness to 50 %...");
                success = await device.SetBrightness(50, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting RGB color to red ...");
                success = await device.SetRgbColor(255, 0, 0, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting RGB color to green...");
                success = await device.SetRgbColor(0, 255, 0, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting color increase circle...");
                success = await device.SetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Circle, YeelightAPI.Models.Adjust.AdjustProperty.Color);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting RGB color to blue...");
                success = await device.SetRgbColor(0, 0, 255, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting HSV color to red...");
                success = await device.SetHsvColor(0, 100, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting HSV color to green...");
                success = await device.SetHsvColor(120, 100, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting HSV color to blue...");
                success = await device.SetHsvColor(240, 100, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting Color Temperature to 1700k ...");
                success = await device.SetColorTemperature(1700, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting color temperature increase ...");
                success = await device.SetAdjust(YeelightAPI.Models.Adjust.AdjustAction.Increase, YeelightAPI.Models.Adjust.AdjustProperty.Ct);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Setting Color Temperature to 6500k ...");
                success = await device.SetColorTemperature(6500, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Starting color flow ...");
                const int repeat = 0;
                var flow = new ColorFlow(repeat, ColorFlowEndAction.Restore)
                {
                    new ColorFlowRgbExpression(255, 0, 0, 1, 500),
                    new ColorFlowRgbExpression(0, 255, 0, 100, 500),
                    new ColorFlowRgbExpression(0, 0, 255, 50, 500),
                    new ColorFlowSleepExpression(2000),
                    new ColorFlowTemperatureExpression(2700, 100, 500),
                    new ColorFlowTemperatureExpression(5000, 1, 500)
                };
                success = await device.StartColorFlow(flow);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(10 * 1000);
            });

            await Try(async () =>
            {
                Console.WriteLine("Stopping color flow ...");
                success = await device.StopColorFlow();
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("Starting fluent color flow ...");
                var fluentFlow = await device.Flow()
                    .RgbColor(255, 0, 0, 50, 1000)
                    .Sleep(2000)
                    .RgbColor(0, 255, 0, 50, 1000)
                    .Sleep(2000)
                    .RgbColor(0, 0, 255, 50, 1000)
                    .Sleep(2000)
                    .Temperature(2700, 100, 1000)
                    .Sleep(2000)
                    .Temperature(6500, 100, 1000)
                    .Play(ColorFlowEndAction.Keep);

                await fluentFlow.StopAfter(5000);

                WriteLineWithColor("Color flow ended", ConsoleColor.DarkCyan);
            });

            await Try(async () =>
            {
                Console.WriteLine("adjust brightness ++");
                success=await device.AdjustBright(50, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("adjust brightness --");
                success=await device.AdjustBright(-50, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("adjust color ++");
                success=await device.AdjustColor(50, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("adjust color --");
                success=await device.AdjustColor(-50, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("adjust color temperature ++");
                success=await device.AdjustColorTemperature(50, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            await Try(async () =>
            {
                Console.WriteLine("adjust color temperature --");
                success=await device.AdjustColorTemperature(-50, smooth);
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });


            await Try(async () =>
            {
                Console.WriteLine("Toggling bulb state...");
                success = await device.Toggle();
                globalSuccess &= success;
                WriteLineWithColor($"command success : {success}", ConsoleColor.DarkCyan);
                await Task.Delay(delay);
            });

            if (success)
            {
                WriteLineWithColor("Tests are successful", ConsoleColor.DarkGreen);
            }
            else
            {
                WriteLineWithColor("Tests failed", ConsoleColor.DarkRed);
            }

            return globalSuccess;
        }

        private static async Task Try(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                WriteLineWithColor(ex.Message, ConsoleColor.Magenta);
            }
        }

        private static void WriteLineWithColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}