namespace AdminHomeSync.Components.Services
{
    public class DeviceService : IDeviceService
    {
        // Fetches user devices
        public async Task<List<UserDevice>> GetUserDevicesAsync()
        {
            //mock data palitan later
            await Task.Delay(100);

            return new List<UserDevice>
            {
                new UserDevice
                {
                    Id = 1,
                    Name = "Jobert Batumbakal",
                    Lights = new Device { Name = "Light", Type = "Arduino", Pin = 1, IsConnected = true },
                    Fan = new Device { Name = "Fan", Type = "Arduino", Pin = 2, IsConnected = false },
                    MotionSensor = new Device { Name = "Motion Sensor", Type = "Arduino", Pin = 3, IsConnected = true }
                },
                new UserDevice
                {
                    Id = 2,
                    Name = "Arman",
                    Lights = new Device { Name = "Light", Type = "Arduino", Pin = 4, IsConnected = true },
                    Fan = new Device { Name = "Fan", Type = "Arduino", Pin = 5, IsConnected = true },
                    MotionSensor = new Device { Name = "Motion Sensor", Type = "Arduino", Pin = 6, IsConnected = true }
                },
                new UserDevice
                {
                    Id = 3,
                    Name = "Meow M. Meow",
                    Lights = new Device { Name = "Light", Type = "Arduino", Pin = 7, IsConnected = true },
                    Fan = new Device { Name = "Fan", Type = "Arduino", Pin = 8, IsConnected = true },
                    MotionSensor = new Device { Name = "Motion Sensor", Type = "Arduino", Pin = 9, IsConnected = false }
                },

                new UserDevice
                {
                    Id = 4,
                    Name = "Meow M. Meow",
                    Lights = new Device { Name = "Light", Type = "Arduino", Pin = 7, IsConnected = true },
                    Fan = new Device { Name = "Fan", Type = "Arduino", Pin = 8, IsConnected = true },
                    MotionSensor = new Device { Name = "Motion Sensor", Type = "Arduino", Pin = 9, IsConnected = false }
                },

                new UserDevice
                {
                    Id = 5,
                    Name = "Meow M. Meow",
                    Lights = new Device { Name = "Light", Type = "Arduino", Pin = 7, IsConnected = true },
                    Fan = new Device { Name = "Fan", Type = "Arduino", Pin = 8, IsConnected = true },
                    MotionSensor = new Device { Name = "Motion Sensor", Type = "Arduino", Pin = 9, IsConnected = false }
                },

            };
        }
    }

    //Device connected to the Arduino
    public class Device
    {
        public string Name { get; set; } = "Unknown";
        public string Type { get; set; } = "Unknown";
        public int Pin { get; set; } = 0;
        public bool IsConnected { get; set; } = false;
    }

    // Arduino device owned by a user
    public class UserDevice
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Unnamed";
        public Device Lights { get; set; } = new Device();
        public Device Fan { get; set; } = new Device();
        public Device MotionSensor { get; set; } = new Device();
    }

    // For fetching user device data
    public interface IDeviceService
    {
        Task<List<UserDevice>> GetUserDevicesAsync();
    }
}
