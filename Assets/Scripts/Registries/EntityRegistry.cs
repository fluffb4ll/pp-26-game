using System.Collections.Generic;
using Brainrot;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Реестр игровых сущностей - содержит ссылки на важные игровые объекты.
    /// </summary>
    public class EntityRegistry : MonoBehaviour
    {
        private Dictionary<string, BrainrotObject> _brainrots;
        private Dictionary<string, Workbench.Workbench> _workbenches;
        private Dictionary<string, SpawnManager> _spawners;
        
        public Dictionary<string, BrainrotObject> GetBrainrots() => _brainrots;
        public Dictionary<string, Workbench.Workbench> GetWorkbenches() => _workbenches;
        public Dictionary<string, SpawnManager> GetSpawners() => _spawners;

        public bool AddBrainrot(string id, BrainrotObject brainrot) => _brainrots.TryAdd(id, brainrot);
        
        public bool AddWorkbench(string id, Workbench.Workbench workbench) => _workbenches.TryAdd(id, workbench);
        public bool AddSpawner(string id, SpawnManager spawner) => _spawners.TryAdd(id, spawner);
        
        public bool RemoveBrainrot(string id) => _brainrots.Remove(id);
        public bool RemoveWorkbench(string id) => _workbenches.Remove(id);
        public bool RemoveSpawner(string id) => _spawners.Remove(id);

        public BrainrotObject FindBrainrot(string id) => _brainrots.GetValueOrDefault(id);
        public Workbench.Workbench FindWorkbench(string id) => _workbenches.GetValueOrDefault(id);
        public SpawnManager FindSpawner(string id) => _spawners.GetValueOrDefault(id);
        
    }
}