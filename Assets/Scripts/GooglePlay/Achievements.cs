using GooglePlayGames;
using UnityEngine;
using UnityEngine.UI;

public class Achievements : MonoBehaviour
{
 #region PUBLIC_VAR
    public string leaderboard;  // This is where to pass the resource string in!
    public Text textBox;
    #endregion
    #region DEFAULT_UNITY_CALLBACKS
    public long my_score;  // This is where I passed in the users score
    public bool success;
    public bool GPGLoginReturned;

void Start()
    {
        PlayGamesPlatform.Activate();
        LogIn();
    }
    #endregion
    #region BUTTON_CALLBACKS

// Login to Google Account
public bool LogIn()
    {
        Social.localUser.Authenticate((success) =>
        {
            if (success)
            {
                string userInfo = "Username: " + Social.localUser.userName +
                    "\nUser ID: " + Social.localUser.id +
                    "\nIsUnderage: " + Social.localUser.underage;

                textBox.text = userInfo;
            }
            else
            {
                Debug.Log("Login failed");
                textBox.text = Social.Active.ToString() + " Login Failed.";
            }
        });
        return GPGLoginReturned = true;
    }


// Show All Leaderboards
public void OnShowLeaderBoard()
    {
        Social.ShowLeaderboardUI();
    }
	
// Adds a Score to the Leaderboard	
public void AddScoreToLeaderBoard(long my_score)
    {
        if (Social.localUser.authenticated)
        {
            Social.ReportScore(1234, GPGSIds.leaderboard_test, (bool success) =>
            {
                Debug.Log(success ? "Update Score Success" : "Update Score Fail");
            });
        }
    }

// Logout the Google+ Account
public void OnLogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
    }
    #endregion
}