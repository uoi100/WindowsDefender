using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URISchemeExample
{
    /// <summary>
    /// 
    /// This program registers a custom URI Scheme with the OS.
    /// The URI accepts one argument. Eg. urischeme:arg
    /// This argument is retrieved in the Main() function.
    /// 
    /// Followed official Microsoft instructions from here:
    /// https://msdn.microsoft.com/en-us/library/aa767914(v=vs.85).aspx#app_reg
    /// 
    /// Any project that uses this code MUST request administrator access.
    /// To do this you must have an app.manifest file with the following inside:
    /// <requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
    /// 
    /// To create an app.manifest, click the project and go to Add Item.
    /// Scroll down to Application Manifest and select it. It should be straight
    /// forward from there once you open it.
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Prepare registry values
            string path       = "C:\\Program Files\\TowerDefender";
            string filename   = "TowerDefender.exe";
            string uri_scheme = "towerdefender";

            // Register URI Scheme
            CreateURISchemeInRegistry(path, filename, uri_scheme);
        }

        /// <summary>
        /// This function registers a custom URI Scheme with the OS.
        /// When you visit the URI in a web browser, the user will be
        /// prompted to launch the associated application.
        /// Written by Jeff Schweigler.
        /// </summary>
        /// <param name="filename">Name of the file to execute.</param>
        /// <param name="uri_scheme">This is the keyword the browser can launch from.</param>
        /// <param name="filepath">Full path to executable including filename.</param>
        private static void CreateURISchemeInRegistry(string path, string filename, string uri_scheme)
        {
            RegistryKey key = Registry.ClassesRoot.CreateSubKey(uri_scheme);
            key.SetValue("", "URL:" + uri_scheme + " Protocol");
            key.SetValue("URL Protocol", "");
            key = key.CreateSubKey("DefaultIcon");
            key.SetValue("", filename + ",1");
            key = Registry.ClassesRoot.OpenSubKey(uri_scheme, true);
            key = key.CreateSubKey("shell");
            key = key.CreateSubKey("open");
            key = key.CreateSubKey("command");
            key.SetValue("", "\"" + path + "\\" + filename + "\" \"%1\"");
            key.Close();
        }
    }
}
