using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ParentalControls.Common
{
    [Serializable]
    public class AlarmsFile : ISerializable
    {

        HashSet<Alarm> _Alarms = new HashSet<Alarm>();

        public bool IsValidForSaving()
        {
            return String.IsNullOrWhiteSpace(FileName);
        }

        public string FileName
        {
            get;
            set;
        }

        public ICollection<Alarm> Alarms
        {
            get 
            {
                Alarm[] alarms = new Alarm[_Alarms.Count];
                _Alarms.CopyTo(alarms);
                return alarms;
            }
        }

        public void Change(string name, Alarm newalarm)
        {
            Alarm old = default(Alarm);
            foreach (Alarm alarm in _Alarms)
            {
                if (alarm.Name == name)
                {
                    old = alarm;
                    break;
                }
            }

            _Alarms.Remove(old);
            _Alarms.Add(newalarm);
        }

        public void Add(Alarm alarm)
        {
            _Alarms.Add(alarm);
        }

        public void Add(string name, Time alarmTime, DayOfWeek days)
        {
            Add(name, alarmTime, days, true);
        }

        public void Add(string name, Time alarmTime, DayOfWeek days, bool enabled)
        {
            Add(new Alarm(name, alarmTime, days, enabled));
        }

        /// <summary>
        /// Saves to file specified by FileName.
        /// </summary>
        public void Save()
        {
            using(FileStream stream = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                BinaryFormatter binary = new BinaryFormatter();
                binary.Serialize(stream, this);
            }
        }

        /// <summary>
        /// Loads and returns a AlarmsFile.
        /// </summary>
        /// <param name="filename">File to load</param>
        /// <returns>AlarmsFile with FileName.</returns>
        public static AlarmsFile Load(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                try
                {
                    BinaryFormatter binary = new BinaryFormatter();
                    return (AlarmsFile)binary.Deserialize(stream);
                }
                catch 
                { 
                    AlarmsFile file = new AlarmsFile();
                    file.FileName = filename;
                    return file;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Alarm[] alarms = _Alarms.ToArray();
            for (int i = 0; i < _Alarms.Count;i++)
            {
                info.AddValue(i.ToString(), alarms[i]);
            }
        }
    }

    /// <summary>
    /// Represents a time.
    /// </summary>
    [Serializable]
    public struct Time
    {

        public int Hour;
        public int Minutes;
        public int Seconds;

    }

    [Serializable]
    public struct Alarm
    {

        public string Name;
        public Time AlarmTime;
        public DayOfWeek RepeatDays;
        public bool Enabled;

        public Alarm(string name, Time alarmTime, DayOfWeek days, bool enabled)
        {
            Name = name;
            AlarmTime = alarmTime;
            RepeatDays = days;
            Enabled = enabled;
        }

    }
}
