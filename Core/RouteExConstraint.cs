using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
// http://aspnet.codeplex.com/SourceControl/latest#Samples/WebApi/RoutingConstraintsSample/RoutingConstraints.Server/VersionConstraint.cs

namespace MyUsrn.Dnx.Core
{
    /// <summary>
    /// A Constraint implementation that matches an HTTP header -> query string parameter against an expected version value.
    /// </summary>
    public class RouteExConstraint : IHttpRouteConstraint
    {
        //public const string VersionHeaderName = "api-version";
        public const string VersionParameterName = "api-version";

        //private const int DefaultVersion = 1;
        private const string DefaultVersion = "1.0";

        //public RouteExConstraint(int allowedVersion)
        public RouteExConstraint(string allowedVersion)
        {
            AllowedVersion = allowedVersion;
        }

        //public int AllowedVersion
        public string AllowedVersion
        {
            get;
            private set;
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (routeDirection == HttpRouteDirection.UriResolution)
            {
                //int version = GetVersionHeader(request) ?? DefaultVersion;
                string version = GetVersionParameter(request) ?? DefaultVersion;

                return (version == AllowedVersion);
            }

            return true;
        }

        //private int? GetVersionHeader(HttpRequestMessage request)
        //{
        //    string versionAsString;
        //    IEnumerable<string> headerValues;
        //    if (request.Headers.TryGetValues(VersionHeaderName, out headerValues) && headerValues.Count() == 1)
        //    {
        //        versionAsString = headerValues.First();
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //    int version;
        //    if (versionAsString != null && Int32.TryParse(versionAsString, out version))
        //    {
        //        return version;
        //    }

        //    return null;
        //}

        private string GetVersionParameter(HttpRequestMessage request)
        {
            var parameterValue = request.GetQueryNameValuePairs().Where(p => p.Key == VersionParameterName).FirstOrDefault().Value;
            if (!string.IsNullOrEmpty(parameterValue))
            {
                return parameterValue;
            }
            else
            {
                return null;
            }

        }
    }
}
