﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using VFS.Contract;
using System.Security.Authentication;

namespace VFS.Client
{
    class Program
    {
        /// <summary>
        /// End point name to connect to a server
        /// </summary>
        static readonly string _endpointName = "RemoteConsole";

        static void Main(string[] args)
        {
            while (true)
            {
                IRemoteConsole remoteConsole = connectToServer();
                if (remoteConsole == null)
                {
                    return;
                }
                DoWork(remoteConsole);
            }
        }

        /// <summary>
        /// Provide work with remote console <paramref name="remoteConsole"/>
        /// </summary>
        /// <param name="remoteConsole">Remote console</param>
        private static void DoWork(IRemoteConsole remoteConsole)
        {
            while (true)
            {
                Console.Write("{0}> ", remoteConsole.GetUserPosition());
                string command = Console.ReadLine();
                if (command.ToUpper().Equals("QUITE"))
                {
                    remoteConsole.Quite();
                    break;
                }
                Console.WriteLine(remoteConsole.SendCommand(command));
            }
        }

        /// <summary>
        /// Provide create connection to server
        /// </summary>
        /// <returns>Remote console to work with server</returns>
        private static IRemoteConsole connectToServer()
        {
            IRemoteConsole returnValue = null;
            while (returnValue == null)
            {
                Console.Write("> ");
                string command = Console.ReadLine();
                ConnectionData connectionData = null;
                try
                {
                    connectionData = ConnectionData.Parse(command);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine("Please input 'connect <server address/server name> <user name>'");
                }                
                returnValue = tryConnectToServer(connectionData);
            }
            return returnValue;
        }

        /// <summary>
        /// try to connect to the server by connection information <paramref name="connectionData"/>
        /// </summary>
        /// <param name="connectionData">data to connect to a server</param>
        /// <returns>Remote console from server. <c>null</c> - if connection was fail</returns>
        private static IRemoteConsole tryConnectToServer(ConnectionData connectionData)
        {
            InstanceContext context = new InstanceContext(new ConsoleCallback());
            using (DuplexChannelFactory<IRemoteConsole> factory =
                    new DuplexChannelFactory<IRemoteConsole>(context, _endpointName, connectionData.Endpoint))
            {
                IRemoteConsole remoteConsole = factory.CreateChannel();
                try
                {                    
                    bool authenticateSuccess = remoteConsole.Authenticate(connectionData.UserName);
                    if (authenticateSuccess)
                    {
                        return remoteConsole;
                    }
                    else
                    {
                        Console.WriteLine("Authentication was fail");
                    }
                }
                catch (EndpointNotFoundException ex)
                {
                    factory.Abort();
                    Console.WriteLine("Server with this address not found");
                }
            }
            return null;
        }
    }
}
