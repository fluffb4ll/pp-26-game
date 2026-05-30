using System.Collections.Generic;
using Boat;
using Brainrot;
using Enemy;
using Managers;
using Registries;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorkbenchBuyer;

namespace UI
{
    public class Quests : MonoBehaviour
    {
        [SerializeField] private List<Quest> quests;
        
        [SerializeField] private TextMeshProUGUI questDescText;
        [SerializeField] private Image questIcon;
        
        private Quest _currentQuest;
        private SaveManager _saveManager;
        private EntityRegistry _entityRegistry;
        private ActiveQuest _activeQuest;
        
        private void Awake()
        {
            _saveManager = SaveManager.Instance;
            _entityRegistry = EntityRegistry.Instance;
        }
        
        private void Start()
        {
            ChangeQuests(_saveManager.GetCurrentQuestID());
        }

        private void ChangeQuests(int currentQuestID)
        {
            if (currentQuestID >= quests.Count)
            {
                gameObject.SetActive(false);
                return;
            }
            
            _currentQuest = quests[currentQuestID];
            questDescText.SetText(_currentQuest.description);
            //_activeQuest = _currentQuest.objective?.AddComponent<ActiveQuest>();

            switch (_currentQuest.type)
            {
                case QuestType.Kill:
                    foreach (var spawner in _entityRegistry.GetSpawners().Values)
                        spawner.OnEnemyDeath += FinishKillQuest;
                    break;
                case QuestType.Buy:
                    foreach (var buyer in _entityRegistry.GetBuyers().Values)
                        buyer.OnBuyWorkbench += FinishBuyQuest;
                    break;
                case QuestType.PickUp:
                    _entityRegistry.OnBrainrotAdded += SubscribeToNewBrainrotEvents;
                    foreach (var brainrot in _entityRegistry.GetBrainrots().Values)
                        brainrot.OnInteract += FinishPickUpQuest;
                    break;
                case QuestType.Insert:
                case QuestType.Collect:
                    _entityRegistry.OnWorkbenchAdded += SubscribeToNewWorkbenchEvents;
                    foreach (var workbench in _entityRegistry.GetWorkbenches().Values)
                        workbench.OnInteract += FinishWorkbenchQuest;
                    break;
                case QuestType.Travel:
                    _currentQuest.objective.GetComponent<BoatController>().OnTravel += FinishTravelQuest;
                    break;
            }
        }

        private void FinishKillQuest(GameObject enemy)
        {
            foreach (var spawner in _entityRegistry.GetSpawners().Values)
                spawner.OnEnemyDeath -= FinishKillQuest;
            var questID = _saveManager.IncrementCurrentQuestID();
            ChangeQuests(questID);
        }

        private void FinishBuyQuest(Workbench.Workbench workbench)
        {
            foreach (var buyer in _entityRegistry.GetBuyers().Values)
                buyer.OnBuyWorkbench -= FinishBuyQuest;
            var questID = _saveManager.IncrementCurrentQuestID();
            ChangeQuests(questID);
        }

        private void FinishPickUpQuest()
        {
            _entityRegistry.OnBrainrotAdded -= SubscribeToNewBrainrotEvents;
            foreach (var brainrot in _entityRegistry.GetBrainrots().Values)
                brainrot.OnInteract -= FinishPickUpQuest;
            var questID = _saveManager.IncrementCurrentQuestID();
            ChangeQuests(questID);
        }

        private void FinishWorkbenchQuest(QuestType questType)
        {
            if (questType != _currentQuest.type)
                return;
            
            foreach (var workbench in _entityRegistry.GetWorkbenches().Values)
                workbench.OnInteract -= FinishWorkbenchQuest;
            var questID = _saveManager.IncrementCurrentQuestID();
            ChangeQuests(questID);
        }

        private void FinishTravelQuest()
        {
            _currentQuest.objective.GetComponent<BoatController>().OnTravel -= FinishTravelQuest;
            var questID = _saveManager.IncrementCurrentQuestID();
            ChangeQuests(questID);
        }

        private void SubscribeToNewBrainrotEvents(BrainrotObject brainrot)
        {
            brainrot.OnInteract += FinishPickUpQuest;
        }

        private void SubscribeToNewWorkbenchEvents(Workbench.Workbench workbench)
        {
            workbench.OnInteract += FinishWorkbenchQuest;
        }
    }
}