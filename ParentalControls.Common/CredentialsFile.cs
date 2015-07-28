using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParentalControls.Common
{

    public static class SHA256Hash
    {
        public static string Hash(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }

    [Serializable]
    public struct ParentalControlsCredential
    {

        public ParentalControlsCredential(string name, string password, bool hashedAlready)
        {
            Username = name;
            if (!hashedAlready)
                HashedPassword = SHA256Hash.Hash(password);
            else
                HashedPassword = password;
        }

        // Declare an explicit conversion from a RomanNumeral to an int:
        static public explicit operator NetworkCredential(ParentalControlsCredential cred)
        {
            return new NetworkCredential(cred.Username, cred.HashedPassword);
        }

        static public explicit operator ParentalControlsCredential(NetworkCredential cred)
        {
            return new ParentalControlsCredential(cred.UserName, cred.Password, true);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ParentalControlsCredential))
                return false;

            return ((ParentalControlsCredential)obj).Username == this.Username;
        }

        /// <summary>
        /// Username of a credential.
        /// </summary>
        public string Username;
        /// <summary>
        /// Password hashed in SHA256, using UTF-8.
        /// </summary>
        public string HashedPassword;

        /// <summary>
        /// Validates a credential passed with this.
        /// </summary>
        /// <param name="cred">The credential to validate.</param>
        /// <returns>Is the credential valid?</returns>
        public bool ValidateCredentials(ParentalControlsCredential cred)
        {
            //Console.WriteLine("CredA: {0} CredB: {1}", this.ToString(), cred.ToString());
            return (cred.Username == Username && cred.HashedPassword == HashedPassword);
        }

        /*
        public override string ToString()
        {
            return string.Format("ParentalControlsCredential@Username={0}&HashedPassword={1}", Username, HashedPassword);
        }
        */

        public override string ToString()
        {
            return Username;
        }

    }

    [Serializable]
    public class ParentalControlsCredentialsFile
    {

        public ParentalControlsCredentialsFile()
        {

        }

        HashSet<ParentalControlsCredential> _ParentalControlsCredentials = new HashSet<ParentalControlsCredential>();

        public bool Remove(ParentalControlsCredential cred)
        {
            return Remove(cred.Username);
        }

        public bool Remove(string username)
        {
            bool remove = false;
            int i = 0;
            foreach (ParentalControlsCredential cred in _ParentalControlsCredentials)
            {
                Console.WriteLine("Thing: {0} == {1} is {2}", cred.Username, username, (cred.Username == username));
                if (cred.Username == username)
                {
                    remove = true;
                    break;
                }
                i++;
            }

            if (remove)
                return _ParentalControlsCredentials.Remove(_ParentalControlsCredentials.ToList()[i]);

            return false;
        }

        public bool Exists(ParentalControlsCredential cred)
        {
            foreach (ParentalControlsCredential c in _ParentalControlsCredentials)
            {
                //Console.WriteLine("Username: {0} Password: {1}", cred.Username, cred.HashedPassword);
                if (cred.Username == c.Username)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsValidForSaving()
        {
            return String.IsNullOrWhiteSpace(FileName);
        }

        public string FileName
        {
            get;
            set;
        }

        public ICollection<ParentalControlsCredential> ParentalControlsCredentials
        {
            get
            {
                ParentalControlsCredential[] ParentalControlsCredentials = new ParentalControlsCredential[_ParentalControlsCredentials.Count];
                _ParentalControlsCredentials.CopyTo(ParentalControlsCredentials);
                return ParentalControlsCredentials;
            }
        }

        public void Add(ParentalControlsCredential ParentalControlsCredential)
        {
            _ParentalControlsCredentials.Add(ParentalControlsCredential);
        }

        public void Add(string name, string password, bool alreadyHashed = false)
        {
            Add(new ParentalControlsCredential(name, password, alreadyHashed));
        }

        /// <summary>
        /// Saves to file specified by FileName.
        /// </summary>
        public void Save()
        {
            using (BinaryWriter stream = new BinaryWriter(new FileStream(FileName, FileMode.OpenOrCreate)))
            {
                foreach (ParentalControlsCredential cred in _ParentalControlsCredentials)
                {
                    Console.WriteLine("Saving Credential: {0}", cred.ToString());
                    stream.Write(cred.Username);
                    stream.Write(cred.HashedPassword);
                }
            }
        }

        /// <summary>
        /// Saves to file specified by FileName.
        /// <param name="filename">The file to write to.</param>
        /// </summary>
        public void Save(string filename)
        {
            using (BinaryWriter stream = new BinaryWriter(new FileStream(filename, FileMode.OpenOrCreate)))
            {
                foreach (ParentalControlsCredential cred in _ParentalControlsCredentials)
                {
                    stream.Write(cred.Username);
                    stream.Write(cred.HashedPassword);
                }
            }
        }

        /// <summary>
        /// Loads and returns a ParentalControlsCredentialsFile.
        /// </summary>
        /// <param name="filename">File to load</param>
        /// <returns>ParentalControlsCredentialsFile with FileName.</returns>
        public static ParentalControlsCredentialsFile Load(string filename)
        {
            ParentalControlsCredentialsFile file = new ParentalControlsCredentialsFile();
            file.FileName = filename;
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.OpenOrCreate)))
                {
                    ParentalControlsCredential cred = new ParentalControlsCredential();
                    int readat = 0;
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        switch (readat)
                        {
                            case 0:
                                cred.Username = reader.ReadString();
                                readat = 1;
                                break;
                            case 1:
                                cred.HashedPassword = reader.ReadString();
                                readat = 0;
                                file.Add(cred);
                                cred = new ParentalControlsCredential();
                                break;
                        }
                    }
                }
            }
            catch { }
            return file;
        }

        public bool Validate(ParentalControlsCredential parentalControlsCredential)
        {
            //Console.WriteLine("Username: {0} Password: {1}", parentalControlsCredential.Username, parentalControlsCredential.HashedPassword);
            foreach (ParentalControlsCredential cred in _ParentalControlsCredentials)
            {
                //Console.WriteLine("Username: {0} Password: {1}", cred.Username, cred.HashedPassword);
                if (cred.ValidateCredentials(parentalControlsCredential))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
