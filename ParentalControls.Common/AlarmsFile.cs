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
    public class AlarmsFile
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
            using (BinaryWriter stream = new BinaryWriter(new FileStream(FileName, FileMode.OpenOrCreate)))
            {
                foreach (Alarm alarm in _Alarms)
                {
                    stream.Write(alarm.Name);
                    stream.Write(alarm.Enabled);

                    stream.Write(alarm.AlarmTime.Hour);
                    stream.Write(alarm.AlarmTime.Minutes);
                    stream.Write(alarm.AlarmTime.Seconds);

                    stream.Write((int)alarm.RepeatDays);
                }
            }
        }

        /// <summary>
        /// Loads and returns a AlarmsFile.
        /// </summary>
        /// <param name="filename">File to load</param>
        /// <returns>AlarmsFile with FileName.</returns>
        public static AlarmsFile Load(string filename)
        {
            AlarmsFile file = new AlarmsFile();
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.OpenOrCreate)))
                {
                    Alarm alarm = new Alarm();
                    alarm.AlarmTime = new Time();
                    int readat = 0;
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        switch (readat)
                        {
                            case 0:
                                alarm.Name = reader.ReadString();
                                readat++;
                                break;
                            case 1:
                                alarm.Enabled = reader.ReadBoolean();
                                readat++;
                                break;
                            case 2:
                                alarm.AlarmTime.Hour = reader.ReadInt32();
                                readat++;
                                break;
                            case 3:
                                alarm.AlarmTime.Minutes = reader.ReadInt32();
                                readat++;
                                break;
                            case 4:
                                alarm.AlarmTime.Seconds = reader.ReadInt32();
                                readat++;
                                break;
                            case 5:
                                alarm.RepeatDays = (DayOfWeek)reader.ReadInt32();

                                file.Add(alarm);
                                readat = 0;
                                alarm = new Alarm();
                                alarm.AlarmTime = new Time();
                                break;
                            
                        }
                    }
                }
            }
            catch { }
            return file;
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
