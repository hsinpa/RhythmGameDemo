using Hsinpa.Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hsinpa.Snake;
using Utility;

namespace Hsinpa {
    public class SnakePathViewer : MonoBehaviour
    {
        [SerializeField]
        private TextAsset LevelJsonData;

        [SerializeField]
        private LevelSRP LevelSRP;

        [SerializeField]
        private Transform SnakeHolder;

        [SerializeField]
        private SnakeMesh snakePrefab;

        private Types.LevelJSON levelJSON;
        private float startTime;
        private float currentTime;

        private int noteIndex;
        private int noteLength;

        private const float defaultNoteTime = 1;
        private float noteTime = defaultNoteTime;

        private Dictionary<string, System.Action<Types.LevelComponent>> NoteActionTable = new Dictionary<string, System.Action<Types.LevelComponent>>();

        public void Start()
        {
            NoteActionTable = RegisterNoteTable();
            noteIndex = -1;
            
            PlaySnakeView();
        }

        private Dictionary<string, System.Action<Types.LevelComponent>> RegisterNoteTable() {
            Dictionary<string, System.Action<Types.LevelComponent>> actionTable = new Dictionary<string, System.Action<Types.LevelComponent>>();

            actionTable.Add(EventFlag.LevelComponent.SnakeType, ProcessSnakeNote);
            actionTable.Add(EventFlag.LevelComponent.SpeedType, ProcessSpeedNote);

            return actionTable;
        }

        private void PlaySnakeView()
        {
            noteIndex = 0;
            startTime = Time.time;
            levelJSON = JsonUtility.FromJson<Types.LevelJSON>(LevelJsonData.text);
            noteLength = levelJSON.sequence.Length;

            UtilityMethod.ClearChildObject(SnakeHolder);
        }

        private void Update()
        {
            //Not start yet
            if (noteIndex < 0) return;

            currentTime = Time.time - startTime;

            bool doneProcessing = false;

            while (!doneProcessing) {

                bool noteAvailable = isNextNoteAvailable(noteIndex);

                if (noteAvailable) {
                    ProcessNote(noteIndex);
                    noteIndex++;
                }

                doneProcessing = !noteAvailable;
            }

            Debug.Log(currentTime);
        }

        private void ProcessNote(int index) {

            Types.LevelComponent levelComp = levelJSON.sequence[index];

            if (NoteActionTable.TryGetValue(levelComp.type, out System.Action<Types.LevelComponent> action)) {
                action(levelComp);
            }

        }

        private void ProcessSpeedNote(Types.LevelComponent component) {
            Debug.Log($"Type {component.type}, Time {component.time}, Value {component.value}");
        }

        private void ProcessSnakeNote(Types.LevelComponent component) {
            Debug.Log($"Type {component.type}, Time {component.time}, Value {component.value}");
        }

        private bool isNextNoteAvailable(int index) {
            if (index >= noteLength) return false;

            return (levelJSON.sequence[index].time < (currentTime + noteTime));
        }

    }
}
