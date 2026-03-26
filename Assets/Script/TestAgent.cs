using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TestAgent : Agent
{
    public override void Initialize()
    {
        Debug.Log("TestAgent Initialize()");
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(0f);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 不需實作
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 0;
    }
}
