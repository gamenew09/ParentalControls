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
            return (cred.Username == Username && cred.HashedPassword == HashedPassword);
        }

    }

    [Serializable]
    public class ParentalControlsCredentialsFile : ISerializable
    {

        HashSet<ParentalControlsCredential> _ParentalControlsCredentials = new HashSet<ParentalControlsCredential>();

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
            using (FileStream stream = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                BinaryFormatter binary = new BinaryFormatter();
                binary.Serialize(stream, this);
            }
        }

        /// <summary>
        /// Loads and returns a ParentalControlsCredentialsFile.
        /// </summary>
        /// <param name="filename">File to load</param>
        /// <returns>ParentalControlsCredentialsFile with FileName.</returns>
        public static ParentalControlsCredentialsFile Load(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                try
                {
                    BinaryFormatter binary = new BinaryFormatter();
                    return (ParentalControlsCredentialsFile)binary.Deserialize(stream);
                }
                catch
                {
                    ParentalControlsCredentialsFile file = new ParentalControlsCredentialsFile();
                    file.FileName = filename;
                    return file;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ParentalControlsCredential[] ParentalControlsCredentials = _ParentalControlsCredentials.ToArray();
            for (int i = 0; i < _ParentalControlsCredentials.Count; i++)
            {
                info.AddValue(i.ToString(), ParentalControlsCredentials[i]);
            }
        }

        public bool Validate(ParentalControlsCredential parentalControlsCredential)
        {
            Console.WriteLine("Username: {0} Password: {1}", parentalControlsCredential.Username, parentalControlsCredential.HashedPassword);
            foreach (ParentalControlsCredential cred in _ParentalControlsCredentials)
            {
                Console.WriteLine("Username: {0} Password: {1}", cred.Username, cred.HashedPassword);
                if (cred.ValidateCredentials(parentalControlsCredential))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
