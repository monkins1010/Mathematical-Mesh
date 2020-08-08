﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Goedel.Utilities {
    /// <summary>
    /// Debug trace class
    /// </summary>
    public partial class Screen {

        /// <summary>
        /// Write debug output to current trace listener.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void WriteLine(string format, params object[] arg) {

            var caller = AssemblyStack.GetCallerMethodName();
            if (caller == "TaskFunctionality") {
                return;
                }


            Console.WriteLine(format, arg);


            }


        }
    }