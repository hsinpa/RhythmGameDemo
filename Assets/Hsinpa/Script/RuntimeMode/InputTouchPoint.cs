using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTouchPoint : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer _meshRenderer;

    public MeshRenderer meshRenderer => _meshRenderer;

    public float lastEnableTime;

    public int touchID = -1;
}
