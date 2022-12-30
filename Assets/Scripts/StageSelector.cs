using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        public void InitStageList()
        {
            stageList = new List<Stage>();
            for (int i = 0; i < stageCount; i++)
            {
                stageList.Add(new Stage(level, i));
            }
        }
    }

    public class Stage
    {
        public string level;
        public int number;
        public bool isCleared = false;
        public StageGameObject stageGameObject = null;

        private UnityEvent<Stage> SelectEvent;
        private Color deselectColor;
        public StageContainer parent;

        public Stage(string level, int number, bool isCleared = false)
        {
            this.level = level;
            this.number = number;
            this.isCleared = isCleared;
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
        private Stage parent;

        public StageGameObject(GameObject sceneGameObject, Stage parent)
        {
            gameObject = sceneGameObject;
            rectTransform = sceneGameObject.GetComponent<RectTransform>();
            text = sceneGameObject.GetComponentInChildren<Text>();
            this.parent = parent;
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
        }
    }

    public class StageSelector : MonoBehaviour
    {
        public List<StageContainer> stageContainerList;
        public GameObject stagePrefab;
        public GameObject carDummyUIPrefab;
        public GameObject carDummyUI;
        public Stage selectedStage = null;
        
        public UnityEvent<Stage> SelectEvent;

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

            SelectEvent.AddListener((stage) =>
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
                carDummyUI.GetComponent<RectTransform>().anchoredPosition =
                    selectedStage.stageGameObject.rectTransform.anchoredPosition + new Vector2(300, -50);

                SceneManager.LoadScene(stage.level + (stage.number + 1).ToString().PadLeft(2, '0'));
            });
        }
    }
};
