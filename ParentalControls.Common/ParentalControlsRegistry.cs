using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentalControls.Common
{
    public class ParentalControlsRegistry
    {

        public static RegistryKey GetRegistryKey(bool readOnly = false)
        {
            if(readOnly)
                return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Gamenew09\Parental Controls");
            else
                return Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Gamenew09\Parental Controls");
        }

    }
}
