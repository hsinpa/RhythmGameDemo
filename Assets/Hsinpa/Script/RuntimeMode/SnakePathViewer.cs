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

        [SerializeField, Range(0, 1f)]
        private float ScoreThredshold = 0.5f;

        private SnakePathScorer snakePathScorer;

        private Types.LevelJSON levelJSON;
        private float startTime;
        private float currentTime;

        private int noteIndex;
        private int noteLength;

        private const float defaultNoteTime = 1;
        private float noteTime = defaultNoteTime;

        private Dictionary<string, System.Action<Types.LevelComponent>> NoteActionTable = new Dictionary<string, System.Action<Types.LevelComponent>>();

        private List<NoteStruct> noteList = new List<NoteStruct>();

        private float default_time = 4f;
        private float default_distance = 18;

        private int bpm = 1;
        private float user_speed = 1;
        private float system_speed = 1;
        private float speed => user_speed * system_speed;
        private float distance;

        private float deltaTime = 0.02f;
        private Vector3 cameraPosition;
        private TouchStruct touchPoint = new TouchStruct();

        public void Start()
        {
            deltaTime = Time.deltaTime;
            snakePathScorer = new SnakePathScorer(0, ScoreThredshold);
            NoteActionTable = RegisterNoteTable();
            noteIndex = -1;

            cameraPosition = Camera.main.transform.position;

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

            snakePathScorer.OnUpdate(noteList);

        }

        private void FixedUpdate()
        {
            ProcessNoteMovement();
        }

        public TouchStruct OnMouseClick(UnityEngine.Ray rayStruct)
        {
            touchPoint.isValid = false;

            foreach (SnakePathScorer.CurrentSnakeVertex currentSnakeVertex in snakePathScorer.nearestSnakeVertexList)
            {
                Vector3 worldPos = currentSnakeVertex.noteStruct.snakeMesh.transform.position + currentSnakeVertex.noteStruct.snakeMesh.snakeMeshGenerator.midPoints[currentSnakeVertex.index];
                float distance = (rayStruct.origin - worldPos).magnitude;
                Vector3 raycastPos = rayStruct.origin + (rayStruct.direction * distance);
                float similarity = Vector3.Distance(worldPos, raycastPos);

                bool isAccept = similarity < ScoreThredshold;

                if (isAccept) {
                    touchPoint.isValid = true;
                    touchPoint.touchPoint = worldPos;
                    touchPoint.component = currentSnakeVertex.noteStruct.component;

                    return touchPoint;
                }
            }

            return touchPoint;
        }

        private void ProcessNote(int index) {

            Types.LevelComponent levelComp = levelJSON.sequence[index];

            if (NoteActionTable.TryGetValue(levelComp.type, out System.Action<Types.LevelComponent> action)) {
                action(levelComp);
            }
        }

        private void ProcessNoteMovement() {
            int noteLength = noteList.Count;
            Vector3 velocity3D = Vector3.zero;

            for (int i = 0; i < noteLength; i++) {
                velocity3D.z = -noteList[i].velocity * speed;
                noteList[i].snakeMesh.transform.Translate(velocity3D);
            }
        }

        private void ProcessSpeedNote(Types.LevelComponent component) {
            Debug.Log($"Type {component.type}, Time {component.time}, Value {component.value}");

            system_speed = float.Parse(component.value);
        }

        private void ProcessSnakeNote(Types.LevelComponent component) {
            Debug.Log($"Type {component.type}, Time {component.time}, Value {component.value}");

            SnakeMesh snakeNote = SpawnSnakeMesh(component);
            NoteStruct noteStruct = new NoteStruct();
            noteStruct.snakeMesh = snakeNote;
            noteStruct.component = component;
            noteStruct.velocity = ((default_distance - snakeNote.snakePath[0].z) / default_time) * deltaTime;
            noteList.Add(noteStruct);
        }

        private SnakeMesh SpawnSnakeMesh(Types.LevelComponent component) {
            SnakeMesh spawnEmptySnake = UtilityMethod.CreateObjectToParent<SnakeMesh>(SnakeHolder, snakePrefab.gameObject);

            SnakePath snakePathData = LevelSRP.GetSnakePath(component.value);

            if (snakePathData != null) {
                spawnEmptySnake.SetUp();
                spawnEmptySnake.SetSnakePath(snakePathData, false);
                spawnEmptySnake.RenderMesh();

                spawnEmptySnake.transform.position = new Vector3(snakePathData.OffsetPosition.x, -0.8f, default_distance - snakePathData[0].z);
            }

            return spawnEmptySnake;
        }

        private bool isNextNoteAvailable(int index) {
            if (index >= noteLength) return false;

            return (levelJSON.sequence[index].time < (currentTime + noteTime));
        }

        public struct NoteStruct {
            public SnakeMesh snakeMesh;
            public Types.LevelComponent component;
            public float velocity;
        }

        public struct TouchStruct
        {
            public Vector3 touchPoint;
            public Types.LevelComponent component;
            public bool isValid;
        }
    }
}
