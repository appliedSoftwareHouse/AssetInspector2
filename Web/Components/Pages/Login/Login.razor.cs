using AppModels.Login;
using Crypto;
using DB.Core;
using Web.Services;


namespace Web.Components.Pages.Login
{
    public partial class Login
    {

        private string Error = "";

        private string ImagePath { get; set; }
        private string NumberFormat { get; set; }

        private CreateDB CreateDb { get; set; }

        public LoginModel UserLogin { get; set; } = new();

        protected override void OnInitialized()
        {
            //Global = new(Keys);
            ImagePath = Keys.Get<string>("AssetImagePath") ?? "";
            NumberFormat = Global.Format.Number!;

            ImagePath = Keys.Get<string>("AppImages");

            CreateDb = new(db, Keys);           // Create Database Service
        }

        public async Task OnLogin()
        {
            Error = string.Empty;

            // Validate Database, Create Db if not exist and add Admin User Account.
            if (!ValidateDB()) { NavManager.NavigateTo("/Error/DBError/2"); }


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
                var LoginService = new AppAuth.LoginClass(db,Keys);

                if (LoginService.IsConnected)
                {
                    var _LoginModel = new LoginModel()
                    {
                        UserID = UserLogin.UserID,
                        Password = string.Empty,
                        PasswordHash = Encrypt.EncryptString(UserLogin.Password, "Modern"),
                        RememberMe = false
                    };

                    UserProfile? _UserProfile = LoginService.GetUserProfile(_LoginModel);

                    if (_UserProfile.IsLogin)
                    {
                        await UserAuth.SignInAsync(_UserProfile);
                        NavManager.NavigateTo("/Dashboard");

                    }
                    else
                    {
                        Error = L[_UserProfile.LoginMessage];
                    }
                }
            }
        }

        private bool ValidateDB()
        {
            // Createing a new Database instance if not exist, create all data table and system and Admin User default Account

            bool _result = false;
            
            if (CreateDb.IsConnected)
            {
                _result = CreateDb.CreateDataTables();
                if (_result) _result = CreateDb.UpdatePatches();
                if (_result) _result = CreateDb.CreateAdminUser();
            }
            return _result;
        }
    }
}



