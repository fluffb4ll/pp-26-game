using System;
using System.Collections.Generic;
using Brainrot;
using Helpers;
using Managers;
using UnityEngine;
using WorkbenchBuyer;

namespace Registries
{
    /// <summary>
    /// Реестр игровых сущностей - содержит ссылки на важные игровые объекты.
    /// </summary>
    public class EntityRegistry : MonoBehaviour
    {
        public static EntityRegistry Instance { get; private set; }
        
        private Dictionary<int, BrainrotObject> _brainrots = new();
        private Dictionary<BrainrotType, int> _brainrotTypesCounts = new();
        private Dictionary<int, Workbench.Workbench> _workbenches = new();
        private Dictionary<int, SpawnManager> _spawners = new();
        private Dictionary<int, BuyerController> _buyers = new();

        private Action<BrainrotObject> _onBrainrotAdded;
        private Action<Workbench.Workbench> _onWorkbenchAdded;
        

        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        
        public event Action<BrainrotObject> OnBrainrotAdded
        {
            add => _onBrainrotAdded += value;
            remove => _onBrainrotAdded -= value;
        }
        
        public event Action<Workbench.Workbench> OnWorkbenchAdded
        {
            add => _onWorkbenchAdded += value;
            remove => _onWorkbenchAdded -= value;
        }

        public Dictionary<int, BrainrotObject> GetBrainrots() => _brainrots;
        
        public Dictionary<int, Workbench.Workbench> GetWorkbenches() => _workbenches;
        
        public Dictionary<int, SpawnManager> GetSpawners() => _spawners;
        
        public Dictionary<int, BuyerController> GetBuyers() => _buyers;

        public int AddBrainrot(BrainrotType type, BrainrotObject brainrot)
        {
            var brainrotTypeCount = _brainrotTypesCounts.GetValueOrDefault(type);
            if (!_brainrotTypesCounts.TryAdd(type, ++brainrotTypeCount))
                _brainrotTypesCounts[type] = brainrotTypeCount;
            var id = type + "_" + brainrotTypeCount;
            var idHash = HashFunctions.GetDeterministicHashCode(id);
            
            if (!_brainrots.TryAdd(idHash, brainrot)) return 0;
            
            _onBrainrotAdded?.Invoke(brainrot);
            return idHash;
        }

        public int AddWorkbench(Workbench.Workbench workbench)
        {
            var id = "Workbench_" + (_workbenches.Count + 1);
            var idHash = HashFunctions.GetDeterministicHashCode(id);
            if (!_workbenches.TryAdd(idHash, workbench)) return 0;
            
            _onWorkbenchAdded?.Invoke(workbench);
            return idHash;
        }
        
        public int AddSpawner(SpawnManager spawner) 
        {
            var id = "Spawner" + (_buyers.Count + 1);
            var idHash = HashFunctions.GetDeterministicHashCode(id);
            return _spawners.TryAdd(idHash, spawner) ? idHash : 0;
        }

        public int AddBuyer(BuyerController buyer)
        {
            var id = "Buyer_" + (_buyers.Count + 1);
            var idHash = HashFunctions.GetDeterministicHashCode(id);
            return _buyers.TryAdd(idHash, buyer) ? idHash : 0;
        }
        
        public bool RemoveBrainrot(int id)
        {
            if (!_brainrots.ContainsKey(id))
                return false;
            var type = _brainrots[id].GetBrainrotInfo().type;
            _brainrotTypesCounts[type]--;
            _brainrots.Remove(id);
            return true;
        } 
        
        public bool RemoveWorkbench(int id) => _workbenches.Remove(id);
        
        public bool RemoveSpawner(int id) => _spawners.Remove(id);

        public BrainrotObject FindBrainrot(int id) => _brainrots.GetValueOrDefault(id);
        
        public Workbench.Workbench FindWorkbench(int id) => _workbenches.GetValueOrDefault(id);
        
        public SpawnManager FindSpawner(int id) => _spawners.GetValueOrDefault(id);
        
    }
}