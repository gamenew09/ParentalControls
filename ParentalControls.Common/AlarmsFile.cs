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
    public class Time
    {

        public Time(int hour, int minutes, int seconds)
        {
            Hour = hour;
            Minutes = minutes;
            Seconds = seconds;
        }

        public Time()
        {

        }

        public Time(int hour, int minutes)
        {
            Hour = hour;
            Minutes = minutes;

            Seconds = 0;
        }

        public static Alarm Empty = new Alarm();

        public int Hour = -1;
        public int Minutes = -1;
        public int Seconds = -1;

        public bool Valid()
        {
            return !(Hour < 1 || Minutes < 0 || Seconds < 0) || !(Hour > 24 || Minutes > 59 || Seconds > 59);
        }

        public int Get12HourTime(out bool pm)
        {
            pm = (Hour > 12);
            if (pm)
            {
                return Math.Min(Hour - 12, 0);
            }
            else
            {
                return Hour;
            }
        }

    }

    [Serializable]
    public class Alarm
    {

        public static Alarm Empty = new Alarm();

        public string Name;
        public Time AlarmTime;
        public DayOfWeek RepeatDays;
        public bool Enabled;

        public static bool operator == (Alarm a1, Alarm a2)
        {
            return Equals(a1, a2);
        }

        public static bool operator !=(Alarm a1, Alarm a2)
        {
            return !Equals(a1, a2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public Alarm()
        {

        }

        public Alarm(string name, Time alarmTime, DayOfWeek days, bool enabled)
        {
            Name = name;
            AlarmTime = alarmTime;
            RepeatDays = days;
            Enabled = enabled;
        }

    }
}
