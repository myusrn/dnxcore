using System.Collections.Generic;
using System.Web.Http.Routing;

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
// http://aspnet.codeplex.com/SourceControl/latest#Samples/WebApi/RoutingConstraintsSample/RoutingConstraints.Server/VersionedRoute.cs

namespace MyUsrn.Dnx.Core
{
    /// <summary>
    /// Place on an action to expose it directly via a route.
    /// Provides an attribute route that's restricted to a specific version of the api.
    /// </summary>
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    //public sealed class RouteAttribute : Attribute, IDirectRouteFactory, IHttpRouteInfoProvider
    public class RouteExAttribute : RouteFactoryAttribute
    {
        //public RouteExAttribute(string template, int allowedVersion)
        public RouteExAttribute(string template, string allowedVersion)
            : base(template)
        {
            AllowedVersion = allowedVersion;
        }

        //public int AllowedVersion
        public string AllowedVersion
        {
            get;
            private set;
        }

        public override IDictionary<string, object> Constraints
        {
            get
            {
                var constraints = new HttpRouteValueDictionary();
                constraints.Add("version", new RouteExConstraint(AllowedVersion));
                return constraints;
            }
        }
    }
}