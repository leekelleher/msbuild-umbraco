using System;
using System.Collections.Generic;
using System.Text;

namespace MSBuild.Umbraco.Tasks
{
    public class Constants
    {
        internal const string PACKAGE_NAME_XPATH = "umbPackage/info/package/name";
        internal const string PACKAGE_VERSION_XPATH = "umbPackage/info/package/version";
        internal const string PACKAGE_URL = "umbPackage/info/package/url";
        internal const string PACKAGE_LICENSE_XPATH = "umbPackage/info/package/license";
        internal const string PACKAGE_REQUIREMENTS_XPATH = "umbPackage/info/package/requirements";
        internal const string AUTHOR_NAME_XPATH = "umbPackage/info/author/name";
        internal const string AUTHOR_URL_XPATH = "umbPackage/info/author/website";
        internal const string README_XPATH = "umbPackage/info/readme";
    }
}
