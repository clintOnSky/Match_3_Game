using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class TimeManager : MonoBehaviour
{
    #region Singleton class: TimeManager
    public static TimeManager sharedInstance = null;
    private const string _url = "http://worldtimeapi.org/api/ip";
    private DateTime _currentDateTime = DateTime.Now;
    //json file container
    struct TimeData
    {
        public string datetime;
    }
    //makes sure there is only one instance of this always
    void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
        }
        else if (sharedInstance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Debug.Log("TimeManager script is ready");
        StartCoroutine("GetTime");
    }

    public DateTime GetCurrentDateTime()
    {
        // gets date and time without accessing internet using local time
        // just add elapsed time since the game started to the _currentDateTime variable
        return _currentDateTime.AddSeconds(Time.realtimeSinceStartup); 
    }

    //time fether coroutine
    public IEnumerator GetTime()
    {
        Debug.Log("connecting to php");
        UnityWebRequest webRequest = UnityWebRequest.Get(_url);
        yield return webRequest.SendWebRequest();
        if (webRequest.error != null)
        {
            Debug.Log("Error!");
            Debug.Log(GetCurrentDateTime());
        }
        else
        {
            Debug.Log("got the php information");
            TimeData timeData = JsonUtility.FromJson<TimeData>(webRequest.downloadHandler.text);
            Debug.Log("still working");
            _currentDateTime = ParseDateTime(timeData.datetime);
            Debug.Log(_currentDateTime);
            
        }
    }

    DateTime ParseDateTime(string dateTime)
    {
        string date = Regex.Match(dateTime, @"^\d{4}-\d{2}-\d{2}").Value;
        string time = Regex.Match(dateTime, @"\d{2}:\d{2}:\d{2}").Value;

        Debug.Log(date + " and " + time);

        return DateTime.Parse(string.Format("{0} {1}", date, time));
    }
    #endregion
}

/*
{"abbreviation":"WAT","client_ip":"102.89.33.72",
"datetime":"2022-12-07T16:03:20.543693+01:00",
"day_of_week":3,
"day_of_year":341,
"dst":false,
"dst_from":null,
"dst_offset":0,
"dst_until":null,
"raw_offset":3600,
"timezone":"Africa/Lagos",
"unixtime":1670425400,
"utc_datetime":"2022-12-07T15:03:20.543693+00:00",
"utc_offset":"+01:00","week_number":49} 
*/