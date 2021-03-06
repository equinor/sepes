﻿namespace Sepes.Common.Constants
{
    public static class AppRoles
    {
        //App wide roles. Fetched from AD
        public const string Admin = "Sepes-Admin";
        public const string Sponsor = "Sepes-Sponsor";
        public const string DatasetAdmin = "Sepes-Dataset-Admin";
        public const string External = "Sepes-External";
    }

    //Study specific roles, maintained in Sepes application
    public static class StudyRoles
    {
        public const string StudyOwner = "Study Owner";
        public const string SponsorRep = "Sponsor Rep";
        public const string VendorAdmin = "Vendor Admin";
        public const string VendorContributor = "Vendor Contributor";
        public const string StudyViewer = "Study Viewer";  
    }   
}
