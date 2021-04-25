using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace VoxelTerrain.SaveLoad
{
    public class MenuWorldLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _newGame;
        [SerializeField] private GameObject _playfabField;
        [SerializeField] private GameObject _input;
        [SerializeField] private GameObject _randomise;
        [SerializeField] private GameObject _seedInput;
        [SerializeField] private List<TMP_Text> _buttonTexts;
        [SerializeField] private List<GameObject> _buttons;
        [SerializeField] private List<GameObject> _deleteButtons;
        
        private List<string> _directories = new List<string>();
        private string _worldName;
        private string _seed;
        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            _directories.Clear();

            for (int i = 0; i < 5; i++)
            {
                _buttons[i].SetActive(false);
                _deleteButtons[i].SetActive(false);
            }
            
            _newGame.SetActive(true);
            _input.SetActive(true);
            _randomise.SetActive(true);
            _seedInput.SetActive(true);
            
            var activeButtons = 0;
            
            var directories = Directory.GetDirectories(Application.persistentDataPath + "/" + "Worlds" + "/");

            foreach (var directory in directories)
            {
                var str = directory.Replace(Application.persistentDataPath + "/" + "Worlds" + "/", "");
                if (str == "MainMenu") continue;
                _directories.Add(str);
            }

            for (int i = 0; i < 5; i++)
            {
                if (_directories.Count == i) break;

                _buttonTexts[i].text = _directories[i];
                _buttons[i].SetActive(true);
                _deleteButtons[i].SetActive(true);
                activeButtons++;
            }

            if (activeButtons == 5)
            {
                _newGame.SetActive(false);
                _input.SetActive(false);
                _randomise.SetActive(false);
                _seedInput.SetActive(false);
            }
        }

        public void SetWorldName(TMP_InputField worldName) => _worldName = worldName.text;

        public void SetWorldName(TMP_Text worldName) => _worldName = worldName.text;

        public void SetSeed(TMP_InputField seed) => _seed = seed.text;
        public void SetSeed(TMP_Text seed) => _seed = seed.text;

        public void SaveSeed()
        {
            if (string.IsNullOrEmpty(_worldName)) return;
            
            var _chunkDirectory = Application.persistentDataPath + "/" + "Worlds" + "/" + _worldName + "/";

            if (!Directory.Exists(_chunkDirectory)) Directory.CreateDirectory(_chunkDirectory);

            var fullPath = _chunkDirectory + "seed.json";
            
            File.WriteAllText(fullPath, _seed);
        }

        public void SaveWorldName()
        {
            if (string.IsNullOrEmpty(_worldName)) return;
            if (string.IsNullOrEmpty(_seed)) return;

            var _chunkDirectory = Application.persistentDataPath + "/" + "Worlds" + "/" + _worldName + "/";

            if (!Directory.Exists(_chunkDirectory)) Directory.CreateDirectory(_chunkDirectory);
            
            SaveSeed();

            var activeWorldDirectory = Application.persistentDataPath + "/" + "Active_World" + "/";
            
            if (!Directory.Exists(activeWorldDirectory)) Directory.CreateDirectory(activeWorldDirectory);
            
            var fullPath = activeWorldDirectory + "activeWorld" + ".json";

            File.WriteAllText(fullPath, _worldName);
            
            SetAllInactive();
        }
        
        public void LoadWorldName()
        {
            if (string.IsNullOrEmpty(_worldName)) return;
            
            var _chunkDirectory = Application.persistentDataPath + "/" + "Worlds" + "/" + _worldName + "/";

            if (!Directory.Exists(_chunkDirectory)) Directory.CreateDirectory(_chunkDirectory);

            var activeWorldDirectory = Application.persistentDataPath + "/" + "Active_World" + "/";
            
            if (!Directory.Exists(activeWorldDirectory)) Directory.CreateDirectory(activeWorldDirectory);
            
            var fullPath = activeWorldDirectory + "activeWorld" + ".json";

            File.WriteAllText(fullPath, _worldName);
            
            SetAllInactive();
        }

        private void SetAllInactive()
        {
            for (int i = 0; i < 5; i++)
            {
                _buttons[i].SetActive(false);
                _deleteButtons[i].SetActive(false);
            }
            
            _newGame.SetActive(false);
            _input.SetActive(false);
            _randomise.SetActive(false);
            _seedInput.SetActive(false);
            
            SetPlayfabActive();
        }

        private void SetPlayfabActive() => _playfabField.SetActive(true);
        

        public void DeleteWorld(TMP_Text name)
        {
            var worldName = name.text;
            
            var _chunkDirectory = Application.persistentDataPath + "/" + "Worlds" + "/" + worldName + "/";

            if (Directory.Exists(_chunkDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(_chunkDirectory);

                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete(); 
                }

                Directory.Delete(_chunkDirectory);
            }

            var activeWorldDirectory = Application.persistentDataPath + "/" + "Active_World" + "/";

            if (!Directory.Exists(activeWorldDirectory)) return;
            
            var fullPath = activeWorldDirectory + "activeWorld" + ".json";

            File.WriteAllText(fullPath, "");
            
            Refresh();
        }

        public void RandomSeed(TMP_InputField input)
        {
            var seed = Random.Range(1, int.MaxValue).ToString();
            _seed = seed;
            input.SetTextWithoutNotify(seed);
        }

        public void QuitApplication() => Application.Quit();
        
    }
}
