using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageSelectorUI
{
    public class CarDummyUIController : MonoBehaviour
    {
        private RectTransform rectTransform;

        private Stage targetStage = null;
        private float t = 0;

        public IEnumerator MoveTo(Stage stage)
        {
            var startPosition = targetStage == null ? Vector2.zero : new Vector2(rectTransform.anchoredPosition.x, 0);
            targetStage = stage;
            var destination = targetStage.stageGameObject.rectTransform.anchoredPosition;
            var direction = destination - startPosition;

            if (direction.x < 0)
            {
                destination += new Vector2(300, 50);
                startPosition += new Vector2(0, 50);
            }
            else
            {
                destination += new Vector2(300, -50);
                startPosition += new Vector2(0, -50);
            }

            t = 0;
            rectTransform.anchoredPosition = startPosition;
            while (t < 1)
            {
                yield return MoveRoutine(startPosition, destination);
            }

            yield return null;
        }

        private IEnumerator MoveRoutine(Vector2 startPosition, Vector2 destination)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, destination, t * 0.1f);
            t += 0.01f;

            var dir = destination - startPosition;
            rectTransform.right = -dir;

            yield return new WaitForFixedUpdate();
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }
}
