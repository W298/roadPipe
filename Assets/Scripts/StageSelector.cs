using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Linq;

namespace StageSelectorUI
{
    [Serializable]
    public class StageContainer
    {
        public string level;
        public int stageCount;
        public Color deselectColor;
        public List<Stage> stageList;
        public GameObject containerGameObject;
        public bool isLocked = true;

        public void InitStageList()
        {
            stageList = new List<Stage>();
            for (int i = 0; i < stageCount; i++)
            {
                stageList.Add(new Stage(level, i));
            }

            var load = StageClearDataManager.instance.data.data.Where(stageData => stageData.levelName == level).ToList();
            if (load.Count <= 0) return;

            load.Sort((a, b) => a.number.CompareTo(b.number));
            if (load.Last().score >= 3)
            {
                if (load.Count < stageCount)
                {
                    StageClearDataManager.instance.OpenStage(level, load.Count);
                    stageList.Find(stage => stage.number == load.Count).isLocked = false;
                }
                else if (load.Count == stageCount)
                {
                    OpenNextLevel();
                }
            }

            foreach (var data in load)
            {
                var targetStage = stageList.Find(stage => stage.level == data.levelName && stage.number == data.number);
                targetStage.isLocked = false;
                targetStage.score = data.score;
            }
        }

        private void OpenNextLevel()
        {
            var nextLevelName = level switch
            {
                "Y" => "B",
                "B" => "R",
                _ => ""
            };

            if (nextLevelName != "" && !StageSelector.instance.IsLevelLocked(nextLevelName)) StageClearDataManager.instance.OpenStage(nextLevelName, 0);
        }
    }

    public class Stage
    {
        public string level;
        public int number;
        public bool isLocked = true;
        public int score = 0;
        public StageGameObject stageGameObject = null;

        private UnityEvent<Stage> SelectEvent;
        private Color deselectColor;
        public StageContainer parent;

        public Stage(string level, int number, bool isLocked = true, int score = 0)
        {
            this.level = level;
            this.number = number;
            this.isLocked = isLocked;
            this.score = score;
        }

        public void OnClick()
        {
            SelectEvent.Invoke(this);
        }

        public void OnPointerEnter()
        {
            stageGameObject.text.color = Color.white;
        }

        public void OnPointerExit()
        {
            stageGameObject.text.color = deselectColor;
        }

        public void Init(GameObject sceneGameObject, StageContainer stageContainer, UnityEvent<Stage> SelectEvent, Color deselectColor, StageContainer parent)
        {
            this.SelectEvent = SelectEvent;
            this.deselectColor = deselectColor;
            this.parent = parent;

            stageGameObject = new StageGameObject(sceneGameObject, this);

            stageGameObject.gameObject.transform.SetParent(stageContainer.containerGameObject.transform);
            stageGameObject.rectTransform.anchoredPosition = new Vector2(600 * number, 0);
            stageGameObject.rectTransform.localScale = Vector3.one;
            stageGameObject.text.text = level + "-" + (number + 1).ToString().PadLeft(2, '0');
            stageGameObject.text.color = deselectColor;
        }
    }

    public class StageGameObject
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Text text;
        private EventTrigger trigger;
        private ScoreIndicator scoreIndicator;
        private GameObject lockIndicator;
        private Stage parent;

        public void UpdateScore()
        {
            if (parent.isLocked) return;

            lockIndicator.gameObject.SetActive(false);
            scoreIndicator.gameObject.SetActive(true);
            scoreIndicator.Render(parent.score);
        }

        public StageGameObject(GameObject sceneGameObject, Stage parent)
        {
            gameObject = sceneGameObject;
            rectTransform = sceneGameObject.GetComponent<RectTransform>();
            text = sceneGameObject.GetComponentInChildren<Text>();
            this.parent = parent;
            scoreIndicator = sceneGameObject.GetComponentInChildren<ScoreIndicator>(true);
            lockIndicator = sceneGameObject.transform.GetChild(2).gameObject;
            trigger = gameObject.GetComponent<EventTrigger>();

            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener(data =>
            {
                parent.OnPointerEnter();
            });

            var pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener(data =>
            {
                parent.OnPointerExit();
            });

            var pointerClick = new EventTrigger.Entry();
            pointerClick.eventID = EventTriggerType.PointerClick;
            pointerClick.callback.AddListener(data =>
            {
                parent.OnClick();
            });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);
            trigger.triggers.Add(pointerClick);

