using AppModels.Login;
using Crypto;
using DB.Core;


namespace Web.Components.Pages.Login
{
    public partial class Login
    {

        private string Error = "";

        private string ImagePath { get; set; }
        private string NumberFormat { get; set; }



        public LoginModel UserLogin { get; set; } = new();

        protected override void OnInitialized()
        {
            ImagePath = Config.GetValue<string>("AssetImagePath") ?? "";
            NumberFormat = Global.Format.Number!;
        }

        public void OnLogin()
        {
            Error = string.Empty;

            // Validate Database, Create Db if not exist and add Admin User Account.
            if (ValidateDB()) { NavManager.NavigateTo("/Error/DBError/2"); }


            if (string.IsNullOrWhiteSpace(UserLogin?.UserID))
            {
                Error = "Please enter your user ID.";
                return;
            }

            if (string.IsNullOrWhiteSpace(UserLogin?.Password))
            {
                Error = "Please enter your password.";
                return;
            }

            if (Error == string.Empty)
            {




                var db = DbService.DBContext;

                if (db.IsConnected)
                {
                    var _LoginModel = new LoginModel()
                    {
                        UserID = UserLogin.UserID,
                        Password = UserLogin.Password,
                        PasswordHash = string.Empty
                    };

                    UserProfile? _UserProfile = db.GetUserProfile(_LoginModel);

                    if (_UserProfile == null)
                    {
                        NavManager.NavigateTo("/Error/DBError/3");
                    }
                }
            }
        }

        private bool ValidateDB()
        {
            bool _result = false;

            if (DbService.IsDBExist)
            {
                _result = DbService.CreateDataTables;
                if (_result) _result = DbService.UpdatePatches;
                if (_result) _result = DbService.CreateAdminAccount;
            }
            return _result;
        }
    }
}



