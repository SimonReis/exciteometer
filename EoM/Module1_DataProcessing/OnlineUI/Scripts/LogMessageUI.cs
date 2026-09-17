using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace ExciteOMeter
{
    public class LogMessageUI : MonoBehaviour
    {
        [Header("Console Popup Text")]
        public bool showConsoleBar = true;
        public GameObject consoleContainer;
        public TextMeshProUGUI consoleOutputText;

        [Header("Panels to show or hide")]
        public CanvasGroup showWhenNotRecording;
        public CanvasGroup showWhenRecording;


        [Header("Indicator of recording status")]
        public Button startStopLogging;
        public Image recordingStatusImage;
        public TextMeshProUGUI recordingStatusButtonText;
        public string isRecordingText = "Stop Session";
        public string isNotRecordingText = "Start Session";
        public GameObject sessionTimeParent;
        public TextMeshProUGUI sessionTimeText;

        public Color connectedColor = new Color(0,1,0);
        public Color disconnectedColor = new Color(1,0,0);

        bool isHideConsoleCoroutineRunning = false;

        bool isLogging = false;
        bool useGameTimerStatus;

        public static LogMessageUI instance;

        /// <summary>
        /// Singleton
        /// </summary>
        private void Awake()
        {
            // Check singleton, each time the menu scene is loaded, the instance is replaced with the newest script
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if(instance.consoleOutputText != null)
            {
                if(instance.consoleContainer != null)
                {
                    instance.consoleContainer.SetActive(false);
                }
            }

            useGameTimerStatus = FindObjectOfType<Timer>() != null;
            SetRecordingStatus(useGameTimerStatus ? Timer.IsRunning : false);

            WriteConsoleText("Log files stored in:" + SettingsManager.Values.logSettings.mainLogFolder);
        }

        void OnEnable()
        {
            EoM_Events.OnLoggingStateChanged += SetRecordingStatus;
            Timer.OnGameTimerStateChanged += SetGameTimerStatus;
        }


        void OnDisable()
        {
            EoM_Events.OnLoggingStateChanged -= SetRecordingStatus;
            Timer.OnGameTimerStateChanged -= SetGameTimerStatus;
        }

        private void SetGameTimerStatus(bool status)
        {
            if (useGameTimerStatus)
            {
                ApplyRecordingStatus(status);
            }
        }

        void Update()
        {
            ShowElapsedSessionTime();
        }

        // CONSOLE TEXT

        public void WriteConsoleText(string text)
        {
            if(!showConsoleBar) return;
            if(instance.consoleOutputText != null)
            {
                consoleOutputText.text = text;
                ShowConsoleContainer();
            }
        }

        public void ShowConsoleContainer()
        {
            if(!isHideConsoleCoroutineRunning)
            {
                StartCoroutine(ShowConsoleContainer(5.0f));
            }
        }

        IEnumerator ShowConsoleContainer(float timeout)
        {
            if(consoleContainer != null)
            {
                consoleContainer.SetActive(true);
            }
            isHideConsoleCoroutineRunning = true;
            yield return new WaitForSeconds(timeout);

            if(instance.consoleOutputText != null && instance.consoleContainer != null)
            {
                instance.consoleContainer.SetActive(false);
                isHideConsoleCoroutineRunning = false;
            }
        }

        private void ShowElapsedSessionTime()
        {
            if(sessionTimeParent != null && sessionTimeText != null && sessionTimeParent.activeSelf)
            {
                sessionTimeText.text = ExciteOMeterManager.GetTimestampString(0);
            }
        }

        // RECORDING

        public void StartStopRecording()
        {
            // Deactivate button to avoid multiple clicks
            //startStopLogging.interactable = false; // BUG: Sometimes does not stop recording and we cannot stop because is not clickable

            // Stop all the logging
            if(isLogging)
            {
                // Logging has started
                ExciteOMeterOnlineUI.instance.ClearSessionMarkers();
            }
            else
            {
                // Logging has stopped
            }

            // Call general 
            ExciteOMeterManager.instance.StartOrStopSessionLog();
        }

        public void StopRecording()
        {
            // Call general 
            ExciteOMeterManager.instance.StopSessionLog();
        }


        public void SetRecordingStatus(bool status)
        {
            if (useGameTimerStatus)
            {
                return;
            }

            ApplyRecordingStatus(status);
        }

        private void ApplyRecordingStatus(bool status)
        {
            // Update local variable
            isLogging = status;

            // Reenable button of logger
            //startStopLogging.interactable = true;

            // Show buttons with colors
            if(recordingStatusImage != null)
            {
                recordingStatusImage.color = status? connectedColor : disconnectedColor;
            }
            if(recordingStatusButtonText != null)
            {
                recordingStatusButtonText.text = status? isRecordingText : isNotRecordingText;
            }

            // Show/Hide timer message
            if(sessionTimeParent != null)
            {
                sessionTimeParent.SetActive(status? true : false);
            }

            // Show and hide panels
            if(showWhenNotRecording != null)
            {
                showWhenNotRecording.interactable = status? false : true;
                showWhenNotRecording.alpha = status? 0.2f : 1.0f;
            }

            if(showWhenRecording != null)
            {
                showWhenRecording.interactable = status? true : false;
                showWhenRecording.alpha = status? 1.0f : 0.2f;
            }
        }

    }
}


