using System.Management;

namespace USBConnectionDebugger
{
    internal abstract class Program
    {
        private static void Main()
        {
            Console.WriteLine("USB Device Connection Monitor");
            Console.WriteLine("Press Ctrl+C to exit.\n");

            // Create WMI event watchers for USB device arrival and removal
            ManagementEventWatcher arrivalWatcher = new ManagementEventWatcher();
            ManagementEventWatcher removalWatcher = new ManagementEventWatcher();

            try
            {
                // WMI query for device arrival events (creation)
                WqlEventQuery arrivalQuery = new WqlEventQuery(
                    "__InstanceCreationEvent",
                    TimeSpan.FromSeconds(1),
                    "TargetInstance isa \"Win32_PnPEntity\""
                );
                arrivalWatcher.EventArrived += OnDeviceArrived;
                arrivalWatcher.Query = arrivalQuery;
                arrivalWatcher.Start();

                // WMI query for device removal events (deletion)
                WqlEventQuery removalQuery = new WqlEventQuery(
                    "__InstanceDeletionEvent",
                    TimeSpan.FromSeconds(1),
                    "TargetInstance isa \"Win32_PnPEntity\""
                );
                removalWatcher.EventArrived += OnDeviceRemoved;
                removalWatcher.Query = removalQuery;
                removalWatcher.Start();

                // Keep the application running indefinitely.
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Clean up watchers when exiting
                arrivalWatcher.Stop();
                removalWatcher.Stop();
                arrivalWatcher.Dispose();
                removalWatcher.Dispose();
            }
        }

        static void OnDeviceArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[CONNECTED]");
                Console.WriteLine($"Timestamp   : {DateTime.Now}");
                Console.WriteLine($"Device Name : {instance["Name"] ?? "Unknown"}");
                Console.WriteLine($"Device ID   : {instance["DeviceID"] ?? "N/A"}");
                Console.WriteLine($"Manufacturer: {instance["Manufacturer"] ?? "Unknown"}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing device arrival: {ex.Message}");
            }
        }

        static void OnDeviceRemoved(object sender, EventArrivedEventArgs e)
        {
            try
            {
                ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[DISCONNECTED]");
                Console.WriteLine($"Timestamp   : {DateTime.Now}");
                Console.WriteLine($"Device Name : {instance["Name"] ?? "Unknown"}");
                Console.WriteLine($"Device ID   : {instance["DeviceID"] ?? "N/A"}");
                Console.WriteLine($"Manufacturer: {instance["Manufacturer"] ?? "Unknown"}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing device removal: {ex.Message}");
            }
        }
    }
}