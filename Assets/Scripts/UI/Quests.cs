using System.Collections.Generic;
using Brainrot;
using Enemy;
using Managers;
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
        private GameManager _gameManager;
        private ActiveQuest _activeQuest;
        
        private void Awake()
        {
            _gameManager = GameManager.Instance;
        }
        
        private void Start()
        {
            ChangeQuests(_gameManager.GetCurrentQuestID());
        }

        private void ChangeQuests(int currentQuestID)
        {
            _currentQuest = quests[currentQuestID];
            Debug.Log(currentQuestID);
            questDescText.SetText(_currentQuest.description);
            _activeQuest = _currentQuest.objective?.AddComponent<ActiveQuest>();

            switch (_currentQuest.type)
            {
                case QuestType.Kill:
                    foreach (var spawner in _gameManager.GetSpawners())
                        spawner.OnEnemyDeath += FinishKillQuest;
                    break;
                case QuestType.Buy:
                    _currentQuest.objective.GetComponent<BuyerController>().OnBuyWorkbench += FinishBuyQuest;
                    break;
                case QuestType.PickUp:
                    _currentQuest.objective.GetComponent<BrainrotObject>().OnInteract += FinishPickUpQuest;
                    break;
            }
        }

        private void FinishKillQuest(GameObject enemy)
        {
            foreach (var spawner in _gameManager.GetSpawners())
                spawner.OnEnemyDeath -= FinishKillQuest;
            
            var questID = _gameManager.IncrementCurrentQuestID();
            var newQuest = quests[questID];
            Debug.Log(enemy);
            // TODO: добавить в список для квестов
            newQuest.objective = enemy
                .GetComponent<EnemyCombat>()
                .GetSpawnableBrainrot();
            quests[questID] = newQuest;
            
            ChangeQuests(questID);
        }

        private void FinishBuyQuest(Workbench.Workbench workbench)
        {
            _currentQuest.objective.GetComponent<BuyerController>().OnBuyWorkbench -= FinishBuyQuest;
            
            var questID = _gameManager.IncrementCurrentQuestID();
            var newQuest = quests[questID];
            // TODO: поменять так, чтобы для выполнения подходил любой из станков (список станков и ивент на каждом?)
            // мб также сделать эту хуйню более динамической, чтобы всё работало, если следующий квест не на Insert
            newQuest.objective = workbench.gameObject;
            quests[questID] = newQuest;
            
            ChangeQuests(questID);
        }

        private void FinishPickUpQuest(BrainrotObject brainrot)
        {
            brainrot.OnInteract -= FinishPickUpQuest;
            var questID = _gameManager.IncrementCurrentQuestID();
            ChangeQuests(questID);
        }
        
        private void FinishQuest()
        {
            
        }
    }
}