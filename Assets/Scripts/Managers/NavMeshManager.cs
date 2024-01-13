using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(Manager))]
public class NavMeshManager : MonoBehaviour
{
    [SerializeField] private NavMeshSurface[] surfaces;

    public void GenerateNavMeshSurface()
    {
        foreach (NavMeshSurface surface in surfaces) surface.BuildNavMesh();
    }
}
