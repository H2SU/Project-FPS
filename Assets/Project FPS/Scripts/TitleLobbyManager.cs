using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt.Matchmaking;
using UnityEngine.UI;
using UdpKit;
#pragma warning disable CS0414

public class TitleLobbyManager : Bolt.GlobalEventListener
{
    public static TitleLobbyManager TLM { get; private set; }
    private void Awake() => TLM = this;

    public Text LogText;
    public InputField SessionInput;
    public InputField NicknameInput;
    public string myNickname => NicknameInput.text;
    public string mySession => SessionInput.text;
    public GameObject cameraAudio;

    [Header("Sounds")]
    [Tooltip("�κ� BGM"), SerializeField]
    private string bgm = "DayDreamSound - TTRM";

    [Tooltip("���콺 Ŭ�� ȿ����"), SerializeField]
    AudioClip mouseClick;

    [Tooltip("���콺 ���� ȿ����"), SerializeField]
    AudioClip mouseOnOver;

    public void StartServer() => BoltLauncher.StartServer();
    public void StartClient() => BoltLauncher.StartClient();
    public void JoinSession() => BoltMatchmaking.JoinSession(SessionInput.text);

    public override void BoltStartDone()
    {
        if(BoltNetwork.IsServer)
            BoltMatchmaking.CreateSession(sessionID: SessionInput.text, sceneToLoad: "FPSGame");
    }

    public override void SessionListUpdated(Map<System.Guid, UdpSession> sessionList)
    {
        string log = "";
        foreach(var session in sessionList)
        {
            UdpSession photonSession = session.Value;
            if (photonSession.Source == UdpSessionSource.Photon)
                    log += $"{photonSession.HostName}\n";
        }
        LogText.text = log;
    }

    public void OnMouseOver() => cameraAudio.GetComponent<AudioSource>().PlayOneShot(mouseOnOver);
    public void OnMouseDown() => cameraAudio.GetComponent<AudioSource>().PlayOneShot(mouseClick);
}