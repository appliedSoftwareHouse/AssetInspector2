using AppModels.Login;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB.Core.Mapping
{
    public static class ObjectMapping
    {
        public static UserProfile ToUserProfile(this UserSession userSession)
        {
            UserProfile userProfile = new(); ;

            userProfile.UserID = userSession.UserID;
            userProfile.DisplayName = userSession.DisplayName;
            userProfile.Email = userSession.Email;
            userProfile.PhoneNumber = userSession.PhoneNumber;
            userProfile.UserRole = userSession.UserRole;
            userProfile.Designation = userSession.Designation;
            userProfile.Session = userSession.Session;
            userProfile.IsActive = true;

            return userProfile;
        }
    }
}
