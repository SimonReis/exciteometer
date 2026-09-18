using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ExciteOMeter
{
    public class ReactInletUI : MonoBehaviour
    {
        [Header("Incoming data type")]
        public DataType dataType = DataType.NONE;

        [Header("UI setup")]
        public TextMeshProUGUI labelText;
        public Image connectionStatusImage;
        public Image recordingStatusImage;
        public TextMeshProUGUI valueText;

        public Color connectedColor = new Color(0,1,0);
        public Color disconnectedColor = new Color(1,0,0);

        [Header("Line")]
        public OnlineLineGraph onlineLine;
        public Color lineColor = new Color(1,0,0);
        public Color notRecordingColor = new Color(0.95f,0.95f,0.95f);
        
        private bool currentlyConnected = false;
        private bool useGameTimerStatus;

        private void Start()
        {
            // Setup UI children
            if(labelText == null) labelText = transform.GetComponentInChildren<TextMeshProUGUI>();
            if(connectionStatusImage ==null) connectionStatusImage = transform.GetComponentInChildren<Image>();
            if(recordingStatusImage == null)
            {
                Transform recording = transform.Find("Recording");
                if(recording != null) recordingStatusImage = recording.GetComponentInChildren<Image>();
            }
            if(labelText != null) labelText.text = dataType.ToString();

            // Setup connection indication
            currentlyConnected = EoM_Events.IsStreamConnected(dataType);
            SetConnectedStatus(currentlyConnected);

            // Recording status
            useGameTimerStatus = FindObjectOfType<Timer>() != null;
            SetRecordingStatus(useGameTimerStatus ? Timer.IsRunning : false);
        }

        void OnEnable()
        {
            EoM_Events.OnStreamConnected += StreamConnection;
            EoM_Events.OnStreamDisconnected += StreamDisconnection;
            EoM_Events.OnDataReceived += DataReceived;
            EoM_Events.OnLoggingStateChanged += SetRecordingStatus;
            Timer.OnGameTimerStateChanged += SetGameTimerStatus;
        }

        void OnDisable()
        {
            EoM_Events.OnStreamConnected -= StreamConnection;
            EoM_Events.OnStreamDisconnected -= StreamDisconnection;
            EoM_Events.OnDataReceived -= DataReceived;
            EoM_Events.OnLoggingStateChanged -= SetRecordingStatus;
            Timer.OnGameTimerStateChanged -= SetGameTimerStatus;
        }

        private void SetGameTimerStatus(bool status)
        {
            if (useGameTimerStatus)
            {
                SetRecordingIndicator(status);
            }
        }
        
        void OnValidate()
        {
            onlineLine.UpdateLineColor(lineColor);
        }

        private void StreamConnection(DataType type)
        {
            // If type of data from new LSL connection is equal to this flag's type
            if (type == dataType)
            {
                currentlyConnected = true;
                SetConnectedStatus(currentlyConnected);
            }
        }

        private void StreamDisconnection(DataType type)
        {
            // If type of data from new LSL connection is equal to this flag's type
            if (type == dataType)
            {
                currentlyConnected = false;
                SetConnectedStatus(currentlyConnected);
                ResetLiveDataDisplay();
            }

            if (ExciteOMeterOnlineUI.instance != null)
                ExciteOMeterOnlineUI.instance.ShowDisconnectedSignal();
        }

        private void ResetLiveDataDisplay()
        {
            if (valueText != null)
                valueText.text = "-";

            if (onlineLine != null)
            {
                onlineLine.RestartPlot();
                onlineLine.UpdateLineColor(disconnectedColor);
            }
        }

        // Sets the color of the image as connected or disconnected
        private void SetConnectedStatus(bool status)
        {
            connectionStatusImage.color = status? connectedColor : disconnectedColor;
        }

        
        private void DataReceived(DataType type, float timestamp, float value)
        {
            if(type == dataType && valueText != null)
            {
                valueText.text = value.ToString("F0");
                if(onlineLine != null)
                    onlineLine.PlotNewSample(value);
            }
        }

        public void SetRecordingStatus(bool status)
        {
            if (useGameTimerStatus)
            {
                return;
            }

            SetRecordingIndicator(status);
        }

        private void SetRecordingIndicator(bool status)
        {
            if (recordingStatusImage != null)
            {
                recordingStatusImage.color = status ? connectedColor : disconnectedColor;
            }

            if(status)
            {
                // Log started
                onlineLine.RestartPlot();
                onlineLine.UpdateLineColor(lineColor);
            }
            else
            {
                // Not recording, show the lines in different color
                onlineLine.UpdateLineColor(notRecordingColor);
            }
        }
    }
}
