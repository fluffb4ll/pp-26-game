using Interfaces;
using UnityEngine;

namespace UI
{
    public class ActiveQuest : MonoBehaviour
    {
        [SerializeField] private GameObject activeQuestPointerPrefab;
        
        private IInteractable _interactable;
        private IDamageable _damageable;
        
        
    }
}