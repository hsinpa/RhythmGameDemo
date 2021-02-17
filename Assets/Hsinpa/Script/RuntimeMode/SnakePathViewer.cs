using Hsinpa.Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa {
    public class SnakePathViewer : MonoBehaviour
    {
        [SerializeField]
        private TextAsset LevelJsonData;

        [SerializeField]
        private LevelSRP LevelSRP;

        private Types.LevelJSON levelJSON;
        private float startTime;
        private float currentTime;

        public void Start()
        {
            startTime = Time.time;
            levelJSON = JsonUtility.FromJson<Types.LevelJSON>(LevelJsonData.text);
        }

        private void Update()
        {
            currentTime = Time.time - startTime;

            Debug.Log(currentTime);
        }
    }
}