            UpdateScore();
        }
    }

    [Serializable]
    public class StageData
    {
        public string levelName;
        public int number;
        public int score;

        public StageData(string levelName, int number, int score)
        {
            this.levelName = levelName;
            this.number = number;
            this.score = score;
        }
    }

    [Serializable]
    public class StageClearData
    {
        public List<StageData> data;

        public StageClearData()
        {
            data = new List<StageData> { new StageData("Y", 0, 0) };
        }
    }

    public class StageClearDataManager
    {
        private static StageClearDataManager _instance;
        public static StageClearDataManager instance => _instance ??= new StageClearDataManager();

        private StageClearData _data = null;
        public StageClearData data
        {
            get => _data ??= LoadFromJSON();
            set
            {
                _data = value;
                SaveAsJSON(value);
            }
        }

        public void OpenStage(string levelName, int number)
        {
            if (data.data.Exists(info => info.levelName == levelName && info.number == number)) return;

            data.data.Add(new StageData(levelName, number, 0));
            data = data;
        }

        public void SaveScore(string levelName, int number, int score)
        {
            var target = data.data.Find(data => data.levelName == levelName && data.number == number);
            if (target == null)
            {
                target = new StageData(levelName, number, score);
                data.data.Add(target);
            }
            else
            {
                if (score > target.score) target.score = score;
            }

            data = data;
        }

        private StageClearData LoadFromJSON()
        {
            var path = Path.Combine(Application.dataPath, "stageClearDB.json");

            if (!File.Exists(path))
            {
                data = new StageClearData();
                return data;
            }

            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<StageClearData>(json);
        }

        private void SaveAsJSON(StageClearData value)
        {
            var json = JsonUtility.ToJson(value, true);
            var path = Path.Combine(Application.dataPath, "stageClearDB.json");
            File.WriteAllText(path, json);
        }
    }

    public class StageSelector : MonoBehaviour
    {
        private static StageSelector _instance = null;
        public static StageSelector instance => _instance ??= FindObjectOfType<StageSelector>();

        public List<StageContainer> stageContainerList;
        public GameObject stagePrefab;
        public GameObject carDummyUIPrefab;
        public GameObject carDummyUI;
        public Stage selectedStage = null;

        public LoadingPanel loadingPanel;
        public UnityEvent<Stage> SelectEvent;

        private IEnumerator routine = null;

        private IEnumerator OnSelectStage(Stage stage)
        {
            if (selectedStage == null || (selectedStage != null && stage.parent != selectedStage.parent))
            {
                if (carDummyUI != null) Destroy(carDummyUI);
                carDummyUI = Instantiate(carDummyUIPrefab, Vector3.zero, Quaternion.identity);
                carDummyUI.transform.SetParent(stage.parent.containerGameObject.transform);
                carDummyUI.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 180);
                carDummyUI.GetComponent<Image>().color =
                    stage.parent.containerGameObject.GetComponent<Image>().color;
            }

            selectedStage = stage;
            yield return carDummyUI.GetComponent<CarDummyUIController>().MoveTo(stage);
        }

        public void StartGame()
        {
            if (selectedStage == null) return;
            loadingPanel.LoadScene(selectedStage.level + (selectedStage.number + 1).ToString().PadLeft(2, '0'));
        }

        public bool IsLevelLocked(string stageName)
        {
            var targetLevel = stageContainerList.Find(cont => cont.level == stageName);

            return targetLevel == null || targetLevel.isLocked;
        }

        private void Awake()
        {
            SelectEvent.AddListener(stage =>
            {
                if (stage.isLocked) return;
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
                routine = OnSelectStage(stage);
                StartCoroutine(routine);
            });
        }

        private void Start()
        {
            foreach (var stageContainer in stageContainerList)
            {
                stageContainer.InitStageList();
                stageContainer.stageList.ForEach(stage =>
                {
                    var go = Instantiate(stagePrefab, Vector3.zero, Quaternion.identity);
                    stage.Init(go, stageContainer, SelectEvent, stageContainer.deselectColor, stageContainer);
                });
            }
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
};
