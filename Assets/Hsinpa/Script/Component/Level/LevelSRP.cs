using Hsinpa.Snake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hsinpa.Level
{
    [System.Serializable, CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Level", order = 1)]
    public class LevelSRP : ScriptableObject
    {
        [SerializeField]
        private SnakePath[] _snakePaths;

        public SnakePath[] snakePath => _snakePaths;

        public SnakePath GetSnakePath(string id) {
            if (_snakePaths == null) return null;

            return _snakePaths.First(x => x.name == id);
        }
    }
}