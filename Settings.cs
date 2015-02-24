using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersSupport
{
    public static class Settings
    {
        public static object Get(string name)
        {
            object settingVal = null;

            try {
                settingVal = Properties.LocalSettings.Default[name];
            }
            catch (Exception e)
            {
                try
                {
                    settingVal = Properties.Settings.Default[name];
                }
                catch (Exception e2)
                {
                    settingVal = null;
                }
            }

            return settingVal;
        }
    }
}
