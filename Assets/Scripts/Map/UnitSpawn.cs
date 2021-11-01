using System;
using System.Collections;
using Microsoft.Win32;
using UnityEngine;

[RequireComponent(typeof(Tile))]
public class UnitSpawn : MonoBehaviour
{
    public Facing Facing;
    public Unit Prefab;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.5f); //Quaternion.AngleAxis(GetComponent<Tile>().FaceToRot(Facing), Vector3.up));
    }
#endif
}
