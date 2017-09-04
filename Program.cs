using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace BigBrothersAssistant
{
    class BigBrothersAssistant
    {

        //Active user throughout the program
        public static User ActiveUser;

        //Registry key needed to autorun at system startup
        static RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        static Boolean status = true;

        //Windows function imports to hide the console window
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        public static void Main()
        {
            //Instantiate the user
            ActiveUser = new User();

            Console.Title = "Big Brother's assistant v1.0";
            Console.WriteLine("Big Brother's assistant v1.0");

            //Check if user exists/create new user
            string path = Directory.GetCurrentDirectory();
            string fullpath = path + "/name.txt";
            List<String> file = new List<string>();
            if (File.Exists(fullpath) && new FileInfo(fullpath).Length > 0)
            {
                using (StreamReader sr = File.OpenText(fullpath))
                {

                    while (sr.Peek() >= 0)
                    {
                        file.Add(sr.ReadLine());
                    }
                    //ActiveUser = ActiveUser.getUser(savedName);
                    if (file.Contains("Hide"))
                    {
                        handleCommands("Hide");
                    }
                    if (file.Count > 0)
                    {
                        Console.WriteLine(DateTime.Now + " Logged in as: " + file[0]);

                    }
                    sr.Close();
                }
            }
            else
            {
                Console.WriteLine("Please write your rsn(To create user):");
                String Username = Console.ReadLine();
                using (StreamWriter sw = File.CreateText(fullpath))
                {
                    sw.WriteLine(Username);
                    sw.Close();
                }
            }
            //Run system at startup and hide window
            if (!file.Contains("OnStart"))
            {
                Console.WriteLine("Start at startup?(y/n)");
                String cr = Console.ReadLine();
                if (cr == "y")
                {
                    rk.SetValue("BigBrothersAssistant", System.Reflection.Assembly.GetExecutingAssembly().Location);
                    Console.WriteLine(DateTime.Now + " BigBrothersAssistant now launches on system start");
                    using (StreamWriter sw = new StreamWriter(fullpath, true))
                    {
                        sw.WriteLine("OnStart");
                        sw.Close();
                    }
                }
            }
            if (!file.Contains("Hide"))
            {
                Console.WriteLine("Do you want the console window automatically hidden on next system reset?(y/n)");
                String wl = Console.ReadLine();
                if (wl == "y")
                {
                    using (StreamWriter sw = new StreamWriter(fullpath, true))
                    {
                        sw.WriteLine("Hide");
                        sw.Close();
                    }
                }
            }
            //Preliminary check before attaching event handlers
            Console.WriteLine(DateTime.Now + " Checks if RuneScape is currently running...");
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName == "rs2client")
                {
                    //Notify system that RS is active
                    Console.WriteLine(DateTime.Now + " Runescape is running...");
                    ActiveUser.Active = true;
                    //Send data to db
                    Console.WriteLine(DateTime.Now + " Notified Big brother...");
                }
            }
            Console.WriteLine(DateTime.Now + " Starts logging...");
            //Starts eventWatchers
            try
            {
                ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace WHERE ProcessName = 'rs2client.exe'"));
                startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
                startWatch.Start();
                ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace WHERE ProcessName = 'rs2client.exe'"));
                stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
                stopWatch.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("We were unable to create the event listeners, did you run the software as admin?");
                throw;
            }
            Console.WriteLine("Application is now running, type \"Help\" for commands");
            //Idle
            while (status)
            {
                var command = Console.ReadLine();
                handleCommands(command);
            }
        }

        //RS stops
        static void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (ActiveUser.Active)
            {
                //Notify system that RS is inactive
                Console.WriteLine(DateTime.Now + " Runescape is not running...");
                ActiveUser.Active = false;
                //Send data to db
                Console.WriteLine(DateTime.Now + " Notified Big brother...");
            }
        }
        //RS starts
        static void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            //Notify system that RS is active
            if (ActiveUser.Active == false)
            {
                Console.WriteLine(DateTime.Now + " Runescape is running...");
                ActiveUser.Active = true;
                //Send data to db
                Console.WriteLine(DateTime.Now + " Notified Big brother...");
            }
        }

        //CLI handler
        static void handleCommands(string Command)
        {
            if (Command == "Help")
            {
                Console.Clear();
                Console.WriteLine("\"StopAutorun\" - Stops the application from launching on system start");
                Console.WriteLine("\"StartAutorun\" - Starts the application on system start");
                Console.WriteLine("\"DeleteUser\" - Stops autologin and forces user to retype username");
                Console.WriteLine("\"Hide\" - Immediatly hides the console window");
                Console.WriteLine("\"Quit\" - Stops eventlisteners and shuts down the system");
            }
            else if (Command == "StopAutorun")
            {
                rk.DeleteValue("BigBrothersAssistant", false);
                Console.WriteLine(DateTime.Now + " BigBrothersAssistant no longer launches on system start");
            }
            else if (Command == "StartAutorun")
            {
                rk.SetValue("BigBrothersAssistant", System.Reflection.Assembly.GetExecutingAssembly().Location);
                Console.WriteLine(DateTime.Now + " BigBrothersAssistant now launches on system start");
            }
            else if (Command == "DeleteUser")
            {
                File.Delete(Directory.GetCurrentDirectory() + "/name.txt");
                Console.WriteLine(DateTime.Now + " User has been deleted");
            }
            else if (Command == "Quit")
            {
                status = false;
                Console.WriteLine(DateTime.Now + " Stops logging...");
            }
            else if (Command == "Hide")
            {
                IntPtr hWnd = GetConsoleWindow();
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, 0);
                }
            }
            else
            {
                Console.WriteLine("Unknown command - type \"Help\" for available commands");
            }
        }
    }
}
