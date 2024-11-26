using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace AdminHomeSync.Components.Services
{
    public class DeviceService
    {
        private readonly FirebaseClient _firebaseClient;

        public DeviceService()
        {
            _firebaseClient = new FirebaseClient("https://homesync-3be92-default-rtdb.firebaseio.com/");
        }

        public async Task<List<UserDevice>> GetDevicesDataAsync()
        {
            try
            {
                var devices = await _firebaseClient
                    .Child("smartHomeControl/devices")
                    .OnceAsync<DeviceControl>();

                var users = await _firebaseClient
                    .Child("users/users")
                    .OnceAsync<AppUser>();

                var userDevices = new List<UserDevice>();

                foreach (var device in devices)
                {
                    var userId = device.Object.User;
                    var user = users.FirstOrDefault(u => u.Key == userId)?.Object;
                    bool isUserConnected = device.Object.IsConnected;

                    int deviceId = 0;
                    string deviceKeyPart = device.Key.Split('_')[1];
                    int.TryParse(deviceKeyPart, out deviceId);

                    userDevices.Add(new UserDevice
                    {
                        Id = deviceId,
                        // If the device is connected, show the user's name; otherwise, show "None"
                        Name = isUserConnected ? $"{user?.FirstName} {user?.LastName}" : "None",
                        Lights = new Device
                        {
                            Name = "Light",
                            Type = "Arduino",
                            IsConnected = device.Object.Controls.ContainsKey("led") && device.Object.Controls["led"] == "on"
                        },
                        Fan = new Device
                        {
                            Name = "Fan",
                            Type = "Arduino",
                            IsConnected = device.Object.Controls.ContainsKey("fan") && device.Object.Controls["fan"] == "on"
                        },
                        MotionSensor = new Device
                        {
                            Name = "Motion Sensor",
                            Type = "Arduino",
                            IsConnected = device.Object.Controls.ContainsKey("pir") && device.Object.Controls["pir"] == "on"
                        }
                    });
                }

                return userDevices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching device data: {ex.Message}");
                throw new Exception("There was an error fetching the device data.");
            }
        }
    }

    public class DeviceControl
    {
        public Dictionary<string, string> Controls { get; set; } = new Dictionary<string, string>();
        public bool IsConnected { get; set; }
        public string PairingCode { get; set; }
        public SensorData SensorData { get; set; }
        public string Status { get; set; }
        public string User { get; set; }
    }

    public class SensorData
    {
        public bool MotionDetected { get; set; }
    }

    public class AppUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Birthdate { get; set; }
        public string Role { get; set; }
        public string Sex { get; set; }
    }

    public class UserDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Device Lights { get; set; }
        public Device Fan { get; set; }
        public Device MotionSensor { get; set; }
    }

    public class Device
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsConnected { get; set; }
    }
}

