﻿using System.Collections;   
using Lockstep.Client;
using Lockstep.Commands;
using Lockstep.Core;        
using Lockstep.Core.Interfaces;       
using UnityEngine;           
                              
public class RTSNetworkedSimulation : MonoBehaviour
{
    public static RTSNetworkedSimulation Instance;
                                        
    private Simulation _simulation;
    private readonly LiteNetLibClient _client = new LiteNetLibClient(); 
                                           
    public RTSEntityDatabase EntityDatabase;
    private bool _simulationStarted;

    public bool Connected => _client.Connected;

    public string ServerIp = "127.0.0.1";
    public int ServerPort = 9050;

    private void Awake()
    {
        Instance = this;
        var dataSource = new NetworkedDataSource(_client)
            .RegisterCommand(() => new SpawnCommand())
            .RegisterCommand(() => new NavigateCommand());

        _simulation =
            new Simulation(new LockstepSystems(Contexts.sharedInstance, new UnityGameService(EntityDatabase), new UnityLogger()), dataSource);      
            

        _simulation.Started += (sender, args) => _simulationStarted = true;
    }


    public void Execute(ISerializableCommand command)
    {
        _simulation.Execute(command);
    }

    private void Start()
    {
        _client.Start();
        StartCoroutine(AutoConnect());
    }

    private void OnDestroy()
    {
        _client.Stop();   
    }


    void Update()
    {
        _client.Update();
    }

    void FixedUpdate()
    {
        //if (!_simulationStarted)
        //{
        //    return;
        //}   

        //simulation.Simulate(); 

        //if (simulation.FrameCounter % 10 == 0)
        //{
        //    LockstepNetwork.Instance.SendHashCode(new Checksum{FrameNumber = simulation.FrameCounter, Value = simulation.CalculateChecksum()});
        //}                               
    }

    void OnGUI()
    {
        if (_simulationStarted)
        {
            GUILayout.BeginVertical(GUILayout.Width(100f));
            GUI.color = Color.white;
            GUILayout.Label("HashCode: " + Contexts.sharedInstance.gameState.hashCode.value);
            GUILayout.EndVertical();
        }
    }

    public IEnumerator AutoConnect()
    {
        while (!Connected)
        {
            _client.Connect(ServerIp, ServerPort);
            yield return new WaitForSeconds(1);
        }

        yield return null;
    }
}
