using System.Collections;
using Firebase.Auth;
using UnityEngine;
using Firebase;
using TMPro;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Space(5f)]

    [Header("Login References")]
    [SerializeField]
    private TMP_InputField loginEmail;
    [SerializeField]
    private TMP_InputField loginPassword;
    [SerializeField]
    private TMP_Text loginOutputText;
    [Space(5f)]

    [Header("Register References")]
    [SerializeField]
    private TMP_InputField registerUsername;
    [SerializeField]
    private TMP_InputField registerEmail;
    [SerializeField]
    private TMP_InputField registerPassword;
    [SerializeField]
    private TMP_InputField registerConfirmPassword;
    [SerializeField]
    private TMP_Text registerOutputText;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    private void Start()
    {
        StartCoroutine(CheckAndFixDependancies());
    }

    private IEnumerator CheckAndFixDependancies()
    {
        var checkDependancyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(predicate: () => checkDependancyTask.IsCompleted);
        var dependencyStatus = checkDependancyTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }
    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckAutoLogin());
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }
    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out");
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.DisplayName);
            }
        }
    }
    private IEnumerator CheckAutoLogin()
    {
        yield return new WaitForEndOfFrame();
        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();
            yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);
            AutoLogin();
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }
    private void AutoLogin()
    {
        if (user != null)
        {
            if (user.IsEmailVerified)
            {
                GameManager.instance.ChangeScene(1);
            }
            else
            {
                StartCoroutine(SendVerificationEmail());
            }
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }
    public void ClearOutputs()
    {
        loginOutputText.text = "";
        registerOutputText.text = "";
    }
    public void LoginButton()
    {
        StartCoroutine(LoginLogic(loginEmail.text, loginPassword.text));
    }
    public void RegisterButton() 
    {
        StartCoroutine(RegisterLogic(registerUsername.text, registerEmail.text, registerPassword.text, registerConfirmPassword.text));
    }
    private IEnumerator LoginLogic(string _email, string _password)
    {
        Credential credential = EmailAuthProvider.GetCredential(_email, _password);
        var loginTask = auth.SignInWithCredentialAsync(credential);
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);
        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Try Again!";
            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please Enter Your Email";
                    break;
                case AuthError.MissingPassword:
                    output = "Please Enter Your Password";
                    break;
                case AuthError.InvalidEmail:
                    output = "Invalid Email";
                    break;
                case AuthError.WrongPassword:
                    output = "Incorrect Password";
                    break;
                case AuthError.UserNotFound:
                    output = "Account Does Not Exist";
                    break;
            }
            loginOutputText.text = output;
        }
        else
        {
            if (user.IsEmailVerified)
            {
                yield return new WaitForSeconds(1f);
                GameManager.instance.ChangeScene(1);
            }
            else
            {
                StartCoroutine(SendVerificationEmail());
            }
        }
    }
    private IEnumerator RegisterLogic(string _username, string _email, string _password, string _confirmPassword)
    {
        if (_username == "")
        {
            registerOutputText.text = "Please Enter Your Username";
        }
        else if (_password != _confirmPassword)
        {
            registerOutputText.text = "Password Does Not Match!";
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);
            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Try Again!";
                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        output = "Invalid Reciepient Email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email Already In Use";
                        break;
                    case AuthError.WeakPassword:
                        output = "Weak Password";
                        break;
                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please Enter Your Password";
                        break;
                }
                registerOutputText.text = output;
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = _username,
                    PhotoUrl = new System.Uri("https://pbs.twimg.com/media/EFKdt0bWsAIfcj9.jpg"),
                };
                var defaultUserTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);
                if (defaultUserTask.Exception != null)
                {
                    FirebaseException firebaseException = (FirebaseException)defaultUserTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Try Again!";
                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Task Cancelled??";
                            break;
                        case AuthError.SessionExpired:
                            output = "Session Expired!?!?";
                            break;
                    }
                    registerOutputText.text = output;
                }
                else
                {
                    Debug.Log($"Firebase user created successfully: {user.DisplayName} ({user.UserId})");
                    StartCoroutine(SendVerificationEmail());
                }
            }
        }
    }
   private IEnumerator SendVerificationEmail()
    {
        if (user != null)
        {
            var emailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(predicate: () => emailTask.IsCompleted);
            if (emailTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)emailTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Try Again!";
                switch (error)
                {
                    case AuthError.Cancelled:
                        output = "Verification Task was Cancelled";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.TooManyRequests:
                        output = "Too Many Requests!";
                        break;
                } 
                AuthUIManager.instance.AwaitVerification(false, user.Email , output);
            }
            else
            {
                AuthUIManager.instance.AwaitVerification(true, user.Email, null);
                Debug.Log("Email Sent Sucessfully");
            }
        }
    }
    public void UpdateProfilePicture(string _newPfpURL)
    {
        //StartCoroutine(UpdateProfilePictureLogic(_newPfpURL));
    }
    /*private IEnumerator UpdateProfilePictureLogic(string _newPfpURL)
    {
        if (user != null)
        {
            UserProfile profile = new UserProfile();
            try
            {
                UserProfile _profile = new UserProfile
                {
                    PhotoUrl = new System.Uri(_newPfpURL),
                };
                profile = _profile;
            }
            catch
            {
                LobbyManager.instance.Output("Error Fetching Image, Make Sure Your Link Is Valid!");
                yield break;
            }
            var pfpTask = user.UpdateUserProfileAsync(profile);
            yield return new WaitUntil(predicate: () => pfpTask.IsCompleted);
            if (pfpTask.Exception != null)
            {
                Debug.LogError($"Updating Profile Picture was unsuccessful: {pfpTask.Exception}");
            }
            else
            {
                LobbyManager.instance.ChangePfpSuccess();
                Debug.Log("Profile Image Updated Successfully");
            }

        }
    }*/
    public void UpdateEmail(string _newEmail)
    {
        //StartCoroutine(ChangeEmailLogic(_newEmail));
    }
   /* private IEnumerator ChangeEmailLogic(string _newEmail)
    {
        if (user != null)
        {
            if (_newEmail == user.Email)
            {
                LobbyManager.instance.Output("Please Enter Your <i>New</i> Email");
            }
            else
            {
                var changeEmailTask = user.UpdateEmailAsync(_newEmail);
                yield return new WaitUntil(predicate: () => changeEmailTask.IsCompleted);
                if (changeEmailTask.Exception != null)
                {
                    FirebaseException firebaseException = (FirebaseException)changeEmailTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Try Again!";
                    switch (error)
                    {
                        case AuthError.EmailChangeNeedsVerification:
                            output = "Email Change Needs Verification, Please Verify Then Try Again!";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            output = "Email Already In Use";
                            break;
                        case AuthError.MissingEmail:
                            output = "Please Enter Your Email";
                            break;
                        case AuthError.InvalidEmail:
                            output = "Invalid Email";
                            break;
                        case AuthError.Cancelled:
                            output = "Email Update Cancelled, Try Again!";
                            break;
                        case AuthError.SessionExpired:
                            output = "Session Expired, Try Again!";
                            break;
                        case AuthError.NetworkRequestFailed:
                            output = "Network Request Failed, Try Again!";
                            break;
                        case AuthError.RequiresRecentLogin:
                            output = "Please Reverify Your Account";
                            break;
                    }
                    LobbyManager.instance.Output(output);
                }
                else
                {
                    LobbyManager.instance.ChangeEmailSuccess();
                    Debug.Log("User email updated successfully.");
                }
            }
        }
    }*/
    public void ChangePassword(string _newPassword)
    {
        //StartCoroutine(ChangePasswordLogic(_newPassword));
    }
    /*private IEnumerator ChangePasswordLogic(string _newPassword)
    {
        if (user != null)
        {
            var updatePasswordTask = user.UpdatePasswordAsync(_newPassword);
            yield return new WaitUntil(predicate: () => updatePasswordTask.IsCompleted);
            if (updatePasswordTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)updatePasswordTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Try Again!";

                switch (error)
                {
                    case AuthError.WeakPassword:
                        output = "Weak Password";
                        break;
                    case AuthError.UnverifiedEmail:
                        output = "Please Verify Your Email First";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please Enter Your New Password";
                        break;
                    case AuthError.Cancelled:
                        output = "Password Update Cancelled, Try Again!";
                        break;
                    case AuthError.SessionExpired:
                        output = "Session Expired, Try Again!";
                        break;
                    case AuthError.NetworkRequestFailed:
                        output = "Network Request Failed, Try Again!";
                        break;
                    case AuthError.RequiresRecentLogin:
                        output = "Please Reverify Your Account";
                        break;
                }
                LobbyManager.instance.Output(output);
            }
            else
            {
                LobbyManager.instance.ChangePasswordSuccess();
                Debug.Log("User password updated successfully.");
            }
        }
    }*/
   /* public void Reverify(string _email, string _password)
    {
        StartCoroutine(ReverifyLogic(_email, _password));
    }
    private IEnumerator ReverifyLogic(string _email, string _password)
    {
        if (user != null)
        {
            Credential credential = EmailAuthProvider.GetCredential(_email, _password);
            var reverifyTask = user.ReauthenticateAsync(credential);
            yield return new WaitUntil(predicate: () => reverifyTask.IsCompleted);
            if (reverifyTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)reverifyTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Try Again!";
                switch (error)
                {
                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please Enter Your Password";
                        break;
                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.WrongPassword:
                        output = "Incorrect Password";
                        break;
                    case AuthError.UserNotFound:
                        output = "User Not Found";
                        break;
                    case AuthError.UserMismatch:
                        output = "User Mismatch, Please Verify Your Current Account's Credidentials!";
                        break;
                    case AuthError.Cancelled:
                        output = "Reverify Task Cancelled, Try Again!";
                        break;
                    case AuthError.SessionExpired:
                        output = "Session Expired, Try Again!";
                        break;
                    case AuthError.TooManyRequests:
                        output = "Too Many Requests, Try Again Later!";
                        break;
                }
                LobbyManager.instance.Output(output);
            }
            else
            {
                LobbyManager.instance.ReverifySuccess();
                Debug.Log("User reauthenticated successfully.");
            }
        }
    }*/
    /*public void ResetPassword()
    {
        StartCoroutine(ResetPasswordLogic());
    }
    private IEnumerator ResetPasswordLogic()
    {
        if (user != null)
        {
            var resetPasswordTask = auth.SendPasswordResetEmailAsync(user.Email);
            yield return new WaitUntil(predicate: () => resetPasswordTask.IsCompleted);
            if (resetPasswordTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)resetPasswordTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Try Again!";
                switch (error)
                {
                    case AuthError.Cancelled:
                        output = "Reset Password Task Was Cancelled, Try Again!";
                        break;
                    case AuthError.SessionExpired:
                        output = "Session Expired, Try Again!";
                        break;
                }
                LobbyManager.instance.Output(output);
            }
            else
            {
                LobbyManager.instance.ResetPasswordEmailSuccess();
            }
        }
    }*/
   /* public void DeleteUser()
    {
        StartCoroutine(DeleteUserLogic());
    }
    private IEnumerator DeleteUserLogic()
    {
        if (user != null)
        {
            var deleteUserTask = user.DeleteAsync();
            yield return new WaitUntil(predicate: () => deleteUserTask.IsCompleted);
            if (deleteUserTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)deleteUserTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Try Again!";
                switch (error)
                {
                    case AuthError.Cancelled:
                        output = "Delete User Task Was Cancelled, Try Again!";
                        break;
                    case AuthError.SessionExpired:
                        output = "Session Expired, Try Again!";
                        break;
                    case AuthError.RequiresRecentLogin:
                        output = "Please Reverify Your Account";
                        break;
                }
                LobbyManager.instance.Output(output);
            }
            else
            {
                GameManager.instance.ChangeScene(0);
                Debug.Log("User Deleted Successfully.");
            }
        }
    }*/
    public void SignOut()
    {
        auth.SignOut();
        GameManager.instance.ChangeScene(0);
    }
}