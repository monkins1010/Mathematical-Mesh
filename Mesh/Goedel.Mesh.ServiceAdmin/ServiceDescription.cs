﻿//  © 2021 by Phill Hallam-Baker
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  

using Goedel.Utilities;

using System;
using System.Collections.Generic;


namespace Goedel.Mesh.ServiceAdmin {

    /// <summary>
    /// Delegate returning a host <paramref name="hostConfiguration"/> for the
    /// service  <paramref name="serviceConfiguration"/>
    /// </summary>
    /// <param name="serviceConfiguration">The service configuration.</param>
    /// <param name="hostConfiguration">The configuration of this specific host.</param>
    /// <returns></returns>
    public delegate Goedel.Protocol.JpcInterface ServiceFactoryDelegate(
        ServiceConfiguration serviceConfiguration,
        HostConfiguration hostConfiguration);

    /// <summary>
    /// Service description used to advertise a service provider.
    /// </summary>
    public record ServiceDescription(
            string WellKnown, ServiceFactoryDelegate Factory) {
        }

    /// <summary>
    /// Describes a service configuration.
    /// </summary>
    public partial class ServiceConfiguration {
        }

    /// <summary>
    /// Describes a host configuration.
    /// </summary>
    public partial class HostConfiguration {
        }

    public partial class Configuration {

        public HostConfiguration GetHostConfiguration(string Id) {
            bool nonHost = false;
            
            if (Entries == null) {
                return null;
                }
            foreach (var entry in Entries) {

                if (entry.Id?.ToLower() == Id) {
                    if (entry is HostConfiguration) {
                        return entry as HostConfiguration;
                        }
                    nonHost = true;
                    }
                }
            nonHost.AssertFalse(ConfigurationNotHost.Throw, MachineName);

            return null;
            }

        }



    }
